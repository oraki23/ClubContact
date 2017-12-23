/*------------------------------------------------------------------------------------

CONTRÔLEUR GÉRANT L'AUTHENTIFICATION DANS LE SITE WEB
    GÈRE AUTANT L'AUTHENTIFICATION DES MEMBRES QUE L'AUTHENTIFICATION DES ADMINISTRATEURS

--------------------------------------------------------------------------------------
Par: Anthony Brochu et Marie-Ève Massé
Novembre 2017
Club Contact
------------------------------------------------------------------------------------*/

using Microsoft.AspNet.Identity;
using ProjetSiteDeRencontre;
using System.Collections.Generic;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using System.Linq;
using System;
using ProjetSiteDeRencontre.Models;
using System.Activities.Expressions;
using Microsoft.Owin.Security;
using System.Data.Entity;
using ProjetSiteDeRencontre.LesUtilitaires;
using ProjetSiteDeRencontre.Controllers;

/// <summary>
/// Controleur permettant l'authentification
/// </summary>
public class AuthentificationController : BaseController
{
    protected override void OnActionExecuting(ActionExecutingContext filterContext)
    {
        base.OnActionExecuting(filterContext);
    }

    /// <summary>
    /// Action renvoyant à la vue login qui permet de se connecter
    /// </summary>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpGet]
    public ActionResult Login(string ReturnUrl)
    {
        if(User.Identity.IsAuthenticated)
        {
            return RedirectToAction("Home", "Home");
        }
        else if(ReturnUrl != null)
        {
            if(ReturnUrl.ToLower().Contains("admin"))
            {
                return View("LoginAdmin");
            }
        }
        ViewBag.Title = "Se connecter";
        return View();
    }

    /// <summary>
    /// Action traitant le nom d'utilisateur et s'assure que le mot de passe correspond au mot de passe dans la base de données
    /// On crée aussi les cookies pour la page avec un numéro de commande (si une commande est incomplète) et un no Client
    /// </summary>
    /// <param name="model"></param>
    /// <param name="returnUrl"></param>
    /// <returns></returns>
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public ActionResult Login(LoginViewModel model, string returnUrl)
    {
        ClubContactContext db = new ClubContactContext();

        ViewBag.ReturnUrl = returnUrl;

        ViewBag.Title = "Se connecter";
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        if (!ValidateUser(model.Login, model.Password))
        {
            ModelState.AddModelError(string.Empty, "Le courriel ou le mot de passe est incorrect.");
            return View(model);
        }

        // L'authentification est réussie, 
        // injecter les informations utilisateur dans le cookie d'authentification :
        var userClaims = new List<Claim>();
        
        
        if (model.Login.ToLower() != "admin")
        {
            try
            {
                //on trouve le membre
                Membre leMembre = db.Membres
                    .Where(m => m.courriel.ToLower() == model.Login.ToLower() && m.compteSupprimeParAdmin == null && m.dateSuppressionDuCompte == null)
                    .FirstOrDefault();

                //Gestion de l'abonnement (vérifie qu'il est encore premium)
                Abonnement abonnementActuel = db.Abonnements
                                                    .Where(m => (m.noMembre == leMembre.noMembre) &&
                                                                (m.dateDebut < DateTime.Now) &&
                                                                (m.dateFin > DateTime.Now)
                                                     ).AsNoTracking().FirstOrDefault();
                if (abonnementActuel == null)
                {
                    //Si le membre est premium mais que son abonnement est terminé...
                    if(leMembre.premium)
                    {
                        abonnementActuel.renouveler = false;
                        leMembre.premium = false;

                        db.SaveChanges();
                    }
                }
                else if((abonnementActuel.dateFin.Date - DateTime.Now.Date) <= new TimeSpan(1, 0, 0, 0))
                {
                    //Afficher message qu'il reste juste 1 journée à son abonnement
                }

                if (leMembre != null)
                {
                    // Identifiant utilisateur :
                    userClaims.Add(new Claim(ClaimTypes.Name, leMembre.surnom));
                    userClaims.Add(new Claim(ClaimTypes.NameIdentifier, leMembre.surnom));
                }
                else
                {
                    throw new Exception("Erreur: Le membre recherché est égale à null à la suite de la requête LINQ.");
                }

                //-------------------------création du cookie-------------------------------------------
                HttpCookie myCookie = new HttpCookie("SiteDeRencontre");

                myCookie["noMembre"] = leMembre.noMembre.ToString();
                myCookie["premium"] = leMembre.premium ? "true" : "false";
                myCookie["seSouvenirDeMoi"] = model.seSouvenirDeMoi ? "true" : "false";

                /*---------pour analytics---------*/
                HttpCookie analytics = new HttpCookie("AnalyticsCC");
                analytics.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(analytics);
                /*---------pour analytics---------*/

                if (model.seSouvenirDeMoi)
                {
                    myCookie.Expires = new System.DateTime(2020, 1, 1);
                }

                Response.Cookies.Add(myCookie);
            }
            catch (Exception e)
            {
                Exception exc = new Exception("Action: Login (POST), Contrôleur: Authentification ; Erreur potentielle: Requête LINQ n'a pas fonctionnée. ; login Membre : " + model.Login + ".", e);
                Elmah.ErrorSignal.FromCurrentContext().Raise(exc);

                ModelState.AddModelError(string.Empty, "Une erreur s'est produite lors de la connexion, veuillez réessayer.");
                return View(model);
            }
        }
        else
        {
            // Identifiant utilisateur :
            userClaims.Add(new Claim(ClaimTypes.Name, model.Login));
            userClaims.Add(new Claim(ClaimTypes.NameIdentifier, model.Login));
        }

        // Rôles utilisateur :
        userClaims.AddRange(LoadRoles(model.Login));
        var claimsIdentity = new ClaimsIdentity(userClaims, DefaultAuthenticationTypes.ApplicationCookie);
        var ctx = Request.GetOwinContext();
        var authenticationManager = ctx.Authentication;
        var properties = new AuthenticationProperties { IsPersistent = model.seSouvenirDeMoi };
        authenticationManager.SignIn(properties, claimsIdentity);

        //?? à revoir une fois que ça fonctionne, avec la prof
        // Rediriger vers l'url d'origine :
        if (Url.IsLocalUrl(ViewBag.ReturnUrl))
            return Redirect(ViewBag.ReturnUrl);

        // Par défaut, rediriger vers la page d'accueil :
        return RedirectToAction("Home", "Home");
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public ActionResult LoginAdmin(LoginViewModel model, string returnUrl)
    {
        ClubContactContext db = new ClubContactContext();

        ViewBag.ReturnUrl = returnUrl;

        ViewBag.Title = "Se connecter administrateur";
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        if (!ValidateUserAdmin(model.Login, model.Password))
        {
            ModelState.AddModelError(string.Empty, "Le nom d'utilisateur ou le mot de passe est incorrect.");
            return View(model);
        }

        // L'authentification est réussie, 
        // injecter les informations utilisateur dans le cookie d'authentification :
        var userClaims = new List<Claim>();

        try
        {
            //on trouve le membre
            CompteAdmin leCompte = db.CompteAdmins
                .Where(m => m.nomCompte.ToLower() == model.Login.ToLower())
                .FirstOrDefault();

            if (leCompte != null)
            {
                // Identifiant utilisateur :
                userClaims.Add(new Claim(ClaimTypes.Name, leCompte.nomCompte));
                userClaims.Add(new Claim(ClaimTypes.NameIdentifier, leCompte.nomCompte));
            }
            else
            {
                throw new Exception("Erreur: Le membre recherché est égale à null à la suite de la requête LINQ.");
            }

            //-------------------------création du cookie-------------------------------------------
            HttpCookie myCookie = new HttpCookie("Admin");

            myCookie["niveauDePermission"] = leCompte.permission.nomNiveauDePermissionAdmin.ToString();

            if (model.seSouvenirDeMoi)
            {
                myCookie.Expires = new System.DateTime(2020, 1, 1);
            }

            Response.Cookies.Add(myCookie);
        }
        catch (Exception e)
        {
            Exception exc = new Exception("Action: Login (POST), Contrôleur: Authentification ; Erreur potentielle: Requête LINQ n'a pas fonctionnée. ; login Membre : " + model.Login + ".", e);
            Elmah.ErrorSignal.FromCurrentContext().Raise(exc);

            ModelState.AddModelError(string.Empty, "Une erreur s'est produite lors de la connexion, veuillez réessayer.");
            return View(model);
        }

        // Rôles utilisateur :
        userClaims.AddRange(LoadRoles(model.Login, true));
        var claimsIdentity = new ClaimsIdentity(userClaims, DefaultAuthenticationTypes.ApplicationCookie);
        var ctx = Request.GetOwinContext();
        var authenticationManager = ctx.Authentication;
        var properties = new AuthenticationProperties { IsPersistent = model.seSouvenirDeMoi };
        authenticationManager.SignIn(properties, claimsIdentity);

        //?? à revoir une fois que ça fonctionne, avec la prof
        // Rediriger vers l'url d'origine :
        if (Url.IsLocalUrl(ViewBag.ReturnUrl))
            return Redirect(ViewBag.ReturnUrl);

        // Par défaut, rediriger vers la page d'accueil :
        return RedirectToAction("Gestion", "Admin");
    }

    private bool ValidateUserAdmin(string login, string password)
    {
        ClubContactContext db = new ClubContactContext();

        try
        {
            //on va chercher le membre si celui-ci n'est pas supprimé
            CompteAdmin admin = db.CompteAdmins.Where(m => m.nomCompte.ToLower() == login.ToLower()).FirstOrDefault();

            if (admin != null)
            {
                //on vérifie si le mdp qu'il a entré est égal au mdp de la BD
                if (Membre.hashedPwd(password).Equals(admin.motDePasseHashe))
                {
                    return true;
                }
            }
        }
        catch (Exception e)
        {
            //Renvoyer un message d'erreur dans Elmah, mais on fait comme si la connexion n'as simplement pas fonctionner au niveau de l'utilisateur.
            Elmah.ErrorSignal.FromCurrentContext().Raise(
                new Exception("Action: ValidateUser, Contrôleur: Authentification ; Erreur potentielle: Requête LINQ n'a pas fonctionnée. ; login : " + login + ".", e)
                );
            ModelState.AddModelError(string.Empty, "Une erreur s'est produite lors de la connexion, veuillez réessayer.");
        }


        return false;
    }

    /// <summary>
    /// Méthode validant un utilisateur avec son mot de passe
    /// </summary>
    /// <param name="login"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    private bool ValidateUser(string login, string password)
    {
        //si l'admin veut se logger
        if (login.ToLower().Equals("admin"))
        {
            //son mot de passe est stocké dans le code
            if (password == "jaimeADMIN")
            {
                return true;
            }
        }
        else
        {
            ClubContactContext db = new ClubContactContext();

            try
            {
                //on va chercher le membre si celui-ci n'est pas supprimé
                Membre membre = db.Membres.Where(m => m.courriel.ToLower() == login.ToLower() && m.compteSupprimeParAdmin == null && m.dateSuppressionDuCompte == null).FirstOrDefault();

                if (membre != null)
                {
                    //on vérifie si le mdp qu'il a entré est égal au mdp de la BD
                    if (Membre.hashedPwd(password).Equals(membre.motDePasseHashe))
                    {
                        return true;
                    }
                }
            }
            catch(Exception e)
            {
                //Renvoyer un message d'erreur dans Elmah, mais on fait comme si la connexion n'as simplement pas fonctionner au niveau de l'utilisateur.
                Elmah.ErrorSignal.FromCurrentContext().Raise(
                    new Exception("Action: ValidateUser, Contrôleur: Authentification ; Erreur potentielle: Requête LINQ n'a pas fonctionnée. ; login : " + login + ".", e)
                    );
                ModelState.AddModelError(string.Empty, "Une erreur s'est produite lors de la connexion, veuillez réessayer.");
            }
        }
        return false;
    }

    /// <summary>
    /// Fonction permettant de mettre les rôles par défaut des utilisateurs
    /// </summary>
    /// <param name="login"></param>
    /// <returns></returns>
    private IEnumerable<Claim> LoadRoles(string login, bool admin = false)
    {
        //Role membre gratuit et membre Premium - à revoir

        // 2 rôles, Clients et Admin
        yield return new Claim(ClaimTypes.Role, "Membres");

        if (admin == true)
        {
            yield return new Claim(ClaimTypes.Role, "Admin");
        }
    }

    /// <summary>
    /// Action lorsque l'utilisateur désire se déconnecter
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public ActionResult Logout()
    {
        var ctx = Request.GetOwinContext();
        var authenticationManager = ctx.Authentication;
        authenticationManager.SignOut();

        //on modifie les cookies afin qu'on ne garde pas ceux du client précédemment connecté
        HttpCookie myCookie = new HttpCookie("SiteDeRencontre");

        myCookie.Expires = DateTime.Now.AddDays(-1);
        Response.Cookies.Add(myCookie);

        // Rediriger vers la page d'accueil :
        return RedirectToAction("Home", "Home");
    }


    [HttpGet]
    //Section Mot de passe oublié
    public ActionResult MotDePasseOublier(LoginViewModel loginViewModel)
    {
        return View("MDPOublierCourriel", loginViewModel);
    }

    [HttpPost]
    public ActionResult MotDePasseOublier(LoginViewModel loginViewModel, int? test)
    {
        ClubContactContext db = new ClubContactContext();

        Membre leMembreAReset = db.Membres.Where(m => m.courriel == loginViewModel.Login).FirstOrDefault();

        if (leMembreAReset != null)
        {
            Utilitaires.envoieCourriel(
                    "Réinitialisation de votre mot de passe Club Contact",
                    Utilitaires.RenderRazorViewToString(this, "MotDePasseOublierCourrielEnvoye", leMembreAReset.noMembre),
                    loginViewModel.Login
                    );
        }
        return RedirectToAction("Login");
    }

    [HttpGet]
    public ActionResult ResetMotDePasse(int noMembre)
    {
        ClubContactContext db = new ClubContactContext();

        Membre leMembreAResetLeMotDePasse = db.Membres.Where(m => m.noMembre == noMembre).FirstOrDefault();

        return View("ResetPassword", leMembreAResetLeMotDePasse);
    }

    [HttpPost]
    public ActionResult ResetMotDePasse(Membre leMembre, string motDePasse1)
    {
        ClubContactContext db = new ClubContactContext();

        Membre leMembreAResetLeMotDePasse = db.Membres.Where(m => m.noMembre == leMembre.noMembre).FirstOrDefault();

        ValiderMotDePasse(motDePasse1, null, leMembre.noMembre, true);


        //Reste validation Coté serveur à faire
        leMembreAResetLeMotDePasse.motDePasse = motDePasse1;

        db.SaveChanges();

        return RedirectToAction("Login", "Authentification");
    }
}

