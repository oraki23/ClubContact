/*------------------------------------------------------------------------------------

MODELE CONTENANT LES INFOMRMATIONS RELATIF AUX CONNEXIONS DES MEMBRES AU SITE

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
    public class Connexion
    {
        [Key]
        public int noConnexion { get; set; }

        [DataType(DataType.Date),
          Column(TypeName = "datetime2"),
          DisplayName("Date de la connexion"),
          DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy"),
          Required(ErrorMessage = "La date de la connexion est requise")]
        public DateTime dateConnexion { get; set; }

        public bool? premiumAuMomentDeConnexion { get; set; }

        //Clé étrangère
        public int? noMembre { get; set; }
        public virtual Membre membre { get; set; }
    }
}