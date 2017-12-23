/*------------------------------------------------------------------------------------

MODEL CONTENANT TOUS LES PROPRIÉTÉS RELATIFS AUX AVIS DES MEMBRES À LA SUITE D'UNE ACTIVITÉ

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
    public class AvisActivite
    {
        [Key]
        public int noAvisActivite { get; set; }

        [Range(1, 5)]
        public int? cote { get; set; }

        [StringLength(200, ErrorMessage = "Votre commentaire ne doit pas avoir plus de 200 caractères.")]
        public string commentaire { get; set; }

        public int noMembreEnvoyeur { get; set; }
        public virtual Membre membreEnvoyeur { get; set; }

        public int noActiviteAssocie { get; set; }
        public virtual Activite activiteAssocie { get; set; }
    }
}