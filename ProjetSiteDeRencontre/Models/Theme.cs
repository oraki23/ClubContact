/*------------------------------------------------------------------------------------

CLASSE DE SERVICE POUR LES `HÈMES DES ACTIVITÉS

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
    public class Theme
    {
        [Key]
        public int noTheme { get; set; }

        [DisplayName("Le thème"),
            StringLength(25)]
        public string theme { get; set; }
    }
}