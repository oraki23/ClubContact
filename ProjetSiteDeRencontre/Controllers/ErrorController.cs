/*------------------------------------------------------------------------------------

CONTRÔLEUR POUR LA GESTION DES ERREURS ET QUI RENVOIE LA VUE APPRORIÉE S'IL Y A UNE ERREUR

--------------------------------------------------------------------------------------
Par: Anthony Brochu et Marie-Ève Massé
Novembre 2017
Club Contact
------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using System.Security.Claims;

namespace ProjetSiteDeRencontre.Controllers
{
    public class ErrorController : Controller
    {

        //Action dans le cas où on aurait une erreur
        public ActionResult Error()
        {
            //Si l'utilisateur est l'admin
            if(User.IsInRole("Admin"))
            {
                //On affiche la vue Erreur complète
                return View("~/Views/Error/Error.cshtml");
            }
            else
            {
                //Sinon, on met la vue pour les clients (plus beau)
                return View("~/Views/Error/ErrorClient.cshtml");
            }            
        }

        /*[AllowAnonymous]
        public ActionResult Error()
        {
            // DANS LA VUE ON VA DÉSACTIVER LE LAYOUT, AU CAS OÙ L'ERREUR PROVIENDRAIT DE CELUI_CI:  this.Layout = null; 
            return View("ErrorClient");
        }*/

        /// <summary>
        /// Action dans le cas d'une erreur ajax
        /// </summary>
        /// <param name="url"></param>
        /// <param name="status"></param>
        /// <param name="raison"></param>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult Ajax(string url, string status, string raison)
        {
            // DANS LA VUE ON VA DÉSACTIVER LE LAYOUT, AU CAS OÙ L'ERREUR PROVIENDRAIT DE CELUI_CI:  this.Layout = null; 
            ViewData.Model = "une erreur AJAX lors de l'appel:" + url + "  raison:" + Server.UrlDecode(raison);
            return View("Error");
        }
    }
}
