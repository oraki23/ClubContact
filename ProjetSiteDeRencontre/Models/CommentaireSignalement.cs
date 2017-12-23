/*------------------------------------------------------------------------------------

CLASSE CONTENANT UN COMMENTAIRE SUR UN SIGNALEMENT ENVOYÉ PAR UN ADMINISTRATEUR

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
    public class CommentaireSignalement
    {
        [Key]
        public int noCommentaireSignalement { get; set; }

        [StringLength(300, ErrorMessage = "Le commentaire ne peut pas faire plus de 300 caractères.")]
        public string commentaireSignalement { get; set; }

        [DataType(DataType.Date),
            Column(TypeName = "datetime2"),
            DisplayName("Date du commentaire"),
            DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy")]
        public DateTime dateCommentaire { get; set; }

        public int noCompteAdminEnvoyeur { get; set; }
        public virtual CompteAdmin compteAdminEnvoyeur { get; set; }

        public int noSignalementLie { get; set; }
        public virtual Signalement signalementLie { get; set; }
    }
}