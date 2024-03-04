using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using WURFL;
using WURFL.Commons;
using WURFL.Config;
using WURFL.Aspnet;
using CentraleRischiR2.Classes;
using System.Web.WebPages;
using CentraleRischiR2Library.BridgeClasses;

namespace CentraleRischiR2
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public const String WurflDataFilePath = "~/App_Data/wurfl-latest.zip";

        protected void Application_Start()
        {

            

            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            BundleMobileConfig.RegisterBundles(BundleTable.Bundles);
           

            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("it-IT");
            

            DisplayModeProvider.Instance.Modes.Insert(0, new DefaultDisplayMode("")
            {

                ContextCondition = ctx =>                                            
                        /*SE E' IPAD*/
                        ctx.Request.UserAgent.IndexOf("ipad", StringComparison.OrdinalIgnoreCase) >= 0 ||                    
                        /*SE E' TABLET ANDROID NON MOBILE*/
                        (
                            ctx.Request.UserAgent.IndexOf("android", StringComparison.OrdinalIgnoreCase) >= 0  &&
                            ctx.Request.UserAgent.IndexOf("mobile", StringComparison.OrdinalIgnoreCase) < 0
                        )                        

            });            
          
            DisplayModeProvider.Instance.Modes.Insert(0, new DefaultDisplayMode("mobile")
            {

                ContextCondition = ctx =>
                            /*SE E' IPHONE*/
                            ctx.Request.UserAgent.IndexOf("iphone", StringComparison.OrdinalIgnoreCase) >= 0  ||                    
                            /*SE E' ANDROID MOBILE*/                        
                            (
                                ctx.Request.UserAgent.IndexOf("android", StringComparison.OrdinalIgnoreCase) >= 0 &&
                                ctx.Request.UserAgent.IndexOf("mobile", StringComparison.OrdinalIgnoreCase) >= 0
                            )

            });
          
            
            
        }
        
    }
}