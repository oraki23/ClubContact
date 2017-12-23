/*------------------------------------------------------------------------------------

CLASSE CONTENANT LES INFORMATIONS SUR UN TRAITEMENT D'UN SIGNALEMENT

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
    public class TraitementSignalement
    {
        [Key]
        public int noTraitementSignalement { get; set; }

        [DataType(DataType.Date),
            Column(TypeName = "datetime2"),
            DisplayName("Date du commentaire"),
            DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy")]
        public DateTime dateTraitementSignalement { get; set; }


        public int noSignalementLie { get; set; }

        public virtual Signalement signalementLie { get; set; }

        public int noCompteAdminTraiteur { get; set; }
        public virtual CompteAdmin compteAdminTraiteur { get; set; }


        public int? noMessageLie { get; set; }
        public virtual Message messageLie { get; set; }


        public int noActionTraitement { get; set; }
        public virtual ActionTraitement actionTraitement { get; set; }
    }
}