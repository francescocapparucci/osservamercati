using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CentraleRischiR2.Classes;
using CentraleRischiR2Library;
using CentraleRischiR2Library.BridgeClasses;
using log4net;

namespace CentraleRischiR2.Controllers
{
    public class LoggedHomeController : BaseController
    {

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        [Authorize]
        [HttpPost]
        public ActionResult AccettaPrivacy()
        {
            string codiceAzienda = Request.Form["codiceAzienda"];
            DBHandler.ApprovaPrivacy(codiceAzienda);
            loggeduser.ApprovatoPrivacy = true;
            loggeduser.DataApprovazionePrivacy = DateTime.Now;
            Session["LoggedUser"] = loggeduser;
            return RedirectToAction("Home", "LoggedHome");
        }

            
        [Authorize]
        public ExcelResult GetRiepilogoRecuperoCreditiXLS()
        {

            List<ElementoFatturaRecuperoCrediti> elementiRC = DBHandler.GetRiepilogoRecuperoCrediti(loggeduser.IdUser, loggeduser.IdRuolo, loggeduser.IdMercato, loggeduser.Ambiente);
            elementiRC = elementiRC.OrderByDescending(or => or.DataRichiesta).ToList();
            ExcelResult report = new ExcelResult(elementiRC, "RichiesteRecuperoCrediti.xls");
            return report;

        }

        [Authorize]
        public ExcelResult GetPreferitiHomeXLS()
        {
           
            List<ElementoPreferiti> preferiti = DBHandler.GetPreferitiHome(loggeduser.IdUser, loggeduser.IdRuolo, loggeduser.IdMercato, loggeduser.Ambiente);
            preferiti = preferiti.OrderByDescending(or => or.DataVariazione).ToList();
            ExcelResult report = new ExcelResult(preferiti,"NewsPreferiti.xls");
            return report;
            
        }
        
        [Authorize]        
        public JsonResult GetPreferitiHome()
        {
            using (DemoR2Entities context = new DemoR2Entities())
            {
                loggeduser.ReportCount =  context.richiesta_report.Where(rep => rep.id_utente == loggeduser.IdUser).Count();
            }
            /*Ricevo parametri da chiamata POST AJAX*/
            string sidx = "";
            if (Request.Form["sidx"].Equals(""))
            {
                if (loggeduser.IdRuolo == 2)
                {
                    sidx = "DataAggiornamento";
                }
                else
                {
                    sidx = "Esposizione";
                }
            }
            else
            {
                sidx = Request.Form["sidx"];
            }

            string sord = Request.Form["sord"];
            int rows = Convert.ToInt32(Request.Form["rows"]);
            int page = Convert.ToInt32(Request.Form["page"]);
            string searchField = !String.IsNullOrEmpty(Request.Form["searchField"]) ? Request.Form["searchField"] : String.Empty;
            string searchString = !String.IsNullOrEmpty(Request.Form["searchString"]) ? Request.Form["searchString"] : String.Empty;
            searchString = searchString.ToUpper();

            /*Lista preferiti per Operatore Loggato*/
            List<ElementoPreferiti> preferiti = DBHandler.GetPreferitiHome(loggeduser.IdUser, loggeduser.IdRuolo, loggeduser.IdMercato, loggeduser.Ambiente);

            int recordTotali = preferiti.Count;
            int pagineTotali = recordTotali / rows;


            if (searchField != String.Empty)
            {
                switch (searchField)
                {
                    case "id":
                        preferiti = preferiti.Where(p => p.PartitaIva.Contains(searchString)).ToList();
                        break;
                    case "RagioneSociale":
                        preferiti = preferiti.Where(p => p.RagioneSociale.Contains(searchString)).ToList();
                        break;
                }
            }

            /*Ordino i preferiti*/
            if (sord == "desc")
            {
                switch (sidx)
                {
                    case "DataAggiornamento":
                        preferiti = preferiti.OrderByDescending(ord => ord.DataAggiornamento).ToList();
                        break;
                    case "DataVariazione":
                        preferiti = preferiti.OrderByDescending(ord => ord.DataVariazione).ToList();
                        break;
                    case "id":
                        preferiti = preferiti.OrderByDescending(ord => ord.PartitaIva).ToList();
                        break;
                    case "RagioneSociale":
                        preferiti = preferiti.OrderByDescending(ord => ord.RagioneSociale).ToList();
                        break;
                    case "Esposizione":
                        preferiti = preferiti.OrderByDescending(ord => ord.Esposizione).ToList();
                        break;                                        
                    case "Rating":
                        preferiti = preferiti.OrderByDescending(ord => ord.Rating).ToList();
                        break;
                    case "DescrizioneVariazione":
                        preferiti = preferiti.OrderByDescending(ord => ord.DescrizioneVariazione).ToList();
                        break;
                    case "UtenteNews":
                        preferiti = preferiti.OrderByDescending(ord => ord.UtenteNews).ToList();
                        break;
                    default:
                        preferiti = preferiti.OrderByDescending(ord => ord.DataVariazione).ToList();
                        break;
                }                
            }
            else
            {
                switch (sidx)
                {
                    case "DataAggiornamento":
                        preferiti = preferiti.OrderBy(ord => ord.DataAggiornamento).ToList();
                        break;
                    case "DataVariazione":
                        preferiti = preferiti.OrderBy(ord => ord.DataVariazione).ToList();
                        break;
                    case "id":
                        preferiti = preferiti.OrderBy(ord => ord.PartitaIva).ToList();
                        break;
                    case "RagioneSociale":
                        preferiti = preferiti.OrderBy(ord => ord.RagioneSociale).ToList();
                        break;
                    case "Esposizione":
                        preferiti = preferiti.OrderBy(ord => ord.Esposizione).ToList();
                        break;
                    case "DescrizioneVariazione":
                        preferiti = preferiti.OrderBy(ord => ord.DescrizioneVariazione).ToList();
                        break;
                    case "Rating":
                        preferiti = preferiti.OrderBy(ord => ord.Rating).ToList();
                        break;
                    case "UtenteNews":
                        preferiti = preferiti.OrderBy(ord => ord.UtenteNews).ToList();
                        break;
                    default:
                        preferiti = preferiti.OrderBy(ord => ord.DataVariazione).ToList();
                        break;
                }                
            }


            /*Pagino i preferiti ottenuti*/

            preferiti = preferiti.Skip((page > 0 ? page - 1 : 0) * rows).Take(rows).ToList();
            
            /*Formattazione JSON per JqGrid*/
            var result = new { page = page, total = (recordTotali + rows - 1) / rows, records = recordTotali, rows = preferiti };
            
            return Json(result,JsonRequestBehavior.AllowGet);
        }

        

        [Authorize]

        public ActionResult GetDettaglioAnagrafica()
        {
            /*Ricevo parametri da chiamata POST AJAX*/

            string piva = Request.Form["piva"];


            /*Lista Aziende Recupero Crediti per Operatore Loggato*/
            ElementoAnagraficheReport anag = DBHandler.GetDettaglioAnagrafica(piva);
            string strReturn = "";
            if (anag != null)
            {
                strReturn = "<table style='border:1px;width:100%' border=1>";
                strReturn += String.Format("<tr><td style='vertical-align:top;text-align:left' colspan='3'><b>{0}</b></td></tr>", anag.RagioneSociale);
                strReturn += String.Format("<tr><td style='vertical-align:top;text-align:left'>Indirizzo:<br /><b>{0} {1}</b><br /><b>{2} {3}</b></td><td style='vertical-align:top;text-align:left'>CCIA/NREA:<br /><b>{4}/{5}</b></td></tr>", anag.Indirizzo, anag.Comune, anag.Cap, anag.Provincia, anag.CCIA, anag.NREA);
                strReturn += String.Format("<tr><td colspan='3' style='vertical-align:top;text-align:left'>PEC:<br /><b>{0}</b></td></tr>", anag.PEC);                
                strReturn += String.Format("<tr><td style='vertical-align:top;text-align:left'>Iscrizione in CCIAA:<br /><b>{0}</b></td><td style='vertical-align:top;text-align:left' colspan='2'>Cod.Fisc./P.Iva:<br /><b>{1}</b></td></tr>", anag.iscrizioneCCIA, anag.PartitaIva);
                strReturn += String.Format("<tr><td style='vertical-align:top;text-align:left' colspan='3'>Descrizione Attività:<br /><b>{0}</b></td></tr>", anag.DescrizioneAttivita);
                strReturn += String.Format("<tr><td style='vertical-align:top;text-align:left' colspan='3'>Stato Attività:<br /><b>{0}</b></td></tr>", anag.StatoAttivita);
                strReturn += "</table>";
            
            }            
                        
            

            return Content(strReturn);
        }

        public ActionResult GetDettaglioValutazione()
        {
            /*Ricevo parametri da chiamata POST AJAX*/

            string piva = Request.Form["piva"];


            /*Lista Aziende Recupero Crediti per Operatore Loggato*/
            ElementoValutazione valutazione = DBHandler.GetDettaglioValutazione(piva);
            string strReturn = "";
            if (valutazione != null)
            {
                strReturn = "<table style='border:1px;width:100%' border=1>";
                string strImmagineValutazione = "";
                
                switch (valutazione.ValutazioneGlobale)
                { 
                    case "1":
                        strImmagineValutazione = "<img src='../content/images/sfera_verde.gif' alt='Verde 1' title='Verde 1' />";
                        break;
                    case "2":
                        strImmagineValutazione = "<img src='../content/images/sfera_verde.gif' alt='Verde 2' title='Verde 2' />";                            
                        break;
                    case "3":
                        strImmagineValutazione = "<img src='../content/images/sfera_verde.gif' alt='Verde 3' title='Verde 3' />";    
                        break;
                    case "4":
                        strImmagineValutazione = "<img src='../content/images/sfera_gialla.gif' alt='Gialla' title='Gialla' />";    
                        break;
                    case "5":
                        strImmagineValutazione = "<img src='../content/images/sfera_rossa.gif' alt='Rossa' title='Rossa' />";         
                        break;
                }

                strReturn += String.Format("<tr><td style='text-align:left'>Valutazione Globale:</td><td style='vertical-align:middle;text-align:center'>{0}</td></tr>", strImmagineValutazione);
                strReturn += String.Format("<tr><td style='text-align:left'>Valutazione Cerved:</td><td style='text-align:left'>Affidabilità {0}</td></tr>", valutazione.ValutazioneCervedSemaforo);
                strReturn += String.Format("<tr><td style='text-align:left'>Valutazione Osserva:</td><td style='text-align:left'>{0}</td></tr>", valutazione.ValutazioneOsservaSemaforo);
                strReturn += String.Format("<tr><td style='text-align:left'>Eventi Negativi:</td><td style='text-align:left'>{0}</td></tr>", valutazione.EventiNegativi);
                strReturn += String.Format("<tr><td style='text-align:left'>Fido:</td><td style='text-align:left'>{0}</td></tr>", valutazione.Fido);
                strReturn += "</table>";    
            }

            

            return Content(strReturn);
        }



           [Authorize]
           public new JsonResult GetRichiesteRecuperoCrediti()
           {
               /*Ricevo parametri da chiamata POST AJAX*/
               string sidx = String.IsNullOrEmpty(Request.Form["sidx"]) ? "Esposizione" : Request.Form["sidx"];
               string sord = Request.Form["sord"];
               int rows = Convert.ToInt32(Request.Form["rows"]);
               int page = Convert.ToInt32(Request.Form["page"]);
               string searchField = !String.IsNullOrEmpty(Request.Form["searchField"]) ? Request.Form["searchField"] : String.Empty;
               string searchString = !String.IsNullOrEmpty(Request.Form["searchString"]) ? Request.Form["searchString"] : String.Empty;
               searchString = searchString.ToUpper();

               /*Lista Aziende Recupero Crediti per Operatore Loggato*/
               List<ElementoAziendaRecuperoCrediti> listaRc = DBHandler.GetRichiesteRecuperoCreditiHome(loggeduser.IdUser, loggeduser.IdRuolo, loggeduser.IdMercato,"STANDARD");

               int recordTotali = listaRc.Count;
               int pagineTotali = recordTotali / rows;


               if (searchField != String.Empty)
               {
                   switch (searchField)
                   {
                       case "idRC":
                           listaRc = listaRc.Where(p => p.PartitaIva.Contains(searchString)).ToList();
                           break;
                       case "RagioneSocialeRC":
                           listaRc = listaRc.Where(p => p.RagioneSocialeRC.Contains(searchString)).ToList();
                           break;
                   }
               }

               /*Ordino Aziende Recupero Crediti*/

               if (sord == "desc")
               {
                   switch (sidx)
                   {
                       case "RagioneSociale":
                           listaRc = listaRc.OrderByDescending(ord => ord.RagioneSocialeRC).ToList();
                           break;
                       case "idRC":
                           listaRc = listaRc.OrderByDescending(ord => ord.PartitaIva).ToList();
                           break;
                       case "StatoRichiesta":
                           listaRc = listaRc.OrderByDescending(ord => ord.StatoRichiesta).ToList();
                           break;
                       case "DataRichiesta":
                           listaRc = listaRc.OrderByDescending(ord => ord.DataRichiesta).ToList();
                           break;
                       case "SommaFatture":
                           listaRc = listaRc.OrderByDescending(ord => ord.SommaFatture).ToList();
                           break;
                       default:
                           listaRc = listaRc.OrderByDescending(ord => ord.PartitaIva).ToList();
                           break;
                   }
               }
               else
               {
                   switch (sidx)
                   {
                       case "RagioneSociale":
                           listaRc = listaRc.OrderBy(ord => ord.RagioneSocialeRC).ToList();
                           break;
                       case "id":
                           listaRc = listaRc.OrderBy(ord => ord.PartitaIva).ToList();
                           break;
                       case "StatoRichiesta":
                           listaRc = listaRc.OrderBy(ord => ord.StatoRichiesta).ToList();
                           break;
                       case "DataRichiesta":
                           listaRc = listaRc.OrderBy(ord => ord.DataRichiesta).ToList();
                           break;
                       case "SommaFatture":
                           listaRc = listaRc.OrderBy(ord => ord.SommaFatture).ToList();
                           break;
                       default:
                           listaRc = listaRc.OrderBy(ord => ord.PartitaIva).ToList();
                           break;
                   }
               }

               /*Pagino Aziende Recupero Crediti ottenute*/
               listaRc = listaRc.Skip((page > 0 ? page - 1 : 0) * rows).Take(rows).ToList();

               /*Formattazione JSON per JqGrid*/
               var result = new { page, total = (recordTotali + rows - 1) / rows, records = recordTotali, rows = listaRc };

               return Json(result, JsonRequestBehavior.AllowGet);
           }

        [Authorize]        
        public ActionResult Home()
        {
            if (loggeduser == null)
            {
                RedirectToAction("Index", "Home");
            }

            ViewBag.RicercaAbilitata = loggeduser.AbilitatoRicerca;
            ViewBag.ProvinceItaliane = ProvinceItaliane;            
            ViewBag.Username = loggeduser.Username;
            ViewBag.CodiceAzienda = loggeduser.CodiceAzienda;
            ViewBag.Azienda = loggeduser.Azienda;
            ViewBag.Mercato = loggeduser.Mercato;
            ViewBag.Indirizzo = loggeduser.Indirizzo;
            ViewBag.Localita = loggeduser.Localita;
            ViewBag.Demo = loggeduser.Demo;
            using (DemoR2Entities context = new DemoR2Entities())
            {
                ViewBag.ReportCount = context.richiesta_report.Where(rep => rep.id_utente == loggeduser.IdUser && rep.evasa == false).Count();
                loggeduser.ReportCount = ViewBag.ReportCount;
            }
            ViewBag.ApprovatoPrivacy = loggeduser.ApprovatoPrivacy;
            ViewBag.DataApprovazionePrivacy = loggeduser.DataApprovazionePrivacy;
            ViewBag.Ambiente = loggeduser.Ambiente;
            ViewBag.IdRuolo = loggeduser.IdRuolo;
            ViewBag.IdUser = loggeduser.IdUser;
            ViewBag.NumeroMonitoraggiReportDisponibili = NumeroMonitoraggiReportDisponibili;
            ViewBag.VerificaSuperamentoSogliaReport = VerificaSuperamentoSogliaReport;            

            if (Request.Browser.IsMobileDevice)
            {
                List<ElementoPreferiti> preferiti = DBHandler.GetPreferitiHome(loggeduser.IdUser, loggeduser.IdRuolo, loggeduser.IdMercato, loggeduser.Ambiente);
                preferiti = preferiti.OrderByDescending(ord => ord.DataVariazione).Take(10).ToList();;                
                ViewBag.Preferiti = preferiti;
            }

            
            return View();
        }

        

        
                


    }
}
