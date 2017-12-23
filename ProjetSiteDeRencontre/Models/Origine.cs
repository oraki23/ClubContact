/*------------------------------------------------------------------------------------

CLASSE DE SERVICE POUR LES ORIGINES DES MEMBRES

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
    public class Origine
    {
        [Key]
        public int noOrigine { get; set; }

        [DisplayName("L'origine"),
            StringLength(50)]
        public string origine { get; set; }
    }
}