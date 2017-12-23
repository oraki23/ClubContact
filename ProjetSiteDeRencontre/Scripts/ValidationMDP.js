/*------------------------------------------------------------------------------------

FICHIER JAVASCRIPT CONTENANT LES FONCTIONS PERMETTANT LA VALIDATION DE MOT DE PASSE

--------------------------------------------------------------------------------------
Par: Anthony Brochu et Marie-Ève Massé
Novembre 2017
Club Contact
------------------------------------------------------------------------------------*/

function validerMDP(idForm, nouveauMembre, path, noMembre, motDePasseOLD, motDePasse1, motDePasse) {

    var validator = $(idForm).validate();
    validator.showErrors({ "motDePasse": null });

    var ancienMDPValide = true;

    if (!nouveauMembre) {
        if (motDePasse1 != "" || motDePasse != "") {

            ancienMDPValide = false;

            $.ajax({
                url: path,
                type: 'POST',
                dataType: 'JSON',
                data: { motDePasseAncien: motDePasseOLD, noMembre: noMembre },
                async: false,
                success: function (results) {
                    if (results == true) {
                        ancienMDPValide = true;
                    }
                }
            });
        }
    }

    if (!ancienMDPValide) {
        validator.showErrors({ "motDePasse": "Votre mot de passe actuel est invalide." });
        return false;
    }
    else if ($('#motDePasse1').val() != $('#motDePasse').val()) {
        validator.showErrors({ "motDePasse": "Les mots de passe doivent être identiques." });
        return false;
    }
    else if ($('#motDePasse1').val() != "" || $('#motDePasse').val() != "") {
        if ($('#motDePasse1').val().length < 8 && $('#motDePasse1').val().length > 0) {
            validator.showErrors({ "motDePasse": "Le nouveau mot de passe doit contenir au moins 8 caractères." });
            return false;
        }
        else if (!hasLetters($('#motDePasse1').val())) {
            validator.showErrors({ "motDePasse": "Le nouveau mot de passe doit avoir au moins une lettre." });
            return false;
        }
        else if (!hasUpperCase($('#motDePasse1').val())) {
            validator.showErrors({ "motDePasse": "Le nouveau mot de passe doit avoir au moins une lettre majuscule." });
            return false;
        }
        else if (!hasLowerCase($('#motDePasse1').val())) {
            validator.showErrors({ "motDePasse": "Le nouveau mot de passe doit avoir au moins une lettre minuscule." });
            return false;
        }
        else if (!hasNumbers($('#motDePasse1').val())) {
            validator.showErrors({ "motDePasse": "Le nouveau mot de passe doit avoir au moins un chiffre." });
            return false;
        }
    }
    else if (noMembre == 0) {
        if ($('#motDePasse1').val() == "") {
            validator.showErrors({ "motDePasse": "Veuillez entrer un mot de passe." });
            return false;
        }
    }

    return true;
}

function hasUpperCase(str) {
    return str.toLowerCase() != str;
}
function hasLowerCase(str) {
    return str.toUpperCase() != str;
}
function hasNumbers(str) {
    return /\d/.test(str);
}
function hasLetters(str) {
    return /\D/.test(str);
}