/*------------------------------------------------------------------------------------

VIEWMODEL UTILISÉ DANS LA SECTION GESTION DU CONTRÔLEUR ADMIN, 
CONTENANT LES INFORMATIONS POUR LA RECHERCHE DES DIFFÉRENTS ÉLEMENTS
ET POUR LA MODIFICATION DES SIGNALEMENTS

--------------------------------------------------------------------------------------
Par: Anthony Brochu et Marie-Ève Massé
Novembre 2017
Club Contact
------------------------------------------------------------------------------------*/

using ProjetSiteDeRencontre.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace ProjetSiteDeRencontre.ViewModel
{
    public class GestionViewModel
    {
        public int? noTabSelected { get; set; }

        //Signalement
        public List<Signalement> lesSignalement { get; set; }

        public int? nbSignalementTotalTrouves { get; set; }

        public int? noEtatSignalementRecherche { get; set; }

        public int? nbSignalementTotalRecuSite { get; set; }

        public int? nbSignalementTotalNouveau { get; set; }

        public int? nbSignalementTotalTraites { get; set; }

        public List<CommentaireSignalement> nouveauxCommentaires { get; set; }

        //Tout ce qui attrait à Membres
        public List<Membre> lesMembres { get; set; }

        public bool? membresPremiumUniquement { get; set; }

        public int? nbMembresTotalTrouves { get; set; }

        public int? ageMin { get; set; }

        public int? ageMax { get; set; }

        public string triMembrePar { get; set; }

        public int? noVille { get; set; }
        
        public int? noProvince { get; set; }

        public bool afficherMembresDesactiver { get; set; }

        //Tout ce qui attrait à l'abonnement
        public double? revenuTotal { get; set; }

        public double? TPSTotal { get; set; }
        
        public double? TVQTotal { get; set; }

        public int? nbAbonnements1Mois { get; set; }

        public int? nbAbonnements6Mois { get; set; }

        public int? nbAbonnements12Mois { get; set; }

        public int? nbDesabonnementTotal { get; set; }

        public List<Abonnement> listeDesAbonnements { get; set; }

        //Gestion des dates
        public int? jourDebut { get; set; }

        public int? moisDebut { get; set; }

        public int? anneeDebut { get; set; }

        public int? jourFin { get; set; }

        public int? moisFin { get; set; }

        public int? anneeFin { get; set; }
    }
}