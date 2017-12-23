/*------------------------------------------------------------------------------------

CLASSE DE SERVICE POUR LES RAISONS DES MEMBRES SUR LE SITE

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
    public class RaisonsSurSite
    {
        public RaisonsSurSite()
        {
            List<Membre> listeMembres = new List<Membre>();
        }

        [Key]
        public int noRaisonSurSite { get; set; }

        [DisplayName("La raison"),
            StringLength(25)]
        public string raison { get; set; }

        //Relation n-n
        public virtual List<Membre> listeMembre { get; set; }
    }
}