/*------------------------------------------------------------------------------------

CLASSE DE SERVICE POUR LES NIVEAUX D'ÉTUDES POSSIBLES DES MEMBRES

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
    public class NiveauEtude
    {
        [Key]
        public int noNiveauEtude { get; set; }

        [Required,
            DisplayName("Études"),
            StringLength(30)]
        public string nomNiveauEtude { get; set; }
    }
}