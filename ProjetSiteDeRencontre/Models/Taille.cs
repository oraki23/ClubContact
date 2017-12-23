/*------------------------------------------------------------------------------------

CLASSE DE SERVICE POUR LES TAILLES DES MEMBRES

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
    public class Taille
    {
        [Key]
        public int noTaille { get; set; }

        [DisplayName("Taille"),
            StringLength(25)]
        public string taille { get; set; }
    }
}