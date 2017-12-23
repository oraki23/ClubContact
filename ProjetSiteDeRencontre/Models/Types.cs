/*------------------------------------------------------------------------------------

CLASSE DE SERVICE POUR LES TYPES DES HOBBIES

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
    public class Types
    {
        public Types()
        {
            listeHobbies = new List<Hobbie>();
        }

        [Key]
        public int noType { get; set; }

        [DisplayName("Nom du type de hobbie"),
            StringLength(25)]
        public string nomType { get; set; }


        public virtual List<Hobbie> listeHobbies { get; set; }

    }
}