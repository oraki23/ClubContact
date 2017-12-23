/*------------------------------------------------------------------------------------

CONTRÔLEUR GESTION LA PAGE D'ACCUEIL ET LA PAGE FAQ DU SITE
NOTAMMENT LA PRÉINSCRIPTION DANS LA PAGE D'ACCUEIL

--------------------------------------------------------------------------------------
Par: Anthony Brochu et Marie-Ève Massé
Novembre 2017
Club Contact
------------------------------------------------------------------------------------*/

using ProjetSiteDeRencontre.Models;
using ProjetSiteDeRencontre.ViewModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProjetSiteDeRencontre.Controllers
{
    public class HomeController : BaseController
    {
        private ClubContactContext db;

        public HomeController()
        {
            db = new ClubContactContext();
        }

        [AllowAnonymous]
        public ActionResult Home()
        {
            PreInscription preInscription = new PreInscription();

            List<Membre> selectionDuMoisPremiumRecent = db.Abonnements.Where(m => m.membre.dateSuppressionDuCompte == null && m.membre.compteSupprimeParAdmin == null)
                                            .OrderByDescending(m => m.datePaiement)
                                            .DistinctBy(m => m.noMembre)
                                            .Select(m => m.membre)
                                            .Take(6)
                                            .ToList();

            List<int> listeNoMembre = selectionDuMoisPremiumRecent.Select(m => m.noMembre).ToList();

            List<Membre> selectionDuMoisNouveauxMembres = db.Membres.Where(m => 
                                                    !listeNoMembre.Contains(m.noMembre) &&
                                                    (m.dateSuppressionDuCompte == null && m.compteSupprimeParAdmin == null)
                                                )
                                            .OrderByDescending(m => m.dateInscription)
                                            .Take(6)
                                            .ToList();

            selectionDuMoisNouveauxMembres.AddRange(selectionDuMoisPremiumRecent);

            ViewBag.selectionDuMois = selectionDuMoisNouveauxMembres;

            if (Request.IsAuthenticated)
            {
                int no;
                verifierSiCookieNoMembreExiste(out no);
                if(no != -1)
                {
                    Membre membreConnnecte = db.Membres.Where(p => p.noMembre == no).FirstOrDefault();

                    /*List<Visite> cinqDernieresVisites = db.Visites.Where(m => m.noMembreVisite == no)
                        .OrderBy(m => m.dateVisite)
                        .DistinctBy(m => m.noMembreVisiteur)
                        .Take(5).ToList();

                    //Méthode assez peu conventionnelle, mais comme on ne sauvegardera pas les données, 
                    //on peut se permettre de modifier les cartes ;)
                    membreConnnecte.listeDeVisitesDeMonProfil = cinqDernieresVisites;*/

                    ViewBag.premium = membreConnnecte.premium;

                    return View("AccueilMembres", membreConnnecte);
                }
            }
            ViewBag.sexe = new SelectList(new SelectListItem[]
                {
                    new SelectListItem { Text = "Une femme", Value = false.ToString() },
                    new SelectListItem { Text = "Un homme", Value = true.ToString() },
                }, "Value", "Text");

            ViewBag.raisonsSurSite = new SelectList(db.RaisonsSurSites, "noRaisonSurSite", "raison");
            ViewBag.nbRaisonsSurSite = db.RaisonsSurSites.ToList().Count;

            RaisonsSurSite sansButPrecis = db.RaisonsSurSites.Where(r => r.raison == "Sans but précis").FirstOrDefault();
            if (sansButPrecis != null)
            {
                ViewBag.noRaisonSansButPrecis = sansButPrecis.noRaisonSurSite;
            }
            return View("AccueilVisiteurs", preInscription);
        }

        public ActionResult FAQ()
        {
            return View("FAQ");
        }
    }
}