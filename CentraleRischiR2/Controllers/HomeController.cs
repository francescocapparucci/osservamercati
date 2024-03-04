using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.Configuration;
using CentraleRischiR2.Classes;
using CentraleRischiR2Library;
using log4net;

namespace CentraleRischiR2.Controllers
{
    public class HomeController : BaseController
    {
        //
        // GET: /Home/
        private static

            readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        [HttpPost]
        public ActionResult SendPassword(CentraleRischiR2.Models.User user)
        {
            
            if(! DBHandler.sendPassword(user.IdUser, user.Email))
            {
                return View("UserNotFound");
            }
            return View("Success");
        }

        public ActionResult IndexPass()
        {
            return View("IndexPass");
        }

        public ActionResult Policy()
        {
            return View("Policy");
        }

        public ActionResult Index(CentraleRischiR2.Models.User user)
        {
            Log.Info("begin Home Index**");
            /*Response.Write("<h1>"+ Request.UserAgent +"</h1>");*/
            ViewBag.Ambiente = WebConfigurationManager.AppSettings["Ambiente"];
            if (user.IsValid(user.Name, user.Password))
            {
                //TempData["DataUrl"] = "data-url=LoggedHome/Index";//
                FormsAuthentication.SetAuthCookie(user.Name, user.RememberMe);
                return RedirectToAction("Home", "LoggedHome");
            }
            /*
            else
            {
                ModelState.AddModelError("invalidLogin", "Login data is incorrect!");
                return RedirectToAction("Index", "Home");
            }
            */

            Log.Info("end Home Index**");
            return View();
        }


        [HttpPost]
        public ActionResult pass(Models.User user)
        {
            Log.Info("begin Home Login**");
            if (user.IsValid(user.Name, user.Password))
            {
                //TempData["DataUrl"] = "data-url=LoggedHome/Index";//
                FormsAuthentication.SetAuthCookie(user.Name, user.RememberMe);
                return RedirectToAction("Home", "LoggedHome");
            }
            else
            {
                ModelState.AddModelError("invalidLogin", "Login data is incorrect!");
                return RedirectToAction("Index", "Home");
            }

        }


        [HttpPost]
        public ActionResult Login(Models.User user)
        {
            Log.Info("begin Home Login**");
            if (user.IsValid(user.Name, user.Password))
            {
                //TempData["DataUrl"] = "data-url=LoggedHome/Index";//
                FormsAuthentication.SetAuthCookie(user.Name, user.RememberMe);
                return RedirectToAction("Home", "LoggedHome");
            }
            else
            {
                ViewBag.ErrorMessage =  @"<input type=""hidden"" name=""example"" value=""Errore Login"">";
                ModelState.AddModelError("invalidLogin", "Login data is incorrect!");
                return RedirectToAction("Index", "Home");
            }

        }
        [Authorize]
        public ActionResult Logout()
        {
            Log.Info("begin Home Logout**");
            ViewBag.Ambiente = WebConfigurationManager.AppSettings["Ambiente"];
            FormsAuthentication.SignOut();
            Session.Abandon();
            if (Request.Cookies["OSSERVACR2"] != null)
            {
                HttpCookie myCookie = new HttpCookie("OSSERVACR2");
                myCookie.Expires = DateTime.Now.AddDays(-1d);
                Response.Cookies.Add(myCookie);
                if (ViewBag.Ambiente == "STANDARD")
                {
                    Response.Redirect("https://www.osservamercati.it/?signout=y");
                }
                else
                {
                    Response.Redirect("https://www.osservacrediti.it?signout=y");
                }
            }
            Log.Info("end Home Logout**");
            return RedirectToAction("Index", "Home");
        }


    }
}
