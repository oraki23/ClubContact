/*------------------------------------------------------------------------------------

CONTRÔLEUR POUR LA GESTION DES COMPTES UTILISATEURS POUR LES ADMINISTRATEUR
    CRÉATION
    MODIFICATION
    SUPPRESSION
    LISTE (INDEX)

--------------------------------------------------------------------------------------
Par: Anthony Brochu et Marie-Ève Massé
Novembre 2017
Club Contact
------------------------------------------------------------------------------------*/

using ProjetSiteDeRencontre.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

namespace ProjetSiteDeRencontre.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CompteAdminController : Controller
    {
        ClubContactContext db = new ClubContactContext();

        // GET: CompteAdmin
        public ActionResult Index()
        {
            List<CompteAdmin> lesComptesAdmin = db.CompteAdmins.ToList();

            return View(lesComptesAdmin);
        }

        public ActionResult Create()
        {
            CompteAdmin nouveauCompteAdmin = new CompteAdmin();
            nouveauCompteAdmin.permission = db.NiveauDePermissionAdmins.Where(m => m.nomNiveauDePermissionAdmin == "Limité").FirstOrDefault();

            ViewBagNecessaireAdmin(nouveauCompteAdmin);

            return View("Edit", nouveauCompteAdmin);
        }

        [HttpGet]
        public ActionResult Edit(int? id)
        {
            if(id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }

            CompteAdmin leCompteAdmin = db.CompteAdmins.Where(m => m.noCompteAdmin == id).FirstOrDefault();

            if(leCompteAdmin == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }

            ViewBagNecessaireAdmin(leCompteAdmin);

            return View("Edit", leCompteAdmin);
        }

        [HttpPost]
        public ActionResult Edit(CompteAdmin compteAdmin)
        {
            if(ModelState.IsValid)
            {
                if (compteAdmin.noCompteAdmin == 0)
                {
                    db.Entry(compteAdmin).State = EntityState.Added;
                }
                else
                {
                    if (compteAdmin.motDePasseHashe == "" || compteAdmin.motDePasseHashe == null)
                    {
                        compteAdmin.motDePasseHashe = db.CompteAdmins.Where(m => m.noCompteAdmin == compteAdmin.noCompteAdmin).Select(m => m.motDePasseHashe).FirstOrDefault();
                    }

                    db.Entry(compteAdmin).State = EntityState.Modified;
                }

                db.SaveChanges();
            }
            else
            {
                return View("Edit", compteAdmin);
            }

            return RedirectToAction("Index", "CompteAdmin");
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }

            CompteAdmin leCompteAdmin = db.CompteAdmins.Where(m => m.noCompteAdmin == id).FirstOrDefault();

            if (leCompteAdmin == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }

            ViewBag.disabled = true;
            ViewBagNecessaireAdmin(leCompteAdmin);

            return View("Edit", leCompteAdmin);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }

            CompteAdmin leCompteAdmin = db.CompteAdmins.Where(m => m.noCompteAdmin == id).FirstOrDefault();

            if (leCompteAdmin == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }

            ViewBagNecessaireAdmin(leCompteAdmin);

            return View(leCompteAdmin);
        }

        [HttpPost]
        public ActionResult DeleteConfirmed(CompteAdmin compteASupprimer)
        {
            CompteAdmin leCompteAdmin = db.CompteAdmins.Where(m => m.noCompteAdmin == compteASupprimer.noCompteAdmin).FirstOrDefault();

            if(leCompteAdmin == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }

            db.CompteAdmins.Remove(leCompteAdmin);

            db.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult ModifierMotDePasse(string id)
        {
            if(id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }

            CompteAdmin compteAdminCo = db.CompteAdmins.Where(m => m.nomCompte == id).FirstOrDefault();

            if(compteAdminCo == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }

            if(compteAdminCo.nomCompte != User.Identity.Name)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);
            }


            return View("ModifierMotDePasse", compteAdminCo);
        }

        [HttpPost]
        public ActionResult ModifierMotDePasse(CompteAdmin compteAdmin)
        {
            if (ModelState.IsValid)
            {
                if (compteAdmin.motDePasseHashe == "" || compteAdmin.motDePasseHashe == null)
                {
                    compteAdmin.motDePasseHashe = db.CompteAdmins.Where(m => m.noCompteAdmin == compteAdmin.noCompteAdmin).Select(m => m.motDePasseHashe).FirstOrDefault();
                }

                db.Entry(compteAdmin).State = EntityState.Modified;

                db.SaveChanges();
            }
            else
            {
                return View("Edit", compteAdmin);
            }

            TempData["motDePasseAdminSauvegarder"] = "Votre mot de passe a bien été sauvegardé.";
            TempData.Keep();

            return RedirectToAction("ModifierMotDePasse", "CompteAdmin");
        }

        [NonAction]
        public void ViewBagNecessaireAdmin(CompteAdmin compteAdmin)
        {
            if(compteAdmin.noCompteAdmin == 0)
            {
                ViewBag.Create = true;
            }
            else
            {
                ViewBag.Create = false;
            }

            ViewBag.niveauxPermission = new SelectList(db.NiveauDePermissionAdmins, "noNiveauDePermissionAdmin", "nomNiveauDePermissionAdmin", compteAdmin.noPermission);
        }
    }
}