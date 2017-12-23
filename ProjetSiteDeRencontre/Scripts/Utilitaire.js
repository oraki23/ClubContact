/*------------------------------------------------------------------------------------

FICHIER CONTENANT DIVERSES FONCTIONS JAVASCRIPTS UTILE UN PEU PARTOUT SUR LE SITE

--------------------------------------------------------------------------------------
Par: Anthony Brochu et Marie-Ève Massé
Novembre 2017
Club Contact
------------------------------------------------------------------------------------*/

function RemplirVilles(path) {
    $.ajax({
        url: path,
        type: 'GET',
        contentType: 'application/json; charset=utf-8',
        dataType: 'JSON',
        data: { "noProvince" : $('#noProvince').val() },
        success: function (result) {
            $('#noVille').html("");
            $.each(result, function (i, ville) {
                $('#noVille').append(
                    $('<option></option>').val(ville.noVille).html(ville.nomVille))
            });
        }    
});
}
        
function RemplirVillesRecherche(path) {
    $.ajax({
        url: path,
        type: 'GET',
        contentType: 'application/json; charset=utf-8',
        dataType: 'JSON',
        data: { "noProvince": $('#noProvince').val() },
        success: function (result) {
            $('#noVille').html("");
            $('#noVille').append(
                    $('<option value=\'\'></option>').html("Sans importance"));
            $.each(result, function (i, ville) {
                $('#noVille').append(
                    $('<option></option>').val(ville.noVille).html(ville.nomVille))
            });
        }
    });
}

function RemplirVillesDebutRecherche(noVille, path) {
    $.ajax({
        url: path,
        type: 'GET',
        dataType: "JSON",
        data: { "noProvince": $('#noProvince').val() },
        success: function (result) {
            $('#noVille').html("");
            $('#noVille').append(
                    $('<option value=\'\'></option>').html("Sans importance"));
            $.each(result, function (i, ville) {
                if (ville.noVille == noVille) {
                    $('#noVille').append(
                    $('<option selected=\'selected\'></option>').val(ville.noVille).html(ville.nomVille))
                }
                else {
                    $('#noVille').append(
                    $('<option></option>').val(ville.noVille).html(ville.nomVille))
                }
            });
        }
    });
}

function RemplirVillesDebut(noVille, path) {
    $.ajax({
        url: path,
        type: 'GET',
        dataType: "JSON",
        data: { "noProvince": $('#noProvince').val() },
        success: function (result) {
            $('#noVille').html("");
            $.each(result, function (i, ville) {
                if (ville.noVille == noVille)
                {
                    $('#noVille').append(
                    $('<option selected=\'selected\'></option>').val(ville.noVille).html(ville.nomVille))
                }
                else {
                    $('#noVille').append(
                    $('<option></option>').val(ville.noVille).html(ville.nomVille))
                }   
            });
        }
    });
}

function RemplirHobbiesRecherche(path) {
    $.ajax({
        url: path,
        type: 'GET',
        contentType: 'application/json; charset=utf-8',
        dataType: 'JSON',
        data: { "noTypeActiviteRecherche": $('#noTypeActiviteRecherche').val() },
        success: function (result) {
            $('#noTypeInterets').html("");
            $('#noTypeInterets').append(
                    $('<option value=\'\'></option>').html("Sans importance"));
            $.each(result, function (i, hobbie) {
                $('#noTypeInterets').append(
                    $('<option></option>').val(hobbie.noHobbie).html(hobbie.nomHobbie))
            });
        }
    });
}

function RemplirHobbiesDebutRecherche(noHobbie, path) {
    $.ajax({
        url: path,
        type: 'GET',
        dataType: "JSON",
        data: { "noTypeActiviteRecherche": $('#noTypeActiviteRecherche').val() },
        success: function (result) {
            $('#noTypeInterets').html("");
            $('#noTypeInterets').append(
                    $('<option value=\'\'></option>').html("Sans importance"));
            $.each(result, function (i, hobbie) {
                if (hobbie.noHobbie == noHobbie) {
                    $('#noTypeInterets').append(
                    $('<option selected=\'selected\'></option>').val(hobbie.noHobbie).html(hobbie.nomHobbie))
                }
                else {
                    $('#noTypeInterets').append(
                    $('<option></option>').val(hobbie.noHobbie).html(hobbie.nomHobbie))
                }
            });
        }
    });
}

function afficherPhotoProfil(theUserName, path) {
    $.ajax({
        url: path,
        type: 'GET',
        data: { userName: theUserName },
        success: function (result) {
            $('#photoProfilPetit').attr("src", result);
        }
    });
}

function supprimerPhoto(premium, lobjet) {
    $(lobjet).parent().parent().hide();
    $(lobjet).prev('input').val('true');

    //console.log();
    if ($(lobjet).parent().parent().find('.divRadioButton').children().is(":checked"))
    {
        $(lobjet).parent().parent().find('.divRadioButton').children().removeAttr('checked');
        if ($("input:radio[name='selectionPhotoProfil']").length > 0) {
            $(lobjet).parent().parent().nextAll('.lignePhoto:visible').first().find('.divRadioButton').children().prop('checked', true);
            if(!$("input:radio[name='selectionPhotoProfil']:checked").val())
            {
                //RIEN n'EST COCHÉ
                $(lobjet).parent().parent().prevAll('.lignePhoto:visible').first().find('.divRadioButton').children().prop('checked', true);
            }
        }
    }
    //$(lobjet).parent().parent().find('.divRadioButton').children().prop('checked', false);

    var nbMaxPhotos = 5;
    var nbDePhotos = parseInt($('#nbPhotos').val()) - 1;

    if (premium.toLowerCase() == 'true') {
        nbMaxPhotos = 12;
    }

    $('#nbPhotos').val(nbDePhotos);
    $('#nbPhotosAffiche').text(nbDePhotos + "/" + nbMaxPhotos);

    $('#maxPhotosErrorMessage').hide();
}

function supprimerPhotoActivite(lobjet, i) {
    var iString = i.toString();

    $(lobjet).parent().parent().hide();
    $(lobjet).prev('input').val('true');

    var nbMaxPhotos = 5;
    var nbDePhotos = parseInt($('#nbPhotos' + iString).val()) - 1;

    console.log(nbDePhotos);

    $('#nbPhotos' + iString).val(nbDePhotos);
    $('#nbPhotosAffiche' + iString).text(nbDePhotos + "/" + nbMaxPhotos);

    $('#maxPhotosErrorMessage' + iString).hide();
}

function suppressionListeNoire(lobjet) {
    $(lobjet).parent().parent().hide();
    $(lobjet).prev('input').val('true');
}

function afficheNbPhotoUtiliserSurRestante(premium) {
    var nbMaxPhotos = 5;
    var premium = premium.toLowerCase();

    if (premium == 'true') {
        nbMaxPhotos = 12;
    }

    $('#nbPhotosAffiche').text(parseInt($('#nbPhotos').val()) + "/" + nbMaxPhotos);
}

function PreviewImage(i) {
    var preview = document.getElementById("imgShow" + i);

    var file = document.getElementById("imgUpload" + i).files[0];

    var reader = new FileReader();

    reader.onloadend = function () {
        preview.src = reader.result;
    }

    if (file) {
        reader.readAsDataURL(file);
    }
    else {
        preview.src = "";
    }
}

//Variable pour déterminer si les champs sont actuellement disabled.
var disabled = false;

function verifierRaisonSiteExclusif(noRaisonsSansButPrecis, nbRaisonsSurSite, selectedItems) {
    //Variable pour déterminer si sansButPrécis est séléctionné
    var sansButPrecis = false;

    if (selectedItems != null) {
        for (i = 0; i < selectedItems.length; i++) {
            if (selectedItems[i] == noRaisonsSansButPrecis) {
                sansButPrecis = true;

                for (j = 0; j < nbRaisonsSurSite; j++) {
                    if (j != noRaisonsSansButPrecis - 1) {
                        $('.raisonsSite')[0].sumo.disableItem(j);
                    }
                }
                disabled = true;
                break;
            }
            else {
                sansButPrecis = false;
            }
        }
    }

    if (!sansButPrecis && disabled) {
        for (j = 0; j < nbRaisonsSurSite; j++) {
            $('.raisonsSite')[0].sumo.enableItem(j);
        }
        disabled = false;
    }
}

function getNbMessageDansBoite(boite, path, noMembre)
{
    $.ajax({
        url: path,
        type: 'POST',
        data: { noMembre: noMembre, boite: boite },
        success: function(results){
            if(results > 0)
            {
                if (boite == "inbox")
                {
                    $('#boiteDeReceptionNbMessage').text("Boîte de réception (" + results + ")");
                }
                else if(boite == "deleted")
                {
                    $('#deletedNbMessage').text("Messages Supprimés (" + results + ")");
                }
            }
            else
            {
                if (boite == "inbox")
                {
                    $('#boiteDeReceptionNbMessage').text("Boîte de réception");
                }
                else if (boite == "deleted")
                {
                    $('#deletedNbMessage').text("Messages Supprimés");
                }
            }
        }
    });
}
function ShowMenu(control, e, noMembre) {
    var posx = e.clientX + window.pageXOffset + 'px'; //Left Position of Mouse Pointer
    var posy = e.clientY + window.pageYOffset + 'px'; //Top Position of Mouse Pointer
    document.getElementById(control + noMembre).style.position = 'absolute';
    document.getElementById(control + noMembre).style.display = 'inline';
    document.getElementById(control + noMembre).style.left = posx;
    document.getElementById(control + noMembre).style.top = posy;
}
function HideMenu(control) {
    $('.' + control).hide();
}

function supprimerContactAJAX(noMembreASupprimer, noMembreCo, path) {
    var supprimerConfirmation = confirm("Voulez-vous vraiment supprimer ce contact?");
    if (supprimerConfirmation == true) {
        $.ajax({
            url: path,
            type: 'POST',
            data: { noMembreCo: noMembreCo, noMembreASupprimer: noMembreASupprimer, },
            success: function (results) {
                location.reload();
            }
        });
    }
    
}