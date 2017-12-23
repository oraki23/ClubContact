/*------------------------------------------------------------------------------------

CONTRÔLEUR POUR TOUTE LA PARTIE "UTILITAIRE" DU SITE, COMME:
    INITIALISE
    FONCTIONS UTILES
    TEST

À NOTER QUE CERTAINES PARTIE DE CE CONTRÔLEUR NE DEVRONT PAS ALLER EN PRODUCTION CAR ILS SERONT
INUTILES ET POURRAIENT PRODUIRE DES CONSÉQUENCES CATASTROPHIQUES

--------------------------------------------------------------------------------------
Par: Anthony Brochu et Marie-Ève Massé
Novembre 2017
Club Contact
------------------------------------------------------------------------------------*/

using FizzWare.NBuilder;
using ProjetSiteDeRencontre.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ProjetSiteDeRencontre.LesUtilitaires;
using System.Data.Entity.Infrastructure;

namespace ProjetSiteDeRencontre.Controllers
{
    //On peut commenter cette ligne si la connexion ne fonctionne pas, mais il
    //ne faut pas oublier de la remettre afin que pas n'importe qui puisse
    //initialiser la BD!!
    [Authorize]
    public class UtilitaireController : BaseController
    {
        // GET: Initialise
        /// <summary>
        /// Permet de faire l'initialisation de la base de données
        /// </summary>
        /// <returns></returns>
        /// 
        private ClubContactContext db;

        public UtilitaireController()
        {
            db = new ClubContactContext();
        }

        [AllowAnonymous]
        public string CalculerDistanceAJAX(int noMembreDetails)
        {
            if(Request.IsAuthenticated)
            {
                int noCookie;
                verifierSiCookieNoMembreExiste(out noCookie);

                if(noCookie != -1)
                {
                    if(noCookie != noMembreDetails)
                    {
                        try
                        {
                            Ville villeMembreDetails = db.Membres.Where(m => m.noMembre == noMembreDetails).Select(m => m.ville).FirstOrDefault();
                            Ville villeMembreConnectee = db.Membres.Where(m => m.noMembre == noCookie).Select(m => m.ville).FirstOrDefault();

                            return Math.Round((Utilitaires.calcDistance((decimal)villeMembreDetails.lat,
                            (decimal)villeMembreDetails.lng,
                            (decimal)villeMembreConnectee.lat,
                            (decimal)villeMembreConnectee.lng)), 0).ToString() + " km";
                        }
                        catch(Exception e)
                        {
                            Dictionary<string, string> parametres = new Dictionary<string, string>() {
                                { "noMembreDetails" , noMembreDetails.ToString() },
                                { "noCookie" , noCookie.ToString() }
                            };

                            Elmah.ErrorSignal.FromCurrentContext().Raise(
                                Utilitaires.templateException("CalculerDistanceAJAX", "Utilitaire", "Requête LINQ d'une des 2 villes n'a pas fonctionnée.", e, parametres)
                                );

                            return "Erreur.";
                        }                
                    }
                }
            }

            return "";
        }

        [AllowAnonymous]
        public void SignalerMembre(int noMembreFaisantPlainte, int noMembreContreQuiEstPlainte, string raisonDeLaPlainte, int? noMessageJoint)
        {
            Signalement leSignalement = new Signalement();

            Membre leMembreFaisantPlainte = db.Membres.Where(m => m.noMembre == noMembreFaisantPlainte).FirstOrDefault();

            Membre leMembreContreQuiEstPlainte = db.Membres.Where(m => m.noMembre == noMembreContreQuiEstPlainte).FirstOrDefault();

            EtatSignalement etatSignalementEnvoye = db.EtatSignalements.Where(e => e.nomEtatSignalement == "Nouveau").FirstOrDefault();

            leSignalement.membreContreQuiEstPlainte = leMembreContreQuiEstPlainte;
            leSignalement.membreFaisantPlainte = leMembreFaisantPlainte;
            leSignalement.etatSignalementActuel = etatSignalementEnvoye;
            leSignalement.noEtatSignalementActuel = etatSignalementEnvoye.noEtatSignalement;

            if (noMessageJoint != null)
            {
                Message leMessageJoint = db.Messages.Where(m => m.noMessage == noMessageJoint).FirstOrDefault();

                leSignalement.messageJoint = leMessageJoint;
            }

            leSignalement.raisonDeLaPlainte = raisonDeLaPlainte;

            leSignalement.dateSignalement = DateTime.Now;
            try
            {
                db.Signalements.Add(leSignalement);

                db.SaveChanges();

                //Envoie d'un message automatisé de réception
                Message messageAccuseDeReception = new Message();
                messageAccuseDeReception.dateEnvoi = DateTime.Now;
                messageAccuseDeReception.sujetMessage = "Réception de votre signalement contre " + leMembreContreQuiEstPlainte.surnom;
                messageAccuseDeReception.contenuMessage = "Votre signalement contre " + leMembreContreQuiEstPlainte.surnom + " a bien été reçu, et sera traité par nos administrateurs dans les" +
                        " plus brefs délais. Merci de nous aider à faire de Club Contact un endroit sécure et plaisant!" + Environment.NewLine + Environment.NewLine + "- Le service d'administration de Club Contact";
                messageAccuseDeReception.lu = false;
                messageAccuseDeReception.membreReceveur = leMembreFaisantPlainte;
                messageAccuseDeReception.noMembreReceveur = leMembreFaisantPlainte.noMembre;

                db.Messages.Add(messageAccuseDeReception);
                db.SaveChanges();
            }
            catch(DbUpdateException e)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(Utilitaires.templateException("(AJAX) SignalerMembre", "Utilitaire", "Mise à jour de la base de données à la suite de l'ajout d'une plainte à échoué.", e, null));
            }
        }

        [AllowAnonymous]
        public void EnleverUnePhotoViewBag()
        {
            ViewBag.nbDePhotos--;
        }

        [AllowAnonymous]
        public ActionResult TrouverVillesAPartirDeNoProvince(int noProvince)
        {
            try
            {
                db.Configuration.ProxyCreationEnabled = false; // Évite une référence circulaire
                List<Ville> villesDeLaProvince = db.Villes.Where(vi => vi.noProvince == noProvince).OrderBy(v => v.nomVille).ToList();

                return Json(villesDeLaProvince, JsonRequestBehavior.AllowGet);
            }
            catch(Exception e)
            {
                Dictionary<string, string> parametres = new Dictionary<string, string>()
                {
                    {"noProvince recherchée", noProvince.ToString() }
                };
                throw Utilitaires.templateException("TrouverVillesAPartirDeNoProvince (AJAX)", "Utilitaire",
                    "Requête LINQ n'a pas fonctionnée.",
                    e, parametres);
            }
        }

        [AllowAnonymous]
        public ActionResult TrouverHobbiesAPartirDeNoProvince(int noTypeActiviteRecherche)
        {
            try
            {
                db.Configuration.ProxyCreationEnabled = false; // Évite une référence circulaire
                List<Hobbie> hobbiesDuType = db.Hobbies.Where(ty => ty.noType == noTypeActiviteRecherche).OrderBy(t => t.nomHobbie).ToList();

                return Json(hobbiesDuType, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Dictionary<string, string> parametres = new Dictionary<string, string>()
                {
                    {"noProvinceRecherchée", noTypeActiviteRecherche.ToString() }
                };
                throw Utilitaires.templateException("TrouverHobbiesAPartirDeNoProvince (AJAX)", "Utilitaire",
                    "Requête LINQ n'a pas fonctionnée.",
                    e, parametres);
            }
        }

        [AllowAnonymous]
        public ActionResult ValiderAncienMDP(string motDePasseAncien, int noMembre)
        {
            try
            {
                db.Configuration.ProxyCreationEnabled = false; // Évite une référence circulaire

                Membre membre = db.Membres.Where(m => m.noMembre == noMembre).FirstOrDefault();

                if (membre != null)
                {
                    //on vérifie si le mdp qu'il a entré est égal au mdp de la BD
                    if (Membre.hashedPwd(motDePasseAncien).Equals(membre.motDePasseHashe))
                    {
                        return Json(true);
                    }
                }
            }
            catch (Exception e)
            {
                //Renvoyer un message d'erreur dans Elmah, mais on fait comme si la connexion n'as simplement pas fonctionner au niveau de l'utilisateur.
                Elmah.ErrorSignal.FromCurrentContext().Raise(
                    new Exception("Action: ValiderAncienMDP, Contrôleur: Utilitaire ; Erreur potentielle: Requête LINQ n'a pas fonctionnée.", e)
                    );
                ModelState.AddModelError(string.Empty, "Une erreur s'est produite lors de l'enregistrement, veuillez réessayer.");
            }
            return Json(false);
        }

        [AllowAnonymous]
        public ActionResult ValiderAncienMDPAdmin(string motDePasseAncien, int noMembre)
        {
            try
            {
                db.Configuration.ProxyCreationEnabled = false; // Évite une référence circulaire

                CompteAdmin admin = db.CompteAdmins.Where(m => m.noCompteAdmin == noMembre).FirstOrDefault();

                if (admin != null)
                {
                    //on vérifie si le mdp qu'il a entré est égal au mdp de la BD
                    if (CompteAdmin.hashedPwd(motDePasseAncien).Equals(admin.motDePasseHashe))
                    {
                        return Json(true);
                    }
                }
            }
            catch (Exception e)
            {
                //Renvoyer un message d'erreur dans Elmah, mais on fait comme si la connexion n'as simplement pas fonctionner au niveau de l'utilisateur.
                Elmah.ErrorSignal.FromCurrentContext().Raise(
                    new Exception("Action: ValiderAncienMDPAdmin, Contrôleur: Utilitaire ; Erreur potentielle: Requête LINQ n'a pas fonctionnée.", e)
                    );
                ModelState.AddModelError(string.Empty, "Une erreur s'est produite lors de l'enregistrement, veuillez réessayer.");
            }
            return Json(false);
        }

        [AllowAnonymous]
        public void AjouterConnexionSite(string visiteur)
        {
            Connexion laConnexion = new Connexion();

            laConnexion.dateConnexion = DateTime.Now;

            int noMembreCo;
            verifierSiCookieNoMembreExiste(out noMembreCo);

            HttpCookie myCookie = new HttpCookie("AnalyticsCC");

            if (visiteur.ToLower() == false.ToString().ToLower())
            {
                Membre leMembreCo = db.Membres.Where(m => m.noMembre == noMembreCo).FirstOrDefault();

                laConnexion.premiumAuMomentDeConnexion = bool.Parse(Request.Cookies["SiteDeRencontre"]["premium"].ToString());
                laConnexion.noMembre = noMembreCo;
                laConnexion.membre = leMembreCo;

                /*bool seSouvenirDeMoi = (Request.Cookies["SiteDeRencontre"]["seSouvenirDeMoi"] == "true" ? true : false);

                myCookie["noMembre"] = leMembreCo.noMembre.ToString();
                myCookie["premium"] = leMembreCo.premium ? "true" : "false";
                myCookie["seSouvenirDeMoi"] = seSouvenirDeMoi ? "true" : "false";

                if (seSouvenirDeMoi)
                {
                    myCookie.Expires = new System.DateTime(2020, 1, 1);
                }*/
            }

            myCookie["dejaEnregistrer"] = true.ToString();

            Response.Cookies.Add(myCookie);

            db.Connexions.Add(laConnexion);

            db.SaveChanges();
        }

        [AllowAnonymous]
        public string TrouverNomFichierPhotoProfil(string userName)
        {
            try
            {
                db.Configuration.ProxyCreationEnabled = false;
                Photo photoProfil = db.Photos.Where(p => p.membre.surnom == userName && p.photoProfil == true).FirstOrDefault();
                bool? membreHomme = db.Membres.Where(p => p.surnom == userName).Select(p => p.homme).FirstOrDefault();


                if (photoProfil != null)
                {
                    return Url.Content("~/Upload/PhotosMembres/") + photoProfil.nomFichierPhoto;
                }
                else
                {
                    //Si le membre est connecté
                    if(membreHomme != null)
                    {
                        if (membreHomme == true)
                        {
                            return Url.Content("~/Photos/ico/profilDefaultGars.jpg");
                        }
                        else
                        {
                            return Url.Content("~/Photos/ico/profilDefaultFille.jpg");
                        }
                    }
                    //Si aucun n'est connecté, on met la photo du gars
                    else
                    {
                        return Url.Content("~/Photos/ico/profilDefaultGars.jpg");
                    }
                }
            }
            catch(Exception e)
            {
                throw new Exception("Action: TrouverNomFichierPhotoProfil, Contrôleur: Utilitaire ; Erreur potentielle: Requête LINQ n'a pas fonctionnée. ; userName recherché: " + userName + ".", e);
            }
        }

        /*public ActionResult EnvoiCourriel(string courriel)
        {
            return;
        }*/

        #region tests

        public ActionResult DeclencheErreur()
        {
            throw new Exception("Erreur test");
            return RedirectToAction("Home", "Home");
        }

        public ActionResult DeclencheErreurDocumentee()
        {
            int i = 0;
            try
            {
                int j = 16;
                int k = j / i;
                return RedirectToAction("Create");
            }
            catch (Exception exc)
            {
                Exception exdocumenté = new Exception("Une erreur volontaire s'est produite dans DéclencheErreurDocumentée, " +
                                                      "contrôleur Utilitaires, action DeclencheErreurDocumentée, contexte: je voulais que cela plante...",
                                                         exc);
                throw exdocumenté;
            };
            return RedirectToAction("Home", "Home");
        }

        public ActionResult DeclencheErreurSansArreter()
        {
            try
            {
                throw new Exception("Allo");
                return RedirectToAction("Home", "Home");
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return RedirectToAction("Home", "Home");
            }
        }

        public ActionResult testPaypal()
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
            ServicePointManager.DefaultConnectionLimit = 9999;
            String retour = LesUtilitaires.Utilitaires.doTransaction("Axel:1234", "4032038530238108", "Visa", "032020", "123", "1.01", "Test", "User", "235, rue Saint-Jacques", "Granby", "QC",
                "J2G9H7");
            return Content(retour);
        }
        #endregion

        [Authorize(Roles = "Admin")]
        public ActionResult LatLngVilles()
        {
            List<Ville> lesVilles = db.Villes.ToList();

            bool correct = false;

            while(!correct)
            {
                correct = true;
                foreach (Ville v in lesVilles)
                {
                    if (v.lat == 0 || v.lat == null || v.lng == 0 || v.lng == null)
                    {
                        decimal lat;
                        decimal lng;
                        LesUtilitaires.Utilitaires.trouverLatLng(v.nomVille, v.province.nomProvince, out lat, out lng);
                        v.lat = lat;
                        v.lng = lng;
                        System.Threading.Thread.Sleep(1000);

                        if (lat == 0 || lng == 0)
                        {
                            correct = false;
                        }
                    }
                }
                db.SaveChanges();
            }

            ViewBag.reussi = true;
            return View("Initialise");
        }

        [Authorize(Roles = "Admin")]
        public ActionResult InitialiseApresResetBD()
        {
            Initialise(1);
            Initialise(300);

            LatLngVilles();

            ListeEmoticons();

            return View("Initialise");
        }

        #region TEST
        public void TESTAjouterAvisActivite()
        {
            Activite activite1 = db.Activites.Where(m => m.noActivite == 1).FirstOrDefault();

            AvisActivite activiteCote = new AvisActivite();

            activiteCote.activiteAssocie = activite1;
            activiteCote.commentaire = "Ton activité était vraiment le fun! J'ai eu beaucoup de plaisir!";
            activiteCote.cote = 4;
            activiteCote.membreEnvoyeur = db.Membres.Where(m => m.noMembre == 6).FirstOrDefault();

            AvisActivite activiteCote2 = new AvisActivite();

            activiteCote2.activiteAssocie = activite1;
            activiteCote2.commentaire = "J'ai eu beaucoup de plaisir! Ton activité était vraiment le fun!";
            activiteCote2.cote = 2;
            activiteCote2.membreEnvoyeur = db.Membres.Where(m => m.noMembre == 8).FirstOrDefault();

            db.AvisActivites.Add(activiteCote);
            db.AvisActivites.Add(activiteCote2);

            db.SaveChanges();
        }

        public void TESTAjouterMembreAActivite()
        {
            Activite activite1 = db.Activites.Where(m => m.noActivite == 1).FirstOrDefault();

            List<Membre> sixMembresRandom = db.Membres
                .Where(m => m.noMembre == 6 ||
                m.noMembre == 7 ||
                m.noMembre == 8 ||
                m.noMembre == 9 ||
                m.noMembre == 10 ||
                m.noMembre == 11).ToList();

            activite1.membresParticipants.AddRange(sixMembresRandom);

            db.SaveChanges();
        }

        public void TESTAjouterMembreCoAActivite()
        {
            int noMembreCo = int.Parse(Request.Cookies["SiteDeRencontre"]["noMembre"]);

            Membre membre = db.Membres.Where(m => m.noMembre == noMembreCo).FirstOrDefault();

            Activite activite1 = db.Activites.Where(m => m.noActivite == 1).FirstOrDefault();

            activite1.membresParticipants.Add(membre);

            db.SaveChanges();
        }
        #endregion

        [Authorize(Roles = "Admin")]
        public ActionResult AjusterDateInscriptionMembresGeneres()
        {
            List<Membre> lesMembres = db.Membres.ToList();

            for(int i = 0; i < lesMembres.Count; i++)
            {
                lesMembres[i].dateInscription = lesMembres[i].dateInscription.Value.AddSeconds(-i);
            }

            try
            {
                db.SaveChanges();

                ViewBag.reussi = true;

                return View("Initialise");
            }
            catch (Exception e)
            {
                ViewBag.reussi = false;
                ViewBag.messageErreur = e.Message;

                return View("Initialise");
            }
        }

        [Authorize(Roles = "Admin")]
        public ActionResult ListeEmoticons()
        {
            int nbEmoticons = 132;

            db.Database.ExecuteSqlCommand("Delete from Emoticons");
            db.Database.ExecuteSqlCommand("DBCC CHECKIDENT('Emoticons', RESEED, 0)");

            List<Emoticon> listeEmoticon = new List<Emoticon>();

            for(int i = 1; i <= nbEmoticons; i++)
            {
                listeEmoticon.Add(new Emoticon { nomFichierEmoticon = i.ToString("00000") + ".png", premiumOnly = (i > 33) });
            }

            try
            {
                db.Emoticons.AddRange(listeEmoticon);

                db.SaveChanges();

                ViewBag.reussi = true;

                return View("Initialise");
            }
            catch(Exception e)
            {
                ViewBag.reussi = false;
                ViewBag.messageErreur = e.Message;

                return View("Initialise");
            }
        }

        [Authorize(Roles = "Admin")]
        public ActionResult GenererConnexions()
        {
            try
            {
                List<Membre> membres = db.Membres.ToList();

                foreach (Membre m in membres)
                {
                    int nombreConnexionAGenerer = Faker.RandomNumber.Next(1, 5);

                    for(int i = 0; i < nombreConnexionAGenerer; i++)
                    {
                        Connexion connexionDuMembre = new Connexion();

                        connexionDuMembre.dateConnexion = DateTime.Now.AddDays(-Faker.RandomNumber.Next(0, 100));
                        connexionDuMembre.membre = m;
                        connexionDuMembre.noMembre = m.noMembre;
                        connexionDuMembre.premiumAuMomentDeConnexion = m.premium;

                        db.Connexions.Add(connexionDuMembre);
                    }                    
                }

                int nombreConnexionAGenererVisiteur = Faker.RandomNumber.Next(1, 100);

                for (int i = 0; i < nombreConnexionAGenererVisiteur; i++)
                {
                    Connexion connexionDuMembre = new Connexion();

                    connexionDuMembre.dateConnexion = DateTime.Now.AddDays(-Faker.RandomNumber.Next(0, 100));
                    connexionDuMembre.membre = null;
                    connexionDuMembre.noMembre = null;
                    connexionDuMembre.premiumAuMomentDeConnexion = null;

                    db.Connexions.Add(connexionDuMembre);
                }

                db.SaveChanges();

                ViewBag.reussi = true;

                return View("Initialise");
            }
            catch (Exception e)
            {
                ViewBag.reussi = false;
                ViewBag.messageErreur = e.Message;

                return View("Initialise");
            }
        }

        [Authorize(Roles = "Admin")]
        public ActionResult GenererAbonnements()
        {
            try
            {
                List<Membre> membres = db.Membres.Where(m => m.premium == true).ToList();

                foreach (Membre m in membres)
                {
                    if (m.premium)
                    {
                        List<int> nbMoisPossibles = new List<int> { 1, 6, 12 };
                        int nbMois = Pick<int>.RandomItemFrom(nbMoisPossibles);

                        DateTime now = DateTime.Now;

                        Abonnement lAbonnementDuMembre = new Abonnement();
                        lAbonnementDuMembre.dateDebut = now;
                        lAbonnementDuMembre.dateFin = DateTime.Now.AddMonths(nbMois);
                        lAbonnementDuMembre.coutTotal = (nbMois == 1 ? 9.99 : (nbMois == 6 ? 47.99 : 71.99));
                        lAbonnementDuMembre.membre = m;
                        lAbonnementDuMembre.noMembre = m.noMembre;
                        lAbonnementDuMembre.datePaiement = now;

                        db.Abonnements.Add(lAbonnementDuMembre);
                    }
                }
                db.SaveChanges();

                ViewBag.reussi = true;

                return View("Initialise");
            }
            catch (Exception e)
            {
                ViewBag.reussi = false;
                ViewBag.messageErreur = e.Message;

                return View("Initialise");
            }
        }

        [Authorize(Roles = "Admin")]
        //DateActivitePlusGrandQuAujourdhui(ErrorMessage = "Veuillez choisir une date dans le futur.")] //A enlever lors d'un initialise dans fichier Activite.CS
        public ActionResult Initialise(int? id)
        {
            try
            {
                if(id == null)
                {
                    //par défaut, 300 membres
                    id = 300;
                }

                int nbMembresEtActivitesBidons = (int)id;
                Boolean[] booleann = { true, false };
                var numberGenerator = new RandomGenerator();

                #region ViderTables
                //Vider les tables
                db.Database.ExecuteSqlCommand("Delete from TraitementSignalements");
                db.Database.ExecuteSqlCommand("Delete from CommentaireSignalements");
                db.Database.ExecuteSqlCommand("Delete from ActionTraitements");

                db.Database.ExecuteSqlCommand("Delete from Signalements");
                db.Database.ExecuteSqlCommand("Delete from EtatSignalements");

                db.Database.ExecuteSqlCommand("Delete from ForfaitPremiums");
                db.Database.ExecuteSqlCommand("Delete from Abonnements");
                db.Database.ExecuteSqlCommand("Delete from AvisActivites");
                db.Database.ExecuteSqlCommand("Delete from ActiviteMembres");
                db.Database.ExecuteSqlCommand("Delete from PhotosActivites");
                db.Database.ExecuteSqlCommand("Delete from Activites");
                db.Database.ExecuteSqlCommand("Delete from BloqueMembre");
                db.Database.ExecuteSqlCommand("Delete from ContactMembre");
                db.Database.ExecuteSqlCommand("Delete from Visites");
                db.Database.ExecuteSqlCommand("Delete from Connexions");
                db.Database.ExecuteSqlCommand("Delete from FavorisAvec");
                db.Database.ExecuteSqlCommand("Delete from HobbieMembres");
                db.Database.ExecuteSqlCommand("Delete from Hobbies");
                db.Database.ExecuteSqlCommand("Delete from RaisonsSurSiteMembres");
                db.Database.ExecuteSqlCommand("Delete from Messages");
                db.Database.ExecuteSqlCommand("Delete from Gifts");
                db.Database.ExecuteSqlCommand("Delete from Membres");
                db.Database.ExecuteSqlCommand("Delete from Origines");
                db.Database.ExecuteSqlCommand("Delete from Photos");
                db.Database.ExecuteSqlCommand("Delete from Provinces");
                db.Database.ExecuteSqlCommand("Delete from Publications");

                db.Database.ExecuteSqlCommand("Delete from CompteAdmins");
                db.Database.ExecuteSqlCommand("Delete from NiveauDePermissionAdmins");

                db.Database.ExecuteSqlCommand("Delete from RaisonsSurSites");
                db.Database.ExecuteSqlCommand("Delete from Religions");
                db.Database.ExecuteSqlCommand("Delete from Silhouettes");
                db.Database.ExecuteSqlCommand("Delete from Tailles");
                db.Database.ExecuteSqlCommand("Delete from Themes");
                db.Database.ExecuteSqlCommand("Delete from Types");
                db.Database.ExecuteSqlCommand("Delete from Villes");
                
                db.Database.ExecuteSqlCommand("Delete from YeuxCouleurs");
                db.Database.ExecuteSqlCommand("Delete from CheveuxCouleurs");
                db.Database.ExecuteSqlCommand("Delete from Occupations");
                db.Database.ExecuteSqlCommand("Delete from NiveauEtudes");
                db.Database.ExecuteSqlCommand("Delete from SituationFinancieres");
                
                db.Database.ExecuteSqlCommand("Delete from DesirEnfants");

                db.Database.ExecuteSqlCommand("DBCC CHECKIDENT('TraitementSignalements', RESEED, 0)");
                db.Database.ExecuteSqlCommand("DBCC CHECKIDENT('CommentaireSignalements', RESEED, 0)");
                db.Database.ExecuteSqlCommand("DBCC CHECKIDENT('ActionTraitements', RESEED, 0)");

                db.Database.ExecuteSqlCommand("DBCC CHECKIDENT('Gifts', RESEED, 0)");
                db.Database.ExecuteSqlCommand("DBCC CHECKIDENT('ForfaitPremiums', RESEED, 0)");
                db.Database.ExecuteSqlCommand("DBCC CHECKIDENT('CompteAdmins', RESEED, 0)");
                db.Database.ExecuteSqlCommand("DBCC CHECKIDENT('NiveauDePermissionAdmins', RESEED, 0)");
                db.Database.ExecuteSqlCommand("DBCC CHECKIDENT('EtatSignalements', RESEED, 0)");
                db.Database.ExecuteSqlCommand("DBCC CHECKIDENT('Abonnements', RESEED, 0)");
                //db.Database.ExecuteSqlCommand("DBCC CHECKIDENT('ActiviteMembres', RESEED, 0)");
                db.Database.ExecuteSqlCommand("DBCC CHECKIDENT('AvisActivites', RESEED, 0)");
                db.Database.ExecuteSqlCommand("DBCC CHECKIDENT('PhotosActivites', RESEED, 0)");
                db.Database.ExecuteSqlCommand("DBCC CHECKIDENT('Activites', RESEED, 0)");
                db.Database.ExecuteSqlCommand("DBCC CHECKIDENT('CheveuxCouleurs', RESEED, 0)");
                db.Database.ExecuteSqlCommand("DBCC CHECKIDENT('Connexions', RESEED, 0)");
                //db.Database.ExecuteSqlCommand("DBCC CHECKIDENT('FavorisAvec', RESEED, 0)");
                //db.Database.ExecuteSqlCommand("DBCC CHECKIDENT('HobbieMembres', RESEED, 0)");
                db.Database.ExecuteSqlCommand("DBCC CHECKIDENT('Hobbies', RESEED, 0)");
                db.Database.ExecuteSqlCommand("DBCC CHECKIDENT('Membres', RESEED, 0)");
                db.Database.ExecuteSqlCommand("DBCC CHECKIDENT('Messages', RESEED, 0)");
                db.Database.ExecuteSqlCommand("DBCC CHECKIDENT('Origines', RESEED, 0)");
                db.Database.ExecuteSqlCommand("DBCC CHECKIDENT('Photos', RESEED, 0)");
                db.Database.ExecuteSqlCommand("DBCC CHECKIDENT('Provinces', RESEED, 0)");
                db.Database.ExecuteSqlCommand("DBCC CHECKIDENT('Publications', RESEED, 0)");
                //db.Database.ExecuteSqlCommand("DBCC CHECKIDENT('RaisonsSurSiteMembres', RESEED, 0)");
                db.Database.ExecuteSqlCommand("DBCC CHECKIDENT('RaisonsSurSites', RESEED, 0)");
                db.Database.ExecuteSqlCommand("DBCC CHECKIDENT('Religions', RESEED, 0)");
                db.Database.ExecuteSqlCommand("DBCC CHECKIDENT('Silhouettes', RESEED, 0)");
                db.Database.ExecuteSqlCommand("DBCC CHECKIDENT('Tailles', RESEED, 0)");
                db.Database.ExecuteSqlCommand("DBCC CHECKIDENT('Themes', RESEED, 0)");
                db.Database.ExecuteSqlCommand("DBCC CHECKIDENT('Types', RESEED, 0)");
                db.Database.ExecuteSqlCommand("DBCC CHECKIDENT('Villes', RESEED, 0)");
                db.Database.ExecuteSqlCommand("DBCC CHECKIDENT('Visites', RESEED, 0)");
                db.Database.ExecuteSqlCommand("DBCC CHECKIDENT('YeuxCouleurs', RESEED, 0)");
                db.Database.ExecuteSqlCommand("DBCC CHECKIDENT('Occupations', RESEED, 0)");
                db.Database.ExecuteSqlCommand("DBCC CHECKIDENT('NiveauEtudes', RESEED, 0)");
                db.Database.ExecuteSqlCommand("DBCC CHECKIDENT('SituationFinancieres', RESEED, 0)");
                db.Database.ExecuteSqlCommand("DBCC CHECKIDENT('DesirEnfants', RESEED, 0)");

                db.Database.ExecuteSqlCommand("DBCC CHECKIDENT('Signalements', RESEED, 0)");
                #endregion

                #region Données Qui Restent
                #region Villes
                //Liste des villes du Canada
                List<Ville> Alberta = new List<Ville>()
                {
                    new Ville() { nomVille="Banff" },           
                    new Ville() { nomVille="Brooks" },
                    new Ville() { nomVille="Calgary" },
                    new Ville() { nomVille="Edmonton" },
                    new Ville() { nomVille="Fort McMurray" },
                    new Ville() { nomVille="Grande Prairie" },
                    new Ville() { nomVille="Jasper" },
                    new Ville() { nomVille="Lake Louise" },
                    new Ville() { nomVille="Lethbridge" },
                    new Ville() { nomVille="Medicine Hat" },
                    new Ville() { nomVille="Red Deer" },
                    new Ville() { nomVille="Saint Albert" }
                };

                List<Ville> Colombie_Brit = new List<Ville>()
                {
                    new Ville() { nomVille="Barkerville" },
                    new Ville() { nomVille="Burnaby" },
                    new Ville() { nomVille="Campbell River" },
                    new Ville() { nomVille="Chilliwack" },
                    new Ville() { nomVille="Courtenay" },
                    new Ville() { nomVille="Cranbrook" },
                    new Ville() { nomVille="Dawson Creek" },
                    new Ville() { nomVille="Delta" },
                    new Ville() { nomVille="Esquimalt" },
                    new Ville() { nomVille="Fort Saint James" },
                    new Ville() { nomVille="Fort Saint John" },
                    new Ville() { nomVille="Hope" },
                    new Ville() { nomVille="Kamloops" },
                    new Ville() { nomVille="Kelowna" },
                    new Ville() { nomVille="Kimberley" },
                    new Ville() { nomVille="Kitimat" },
                    new Ville() { nomVille="Langley" },
                    new Ville() { nomVille="Nanaimo" },
                    new Ville() { nomVille="Nelson" },
                    new Ville() { nomVille="New Westminster" },
                    new Ville() { nomVille="North Vancouver" },
                    new Ville() { nomVille="Oak Bay" },
                    new Ville() { nomVille="Penticton" },
                    new Ville() { nomVille="Powell River" },
                    new Ville() { nomVille="Prince George" },
                    new Ville() { nomVille="Prince Rupert" },
                    new Ville() { nomVille="Quesnel" },
                    new Ville() { nomVille="Revelstoke" },
                    new Ville() { nomVille="Rossland" },
                    new Ville() { nomVille="Trail" },
                    new Ville() { nomVille="Vancouver" },
                    new Ville() { nomVille="Vernon" },
                    new Ville() { nomVille="Victoria" },
                    new Ville() { nomVille="West Vancouver" },
                    new Ville() { nomVille="White Rock" }
                };

                List<Ville> Manitoba = new List<Ville>()
                {
                    new Ville() { nomVille="Brandon" },   
                    new Ville() { nomVille="Churchill" },
                    new Ville() { nomVille="Dauphin" },
                    new Ville() { nomVille="Flin Flon" },
                    new Ville() { nomVille="Kildonan" },
                    new Ville() { nomVille="Saint Boniface" },
                    new Ville() { nomVille="Swan River" },
                    new Ville() { nomVille="Thompson" },
                    new Ville() { nomVille="Winnipeg" },
                    new Ville() { nomVille="York Factory" }
                };

                List<Ville> Nouveau_Brunswick = new List<Ville>()
                {
                    new Ville() { nomVille="Bathurst" }, 
                    new Ville() { nomVille="Caraquet" },
                    new Ville() { nomVille="Dalhousie" },
                    new Ville() { nomVille="Fredericton" },
                    new Ville() { nomVille="Miramichi" },
                    new Ville() { nomVille="Moncton" },
                    new Ville() { nomVille="Saint John" }
                };

                List<Ville> Terre_Neuve_Labrador = new List<Ville>()
                {
                    new Ville() { nomVille="Argentia" },
                    new Ville() { nomVille="Bonavista" },
                    new Ville() { nomVille="Channel-Port aux Basques" },
                    new Ville() { nomVille="Corner Brook" },
                    new Ville() { nomVille="Ferryland" },
                    new Ville() { nomVille="Gander" },
                    new Ville() { nomVille="Grand Falls–Windsor" },
                    new Ville() { nomVille="Happy Valley–Goose Bay" },
                    new Ville() { nomVille="Harbour Grace" },
                    new Ville() { nomVille="Labrador City" },
                    new Ville() { nomVille="Placentia" },
                    new Ville() { nomVille="Saint Anthony" },
                    new Ville() { nomVille="St. John’s" },
                    new Ville() { nomVille="Wabana" }
                };

                List<Ville> Territoire_Nord_Ouest = new List<Ville>()
                {
                    new Ville() { nomVille="Fort Smith" },
                    new Ville() { nomVille="Hay River" },
                    new Ville() { nomVille="Inuvik" },
                    new Ville() { nomVille="Tuktoyaktuk" },
                    new Ville() { nomVille="Yellowknife" }    
                };

                List<Ville> Nouvelle_Ecosse = new List<Ville>()
                {
                    new Ville() { nomVille="Baddeck" },
                    new Ville() { nomVille="Digby" },
                    new Ville() { nomVille="Glace Bay" },
                    new Ville() { nomVille="Halifax" },
                    new Ville() { nomVille="Liverpool" },
                    new Ville() { nomVille="Louisbourg" },
                    new Ville() { nomVille="Lunenburg" },
                    new Ville() { nomVille="Pictou" },
                    new Ville() { nomVille="Port Hawkesbury" },
                    new Ville() { nomVille="Springhill" },
                    new Ville() { nomVille="Sydney" },
                    new Ville() { nomVille="Yarmouth" }       
                };

                List<Ville> Nunavut = new List<Ville>()
                {
                    new Ville() { nomVille="Iqaluit" }
                };

                List<Ville> Ontario = new List<Ville>()
                {

                    new Ville() { nomVille="Bancroft" },
                    new Ville() { nomVille="Barrie" },
                    new Ville() { nomVille="Belleville" },
                    new Ville() { nomVille="Brampton" },
                    new Ville() { nomVille="Brantford" },
                    new Ville() { nomVille="Brockville" },
                    new Ville() { nomVille="Burlington" },
                    new Ville() { nomVille="Cambridge" },
                    new Ville() { nomVille="Chatham" },
                    new Ville() { nomVille="Chatham-Kent" },
                    new Ville() { nomVille="Cornwall" },
                    new Ville() { nomVille="Elliot Lake" },
                    new Ville() { nomVille="Etobicoke" },
                    new Ville() { nomVille="Fort Erie" },
                    new Ville() { nomVille="Fort Frances" },
                    new Ville() { nomVille="Gananoque" },
                    new Ville() { nomVille="Guelph" },
                    new Ville() { nomVille="Hamilton" },
                    new Ville() { nomVille="Iroquois Falls" },
                    new Ville() { nomVille="Kapuskasing" },
                    new Ville() { nomVille="Kawartha Lakes" },
                    new Ville() { nomVille="Kenora" },
                    new Ville() { nomVille="Kingston" },
                    new Ville() { nomVille="Kirkland Lake" },
                    new Ville() { nomVille="Kitchener" },
                    new Ville() { nomVille="Laurentian Hills" },
                    new Ville() { nomVille="London" },
                    new Ville() { nomVille="Midland" },
                    new Ville() { nomVille="Mississauga" },
                    new Ville() { nomVille="Moose Factory" },
                    new Ville() { nomVille="Moosonee" },
                    new Ville() { nomVille="Niagara Falls" },
                    new Ville() { nomVille="Niagara-on-the-Lake" },
                    new Ville() { nomVille="North Bay" },
                    new Ville() { nomVille="North York" },
                    new Ville() { nomVille="Oakville" },
                    new Ville() { nomVille="Orillia" },
                    new Ville() { nomVille="Oshawa" },
                    new Ville() { nomVille="Ottawa" },
                    new Ville() { nomVille="Parry Sound" },
                    new Ville() { nomVille="Perth" },
                    new Ville() { nomVille="Peterborough" },
                    new Ville() { nomVille="Picton" },
                    new Ville() { nomVille="Port Colborne" },
                    new Ville() { nomVille="Saint Catharines" },
                    new Ville() { nomVille="Saint Thomas" },
                    new Ville() { nomVille="Sarnia-Clearwater" },
                    new Ville() { nomVille="Sault Sainte Marie" },
                    new Ville() { nomVille="Scarborough" },
                    new Ville() { nomVille="Simcoe" },
                    new Ville() { nomVille="Stratford" },
                    new Ville() { nomVille="Sudbury" },
                    new Ville() { nomVille="Temiskaming Shores" },
                    new Ville() { nomVille="Thorold" },
                    new Ville() { nomVille="Thunder Bay" },
                    new Ville() { nomVille="Timmins" },
                    new Ville() { nomVille="Toronto" },
                    new Ville() { nomVille="Trenton" },
                    new Ville() { nomVille="Waterloo" },
                    new Ville() { nomVille="Welland" },
                    new Ville() { nomVille="West Nipissing" },
                    new Ville() { nomVille="Windsor" },
                    new Ville() { nomVille="Woodstock" },
                    new Ville() { nomVille="York" }        
                    };

                List<Ville> Ile_Prince_Edouard = new List<Ville>()
                {
                    new Ville() { nomVille="Borden" }, 
                    new Ville() { nomVille="Cavendish" },
                    new Ville() { nomVille="Charlottetown" },
                    new Ville() { nomVille="Souris" },
                    new Ville() { nomVille="Summerside" }        
                };

                List<Ville> Quebec = new List<Ville>()
                {
                    new Ville() { nomVille="Asbestos" },
                    new Ville() { nomVille="Baie-Comeau" },
                    new Ville() { nomVille="Beloeil" },
                    new Ville() { nomVille="Cap-de-la-Madeleine" },
                    new Ville() { nomVille="Chambly" },
                    new Ville() { nomVille="Charlesbourg" },
                    new Ville() { nomVille="Châteauguay" },
                    new Ville() { nomVille="Chibougamau" },
                    new Ville() { nomVille="Côte-Saint-Luc" },
                    new Ville() { nomVille="Dorval" },
                    new Ville() { nomVille="Gaspé" },
                    new Ville() { nomVille="Gatineau" },
                    new Ville() { nomVille="Granby" },
                    new Ville() { nomVille="Havre-Saint-Pierre" },
                    new Ville() { nomVille="Hull" },
                    new Ville() { nomVille="Jonquière" },
                    new Ville() { nomVille="Kuujjuaq" },
                    new Ville() { nomVille="La Salle" },
                    new Ville() { nomVille="La Tuque" },
                    new Ville() { nomVille="Lachine" },
                    new Ville() { nomVille="Laval" },
                    new Ville() { nomVille="Lévis" },
                    new Ville() { nomVille="Longueuil" },
                    new Ville() { nomVille="Magog" },
                    new Ville() { nomVille="Matane" },
                    new Ville() { nomVille="Montréal" },
                    new Ville() { nomVille="Montréal-Nord" },
                    new Ville() { nomVille="Percé" },
                    new Ville() { nomVille="Port-Cartier" },
                    new Ville() { nomVille="Quebec" },
                    new Ville() { nomVille="Rimouski" },
                    new Ville() { nomVille="Rouyn-Noranda" },
                    new Ville() { nomVille="Saguenay" },
                    new Ville() { nomVille="Saint-Eustache" },
                    new Ville() { nomVille="Saint-Hubert" },
                    new Ville() { nomVille="Sainte-Anne-de-Beaupré" },
                    new Ville() { nomVille="Sainte-Foy" },
                    new Ville() { nomVille="Sainte-Thérèse" },
                    new Ville() { nomVille="Sept-Îles" },
                    new Ville() { nomVille="Sherbrooke" },
                    new Ville() { nomVille="Sorel-Tracy" },
                    new Ville() { nomVille="Trois-Rivières" },
                    new Ville() { nomVille="Val-d’Or" },
                    new Ville() { nomVille="Waskaganish" }    
                };

                List<Ville> Saskatchewan = new List<Ville>()
                {
                    new Ville() { nomVille="Batoche" },
                    new Ville() { nomVille="Cumberland House" },
                    new Ville() { nomVille="Estevan" },
                    new Ville() { nomVille="Flin Flon" },
                    new Ville() { nomVille="Moose Jaw" },
                    new Ville() { nomVille="Prince Albert" },
                    new Ville() { nomVille="Regina" },
                    new Ville() { nomVille="Saskatoon" },
                    new Ville() { nomVille="Uranium City" } 
                };

                List<Ville> Yukon = new List<Ville>()
                {
                    new Ville() { nomVille="Dawson" },
                    new Ville() { nomVille="Watson Lake" },
                    new Ville() { nomVille="Whitehorse" }
                };

                //Ajout de toutes les listes de villes à la BD
                db.Villes.AddRange(Alberta);
                db.Villes.AddRange(Colombie_Brit);
                db.Villes.AddRange(Manitoba);
                db.Villes.AddRange(Nouveau_Brunswick);
                db.Villes.AddRange(Terre_Neuve_Labrador);
                db.Villes.AddRange(Territoire_Nord_Ouest);
                db.Villes.AddRange(Nouvelle_Ecosse);
                db.Villes.AddRange(Nunavut);
                db.Villes.AddRange(Ontario);
                db.Villes.AddRange(Ile_Prince_Edouard);
                db.Villes.AddRange(Quebec);
                db.Villes.AddRange(Saskatchewan);
                db.Villes.AddRange(Yukon);

                #endregion

                #region Provinces
                //Liste des provinces du Canada
                List<Province> Provinces = new List<Province>()
                {
                    new Province() { nomProvince="Alberta", listeVilles = Alberta, abbrProvince = "AB"},
                    new Province() { nomProvince="Colombie-Britannique", listeVilles = Colombie_Brit, abbrProvince = "BC"},
                    new Province() { nomProvince="Île-du-Prince-Édouard", listeVilles = Ile_Prince_Edouard, abbrProvince = "IP"},
                    new Province() { nomProvince="Manitoba", listeVilles = Manitoba, abbrProvince = "MB"},
                    new Province() { nomProvince="Nouveau-Brunswick", listeVilles = Nouveau_Brunswick, abbrProvince = "NB"},
                    new Province() { nomProvince="Nouvelle-Écosse", listeVilles = Nouvelle_Ecosse, abbrProvince = "NS"},
                    new Province() { nomProvince="Ontario", listeVilles = Ontario, abbrProvince = "ON"},
                    new Province() { nomProvince="Québec", listeVilles = Quebec, abbrProvince = "QC"},
                    new Province() { nomProvince="Saskatchewan", listeVilles = Saskatchewan, abbrProvince = "SK"},
                    new Province() { nomProvince="Terre-Neuve-et-Labrador", listeVilles = Terre_Neuve_Labrador, abbrProvince = "NL"},
                    new Province() { nomProvince="Nunavut", listeVilles = Nunavut, abbrProvince = "NU"},
                    new Province() { nomProvince="Territoires du Nord-Ouest", listeVilles = Territoire_Nord_Ouest, abbrProvince = "NT"},
                    new Province() { nomProvince="Yukon", listeVilles = Yukon, abbrProvince = "YT"}
                };
                //Ajoute les provinces à la BD
                db.Provinces.AddRange(Provinces);

                #endregion 

                #region Silhouettes
                //Liste des silhouettes possibles
                List<Silhouette> silhouettes = new List<Silhouette>()
                {
                    new Silhouette() { nomSilhouette="Mince" },
                    new Silhouette() { nomSilhouette="Proportionnelle" },
                    new Silhouette() { nomSilhouette="Athlétique" },
                    new Silhouette() { nomSilhouette="Musclée" },
                    new Silhouette() { nomSilhouette="Enrobée" },
                    new Silhouette() { nomSilhouette="Taille forte" }
                };
                //Ajoute les silhouettes à la BD
                db.Silhouettes.AddRange(silhouettes);
                #endregion

                #region Tailles
                //Liste des tailles possibles
                List<Taille> tailles = new List<Taille>()
                {
                    new Taille() { taille="Petit" },
                    new Taille() { taille="Taille moyenne" },
                    new Taille() { taille="Grand" },
                    new Taille() { taille="Très grand" }
                };
                //Ajoute les tailles à la BD
                db.Tailles.AddRange(tailles);
                #endregion

                #region Religions
                //Liste des religions possibles
                List<Religion> religions = new List<Religion>()
                {
                    new Religion() { religion="Christianisme" },
                    new Religion() { religion="Islam" },
                    new Religion() { religion="Hindouisme" },
                    new Religion() { religion="Bouddhisme" },
                    new Religion() { religion="Judaïsme" },
                    new Religion() { religion="Spiritisme" },
                    new Religion() { religion="Aucune" }
                };
                //Ajoute les religions à la BD
                db.Religions.AddRange(religions);
                #endregion

                #region YeuxCouleur
                //Liste des couleurs de yeux possibles
                List<YeuxCouleur> yeux = new List<YeuxCouleur>()
                {
                    new YeuxCouleur() { nomYeuxCouleur="Bleu" },
                    new YeuxCouleur() { nomYeuxCouleur="Vert" },
                    new YeuxCouleur() { nomYeuxCouleur="Pers" },
                    new YeuxCouleur() { nomYeuxCouleur="Brun" }
                };
                //Ajoute les couleurs de yeux possibles à la BD
                db.YeuxCouleurs.AddRange(yeux);
                #endregion

                #region CheveuxCouleur
                //Liste des couleurs de cheveux possibles
                List<CheveuxCouleur> cheveux = new List<CheveuxCouleur>()
                {
                    new CheveuxCouleur() { nomCheveuxCouleur="Brun" },
                    new CheveuxCouleur() { nomCheveuxCouleur="Blond" },
                    new CheveuxCouleur() { nomCheveuxCouleur="Roux" },
                    new CheveuxCouleur() { nomCheveuxCouleur="Noir" },
                    new CheveuxCouleur() { nomCheveuxCouleur="Autres" }
                };
                //Ajoute les couleurs de cheveux possibles à la BD
                db.CheveuxCouleurs.AddRange(cheveux);
                #endregion

                #region Origines
                //Liste des origines possibles
                List<Origine> origines = new List<Origine>()
                {
                    new Origine() { origine="Européenne" },
                    new Origine() { origine="Asiatique" },
                    new Origine() { origine="Africaine" },
                    new Origine() { origine="Latine" },
                    new Origine() { origine="Autochtone" },
                    new Origine() { origine="Américaine" }
                };
                //Ajoute les origines possibles à la BD
                db.Origines.AddRange(origines);
                #endregion

                #region Hobbies
                //Liste des hobbies possibles
                List<Hobbie> sports = new List<Hobbie>()
                {
                    new Hobbie() { nomHobbie="Hockey" },
                    new Hobbie() { nomHobbie="Basketball" },
                    new Hobbie() { nomHobbie="Soccer" },
                    new Hobbie() { nomHobbie="Vélo" },
                    new Hobbie() { nomHobbie="Football" },
                    new Hobbie() { nomHobbie="Course" },
                    new Hobbie() { nomHobbie="Golf" },
                    new Hobbie() { nomHobbie="VolleyBall" },
                    new Hobbie() { nomHobbie="Natation" },
                    new Hobbie() { nomHobbie="Baseball" },
                    new Hobbie() { nomHobbie="Yoga" },
                    new Hobbie() { nomHobbie="Karaté" },
                    new Hobbie() { nomHobbie="Fitness" },
                };
                List<Hobbie> social = new List<Hobbie>()
                {
                    new Hobbie() { nomHobbie="Restaurant" },
                    new Hobbie() { nomHobbie="Vin et fromage" },
                    new Hobbie() { nomHobbie="Jeux de société" },
                    new Hobbie() { nomHobbie="Bénévolat" },
                    new Hobbie() { nomHobbie="Party" },
                    new Hobbie() { nomHobbie="Cinéma" },
                };

                List<Hobbie> arts = new List<Hobbie>() {
                    new Hobbie() { nomHobbie="Danse" },
                    new Hobbie() { nomHobbie="Arts dramatique" },
                    new Hobbie() { nomHobbie="Instruments de musique" },
                    new Hobbie() { nomHobbie="Écriture" },
                    new Hobbie() { nomHobbie="Photographie" },
                    new Hobbie() { nomHobbie="Arts plastiques" },
                    };

                List<Hobbie> culture = new List<Hobbie>() {
                    new Hobbie() { nomHobbie="Concerts" },
                    new Hobbie() { nomHobbie="Musées" },
                    new Hobbie() { nomHobbie="Spectacle d'humour" },
                    new Hobbie() { nomHobbie="Théâtre" },
                    new Hobbie() { nomHobbie="Voyage" },
                    };

                List<Hobbie> musique = new List<Hobbie>() {
                    new Hobbie() { nomHobbie="Classique" },
                    new Hobbie() { nomHobbie="Populaire" },
                    new Hobbie() { nomHobbie="Métal" },
                    new Hobbie() { nomHobbie="Jazz" },
                    new Hobbie() { nomHobbie="Heavy Métal" },
                    new Hobbie() { nomHobbie="Country" },
                    new Hobbie() { nomHobbie="Électronique" },
                    new Hobbie() { nomHobbie="Alternatif" },
                    new Hobbie() { nomHobbie="Rock" },
                    new Hobbie() { nomHobbie="Opéra" },
                    new Hobbie() { nomHobbie="Punk" },
                    new Hobbie() { nomHobbie="New Age" },
                    };

                List<Hobbie> passeTemps = new List<Hobbie>()
                {
                    new Hobbie() { nomHobbie="Jardinage" },
                    new Hobbie() { nomHobbie="Philatélie" },
                    new Hobbie() { nomHobbie="Cuisine" },
                    new Hobbie() { nomHobbie="Série télé" },
                    new Hobbie() { nomHobbie="Décoration" },
                    new Hobbie() { nomHobbie="Lecture" },
                    new Hobbie() { nomHobbie="Surfer sur internet" }
                };
                //Ajoute les hobbies possibles à la BD
                db.Hobbies.AddRange(sports);
                db.Hobbies.AddRange(social);
                db.Hobbies.AddRange(arts);
                db.Hobbies.AddRange(culture);
                db.Hobbies.AddRange(musique);
                db.Hobbies.AddRange(passeTemps);
                #endregion

                #region TypesHobbies
                //Liste des types de hobbies possibles
                List<Types> types = new List<Types>()
                {
                    new Types() { nomType="Sport", listeHobbies=sports },
                    new Types() { nomType="Social" , listeHobbies=social},
                    new Types() { nomType="Art", listeHobbies=arts },
                    new Types() { nomType="Culture", listeHobbies=culture },
                    new Types() { nomType="Musique", listeHobbies=musique },
                    new Types() { nomType="Passe-temps", listeHobbies=passeTemps },
                };
                //Ajoute les types de hobbies possible à la BD
                db.Types.AddRange(types);
                #endregion

                #region RaisonsSurSite
                //Liste des raisons d'être sur le site possibles
                List<RaisonsSurSite> raisons = new List<RaisonsSurSite>()
                {
                    new RaisonsSurSite() { raison="Amour" },
                    new RaisonsSurSite() { raison="Amitié" },
                    new RaisonsSurSite() { raison="Activité" },
                    new RaisonsSurSite() { raison="Partager" },
                    new RaisonsSurSite() { raison="Sans but précis" }
                };
                //Ajoute les raisons d'être sur le site possibles à la BD
                db.RaisonsSurSites.AddRange(raisons);
                #endregion

                #region Themes
                //Liste des thèmes possibles pour les activités
                List<Theme> themes = new List<Theme>()
                {
                    new Theme() { theme="Sport" },
                    new Theme() { theme="Social" },
                    new Theme() { theme="Art" },
                    new Theme() { theme="Culture" }
                };
                //Ajoute les thèmes possibles pour les activités à la BD
                db.Themes.AddRange(themes);
                #endregion

                #region Occupation
                //Liste des occupations possibles pour les membres
                List<Occupation> occupations = new List<Occupation>()
                {
                    new Occupation() { nomOccupation="Entrepreneur" },
                    new Occupation() { nomOccupation="Sans emploi" },
                    new Occupation() { nomOccupation="Travailleur autonome" },
                    new Occupation() { nomOccupation="Travailleur" },
                    new Occupation() { nomOccupation="Retraité" },
                    new Occupation() { nomOccupation="Étudiant" }
                };
                //Ajoute les occupations possibles pour les membres à la BD
                db.Occupations.AddRange(occupations);
                #endregion

                #region SituationFinanciere
                //Liste des situationFinancieres possibles pour les membres
                List<SituationFinanciere> situationFinancieres = new List<SituationFinanciere>()
                {
                    new SituationFinanciere() { nomSituationFinanciere="Classe moyenne" },
                    new SituationFinanciere() { nomSituationFinanciere="À l'aise" },
                    new SituationFinanciere() { nomSituationFinanciere="Très à l'aise" }
                };
                //Ajoute les situationFinancieres possibles pour les membres à la BD
                db.SituationsFinancieres.AddRange(situationFinancieres);
                #endregion

                #region NiveauEtude
                //Liste des niveaux d'études possibles pour les membres
                List<NiveauEtude> niveauEtudes = new List<NiveauEtude>()
                {
                    new NiveauEtude() { nomNiveauEtude="Autodidacte" },
                    new NiveauEtude() { nomNiveauEtude="Secondaire" },
                    new NiveauEtude() { nomNiveauEtude="Collégiale" },
                    new NiveauEtude() { nomNiveauEtude="Universitaire" }
                };
                //Ajoute les niveaux d'études possibles pour les membres à la BD
                db.NiveauEtudes.AddRange(niveauEtudes);
                #endregion

                #region DesirEnfant
                //Liste des niveaux d'études possibles pour les membres
                List<DesirEnfant> desirEnfants = new List<DesirEnfant>()
                {
                    new DesirEnfant() { desirEnfant="Oui" },
                    new DesirEnfant() { desirEnfant="Non" },
                    new DesirEnfant() { desirEnfant="Peut-être" }
                };
                //Ajoute les niveaux d'études possibles pour les membres à la BD
                db.DesirEnfants.AddRange(desirEnfants);
                #endregion

                #region NiveauPermissions
                //Liste des niveaux de permission possibles pour les admins
                string nomNiveauPermissionComplet = "Complet";
                List<NiveauDePermissionAdmin> niveauDePermissionAdmins = new List<NiveauDePermissionAdmin>()
                {
                    new NiveauDePermissionAdmin() { nomNiveauDePermissionAdmin="Limité" },
                    new NiveauDePermissionAdmin() { nomNiveauDePermissionAdmin=nomNiveauPermissionComplet }
                };
                db.NiveauDePermissionAdmins.AddRange(niveauDePermissionAdmins);
                #endregion

                #region EtatSignalement
                //Liste des états de signalement possibles pour les admins
                List<EtatSignalement> etatSignalements = new List<EtatSignalement>()
                {
                    new EtatSignalement() { nomEtatSignalement="Nouveau" },
                    new EtatSignalement() { nomEtatSignalement="Assigné" },
                    new EtatSignalement() { nomEtatSignalement="En suivi" },
                    new EtatSignalement() { nomEtatSignalement="À traiter" },
                    new EtatSignalement() { nomEtatSignalement="Traité" }
                };
                db.EtatSignalements.AddRange(etatSignalements);
                #endregion

                ListeEmoticons();

                #region PremierCompteAdmin
                //Liste des états de signalement possibles pour les admins
                CompteAdmin compteAdminPrincipal = new CompteAdmin
                {
                    nomCompte = "adminPrincipal",
                    motDePasse = "abcde",
                    permission = niveauDePermissionAdmins.Where(n => n.nomNiveauDePermissionAdmin == nomNiveauPermissionComplet).FirstOrDefault(),
                    noPermission = niveauDePermissionAdmins.Where(n => n.nomNiveauDePermissionAdmin == nomNiveauPermissionComplet).FirstOrDefault().noNiveauDePermissionAdmin
                };
                db.CompteAdmins.Add(compteAdminPrincipal);
                #endregion

                #region ForfaitPremiums
                //Liste des forfaits premium de base
                List<ForfaitPremium> forfaitPremiums = new List<ForfaitPremium>()
                {
                    new ForfaitPremium() { nbMoisAbonnement = 1, prixParMois = 9.99, prixTotal = 9.99 },
                    new ForfaitPremium() { nbMoisAbonnement = 6, prixParMois = 7.99, prixTotal = 47.99, pourcentageDeRabais = 20 },
                    new ForfaitPremium() { nbMoisAbonnement = 12, prixParMois = 5.99, prixTotal = 71.99, pourcentageDeRabais = 40 }
                };
                db.ForfaitPremiums.AddRange(forfaitPremiums);
                #endregion

                #region ActionTraitements
                //Liste des forfaits premium de base
                List<ActionTraitement> actionTraitements = new List<ActionTraitement>()
                {
                    new ActionTraitement() { nomActionTraitement = "Compte bloqué" },
                    new ActionTraitement() { nomActionTraitement = "Avertissement envoyé" },
                    new ActionTraitement() { nomActionTraitement = "État de la plainte modifié à: Nouveau" },
                    new ActionTraitement() { nomActionTraitement = "État de la plainte modifié à: Assigné" },
                    new ActionTraitement() { nomActionTraitement = "État de la plainte modifié à: En suivi" },
                    new ActionTraitement() { nomActionTraitement = "État de la plainte modifié à: À traiter" },
                    new ActionTraitement() { nomActionTraitement = "État de la plainte modifié à: Traité" }
                };
                db.ActionTraitements.AddRange(actionTraitements);
                #endregion

                #endregion

                //On passe aux données bidons

                if (id != -1)
                {
                    #region Génération données Bidon

                    #region Generation De plusieurs listes de hobbies différentes (Pas d'ajout à la BD)
                    //Génération de plusieurs listes de hobbies différentes, 
                    //qu'on va ensuite affecter aux membres
                    List<List<Hobbie>> hob = new List<List<Hobbie>>();

                    for (int i = 0; i < nbMembresEtActivitesBidons; i++)
                    {
                        //on ajoute entre 1 et x hobbies de chacune des catégories
                        List<Hobbie> hobbies = new List<Hobbie>();
                        int nbr = Faker.RandomNumber.Next(1, 15);
                        for (int j = 0; j < nbr; j++)
                        {
                            hobbies.Add(Pick<Hobbie>.RandomItemFrom(social));
                        }
                        nbr = Faker.RandomNumber.Next(1, 5);
                        for (int j = 0; j < nbr; j++)
                        {
                            hobbies.Add(Pick<Hobbie>.RandomItemFrom(sports));
                        }
                        nbr = Faker.RandomNumber.Next(1, 15);
                        for (int j = 0; j < nbr; j++)
                        {
                            hobbies.Add(Pick<Hobbie>.RandomItemFrom(culture));
                        }
                        nbr = Faker.RandomNumber.Next(1, 15);
                        for (int j = 0; j < nbr; j++)
                        {
                            hobbies.Add(Pick<Hobbie>.RandomItemFrom(musique));
                        }
                        nbr = Faker.RandomNumber.Next(1, 15);
                        for (int j = 0; j < nbr; j++)
                        {
                            hobbies.Add(Pick<Hobbie>.RandomItemFrom(social));
                        }
                        nbr = Faker.RandomNumber.Next(1, 15);
                        for (int j = 0; j < nbr; j++)
                        {
                            hobbies.Add(Pick<Hobbie>.RandomItemFrom(arts));
                        }
                        nbr = Faker.RandomNumber.Next(1, 15);
                        for (int j = 0; j < nbr; j++)
                        {
                            hobbies.Add(Pick<Hobbie>.RandomItemFrom(passeTemps));
                        }
                        hob.Add(hobbies);
                    }
                    #endregion

                    #region Generation De plusieurs listes de raisons sur le site différentes (Pas d'ajout à la BD)
                    //Génération de plusieurs listes de raisons sur le site différentes, 
                    //qu'on va ensuite affecter aux membres
                    List<List<RaisonsSurSite>> raisonss = new List<List<RaisonsSurSite>>();
                    for (int i = 0; i < nbMembresEtActivitesBidons; i++)
                    {
                        List<RaisonsSurSite> rai = new List<RaisonsSurSite>();
                        int nbr = Faker.RandomNumber.Next(1, 2);
                        for (int j = 0; j < nbr; j++)
                        {
                            rai.Add(Pick<RaisonsSurSite>.RandomItemFrom(raisons));
                        }

                        raisonss.Add(rai);
                    }
                    #endregion

                    #region Generation des membres
                    //Génère les membres
                    var membres = Builder<Membre>.CreateListOfSize(nbMembresEtActivitesBidons)
                        .All()
                            .With(m => m.nom = Faker.Name.Last())
                            .With(m => m.prenom = Faker.Name.First())
                            .With(m => m.surnom = (Faker.Name.First()))
                            .With(m => m.courriel = Faker.Internet.Email())
                            .With(m => m.dateNaissance = DateTime.Now.AddYears(-numberGenerator.Next(20, 60)).AddDays(-numberGenerator.Next(1, 360)))
                            .With(m => m.homme = Pick<Boolean>.RandomItemFrom(booleann))
                            .With(m => m.emailConfirme = false)
                            .With(m => m.taille = Pick<Taille>.RandomItemFrom(tailles))
                            .With(m => m.silhouette = Pick<Silhouette>.RandomItemFrom(silhouettes))
                            .With(m => m.description = LesUtilitaires.Utilitaires.Truncate(Faker.Lorem.Paragraph(2), 200))
                            .With(m => m.rechercheHomme = Pick<Boolean>.RandomItemFrom(booleann))
                            .With(m => m.motDePasse = "abcde")
                            .With(m => m.nbEnfants = Faker.RandomNumber.Next(0, 8))
                            .With(m => m.nbAnimaux = Faker.RandomNumber.Next(0, 8))
                            .With(m => m.fumeur = Pick<Boolean>.RandomItemFrom(booleann))
                            .With(m => m.premium = Pick<Boolean>.RandomItemFrom(booleann))
                            .With(m => m.province = Pick<Province>.RandomItemFrom(Provinces))
                            .With(m => m.ville = Pick<Ville>.RandomItemFrom(m.province.listeVilles))
                            .With(m => m.religion = Pick<Religion>.RandomItemFrom(religions))
                            .With(m => m.yeuxCouleur = Pick<YeuxCouleur>.RandomItemFrom(yeux))
                            .With(m => m.cheveuxCouleur = Pick<CheveuxCouleur>.RandomItemFrom(cheveux))
                            .With(m => m.origine = Pick<Origine>.RandomItemFrom(origines))
                            .With(m => m.listeHobbies = Pick<List<Hobbie>>.RandomItemFrom(hob).ToList())
                            .With(m => m.listeRaisonsSurSite = Pick<List<RaisonsSurSite>>.RandomItemFrom(raisonss).ToList())
                            .With(m => m.occupation = Pick<Occupation>.RandomItemFrom(occupations))
                            .With(m => m.niveauEtude = Pick<NiveauEtude>.RandomItemFrom(niveauEtudes))
                            .With(m => m.situationFinanciere = Pick<SituationFinanciere>.RandomItemFrom(situationFinancieres))
                            .With(m => m.desirEnfant = Pick<DesirEnfant>.RandomItemFrom(desirEnfants))
                            .With(m => m.dateSuppressionDuCompte = null)
                            .With(m => m.compteSupprimeParAdmin = null)
                            .With(m => m.dateInscription = DateTime.Now.AddSeconds(-Faker.RandomNumber.Next(0, 20000)))
                            .Build();

                    foreach(Membre m in membres)
                    {
                        if(m.premium)
                        {
                            List<int> nbMoisPossibles = forfaitPremiums.Select(f => f.nbMoisAbonnement).ToList();
                            int nbMois = Pick<int>.RandomItemFrom(nbMoisPossibles);

                            List<string> typeCartes = new List<string> { "Visa", "MasterCard", "Amex" };

                            DateTime now = DateTime.Now;

                            Abonnement lAbonnementDuMembre = new Abonnement();
                            lAbonnementDuMembre.dateDebut = now;
                            lAbonnementDuMembre.dateFin = DateTime.Now.AddMonths(nbMois);
                            lAbonnementDuMembre.coutTotal = forfaitPremiums.Where(f => f.nbMoisAbonnement == nbMois).Select(f => f.prixTotal).FirstOrDefault();
                            lAbonnementDuMembre.membre = m;
                            lAbonnementDuMembre.noMembre = m.noMembre;
                            lAbonnementDuMembre.datePaiement = now;
                            lAbonnementDuMembre.typeAbonnement = nbMois;
                            lAbonnementDuMembre.renouveler = null;

                            //Nouvelles informations
                            lAbonnementDuMembre.prenomSurCarte = m.prenom;
                            lAbonnementDuMembre.nomSurCarte = m.nom;
                            lAbonnementDuMembre.quatreDerniersChiffres = Faker.RandomNumber.Next(0,9).ToString() + Faker.RandomNumber.Next(0, 9).ToString() + Faker.RandomNumber.Next(0, 9).ToString() + Faker.RandomNumber.Next(0, 9).ToString();
                            lAbonnementDuMembre.typeCarte = Pick<string>.RandomItemFrom(typeCartes);
                            lAbonnementDuMembre.adresseFacturation = Utilitaires.Truncate(Faker.Address.StreetAddress(), 60);
                            lAbonnementDuMembre.villeFacturation = m.ville.nomVille;
                            lAbonnementDuMembre.codePostalFacturation = "J2G 3H7";
                            lAbonnementDuMembre.provinceFacturation = m.province;
                            lAbonnementDuMembre.noProvince = m.province.noProvince;

                            lAbonnementDuMembre.prixTPS =  Math.Round(lAbonnementDuMembre.coutTotal * 0.05, 2);
                            if(lAbonnementDuMembre.provinceFacturation.nomProvince == "Québec")
                            {
                                lAbonnementDuMembre.prixTVQ = Math.Round(lAbonnementDuMembre.coutTotal * 0.09975, 2);
                            }

                            db.Abonnements.Add(lAbonnementDuMembre);
                        }
                    }

                    db.Membres.AddRange(membres);
                    #endregion

                    #region Création d'une liste avec tous les membres premium (Pas d'ajout à la BD)
                    List<Membre> membresPremium = new List<Membre>();
                    foreach (Membre m in membres)
                    {
                        if (m.premium == true)
                        {
                            membresPremium.Add(m);
                        }
                    }
                    #endregion

                    #region Generation des activites
                    List<bool?> trueOuFalseOuNull = new List<bool?> { true, false, null };

                    IList<Activite> activites = new List<Activite>();

                    if (membresPremium.Count > 0)
                    {
                        for (int i = 0; i < nbMembresEtActivitesBidons; i++)
                        {
                            int ageMin = Faker.RandomNumber.Next(18, 90);
                            int ageMax = Faker.RandomNumber.Next(ageMin, 90);

                            int random = Faker.RandomNumber.Next(1, 3);

                            int random2 = Faker.RandomNumber.Next(1, 3);

                            Activite activite;
                            if (random == 1)
                            {
                                activite = Builder<Activite>.CreateNew()
                                .With(a => a.nom = LesUtilitaires.Utilitaires.Truncate(Faker.Lorem.Paragraph(5), 20))
                                .With(a => a.description = LesUtilitaires.Utilitaires.Truncate(Faker.Lorem.Paragraph(3), 250))
                                .With(a => a.date = DateTime.Now.AddDays(-numberGenerator.Next(1, 100)))
                                .With(a => a.cout = Faker.RandomNumber.Next(0, 100))
                                .With(a => a.ageMax = ageMax)
                                .With(a => a.ageMin = ageMin)
                                .With(a => a.adresse = (Faker.RandomNumber.Next(0, 5000).ToString() + ", Rue " + LesUtilitaires.Utilitaires.Truncate(Faker.Lorem.Paragraph(5), 26)))
                                .With(a => a.province = Pick<Province>.RandomItemFrom(Provinces))
                                .With(a => a.ville = Pick<Ville>.RandomItemFrom(a.province.listeVilles))
                                .With(a => a.theme = Pick<Theme>.RandomItemFrom(themes))
                                .With(a => a.membreOrganisateur = Pick<Membre>.RandomItemFrom(membresPremium))
                                .With(a => a.nbParticipantsMax = (random2 == 1 ? Faker.RandomNumber.Next(2, 30) : (int?)null))
                                .With(a => a.hommeSeulement = Pick<bool?>.RandomItemFrom(trueOuFalseOuNull))
                                .Build();
                            }
                            else
                            {
                                activite = Builder<Activite>.CreateNew()
                                .With(a => a.nom = LesUtilitaires.Utilitaires.Truncate(Faker.Lorem.Paragraph(5), 20))
                                .With(a => a.description = LesUtilitaires.Utilitaires.Truncate(Faker.Lorem.Paragraph(3), 250))
                                .With(a => a.date = DateTime.Now.AddDays(numberGenerator.Next(1, 100)))
                                .With(a => a.cout = Faker.RandomNumber.Next(0, 100))
                                .With(a => a.ageMax = ageMax)
                                .With(a => a.ageMin = ageMin)
                                .With(a => a.adresse = (Faker.RandomNumber.Next(0, 5000).ToString() + ", Rue " + LesUtilitaires.Utilitaires.Truncate(Faker.Lorem.Paragraph(5), 26)))
                                .With(a => a.province = Pick<Province>.RandomItemFrom(Provinces))
                                .With(a => a.ville = Pick<Ville>.RandomItemFrom(a.province.listeVilles))
                                .With(a => a.theme = Pick<Theme>.RandomItemFrom(themes))
                                .With(a => a.membreOrganisateur = Pick<Membre>.RandomItemFrom(membresPremium))
                                .With(a => a.nbParticipantsMax = (random2 == 1 ? Faker.RandomNumber.Next(2, 30) : (int?)null))
                                .With(a => a.hommeSeulement = Pick<bool?>.RandomItemFrom(trueOuFalseOuNull))
                                .Build();
                            }
                            #region Ajout Participants
                            int nbParticipantsAAjouter;
                            if(activite.nbParticipantsMax != null)
                            {
                                //Si on a encore de la place
                                if(activite.nbParticipantsMax > activite.membresParticipants.Count)
                                {
                                    nbParticipantsAAjouter = Faker.RandomNumber.Next(0, ((int)activite.nbParticipantsMax - activite.membresParticipants.Count));
                                }
                                else
                                {
                                    nbParticipantsAAjouter = 0;
                                }
                            }
                            else
                            {
                                nbParticipantsAAjouter = Faker.RandomNumber.Next(0, 20);
                            }

                            //Tous les membres qui ne sont pas le membre organisateur
                            IList<Membre> membresRestant = membres.Where(m => m.noMembre != activite.membreOrganisateur.noMembre).ToList();
                            for (int j = 0; j < nbParticipantsAAjouter; j++)
                            {
                                //Trouver tous les membres qui correspondent au profil de l'activité
                                List<Membre> membresPotentiels = membresRestant.Where(m =>
                                            ((activite.hommeSeulement == null ? true : false) || (activite.hommeSeulement == m.homme)) &&
                                            ((activite.ageMax == null ? true : false) || (activite.ageMax >= m.age)) &&
                                            ((activite.ageMin == null ? true : false) || (activite.ageMin <= m.age))
                                        ).ToList();

                                //Si on ne trouve aucun membre qui correspond au profil qu'on recherche
                                if (membresPotentiels.Count > 0)
                                {
                                    Membre leMembreAAjouter = Pick<Membre>.RandomItemFrom(membresPotentiels);

                                    activite.membresParticipants.Add(leMembreAAjouter);
                                    membresRestant.Remove(leMembreAAjouter);
                                    membresPotentiels.Remove(leMembreAAjouter);
                                }
                                else
                                {
                                    //On break;
                                    break;
                                }
                            }
                            #endregion

                            #region AvisActivite
                            if(activite.date < DateTime.Now)
                            {
                                int nbAvisAjouter;
                                //Si on a encore de la place
                                nbAvisAjouter = Faker.RandomNumber.Next(0, activite.membresParticipants.Count);

                                //Tous les membres qui ne sont pas le membre organisateur
                                IList<Membre> membresRestantAvisActivite = membres.Where(m => m.noMembre != activite.membreOrganisateur.noMembre && 
                                                                                  /*m.listeActivites.Select(a => a.noActivite).Contains(activite.noActivite)*/
                                                                                  activite.membresParticipants.Contains(m)).ToList();
                                for (int j = 0; j < nbParticipantsAAjouter; j++)
                                {
                                    //Si on ne trouve aucun membre qui correspond au profil qu'on recherche
                                    if (membresRestantAvisActivite.Count > 0)
                                    {
                                        Membre leMembre = Pick<Membre>.RandomItemFrom(membresRestantAvisActivite);

                                        List<int?> cote = new List<int?> { 1, 2, 3, 4, 5, null };

                                        int randomNumber = Faker.RandomNumber.Next(1, 3);

                                        AvisActivite avisActivite = Builder<AvisActivite>.CreateNew()
                                            .With(a => a.cote = Pick<int?>.RandomItemFrom(cote))
                                            .With(a => a.commentaire = randomNumber == 1 ? LesUtilitaires.Utilitaires.Truncate(Faker.Lorem.Paragraph(3), 50) : null)
                                            .With(a => a.activiteAssocie = activite)
                                            .With(a => a.membreEnvoyeur = leMembre)
                                            .Build();

                                        //avisActivites.Add(avisActivite);
                                        //activite.listeAvisActivite.Add(avisActivite);
                                        //On ajoute l'avisActivite à la BD
                                        db.AvisActivites.Add(avisActivite);
                                        membresRestantAvisActivite.Remove(leMembre);
                                    }
                                    else
                                    {
                                        //On break;
                                        break;
                                    }
                                }
                            }
                            #endregion

                            activites.Add(activite);
                        }
                        db.Activites.AddRange(activites);
                    }
                    #endregion

                    #region Generation d'avis sur les activités passés

                    List<Activite> activitesPasser = activites.Where(a => a.date < DateTime.Now).ToList();

                    List<AvisActivite> avisActivites = new List<AvisActivite>();

                    if (activitesPasser.Count() > 0)
                    {
                        IList<Membre> membresRestant = membres;

                        for(int i = 0; i < 100; i++)
                        {
                            if(membresRestant.Count > 0)
                            {
                                AvisActivite avisActivite;

                                //On ne veut pas que le membre apparaisse à 2 places.......
                                Membre leMembre = Pick<Membre>.RandomItemFrom(membresRestant);
                                membresRestant.Remove(leMembre);

                                List<int?> cote = new List<int?> { 1, 2, 3, 4, 5, null };

                                int randomNumber = Faker.RandomNumber.Next(1, 3);

                                avisActivite = Builder<AvisActivite>.CreateNew()
                                    .With(a => a.cote = Pick<int?>.RandomItemFrom(cote))
                                    .With(a => a.commentaire = randomNumber == 1 ? LesUtilitaires.Utilitaires.Truncate(Faker.Lorem.Paragraph(3), 50) : null)
                                    .With(a => a.activiteAssocie = Pick<Activite>.RandomItemFrom(activitesPasser))
                                    .With(a => a.membreEnvoyeur = leMembre)
                                    .Build();

                                avisActivites.Add(avisActivite);
                            }
                            else
                            {
                                break;
                            }
                        }

                        db.AvisActivites.AddRange(avisActivites);
                    }
                    #endregion

                    #endregion
                }

                db.SaveChanges();

                ViewBag.reussi = true;

                return View();
            }
            catch (DbEntityValidationException dbEx)
            {
                ViewBag.reussi = false;
                string messageErreur = dbEx.Message + "<br/> <br/>" + "<strong>Erreurs principales:</strong><br/><br/>";

                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        //on évite de montrer plusieurs fois la meme erreur pour la meme propriété, ca sert à rien
                        if (!messageErreur.Contains("Propriété: " + validationError.PropertyName + " Erreur: " + validationError.ErrorMessage + "<br/> "))
                        {
                            messageErreur += "Propriété: " + validationError.PropertyName + " Erreur: " + validationError.ErrorMessage + "<br/> ";
                        }
                    }
                }

                ViewBag.messageErreur = messageErreur;
                return View();
            }
            catch (Exception e)
            {
                ViewBag.reussi = false;
                ViewBag.messageErreur = e.Message;

                return View();
            }
        }
    }
}
