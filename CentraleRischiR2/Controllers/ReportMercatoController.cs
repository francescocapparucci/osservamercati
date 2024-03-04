using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CentraleRischiR2Library;
using CentraleRischiR2Library.BridgeClasses;
using CentraleRischiR2.Classes;
using System.Web.Script.Serialization;

namespace CentraleRischiR2.Controllers
{
    public class ReportMercatoController : BaseController
    {
        //
        // GET: /ReportMercato/
        [Authorize]
        public ActionResult RepMer()
        { 
            ViewBag.IdUser = loggeduser.IdUser;            
            ViewBag.ProvinceItaliane = ProvinceItaliane;
            ViewBag.Username = loggeduser.Username;
            ViewBag.Azienda = loggeduser.Azienda;
            ViewBag.Mercato = loggeduser.Mercato;
            ViewBag.Ambiente = loggeduser.Ambiente;
            ViewBag.IdRuolo = loggeduser.IdRuolo;
            using (DemoR2Entities context = new DemoR2Entities())
            {
                ViewBag.ReportCount = context.richiesta_report.Where(rep => rep.id_utente == loggeduser.IdUser && rep.evasa == false).Count();
                loggeduser.ReportCount = ViewBag.ReportCount;
            }
            ViewBag.Demo = loggeduser.Demo;
            ViewBag.NumeroMonitoraggiReportDisponibili = NumeroMonitoraggiReportDisponibili;
            ViewBag.VerificaSuperamentoSogliaReport = VerificaSuperamentoSogliaReport;
            ViewBag.RicercaAbilitata = loggeduser.AbilitatoRicerca;
            string appo = Request.QueryString["piva"];

            string titoloRC = "Tutti i Clienti";
            if (!String.IsNullOrEmpty(Request.QueryString["piva"]))
            {
            ViewBag.PartitaIva = Request.QueryString["piva"];
                ElementoAnagraficheOsservatorio aziendaO = DBHandler.getAziendeOperatori(ViewBag.PartitaIva, ViewBag.Mercato);
                ViewBag.TitoloRC = "Analisi Portafoglio Recupero Crediti Cliente " + aziendaO.RagioneSociale;
            }
            ViewBag.TitoloRC = titoloRC;
            if(loggeduser.IdRuolo == 1)
            {
                return View("RepMer");
            }
            else
            {
                return View("RepMerOp");
            }
            
        }

        [Authorize]
        /*NB VERRA' CREATO UN CONTROLLER BASE SU CUI ANDARE A INSERIRE QUESTO METODO E IL RETRIEVE DELLO USER*/
        public JsonResult GetPreferitiReport(string chiave, string valore)
        {
            /*Ricevo parametri da chiamata POST AJAX*/
            string sidx = String.IsNullOrEmpty(Request.Form["sidx"]) ? "Esposizione" : Request.Form["sidx"];
            string sord = Request.Form["sord"];
            string piva = Request.Form["piva"];
            int rows = Convert.ToInt32(Request.Form["rows"]);
            int page = Convert.ToInt32(Request.Form["page"]);

            List<ElementoReport> preferiti = new List<ElementoReport>();
            if (loggeduser.IdRuolo == 1)
            {
                preferiti = DBHandler.GetListReport(loggeduser.IdUser, loggeduser.IdMercato);
            }
            else
            {
                preferiti = DBHandler.GetListReportOpe(loggeduser.IdUser, loggeduser.IdMercato);
            }
            

            string searchField = !String.IsNullOrEmpty(Request.Form["searchField"]) ? Request.Form["searchField"] : String.Empty;
            string searchString = !String.IsNullOrEmpty(Request.Form["searchString"]) ? Request.Form["searchString"] : String.Empty;
            searchString = searchString.ToUpper();

            int recordTotali = preferiti.Count;
            int pagineTotali = recordTotali / rows;



            /*Pagino i preferiti ottenuti*/
            preferiti = preferiti.Skip((page > 0 ? page - 1 : 0) * rows).Take(rows).ToList();

            /*Formattazione JSON per JqGrid*/
            var result = new { page = page, total = (recordTotali + rows - 1) / rows, records = recordTotali, rows = preferiti };

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public ActionResult EliminaReportCount()
        {
            string partitaIvaCliente = Request.Form["clientePiva"];
            
            string ragioneSocialeCliente = Server.UrlDecode(Request.Form["clienteRagioneSociale"]);
            var deserializer = new JavaScriptSerializer();
            List<ElementoInserimentoFatturaRecuperoCrediti> fatture = deserializer.Deserialize<List<ElementoInserimentoFatturaRecuperoCrediti>>(Request.Form["fatture"]);
            //long idRichiesta = DBHandler.InserisciRichiestaRecuperoCrediti(tipologiaRecupero, loggeduser.CodiceAzienda, partitaIvaCliente, ragioneSocialeCliente, fatture);

            return View("RecuperoCrediti");
        }

        [Authorize]
        public JsonResult GetReport()
        {
            List<ReportAiende> preferiti2 = new List<ReportAiende>();
            if (loggeduser == null)
            {
                RedirectToAction("Index", "Home");
            }
            ViewBag.Username = loggeduser.Username;
            ViewBag.Azienda = loggeduser.Azienda;
            ViewBag.Mercato = loggeduser.Mercato;
            using (DemoR2Entities context = new DemoR2Entities())
            {
                ViewBag.ReportCount = context.richiesta_report.Where(rep => rep.id_utente == loggeduser.IdUser && rep.evasa == false).Count();
                loggeduser.ReportCount = ViewBag.ReportCount;
            }
            ViewBag.RicercaAbilitata = loggeduser.AbilitatoRicerca;

            string sidx = String.IsNullOrEmpty(Request.Form["sidx"]) ? "DataAggiornamento" : Request.Form["sidx"];
            string sord = Request.Form["sord"];
            int rows = Convert.ToInt32(Request.Form["rows"]);
            int page = Convert.ToInt32(Request.Form["page"]);
            string piva = Request.Form["piva"] != null ? Request.Form["piva"] : "";
            string meseRif = Request.Form["meseRif"] != null ? Request.Form["meseRif"] : "";
            string searchField = !String.IsNullOrEmpty(Request.Form["searchField"]) ? Request.Form["searchField"] : String.Empty;
            string searchString = !String.IsNullOrEmpty(Request.Form["searchString"]) ? Request.Form["searchString"] : String.Empty;
            //searchString = searchString.ToUpper();
            int idUser = loggeduser.IdUser;
            string data = "";

            preferiti2 = DBHandler.GetReportList(piva,meseRif,loggeduser.IdRuolo,loggeduser.IdUser);

            preferiti2 = preferiti2.OrderByDescending(or => or.DataRichiesta).ToList();
            if (preferiti2.Count <= 0)
            {
                RedirectToAction("Index", "Home");
            }
            ViewBag.Preferiti = preferiti2;
            ViewBag.TitoloRC = "Richieste Operatori";
            int recordTotali = preferiti2.Count;
            int pagineTotali = recordTotali / rows;

            if (searchField != String.Empty)
            {
                switch (searchField)
                {
                    case "id":

                        preferiti2 = preferiti2.Where(p => p.partitaIva.IndexOf(searchString, StringComparison.InvariantCultureIgnoreCase) >= 0).ToList();
                        break;
                    case "RagioneSociale":
                        preferiti2 = preferiti2.Where(p => p.RagioneSociale.IndexOf(searchString, StringComparison.InvariantCultureIgnoreCase) >= 0).ToList();
                        break;
                }
            }


            preferiti2 = preferiti2.Skip((page > 0 ? page - 1 : 0) * rows).Take(rows).ToList();

            /*Formattazione JSON per JqGrid*/
            var result = new { page = page, total = (recordTotali + rows - 1) / rows, records = recordTotali, rows = preferiti2 };

            return Json(result, JsonRequestBehavior.AllowGet);

        }
    }
}
