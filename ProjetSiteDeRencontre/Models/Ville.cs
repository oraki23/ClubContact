/*------------------------------------------------------------------------------------

CLASSE DE SERVICE POUR LES VILLES DU CANADA DANS LA BASE DE DONNÉES

--------------------------------------------------------------------------------------
Par: Anthony Brochu et Marie-Ève Massé
Novembre 2017
Club Contact
------------------------------------------------------------------------------------*/

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ProjetSiteDeRencontre.Models
{
    public class Ville
    {
        [Key]
        public int noVille { get; set; }

        [DisplayName("La ville"),
            StringLength(35)]
        public string nomVille { get; set; }

        public decimal? lat { get; set; }
        public decimal? lng { get; set; }

        //Clé étrangère
        public int noProvince { get; set; }
        public virtual Province province { get; set; }
    }
}