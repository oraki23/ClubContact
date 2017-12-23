/*------------------------------------------------------------------------------------

CLASSE DÉFINISSANT LES BUNDLES DE SCRIPTS ET DE STYLES QUI SERONT UTILISÉS DANS L'APPLICATION

--------------------------------------------------------------------------------------
Par: Anthony Brochu et Marie-Ève Massé
Novembre 2017
Club Contact
------------------------------------------------------------------------------------*/

using System.Web.Optimization;

namespace ProjetSiteDeRencontre
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*",
                        "~/Scripts/Validators.js"));

            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new ScriptBundle("~/bundles/utilitaire").Include(
                      "~/Scripts/Utilitaire.js",
                      "~/Scripts/SlideShow.js",
                      "~/Scripts/ValidationMDP.js",
                      "~/Scripts/jquery.sumoselect.js",
                      "~/Scripts/jquery-ui-1.12.1.js"
                      ));

            bundles.Add(new StyleBundle("~/Content/cssGeneral").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/LesBaseStyle.css",
                      "~/Content/IndexActivite.css",
                      "~/Content/sumoselect.css",
                      "~/Content/themes/base/jquery-ui.css",
                      "~/Content/SectionAdmin.css"
                      )
                      );

            bundles.Add(new StyleBundle("~/Content/cssMembre").Include(
                      "~/Content/Membre.css"));

            bundles.Add(new StyleBundle("~/Content/cssActivite").Include(
                      "~/Content/Activite.css"));

            bundles.Add(new StyleBundle("~/Content/cssDetailsMembre").Include(
                      "~/Content/DetailsMembres.css"));

            bundles.Add(new StyleBundle("~/Content/cssHome").Include(
                      "~/Content/Home.css"));
        }
    }
}
