/*------------------------------------------------------------------------------------

CLASSE DE SERVICE POUR LA SITUATION FINANCIÈRE DES MEMBRES

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
    public class SituationFinanciere
    {
        [Key]
        public int noSituationFinanciere { get; set; }
        [Required,
            DisplayName("Situation financière"),
            StringLength(30)]
        public string nomSituationFinanciere { get; set; }
    }
}