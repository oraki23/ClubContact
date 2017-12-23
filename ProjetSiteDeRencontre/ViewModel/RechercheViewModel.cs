/*------------------------------------------------------------------------------------

VIEWMODEL UTILISÉ POUR LA RECHERCHE DE MEMBRES

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
    public class RechercheViewModel
    {
        public List<Membre> membres { get; set; }

        [DisplayName("Je recherche")]
        public bool? homme { get; set; }

        [Range(18, int.MaxValue, ErrorMessage = "Veuillez choisir un âge de 18 ans et plus.")]
        [NombrePlusPetitQue("ageMax", "L'âge minimum doit être plus petite que l'âge maximum.")]
        [DisplayName("Âge")]
        public int? ageMin { get; set; }

        //[NombrePlusGrandQue("ageMin", "L'âge maximum doit être plus grande que l'âge minimum.")]
        public int? ageMax { get; set; }

        [DisplayName("Qui recherche")]
        public bool? chercheHomme { get; set; }

        [DisplayName("Objectif")]
        public int? noRaisonsSite { get; set; }

        [DisplayName("Province")]
        public int? noProvince { get; set; }

        [DisplayName("Ville")]
        public int? noVille { get; set; }

        [DisplayName("Distance maximale")]
        public int? distanceKmDeMoi { get; set; }

        //Physique
        [DisplayName("Cheveux")]
        public int? noCheveuxCouleur { get; set; }

        [DisplayName("Yeux")]
        public int? noYeuxCouleur { get; set; }

        [DisplayName("Silhouette")]
        public int? noSilhouette { get; set; }

        [DisplayName("Taille")]
        public int? noTaille { get; set; }

        //Informations supplémentaires
        [Range(0, int.MaxValue, ErrorMessage = "Veuillez choisir un nombre d'enfants positif.")]
        [NombrePlusPetitQue("nbEnfantsMax", "Le nombre d'enfants minimum doit être plus petit que le maximum.")]
        [DisplayName("Nombre d'enfants")]
        public int? nbEnfantsMin { get; set; }

        //[Range(0, int.MaxValue, ErrorMessage = "Veuillez choisir un nombre d'enfants positif.")]
        //[NombrePlusGrandQue("nbEnfantsMin", "Le nombre d'enfants maximum doit être plus grand que le minimum.")]
        public int? nbEnfantsMax { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Veuillez choisir un nombre d'animaux positif.")]
        [NombrePlusPetitQue("nbAnimauxMax", "Le nombre d'animaux minimum doit être plus petit que le maximum.")]
        [DisplayName("Nombre d'animaux")]
        public int? nbAnimauxMin { get; set; }

        //[Range(0, int.MaxValue, ErrorMessage = "Veuillez choisir un nombre d'animaux positif.")]
        //[NombrePlusGrandQue("nbAnimauxMin", "Le nombre d'animaux maximum doit être plus grand que le minimum.")]
        public int? nbAnimauxMax { get; set; }

        [DisplayName("Fumeur")]
        public bool? fumeur { get; set; }

        [DisplayName("Origine")]
        public int? noOrigine { get; set; }

        [DisplayName("Religion")]
        public int? noReligion { get; set; }

        [DisplayName("Désir avoir des enfants")]
        public int? noDesirEnfant { get; set; }

        [DisplayName("Occupation")]
        public int? noOccupation { get; set; }

        [DisplayName("Situation financière")]
        public int? noSituationFinanciere { get; set; }

        [DisplayName("Niveau d'études")]
        public int? noNiveauEtude { get; set; }

        public decimal? latMembreConnectee { get; set; }
        public decimal? lngMembreConnectee { get; set; }

        [DisplayName("Type d'activité")]
        public int? noTypeActiviteRecherche { get; set; }

        [DisplayName("Champ d'intérêt")]
        public int? noTypeInterets { get; set; }

        //TRI
        
        //On pourrait mettre le nom d'une propriété
        public string triPar { get; set; }
    }
}