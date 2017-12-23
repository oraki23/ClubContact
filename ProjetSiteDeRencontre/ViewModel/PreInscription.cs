/*------------------------------------------------------------------------------------

VIEWMODEL UTILISÉ SUR LA PAGE HOME AVEC LE FORMULAIRE DE "PRÉINSCRIPTION" OÙ LE NOUVEL
UTILISATEUR COMMENCERA À ENTRER SES INFORMATIONS

--------------------------------------------------------------------------------------
Par: Anthony Brochu et Marie-Ève Massé
Novembre 2017
Club Contact
------------------------------------------------------------------------------------*/

using System.ComponentModel.DataAnnotations;
using ProjetSiteDeRencontre.Models;

namespace ProjetSiteDeRencontre.ViewModel
{
    public class PreInscription
    {
        [Required(ErrorMessage = "Veuillez entrer votre sexe.")]
        public bool? homme { get; set; }

        [Required(ErrorMessage = "Veuillez choisir une raison.")]
        public RaisonsSurSite raisonsSurSite { get; set; }
    }
}