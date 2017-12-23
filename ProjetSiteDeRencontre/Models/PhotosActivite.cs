/*------------------------------------------------------------------------------------

CLASSE CONTENANT LES INFORMATIONS DES PHOTOS RELATIVES AUX ACTIVITÉS, DONT LE NOM DU 
FICHIER POUR SAVOIR OÙ ALLER LE CHERCHER

--------------------------------------------------------------------------------------
Par: Anthony Brochu et Marie-Ève Massé
Novembre 2017
Club Contact
------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ProjetSiteDeRencontre.Models
{
    public class PhotosActivite
    {
        [Key]
        public int noPhotoActivite { get; set; }

        [DisplayName("Photo de profil"),
            Required(ErrorMessage = "Veuillez choisir une photo principale.")]
        public bool photoPrincipale { get; set; }

        [DisplayName("Nom fichier"),
            StringLength(70)]
        public string nomFichierPhoto { get; set; }

        //Clé étrangère
        public int noActivite { get; set; }
        public virtual Activite activite { get; set; }

        public int? noMembreQuiPublie { get; set; }
        public virtual Membre membreQuiPublie { get; set; }
    }
}