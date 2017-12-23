/*------------------------------------------------------------------------------------

CONTRÔLEUR POUR TOUT CE QUI ATTRAIT À LA GESTION PAR L'ADMINISTRATEUR.
À NOTER QUE LES FONCTIONNALITÉS SUIVANTES:
    FORFAITSPREMIUM
    COMPTESADMIN    
..SONT DANS UN AUTRE CONTRÔLEUR

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
using ProjetSiteDeRencontre.LesUtilitaires;

namespace ProjetSiteDeRencontre.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : BaseController
    {
        ClubContactContext db = new ClubContactContext();
        // GET: Admin
        [HttpGet]
        public ActionResult Gestion(int? page, GestionViewModel gestionViewModel, int? tab, int? vientDePagination)
        {
            if (TempData["gestionViewModelPOST"] != null)
            {
                gestionViewModel = TempData["gestionViewModelPOST"] as GestionViewModel;
            }

            if (vientDePagination != null)
            {
                if(TempData["gestionViewModelAncien"] != null)
                {
                    gestionViewModel = TempData["gestionViewModelAncien"] as GestionViewModel;
                }
            }
            //Si on a fait un submit ou une chose autre que la pagination, on remet les pages à 0!
            else
            {
                page = null;
            }

            //On crée le gestion ViewModel s'il n'existe pas
            if (gestionViewModel == null)
            {
                gestionViewModel = new GestionViewModel();
            }

            if (tab != null)
            {
                gestionViewModel.noTabSelected = (int)tab;
            }
            else if (gestionViewModel.noTabSelected == null)
            {
                gestionViewModel.noTabSelected = 1;
            }

            //Membres
            if (gestionViewModel.noTabSelected == 1)
            {
                int nbMembresParPage = 50;

                /*(m.compteSupprimeParAdmin == null && m.dateSuppressionDuCompte == null)*/
                IQueryable<Membre> listeDeMembres = Statistiques.ListeMembres(gestionViewModel.membresPremiumUniquement, gestionViewModel.ageMin, gestionViewModel.ageMax, gestionViewModel.noProvince, gestionViewModel.noVille, gestionViewModel.afficherMembresDesactiver);

                gestionViewModel.nbMembresTotalTrouves = listeDeMembres.Count();

                GestionPagination(page, nbMembresParPage, (int)gestionViewModel.nbMembresTotalTrouves);

                ViewBag.premiumUniquement = new List<SelectListItem>
                {
                    new SelectListItem { Text = "Premium", Value = true.ToString() },
                    new SelectListItem { Text = "Gratuits", Value = false.ToString() },
                };

                if (gestionViewModel.triMembrePar == "age")
                {
                    listeDeMembres = listeDeMembres.OrderByDescending(m => m.dateNaissance).ThenBy(m => m.noMembre);
                }
                else if(gestionViewModel.triMembrePar == "sexe")
                {
                    listeDeMembres = listeDeMembres.OrderByDescending(m => m.homme).ThenBy(m => m.noMembre);
                }
                else if(gestionViewModel.triMembrePar == "dateInscription")
                {
                    listeDeMembres = listeDeMembres.OrderByDescending(m => m.dateInscription).ThenBy(m => m.noMembre);
                }
                else if(gestionViewModel.triMembrePar == "derniereConnexion")
                {
                    listeDeMembres = listeDeMembres.OrderByDescending(m => m.listeConnexions.OrderByDescending(x => x.dateConnexion).Select(x => (DateTime?)x.dateConnexion).FirstOrDefault()

                        ).ThenBy(m => m.noMembre);
                }
                else
                {
                    listeDeMembres = listeDeMembres.OrderBy(m => m.noMembre);
                }

                ViewBag.province = new SelectList(db.Provinces, "noProvince", "nomProvince", gestionViewModel.noProvince);

                gestionViewModel.lesMembres = listeDeMembres
                    .Skip((int)(ViewBag.currentPage - 1) * nbMembresParPage)
                .Take(nbMembresParPage)
                .Include(a => a.listeRaisonsSurSite)
                .Include(a => a.listeConnexions)
                .ToList();
            }
            //Signalement
            else if (gestionViewModel.noTabSelected == 2)
            {
                List<Signalement> signalementsAChangerDEtat = db.Signalements.Where(m => m.etatSignalementActuel.nomEtatSignalement == "En suivi" &&
                                                                                         m.dateSuiviNecessaire <= DateTime.Now).ToList();
                if(signalementsAChangerDEtat.Count > 0)
                {
                    foreach(Signalement s in signalementsAChangerDEtat)
                    {
                        s.etatSignalementActuel = db.EtatSignalements.Where(m => m.nomEtatSignalement == "À traiter").FirstOrDefault();
                    }
                    db.SaveChanges();
                }

                int nbSignalementParPage = 50;

                IQueryable<Signalement> listeSignalement = Statistiques.ListeSignalement(gestionViewModel.noEtatSignalementRecherche);

                gestionViewModel.nbSignalementTotalTrouves = listeSignalement.Count();

                GestionPagination(page, nbSignalementParPage, (int)gestionViewModel.nbSignalementTotalTrouves);

                /*ViewBag.premiumUniquement = new List<SelectListItem>
                {
                    new SelectListItem { Text = "Premium", Value = true.ToString() },
                    new SelectListItem { Text = "Gratuits", Value = false.ToString() },
                };*/

                ViewBag.listeEtatSignalementPlainte = db.EtatSignalements;

                gestionViewModel.lesSignalement = listeSignalement.OrderBy(m => m.noEtatSignalementActuel).ThenByDescending(m => m.dateSignalement).ThenBy(m => m.noSignalement)
                    .Skip((int)(ViewBag.currentPage - 1) * nbSignalementParPage)
                .Take(nbSignalementParPage)
                .Include(m => m.lesCommentairesSurCeSignalement)
                .ToList();

                foreach(Signalement s in gestionViewModel.lesSignalement)
                {
                    CommentaireSignalement nouveauCommentaire = new CommentaireSignalement();
                    nouveauCommentaire.compteAdminEnvoyeur = db.CompteAdmins.Where(m => m.nomCompte == User.Identity.Name).FirstOrDefault();
                    nouveauCommentaire.dateCommentaire = DateTime.MinValue;
                    nouveauCommentaire.noCompteAdminEnvoyeur = nouveauCommentaire.compteAdminEnvoyeur.noCompteAdmin;
                    nouveauCommentaire.signalementLie = s;
                    nouveauCommentaire.noSignalementLie = s.noSignalement;

                    s.lesCommentairesSurCeSignalement.Add(nouveauCommentaire);
                }

                gestionViewModel.nbSignalementTotalNouveau = Statistiques.NbSignalementsRecu(false, true);
                gestionViewModel.nbSignalementTotalRecuSite = Statistiques.NbSignalementsRecu(false, false);
                gestionViewModel.nbSignalementTotalTraites = Statistiques.NbSignalementsRecu(true, false);
            }
            //Abonnement
            else
            {
                int nbAbonnementsParPage = 25;

                gestionViewModel.nbAbonnements1Mois = Statistiques.NbAbonnement(1, gestionViewModel.anneeDebut, gestionViewModel.moisDebut, gestionViewModel.jourDebut,
                                                                                    gestionViewModel.anneeFin, gestionViewModel.moisFin, gestionViewModel.jourFin);
                gestionViewModel.nbAbonnements6Mois = Statistiques.NbAbonnement(6, gestionViewModel.anneeDebut, gestionViewModel.moisDebut, gestionViewModel.jourDebut,
                                                                                    gestionViewModel.anneeFin, gestionViewModel.moisFin, gestionViewModel.jourFin);
                gestionViewModel.nbAbonnements12Mois = Statistiques.NbAbonnement(12, gestionViewModel.anneeDebut, gestionViewModel.moisDebut, gestionViewModel.jourDebut,
                                                                                    gestionViewModel.anneeFin, gestionViewModel.moisFin, gestionViewModel.jourFin);

                gestionViewModel.nbDesabonnementTotal = Statistiques.NbDesabonnement(gestionViewModel.anneeDebut, gestionViewModel.moisDebut, gestionViewModel.jourDebut,
                                                                                    gestionViewModel.anneeFin, gestionViewModel.moisFin, gestionViewModel.jourFin);

                gestionViewModel.revenuTotal = Statistiques.Revenu(null, gestionViewModel.anneeDebut, gestionViewModel.moisDebut, gestionViewModel.jourDebut,
                                                                                    gestionViewModel.anneeFin, gestionViewModel.moisFin, gestionViewModel.jourFin);

                gestionViewModel.TPSTotal = Statistiques.RevenuTPS(null, gestionViewModel.anneeDebut, gestionViewModel.moisDebut, gestionViewModel.jourDebut,
                                                                                    gestionViewModel.anneeFin, gestionViewModel.moisFin, gestionViewModel.jourFin);

                gestionViewModel.TVQTotal = Statistiques.RevenuTVQ(null, gestionViewModel.anneeDebut, gestionViewModel.moisDebut, gestionViewModel.jourDebut,
                                                                                    gestionViewModel.anneeFin, gestionViewModel.moisFin, gestionViewModel.jourFin);

                IQueryable<Abonnement> listeDesAbonnementQuery = Statistiques.LesAbonnements(gestionViewModel.anneeDebut, gestionViewModel.moisDebut, gestionViewModel.jourDebut,
                                                                                    gestionViewModel.anneeFin, gestionViewModel.moisFin, gestionViewModel.jourFin);

                GestionPagination(page, nbAbonnementsParPage, listeDesAbonnementQuery.Count());

                gestionViewModel.listeDesAbonnements = listeDesAbonnementQuery.OrderByDescending(m => m.datePaiement).ThenBy(m => m.noAbonnement)
                    .Skip((int)(ViewBag.currentPage - 1) * nbAbonnementsParPage)
                .Take(nbAbonnementsParPage)
                .Include(m => m.provinceFacturation)
                .ToList();

                int? anneeLaPlusLoin = db.Abonnements.OrderBy(m => m.datePaiement).Select(m => m.datePaiement).FirstOrDefault().Year;
                int? anneeLaPlusProche = db.Abonnements.OrderByDescending(m => m.datePaiement).Select(m => m.datePaiement).FirstOrDefault().Year;

                setupViewBagMoisEtAnnee(gestionViewModel.moisDebut, gestionViewModel.moisFin, gestionViewModel.anneeDebut, gestionViewModel.anneeFin, anneeLaPlusLoin, anneeLaPlusProche);
            }

            TempData["gestionViewModelAncien"] = gestionViewModel;

            return View("Gestion", gestionViewModel);
        }

        [HttpPost]
        public ActionResult Gestion(int? page, GestionViewModel gestionViewModel, int? tab, int? vientDePagination, int? noMembreDesactiver, int? noSignalementLie, string btnSubmit)
        {
            if (noMembreDesactiver != null)
            {
                return RedirectToAction("DesactiverCompte", new { noMembreDesactiver = noMembreDesactiver, noSignalement = noSignalementLie });
            }
            else if(btnSubmit != null)
            {               
                if (btnSubmit.Contains("ChangerEtatPlainte"))
                {
                    int noSignalement = int.Parse(btnSubmit.Remove(0, 18));
                    int indexSignalement = gestionViewModel.lesSignalement.FindIndex(a => a.noSignalement == noSignalement);

                    CompteAdmin adminQuiTraite = db.CompteAdmins.Where(a => a.nomCompte == User.Identity.Name.ToString()).FirstOrDefault();

                    int noSignalementActuel = gestionViewModel.lesSignalement[indexSignalement].noEtatSignalementActuel;

                    if (db.EtatSignalements.Where(m => m.noEtatSignalement == noSignalementActuel).FirstOrDefault().nomEtatSignalement == "Assigné")
                    {
                        gestionViewModel.lesSignalement[indexSignalement].adminQuiTraite = adminQuiTraite;
                        gestionViewModel.lesSignalement[indexSignalement].noCompteAdmin = adminQuiTraite.noCompteAdmin;
                    }
                    else if(db.EtatSignalements.Where(m => m.noEtatSignalement == noSignalementActuel).FirstOrDefault().nomEtatSignalement == "Traité")
                    {
                        int noMembreContreQuiEstPlainte = gestionViewModel.lesSignalement[indexSignalement].noMembreContreQuiEstPlainte;
                        int noMembreQuiSePlaint = gestionViewModel.lesSignalement[indexSignalement].noMembreFaisantPlainte;
                        Membre leMembreContreQuiEstPlainte = db.Membres.Where(m => m.noMembre == noMembreContreQuiEstPlainte).FirstOrDefault();
                        Membre leMembreQuiSePlaint = db.Membres.Where(m => m.noMembre == noMembreQuiSePlaint).FirstOrDefault();

                        //Envoie d'un message automatisé de traitement
                        Message messageAccuseApresTraitement = new Message();
                        messageAccuseApresTraitement.dateEnvoi = DateTime.Now;
                        messageAccuseApresTraitement.sujetMessage = "Votre signalement contre " + leMembreContreQuiEstPlainte.surnom + " a été traité.";
                        messageAccuseApresTraitement.contenuMessage = "Votre signalement contre " + leMembreContreQuiEstPlainte.surnom + " a bien été traité, et une action a eu lieu afin que le comportement" +
                                " fautif ne se reproduise plus. Merci d'aider à rendre Club Contact un environnement sécuritaire!" + Environment.NewLine + Environment.NewLine + "- Le service d'administration de Club Contact";
                        messageAccuseApresTraitement.lu = false;
                        messageAccuseApresTraitement.membreReceveur = leMembreQuiSePlaint;
                        messageAccuseApresTraitement.noMembreReceveur = leMembreQuiSePlaint.noMembre;

                        db.Messages.Add(messageAccuseApresTraitement);
                        db.SaveChanges();
                    }
                    int noEtatSignalementActuel = gestionViewModel.lesSignalement[indexSignalement].noEtatSignalementActuel;
                    string nomEtatSignalementActuel = db.EtatSignalements.Where(m => m.noEtatSignalement == noEtatSignalementActuel).Select(m => m.nomEtatSignalement).FirstOrDefault();
                    ActionTraitement actionTraitementEtatModifier = db.ActionTraitements.Where(m => m.nomActionTraitement == "État de la plainte modifié à: " + nomEtatSignalementActuel).FirstOrDefault();

                    TraitementSignalement leTraitementEffectuer = new TraitementSignalement();
                    leTraitementEffectuer.compteAdminTraiteur = adminQuiTraite;
                    leTraitementEffectuer.noCompteAdminTraiteur = adminQuiTraite.noCompteAdmin;
                    leTraitementEffectuer.actionTraitement = actionTraitementEtatModifier;
                    leTraitementEffectuer.noActionTraitement = actionTraitementEtatModifier.noActionTraitement;
                    leTraitementEffectuer.dateTraitementSignalement = DateTime.Now;
                    leTraitementEffectuer.noSignalementLie = gestionViewModel.lesSignalement[indexSignalement].noSignalement;
                    leTraitementEffectuer.signalementLie = gestionViewModel.lesSignalement[indexSignalement];

                    db.TraitementSignalements.Add(leTraitementEffectuer);

                    //Retrait du nouveau signalement
                    CommentaireSignalement leNouveauCommentaire = gestionViewModel.lesSignalement[indexSignalement].lesCommentairesSurCeSignalement.Last();
                    gestionViewModel.lesSignalement[indexSignalement].lesCommentairesSurCeSignalement.Remove(leNouveauCommentaire);
                    db.Entry(leNouveauCommentaire).State = EntityState.Detached;

                    foreach(CommentaireSignalement c in gestionViewModel.lesSignalement[indexSignalement].lesCommentairesSurCeSignalement)
                    {
                        db.Entry(c).State = EntityState.Unchanged;
                    }

                    db.Entry(gestionViewModel.lesSignalement[indexSignalement]).State = EntityState.Modified;

                    try
                    {
                        db.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        Elmah.ErrorSignal.FromCurrentContext().Raise(Utilitaires.templateException("Gestion", "Admin", "Mise à jour de la base de données à la suite de la modification de l'état d'un signalement à échoué.", e, null, "post"));
                        TempData["gestionViewModelPOST"] = gestionViewModel;

                        TempData["Erreur"] = "Une erreur est survenu lors du changement de l'état de la plainte, veuillez réessayer.";
                        TempData.Keep();

                        return RedirectToAction("Gestion", new { page = page, tab = tab, vientDePagination = vientDePagination });
                    }

                }
                else if(btnSubmit.Contains("commentaireSignalement"))
                {
                    int index;
                    if(int.TryParse(btnSubmit.Substring(22), out index))
                    {
                        CommentaireSignalement leNouveauCommentaire = gestionViewModel.lesSignalement[index].lesCommentairesSurCeSignalement.Last();

                        if(leNouveauCommentaire.noCommentaireSignalement == 0 && leNouveauCommentaire.commentaireSignalement != "" && leNouveauCommentaire.commentaireSignalement != null)
                        {
                            leNouveauCommentaire.dateCommentaire = DateTime.Now;

                            db.CommentaireSignalements.Add(leNouveauCommentaire);
                            //db.Entry(leNouveauCommentaire).State = EntityState.Added;

                            db.SaveChanges();
                        }
                    }
                }
            }

            TempData["gestionViewModelPOST"] = gestionViewModel;

            return RedirectToAction("Gestion", new { page = page, tab = tab, vientDePagination = vientDePagination });
        }

        //[HttpGet]
        public ActionResult Statistique(int? page, StatistiqueViewModel statistiqueViewModel, int? tab, int? vientDePagination)
        {
            if (TempData["statistiqueViewModelPOST"] != null)
            {
                statistiqueViewModel = TempData["statistiqueViewModelPOST"] as StatistiqueViewModel;
            }

            if (vientDePagination != null)
            {
                statistiqueViewModel = TempData["statistiqueViewModelAncien"] as StatistiqueViewModel;
            }
            //Si on a fait un submit ou une chose autre que la pagination, on remet les pages à 0!
            else
            {
                page = null;
            }

            //On crée le gestion ViewModel s'il n'existe pas
            if (statistiqueViewModel == null)
            {
                statistiqueViewModel = new StatistiqueViewModel();
            }

            if (tab != null)
            {
                statistiqueViewModel.noTabSelected = (int)tab;
            }
            else if (statistiqueViewModel.noTabSelected == null)
            {
                statistiqueViewModel.noTabSelected = 1;
            }

            //Visites
            if (statistiqueViewModel.noTabSelected == 1)
            {
                int? anneeLaPlusLoin = db.Connexions.OrderBy(m => m.dateConnexion).Select(m => m.dateConnexion).FirstOrDefault().Year;
                int? anneeLaPlusProche = db.Connexions.OrderByDescending(m => m.dateConnexion).Select(m => m.dateConnexion).FirstOrDefault().Year;

                setupViewBagMoisEtAnnee(statistiqueViewModel.moisDebut, statistiqueViewModel.moisFin, statistiqueViewModel.anneeDebut, statistiqueViewModel.anneeFin, anneeLaPlusLoin, anneeLaPlusProche);

                statistiqueViewModel.nbVisitesGratuits = Statistiques.NbConnexion(statistiqueViewModel.anneeDebut, statistiqueViewModel.moisDebut, statistiqueViewModel.jourDebut,
                                                                                        statistiqueViewModel.anneeFin, statistiqueViewModel.moisFin, statistiqueViewModel.jourFin,
                                                                                        false);

                statistiqueViewModel.nbVisitesPremium = Statistiques.NbConnexion(statistiqueViewModel.anneeDebut, statistiqueViewModel.moisDebut, statistiqueViewModel.jourDebut,
                                                                                        statistiqueViewModel.anneeFin, statistiqueViewModel.moisFin, statistiqueViewModel.jourFin,
                                                                                        true);

                statistiqueViewModel.nbVisitesVisiteurs = Statistiques.NbConnexion(statistiqueViewModel.anneeDebut, statistiqueViewModel.moisDebut, statistiqueViewModel.jourDebut,
                                                                                        statistiqueViewModel.anneeFin, statistiqueViewModel.moisFin, statistiqueViewModel.jourFin,
                                                                                        null);
            }
            //Messagerie
            else if (statistiqueViewModel.noTabSelected == 2)
            {
                int? anneeLaPlusLoinMessage = db.Messages.OrderBy(m => m.dateEnvoi).Select(m => m.dateEnvoi).FirstOrDefault().Year;
                int? anneeLaPlusProcheMessage = db.Messages.OrderByDescending(m => m.dateEnvoi).Select(m => m.dateEnvoi).FirstOrDefault().Year;

                int? anneeLaPlusLoinCadeaux = db.Gifts.OrderBy(m => m.dateEnvoi).Select(m => m.dateEnvoi).FirstOrDefault().Year;
                int? anneeLaPlusProcheCadeaux = db.Gifts.OrderByDescending(m => m.dateEnvoi).Select(m => m.dateEnvoi).FirstOrDefault().Year;

                int? anneeLaPlusLoin = null;
                int? anneeLaPlusProche = null;

                if(anneeLaPlusLoinCadeaux == 1)
                {
                    anneeLaPlusLoinCadeaux = null;
                }
                if(anneeLaPlusLoinMessage == 1)
                {
                    anneeLaPlusLoinMessage = null;
                }
                if(anneeLaPlusProcheCadeaux == 1)
                {
                    anneeLaPlusProcheCadeaux = null;
                }
                if(anneeLaPlusProcheMessage == 1)
                {
                    anneeLaPlusProcheMessage = null;
                }

                if (anneeLaPlusLoinCadeaux != null && anneeLaPlusLoinMessage != null)
                {
                    anneeLaPlusLoin = (anneeLaPlusLoinCadeaux > anneeLaPlusLoinMessage ? anneeLaPlusLoinMessage : anneeLaPlusLoinCadeaux);
                }
                else if(anneeLaPlusLoinCadeaux == null)
                {
                    anneeLaPlusLoin = anneeLaPlusLoinMessage;
                }
                else if(anneeLaPlusLoinMessage == null)
                {
                    anneeLaPlusLoin = anneeLaPlusLoinCadeaux;
                }

                if (anneeLaPlusProcheCadeaux != null && anneeLaPlusProcheMessage != null)
                {
                    anneeLaPlusProche = (anneeLaPlusProcheCadeaux < anneeLaPlusProcheMessage ? anneeLaPlusProcheMessage : anneeLaPlusProcheCadeaux);
                }
                else if (anneeLaPlusProcheCadeaux == null)
                {
                    anneeLaPlusProche = anneeLaPlusProcheMessage;
                }
                else if (anneeLaPlusProcheMessage == null)
                {
                    anneeLaPlusProche = anneeLaPlusProcheCadeaux;
                }
                
                setupViewBagMoisEtAnnee(statistiqueViewModel.moisDebut, statistiqueViewModel.moisFin, statistiqueViewModel.anneeDebut, statistiqueViewModel.anneeFin, anneeLaPlusLoin, anneeLaPlusProche);

                statistiqueViewModel.nbCadeauxEnvoyes = Statistiques.NbMessagesOuCadeaux(statistiqueViewModel.anneeDebut, statistiqueViewModel.moisDebut, statistiqueViewModel.jourDebut,
                                                                                        statistiqueViewModel.anneeFin, statistiqueViewModel.moisFin, statistiqueViewModel.jourFin,
                                                                                        true);

                statistiqueViewModel.nbMessagesEnvoyes = Statistiques.NbMessagesOuCadeaux(statistiqueViewModel.anneeDebut, statistiqueViewModel.moisDebut, statistiqueViewModel.jourDebut,
                                                                                        statistiqueViewModel.anneeFin, statistiqueViewModel.moisFin, statistiqueViewModel.jourFin,
                                                                                        false);
            }
            //Activites
            else
            {
                int nbOrganisateursParPage = 25;

                int? anneeLaPlusLoin = db.Activites.OrderBy(m => m.date).Select(m => m.date).FirstOrDefault().Year;
                int? anneeLaPlusProche = db.Activites.OrderByDescending(m => m.date).Select(m => m.date).FirstOrDefault().Year;

                setupViewBagMoisEtAnnee(statistiqueViewModel.moisDebut, statistiqueViewModel.moisFin, statistiqueViewModel.anneeDebut, statistiqueViewModel.anneeFin, anneeLaPlusLoin, anneeLaPlusProche);

                //Changer null pour false si on veut seulement les activités non-annulés
                statistiqueViewModel.nbActivitesOrganisesTrouves = Statistiques.NbActiviteEtParticipantsOrganises(false, null, statistiqueViewModel.activitePayante, statistiqueViewModel.noThemeRecherche, statistiqueViewModel.noProvince, statistiqueViewModel.noVille,
                                                                                                        statistiqueViewModel.anneeDebut, statistiqueViewModel.moisDebut, statistiqueViewModel.jourDebut,
                                                                                                        statistiqueViewModel.anneeFin, statistiqueViewModel.moisFin, statistiqueViewModel.jourFin);
                statistiqueViewModel.nbActivitesAnnuleeTrouves = Statistiques.NbActiviteEtParticipantsOrganises(false, true, statistiqueViewModel.activitePayante, statistiqueViewModel.noThemeRecherche, statistiqueViewModel.noProvince, statistiqueViewModel.noVille,
                                                                                                        statistiqueViewModel.anneeDebut, statistiqueViewModel.moisDebut, statistiqueViewModel.jourDebut,
                                                                                                        statistiqueViewModel.anneeFin, statistiqueViewModel.moisFin, statistiqueViewModel.jourFin);

                statistiqueViewModel.nbParticipantsTrouves = Statistiques.NbActiviteEtParticipantsOrganises(true, false, statistiqueViewModel.activitePayante, statistiqueViewModel.noThemeRecherche, statistiqueViewModel.noProvince, statistiqueViewModel.noVille,
                                                                                                        statistiqueViewModel.anneeDebut, statistiqueViewModel.moisDebut, statistiqueViewModel.jourDebut,
                                                                                                        statistiqueViewModel.anneeFin, statistiqueViewModel.moisFin, statistiqueViewModel.jourFin);

                IQueryable<Membre> listeOrganisateurRequete = Statistiques.ListeOrganisateursActivite(null, statistiqueViewModel.activitePayante, statistiqueViewModel.noThemeRecherche, statistiqueViewModel.noProvince, statistiqueViewModel.noVille,
                                                                                                        statistiqueViewModel.anneeDebut, statistiqueViewModel.moisDebut, statistiqueViewModel.jourDebut,
                                                                                                        statistiqueViewModel.anneeFin, statistiqueViewModel.moisFin, statistiqueViewModel.jourFin,
                                                                                                        statistiqueViewModel.afficherMembresDesactiver);

                GestionPagination(page, nbOrganisateursParPage, listeOrganisateurRequete.Count());
                statistiqueViewModel.listeOrganisateurs = listeOrganisateurRequete.OrderBy(m => m.noMembre)
                                                        .Skip((int)(ViewBag.currentPage - 1) * nbOrganisateursParPage)
                                                        .Take(nbOrganisateursParPage)
                                                        .ToList();

                ViewBag.payant = new List<SelectListItem>
                {
                    new SelectListItem() { Text = "Payant", Value = true.ToString() },
                    new SelectListItem() { Text = "Gratuit", Value = false.ToString() }
                };

                ViewBag.province = new SelectList(db.Provinces, "noProvince", "nomProvince", statistiqueViewModel.noProvince);
                ViewBag.themes = new SelectList(db.Themes, "noTheme", "theme", statistiqueViewModel.noThemeRecherche);
            }

            TempData["statistiqueViewModelAncien"] = statistiqueViewModel;

            return View("Statistique", statistiqueViewModel);
        }

        public ActionResult DesactiverCompte(int noMembreDesactiver, int noSignalement)
        {
            Membre mem = db.Membres.Where(m => m.noMembre == noMembreDesactiver).Include(p => p.listePhotosMembres).Include(a => a.listeActivitesOrganises)
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
                mem.compteSupprimeParAdmin = true;

                //On annule chacune de ses activites FUTURES.
                foreach (Activite a in mem.listeActivitesOrganises)
                {
                    if (a.date > DateTime.Now)
                    {
                        Activite acti;
                        Utilitaires.AnnulerActivite(a.noActivite, null, out acti);
                    }
                }

                //On retire sa participation à tous les activités FUTURES.
                foreach (Activite a in mem.listeActivites)
                {
                    if (a.date > DateTime.Now)
                    {
                        Utilitaires.ParticiperActivite(a.noActivite, noMembreDesactiver);
                    }
                }

                db.SaveChanges();
            }

            CompteAdmin compteAdminTraiteur = db.CompteAdmins.Where(m => m.nomCompte == User.Identity.Name).FirstOrDefault();
            ActionTraitement actionCompteBloque = db.ActionTraitements.Where(m => m.nomActionTraitement == "Compte bloqué").FirstOrDefault();


            Signalement leSignalement = db.Signalements.Where(m => m.noSignalement == noSignalement).FirstOrDefault();
            leSignalement.dateSuiviNecessaire = null;
            leSignalement.etatSignalementActuel = db.EtatSignalements.Where(m => m.nomEtatSignalement == "Traité").FirstOrDefault();
            leSignalement.adminQuiTraite = compteAdminTraiteur;


            //Envoie d'un message automatisé de traitement
            Message messageAccuseApresTraitement = new Message();
            messageAccuseApresTraitement.dateEnvoi = DateTime.Now;
            messageAccuseApresTraitement.sujetMessage = "Votre signalement contre " + leSignalement.membreContreQuiEstPlainte.surnom + " a été traité.";
            messageAccuseApresTraitement.contenuMessage = "Votre signalement contre " + leSignalement.membreContreQuiEstPlainte.surnom + " a bien été traité, et une action a eu lieu afin que le comportement" +
                    " fautif ne se reproduise plus. Merci d'aider à rendre Club Contact un environnement sécuritaire!" + Environment.NewLine + Environment.NewLine + "- Le service d'administration de Club Contact";
            messageAccuseApresTraitement.lu = false;
            messageAccuseApresTraitement.membreReceveur = leSignalement.membreFaisantPlainte;
            messageAccuseApresTraitement.noMembreReceveur = leSignalement.membreFaisantPlainte.noMembre;

            db.Messages.Add(messageAccuseApresTraitement);
            db.SaveChanges();
            //Fin envoie d'un message automatisé de traitement

            TraitementSignalement leTraitementEffectuer = new TraitementSignalement();
            leTraitementEffectuer.compteAdminTraiteur = compteAdminTraiteur;
            leTraitementEffectuer.noCompteAdminTraiteur = compteAdminTraiteur.noCompteAdmin;
            leTraitementEffectuer.actionTraitement = actionCompteBloque;
            leTraitementEffectuer.noActionTraitement = actionCompteBloque.noActionTraitement;
            leTraitementEffectuer.dateTraitementSignalement = DateTime.Now;
            leTraitementEffectuer.noSignalementLie = leSignalement.noSignalement;
            leTraitementEffectuer.signalementLie = leSignalement;

            db.TraitementSignalements.Add(leTraitementEffectuer);

            List<Signalement> lesSignalementDuMembreQuiSeraDesactiver = db.Signalements.Where(m => m.noMembreContreQuiEstPlainte == noMembreDesactiver && m.noSignalement != leSignalement.noSignalement).ToList();
            foreach(Signalement s in lesSignalementDuMembreQuiSeraDesactiver)
            {
                s.dateSuiviNecessaire = null;
                s.etatSignalementActuel = db.EtatSignalements.Where(m => m.nomEtatSignalement == "Traité").FirstOrDefault();
                if(s.adminQuiTraite == null && s.noCompteAdmin == null)
                {
                    s.adminQuiTraite = compteAdminTraiteur;
                    s.noCompteAdmin = compteAdminTraiteur.noCompteAdmin;
                }

                TraitementSignalement traitementEffectureSurAutre = new TraitementSignalement();
                traitementEffectureSurAutre.compteAdminTraiteur = compteAdminTraiteur;
                traitementEffectureSurAutre.noCompteAdminTraiteur = compteAdminTraiteur.noCompteAdmin;
                traitementEffectureSurAutre.actionTraitement = actionCompteBloque;
                traitementEffectureSurAutre.noActionTraitement = actionCompteBloque.noActionTraitement;
                traitementEffectureSurAutre.dateTraitementSignalement = DateTime.Now;
                traitementEffectureSurAutre.noSignalementLie = s.noSignalement;
                traitementEffectureSurAutre.signalementLie = s;

                db.TraitementSignalements.Add(traitementEffectureSurAutre);
            }



            db.SaveChanges();

            return RedirectToAction("Gestion", "Admin", new { tab = 2 });
        }

        [NonAction]
        public void setupViewBagMoisEtAnnee(int? moisDebutSelected, int? moisFinSelected, int? anneeDebutSelected, int? anneeFinSelected, int? anneeLaPlusLoin, int? anneeLaPlusProche)
        {
            #region moisDebut
            List<SelectListItem> mois = (new List<SelectListItem>
                {
                    new SelectListItem { Text = "Janvier", Value = "1" },
                    new SelectListItem { Text = "Février", Value = "2" },
                    new SelectListItem { Text = "Mars", Value = "3" },
                    new SelectListItem { Text = "Avril", Value = "4" },
                    new SelectListItem { Text = "Mai", Value = "5" },
                    new SelectListItem { Text = "Juin", Value = "6" },
                    new SelectListItem { Text = "Juillet", Value = "7" },
                    new SelectListItem { Text = "Août", Value = "8" },
                    new SelectListItem { Text = "Septembre", Value = "9" },
                    new SelectListItem { Text = "Octobre", Value = "10" },
                    new SelectListItem { Text = "Novembre", Value = "11" },
                    new SelectListItem { Text = "Décembre", Value = "12" },
                });

            ViewBag.moisDebutViewBag = new SelectList(mois, "Value", "Text", moisDebutSelected);
            ViewBag.moisFinViewBag = new SelectList(mois, "Value", "Text", moisFinSelected);

            #endregion

            #region anneeDebut
            List<SelectListItem> annee = new List<SelectListItem>();
            if (anneeLaPlusLoin != null && anneeLaPlusProche != null)
            {
                for (int i = (int)anneeLaPlusLoin; i <= anneeLaPlusProche; i++)
                {
                    annee.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
                }
            }

            ViewBag.anneeDebutViewBag = new SelectList(annee, "Value", "Text", anneeDebutSelected);
            ViewBag.anneeFinViewBag = new SelectList(annee, "Value", "Text", anneeFinSelected);
            #endregion
        }

        public void ActiverDesactiverCompte(int noMembre)
        {
            Membre mem = db.Membres.Where(m => m.noMembre == noMembre).Include(p => p.listePhotosMembres).Include(a => a.listeActivitesOrganises)
                .Include(a => a.listeHobbies).Include(a => a.listeRaisonsSurSite).Include(a => a.listeDeVisitesDeMonProfil)
                .FirstOrDefault();

            if(mem != null)
            {
                //compte est actif, donc on désactive
                if(mem.dateSuppressionDuCompte == null && mem.compteSupprimeParAdmin == null)
                {
                    List<Activite> activites = db.Activites.Where(a => a.noMembreOrganisateur == mem.noMembre).ToList();
                    List<Photo> photos = db.Photos.Where(p => p.noMembre == mem.noMembre).ToList();

                    List<Visite> visites = db.Visites.Where(p => p.noMembreVisite == mem.noMembre || p.noMembreVisiteur == mem.noMembre).ToList();

                    mem.dateSuppressionDuCompte = DateTime.Now;
                    mem.compteSupprimeParAdmin = false;

                    //On annule chacune de ses activites FUTURES.
                    foreach (Activite a in mem.listeActivitesOrganises)
                    {
                        if (a.date > DateTime.Now)
                        {
                            Activite acti;
                            Utilitaires.AnnulerActivite(a.noActivite, null, out acti);
                        }
                    }

                    //On retire sa participation à tous les activités FUTURES.
                    foreach (Activite a in mem.listeActivites)
                    {
                        if (a.date > DateTime.Now)
                        {
                            Utilitaires.ParticiperActivite(a.noActivite, noMembre);
                        }
                    }
                }
                else
                {
                    mem.dateSuppressionDuCompte = null;
                    mem.compteSupprimeParAdmin = null;
                }

                db.SaveChanges();
            }
        }
    }
}

//Déchets

/*public ActionResult GestionComptesAdmin()
    {
        List<CompteAdmin> lesComptesAdmin = db.CompteAdmins.ToList();

        return View(lesComptesAdmin);
    }*/


//Contrôleur POST ne sera peut-être pas utile
/*[HttpPost]
public ActionResult Statistique(int? page, StatistiqueViewModel statistiqueViewModel, int? tab, int? vientDePagination, int btnSubmit)
{
    TempData["statistiqueViewModelPOST"] = statistiqueViewModel;

    return RedirectToAction("Statistique", new { page = page, tab = tab, vientDePagination = vientDePagination });
}*/
