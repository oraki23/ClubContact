/*------------------------------------------------------------------------------------

FICHIER JAVASCRIPT DÉFINISANT LES VALIDATEURS CUSTOM JQUERY QUI SERONT UTILISÉS DANS
L'APPLICATION

CETTE MÉTHODE ET LES COMMENTAIRES ONT ÉTÉ TROUVÉS D'INTERNET.

--------------------------------------------------------------------------------------
Par: Anthony Brochu et Marie-Ève Massé
Novembre 2017
Club Contact
------------------------------------------------------------------------------------*/

// File Created: 2 octobre, 2017 
// Modifié et utilisé par: Anthony Brochu et Marie-Ève Massé

// Value is the element to be validated, params is the array of name/value pairs of the parameters extracted from the HTML, element is the HTML element that the validator is attached to
$.validator.addMethod("datepluspetitque", function (value, element, params) {
    return Date.parse(value) <= params;
});

$.validator.addMethod("dateplusgrandque", function (value, element, params) {
    return Date.parse(value) >= params;
});

$.validator.addMethod("nombreplusgrandque", function (value, element, params) {
    return value >= $(params).val();
});

$.validator.addMethod("nombrepluspetitque", function (value, element, params) {
    if ($(params).val() == "")
    {
        return true;
    }
    return value <= $(params).val();
});

/* The adapter signature:
adapterName is the name of the adapter, and matches the name of the rule in the HTML element.
 
params is an array of parameter names that you're expecting in the HTML attributes, and is optional. If it is not provided,
then it is presumed that the validator has no parameters.
 
fn is a function which is called to adapt the HTML attribute values into jQuery Validate rules and messages.
 
The function will receive a single parameter which is an options object with the following values in it:
element
The HTML element that the validator is attached to
 
form
The HTML form element
 
message
The message string extract from the HTML attribute
 
params
The array of name/value pairs of the parameters extracted from the HTML attributes
 
rules
The jQuery rules array for this HTML element. The adapter is expected to add item(s) to this rules array for the specific jQuery Validate validators
that it wants to attach. The name is the name of the jQuery Validate rule, and the value is the parameter values for the jQuery Validate rule.
 
messages
The jQuery messages array for this HTML element. The adapter is expected to add item(s) to this messages array for the specific jQuery Validate validators that it wants to attach, if it wants a custom error message for this rule. The name is the name of the jQuery Validate rule, and the value is the custom message to be displayed when the rule is violated.
*/
$.validator.unobtrusive.adapters.add("datepluspetitque", ["maxvalue"], function (options) {
    options.rules["datepluspetitque"] = options.params.maxvalue;
    options.messages["datepluspetitque"] = options.message;
});

$.validator.unobtrusive.adapters.add("dateplusgrandque", ["minvalue"], function (options) {
    options.rules["dateplusgrandque"] = options.params.minvalue;
    options.messages["dateplusgrandque"] = options.message;
});

$.validator.unobtrusive.adapters.add("nombreplusgrandque", ["otherpropertyname"], function (options) {
    options.rules["nombreplusgrandque"] = "#" + options.params.otherpropertyname;
    options.messages["nombreplusgrandque"] = options.message;
})

$.validator.unobtrusive.adapters.add("nombrepluspetitque", ["otherpropertyname"], function (options) {
    options.rules["nombrepluspetitque"] = "#" + options.params.otherpropertyname;
    options.messages["nombrepluspetitque"] = options.message;
})

/*Custom 100%*/

$.validator.addMethod("datePlusGrandeQueNow", function (value, element) {
    var annee = parseInt(value.substring(0, 4));
    var mois = parseInt(value.substring(5, 7)) - 1;
    var jour = parseInt(value.substring(8, 10));

   /* var heure = parseInt(value.substring(11, 13));
    var minute = parseInt(value.substring(14, 16));*/

    var date = new Date(annee, mois, jour);

    return date >= new Date($.now());
}, 'Veuillez choisir une date dans le futur.');