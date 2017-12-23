/*------------------------------------------------------------------------------------

CLASSE DE SERVICE POUR LES RELIGIONS DES MEMBRES

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
    public class Religion
    {
        [Key]
        public int noReligion { get; set; }

        [DisplayName("La religion"),
            StringLength(25)]
        public string religion{ get; set; }
    }
}