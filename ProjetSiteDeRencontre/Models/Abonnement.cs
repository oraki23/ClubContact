/*------------------------------------------------------------------------------------

MODEL CONTENANT TOUS LES PROPRIÉTÉS RELATIFS AUX ABONNEMENTS VERS PREMIUM DES MEMBRES

--------------------------------------------------------------------------------------
Par: Anthony Brochu et Marie-Ève Massé
Novembre 2017
Club Contact
------------------------------------------------------------------------------------*/

using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjetSiteDeRencontre.Models
{
    /// <summary>
    /// Classe permettant la gestion des abonnements des membres
    /// </summary>
    public class Abonnement
    {
        [Key]
        public int noAbonnement { get; set; }

        [Range(1, 12, ErrorMessage = "Le type d'abonnement doit être le nombre de mois de 1 à 12"),
            Required(ErrorMessage = "Le type d'abonnement est requis")]
        public int typeAbonnement { get; set; }

        [DataType(DataType.Date),
            Column(TypeName = "datetime2"),
            DisplayName("Date de début de l'abonnement"),
            DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd"),
            Required(ErrorMessage = "La date de début d'abonnement est requise")]           //Ajouter la génération de la journée (date auj)
        public DateTime dateDebut { get; set; }

        [DataType(DataType.Date),
            Column(TypeName = "datetime2"),
            DisplayName("Date de fin de l'abonnement"),
            DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd"),
            Required(ErrorMessage = "La date de fin d'abonnement est requise")]         //Ajouter la génération de la journée (date auj + 30)
        public DateTime dateFin { get; set; }

        [DataType(DataType.Date),
            Column(TypeName = "datetime2"),
            DisplayName("Date du paiement de l'abonnement"),
            DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd"),//Ajouter la sauvegarde de la journée ou se fait le paiement
            Required(ErrorMessage = "Date du paiement de l'abonnement est requise")]
        public DateTime datePaiement { get; set; }

        [DisplayName("Coût total")]
        public double coutTotal { get; set; }

        [Range(0, double.MaxValue)]
        public double prixTPS { get; set; }

        [Range(0, double.MaxValue)]
        public double? prixTVQ { get; set; }

        [DisplayName("Renouvellé?")]
        public bool? renouveler { get; set; }

        //Informations de la carte
        [StringLength(75)]
        public string prenomSurCarte { get; set; }

        [StringLength(75)]
        public string nomSurCarte { get; set; }

        [StringLength(4)]
        public string quatreDerniersChiffres { get; set; }

        [StringLength(10)]
        public string typeCarte { get; set; }

        [StringLength(75)]
        public string adresseFacturation { get; set; }

        [StringLength(75)]
        public string villeFacturation { get; set; }

        [StringLength(7)]
        public string codePostalFacturation { get; set; }

        public int? noProvince { get; set; }

        public Province provinceFacturation { get; set; }

        //Clés étrangères
        public int noMembre { get; set; }
        public virtual Membre membre { get; set; }
    }
}