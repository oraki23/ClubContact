/*------------------------------------------------------------------------------------

CONTRÔLEUR FAISANT TOUT CE QUI TOUCHE AUX ACTIVITÉS:
    CRÉATION
    MODIFICATION
    LISTE (INDEX)
    ..ETC

--------------------------------------------------------------------------------------
Par: Anthony Brochu et Marie-Ève Massé
Novembre 2017
Club Contact
------------------------------------------------------------------------------------*/

using ProjetSiteDeRencontre.LesUtilitaires;
using ProjetSiteDeRencontre.Models;
using ProjetSiteDeRencontre.ViewModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace ProjetSiteDeRencontre.Controllers
{
    public class ActivitesController : BaseController
    {
        private ClubContactContext db;

        public ActivitesController()
        {
            db = new ClubContactContext();
        }

        [HttpGet]
        public ActionResult Index(int? page, RechercheActiviteViewModel rechercheActivite, int? vientDePagination)
        {
            int nbItemsParPages = 5;
            if (vientDePagination != null && TempData["rechercheActiviteAncienne"] != null)
            {
                rechercheActivite = TempData["rechercheActiviteAncienne"] as RechercheActiviteViewModel;
            }
            else
            {
                page = null;
            }

            if (TempData["rechercheActivite"] != null)
            {
                rechercheActivite = TempData["rechercheActivite"] as RechercheActiviteViewModel;
            }
            if (rechercheActivite == null)
            {
                rechercheActivite = new RechercheActiviteViewModel();
            }

            //Par défaut, on met la recherche d'activité aux activités futurs
            if (rechercheActivite.activiteFuturs == null)
            {
                rechercheActivite.activiteFuturs = true;
            }

            try
            {
                int noMembreCo;
                verifierSiCookieNoMembreExiste(out noMembreCo);

                IQueryable<Activite> listActiviteTrouves = Utilitaires.RechercheActiviteAvecCriteres(noMembreCo, rechercheActivite);
                
                GestionPagination(page, nbItemsParPages, rechercheActivite.nbActivitesTrouvesTotal);

                rechercheActivite.resultatsActivite = listActiviteTrouves
                .Skip((int)(ViewBag.currentPage - 1) * nbItemsParPages)
                .Take(nbItemsParPages)
                .Include(a => a.listeAvisActivite)
                .Include(a => a.listePhotosActivites)
                .ToList();

                TempData["rechercheActiviteAncienne"] = rechercheActivite;

                if (noMembreCo != -1)
                {
                    Membre membreCo = db.Membres.Where(m => m.noMembre == noMembreCo).FirstOrDefault();
                    if(membreCo != null)
                    {
                     //Si le membre est connecté, et qu'il a déjà envoyé un commentaire, on va le rechercher, sinon on en crée un nouveau.
                        foreach (Activite a in rechercheActivite.resultatsActivite)
                        {
                            AvisActivite avisDuMembreCo = a.listeAvisActivite.Where(m => m.noMembreEnvoyeur == membreCo.noMembre && m.noActiviteAssocie == a.noActivite).FirstOrDefault();
                            if (avisDuMembreCo == null)
                            {
                                AvisActivite nouvelleAvisActivite = new AvisActivite();
                                nouvelleAvisActivite.membreEnvoyeur = membreCo;
                                nouvelleAvisActivite.noMembreEnvoyeur = membreCo.noMembre;
                                nouvelleAvisActivite.activiteAssocie = a;
                                nouvelleAvisActivite.noActiviteAssocie = a.noActivite;

                                a.listeAvisActivite.Add(nouvelleAvisActivite);
                            }
                        }
                    }
                }

                ViewBag.listeThemes = new SelectList(db.Themes, "noTheme", "theme", rechercheActivite.noTheme);

                return View("Index", rechercheActivite);
            }
            catch (Exception e)
            {
                Dictionary<string, string> parametres = new Dictionary<string, string>()
                    {
                        {"Page", page.ToString() }
                    };
                throw Utilitaires.templateException("Index", "Activites", "Requête LINQ pour aller chercher les activites des résultats de recherche n'a pas fonctionnée.", e, parametres, "get");
            }
        }

        [HttpPost]
        public ActionResult Index(int? page, RechercheActiviteViewModel rechercheActivite, string btnAjoutCommentaire,
            List<List<string>> deletePhotos, List<List<HttpPostedFileBase>> fichiersPhotos)
        {
            int noMembreCo;
            verifierSiCookieNoMembreExiste(out noMembreCo);

            if (rechercheActivite.dateRecherche != null)
            {
                if (((DateTime)rechercheActivite.dateRecherche).Date < DateTime.Now.Date)
                {
                    rechercheActivite.activiteFuturs = false;
                }
                else
                {
                    rechercheActivite.activiteFuturs = true;
                }
            }

            int noActiviteModifier;
            if(int.TryParse(btnAjoutCommentaire, out noActiviteModifier))
            {
                int indexActiviteBouton = rechercheActivite.resultatsActivite.FindIndex(a => a.noActivite == noActiviteModifier);

                int indexAvisMembre = rechercheActivite.resultatsActivite[indexActiviteBouton].listeAvisActivite.FindIndex(m => m.noMembreEnvoyeur == noMembreCo);

                if (rechercheActivite.resultatsActivite[indexActiviteBouton].listeAvisActivite[indexAvisMembre].noAvisActivite == 0)
                {
                    db.Entry(rechercheActivite.resultatsActivite[indexActiviteBouton].listeAvisActivite[indexAvisMembre]).State = EntityState.Added;
                }
                else
                {
                    db.Entry(rechercheActivite.resultatsActivite[indexActiviteBouton].listeAvisActivite[indexAvisMembre]).State = EntityState.Modified;
                }

                try
                {
                    db.SaveChanges();
                }
                catch(DbUpdateException e)
                {
                    Elmah.ErrorSignal.FromCurrentContext().Raise(Utilitaires.templateException("Index", "Activites", "Mise à jour de la base de données à la suite de l'ajout d'un commentaire à échoué.", e, null, "post"));
                    TempData["rechercheActivite"] = rechercheActivite;

                    TempData["messageErreur"] = "Une erreur est survenu lors de la sauvegarde de votre commentaire, veuillez réessayer.";
                    TempData.Keep();

                    return RedirectToAction("Index", "Activites", new { page = page });
                }
                
            }
            else if(btnAjoutCommentaire == "recherche")
            {
                page = null;
            }
            else if(btnAjoutCommentaire != null)
            {
                if (btnAjoutCommentaire.Contains("photosActivite"))
                {
                    string iString = btnAjoutCommentaire.Remove(0, 14);
                    int i;
                    if(!int.TryParse(iString, out i))
                    {
                        Elmah.ErrorSignal.FromCurrentContext().Raise(Utilitaires.templateException("Index", "Activites", "Parse du int lors de la sauvegarde des photos des activités n'a pas fonctionné, nom photosActivite a peut-être été modifié.", null, null, "post"));
                        TempData["rechercheActivite"] = rechercheActivite;

                        TempData["messageErreur"] = "Une erreur est survenu lors de la sauvegarde de vos photos, veuillez réessayer.";
                        TempData.Keep();

                        return RedirectToAction("Index", "Activites", new { page = page });
                    }

                    Activite activiteModifier = TrouverInformationsDActiviteEtAjouterPhotos(rechercheActivite.resultatsActivite[i]);

                    List<PhotosActivite> listeActivitesPhotos = activiteModifier.listePhotosActivites;

                    for(int y = 0; y < listeActivitesPhotos.Count; y++)
                    {
                        if (listeActivitesPhotos[y].noPhotoActivite == 0 && fichiersPhotos[i][y] == null)
                        {
                            db.Entry(listeActivitesPhotos[y]).State = EntityState.Detached;

                            listeActivitesPhotos.RemoveAt(y);
                            deletePhotos[i].RemoveAt(y);
                            fichiersPhotos[i].RemoveAt(y);
                            y--;
                        }
                        else if(listeActivitesPhotos[y].noPhotoActivite == 0)
                        {
                            db.Entry(listeActivitesPhotos[y]).State = EntityState.Added;
                        }
                    }

                    db.Entry(activiteModifier).State = EntityState.Modified;

                    //étape 2
                    for(int y = 0; y < listeActivitesPhotos.Count; y++)
                    {
                        //Si on veut supprimer la photo
                        if (deletePhotos[i].Count > y && deletePhotos[i][y] == "true")
                        {
                            if(listeActivitesPhotos[y].noPhotoActivite == 0)
                            {
                                listeActivitesPhotos.RemoveAt(y);
                                deletePhotos[i].RemoveAt(y);
                                fichiersPhotos[i].RemoveAt(y);
                                y--;
                            }
                            else
                            {
                                var path = Url.Content("~/Upload/PhotosActivites/" + listeActivitesPhotos[y].nomFichierPhoto);
                                var fullPath = Request.MapPath(path);

                                if(System.IO.File.Exists(fullPath))
                                {
                                    try
                                    {
                                        System.IO.File.Delete(fullPath);
                                    }
                                    catch(Exception e)
                                    {
                                        Elmah.ErrorSignal.FromCurrentContext().Raise(Utilitaires.templateException("Index", "Activites", "La suppression de la photo: " + fullPath + " ne s'est pas complété correctement.", e, null, "post"));
                                    }
                                }
                                else
                                {
                                    Elmah.ErrorSignal.FromCurrentContext().Raise(Utilitaires.templateException("Index", "Activites", "La suppression de la photo: " + fullPath + " ne s'est pas complété correctement car le fichier n'existe pas.", null, null, "post"));
                                }

                                //listeActivitesPhotos.Remove(listeActivitesPhotos[y]);
                                db.Entry(listeActivitesPhotos[y]).State = EntityState.Deleted;

                                deletePhotos[i].RemoveAt(y);
                                fichiersPhotos[i].RemoveAt(y);
                                y--;
                            }
                        }
                        else if(listeActivitesPhotos[y].noPhotoActivite == 0)
                        {
                            db.Entry(listeActivitesPhotos[y]).State = EntityState.Added;
                        }
                        else
                        {
                            db.Entry(listeActivitesPhotos[y]).State = EntityState.Modified;
                        }
                    }

                    try
                    {
                        db.SaveChanges();
                    }
                    catch(DbUpdateException e)
                    {
                        Elmah.ErrorSignal.FromCurrentContext().Raise(Utilitaires.templateException("Index", "Activites", "Mise à jour de la base de données au premier SaveChanges() des photos des avisActivites a échoué.", e, null, "post"));
                        TempData["rechercheActivite"] = rechercheActivite;

                        TempData["messageErreur"] = "Une erreur est survenu lors de la sauvegarde de vos photos, veuillez réessayer.";
                        TempData.Keep();

                        return RedirectToAction("Index", "Activites", new { page = page });
                    }

                    //Étape Gestion 3
                    for(int y = 0; y < listeActivitesPhotos.Count; y++)
                    {
                        if(fichiersPhotos[i][y] != null && !fichiersPhotos[i][y].FileName.ToString().ToLower().EndsWith(".jpg"))
                        {
                            TempData["messageErreur"] = fichiersPhotos[i][y].FileName.ToString() + " doit être de type jpg";

                            db.Entry(listeActivitesPhotos[y]).State = EntityState.Deleted;

                            deletePhotos[i].RemoveAt(y);
                            fichiersPhotos[i].RemoveAt(y);
                            y--;

                            break;
                        }
                        else if(fichiersPhotos[i][y] != null)
                        {
                            try
                            {
                                Membre membreCo = db.Membres.Where(m => m.noMembre == noMembreCo).FirstOrDefault();

                                string fname = Path.Combine(Server.MapPath("~/Upload/PhotosActivites/") + activiteModifier.noActivite + "$" + noMembreCo + "$" + string.Format("{0:00000}", listeActivitesPhotos[y].noPhotoActivite) + ".jpg");

                                fichiersPhotos[i][y].SaveAs(fname);

                                listeActivitesPhotos[y].nomFichierPhoto = activiteModifier.noActivite + "$" + noMembreCo + "$" + string.Format("{0:00000}", listeActivitesPhotos[y].noPhotoActivite) + ".jpg";
                            }
                            catch(Exception e)
                            {
                                Elmah.ErrorSignal.FromCurrentContext().Raise(Utilitaires.templateException("Index", "Activites", "Impossible de sauvegarder le fichier: " + fichiersPhotos[i][y].FileName.ToString() + ".", e, null, "post"));
                                this.ModelState.AddModelError("nomFichierPhoto", fichiersPhotos[i][y].FileName.ToString() + " incapable de sauvgarder.");
                            }
                        }
                        else
                        {
                            if(listeActivitesPhotos[y].nomFichierPhoto.Equals("00000.jpg"))
                            {
                                TempData["messageErreur"] = "Veuillez définir une photo pour la/les nouvelle(s) photo(s)";
                            }
                        }
                    }

                    try
                    {
                        db.SaveChanges();
                    }
                    catch (DbUpdateException e)
                    {
                        Elmah.ErrorSignal.FromCurrentContext().Raise(Utilitaires.templateException("Index", "Activites", "Mise à jour de la base de données pour la deuxième fois à la suite de l'ajout de photos a échoué.", e, null, "post"));
                        TempData["rechercheActivite"] = rechercheActivite;

                        TempData["messageErreur"] = "Une erreur est survenu lors de la sauvegarde de vos photos, veuillez réessayer.";
                        TempData.Keep();

                        return RedirectToAction("Index", "Activites", new { page = page });
                    }
                }
            }
            
            TempData["rechercheActivite"] = rechercheActivite;

            return RedirectToAction("Index", "Activites", new { page = page });
        }

        public ActionResult AjouterLignePhotoActivite(int i, int index, int noActivite)
        {
            int noMembreCo;
            if(!verifierSiCookieNoMembreExiste(out noMembreCo))
            {
                TempData["Message"] = "Veuillez vous identifier.";
                return RedirectToAction("Login", "Authentification");
            }
            Membre membreCo = db.Membres.Where(m => m.noMembre == noMembreCo).FirstOrDefault();

            Activite activite = db.Activites.Where(m => m.noActivite == noActivite).FirstOrDefault();

            if(membreCo != null && activite != null)
            {
                PhotosActivite newPhoto = new PhotosActivite();
                newPhoto.membreQuiPublie = membreCo;
                newPhoto.noMembreQuiPublie = noMembreCo;

                newPhoto.photoPrincipale = false;
                newPhoto.activite = activite;
                newPhoto.noActivite = noActivite;

                ViewData.TemplateInfo.HtmlFieldPrefix = "resultatsActivite[" + i + "].listePhotosActivites[" + index + "]";

                newPhoto.nomFichierPhoto = noActivite + "$" + noMembreCo + "$" + "00000.jpg";

                ViewBag.i = i;
                ViewBag.x = index;

                return PartialView("~\\Views\\Shared\\EditorTemplates\\PhotosActivite.cshtml", newPhoto);
            }
            else
            {
                TempData["messageErreur"] = "Une erreur s'est produite lors de l'ajout d'une nouvelle photo. Veuillez réessayer plus tard, ou essayez de vous déconnecter et de vous reconnecter, puis réessayez.";
                return null;
            }
        }

        [HttpGet]
        [Authorize]
        public ActionResult Create()
        {
            Activite activite = new Activite();

            activite.annulee = false;

            ViewBag.listeProvinces = new SelectList(db.Provinces, "noProvince", "nomProvince", activite.province);

            ViewBagNecessaireModifierActivite(activite);

            ViewBag.Create = true;

            DateTime? abonnementLePlusRecent = null;

            int noMembreCo;
            if(verifierSiCookieNoMembreExiste(out noMembreCo))
            {
                abonnementLePlusRecent = db.Abonnements.Where(m => m.noMembre == noMembreCo).OrderByDescending(m => m.dateFin).Select(m => m.dateFin).FirstOrDefault();
            }
            
            if (abonnementLePlusRecent != null)
            {
                ViewBag.finAbonnement = abonnementLePlusRecent;
            }

            return View("Edit", activite);
        }

        [HttpGet]
        [Authorize]
        public ActionResult Edit(int? id)
        {
            ViewBag.titre = "edit";

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Activite acti = db.Activites.Where(p => p.noActivite == id).FirstOrDefault();

            if(acti == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }

            int noMembreCo;
            verifierSiCookieNoMembreExiste(out noMembreCo);

            if (!User.IsInRole("Admin"))
            {
                if (acti.noMembreOrganisateur != noMembreCo)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                }
            }

            try
            {
                ViewBagNecessaireModifierActivite(acti);

                DateTime? abonnementLePlusRecent = db.Abonnements.Where(m => m.noMembre == noMembreCo).OrderByDescending(m => m.dateFin).Select(m => m.dateFin).FirstOrDefault();

                if(abonnementLePlusRecent != null)
                {
                    ViewBag.finAbonnement = abonnementLePlusRecent;
                }
                return View("Edit", acti);
            }
            catch (Exception e)
            {
                //On log l'erreur, mais on peut tout de même continuer
                Dictionary<string, string> parametres = new Dictionary<string, string>() {
                    { "id", (id == null ? "null" : id.ToString()) }
                };
                Elmah.ErrorSignal.FromCurrentContext().Raise(Utilitaires.templateException("Edit", "Activites", "Requête LINQ n'a pas fonctionnée.", e, null, "get"));

                return View("Edit", acti);
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult Edit(Activite activite, HttpPostedFileBase fichierPhotos)
        {
            int noMembreCo;
            if(!verifierSiCookieNoMembreExiste(out noMembreCo))
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            Membre leMembreCo = db.Membres.Where(m => m.noMembre == noMembreCo).FirstOrDefault();

            if(leMembreCo == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            ViewBag.titre = "Edit";
            bool nouvelleActivite = (activite.noActivite == 0 ? true : false);

            bool nouvellePhoto;
            PhotosActivite laPhotoPrincipale = activite.listePhotosActivites.Where(p => p.photoPrincipale == true && p.noActivite == activite.noActivite).FirstOrDefault();
            if (fichierPhotos != null)
            {
                if (laPhotoPrincipale != null)
                {
                    nouvellePhoto = false;
                }
                else
                {
                    nouvellePhoto = true;
                }
            }
            else
            {
                nouvellePhoto = false;
            }

            DateTime? abonnementLePlusRecent = db.Abonnements.Where(m => m.noMembre == noMembreCo).OrderByDescending(m => m.dateFin).Select(m => m.dateFin).FirstOrDefault();

            if (abonnementLePlusRecent != null)
            {
                ViewBag.finAbonnement = abonnementLePlusRecent;
            }

            if (activite.date == null || activite.date < DateTime.Now)
            {
                this.ModelState.AddModelError("date", "Veuillez choisir une date dans le futur.");
            }
            else if (abonnementLePlusRecent != null)
            {
                if(activite.date >= abonnementLePlusRecent)
                {
                    this.ModelState.AddModelError("date", "Veuillez choisir une date avant que votre abonnement se termine.");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (nouvelleActivite == true)
                    {
                        db.Entry(activite).State = EntityState.Added;
                    }
                    else
                    {
                        db.Entry(activite).State = EntityState.Modified;
                    }

                    activite.membreOrganisateur = leMembreCo;
                    activite.noMembreOrganisateur = leMembreCo.noMembre;

                    //db.SaveChanges();

                    if(nouvellePhoto)
                    {
                        laPhotoPrincipale = new PhotosActivite();
                        //laPhotoPrincipale.noActivite = activite.noActivite;
                        laPhotoPrincipale.activite = activite;
                        laPhotoPrincipale.noMembreQuiPublie = leMembreCo.noMembre;
                        laPhotoPrincipale.membreQuiPublie = leMembreCo;
                        laPhotoPrincipale.photoPrincipale = true;

                        db.PhotosActivites.Add(laPhotoPrincipale);
                    }

                    if (fichierPhotos != null && !fichierPhotos.FileName.ToString().ToLower().EndsWith(".jpg"))
                    {
                        this.ModelState.AddModelError("nomFichierPhoto", fichierPhotos.FileName.ToString() + " doit être de type jpg");
                    }
                    else if (fichierPhotos != null)
                    {
                        db.SaveChanges();

                        try
                        {
                            string nomFichierNouvellePhoto = activite.noActivite + "$" + leMembreCo.noMembre + "$" + string.Format("{0:00000}", laPhotoPrincipale.noPhotoActivite) + ".jpg";

                            string fname = Path.Combine(Server.MapPath("~/Upload/PhotosActivites/") + nomFichierNouvellePhoto);
                            fichierPhotos.SaveAs(fname);

                            laPhotoPrincipale.nomFichierPhoto = nomFichierNouvellePhoto;
                        }
                        catch (Exception e)
                        {
                            this.ModelState.AddModelError("nomFichierPhoto", fichierPhotos.FileName.ToString() + " incapable de sauvgarder.");
                        }
                    }

                    if(ModelState.IsValid)
                    {
                        db.SaveChanges();
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

                        ViewBagNecessaireModifierActivite(activite);

                        if (!nouvelleActivite)
                        {
                            ViewBag.titre = "edit";
                        }

                        return View("Edit", activite);
                    }

                }
                catch (DbEntityValidationException ex)
                {
                    var error = ex.EntityValidationErrors.First().ValidationErrors.First();
                    this.ModelState.AddModelError(error.PropertyName, error.ErrorMessage);

                    ViewBagNecessaireModifierActivite(activite);

                    if (!nouvelleActivite)
                    {
                        ViewBag.titre = "edit";
                    }

                    return View("Edit", activite);
                }
                catch (DbUpdateException e)
                {
                    Elmah.ErrorSignal.FromCurrentContext().Raise(Utilitaires.templateException("Edit", "Activites", "Mise à jour de la base de données à la suite de la modification d'un activité a échoué.", e, null, "post"));

                    TempData["Erreur"] = "Une erreur est survenu lors de la sauvegarde de votre activité, veuillez réessayer.";
                    TempData.Keep();

                    ViewBagNecessaireModifierActivite(activite);

                    if(!nouvelleActivite)
                    {
                        ViewBag.titre = "edit";
                    }

                    return View("Edit", activite);
                }
            }
            else
            {
                foreach (var key in this.ModelState.Keys)
                {
                    if (this.ModelState[key].Errors.Count != 0)
                    {
                        TempData["Erreur"] += "Erreur: " + this.ModelState[key].Errors[0].ErrorMessage;
                        TempData.Keep();
                    }
                }

                ViewBagNecessaireModifierActivite(activite);

                if (!nouvelleActivite)
                {
                    ViewBag.titre = "edit";
                }

                return View("Edit", activite);
            }

            if (!nouvelleActivite)
            {
                Activite lActivite = db.Activites.Where(a => a.noActivite == activite.noActivite).Include(a => a.membresParticipants).FirstOrDefault();

                if(lActivite != null)
                {
                    foreach(Membre m in lActivite.membresParticipants)
                    {
                        LesUtilitaires.Utilitaires.envoieCourriel(
                    "Changement dans une des activités à laquelle vous participez - Club Contact",
                    LesUtilitaires.Utilitaires.RenderRazorViewToString(this, "NotificationActivite", lActivite),
                    m.courriel
                    );
                    }
                }
            }
            
            return RedirectToAction("Index", "Activites");
        }

        [Authorize]
        public ActionResult Annulation(int? id)
        {
            ViewBag.Title = "Supprimer mon activité";

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Activite acti;

            if (!User.IsInRole("Admin") && Request.IsAuthenticated)
            {
                string resultat = Utilitaires.AnnulerActivite((int)id, int.Parse(Request.Cookies["SiteDeRencontre"]["noMembre"]), out acti);

                if (resultat == "Activité non trouvée.")
                {
                    return new HttpStatusCodeResult(HttpStatusCode.NotFound);
                }
                else if (resultat == "Interdit")
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                }
                else if (resultat == "Reussi")
                {
                    foreach (Membre m in acti.membresParticipants)
                    {
                        LesUtilitaires.Utilitaires.envoieCourriel(
                            "Annulation d'une des activités à laquelle vous participiez! - Club Contact",
                            LesUtilitaires.Utilitaires.RenderRazorViewToString(this, "NotificationActivite", acti),
                            m.courriel
                            );
                    }
                }
                else
                {
                    return new HttpStatusCodeResult(HttpStatusCode.NotFound);
                }
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            return RedirectToAction("Index", "Activites");
        }

        public string ParticiperActivite(int noActivite)
        {
            string etaitDejaParticipant;

            int noMembreCo;
            verifierSiCookieNoMembreExiste(out noMembreCo);

            if (noMembreCo != -1)
            {
                etaitDejaParticipant = Utilitaires.ParticiperActivite(noActivite, noMembreCo);

                return etaitDejaParticipant;
            }

            return "NonConnecte";
        }

        [NonAction]
        public Activite TrouverInformationsDActiviteEtAjouterPhotos(Activite activiteAvecPhotos)
        {
            int noActivite = activiteAvecPhotos.noActivite;

            Activite activiteModifierDepuisDB = db.Activites.Where(m => m.noActivite == noActivite).Include(m => m.listePhotosActivites).AsNoTracking().FirstOrDefault();

            if(activiteModifierDepuisDB == null)
            {
                throw Utilitaires.templateException("TrouverInformationDActiviteEtAjouterPhotos", "Activites", "activiteModifierDepuisDB est null.");
            }

            activiteModifierDepuisDB.listePhotosActivites = activiteAvecPhotos.listePhotosActivites;

            activiteAvecPhotos = activiteModifierDepuisDB;

            return activiteAvecPhotos;
        }

        [NonAction]
        public void ViewBagNecessaireModifierActivite(Activite activite)
        {
            try
            {
                ViewBag.listeProvinces = new SelectList(db.Provinces.OrderBy(p => p.nomProvince), "noProvince", "nomProvince", activite.province);

                ViewBag.sexe = new List<SelectListItem>
                {
                    new SelectListItem { Text = "Des femmes", Value = false.ToString() },
                    new SelectListItem { Text = "Des hommes", Value = true.ToString() },
                };

                ViewBag.listeThemes = new SelectList(db.Themes, "noTheme", "theme", activite.theme);

            }
            catch(Exception e)
            {
                throw Utilitaires.templateException("ViewBagNecessaireModifierActivite", "Activites", "Requête LINQ pour remplir les ViewBag n'ont pas fonctionnés.");
            }
        }
    }
}