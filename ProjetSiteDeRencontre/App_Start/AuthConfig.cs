/*------------------------------------------------------------------------------------

CLASSE APPELÉE VIA APPLICATION_START DANS GLOBAL.ASAX.CS
DÉFINIT LE MODÈLE D'AUTHENTIFICATION QUE NOUS ALLONS UTILISER, ET CE QUI VA SE PASSER SI UN USAGER N'EST PAS AUTHENTIFIÉ
DIFFÉRENTS PACKAGES SONT NÉCESSAIRES.  INDIQUE AUSSI LE MODÈLE POUR LE TEST ANTIPIRATAGE (ANTIFORGERY) 

--------------------------------------------------------------------------------------
Par: Anthony Brochu et Marie-Ève Massé
Novembre 2017
Club Contact
------------------------------------------------------------------------------------*/

using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;
using System.Web.Helpers;
using System.Security.Claims;


[assembly: OwinStartup(typeof(AuthConfig))]
public class AuthConfig
{
    /// <summary>
    /// Fait la configuration de base de sécurité de l'application
    /// </summary>
    /// <param name="app"></param>
    public void Configuration(IAppBuilder app)
    {
        app.UseCookieAuthentication(new CookieAuthenticationOptions
        {
            AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
            LoginPath = new PathString("/Authentification/Login")
        });

        AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier;
    }

    public static void RegisterAuth()
    {

    }
}
