/*------------------------------------------------------------------------------------

CLASSE INUTILISÉ, MAIS TOUT DE MÊME GARDÉ POUR UNE FUTURE IMPLÉMENTATION.
PERMETTRA AUX MEMBRES DE PUBLIER DES PUBLICATIONS QUI SERONT DATÉS ET QUI SERONT
VISIBLES PAR D'AUTRES MEMBRES

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
    public class Publication
    {
        [Key]
        public int noPublication { get; set; }

        [DataType(DataType.Date),
            Column(TypeName = "datetime2"),
            DisplayName("Date de publication"),
            DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy"),
            Required(ErrorMessage = "La date de publication est requise")]
        public DateTime DatePublication { get; set; }

        [DisplayName("Contenu publication"),
            StringLength(100)]
        public string contenu { get; set; }

        //Clés étrangères
        public int noMembreEnvoyeur { get; set; }
        public virtual Membre envoyeur { get; set; }
    }
}