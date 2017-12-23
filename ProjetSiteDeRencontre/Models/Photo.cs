/*------------------------------------------------------------------------------------

CLASSE CONTENANT LES DIFFÉRENTES INFORMATIONS SUR LES PHOTOS DES MEMBRES STOCKÉS DANS LA BD.

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
    public class Photo
    {
        [Key]
        public int noPhoto { get; set; }

        [DisplayName("Photo de profil"),
            Required(ErrorMessage = "Veuillez choisir une photo de profil")]
        public bool photoProfil { get; set; }

        [DisplayName("Nom fichier"),
            StringLength(70)]
        public string nomFichierPhoto { get; set; }

        //Clé étrangère
        public int noMembre { get; set; }
        public virtual Membre membre { get; set; }
    }
}