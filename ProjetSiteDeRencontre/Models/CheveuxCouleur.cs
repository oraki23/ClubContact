/*------------------------------------------------------------------------------------

CLASSE DE SERVICE POUR LES COULEURS DE CHEVEUX DES MEMBRES

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
    public class CheveuxCouleur
    {
        [Key]
        public int noCheveuxCouleur { get; set; }

        [DisplayName("Couleur de cheveux"),
            Required,
            StringLength(30)]
        public string nomCheveuxCouleur { get; set; }
    }
}