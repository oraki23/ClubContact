/*------------------------------------------------------------------------------------

CLASSE DE SERVICE POUR LES PROVINCES DU CANADA

--------------------------------------------------------------------------------------
Par: Anthony Brochu et Marie-Ève Massé
Novembre 2017
Club Contact
------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ProjetSiteDeRencontre.Models
{
    public class Province
    {
        public Province()
        {
            listeVilles = new List<Ville>();
        }

        [Key]
        public int noProvince { get; set; }

        [DisplayName("Nom de la province"),
            StringLength(30)]
        public string nomProvince { get; set; }

        [DisplayName("Abréviation de la province"),
            StringLength(2)]
        public string abbrProvince { get; set; }

        //Relations n-n
        public virtual List<Ville> listeVilles { get; set; }
    }
}