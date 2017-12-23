/*------------------------------------------------------------------------------------

FICHIER SECONDAIRE DU CONTRÔLLEUR MEMBRE OÙ SE RETROUVE TOUTES LES ACTIONS UTILITAIRES
ET NON-ACTION

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
using System.Data.Entity.SqlServer;
using ProjetSiteDeRencontre.LesUtilitaires;

namespace ProjetSiteDeRencontre.Controllers
{
    /// <summary>
    /// Contrôleur permettant la gestion des membres. Permet aussi l'initialisation de la Base de données
    /// </summary>
    public partial class MembresController : BaseController
    {
        //Traitement des erreurs exhaustif non-nécessaire ici, car on ne retourne que la requete, on ne l'exécute pas.
        [NonAction]
        public IQueryable<Membre> RechercheMembreAvecProprietes(RechercheViewModel laRecherche, int noMembre)
        {
            #region trouverAgeEnChiffre
            //DateTime? ageMax = null;
            //DateTime? ageMin = null;

            //if (laRecherche.ageMax != null)
            //{
            //    ageMax = DateTime.Now.AddYears(-(int)laRecherche.ageMax);
            //}

            //if (laRecherche.ageMin != null)
            //{
            //    ageMin = DateTime.Now.AddYears(-(int)laRecherche.ageMin);
            //}
            #endregion

            IQueryable<Membre> membresTrouves = db.Membres;
            Membre membreActuel;
            try
            {
                membreActuel = db.Membres.Where(m => m.noMembre == noMembre).Include(m => m.listeNoire).FirstOrDefault();
            }
            catch(Exception e)
            {
                Dictionary<string, string> parametres = new Dictionary<string, string>() {
                    { "noMembre", noMembre.ToString()  }
                };
                throw Utilitaires.templateException("(Non-Action) RechercheMembreAvecProprietes", "Membres", "Requête LINQ n'a pas fonctionnée ou la BD est inaccessible.", e, parametres);
            }

            //on doit avoir un membre, sinon on ne peut pas tester la distance en KM
            if (membreActuel != null)
            {
                //Si on veut la recherche par KM
                if (laRecherche.distanceKmDeMoi != null)
                {
                    //La BD doit avoir été bien initialisée
                    if (membreActuel.ville.lat != null || membreActuel.ville.lng != null)
                    {
                        double latMembre = (double)membreActuel.ville.lat;
                        double lngMembre = (double)membreActuel.ville.lng;

                        //on définit la requête LINQ to SQL pour trouver les membres à une distance X
                        membresTrouves =
                        #region RequeteSQLCalculsDistance
                            (from m in db.Membres
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
                                where distInKM < laRecherche.distanceKmDeMoi
                                select m);
                        #endregion
                    }
                }

                //On ajoute la partie si le membre est connecté (on l'enlève des recherches pour pas qu'il se voit)
                membresTrouves = membresTrouves
                    .Where(m => (m.noMembre != noMembre) &&
                                //BlackListe
                                (!m.listeMembresQuiMontBloque.Select(x => x.noMembre).Contains(membreActuel.noMembre)) &&
                                (!m.listeNoire.Select(x => x.noMembre).Contains(membreActuel.noMembre))
                           );

                
            }


            //Après avoir ajouté les wheres et calculs possibles, on ajoute la query commune et on retourne le tout!
            //la recherche par age sera aussi implémenter de cette façon dans AdminController pour la liste des membres
            if(laRecherche.ageMin != null)
            {
                membresTrouves = (from m in membresTrouves
                                  let years = DateTime.Now.Year - ((DateTime)m.dateNaissance).Year
                                  let birthdayThisYear = DbFunctions.AddYears(m.dateNaissance, years)

                                  where laRecherche.ageMax >= (birthdayThisYear > DateTime.Now ? years - 1 : years)
                                  select m
                              );
            }
            if(laRecherche.ageMax != null)
            {
                membresTrouves = (from m in membresTrouves
                                  let years = DateTime.Now.Year - ((DateTime)m.dateNaissance).Year
                                  let birthdayThisYear = DbFunctions.AddYears(m.dateNaissance, years)

                                  where laRecherche.ageMin <= (birthdayThisYear > DateTime.Now ? years - 1 : years)
                                  select m
                              );
            }

            membresTrouves = membresTrouves
                #region RequeteLINQPourTousLesCriteres
            .Where(m => //On s'assure que le membre n'est pas supprimé, car on n'en veut pas dans notre recherche.
                        (m.compteSupprimeParAdmin == null && m.dateSuppressionDuCompte == null) &&

                        //((ageMax == null ? true : false) || DbFunctions.TruncateTime(m.dateNaissance) >= DbFunctions.TruncateTime(ageMax)) && //Signe = ajouté 2017-11-19 Anthony Brochu
                        //((ageMin == null ? true : false) || DbFunctions.TruncateTime(m.dateNaissance) <= DbFunctions.TruncateTime(ageMin)) && //Signe = ajouté 2017-11-19 Anthony Brochu
                        ((laRecherche.homme == null ? true : false) || m.homme == laRecherche.homme) &&
                        ((laRecherche.noRaisonsSite == null ? true : false) || m.listeRaisonsSurSite.Select(r => r.noRaisonSurSite).Contains((laRecherche.noRaisonsSite ?? 0))) &&
                        ((laRecherche.chercheHomme == null ? true : false) || m.rechercheHomme == laRecherche.chercheHomme) &&
                        ((laRecherche.noProvince == null ? true : false) || m.noProvince == laRecherche.noProvince) &&
                        ((laRecherche.noVille == null ? true : false) || m.noVille == laRecherche.noVille) &&
                        //Caractéristiques Physiques
                        ((laRecherche.noYeuxCouleur == null ? true : false) || m.noYeuxCouleur == laRecherche.noYeuxCouleur) &&
                        ((laRecherche.noCheveuxCouleur == null ? true : false) || m.noCheveuxCouleur == laRecherche.noCheveuxCouleur) &&
                        ((laRecherche.noSilhouette == null ? true : false) || m.noSilhouette == laRecherche.noSilhouette) &&
                        ((laRecherche.noTaille == null ? true : false) || m.noTaille == laRecherche.noTaille) &&
                        //Informations supplémentaires
                        ((laRecherche.fumeur == null ? true : false) || m.fumeur == laRecherche.fumeur) &&
                        ((laRecherche.noReligion == null ? true : false) || m.noReligion == laRecherche.noReligion) &&
                        ((laRecherche.noOrigine == null ? true : false) || m.noOrigine == laRecherche.noOrigine) &&
                        ((laRecherche.noDesirEnfant == null ? true : false) || m.noDesirEnfant == laRecherche.noDesirEnfant) &&
                        ((laRecherche.noOccupation == null ? true : false) || m.noOccupation == laRecherche.noOccupation) &&
                        ((laRecherche.noSituationFinanciere == null ? true : false) || m.noSituationFinanciere == laRecherche.noSituationFinanciere) &&
                        ((laRecherche.noNiveauEtude == null ? true : false) || m.noNiveauEtude == laRecherche.noNiveauEtude) &&

                        ((laRecherche.nbEnfantsMax == null ? true : false) || m.nbEnfants <= laRecherche.nbEnfantsMax) && //Signe "=" ajouté 2017-11-19 Anthony Brochu
                        ((laRecherche.nbEnfantsMin == null ? true : false) || m.nbEnfants >= laRecherche.nbEnfantsMin) && //Signe "=" ajouté 2017-11-19 Anthony Brochu

                        ((laRecherche.nbAnimauxMax == null ? true : false) || m.nbAnimaux <= laRecherche.nbAnimauxMax) && //Signe "=" ajouté 2017-11-19 Anthony Brochu
                        ((laRecherche.nbAnimauxMin == null ? true : false) || m.nbAnimaux >= laRecherche.nbAnimauxMin) //Signe "=" ajouté 2017-11-19 Anthony Brochu
                        );

            if(laRecherche.noTypeActiviteRecherche != null)
            {
                membresTrouves = membresTrouves
                        .Where(m => m.listeHobbies.Select(t => t.noType).Contains((int)laRecherche.noTypeActiviteRecherche));
            }

            if(laRecherche.noTypeInterets != null)
            {
                membresTrouves = membresTrouves
                        .Where(m => m.listeHobbies.Select(t => t.noHobbie).Contains((int)laRecherche.noTypeInterets));
            }
            #endregion

            return membresTrouves;
        }

        [NonAction]
        public void GestionPhotos(int etapeGestion, Membre membre, List<string> deletePhotos, List<HttpPostedFileBase> fichiersPhotos)
        {
            if (etapeGestion == 1)
            {
                for (int i = 0; i < membre.listePhotosMembres.Count; i++)
                {
                    if (membre.listePhotosMembres[i].noPhoto == 0 && fichiersPhotos[i] == null)
                    {
                        db.Entry(membre.listePhotosMembres[i]).State = EntityState.Detached;

                        membre.listePhotosMembres.RemoveAt(i);
                        deletePhotos.RemoveAt(i);
                        fichiersPhotos.RemoveAt(i);
                        i--;
                    }
                    else if (membre.listePhotosMembres[i].noPhoto == 0)
                    {
                        db.Entry(membre.listePhotosMembres[i]).State = EntityState.Added;
                    }
                }
            }
            else if (etapeGestion == 2)
            {
                for (int i = 0; i < membre.listePhotosMembres.Count; i++)
                {
                    if (deletePhotos.Count > i && deletePhotos[i] == "true")
                    {
                        //si on a supprimé une photo qu'on vient juste d'ajouter
                        if (membre.listePhotosMembres[i].noPhoto == 0)
                        {
                            //on la détache du contexte, car elle n'a pas été ajouté donc ne peut pas être retiré
                            db.Entry(membre.listePhotosMembres[i]).State = EntityState.Detached;

                            deletePhotos.RemoveAt(i);
                            fichiersPhotos.RemoveAt(i);
                            i--;
                        }
                        else
                        {
                            var path = Url.Content("~/Upload/PhotosMembres/" + membre.listePhotosMembres[i].nomFichierPhoto);
                            var fullPath = Request.MapPath(path);

                            if (System.IO.File.Exists(fullPath))
                            {
                                try
                                {
                                    System.IO.File.Delete(fullPath);
                                }
                                catch (Exception e)
                                {
                                    e.InnerException.Data.Add("Suppression", "La suppression de la photo: " + fullPath + " ne s'est pas complété correctement.");
                                    Elmah.ErrorSignal.FromCurrentContext().Raise(e);
                                }
                            }

                            db.Entry(membre.listePhotosMembres[i]).State = EntityState.Deleted;
                            //comme on enlève la photo, on enlève aussi le fichier associé (c'est logique)
                            //si on le met pas, on va avoir des bogues dans la 2iem boucle for!!

                            deletePhotos.RemoveAt(i);
                            fichiersPhotos.RemoveAt(i);
                            i--;
                        }
                    }
                    else if (membre.listePhotosMembres[i].noPhoto == 0)
                    {
                        db.Entry(membre.listePhotosMembres[i]).State = EntityState.Added;
                    }
                    else
                    {
                        db.Entry(membre.listePhotosMembres[i]).State = EntityState.Modified;
                    }
                }
            }
            else if (etapeGestion == 3)
            {
                for (int i = 0; i < membre.listePhotosMembres.Count; i++)
                {
                    if (fichiersPhotos[i] != null && !fichiersPhotos[i].FileName.ToString().ToLower().EndsWith(".jpg"))
                    {
                        this.ModelState.AddModelError("nomFichierPhoto", fichiersPhotos[i].FileName.ToString() + " doit être de type jpg");

                        //if (membre.listePhotosMembres[i].photoProfil)
                        //{
                        //    //Si on a une autre photo
                        //    if(membre.listePhotosMembres.Count > 1)
                        //    {
                        //        //On parcours les photos
                        //        for(int j = 0; j < membre.listePhotosMembres.Count; j++)
                        //        {
                        //            //Si la photo est différente de l'index actuelle
                        //            if(i != j)
                        //            {
                        //                //on la met photo de profil
                        //                membre.listePhotosMembres[j].photoProfil = true;

                        //                j = membre.listePhotosMembres.Count;
                        //                break;
                        //            }
                        //        }
                        //    }
                        //}

                        db.Entry(membre.listePhotosMembres[i]).State = EntityState.Deleted;
                        //membre.listePhotosMembres.Remove(membre.listePhotosMembres[i]);

                        deletePhotos.RemoveAt(i);
                        fichiersPhotos.RemoveAt(i);
                        i--;

                        //On sauvegarde la DB maintenant, car sinon la photo ne sera pas supprimé dans la BD et elle va réapparaitre plus tard
                        db.SaveChanges();

                        break;
                    }
                    else if (fichiersPhotos[i] != null)
                    {
                        try
                        {
                            string fname = Path.Combine(Server.MapPath("~/Upload/PhotosMembres/") + membre.noMembre + "$" + string.Format("{0:00000}", membre.listePhotosMembres[i].noPhoto) + ".jpg");
                            //Directory.CreateDirectory(Path.Combine(Server.MapPath("~/Upload/PhotosMembres/") + membre.noMembre));
                            fichiersPhotos[i].SaveAs(fname);

                            membre.listePhotosMembres[i].nomFichierPhoto = membre.noMembre + "$" + string.Format("{0:00000}", membre.listePhotosMembres[i].noPhoto) + ".jpg";
                        }
                        catch (Exception e)
                        {
                            this.ModelState.AddModelError("nomFichierPhoto", fichiersPhotos[i].FileName.ToString() + " incapable de sauvgarder.");
                        }
                    }
                    else
                    {
                        //la personne n'a pas ajouter de photo
                        if (membre.listePhotosMembres[i].nomFichierPhoto.Equals("00000.jpg"))
                        {
                            this.ModelState.AddModelError("nomFichierPhoto", "Veuillez définir une photo pour la/les nouvelle(s) photo(s)");
                        }
                    }
                }
            }
        }

        [NonAction]
        public void RemplirListesDeroulantesEtPreselectionnerHobbies(Membre membre, bool sansButPrecisInclus = true)
        {
            try
            {
                #region ViewBagsDeBase
                ViewBag.listeYeuxCouleur = new SelectList(db.YeuxCouleurs, "noYeuxCouleur", "nomYeuxCouleur", membre.noYeuxCouleur);
                ViewBag.listeCheveuxCouleur = new SelectList(db.CheveuxCouleurs, "noCheveuxCouleur", "nomCheveuxCouleur", membre.noCheveuxCouleur);
                ViewBag.listeSilhouette = new SelectList(db.Silhouettes, "noSilhouette", "nomSilhouette", membre.noSilhouette);
                ViewBag.listeTaille = new SelectList(db.Tailles, "noTaille", "taille", membre.noTaille);
                ViewBag.listeReligion = new SelectList(db.Religions, "noReligion", "religion", membre.noReligion);
                ViewBag.listeOrigine = new SelectList(db.Origines, "noOrigine", "origine", membre.noOrigine);
                ViewBag.listeOccupation = new SelectList(db.Occupations, "noOccupation", "nomOccupation", membre.noOccupation);
                ViewBag.listeSituationFinanciere = new SelectList(db.SituationsFinancieres, "noSituationFinanciere", "nomSituationFinanciere", membre.noSituationFinanciere);
                ViewBag.listeNiveauEtude = new SelectList(db.NiveauEtudes, "noNiveauEtude", "nomNiveauEtude", membre.noNiveauEtude);
                ViewBag.listeDesirEnfant = new SelectList(db.DesirEnfants, "noDesirEnfant", "desirEnfant", membre.noDesirEnfant);

                ViewBag.listeProvince = new SelectList(db.Provinces.OrderBy(p => p.nomProvince), "noProvince", "nomProvince");

                ViewBag.estFumeur = new List<SelectListItem>
                {
                    new SelectListItem { Text = "Oui", Value = true.ToString() },
                    new SelectListItem { Text = "Non", Value = false.ToString() },
                };

                ViewBag.sexe = new List<SelectListItem>
                {
                    new SelectListItem { Text = "Femme", Value = false.ToString() },
                    new SelectListItem { Text = "Homme", Value = true.ToString() },
                };

                ViewBag.sexeRecherche = new List<SelectListItem>
                {
                    new SelectListItem { Text = "Une femme", Value = false.ToString() },
                    new SelectListItem { Text = "Un homme", Value = true.ToString() },
                };

                ViewBag.sexeRecherche2 = new List<SelectListItem>
                {
                    new SelectListItem { Text = "Une femme", Value = false.ToString() },
                    new SelectListItem { Text = "Un homme", Value = true.ToString() },
                };
                #endregion

                ViewBag.listeHobbie = db.Hobbies.Include(p => p.type).ToList();
                ViewBag.listeType = db.Types.ToList();

                //spécial
                if (!sansButPrecisInclus)
                {
                    ViewBag.raisonsSurSite = new SelectList(db.RaisonsSurSites.Where(r => r.raison != "Sans but précis"), "noRaisonSurSite", "raison");
                }
                else
                {
                    ViewBag.raisonsSurSite = new SelectList(db.RaisonsSurSites, "noRaisonSurSite", "raison");
                }
                
                ViewBag.nbRaisonsSurSite = db.RaisonsSurSites.ToList().Count;

                RaisonsSurSite sansButPrecis = db.RaisonsSurSites.Where(r => r.raison == "Sans but précis").FirstOrDefault();
                if (sansButPrecis != null)
                {
                    ViewBag.noRaisonSansButPrecis = sansButPrecis.noRaisonSurSite;
                }

                if (membre.noMembre != 0)
                {
                    ViewBag.listeHobbiesMembre = db.Hobbies.Where(h => h.listeMembre.Any(m => m.noMembre == membre.noMembre)).ToList();
                }
            }
            catch (Exception e)
            {
                Dictionary<string, string> parametres = new Dictionary<string, string>() {
                    { "noMembre", membre.noMembre.ToString()  }
                    };
                throw Utilitaires.templateException("RemplirListesDeroulantesEtPreselectionnerHobbies", "Membres", "LRequête LINQ n'a pas fonctionnée ou la BD est inaccessible.", e, parametres, "post");
            }
        }

        [NonAction]
        public void RemplirListeDeroulantesPaiement(PaiementViewModel paiement)
        {
            ViewBag.listeCartes = new SelectList(new List<String> { "Visa", "MasterCard", "Amex" }, paiement.typeCarte);

            ViewBag.listeProvince = new SelectList(db.Provinces, "noProvince", "nomProvince", paiement.provinceFacturation);

            ViewBag.listeMois = new SelectList(new List<SelectListItem> {
                #region ListeMois
                new SelectListItem() { Value = "1", Text = "Janvier 01" },
                new SelectListItem() { Value = "2", Text = "Février 02" },
                new SelectListItem() { Value = "3", Text = "Mars 03" },
                new SelectListItem() { Value = "4", Text = "Avril 04" },
                new SelectListItem() { Value = "5", Text = "Mai 05" },
                new SelectListItem() { Value = "6", Text = "Juin 06" },
                new SelectListItem() { Value = "7", Text = "Juillet 07" },
                new SelectListItem() { Value = "8", Text = "Août 08" },
                new SelectListItem() { Value = "9", Text = "Septembre 09" },
                new SelectListItem() { Value = "10", Text = "Octobre 10" },
                new SelectListItem() { Value = "11", Text = "Novembre 11" },
                new SelectListItem() { Value = "12", Text = "Décembre 12" }
                #endregion
            }, "Value", "Text", paiement.carteMoisExpiration);


            List<String> annees = new List<String>();
            for (int i = DateTime.Now.Year; i < DateTime.Now.Year + 20; i++)
            {
                annees.Add(i.ToString());
            }

            ViewBag.listeAnnees = new SelectList(annees, paiement.carteAnneeExpiration);

            ViewBag.forfaitsPremium = db.ForfaitPremiums.ToList();
        }

        [Authorize(Roles = "admin")]
        public void EnvoyerCourrielTest()
        {
            Utilitaires.envoieCourriel(
                    "Confirmation de votre compte Club Contact (TEST)",
                    Utilitaires.RenderRazorViewToString(this, "CourrielConfirmation", new Membre { noMembre = 1 }),
                    "23bluo@gmail.com"
                    );
        }
    }
}