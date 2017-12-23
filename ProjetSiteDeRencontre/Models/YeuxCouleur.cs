/*------------------------------------------------------------------------------------

CLASSE DE SERVICE POUR LES COULEURS DES YEUX DES MEMBRES

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
    public class YeuxCouleur
    {
        [Key]
        public int noYeuxCouleur { get; set; }

        [DisplayName("Couleur yeux"),
            StringLength(20)]
        public string nomYeuxCouleur { get; set; }
    }
}