/*------------------------------------------------------------------------------------

CLASSE CONTENANT LES INFORMATIONS DES CADEAUX QUI SONT ENVOYÉS ENTRE LES MEMBRES

--------------------------------------------------------------------------------------
Par: Anthony Brochu et Marie-Ève Massé
Novembre 2017
Club Contact
------------------------------------------------------------------------------------*/

using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjetSiteDeRencontre.Models
{
    public class Gift
    {
        [Key]
        public int noGift { get; set; }

        [DataType(DataType.Date),
            Column(TypeName = "datetime2"),
            DisplayName("Date d'envoi"),
            DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy")]
        public DateTime dateEnvoi { get; set; }

        public bool vientDePremium { get; set; }

        public bool supprimeDeReceveur { get; set; }

        //Clés étrangères
        public int noMembreEnvoyeur { get; set; }
        public virtual Membre membreEnvoyeur { get; set; }

        public int noMembreReceveur { get; set; }
        public virtual Membre membreReceveur { get; set; }

        public int noEmoticonEnvoye { get; set; }
        public virtual Emoticon emoticonEnvoye { get; set;}
    }
}