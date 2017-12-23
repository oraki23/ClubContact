/*------------------------------------------------------------------------------------

CLASSE PERMETTANT DE CONFIGURER LES FILTRES GLOBAUX. INUTILE DANS CETTE APPLICATION

--------------------------------------------------------------------------------------
Par: Anthony Brochu et Marie-Ève Massé
Novembre 2017
Club Contact
------------------------------------------------------------------------------------*/

using System.Web;
using System.Web.Mvc;

namespace ProjetSiteDeRencontre
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            //filters.Add(new HandleErrorAttribute());
        }
    }
}
