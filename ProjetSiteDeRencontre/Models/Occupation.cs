/*------------------------------------------------------------------------------------

CLASSE DE SERVICE POUR LES OCCUPATIONS DES MEMBRES

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
    public class Occupation
    {
        [Key]
        public int noOccupation { get; set; }

        [Required,
            DisplayName("Occupation"),
            StringLength(30)]
        public string nomOccupation { get; set; }
    }
}