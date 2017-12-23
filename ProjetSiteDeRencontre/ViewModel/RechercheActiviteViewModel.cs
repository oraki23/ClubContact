/*------------------------------------------------------------------------------------

VIEWMODEL UTILISÉ POUR LA RECHERCHE D'ACTIVITÉS

--------------------------------------------------------------------------------------
Par: Anthony Brochu et Marie-Ève Massé
Novembre 2017
Club Contact
------------------------------------------------------------------------------------*/

using ProjetSiteDeRencontre.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace ProjetSiteDeRencontre.ViewModel
{
    public class RechercheActiviteViewModel
    {
        public RechercheActiviteViewModel()
        {
            resultatsActivite = new List<Activite>();
        }

        public DateTime? dateRecherche { get; set; }

        [DisplayName("Mot clé")]
        public string motCle { get; set; }

        [DisplayName("Thème")]
        public int? noTheme { get; set; }

        public bool? activiteFuturs { get; set; }

        [DisplayName("Activités que je participe uniquement")]
        public bool uniquementActivitesQueJeParticipe { get; set; }

        public int nbActivitesTrouvesTotal { get; set; }

        [DisplayName("Activités que j'organise")]
        public bool organiseParMoi { get; set; }

        public string triPar { get; set; }

        public AvisActivite avisMembreConnecte { get; set; }

        public List<DateTime> datesOuIlYAActivites { get; set; }

        public List<Activite> resultatsActivite { get; set; }
    }
}