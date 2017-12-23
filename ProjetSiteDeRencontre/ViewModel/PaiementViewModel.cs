/*------------------------------------------------------------------------------------

VIEWMODEL UTILISÉ AFIN DE CONTENIR LES INFORMATIONS DU PAIEMENT POUR PASSER À 
PREMIUM.

--------------------------------------------------------------------------------------
Par: Anthony Brochu et Marie-Ève Massé
Novembre 2017
Club Contact
------------------------------------------------------------------------------------*/

using ProjetSiteDeRencontre.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ProjetSiteDeRencontre.ViewModel
{
    public class PaiementViewModel
    {
        public int noMembre { get; set; }

        [Required(ErrorMessage = "Veuillez choisir combien de mois vous désirez vous abonner."),
            Range(1,12)]
        public int nbMois { get; set; }

        [Required(ErrorMessage = "Veuillez entrer le prenom inscrit sur la carte."),
            DisplayName("Prénom du titulaire de la carte")]
        public string prenomSurCarte { get; set; }

        [Required(ErrorMessage = "Veuillez entrer le nom inscrit sur la carte."),
            DisplayName("Nom du titulaire de la carte")]
        public string nomSurCarte { get; set; }

        [Required(ErrorMessage = "Veuillez choisir le type de carte."),
            DisplayName("Type de carte")]
        public string typeCarte { get; set; }

        [Required(ErrorMessage = "Le no de la carte est nécessaire."),
            StringLength(16, MinimumLength = 16, ErrorMessage = "Votre no de carte doit avoir 16 chiffres."),
            RegularExpression("^[0-9]*$", ErrorMessage = "Le no de carte de crédit doit avoir uniquement des chiffres."),
            DisplayName("Numéro de la carte")]
        public string noCarteCredit { get; set; }

        [Required(ErrorMessage = "Le code de vérification de la carte est nécessaire."),
            StringLength(3, MinimumLength = 3, ErrorMessage = "Votre code de vérification doit avoir 3 chiffres."),
            RegularExpression("^[0-9]*$", ErrorMessage = "Le code de vérification doit avoir uniquement des chiffres."),
            DisplayName("Code de vérification (à 3 chiffres)")]
        public string codeVerification { get; set; }

        [Required(ErrorMessage = "Le mois d'expiration de la carte est nécessaire."),
            Range(1, 12, ErrorMessage = "Veuillez mettre 1 ou 2 chiffres pour le mois d'expiration.")
            DisplayName("Mois d'expiration")]
        public int? carteMoisExpiration { get; set; }

        [Required(ErrorMessage = "L'année d'expiration de la carte est nécessaire."),
            DisplayName("Année d'expiration"),
            Range(2017, int.MaxValue, ErrorMessage = "La date doit être l'année actuelle ou plus haut.")]
        public int? carteAnneeExpiration { get; set; }

        [Required(ErrorMessage = "Veuillez entrer votre adresse de facturation."),
            DisplayName("Adresse de facturation")]
        public string adresseFacturation { get; set; }

        [Required(ErrorMessage = "Veuillez entrer votre ville de facturation."),
            DisplayName("Ville")]
        public string villeFacturation { get; set; }

        [Required(ErrorMessage = "Veuillez entrer votre province de facturation.")]
        public int? noProvince { get; set; }
        [DisplayName("Province")]
        public virtual Province provinceFacturation { get; set; }

        [Required(ErrorMessage = "Veuillez entrer votre code postal de facturation."),
            DisplayName("Code Postal"),
            StringLength(7, ErrorMessage = "Le code postal doit contenir maximum 7 caractères.")]
        public string codePostalFacturation { get; set; }

        public double? TPS { get; set; }

        public double? TVQ { get; set; }
    }
}