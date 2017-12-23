/*------------------------------------------------------------------------------------

CLASSE DE SERVICE POUR LE DÉSIR D'AVOIR DES ENFANTS DES MEMBRES

--------------------------------------------------------------------------------------
Par: Anthony Brochu et Marie-Ève Massé
Novembre 2017
Club Contact
------------------------------------------------------------------------------------*/

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ProjetSiteDeRencontre.Models
{
    public class DesirEnfant
    {
        [Key]
        public int noDesirEnfant { get; set; }

        [Required,
            DisplayName("Désire avoir des enfants"),
            StringLength(30)]
        public string desirEnfant { get; set; }
    }
}