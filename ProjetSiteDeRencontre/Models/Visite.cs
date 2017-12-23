/*------------------------------------------------------------------------------------

CLASSE PERMETTANT DE STOCKER LES VISITES DES MEMBRES VERS D'AUTRES PROFILS.
LES MEMBRES PREMIUM PEUVENT AINSI VOIR QUI A VU LEUR PROFIL.

--------------------------------------------------------------------------------------
Par: Anthony Brochu et Marie-Ève Massé
Novembre 2017
Club Contact
------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ProjetSiteDeRencontre.Models
{
    public class Visite
    {
        [Key]
        public int noVisite { get; set; }

        [DataType(DataType.Date),
           Column(TypeName = "datetime2"),
           DisplayName("Date de la visite"),
           DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy"),
           Required(ErrorMessage = "La date de la visite est requise")]
        public DateTime dateVisite { get; set; }

        //Clés étrangères
        public int noMembreVisite { get; set; }
        public virtual Membre membreVisite { get; set; }

        public int noMembreVisiteur { get; set; }
        public virtual Membre membreVisiteur { get; set; }
    }
}