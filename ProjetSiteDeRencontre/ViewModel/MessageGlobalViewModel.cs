using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ProjetSiteDeRencontre.ViewModel
{
    public class MessageGlobalViewModel
    {
        public bool? envoyerUniquementAPremium { get; set; }

        [DisplayName("Contenu du message"),
            StringLength(1000, ErrorMessage = "Le contenu peut avoir au maximum 1000 caractères.")]
        public string contenuMessage { get; set; }

        [DisplayName("Objet"),
            StringLength(50, ErrorMessage = "L'objet du message peut avoir au maximum 50 caractères.")]
        public string sujetMessage { get; set; }
    }
}