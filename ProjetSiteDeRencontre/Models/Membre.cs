/*------------------------------------------------------------------------------------

CLASSE CONTENANT TOUS LES INFORMATIONS SUR UN MEMBRE, DE MEME QUE TOUS LES LIENS VERS LES 
INFORMATIONS SE TROUVANT AVEC UN LIEN DANS UNE AUTRE TABL

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
using System.Security.Cryptography;

//Marie-Ève Massé

namespace ProjetSiteDeRencontre.Models
{
    public class Membre
    {

        public Membre()
        {
            listeHobbies = new List<Hobbie>();
            listeRaisonsSurSite = new List<RaisonsSurSite>();
            listeMessagesEnvoyes = new List<Message>();
            listeMessagesRecus = new List<Message>();
            listePublications = new List<Publication>();
            listeActivites = new List<Activite>();
            listeFavoris = new List<Membre>();
            listePhotosMembres = new List<Photo>();
            listeNoire = new List<Membre>();
            listeDeVisitesDeMonProfil = new List<Visite>();
            listeMembresQuiMontBloque = new List<Membre>();
            listeDesVisitesQueJaiFait = new List<Visite>();
            listeMembreQuiMontFavoriser = new List<Membre>();
        }

        [Key]
        public int noMembre { get; set; }

        [Required(ErrorMessage = "Le nom est obligatoire"),
            DisplayName("Nom"),
            StringLength(50),
            RegularExpression("^\\D*$", ErrorMessage = "Votre nom ne peut contenir de chiffres")]
        public string nom { get; set; }

        [Required(ErrorMessage = "Le prénom est obligatoire"),
            DisplayName("Prénom"),
            StringLength(50)
            RegularExpression("^\\D*$", ErrorMessage = "Votre prénom ne peut contenir de chiffres")]
        public string prenom { get; set; }

        [Required(ErrorMessage = "Le surnom est obligatoire"),
            DisplayName("Surnom"),
            StringLength(50)]
        public string surnom { get; set; }

        [DisplayName("Courriel"),
            StringLength(50, MinimumLength = 3, ErrorMessage = "La longueur du courriel ne doit pas dépasser 50 caractères et doit avoir au moins 3 caractères."),
            RegularExpression(@"[A-Za-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,4}", ErrorMessage = "Le courriel doit être valide."), 
            Required(ErrorMessage = "L'adresse courriel est obligatoire"),
            CourrielUniqueDansBD]
        public string courriel { get; set; }

        [DataType(DataType.Date),
            Column(TypeName = "datetime2" ),
            DateInscriptionValide(ErrorMessage = "Vous devez avoir 18 ans et plus pour vous inscrire."),
            DisplayName("Date de naissance"),
            DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}"),
            Required(ErrorMessage = "La date de naissance est requise")]//Ajouter routine erreur pour s'assurer que la personne a bien 18 ans
        public DateTime? dateNaissance { get; set; }

        [DataType(DataType.Date),
            Column(TypeName = "datetime2"),
            DisplayName("Date d'inscription"),
            DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime? dateInscription { get; set; }

        [DisplayName("Sexe"),
            Required]
        public Boolean homme { get; set; }

        public Boolean emailConfirme { get; set; }

        [DisplayName("Description"),
            StringLength(300)]
        public string description { get; set; }

        [DisplayName("En amour, je suis intéressé par")]
        public Boolean rechercheHomme { get; set; }

        [StringLength(100),
            DisplayName("Mot de passe")]
        public string motDePasseHashe { get; set; }

        // Permet de hasher le mot de passe afin de ne pas le stocker en clair dans la BD.
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

        [Range(0, Double.MaxValue, ErrorMessage = "Veuillez entrer un nombre en haut de 0.")
            DisplayName("Nombre d'enfants")]
        public int? nbEnfants { get; set; }

        [Range(0, Double.MaxValue, ErrorMessage = "Veuillez entrer un nombre en haut de 0.")
            DisplayName("Nombre d'animaux")]
        public int? nbAnimaux { get; set; }

        [DisplayName("Fumeur")]
        public Boolean? fumeur { get; set; }

        [DisplayName("Premium"),
            Required] 
        public Boolean premium { get; set; }


        //Clés étrangères
        [Required(ErrorMessage = "La province est requise")]
        public int noProvince { get; set; }
        [DisplayName("Province")]
        public virtual Province province { get; set; }

        [Required(ErrorMessage = "La ville est requise")]
        public int noVille { get; set; }
        [DisplayName("Ville")]
        public virtual Ville ville { get; set; }

        //[NotMapped]
        //public int lat { get
        //    {
        //        return (int)Utilitaires.Utilitaires.trouverLatOuLng(ville.nomVille, province.nomProvince, true);
        //    }
        //    set
        //    {
        //        lat = value;
        //    }
        //}

        //[NotMapped]
        //public int lng
        //{
        //    get
        //    {
        //        return (int)Utilitaires.Utilitaires.trouverLatOuLng(ville.nomVille, province.nomProvince, false);
        //    }
        //    set
        //    {
        //        lng = value;
        //    }
        //}

        //public static Expression<Func<Membre, bool>> EstDansDistanceMax(string nomVille1, string nomProvince1, string nomVille2, string nomProvince2, int distanceKmMax)
        //{
        //    var distanceCalculer = Utilitaires.Utilitaires.calculerDistanceEntreDeuxVilles(nomVille1, nomProvince1, nomVille2, nomProvince2);
        //    return p => distanceCalculer <= distanceKmMax;
        //}

        //public static Expression<Func<Membre, bool>> EstDansDistanceMax(string nomVille1, string nomProvince1, int distanceKmMax)
        //{
        //    return p => Utilitaires.Utilitaires.calculerDistanceEntreDeuxVilles(nomVille1, nomProvince1, p.ville.nomVille, p.province.nomProvince) <= distanceKmMax;
        //}

        public int? noOrigine { get; set; }
        [DisplayName("Origine")]
        public virtual Origine origine { get; set; }

        public int? noReligion { get; set; }
        [DisplayName("Religion")]
        public virtual Religion religion { get; set; }

        public int? noYeuxCouleur { get; set; }
        [DisplayName("Couleur des yeux")]
        public virtual YeuxCouleur yeuxCouleur { get; set; }

        public int? noCheveuxCouleur { get; set; }
        [DisplayName("Couleur des cheveux")]
        public virtual CheveuxCouleur cheveuxCouleur { get; set; }

        public int? noSilhouette { get; set; }
        [DisplayName("Silhouette")]
        public virtual Silhouette silhouette { get; set; }

        public int? noTaille { get; set; }
        [DisplayName("Taille")]
        public virtual Taille taille { get; set; }

        public int? noOccupation { get; set; }
        [DisplayName("Occupation")]
        public virtual Occupation occupation { get; set; }

        public int? noSituationFinanciere { get; set; }
        [DisplayName("Situation Financière")]
        public virtual SituationFinanciere situationFinanciere { get; set; }

        public int? noNiveauEtude { get; set; }
        [DisplayName("Niveau d'étude")]
        public virtual NiveauEtude niveauEtude { get; set; }

        public int? noDesirEnfant { get; set; }
        [DisplayName("Désire avoir des enfants")]
        public virtual DesirEnfant desirEnfant { get; set; }

        //Relations n-n
        public virtual List<Hobbie> listeHobbies { get; set; }

        [DisplayName("Je suis ici pour"), Required(ErrorMessage = "Veuillez entrer au moins une raison.")]
        public virtual List<RaisonsSurSite> listeRaisonsSurSite { get; set; }

        public virtual List<Message> listeMessagesEnvoyes { get; set; }

        public virtual List<Message> listeMessagesRecus { get; set; }

        public virtual List<Gift> listeCadeauxEnvoyes { get; set; }

        public virtual List<Gift> listeCadeauxRecus { get; set; }

        public virtual List<Publication> listePublications { get; set; }

        public virtual List<Activite> listeActivites { get; set; }

        public virtual List<Activite> listeActivitesOrganises { get; set; }

        public virtual List<Membre> listeFavoris { get; set; }

        public virtual List<Membre> listeMembreQuiMontFavoriser { get; set; }

        public virtual List<Membre> listeNoire { get; set; }

        public virtual List<Membre> listeMembresQuiMontBloque { get; set; }

        public virtual List<Membre> listeContacts { get; set; }

        public virtual List<Photo> listePhotosMembres { get; set; }

        public virtual List<Visite> listeDeVisitesDeMonProfil { get; set; }

        public virtual List<Visite> listeDesVisitesQueJaiFait { get; set; }

        public virtual List<Signalement> listeSignalementContre { get; set; }
        public virtual List<Signalement> listeSignalementEnvoyer { get; set; }

        public virtual List<Connexion> listeConnexions { get; set; }

        public virtual List<Abonnement> listeAbonnementsMembre { get; set; }

        [NotMapped]
        public int age {
            get
            {
                return LesUtilitaires.Utilitaires.CalculerAge((DateTime)dateNaissance);
            }
        }

        public static object hashedPwd(string password)
        {
            HashAlgorithm hashAlg = new SHA256CryptoServiceProvider();
            byte[] bytValue = System.Text.Encoding.UTF8.GetBytes(password);
            byte[] bytHash = hashAlg.ComputeHash(bytValue);
            string base64 = System.Convert.ToBase64String(bytHash);
            return base64;
        }

        //Gestion suppression du compte

        public bool? compteSupprimeParAdmin { get; set; }

        [DataType(DataType.Date),
            Column(TypeName = "datetime2"),
            DisplayName("Date de suppression du compte")]
        public DateTime? dateSuppressionDuCompte { get; set; }

    }    
}