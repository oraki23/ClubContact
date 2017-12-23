/*------------------------------------------------------------------------------------

MODEL CONTENANT TOUS LES PROPRIÉTÉS RELATIFS AUX ACTIVITÉS CRÉÉS SUR LE SITE

--------------------------------------------------------------------------------------
Par: Anthony Brochu et Marie-Ève Massé
Novembre 2017
Club Contact
------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ProjetSiteDeRencontre.Models
{
    public class Activite
    {
        public Activite()
        {
            listePhotosActivites = new List<PhotosActivite>();
            listeAvisActivite = new List<AvisActivite>();
            membresParticipants = new List<Membre>();
        }

        [Key]
        public int noActivite { get; set; }

        public bool? annulee { get; set; }

        [DisplayName("Titre"),
            StringLength(50, ErrorMessage = "Le titre de l'activité peut avoir au maximum 50 caractères."),
            Required(ErrorMessage = "Un nom pour l'activité est requis.")]
        public string nom { get; set; }

        [DisplayName("Description"),
            StringLength(300, ErrorMessage = "La description de l'activité peut avoir au maximum 300 caractères."),
            Required(ErrorMessage = "Une description de l'activité est requise.")]
        public string description { get; set; }

        [DataType(DataType.DateTime, ErrorMessage = "Veuillez entrer la date dans le format suivant: aaaa-mm-jj hh:mm"),
            Column(TypeName = "datetime2"),
            DisplayName("Date de l'activité"),
            DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd HH:mm}"),
            Required(ErrorMessage = "La date de l'activité est requise.")
            /*DateActivitePlusGrandQuAujourdhui(ErrorMessage = "Veuillez choisir une date dans le futur.")*/] //A enlever lors d'un initialise
        public DateTime date { get; set; }

        [DisplayName("Coût"),
            Range(0, Double.MaxValue, ErrorMessage = "Le coût doit être positif.")]
        public int? cout { get; set; }

        [DisplayName("Sexe")]
        public bool? hommeSeulement { get; set; }

        [DisplayName("Âge maximum"),
            Range(18, Double.MaxValue, ErrorMessage = "L'activité doit être pour des gens en haut de 18 ans seulement.")]
        public int? ageMax { get; set; }

        [DisplayName("Âge minimum"),
            Range(18, Double.MaxValue, ErrorMessage = "L'activité doit être pour des gens en haut de 18 ans seulement."),
            NombrePlusPetitQue("ageMax", "L'âge minimum doit être plus petite que l'âge maximum.")]
        public int? ageMin { get; set; }

        [DisplayName("Adresse"),
            StringLength(75, ErrorMessage = "L'adresse de l'activité peut avoir au maximum 75 caractères.")]
        [Required(ErrorMessage = "Veuillez entrer une adresse.")]
        public string adresse { get; set; }

        [DisplayName("Nombre de participants maximum"),
            Range(1, Double.MaxValue, ErrorMessage = "Veuillez entrer au moins 1 comme nombre de participants maximum.")]

        public int? nbParticipantsMax { get; set; }


        //Clés étrangères

        [Required(ErrorMessage = "Veuillez choisir une ville.")]
        public int noVille { get; set; }
        public virtual Ville ville { get; set; }

        [Required(ErrorMessage = "Veuillez choisir une province.")]
        public int noProvince { get; set; }
        public virtual Province province { get; set; }

        [Required(ErrorMessage = "Veuillez choisir un thème.")]
        public int noTheme { get; set; }
        public virtual Theme theme { get; set; }

        public int noMembreOrganisateur { get; set; }
        public virtual Membre membreOrganisateur { get; set; }

        public virtual List<Membre> membresParticipants { get; set; }

        public virtual List<PhotosActivite> listePhotosActivites { get; set; }

        public virtual List<AvisActivite> listeAvisActivite { get; set; }
    }

}