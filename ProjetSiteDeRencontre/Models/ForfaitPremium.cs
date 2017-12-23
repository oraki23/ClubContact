/*------------------------------------------------------------------------------------

CLASSE CONTENANT LES INFORMATIONS POUR LES FORFAITS PREMIUM OFFERTS
NOTE:
    DANS LE CONTEXTE ACTUEL, IL NE DEVRAIT PAS EN AVOIR PLUS DE 3, À CAUSE DE
    L'ARCHITECTURE EN PLACE, LA SUPPRESSION ET L'AJOUT N'EST PAS POSSIBLE

--------------------------------------------------------------------------------------
Par: Anthony Brochu et Marie-Ève Massé
Novembre 2017
Club Contact
------------------------------------------------------------------------------------*/

using System.ComponentModel.DataAnnotations;

namespace ProjetSiteDeRencontre.Models
{
    public class ForfaitPremium
    {
        [Key]
        public int noForfaitPremium { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Le nombre de mois de l'abonnement doit être plus que 0."),
            Required(ErrorMessage = "Le nombre de mois de l'abonnement est requis.")]
        public int nbMoisAbonnement { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Le prix par mois de l'abonnement doit être plus que 0."),
            Required(ErrorMessage = "Le prix par mois de l'abonnement est requis.")]
        public double prixParMois { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Le prix total de l'abonnement doit être plus que 0."),
            Required(ErrorMessage = "Le prix total de l'abonnement est requis.")]
        public double prixTotal { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Le pourcentage de rabais total de l'abonnement doit être plus que 0.")]
        public double? pourcentageDeRabais { get; set; }
    }
}