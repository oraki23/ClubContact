/*------------------------------------------------------------------------------------

CLASSE DE SERVICE POUR LES HOBBIES POSSIBLES SUR LE SITE

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
    public class Hobbie
    {
        public Hobbie()
        {
            List<Membre> listeMembres = new List<Membre>();
        }
        [Key]
        public int noHobbie { get; set; }

        [DisplayName("Nom du hobbie"),
            Required,
            StringLength(25)]
        public string nomHobbie { get; set; }

        //Clé étrangère
        public int noType { get; set; }
        public virtual Types type { get; set; }

        //Relation n-n
        public virtual List<Membre> listeMembre { get; set; }
    }
}