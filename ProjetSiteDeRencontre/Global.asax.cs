/*------------------------------------------------------------------------------------

FICHIER DE BASE DE L'APPLICATION

--------------------------------------------------------------------------------------
Par: Anthony Brochu et Marie-Ève Massé
Novembre 2017
Club Contact
------------------------------------------------------------------------------------*/

using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using ProjetSiteDeRencontre.Controllers;

namespace ProjetSiteDeRencontre
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_EndRequest()
        {

        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            //Récupère le context dans lequel a eu lieu l'erreur
            HttpContext httpContext = ((MvcApplication)sender).Context;

            //Récupère la dernière exception du serveur
            Exception exception = Server.GetLastError();

            // FORCE ELMAH A LOGGER L'ERREUR
            Elmah.ErrorSignal.FromCurrentContext().Raise(exception, httpContext);

            // CRÉE UN CONTRÔLEUR POUR TRAITER L'ERREUR
            IController errorController = new ErrorController();
            string url = "";
            {
                // get information to be passed to view as model
                string currentController = " ";
                string currentAction = " ";
                RouteData currentRouteData = RouteTable.Routes.GetRouteData(new HttpContextWrapper(httpContext));
                if (currentRouteData != null)
                {
                    if (currentRouteData.Values["controller"] != null && !String.IsNullOrEmpty(currentRouteData.Values["controller"].ToString()))
                    {
                        currentController = currentRouteData.Values["controller"].ToString();
                    }
                    if (currentRouteData.Values["action"] != null && !String.IsNullOrEmpty(currentRouteData.Values["action"].ToString()))
                    {
                        currentAction = currentRouteData.Values["action"].ToString();
                    }
                    url = Request.Url.AbsoluteUri;
                }

                ((Controller)errorController).ViewData.Model = new HandleErrorInfo(exception, currentController, currentAction);
            }
            httpContext.ClearError();
            httpContext.Response.Clear();
            httpContext.Response.ContentType = "text/HTML";
            httpContext.Response.StatusCode = exception is HttpException ? ((HttpException)exception).GetHttpCode() : 500;
            httpContext.Response.TrySkipIisCustomErrors = true;  // avoid IIS7 getting involved

            RouteData routeData = new RouteData();
            routeData.Values["controller"] = "Error";
            routeData.Values["action"] = "Error";
            routeData.Values["urlerreur"] = url;
            errorController.Execute(new RequestContext(new HttpContextWrapper(httpContext), routeData));
        }
    }
}

