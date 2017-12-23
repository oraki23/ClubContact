/*------------------------------------------------------------------------------------

CLASSE DE SERVICE POUR LA LISTE DES EMOTICONS

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
    public class Emoticon
    {
        [Key]
        public int noEmoticon { get; set; }

        public bool premiumOnly { get; set; }

        [StringLength(40)]
        public string nomFichierEmoticon { get; set; }
    }
}