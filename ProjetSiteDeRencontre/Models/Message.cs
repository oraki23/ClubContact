/*------------------------------------------------------------------------------------

CLASSE CONTENANT LES INFORMATIONS DES DIFFÉRENTS MESSAGES QUI S'ENVOIENT 
ENTRE LES MEMBRES

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
    public class Message
    {
        [Key]
        public int noMessage { get; set; }

        [DataType(DataType.Date),
            Column(TypeName = "datetime2"),
            DisplayName("Date de réception"),
            DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy")]
        public DateTime dateEnvoi { get; set; }

        [DisplayName("Contenu du message"),
            StringLength(1000, ErrorMessage = "Le contenu peut avoir au maximum 1000 caractères.")]
        public string contenuMessage { get; set; }

        [DisplayName("Objet"),
            StringLength(50, ErrorMessage = "L'objet du message peut avoir au maximum 50 caractères.")]
        public string sujetMessage { get; set; }

        public bool lu { get; set; }

        public bool dansCorbeilleCoteEnvoyeur { get; set; }
        public bool dansCorbeilleCoteReceveur { get; set; }

        public bool supprimerCoteEnvoyeur { get; set; }
        public bool supprimerCoteReceveur { get; set; }

        //Clés étrangères
        public int? noMembreEnvoyeur { get; set; }
        public virtual Membre membreEnvoyeur { get; set; }

        public int noMembreReceveur { get; set; }
        public virtual Membre membreReceveur { get; set; }

    }
}