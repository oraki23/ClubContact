/*------------------------------------------------------------------------------------

CLASSE DE SERVICE POUR LES SILHOUETTE DES MEMBRES

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
    public class Silhouette
    {
        [Key]
        public int noSilhouette { get; set; }

        [DisplayName("Nom silhouette"),
            StringLength(25)]
        public string nomSilhouette { get; set; }
    }
}