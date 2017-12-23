/*------------------------------------------------------------------------------------

VIEWMODEL UTILISÉ DANS LA SECTION OPTIONS DE LA GESTION DU COMPTE D'UN MEMBRE

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
using System.Security.Cryptography;
using System.Web;

namespace ProjetSiteDeRencontre.ViewModel
{
    public class OptionsCompteViewModel
    {
        public int noMembre { get; set; }

        // Permet de hasher le mot de passe afin de ne pas le stocker en clair dans la BD.
        [NotMapped,
            DataType(DataType.Password),
            DisplayName("Mot de passe")]
        public string motDePasse { get; set; }

        public bool premium { get; set; }
    }
}