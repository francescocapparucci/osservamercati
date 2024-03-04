using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace CentraleRischiR2
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");


            

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "PDF",
                url: "{controller}/{id}",
                defaults: new { controller = "PDF", action = "Check", id = UrlParameter.Optional }
            );

            

            routes.MapRoute(
                name: "Mercato",
                url: "{controller}/{action}/{piva}",
                defaults: new { controller = "Mercato", action = "Andamento", piva = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "GetPreferiti",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "LoggedHome", action = "GetPreferiti", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "LoggedHome",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "LoggedHome", action = "Home", id = UrlParameter.Optional }
            );
        }
    }
}