/*------------------------------------------------------------------------------------

CLASSE DE SERVICE CONTENANT LES ACTIONS QUI ONT ÉTÉ FAITES SUR LES SIGNALEMENTS

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
    public class ActionTraitement
    {
        [Key]
        public int noActionTraitement { get; set; }

        public string nomActionTraitement { get; set; }
    }
}