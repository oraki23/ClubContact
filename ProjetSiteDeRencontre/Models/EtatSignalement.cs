/*------------------------------------------------------------------------------------

CLASSE DE SERVICE POUR LES ÉTATS QUE PEUVENT PRENDRE LES SIGNALEMENTS

--------------------------------------------------------------------------------------
Par: Anthony Brochu et Marie-Ève Massé
Novembre 2017
Club Contact
------------------------------------------------------------------------------------*/

using System.ComponentModel.DataAnnotations;

namespace ProjetSiteDeRencontre.Models
{
    public class EtatSignalement
    {
        [Key]
        public int noEtatSignalement { get; set; }

        [StringLength(30, ErrorMessage = "L'état du signalement doit avoir 30 caractères et moins."),
            Required(ErrorMessage = "Le nom de l'état de signalement est obligatoire.")]
        public string nomEtatSignalement { get; set; }
    }
}