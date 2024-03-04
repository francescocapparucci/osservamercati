using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CentraleRischiR2Library;
using CentraleRischiR2.Classes;
using CentraleRischiR2Library.BridgeClasses;

namespace CentraleRischiR2.Controllers
{
    public class ReportisticaAdminController : BaseController
    {        
        //
        // GET: /ReportisticaAdmin/

        public ActionResult ReportisticaAdmin()
        {
            if (loggeduser == null)
            {
                RedirectToAction("Index", "Home");
            }            
            ViewBag.RicercaAbilitata = loggeduser.AbilitatoRicerca;
            ViewBag.DataInizio = String.Format("{0:dd/MM/yyyy}",DateTime.Now.AddYears(-1));
            ViewBag.DataFine = String.Format("{0:dd/MM/yyyy}", DateTime.Now);
            ViewBag.ProvinceItaliane = ProvinceItaliane;
            ViewBag.Username = loggeduser.Username;
            ViewBag.CodiceAzienda = loggeduser.CodiceAzienda;
            ViewBag.Azienda = loggeduser.Azienda;
            ViewBag.Mercato = loggeduser.Mercato;
            ViewBag.Indirizzo = loggeduser.Indirizzo;
            ViewBag.Localita = loggeduser.Localita;
            ViewBag.Demo = loggeduser.Demo;
            ViewBag.ApprovatoPrivacy = loggeduser.ApprovatoPrivacy;
            ViewBag.DataApprovazionePrivacy = loggeduser.DataApprovazionePrivacy;
            ViewBag.Ambiente = loggeduser.Ambiente;
            ViewBag.IdRuolo = loggeduser.IdRuolo;
            ViewBag.IdUser = loggeduser.IdUser;

            ViewBag.NumeroMonitoraggiReportDisponibili = NumeroMonitoraggiReportDisponibili;
            ViewBag.VerificaSuperamentoSogliaReport = VerificaSuperamentoSogliaReport;

            ViewBag.ElencoCentri = DBHandler.ElencoCentri();
            /*ViewBag.ElencoOperatoriMercato = DBHandler.ElencoOpeartoriMercato(int idMercato);*/








            

            return View();
        }

        
        [Authorize]
        public JsonResult InserisciRichiesteReportFornitore()
        {
            string returnValue = "data";
            string partitaIva = Request.Form["piva"];
            DateTime dataInizio = Convert.ToDateTime(Request.Form["dataInizio"]);
            DateTime dataFine = Convert.ToDateTime(Request.Form["dataFine"]);

           // DBHandler.InserisciRichiesteReportFornitore(partitaIva, loggeduser.IdUser, loggeduser.Email, dataInizio, dataFine);

            return Json(returnValue, JsonRequestBehavior.AllowGet); 
        }

        [Authorize]
        public JsonResult InserisciRichiesteReportRP()
        {
            string returnValue = "data";
            string codiceOperatore = Request.Form["codice"];
            DateTime dataInizio = Convert.ToDateTime(Request.Form["dataInizio"]);
            DateTime dataFine = Convert.ToDateTime(Request.Form["dataFine"]);

            DBHandler.InserisciRichiesteReportRP(codiceOperatore, loggeduser.IdUser, loggeduser.Email, dataInizio, dataFine);
            return Json(returnValue, JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        public JsonResult InserisciRichiesteReportCN()
        {
            string returnValue = "data";            
            DateTime dataInizio = Convert.ToDateTime(Request.Form["dataInizio"]);
            DateTime dataFine = Convert.ToDateTime(Request.Form["dataFine"]);
            string codice = Request.Form["numeroAziende"];
            DBHandler.InserisciRichiesteReportCN(codice, loggeduser.IdUser, loggeduser.Email, dataInizio, dataFine);
            return Json(returnValue, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult InserisciRichiesteReportCRC()
        {
            string returnValue = "data";
            string codiceOperatore = Request.Form["codice"];
            int centro = Convert.ToInt32(Request.Form["centro"]);
            DateTime dataInizio = Convert.ToDateTime(Request.Form["dataInizio"]);
            DateTime dataFine = Convert.ToDateTime(Request.Form["dataFine"]);

           // DBHandler.InserisciRichiesteReportCRC(codiceOperatore, loggeduser.IdUser, loggeduser.Email, dataInizio, dataFine,centro);
            return Json(returnValue, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult InserisciRichiesteReportGlobale()
        {
            string returnValue = "data";
            string codiceOperatore = Request.Form["codice"];
          

            DBHandler.InserisciRichiesteReportGlobale(codiceOperatore, loggeduser.IdUser, loggeduser.Email);
            return Json(returnValue, JsonRequestBehavior.AllowGet);
        }






    }
}
