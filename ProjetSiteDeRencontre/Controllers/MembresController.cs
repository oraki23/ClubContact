/*------------------------------------------------------------------------------------

CONTRÔLEUR POUR TOUT CE QUI ATTRAIT À LA GESTION DES MEMBRES
CELA VA DE LA :
    CRÉATION
    MODIFICATION
    LISTE (INDEX)
    SUPPRESSION/DESACTIVATION

JUSQU'AU FONCTIONNALITÉS DE:
    PAIEMENT
    ABONNEMENT
    ..ETC

--------------------------------------------------------------------------------------
Par: Anthony Brochu et Marie-Ève Massé
Novembre 2017
Club Contact
------------------------------------------------------------------------------------*/

using ProjetSiteDeRencontre.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.IO;
using System.Web.Mvc;
using System.Net;
using System.Globalization;
using Newtonsoft.Json;
using ProjetSiteDeRencontre.ViewModel;
using System.Security.Claims;
using ProjetSiteDeRencontre.LesUtilitaires;
using System.Data.Entity.SqlServer;
using System.Data.Entity.Core.Objects;
using Microsoft.Owin.Security;

namespace ProjetSiteDeRencontre.Controllers
{
    /// <summary>
    /// Contrôleur permettant la gestion des membres. Permet aussi l'initialisation de la Base de données
    /// </summary>
    public partial class MembresController : BaseController
    {
        private ClubContactContext db;

        public MembresController()
        {
            db = new ClubContactContext();
        }
        /*Edit post et get */
        #region
        [HttpGet]
        [Authorize]
        public ActionResult Edit(int? id)
        {
            ViewBag.Title = "Modifier mon profil";

            try
            {
                if(((ClaimsIdentity)User.Identity).FindFirst(ClaimTypes.NameIdentifier).Value != "admin")
                {
                    if (id != int.Parse(Request.Cookies["SiteDeRencontre"]["noMembre"]))
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Action: Edit (GET), Contrôleur: Membres ; Erreur potentielle: Le Cookie noMembre est inexistant ou n'est pas valide. ; id passé en paramètre : " + (id == null ? "null" : id.ToString()) + ".", e);
            }

            try
            {
                Membre membreModifier = db.Membres.Where(m => m.noMembre == id).Include(m => m.listeRaisonsSurSite).Include(m => m.listePhotosMembres).FirstOrDefault();

                List<string> noRaisons = new List<string>();

                foreach (RaisonsSurSite r in membreModifier.listeRaisonsSurSite)
                {
                    noRaisons.Add(r.noRaisonSurSite.ToString());
                }

                TempData["raisonsSurSite"] = JsonConvert.SerializeObject(noRaisons);

                RemplirListesDeroulantesEtPreselectionnerHobbies(membreModifier);

                ViewBag.bouton = "Sauvegarder";
                ViewBag.Create = false;
                ViewBag.nbDePhotos = membreModifier.listePhotosMembres.Count();

                return View("Edit", membreModifier);
            }
            catch(Exception e)
            {
                throw new Exception("Action: Edit (GET), Contrôleur: Membres ; Erreur potentielle: Requête LINQ n'a pas fonctionnée. ; id recherché : " + (id == null ? "null" : id.ToString()) + ".", e);
            }
        }

        [HttpPost]
        public ActionResult Edit(Membre membre, string hobbiesJSON, string selectionPhotoProfil, List<string> deletePhotos, List<HttpPostedFileBase> fichiersPhotos, string raisonsSiteJSON, string motDePasse1/*, string motDePasseOLD*/)
        {
            ViewBag.Title = "Modifier mon profil";

            bool nouveauMembre = (membre.noMembre == 0 ? true : false);

            //on valide le mot de passe dans le cas que le Jquery n'aurait pas fait le travail
            if(nouveauMembre)
            {
                membre.dateInscription = DateTime.Now;

                ValiderMotDePasse(motDePasse1, null, membre.noMembre, nouveauMembre);
            }
            
            if (!nouveauMembre && membre.motDePasseHashe == null)
            {
                try
                {
                    //Aller rechercher le mdp Hashé de la BD afin de le stocker pour pas le perdre
                    //(car on ne le met pas dans un champ hidden ou dans un TempData nul part)
                    string mdpHasheMembre = db.Membres.Where(m => m.noMembre == membre.noMembre).Select(m => m.motDePasseHashe).FirstOrDefault();
                    membre.motDePasseHashe = mdpHasheMembre;
                }
                catch (Exception e)
                {
                    throw new Exception("Action: Edit (POST), Contrôleur: Membres ; Erreur potentielle: Requête LINQ pour aller chercher le mdp Hashé n'a pas fonctionnée. ; Paramètres:"
                            + "\nnoMembre: " + membre.noMembre
                            + "\nnomMembre: " + membre.nom
                            + "\nprenomMembre: " + membre.prenom
                            + "\ncourrielMembre: " + membre.courriel, e);
                }
            }

            //choisir la photo de profil
            for (int i = 0; i < membre.listePhotosMembres.Count; i++)
            {
                if (selectionPhotoProfil != null)
                {
                    if (i == int.Parse(selectionPhotoProfil))
                    {
                        membre.listePhotosMembres[i].photoProfil = true;
                    }
                    else
                    {
                        membre.listePhotosMembres[i].photoProfil = false;
                    }
                }
            }

            //Si on a bien sélectionné des raisonsSite, on enlève les validation du modèle (parce que on va
            //remplacer les valeurs plus tard de toute façon. Testé fonctionnel
            if(raisonsSiteJSON != null || raisonsSiteJSON != string.Empty)
            {
                ModelState["listeRaisonsSurSite"].Errors.Clear();
            }

            if (ModelState.IsValid)
            {
                //Première étape pour la gestion des photos
                GestionPhotos(1, membre, deletePhotos, fichiersPhotos);

                if (nouveauMembre)
                {
                    db.Entry(membre).State = EntityState.Added;
                }
                else
                {
                    db.Entry(membre).State = EntityState.Modified;
                }

                //Deuxième étape pour la gestion des photos
                GestionPhotos(2, membre, deletePhotos, fichiersPhotos);

                try
                {
                    //Première sauvegarde dans la BD
                    db.SaveChanges();
                }
                catch(Exception e)
                {
                    throw new Exception("Action: Edit (POST), Contrôleur: Membres ; Erreur potentielle: Première mise à jour de la BD dans EDIT n'a pas fonctionnée. ; Paramètres:"
                        + "\nnoMembre: " + membre.noMembre
                        + "\nnomMembre: " + membre.nom
                        + "\nprenomMembre: " + membre.prenom
                        + "\ncourrielMembre: " + membre.courriel, e);
                }

                try { 
                    //On va rechercher le membre afin d'inclure la liste
                    //de hobbies, pour ensuite la vider (si jamais il aurait décocher dequoi,
                    //cela permettera de tenir compte des changements). On va donc tout enlever,
                    //puis tout remettre.
                    membre = db.Membres.Where(m => m.noMembre == membre.noMembre).Include(m => m.listeHobbies).Include(m => m.listeRaisonsSurSite).FirstOrDefault();

                    #region SauvegardeListeRaisonSite
                    membre.listeRaisonsSurSite.Clear();  
                    foreach (string s in JsonConvert.DeserializeObject<List<string>>(raisonsSiteJSON.ToString()))
                    {
                        int noRaison = int.Parse(s);
                        RaisonsSurSite raisonSurSite = db.RaisonsSurSites.Where(r => r.noRaisonSurSite == noRaison).FirstOrDefault();

                        //Vérification pour ne pas prendre de risque
                        if (raisonSurSite != null)
                        {
                            membre.listeRaisonsSurSite.Add(raisonSurSite);
                        }
                    }
                    #endregion

                    if (!nouveauMembre)
                    {
                        membre.listeHobbies.Clear();
                        //On parcourt le JSon (qui sera converti en liste de String) et on ajoute
                        //chacun des éléments dans la liste de hobbies directement dans le membre.
                        foreach (string s in JsonConvert.DeserializeObject<List<string>>(hobbiesJSON))
                        {
                            Hobbie hobbie = db.Hobbies.Where(h => h.nomHobbie == s).FirstOrDefault();

                            //Vérification pour ne pas prendre de risque
                            if (hobbie != null)
                            {
                                membre.listeHobbies.Add(hobbie);
                            }
                        }
                    }

                    GestionPhotos(3, membre, deletePhotos, fichiersPhotos);     
                }
                catch(JsonException e)
                {
                    throw new Exception("Action: Edit (POST), Contrôleur: Membres ; Erreur potentielle: Conversion de JSON à List<String> n'a pas fonctionnée. ; Paramètres:"
                        + "\nhobbiesJSON: " + (hobbiesJSON == null ? "null" : hobbiesJSON)
                        + "\nnoMembre: " + membre.noMembre, e);
                }
                catch(Exception e)
                {
                    throw new Exception("Action: Edit (POST), Contrôleur: Membres ; Erreur potentielle: Une des requêtes LINQ dans la gestion des Hobbies et des RaisonsSurSite n'a pas fonctionnée. Sinon, autres erreurs possibles. ; Paramètres:"
                        + "\nnoMembre: " + membre.noMembre
                        + "\nnomMembre: " + membre.nom
                        + "\nprenomMembre: " + membre.prenom
                        + "\ncourrielMembre: " + membre.courriel, e);
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        db.SaveChanges();
                    }
                    catch (DbEntityValidationException ex)
                    {
                        var error = ex.EntityValidationErrors.First().ValidationErrors.First();
                        this.ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Action: Edit (POST), Contrôleur: Membres ; Erreur potentielle: Deuxième mise à jour de la BD dans EDIT n'a pas fonctionnée. ; Paramètres:"
                            + "\nnoMembre: " + membre.noMembre
                            + "\nnomMembre: " + membre.nom
                            + "\nprenomMembre: " + membre.prenom
                            + "\ncourrielMembre: " + membre.courriel, e);
                    }
                }
                else
                {
                    foreach (var key in this.ModelState.Keys)
                    {
                        if (this.ModelState[key].Errors.Count != 0)
                        {
                            TempData["Erreur"] += "Erreur: " + this.ModelState[key].Errors[0].ErrorMessage + "<br/>";
                            TempData.Keep();
                        }
                    }

                    List<string> noRaisons = new List<string>();

                    foreach (RaisonsSurSite r in membre.listeRaisonsSurSite)
                    {
                        noRaisons.Add(r.noRaisonSurSite.ToString());
                    }

                    TempData["raisonsSurSite"] = JsonConvert.SerializeObject(noRaisons);

                    RemplirListesDeroulantesEtPreselectionnerHobbies(membre);

                    ViewBag.listeHobbiesMembre = db.Hobbies.Where(h => h.listeMembre.Any(m => m.noMembre == membre.noMembre)).ToList();

                    ViewBag.Create = nouveauMembre;
                    ViewBag.nbDePhotos = membre.listePhotosMembres.Count();

                    return View("Edit", membre);
                }
            }
            else
            {
                foreach (var key in this.ModelState.Keys)
                {
                    if (this.ModelState[key].Errors.Count != 0)
                    {
                        TempData["Erreur"] += "Erreur: " + this.ModelState[key].Errors[0].ErrorMessage + "<br/>";
                        TempData.Keep();
                    }
                }

                List<string> noRaisons = new List<string>();

                foreach (RaisonsSurSite r in membre.listeRaisonsSurSite)
                {
                    noRaisons.Add(r.noRaisonSurSite.ToString());
                }

                TempData["raisonsSurSite"] = JsonConvert.SerializeObject(noRaisons);

                RemplirListesDeroulantesEtPreselectionnerHobbies(membre);

                ViewBag.listeHobbiesMembre = db.Hobbies.Where(h => h.listeMembre.Any(m => m.noMembre == membre.noMembre)).ToList();

                ViewBag.Create = nouveauMembre;
                ViewBag.nbDePhotos = membre.listePhotosMembres.Count();

                return View("Edit", membre);
            }
            
            if(nouveauMembre)
            {
                //Envoie du courriel

                LesUtilitaires.Utilitaires.envoieCourriel(
                    "Confirmation de votre compte Club Contact",
                    LesUtilitaires.Utilitaires.RenderRazorViewToString(this, "CourrielConfirmation", membre),
                    membre.courriel
                    );

                TempData["Message"] = "Vous êtes maintenant inscrit,<br />vous pouvez désormais vous connecter!";
                return RedirectToAction("Login", "Authentification");
            }
            else
            {
                //TempData["Message"] = "Vos informations ont bien été sauvegardés.";
                return RedirectToAction("Details", "Membres", new { id = membre.noMembre });
            }
        }
        #endregion 

        public ActionResult ConfirmerCourriel(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Membre membre = db.Membres.Where(m => m.noMembre == id).FirstOrDefault();
            if(membre != null)
            {
                membre.emailConfirme = true;

                db.SaveChanges();

                TempData["Message"] = "Votre courriel a bien été confirmé!";

                int valeurCookie;
                if(verifierSiCookieNoMembreExiste(out valeurCookie))
                {
                    if (valeurCookie == membre.noMembre)
                    {
                        return RedirectToAction("Home", "Home");
                    }
                }
            }
            
            return RedirectToAction("Login", "Authentification");
        }

        public ActionResult Index(RechercheViewModel laRecherche, int? page, int? vientDePagination, bool? rechercheAvancee)
        {
            int nbItemsParPages = 20;

            if(vientDePagination != null)
            {
                //Si on vient de cliquer sur un bouton de pagination,
                //on va rechercher l'ancienne recherche et on l'applique
                //.. Si on avait été cherché la nouvelle en faisant post, on aurait aussi amené les 
                //erreurs, ce qu'on ne veut pas!
                laRecherche = TempData["laRechercheAncienne"] as RechercheViewModel;
            }
            else
            {
                page = null;
            }

            int noMembre;
            verifierSiCookieNoMembreExiste(out noMembre);

            if(noMembre != -1)
            {
                try
                {
                    Membre membreConnecte = db.Membres.Where(m => m.noMembre == noMembre).FirstOrDefault();
                    if (membreConnecte != null)
                    {
                        ViewBag.premium = membreConnecte.premium;

                        if (membreConnecte.ville.lat != null && membreConnecte.ville.lng != null)
                        {
                            laRecherche.latMembreConnectee = membreConnecte.ville.lat;
                            laRecherche.lngMembreConnectee = membreConnecte.ville.lng;
                        }
                    }

                    if (rechercheAvancee != null && membreConnecte.premium)
                    {
                        ViewBag.rechercheAvancee = rechercheAvancee;

                        /*Création ViewBag pour liste raisons*/
                        List<RaisonsSurSite> raisonsSurSiteDB = db.RaisonsSurSites.ToList();
                        List<SelectListItem> raisonsSurSiteRechercheAvancee = new List<SelectListItem>();
                        raisonsSurSiteRechercheAvancee.Add(new SelectListItem() { Text = "Trouver l'amour", Value = raisonsSurSiteDB.Where(r => r.raison == "Amour").Select(r => r.noRaisonSurSite).FirstOrDefault().ToString() });
                        raisonsSurSiteRechercheAvancee.Add(new SelectListItem() { Text = "Faire une activité", Value = raisonsSurSiteDB.Where(r => r.raison == "Activité").Select(r => r.noRaisonSurSite).FirstOrDefault().ToString() });
                        //raisonsSurSiteRechercheAvancee.Add(new SelectListItem() { Text = "Partager des sujets", Value = raisonsSurSiteDB.Where(r => r.raison == "Partager").Select(r => r.noRaisonSurSite).FirstOrDefault().ToString() });
                        //raisonsSurSiteRechercheAvancee.Add(new SelectListItem() { Text = "Développer une amitié", Value = raisonsSurSiteDB.Where(r => r.raison == "Amitié").Select(r => r.noRaisonSurSite).FirstOrDefault().ToString() });
                        ViewBag.raisonsSitesAvancee = raisonsSurSiteRechercheAvancee;
                    }
                }
                catch(Exception e)
                {
                    Dictionary<string, string> parametres = new Dictionary<string, string>()
                    {
                        {"noMembre dans le cookie", noMembre.ToString() }
                    };
                    Utilitaires.templateException("Index", "Membres", "Requête LINQ pour aller chercher le membre connecté n'a pas fonctionnée.", e, parametres);
                }
            }
            else
            {
                ViewBag.premium = false;
            }

            if (TempData["laRechercheSuggerer"] != null)
            {
                laRecherche = TempData["laRechercheSuggerer"] as RechercheViewModel;
            }
            else if (laRecherche == null)
            {
                laRecherche = new RechercheViewModel();
            }

            IQueryable<Membre> membresTrouvesQuery;

            if (TempData["membresTrouvesQuery"] != null)
            {
                membresTrouvesQuery = TempData["membresTrouvesQuery"] as IQueryable<Membre>;
            }
            else
            {
                membresTrouvesQuery = RechercheMembreAvecProprietes(laRecherche, noMembre);
            }

            GestionPagination(page, nbItemsParPages, membresTrouvesQuery.Count());

            try
            {

                IQueryable<Membre> membreTrouvesTrier;

                if (laRecherche.triPar == "age")
                {
                    membreTrouvesTrier = membresTrouvesQuery.OrderByDescending(m => m.dateNaissance).ThenBy(m => m.premium ? 0 : 1).ThenBy(m => m.noMembre);
                }
                else if(laRecherche.triPar == "distance")
                {
                    double lngMembre = (double)laRecherche.lngMembreConnectee;
                    double latMembre = (double)laRecherche.latMembreConnectee;

                    membreTrouvesTrier = (from m in membresTrouvesQuery
                     let membreLatitude = (double)m.ville.lat
                     let membreLongitude = (double)m.ville.lng
                     let theta = ((lngMembre - membreLongitude) * Math.PI / 180.0)
                     let requestLat = (latMembre * Math.PI / 180.0)
                     let membreLat = (membreLatitude * Math.PI / 180.0)
                     let dist = (SqlFunctions.Sin(requestLat) * SqlFunctions.Sin(membreLat)) +
                                 (SqlFunctions.Cos(requestLat) * SqlFunctions.Cos(membreLat) * SqlFunctions.Cos(theta))
                     let cosDist = SqlFunctions.Acos(dist)
                     let degDist = (cosDist / Math.PI * 180.0)
                     let absoluteDist = degDist * 60 * 1.1515
                     let distInKM = absoluteDist * 1.609344
                     orderby distInKM, (m.premium ? 0 : 1), m.noMembre
                     select m);
                }
                else
                {
                    membreTrouvesTrier = membresTrouvesQuery.OrderBy(m => m.premium ? 0 : 1).ThenBy(m => m.noMembre);
                }

                ViewBag.nbResultatsRecherche = membreTrouvesTrier.Count();

                List<Membre> membresTrouves = membreTrouvesTrier
                .Skip((int)(ViewBag.currentPage - 1) * nbItemsParPages)
                .Take(nbItemsParPages)
                .Include(m => m.listePhotosMembres)
                .ToList();

                laRecherche.membres = membresTrouves;
            }
            catch(Exception e)
            {
                Dictionary<string, string> parametres = new Dictionary<string, string>()
                    {
                        {"La Query:", membresTrouvesQuery.ToString() }
                    };
                Utilitaires.templateException("Index", "Membres", "Requête LINQ pour aller chercher les membres des résultats de recherche n'a pas fonctionnée.", e, parametres);
            }
            
            RemplirListesDeroulantesEtPreselectionnerHobbies(new Membre(), false);
            ViewBag.listeType = new SelectList(ViewBag.listeType, "noType", "nomType", laRecherche.noTypeActiviteRecherche);

            //On stocke la recherche actuelle, dans le cas où on utiliserais la pagination
            TempData["laRechercheAncienne"] = laRecherche;

            ViewBag.Title = "Recherche de membres";

            //Si le viewBag n'a pas été défini, on le définit.
            if(ViewBag.rechercheAvancee == null)
            {
                ViewBag.rechercheAvancee = false;
            }

            return View("Index", laRecherche);
        }

        [HttpGet]
        public ActionResult Create()
        {
            ViewBag.Title = "Créer mon profil";

            Membre membre = new Membre();

            TempData["raisonsSurSite"] = JsonConvert.SerializeObject("");
            TempData.Keep();

            RemplirListesDeroulantesEtPreselectionnerHobbies(membre);

            ViewBag.bouton = "S'inscrire";
            ViewBag.Create = true;
            ViewBag.Title = "Inscription";

            return View("Edit", membre);
        }

        [HttpPost]
        public ActionResult Create(PreInscription nouveauMembre, string raisonsSiteJSON)
        {
            ViewBag.Title = "Créer mon profil";

            Membre membre = new Membre();

            membre.homme = (bool)nouveauMembre.homme;

            //On stocke les raisons sur le site du nouveau membre dans un TempData
            //pour le traiter dans le dernier Edit
            TempData["raisonsSurSite"] = raisonsSiteJSON;
            TempData.Keep();

            RemplirListesDeroulantesEtPreselectionnerHobbies(membre);

            ViewBag.bouton = "S'inscrire";
            ViewBag.Create = true;
            ViewBag.Title = "Inscription";

            return View("Edit", membre);
        }

        [Authorize]
        public ActionResult Delete(int? id)
        {
            ViewBag.Title = "Supprimer mon profil";

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            try
            {
                Membre membre = db.Membres.Where(pr => pr.noMembre == id).FirstOrDefault();

                if (membre == null)
                {
                    return HttpNotFound();
                }

                return View("Delete", membre);
            }
            catch (Exception e)
            {
                Dictionary<string, string> parametres = new Dictionary<string, string>() {
                    { "id", (id == null ? "null" : id.ToString()) }
                };
                throw Utilitaires.templateException("Delete", "Membres", "Requête LINQ n'a pas fonctionnée.", e, parametres);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int noMembre)
        {
            Membre mem = db.Membres.Where(m => m.noMembre == noMembre).Include(p => p.listePhotosMembres).Include(a => a.listeActivitesOrganises)
                .Include(a => a.listeHobbies).Include(a => a.listeRaisonsSurSite).Include(a => a.listeDeVisitesDeMonProfil)
                .FirstOrDefault();
            List<Activite> activites = db.Activites.Where(a => a.noMembreOrganisateur == mem.noMembre).ToList();
            List<Photo> photos = db.Photos.Where(p => p.noMembre == mem.noMembre).ToList();

            List<Visite> visites = db.Visites.Where(p => p.noMembreVisite == mem.noMembre || p.noMembreVisiteur == mem.noMembre).ToList();

            if (mem == null)
            {
                return HttpNotFound();
            }
            else
            {
                mem.dateSuppressionDuCompte = DateTime.Now;
                mem.compteSupprimeParAdmin = false;

                //On annule chacune de ses activites FUTURES.
                foreach(Activite a in mem.listeActivitesOrganises)
                {
                    if(a.date > DateTime.Now)
                    {
                        Activite acti;
                        Utilitaires.AnnulerActivite(a.noActivite, null, out acti);
                    }
                }

                //On retire sa participation à tous les activités FUTURES.
                foreach(Activite a in mem.listeActivites)
                {
                    if(a.date > DateTime.Now)
                    {
                        Utilitaires.ParticiperActivite(a.noActivite, noMembre);
                    }
                }

                TempData["deleteConfirmed"] = "Votre compte à bel et bien été supprimé, nous espérons vous revoir bientôt!";
                TempData.Keep();

                db.SaveChanges();
            }

            return RedirectToAction("Logout", "Authentification");
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            try
            {
                Membre membre = db.Membres.Where(pr => pr.noMembre == id).Include(pr => pr.listePhotosMembres).Include(pr => pr.listeHobbies).FirstOrDefault();

                ViewBag.listeType = db.Types.ToList();

                if (membre == null)
                {
                    return HttpNotFound();
                }
                else if(membre.compteSupprimeParAdmin != null || membre.dateSuppressionDuCompte != null)
                {
                    //On pourrait ajouter une page différente pour que cela soit plus clair!
                    return HttpNotFound();
                }

                //***Un membre bloqué par un membre ne pourra pas voir la page détails de ce membre
                //Si on a un membre Connecté
                int noMembreCo;
                if (verifierSiCookieNoMembreExiste(out noMembreCo))
                {
                    Membre leMembreCo = db.Membres.Where(m => m.noMembre == noMembreCo).Include(m => m.listeMembresQuiMontBloque).FirstOrDefault();
                    if(leMembreCo.listeMembresQuiMontBloque.Select(m => m.noMembre).Any(m => m == membre.noMembre))
                    {
                        return HttpNotFound();
                    }
                }

                ViewBag.pourcentageCompleteProfil = Utilitaires.CalculerPourcentageMembre(membre);

                ViewBag.membreBloquer = false;
                ViewBag.membreFavoris = false;
                int noMembreCookie;
                verifierSiCookieNoMembreExiste(out noMembreCookie);
                //Si le membre n'est pas allé voir son profil
                if (noMembreCookie != membre.noMembre)
                {
                    //Logique de vérifier si membre est bloqué

                    Membre membreConnecte = db.Membres.Where(m => m.noMembre == noMembreCookie).Include(m => m.listeNoire).Include(m => m.listeDeVisitesDeMonProfil).FirstOrDefault();
                    if(membreConnecte != null)
                    {

                        if(membreConnecte.listeNoire.Contains(membre))
                        {
                            ViewBag.membreBloquer = true;
                        }
                        else
                        {
                            try
                            {
                                Visite visite = new Visite();
                                visite.membreVisite = membre;
                                visite.membreVisiteur = membreConnecte;
                                visite.dateVisite = DateTime.Now;

                                db.Visites.Add(visite);

                                membre.listeDeVisitesDeMonProfil.Add(visite);

                                db.SaveChanges();
                            }
                            catch(Exception e)
                            {
                                Dictionary < string, string> parametres = new Dictionary<string, string>
                                {
                                    { "id", id.ToString() }
                                };

                                throw LesUtilitaires.Utilitaires.templateException("Details", "Membres", "Requete LINQ n'a pas fonctionnée",
                                    e, parametres);
                            }

                        }
                        if(membreConnecte.listeFavoris.Contains(membre))
                        {
                            ViewBag.membreFavoris = true;
                        }
                    }
                }
                
                ViewBag.Title = membre.surnom;

                return View("DetailsMembre", membre);
            }
            catch(Exception e)
            {
                Dictionary<string, string> parametres = new Dictionary<string, string>() {
                    { "id", (id == null ? "null" : id.ToString()) }
                };
                throw Utilitaires.templateException("Details", "Membres", "Requête LINQ n'a pas fonctionnée.", e, parametres);
            }
        }

        [HttpGet]
        [Authorize]
        public ActionResult Options(int? id)
        {
            ViewBag.Title = "Options du compte";

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            try
            {
                if (((ClaimsIdentity)User.Identity).FindFirst(ClaimTypes.NameIdentifier).Value != "admin")
                {
                    if (id != int.Parse(Request.Cookies["SiteDeRencontre"]["noMembre"]))
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                    }
                }
            }
            catch (Exception e)
            {
                Dictionary<string, string> parametres = new Dictionary<string, string>() {
                    { "id", (id == null ? "null" : id.ToString()) }
                };
                throw Utilitaires.templateException("Options", "Membres", "Le Cookie noMembre est inexistant ou n'est pas valide.", e, parametres, "get");
            }

            OptionsCompteViewModel optionMembre = new OptionsCompteViewModel();
            
            try
            {
                var membre = db.Membres.Where(m => m.noMembre == id).Select(m => new { noMembre = m.noMembre, premium = m.premium }).FirstOrDefault();
                optionMembre.noMembre = membre.noMembre;
                optionMembre.premium = membre.premium;
            }
            catch (Exception e)
            {
                Dictionary<string, string> parametres = new Dictionary<string, string>() {
                    { "id", (id == null ? "null" : id.ToString()) }
                };
                throw Utilitaires.templateException("Options", "Membres", "Requête LINQ n'a pas fonctionnée.", e, parametres, "get");
            }   

            return View("Options", optionMembre);
        }

        public ActionResult RetirerDeVisitePuisDetails(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            int noMembreCookie;
            Membre leMembreConnecte;

            try
            {
                verifierSiCookieNoMembreExiste(out noMembreCookie);
                leMembreConnecte = db.Membres.Where(m => m.noMembre == noMembreCookie).Include(m => m.listeDeVisitesDeMonProfil).FirstOrDefault();

                if(leMembreConnecte == null)
                {
                    throw new Exception("Le membreConnecte est égal à null");
                }
            }
            catch (Exception e)
            {
                Dictionary<string, string> parametres = new Dictionary<string, string>() {
                    { "id", id.ToString() }
                };
                throw Utilitaires.templateException("RetirerDeVisitePuisDetails", "Membres", "Le Cookie noMembre est inexistant ou n'est pas valide.", e, parametres, "get");
            }

            try
            {
                List<Visite> visitesDuMembreDetailsVersMembreConnecte = db.Visites.Where(m => m.membreVisite.noMembre == leMembreConnecte.noMembre && m.membreVisiteur.noMembre == id).ToList();

                foreach(Visite v in visitesDuMembreDetailsVersMembreConnecte)
                {
                    leMembreConnecte.listeDeVisitesDeMonProfil.Remove(v);
                    db.Visites.Remove(v);
                }

                db.SaveChanges();
            }
            catch(Exception e)
            {
                Dictionary<string, string> parametres = new Dictionary<string, string>() {
                    { "id", id.ToString() },
                    { "noMembreConnecte", leMembreConnecte.noMembre.ToString() }
                };
                throw Utilitaires.templateException("RetirerDeVisitePuisDetails", "Membres", "La sauvegarde dans la BD n'a pas fonctionnée.", e, parametres);

            }

            return RedirectToAction("Details", "Membres", new { id = id });
        }

        [HttpGet]
        [Authorize]
        public ActionResult VuProfil(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            try
            {
                if (((ClaimsIdentity)User.Identity).FindFirst(ClaimTypes.NameIdentifier).Value != "admin")
                {
                    if (id != int.Parse(Request.Cookies["SiteDeRencontre"]["noMembre"]))
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                    }
                }
            }
            catch (Exception e)
            {
                Dictionary<string, string> parametres = new Dictionary<string, string>() {
                    { "id", (id == null ? "null" : id.ToString()) }
                };
                throw Utilitaires.templateException("VuProfil", "Membres", "Le Cookie noMembre est inexistant ou n'est pas valide.", e, parametres, "get");
            }
            Membre membre;
            try
            {
                membre = db.Membres.Where(m => m.noMembre == id).Include(m => m.listeDeVisitesDeMonProfil).FirstOrDefault();
            }
            catch (Exception e)
            {
                Dictionary<string, string> parametres = new Dictionary<string, string>() {
                    { "id", (id == null ? "null" : id.ToString()) }
                };
                throw Utilitaires.templateException("VuProfil", "Membres", "Requête LINQ n'a pas fonctionnée.", e, parametres, "get");
            }

            return View("VuProfil", membre);
        }

        [HttpGet]
        [Authorize]
        public ActionResult Abonnement(int? id)
        {
            ViewBag.Title = "Mon abonnement";

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            try
            {
                if (((ClaimsIdentity)User.Identity).FindFirst(ClaimTypes.NameIdentifier).Value != "admin")
                {
                    if (id != int.Parse(Request.Cookies["SiteDeRencontre"]["noMembre"]))
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                    }
                }
            }
            catch (Exception e)
            {
                Dictionary<string, string> parametres = new Dictionary<string, string>() {
                    { "id", (id == null ? "null" : id.ToString()) }
                };
                throw Utilitaires.templateException("Abonnement", "Membres", "Le Cookie noMembre est inexistant ou n'est pas valide.", e, parametres, "get");
            }

            OptionsCompteViewModel optionMembre = new OptionsCompteViewModel();

            try
            {
                var membre = db.Membres.Where(m => m.noMembre == id).Select(m => new { noMembre = m.noMembre, premium = m.premium }).FirstOrDefault();
                optionMembre.noMembre = membre.noMembre;
                optionMembre.premium = membre.premium;

                if(membre.premium)
                {
                    DateTime? abonnementLePlusRecent = db.Abonnements.Where(m => m.noMembre == membre.noMembre).OrderByDescending(m => m.dateFin).Select(m => m.dateFin).FirstOrDefault();
                    if (abonnementLePlusRecent != null || abonnementLePlusRecent == new DateTime(0001, 1, 1))
                    {
                        ViewBag.dateDeFinAbonnement = abonnementLePlusRecent;
                    }
                }
            }
            catch (Exception e)
            {
                Dictionary<string, string> parametres = new Dictionary<string, string>() {
                    { "id", (id == null ? "null" : id.ToString()) }
                };
                throw Utilitaires.templateException("Options", "Membres", "Requête LINQ n'a pas fonctionnée.", e, parametres, "get");
            }

            ViewBag.forfaitsPremium = db.ForfaitPremiums.ToList();

            return View("Abonnement", optionMembre);
        }

        [HttpPost]
        [Authorize]
        public ActionResult ChoisirAbonnementVersInformationsPaiement(int? id, int nbMois)
        {
            ViewBag.Title = "Paiement";

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            PaiementViewModel paiement = new PaiementViewModel();
            paiement.noMembre = (int)id;

            RemplirListeDeroulantesPaiement(paiement);

            paiement.nbMois = nbMois;

            return View("InformationsPaiement", paiement);
        }

        [HttpPost]
        [Authorize]
        public ActionResult InformationsPaiement(PaiementViewModel paiement)
        {
            double prix = db.ForfaitPremiums.Where(f => f.nbMoisAbonnement == paiement.nbMois).Select(f => f.prixTotal).FirstOrDefault();

            if (paiement.noProvince != null)
            {
                paiement.provinceFacturation = db.Provinces.Where(p => p.noProvince == paiement.noProvince).FirstOrDefault();

                if (paiement.provinceFacturation == null)
                {
                    paiement.noProvince = null;
                }

                TryValidateModel(paiement);
            }

            if (ModelState.IsValid)
            {
                paiement.TPS = Math.Round(prix * 0.05, 2);

                if(paiement.provinceFacturation.nomProvince == "Québec")
                {
                    paiement.TVQ = Math.Round(prix * 0.09975, 2);
                }
                RemplirListeDeroulantesPaiement(paiement);
                return View("Facture", paiement);
            }
            else
            {
                foreach (var key in this.ModelState.Keys)
                {
                    if (this.ModelState[key].Errors.Count != 0)
                    {
                        TempData["Erreur"] += "Erreur: " + this.ModelState[key].Errors[0].ErrorMessage + "<br/>";
                        TempData.Keep();
                    }
                }

                RemplirListeDeroulantesPaiement(paiement);
                return View("Facture", paiement);
            }
            //return RedirectToAction("Options", "Membres", new { id = paiement.noMembre });
        }

        [HttpPost]
        [Authorize]
        public ActionResult Facture(PaiementViewModel paiement)
        {
            double prixTotalCout = db.ForfaitPremiums.Where(f => f.nbMoisAbonnement == paiement.nbMois).Select(f => f.prixTotal).FirstOrDefault();
            double prixPourTaxes = prixTotalCout;

            if (paiement.noProvince != null)
            {
                paiement.provinceFacturation = db.Provinces.Where(p => p.noProvince == paiement.noProvince).FirstOrDefault();

                if (paiement.provinceFacturation == null)
                {
                    paiement.noProvince = null;
                }

                TryValidateModel(paiement);
            }

            if (ModelState.IsValid)
            {
                paiement.TPS = Math.Round(prixPourTaxes * 0.05, 2);

                if (paiement.provinceFacturation.nomProvince == "Québec")
                {
                    paiement.TVQ = Math.Round(prixPourTaxes * 0.09975, 2);
                }

                double prixTotal = (prixTotalCout + (paiement.TPS ?? 0.00) + (paiement.provinceFacturation.nomProvince == "Québec" ? (paiement.TVQ ?? 0.00) : 0));

                if(EffectuerPaiement(paiement, prixTotal, prixTotalCout))
                {
                    return RedirectToAction("Home", "Home");
                }
                else
                {
                    //To fix
                    RemplirListeDeroulantesPaiement(paiement);
                    ViewBag.disabled = false;
                    return View("Facture", paiement);
                }
            }
            else
            {
                foreach (var key in this.ModelState.Keys)
                {
                    if (this.ModelState[key].Errors.Count != 0)
                    {
                        TempData["Erreur"] += "Erreur: " + this.ModelState[key].Errors[0].ErrorMessage + "<br/>";
                        TempData.Keep();
                    }
                }

                RemplirListeDeroulantesPaiement(paiement);

                return View("InformationsPaiement", paiement);
            }
        }

        [NonAction]
        public bool EffectuerPaiement(PaiementViewModel paiement, double prixACharger, double prixAStocker)
        {
            //avant la transaction

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
            ServicePointManager.DefaultConnectionLimit = 9999;
            string retour = Utilitaires.doTransaction(paiement.prenomSurCarte + " " + paiement.nomSurCarte + ":" + paiement.noMembre,
                    paiement.noCarteCredit,
                    paiement.typeCarte,
                    String.Format("{0:00}", paiement.carteMoisExpiration) + paiement.carteAnneeExpiration,
                    paiement.codeVerification,
                    Math.Round(prixACharger, 2).ToString(CultureInfo.CreateSpecificCulture("en-US")),
                    paiement.prenomSurCarte,
                    paiement.nomSurCarte,
                    paiement.adresseFacturation,
                    paiement.villeFacturation,
                    paiement.provinceFacturation.nomProvince,
                    paiement.codePostalFacturation);

            //après la transaction
            if (retour == "true")
            {
                Membre leMembreDevenuPremium = db.Membres.Where(m => m.noMembre == paiement.noMembre).FirstOrDefault();
                if (leMembreDevenuPremium != null)
                {
                    leMembreDevenuPremium.premium = true;

                    Abonnement abonnementActuel = db.Abonnements
                                                    .Where(m => (m.noMembre == leMembreDevenuPremium.noMembre) &&
                                                                (m.dateDebut < DateTime.Now) &&
                                                                (m.dateFin > DateTime.Now)
                                                                ).FirstOrDefault();
                    
                    Abonnement leNouvelAbonnement = new Abonnement();
                    leNouvelAbonnement.datePaiement = DateTime.Now;
                    leNouvelAbonnement.coutTotal = prixAStocker;
                    leNouvelAbonnement.membre = leMembreDevenuPremium;
                    leNouvelAbonnement.typeAbonnement = paiement.nbMois;

                    //Info Carte
                    leNouvelAbonnement.prenomSurCarte = paiement.prenomSurCarte;
                    leNouvelAbonnement.nomSurCarte = paiement.nomSurCarte;
                    leNouvelAbonnement.quatreDerniersChiffres = paiement.noCarteCredit.ToString().Substring(paiement.noCarteCredit.ToString().Length - 4);
                    leNouvelAbonnement.typeCarte = paiement.typeCarte;
                    leNouvelAbonnement.adresseFacturation = paiement.adresseFacturation;
                    leNouvelAbonnement.villeFacturation = paiement.villeFacturation;
                    leNouvelAbonnement.codePostalFacturation = paiement.codePostalFacturation;
                    leNouvelAbonnement.noProvince = paiement.noProvince;
                    leNouvelAbonnement.provinceFacturation = db.Provinces.Where(m => m.noProvince == paiement.noProvince).FirstOrDefault();

                    if(paiement.TPS != null)
                    {
                        leNouvelAbonnement.prixTPS = (double) paiement.TPS;
                    }
                    
                    if(paiement.TVQ != null)
                    {
                        leNouvelAbonnement.prixTVQ = (double) paiement.TVQ;
                    }

                    //Va rester prix TPS/TVQ

                    if (abonnementActuel != null)
                    {
                        //On a renouveler
                        abonnementActuel.renouveler = true;

                        List<Abonnement> lesAbonnementsApres = db.Abonnements.Where(m => (m.noMembre == leMembreDevenuPremium.noMembre) &&
                                                                                          m.dateFin > abonnementActuel.dateFin)
                                                                                          .OrderBy(m => m.dateFin).ToList();

                        DateTime derniereAbonnementFinDate = abonnementActuel.dateFin;
                        foreach(Abonnement a in lesAbonnementsApres)
                        {
                            a.renouveler = true;
                            derniereAbonnementFinDate = a.dateFin;
                        }

                        leNouvelAbonnement.dateDebut = derniereAbonnementFinDate;
                        leNouvelAbonnement.dateFin = derniereAbonnementFinDate.AddMonths(paiement.nbMois);
                    }
                    else
                    {
                        //On laisse une semaine pour considérer ca comme "renouveller"
                        Abonnement abonnementDeMoinsDe1Semaine = db.Abonnements
                                                    .Where(m => (m.noMembre == leMembreDevenuPremium.noMembre) &&
                                                                (m.dateFin > DbFunctions.AddDays(DateTime.Now, -7))
                                                                ).FirstOrDefault();

                        if(abonnementDeMoinsDe1Semaine != null)
                        {
                            abonnementDeMoinsDe1Semaine.renouveler = true;
                        }
                        leNouvelAbonnement.dateDebut = DateTime.Now;
                        leNouvelAbonnement.dateFin = DateTime.Now.AddMonths(paiement.nbMois);
                    }
                    
                    db.Abonnements.Add(leNouvelAbonnement);

                    db.SaveChanges();

                    bool seSouvenirDeMoi = (Request.Cookies["SiteDeRencontre"]["seSouvenirDeMoi"] == "true" ? true : false);

                    HttpCookie myCookie = new HttpCookie("SiteDeRencontre");

                    myCookie["noMembre"] = leMembreDevenuPremium.noMembre.ToString();
                    myCookie["premium"] = leMembreDevenuPremium.premium ? "true" : "false";
                    myCookie["seSouvenirDeMoi"] = seSouvenirDeMoi ? "true" : "false";

                    if (seSouvenirDeMoi)
                    {
                        myCookie.Expires = new System.DateTime(2020, 1, 1);
                    }

                    Response.Cookies.Add(myCookie);

                    Utilitaires.envoieCourriel(
                    "Félicitation pour votre abonnement premium à Club Contact",
                    Utilitaires.RenderRazorViewToString(this, "ApresAbonnementCourriel", leNouvelAbonnement),
                    leMembreDevenuPremium.courriel
                    );
                }
            }
            else
            {
                TempData["MessagePaiementErreur"] = "Le paiement n'a pas fonctionné, veuillez vérifier vos informations et recommencer.";

                RemplirListeDeroulantesPaiement(paiement);

                return false;
            }

            return true;
        }

        #region ListeFavoris
        [HttpGet]
        [Authorize]
        public ActionResult ListeFavoris(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            try
            {
                Membre membre = db.Membres.Where(e => e.noMembre == id).Include(m => m.listeFavoris).FirstOrDefault();

                if (membre == null)
                {
                    return HttpNotFound();
                }

                return View("ListeFavoris", membre);
            }
            catch (Exception e)
            {
                Dictionary<string, string> parametres = new Dictionary<string, string>() {
                    { "id", id.ToString()  }
                };
                throw Utilitaires.templateException("ListeFavoris", "Membres", "Requête LINQ n'a pas fonctionnée ou la BD est inaccessible.", e, parametres, "get");
            }
        }

        [HttpPost]
        public ActionResult ListeFavoris(Membre membre, List<string> removeMembre)
        {
            try
            {
                membre = db.Membres.Where(m => m.noMembre == membre.noMembre).Include(m => m.listeFavoris).FirstOrDefault();
            }
            catch (Exception e)
            {
                Dictionary<string, string> parametres = new Dictionary<string, string>() {
                    { "noMembre", membre.noMembre.ToString()  }
                };
                throw Utilitaires.templateException("ListeFavoris", "Membres", "Requête LINQ n'a pas fonctionnée", e, parametres, "post");
            }

            if (membre == null)
            {
                return HttpNotFound();
            }

            db.Entry(membre).State = EntityState.Modified;

            //Membre mem = db.Membres.Where(e => e.noMembre == noMembre).Include(r => r.listeNoire).FirstOrDefault();
            //Membre rejete = db.Membres.Where(p => p.noMembre == noMembreRejete).FirstOrDefault();
            //mem.listeNoire.Remove(rejete);
            //db.SaveChanges();

            for (int i = 0; i < membre.listeFavoris.Count; i++)
            {
                if (removeMembre.Count > i && removeMembre[i] == "true")
                {
                    membre.listeFavoris.Remove(membre.listeFavoris[i]);

                    removeMembre.RemoveAt(i);
                    i--;
                }
            }

            try
            {
                db.SaveChanges();
            }
            catch (Exception e)
            {
                Dictionary<string, string> parametres = new Dictionary<string, string>() {
                    { "noMembre", membre.noMembre.ToString()  }
                };
                throw Utilitaires.templateException("ListeFavoris", "Membres", "Mise à jour de la BD n'a pas fonctionnée", e, parametres, "post");
            }

            TempData["Confirmation"] = "Votre liste de favoris a été mise à jour!";
            TempData.Keep();
            return RedirectToAction("ListeFavoris", "Membres", new { id = membre.noMembre });
        }

        [HttpGet]
        public ActionResult AjouterAListeFavoris(int? id, int? noMessageOrigine)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            int cookie;
            if (!verifierSiCookieNoMembreExiste(out cookie))
            {
                return RedirectToAction("Home", "Home");
            }

            try
            {
                Membre mCookie = db.Membres.Where(y => y.noMembre == cookie).FirstOrDefault();

                Membre m = db.Membres.Where(me => me.noMembre == id).FirstOrDefault();

                if (mCookie == null || m == null)
                {
                    return HttpNotFound();
                }

                mCookie.listeFavoris.Add(m);
            }
            catch (Exception e)
            {
                Dictionary<string, string> parametres = new Dictionary<string, string>() {
                    { "noMembreReceveur", id.ToString()  },
                    { "noMembreCookie", cookie.ToString() }
                };
                throw Utilitaires.templateException("AjouterAListeFavoris", "Membres", "Requête LINQ n'a pas fonctionnée ou la BD est inaccessible.", e, parametres, "get");
            }

            try
            {
                db.SaveChanges();
            }
            catch (Exception e)
            {
                Dictionary<string, string> parametres = new Dictionary<string, string>() {
                    { "noMembreReceveur", id.ToString()  },
                    { "noMembreCookie", cookie.ToString() }
                };
                throw Utilitaires.templateException("AjouterAListeFavoris", "Membres", "La mise à jour de la BD n'a pas fonctionnée", e, parametres, "get");

            }

            if (noMessageOrigine != null)
            {
                if (noMessageOrigine == -1)
                {
                    return Redirect(Request.UrlReferrer.ToString());
                }
                return RedirectToAction("InboxMessage", "Messagerie", new { id = noMessageOrigine });
            }

            return RedirectToAction("Details", "Membres", new { id = id });
        }

        public ActionResult SupprimerDeListeFavoris(int? id, int? noMessageOrigine)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            int cookie;
            if (!verifierSiCookieNoMembreExiste(out cookie))
            {
                return RedirectToAction("Home", "Home");
            }

            try
            {
                Membre mCookie = db.Membres.Where(y => y.noMembre == cookie).FirstOrDefault();

                Membre m = db.Membres.Where(me => me.noMembre == id).FirstOrDefault();

                if (mCookie == null || m == null)
                {
                    return HttpNotFound();
                }

                mCookie.listeFavoris.Remove(m);
            }
            catch (Exception e)
            {
                Dictionary<string, string> parametres = new Dictionary<string, string>() {
                    { "noMembreReceveur", id.ToString()  },
                    { "noMembreCookie", cookie.ToString() }
                };
                throw Utilitaires.templateException("SupprimerDeListeFavoris", "Membres", "Requête LINQ n'a pas fonctionnée ou la BD est inaccessible.", e, parametres, "get");
            }

            try
            {
                db.SaveChanges();
            }
            catch (Exception e)
            {
                Dictionary<string, string> parametres = new Dictionary<string, string>() {
                    { "noMembreReceveur", id.ToString()  },
                    { "noMembreCookie", cookie.ToString() }
                };
                throw Utilitaires.templateException("SupprimerDeListeFavoris", "Membres", "La mise à jour de la BD n'a pas fonctionnée", e, parametres, "get");

            }
            
            if(noMessageOrigine != null)
            {
                if (noMessageOrigine == -1)
                {
                    return Redirect(Request.UrlReferrer.ToString());
                }

                return RedirectToAction("InboxMessage", "Messagerie", new { id = noMessageOrigine });
            }

            return RedirectToAction("Details", "Membres", new { id = id });
        }
        #endregion

        #region ListeNoire
        [HttpGet]
        [Authorize]
        public ActionResult ListeNoire(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            try
            {
                Membre membre = db.Membres.Where(e => e.noMembre == id).Include(m => m.listeNoire).FirstOrDefault();

                if (membre == null)
                {
                    return HttpNotFound();
                }

                return View("ListeNoire", membre);
            }
            catch (Exception e)
            {
                Dictionary<string, string> parametres = new Dictionary<string, string>() {
                    { "id", id.ToString()  }
                };
                throw Utilitaires.templateException("ListeNoire", "Membres", "Requête LINQ n'a pas fonctionnée ou la BD est inaccessible.", e, parametres, "get");
            }  
        }

        [HttpPost]
        public ActionResult ListeNoire(Membre membre, List<string> removeMembre)
        {
            try
            {
                membre = db.Membres.Where(m => m.noMembre == membre.noMembre).Include(m => m.listeNoire).FirstOrDefault();
            }
            catch (Exception e)
            {
                Dictionary<string, string> parametres = new Dictionary<string, string>() {
                    { "noMembre", membre.noMembre.ToString()  }
                };
                throw Utilitaires.templateException("ListeNoire", "Membres", "Requête LINQ n'a pas fonctionnée", e, parametres, "post");
            }

            if (membre == null)
            {
                return HttpNotFound();
            }

            db.Entry(membre).State = EntityState.Modified;

            //Membre mem = db.Membres.Where(e => e.noMembre == noMembre).Include(r => r.listeNoire).FirstOrDefault();
            //Membre rejete = db.Membres.Where(p => p.noMembre == noMembreRejete).FirstOrDefault();
            //mem.listeNoire.Remove(rejete);
            //db.SaveChanges();

            for (int i = 0; i < membre.listeNoire.Count; i++)
            {
                if (removeMembre.Count > i && removeMembre[i] == "true")
                {
                    membre.listeNoire.Remove(membre.listeNoire[i]);

                    removeMembre.RemoveAt(i);
                    i--;
                }
            }

            try
            {
                db.SaveChanges();
            }
            catch (Exception e)
            {
                Dictionary<string, string> parametres = new Dictionary<string, string>() {
                    { "noMembre", membre.noMembre.ToString()  }
                };
                throw Utilitaires.templateException("ListeNoire", "Membres", "Mise à jour de la BD n'a pas fonctionnée", e, parametres, "post");
            }

            TempData["Confirmation"] = "Votre liste de personnes rejetées a été mise à jour!";
            TempData.Keep();
            return RedirectToAction("ListeNoire", "Membres", new { id = membre.noMembre });
        }

        [HttpGet]
        public ActionResult AjouterAListeNoire(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            int cookie;
            if(!verifierSiCookieNoMembreExiste(out cookie))
            {
                return RedirectToAction("Home", "Home");
            }

            try
            {
                Membre mCookie = db.Membres.Where(y => y.noMembre == cookie).Include(y => y.listeNoire).Include(y => y.listeFavoris).FirstOrDefault();

                Membre m = db.Membres.Where(me => me.noMembre == id).FirstOrDefault();

                if (mCookie == null || m == null)
                {
                    return HttpNotFound();
                }

                mCookie.listeNoire.Add(m);

                //Si le membre qu'il vient de mettre dans sa liste noire est aussi dans sa liste de favoris, on le retire
                if(mCookie.listeFavoris.Contains(m))
                {
                    mCookie.listeFavoris.Remove(m);
                }
            }
            catch (Exception e)
            {
                Dictionary<string, string> parametres = new Dictionary<string, string>() {
                    { "noMembreRejete", id.ToString()  },
                    { "noMembreCookie", cookie.ToString() }
                };
                throw Utilitaires.templateException("AjouterAListeNoire", "Membres", "Requête LINQ n'a pas fonctionnée ou la BD est inaccessible.", e, parametres, "get");
            }

            try
            {
                db.SaveChanges();
            }
            catch (Exception e)
            {
                Dictionary<string, string> parametres = new Dictionary<string, string>() {
                    { "noMembreRejete", id.ToString()  },
                    { "noMembreCookie", cookie.ToString() }
                };
                throw Utilitaires.templateException("AjouterAListeNoire", "Membres", "La mise à jour de la BD n'a pas fonctionnée", e, parametres, "get");

            }

            //Si on ajoute quelqu'un à sa liste noire, on retire les visites qu'ils nous a fait, comme ca on ne le voit plus s'il avait déjà fait des visites.
            return RedirectToAction("RetirerDeVisitePuisDetails", "Membres", new { id = id });
            //return RedirectToAction("Details", "Membres", new { id = id });
        }

        public ActionResult SupprimerDeListeNoire(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            int cookie;
            if (!verifierSiCookieNoMembreExiste(out cookie))
            {
                return RedirectToAction("Home", "Home");
            }

            try
            {
                Membre mCookie = db.Membres.Where(y => y.noMembre == cookie).FirstOrDefault();

                Membre m = db.Membres.Where(me => me.noMembre == id).FirstOrDefault();

                if (mCookie == null || m == null)
                {
                    return HttpNotFound();
                }

                mCookie.listeNoire.Remove(m);
            }
            catch (Exception e)
            {
                Dictionary<string, string> parametres = new Dictionary<string, string>() {
                    { "noMembreRejete", id.ToString()  },
                    { "noMembreCookie", cookie.ToString() }
                };
                throw Utilitaires.templateException("SupprimerDeListeNoire", "Membres", "Requête LINQ n'a pas fonctionnée ou la BD est inaccessible.", e, parametres, "get");
            }

            try
            {
                db.SaveChanges();
            }
            catch (Exception e)
            {
                Dictionary<string, string> parametres = new Dictionary<string, string>() {
                    { "noMembreRejete", id.ToString()  },
                    { "noMembreCookie", cookie.ToString() }
                };
                throw Utilitaires.templateException("SupprimerDeListeNoire", "Membres", "La mise à jour de la BD n'a pas fonctionnée", e, parametres, "get");

            }

            return RedirectToAction("Details", "Membres", new { id = id });
        }
        #endregion

        [HttpPost]
        public ActionResult Options(OptionsCompteViewModel options, string motDePasse1, string motDePasseOLD)
        {
            ViewBag.Title = "Options du compte";

            string modification = "";

            if(options.motDePasse != null)
            {
                modification = "MDP";

                ValiderMotDePasse(motDePasse1, motDePasseOLD, options.noMembre, false);
            }

            if(ModelState.IsValid)
            {
                try
                {
                    if(modification == "MDP")
                    {
                        Membre mem = db.Membres.Where(m => m.noMembre == options.noMembre).FirstOrDefault();
                        mem.motDePasse = options.motDePasse;

                        db.SaveChanges();

                        options.motDePasse = "";

                        TempData["confirmationMDPModifier"] = "Votre mot de passe a bien été modifié!";
                        TempData.Keep();
                    }
                }
                catch(Exception e)
                {
                    Dictionary<string, string> parametres = new Dictionary<string, string>() {
                    { "noMembre passé en paramètre dans le OptionsCompteViewModel", options.noMembre.ToString()  }
                    };
                    throw Utilitaires.templateException("Options", "Membres", "La requête LINQ lors de la mise à jour du mot de passe n'a pas fonctionnée.", e, parametres, "post");
                }

                return View("Options", options);
            }
            else
            {
                foreach (var key in this.ModelState.Keys)
                {
                    if (this.ModelState[key].Errors.Count != 0)
                    {
                        TempData["Erreur"] += "Erreur: " + this.ModelState[key].Errors[0].ErrorMessage + "<br/>";
                        TempData.Keep();
                    }
                }

                if(modification == "MDP")
                {
                    options.motDePasse = "";
                }
                return View("Options", options);
            }
        }

        [HttpPost]
        public ActionResult AjouteLignePhoto(int index, int noMembre, bool aCocher)
        {
            var newPhoto = new Photo();
            newPhoto.noMembre = noMembre;


            newPhoto.photoProfil = aCocher;
            /*if(index == 0)
            {
                newPhoto.photoProfil = true;
            }
            else
            {
                newPhoto.photoProfil = false;
            }*/

            ViewData.TemplateInfo.HtmlFieldPrefix = string.Format("listePhotosMembres[{0}]", index);
            ViewBag.i = index;

            newPhoto.nomFichierPhoto = noMembre + "$" + "00000.jpg";

            return PartialView("~\\Views\\Shared\\EditorTemplates\\Photo.cshtml", newPhoto);
        }   
    }
}