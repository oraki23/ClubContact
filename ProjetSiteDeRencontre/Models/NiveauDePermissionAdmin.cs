/*------------------------------------------------------------------------------------

CLASSE DE SERVICE POUR LES DIFFÉRENTS NIVEAUX DE PERMISSIONS ADMINISTRATEUR

--------------------------------------------------------------------------------------
Par: Anthony Brochu et Marie-Ève Massé
Novembre 2017
Club Contact
------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ProjetSiteDeRencontre.Models
{
    public class NiveauDePermissionAdmin
    {
        [Key]
        public int noNiveauDePermissionAdmin { get; set; }

        [StringLength(30, ErrorMessage = "Le nom du niveau de permission ne doit pas être plus de 30 charactères."),
            Required(ErrorMessage = "Le nom du niveau de permission est nécessaire.")]
        public string nomNiveauDePermissionAdmin { get; set; }

    }
}