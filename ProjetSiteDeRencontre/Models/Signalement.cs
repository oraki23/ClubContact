/*------------------------------------------------------------------------------------

CLASSE CONTENANT LES INFORMATIONS SUR LES SIGNALEMENT CONTRE UN MEMBRE, DE MÊME QUE LE MEMBRE
QUI A FAIT LA PLAINTE.

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

namespace ProjetSiteDeRencontre.Models
{
    public class Signalement
    {
        [Key]
        public int noSignalement { get; set; }

        [DataType(DataType.Date),
            Column(TypeName = "datetime2"),
            DisplayName("Date signalement"),
            DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy")]
        public DateTime dateSignalement { get; set; }


        public int noMembreFaisantPlainte { get; set; }
        public virtual Membre membreFaisantPlainte { get; set; }

        public int noMembreContreQuiEstPlainte { get; set; }
        public virtual Membre membreContreQuiEstPlainte { get; set; }

        public string raisonDeLaPlainte { get; set; }


        //Message joint (dans le cas qu'il signale alors qu'il est sur un message)
        public int? noMessageJoint { get; set; }
        public virtual Message messageJoint { get; set; }

        //L'état actuel du signalement (obligatoire) comme, par exemple, non-lu, lu, traité, suivi...
        [DisplayName("État du signalement"),
            Required(ErrorMessage = "L'état du signalement est obligatoire")]
        public int noEtatSignalementActuel { get; set; }

        public virtual EtatSignalement etatSignalementActuel { get; set; }
        

        [DataType(DataType.Date),
            Column(TypeName = "datetime2"),
            DisplayName("Date traitement"),
            DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy")]
        public DateTime? dateSuiviNecessaire { get; set; }

        public int? noCompteAdmin { get; set; }
        public virtual CompteAdmin adminQuiTraite { get; set; }


        public virtual List<TraitementSignalement> lesTraitementsSurCeSignalement { get; set; }

        public virtual List<CommentaireSignalement> lesCommentairesSurCeSignalement { get; set; }
    }
}