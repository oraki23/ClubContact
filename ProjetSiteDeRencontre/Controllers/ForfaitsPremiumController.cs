/*------------------------------------------------------------------------------------

CONTRÔLEUR QUI SERT POUR LA GESTION DES FORFAITS PREMIUM DONT:
    MODIFICATION
    LISTE

À NOTER QU'À CAUSE DE L'IMPLÉMENTATION ACTUELLE, IL N'EST PAS POSSIBLE DE:
    CRÉER
    SUPPRIMER
DES FORFAITS.

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
using System.Web.Mvc;
using System.Data.Entity;

namespace ProjetSiteDeRencontre.Controllers
{
    public class ForfaitsPremiumController : Controller
    {
        ClubContactContext db = new ClubContactContext();

        // GET: ForfaitsPremium
        [HttpGet]
        public ActionResult Index(List<ForfaitPremium> lesForfaits)
        {
            if(lesForfaits == null)
            {
                lesForfaits = db.ForfaitPremiums.ToList();
            }

            return View(lesForfaits);
        }

        [HttpPost]
        public ActionResult Index(List<ForfaitPremium> forfaitsPremium, int? test)
        {
            if(ModelState.IsValid)
            {
                foreach (ForfaitPremium fp in forfaitsPremium)
                {
                    db.Entry(fp).State = EntityState.Modified;
                }

                db.SaveChanges();

                TempData["messageReussiteSauvegarde"] = "Sauvegarde réussi!";
                TempData.Keep();
            }
            
            return View(forfaitsPremium);
        }
    }
}