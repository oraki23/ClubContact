/*------------------------------------------------------------------------------------

MODEL CONTENANT TOUS LES INFORMATIONS RELATIVES AUX COMPTES ADMINISTRATEURS

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

namespace ProjetSiteDeRencontre.Models
{
    public class CompteAdmin
    {
        [Key]
        public int noCompteAdmin { get; set; }

        [Index(IsUnique = true),
            NomCompteAdminUniqueDansBD,
            StringLength(50, ErrorMessage = "Votre nom de compte ne doit contenir plus de 50 charactères."),
            Required(ErrorMessage = "Le nom du compte Administrateur est requis."),
            DisplayName("Compte")]
        public string nomCompte { get; set; }

        [StringLength(100),
            DisplayName("Mot de passe")]
        public string motDePasseHashe { get; set; }

        [NotMapped,
            DataType(DataType.Password),
            DisplayName("Mot de passe")]
        public string motDePasse
        {
            //on ne veut pas afficher le mot de passe
            get { return ""; }

            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    motDePasseHashe = hashedPwd(value).ToString();
                }
            }
        }


        //Clé étrangères
        [Required(ErrorMessage = "Le niveau de permission ne doit pas être vide."),
            DisplayName("Niveau de permission")]
        public int noPermission { get; set; }
        public virtual NiveauDePermissionAdmin permission { get; set; }

        public static object hashedPwd(string password)
        {
            HashAlgorithm hashAlg = new SHA256CryptoServiceProvider();
            byte[] bytValue = System.Text.Encoding.UTF8.GetBytes(password);
            byte[] bytHash = hashAlg.ComputeHash(bytValue);
            string base64 = System.Convert.ToBase64String(bytHash);
            return base64;
        }
    }
}