/*------------------------------------------------------------------------------------

CONTRÔLEUR ABSTRAIT CONTENANT LES ACTIONS ET FONCTIONS DE BASE AFIN QU'ILS SOIENT ACCESSIBLES
DEPUIS TOUS LES CONTRÔLEURS AFIN DE NE PAS RÉPÉTER DU CODE

--------------------------------------------------------------------------------------
Par: Anthony Brochu et Marie-Ève Massé
Novembre 2017
Club Contact
------------------------------------------------------------------------------------*/

using ProjetSiteDeRencontre.LesUtilitaires;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProjetSiteDeRencontre.Controllers
{
    public abstract class BaseController : Controller
    {
        [NonAction]
        public void GestionPagination(int? page, int nbItemsParPages, int nbItemsTotal)
        {
            ViewBag.totalPages = Math.Ceiling((decimal)(nbItemsTotal) / nbItemsParPages);
            ViewBag.currentPage = page != null ? page : 1;
            ViewBag.startPage = ViewBag.currentPage - 5;
            ViewBag.endPage = ViewBag.currentPage + 4;

            if (ViewBag.startPage <= 0)
            {
                ViewBag.endPage -= (ViewBag.startPage - 1);
                ViewBag.startPage = 1;
            }

            if (ViewBag.endPage > ViewBag.totalPages)
            {
                ViewBag.endPage = ViewBag.totalPages;
                if (ViewBag.endPage > nbItemsParPages && ViewBag.endPage - 9 > 0)
                {
                    ViewBag.startPage = ViewBag.endPage - 9;
                }
            }
        }

        [NonAction]
        public bool verifierSiCookieNoMembreExiste(out int valeurCookie)
        {
            var cookieSiteRencontre = Request.Cookies["SiteDeRencontre"];
            if (cookieSiteRencontre != null)
            {
                var cookieNoMembre = Request.Cookies["SiteDeRencontre"]["noMembre"];
                if (cookieNoMembre != null)
                {
                    valeurCookie = int.Parse(Request.Cookies["SiteDeRencontre"]["noMembre"].ToString());
                    return true;
                }
            }

            valeurCookie = -1;

            return false;
        }

        [NonAction]
        public void ValiderMotDePasse(string motDePasse1, string motDePasseOLD, int noMembre, bool nouveauMembre)
        {
            bool ancienMDPValide = true;

            if (!nouveauMembre)
            {
                if (motDePasse1 != string.Empty)
                {
                    ancienMDPValide = false;

                    UtilitaireController u = new UtilitaireController();
                    if (((JsonResult)u.ValiderAncienMDP(motDePasseOLD, noMembre)).Data.ToString().ToLower() == "true")
                    {
                        ancienMDPValide = true;
                    }
                }
            }

            #region validationDuMDPEtAffichageErreur
            if (!ancienMDPValide)
            {
                ModelState.AddModelError("motDePasse", "Votre mot de passe actuel est invalide.");
            }
            else if (motDePasse1 != string.Empty)
            {
                if (motDePasse1.Length < 8 && motDePasse1.Length > 0)
                {
                    ModelState.AddModelError("motDePasse", "Le nouveau mot de passe doit contenir au moins 8 caractères.");
                }
                else if (!Utilitaires.hasLetters(motDePasse1))
                {
                    ModelState.AddModelError("motDePasse", "Le nouveau mot de passe doit avoir au moins une lettre.");
                }
                else if (!Utilitaires.hasUpperCase(motDePasse1))
                {
                    ModelState.AddModelError("motDePasse", "Le nouveau mot de passe doit avoir au moins une lettre majuscule.");
                }
                else if (!Utilitaires.hasLowerCase(motDePasse1))
                {
                    ModelState.AddModelError("motDePasse", "Le nouveau mot de passe doit avoir au moins une lettre minuscule.");
                }
                else if (!Utilitaires.hasNumbers(motDePasse1))
                {
                    ModelState.AddModelError("motDePasse", "Le nouveau mot de passe doit avoir au moins un chiffre.");
                }
            }
            else if (nouveauMembre)
            {
                if (motDePasse1 == string.Empty)
                {
                    ModelState.AddModelError("motDePasse", "Veuillez entrer un mot de passe.");
                }
            }
            #endregion
        }
    }
}

//Déchets

/*[NonAction]
    public IQueryable<Membre> RechercheATroisNiveaux(RechercheViewModel laRecherche, Membre leMembre, int niveau = 1)
    {
        laRecherche = new RechercheViewModel();

        int differenceAge = 5;
        int nbHobbieMembre = leMembre.listeHobbies.Count;
        int nbHobbiesCommunNecessaires = 0;
        //Recherche très précise
        if (niveau == 1)
        {
            differenceAge = 5;

            //on a besoin d'au moins la moitié des hobbies pareil
            nbHobbiesCommunNecessaires = (int)Math.Ceiling((decimal)nbHobbieMembre / 2);

            //MEME VILLE POUR LE NIVEAU 1
            laRecherche.noProvince = leMembre.noProvince;
            laRecherche.noVille = leMembre.noVille;

            laRecherche.noDesirEnfant = leMembre.desirEnfant.noDesirEnfant;

            //meme si elle n'est pas là pour l'amour, on recherche quand meme des gens 


            //Si on est là pour une raison précise, on va trouver presque absolument quelqu'un avec
            //le même but sur le site
            if (leMembre.listeRaisonsSurSite.Any(r => r.raison == "Amour"))
            {
                laRecherche.noRaisonsSite = leMembre.listeRaisonsSurSite.Where(r => r.raison == "Amour").Select(r => r.noRaisonSurSite).FirstOrDefault();
            }
            else if (leMembre.listeRaisonsSurSite.Any(r => r.raison == "Amitié"))
            {
                laRecherche.noRaisonsSite = leMembre.listeRaisonsSurSite.Where(r => r.raison == "Amitié").Select(r => r.noRaisonSurSite).FirstOrDefault();
            }
            else if (leMembre.listeRaisonsSurSite.Any(r => r.raison == "Activité"))
            {
                laRecherche.noRaisonsSite = leMembre.listeRaisonsSurSite.Where(r => r.raison == "Activité").Select(r => r.noRaisonSurSite).FirstOrDefault();
            }
            else if (leMembre.listeRaisonsSurSite.Any(r => r.raison == "Partager"))
            {
                laRecherche.noRaisonsSite = leMembre.listeRaisonsSurSite.Where(r => r.raison == "Partager").Select(r => r.noRaisonSurSite).FirstOrDefault();
            }
        }

        //Recherche semi-précise
        if (niveau == 2)
        {
            differenceAge = 7;

            //au moins 1/4 des hobbies pareil
            nbHobbiesCommunNecessaires = (int)Math.Ceiling((decimal)nbHobbieMembre / 4);

            //METTRE RAYONS DE DISTANCE ASSEZ PETIT POUR NIVEAU 2 (Montréal-Granby)
            laRecherche.distanceKmDeMoi = 120;

            //Si on est là pour amour, on va trouver des gens qui sont là pour l'amour aussi,
            //mais sinon, on va trouver tout le monde.
            if (leMembre.listeRaisonsSurSite.Any(r => r.raison == "Amour"))
            {
                laRecherche.noRaisonsSite = leMembre.listeRaisonsSurSite.Where(r => r.raison == "Amour").Select(r => r.noRaisonSurSite).FirstOrDefault();
            }
        }

        //Recherche très générale
        if (niveau == 3)
        {
            differenceAge = 10;
            //il leur faut quand même au moins 1 hobbie nécéssaire!
            nbHobbiesCommunNecessaires = 1;

            //METTRE GROS RAYON QUAND MEME (Montréal-Québec)
            laRecherche.distanceKmDeMoi = 250;

            //laRecherche.noProvince = leMembre.noProvince;
            //laRecherche.noVille = leMembre.noVille;
        }

        //GÉNÉRALITÉS

        //Si la personne est là pour AMOUR, on lui trouve des gens qui sont là pour amour (sauf dans le cas #3)
        //Si la personne est là pour Amitié, on lui trouve des gens qui sont là pour activités
        //AMOUR > Amitié > Activité > Partager

        if (leMembre.listeRaisonsSurSite.Any(r => r.raison == "Amour"))
        {
            laRecherche.homme = leMembre.rechercheHomme;
            laRecherche.chercheHomme = leMembre.homme;
        }
        //critères communs
        #region CritereAge
        if (leMembre.age - differenceAge < 18)
        {
            laRecherche.ageMin = 18;
        }
        else
        {
            laRecherche.ageMin = leMembre.age - differenceAge;
        }
        laRecherche.ageMax = leMembre.age + differenceAge;
        #endregion

        //Les requêtes LINQ n'acceptend pas les Hobbies, car c'est un type trop complexe.
        // On va donc classer à partir des noHobbies (ca fonctionnera autant car ils sont uniques)
        // Les NoHobbie sont des types simple!
        var listeHobbieMembre = leMembre.listeHobbies.Select(h => h.noHobbie);

        IQueryable<Membre> membresTrouves = RechercheMembreAvecProprietes(laRecherche, leMembre.noMembre)
            .Where(m => (m.listeHobbies.Select(h => h.noHobbie).Intersect(listeHobbieMembre)).Count() > nbHobbiesCommunNecessaires);

        if (membresTrouves.Count() == 0)
        {
            if (niveau < 3)
            {
                return RechercheATroisNiveaux(laRecherche, leMembre, niveau + 1);
            }
        }

        return membresTrouves;
    }*/


//[HttpGet]
//public ActionResult Index(RechercheViewModel resultatsRecherche, int? page)
//{
//    GestionPagination(page, nbItemsParPages: 10, membres: resultatsRecherche.membres);

//    //a préremplir!!! avec propriété du ViewModel, les binder à un membre et le passer...
//    RemplirListesDeroulantesEtPreselectionnerHobbies(new Membre());

//    return View("Index", resultatsRecherche);
//}

//[HttpPost]
//public ActionResult Index(RechercheViewModel laRecherche)
//{
//    List<Membre> membreTrouves = db.Membres
//        .Where(m => (m.age < laRecherche.ageMax || (laRecherche.ageMax == null ? true : false)) &&
//                    (m.age > laRecherche.ageMin || (laRecherche.ageMin == null ? true : false)) &&
//                    (m.homme == laRecherche.homme || (laRecherche.homme == null ? true : false)) &&
//                    (m.rechercheHomme == laRecherche.chercheHomme || (laRecherche.chercheHomme == null ? true : false)) &&
//                    (m.noProvince == laRecherche.noProvince || (laRecherche.noProvince == null ? true : false)) &&
//                    (m.noVille == laRecherche.noVille || (laRecherche.noVille == null ? true : false)) &&
//                    //Caractéristiques Physiques
//                    (m.noYeuxCouleur == laRecherche.noYeuxCouleur || (laRecherche.noYeuxCouleur == null ? true : false)) &&
//                    (m.noCheveuxCouleur == laRecherche.noCheveuxCouleur || (laRecherche.noCheveuxCouleur == null ? true : false)) &&
//                    (m.noSilhouette == laRecherche.noSilhouette || (laRecherche.noSilhouette == null ? true : false)) &&
//                    (m.noTaille == laRecherche.noTaille || (laRecherche.noTaille == null ? true : false)) &&
//                    //Informations supplémentaires
//                    (m.fumeur == laRecherche.fumeur || (laRecherche.fumeur == null ? true : false)) &&
//                    (m.noReligion == laRecherche.noReligion || (laRecherche.noReligion == null ? true : false)) &&
//                    (m.noOrigine == laRecherche.noOrigine || (laRecherche.noOrigine == null ? true : false)) &&

//                    (m.nbEnfants < laRecherche.nbEnfantsMax || (laRecherche.nbEnfantsMax == null ? true : false)) &&
//                    (m.nbEnfants > laRecherche.nbEnfantsMin || (laRecherche.nbEnfantsMin == null ? true : false)) &&

//                    (m.nbAnimaux < laRecherche.nbAnimauxMax || (laRecherche.nbAnimauxMax == null ? true : false)) &&
//                    (m.nbAnimaux > laRecherche.nbAnimauxMin || (laRecherche.nbAnimauxMin == null ? true : false))
//                    )
//        .OrderBy(m => m.noMembre)
//        .Skip((int)(ViewBag.currentPage - 1) * nbItemsParPages)
//        .Take(nbItemsParPages)
//        .Include(m => m.listePhotosMembres)
//        .ToList();

//    RechercheViewModel recherche = new RechercheViewModel();
//    recherche.membres = membres;

//    RemplirListesDeroulantesEtPreselectionnerHobbies(new Membre());

//    return View("Index", recherche);
//}


//var membresTrouves = db.Membres
//.Where(m => ((ageMax == null ? true : false) || m.dateNaissance > ageMax) &&
//            ((ageMin == null ? true : false) || m.dateNaissance < ageMin) &&
//            ((laRecherche.homme == null ? true : false) || m.homme == laRecherche.homme) &&
//            ((laRecherche.chercheHomme == null ? true : false) || m.rechercheHomme == laRecherche.chercheHomme) &&
//            ((laRecherche.noProvince == null ? true : false) || m.noProvince == laRecherche.noProvince) &&
//            ((laRecherche.noVille == null ? true : false) || m.noVille == laRecherche.noVille) &&
//            //Caractéristiques Physiques
//            ((laRecherche.noYeuxCouleur == null ? true : false) || m.noYeuxCouleur == laRecherche.noYeuxCouleur) &&
//            ((laRecherche.noCheveuxCouleur == null ? true : false) || m.noCheveuxCouleur == laRecherche.noCheveuxCouleur) &&
//            ((laRecherche.noSilhouette == null ? true : false) || m.noSilhouette == laRecherche.noSilhouette) &&
//            ((laRecherche.noTaille == null ? true : false) || m.noTaille == laRecherche.noTaille) &&
//            //Informations supplémentaires
//            ((laRecherche.fumeur == null ? true : false) || m.fumeur == laRecherche.fumeur) &&
//            ((laRecherche.noReligion == null ? true : false) || m.noReligion == laRecherche.noReligion) &&
//            ((laRecherche.noOrigine == null ? true : false) || m.noOrigine == laRecherche.noOrigine) &&
//            ((laRecherche.noDesirEnfant == null ? true : false) || m.noDesirEnfant == laRecherche.noDesirEnfant) &&
//            ((laRecherche.noOccupation == null ? true : false) || m.noOccupation == laRecherche.noOccupation) &&
//            ((laRecherche.noSituationFinanciere == null ? true : false) || m.noSituationFinanciere == laRecherche.noSituationFinanciere) &&
//            ((laRecherche.noNiveauEtude == null ? true : false) || m.noNiveauEtude == laRecherche.noNiveauEtude) &&

//            ((laRecherche.nbEnfantsMax == null ? true : false) || m.nbEnfants < laRecherche.nbEnfantsMax) &&
//            ((laRecherche.nbEnfantsMin == null ? true : false) || m.nbEnfants > laRecherche.nbEnfantsMin) &&

//            ((laRecherche.nbAnimauxMax == null ? true : false) || m.nbAnimaux < laRecherche.nbAnimauxMax) &&
//            ((laRecherche.nbAnimauxMin == null ? true : false) || m.nbAnimaux > laRecherche.nbAnimauxMin) &&

//            ((laRecherche.distanceKmDeMoi == null ? true : false) || Utilitaires.Utilitaires.calcDistance(m.lat, m.lng, membreActuel.lat, membreActuel.lng) <= laRecherche.distanceKmDeMoi)
//            ).Where(m => m.noMembre != noMembre);



/*IQueryable<Membre> membresTrouves = RechercheMembreAvecProprietes(laRecherche, leMembre.noMembre)
            .Where(m => (m.listeHobbies.Select(h => h.noHobbie).Intersect(listeHobbieMembre)).Count() > nbHobbiesCommunNecessaires)
            //.Where(m => (leMembre.listeHobbies.Select(h => h.noHobbie).Intersect(m.listeHobbies.Select(h => h.noHobbie))).Count() > nbHobbiesCommunNecessaires)
            //.OrderBy(m => m.premium ? 0 : 1).ThenBy(m => m.noMembre)
            //.Include(m => m.listeHobbies)
            //.Include(m => m.listePhotosMembres)
            ;*/

//List<Membre> membresTrouves = RechercheMembreAvecProprietes(laRecherche, leMembre.noMembre)
//    .OrderBy(m => m.premium ? 0 : 1).ThenBy(m => m.noMembre)
//    .Include(m => m.listeHobbies)
//    .Include(m => m.listePhotosMembres)
//    .ToList();

//On trouve les membres qui ont au moins autant de hobbies commun que la moitié de nos hobbies
/*for (int i = membresTrouves.Count - 1; i >= 0; i--)
{
    int nbHobbiesCommun = 0;
    foreach (Hobbie h in leMembre.listeHobbies)
    {
        if (membresTrouves[i].listeHobbies.Contains(h))
        {
            nbHobbiesCommun++;
        }
    }
    if (nbHobbiesCommun < nbHobbiesCommunNecessaires)
    {
        membresTrouves.RemoveAt(i);
    }
}*/

//public void CreerActiviteBidon()
//{
//    var numberGenerator = new RandomGenerator();

//    IList<Activite> activites = new List<Activite>();

//    for (int i = 0; i < 1; i++)
//    {
//        int ageMin = Faker.RandomNumber.Next(18, 90);
//        int ageMax = Faker.RandomNumber.Next(ageMin, 90);

//        int random = Faker.RandomNumber.Next(1, 3);

//        Activite activite;

//        List<bool?> trueOuFalseOuNull = new List<bool?> { true, false, null };

//        if (random == 1)
//        {
//            activite = Builder<Activite>.CreateNew()
//            .With(a => a.nom = LesUtilitaires.Utilitaires.Truncate(Faker.Lorem.Paragraph(5), 20))
//            .With(a => a.description = LesUtilitaires.Utilitaires.Truncate(Faker.Lorem.Paragraph(3), 250))
//            .With(a => a.date = DateTime.Now.AddDays(-numberGenerator.Next(1, 100)))
//            .With(a => a.cout = Faker.RandomNumber.Next(0, 100))
//            .With(a => a.ageMax = ageMax)
//            .With(a => a.ageMin = ageMin)
//            .With(a => a.adresse = (Faker.RandomNumber.Next(0, 5000).ToString() + ", Rue " + LesUtilitaires.Utilitaires.Truncate(Faker.Lorem.Paragraph(5), 15)))
//            .With(a => a.province = Pick<Province>.RandomItemFrom(db.Provinces.ToList()))
//            .With(a => a.ville = Pick<Ville>.RandomItemFrom(a.province.listeVilles))
//            .With(a => a.theme = Pick<Theme>.RandomItemFrom(db.Themes.ToList()))
//            .With(a => a.membreOrganisateur = db.Membres.Where(membre => membre.noMembre == 7).FirstOrDefault())
//            .With(a => a.nbParticipantsMax = Faker.RandomNumber.Next(2, 30))
//            .With(a => a.hommeSeulement = Pick<bool?>.RandomItemFrom(trueOuFalseOuNull))
//            .Build();
//        }
//        else
//        {
//            activite = Builder<Activite>.CreateNew()
//            .With(a => a.nom = LesUtilitaires.Utilitaires.Truncate(Faker.Lorem.Paragraph(5), 20))
//            .With(a => a.description = LesUtilitaires.Utilitaires.Truncate(Faker.Lorem.Paragraph(3), 250))
//            .With(a => a.date = DateTime.Now.AddDays(numberGenerator.Next(1, 100)))
//            .With(a => a.cout = Faker.RandomNumber.Next(0, 100))
//            .With(a => a.ageMax = ageMax)
//            .With(a => a.ageMin = ageMin)
//            .With(a => a.adresse = (Faker.RandomNumber.Next(0, 5000).ToString() + ", Rue " + LesUtilitaires.Utilitaires.Truncate(Faker.Lorem.Paragraph(5), 15)))
//            .With(a => a.province = Pick<Province>.RandomItemFrom(db.Provinces.ToList()))
//            .With(a => a.ville = Pick<Ville>.RandomItemFrom(a.province.listeVilles))
//            .With(a => a.theme = Pick<Theme>.RandomItemFrom(db.Themes.ToList()))
//            .With(a => a.membreOrganisateur = db.Membres.Where(membre => membre.noMembre == 7).FirstOrDefault())
//            .With(a => a.nbParticipantsMax = Faker.RandomNumber.Next(2, 30))
//            .With(a => a.hommeSeulement = Pick<bool?>.RandomItemFrom(trueOuFalseOuNull))
//            .Build();
//        }

//        activites.Add(activite);
//    }

//    db.Activites.AddRange(activites);
//    try
//    {
//        db.SaveChanges();
//    }
//    catch(Exception e)
//    {
//        int allo = 0;
//    }

//}