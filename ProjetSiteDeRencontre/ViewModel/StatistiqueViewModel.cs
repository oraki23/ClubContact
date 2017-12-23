/*------------------------------------------------------------------------------------

VIEWMODEL UTILISÉ DANS LA SECTION STATISTIQUES DU CONTRÔLEUR ADMIN, 
CONTENANT LES INFORMATIONS POUR LA RECHERCHE DES DIFFÉRENTES STATISTIQUES
(RECHERCHE PAR DATE PRINCIPALE)

--------------------------------------------------------------------------------------
Par: Anthony Brochu et Marie-Ève Massé
Novembre 2017
Club Contact
------------------------------------------------------------------------------------*/

using ProjetSiteDeRencontre.Models;
using System.Collections.Generic;

namespace ProjetSiteDeRencontre.ViewModel
{
    public class StatistiqueViewModel
    {
        public int? noTabSelected { get; set; }

        //Gestion des dates
        public int? jourDebut { get; set; }

        public int? moisDebut { get; set; }

        public int? anneeDebut { get; set; }

        public int? jourFin { get; set; }

        public int? moisFin { get; set; }

        public int? anneeFin { get; set; }

        //Messagerie

        public int? nbCadeauxEnvoyes { get; set; }

        public int? nbMessagesEnvoyes { get; set; }


        //Activités
        public bool? activitePayante { get; set; }

        public int? noVille { get; set; }

        public int? noProvince { get; set; }

        public int? noThemeRecherche { get; set; }

        public int? nbActivitesOrganisesTrouves { get; set; }

        public int? nbActivitesAnnuleeTrouves { get; set; }

        public int? nbParticipantsTrouves { get; set; }

        public List<Membre> listeOrganisateurs { get; set; }

        public bool afficherMembresDesactiver { get; set; }

        //Visites
        public int? nbVisitesGratuits { get; set; }

        public int? nbVisitesPremium { get; set; }

        public int? nbVisitesVisiteurs { get; set; }
    }
}