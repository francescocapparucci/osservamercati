using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CentraleRischiR2Library;
using CentraleRischiR2Library.BridgeClasses;
using CentraleRischiR2.Classes;
using System.Configuration;
using System.Web.Configuration;
using System.Web.Script.Serialization;


namespace CentraleRischiR2.Controllers
{
    
    public class MiaAziendaController : BaseController
    {
        
        //
        // GET: /Home/
        private void getUrlSearch()
        {
            ViewBag.Chiave = "";
            ViewBag.Valore = "";

            if (!String.IsNullOrEmpty(Request.QueryString["chiave"]))
            {
                ViewBag.Chiave = Request.QueryString["chiave"];
                ViewBag.Valore = Request.QueryString["valore"];
            }
                        
        }        
        [Authorize]
        public JsonResult GetReportFasceGiorni()
        {
            return Json(DBHandler.GetReportFasceGiorni(loggeduser.IdUser, loggeduser.IdRuolo, loggeduser.IdMercato, loggeduser.Ambiente), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult GetReportFasceVendite()
        {
            return Json(DBHandler.GetReportFasceVendite(loggeduser.IdUser, loggeduser.IdRuolo, loggeduser.IdMercato, loggeduser.Ambiente), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult GetReportFasceEsposizione()
        {
            return Json(DBHandler.GetReportFasceEsposizione(loggeduser.IdUser, loggeduser.IdRuolo, loggeduser.IdMercato, loggeduser.Ambiente), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public ActionResult InviaSollecitoPEC() {
            string partitaIvaCliente = Request.Form["clientePiva"];
            string testo = Request.Form["testo"];            
            List<string> additionalData = new List<string>();
            additionalData.Add(partitaIvaCliente);
            additionalData.Add(loggeduser.Username);
            additionalData.Add(testo);

            MailHandler.WarnMail(loggeduser.IdUser, "RC_SOLLECITO", additionalData);
            return View("RecuperoCrediti");
        }
        [Authorize]
        public ActionResult InserisciRichiestaRecuperoCrediti()
        {
            string partitaIvaCliente = Request.Form["clientePiva"];
            string tipologiaRecupero = Request.Form["tipologiaRecupero"];            
            string ragioneSocialeCliente = Server.UrlDecode(Request.Form["clienteRagioneSociale"]);
            var deserializer = new JavaScriptSerializer();
            List<ElementoInserimentoFatturaRecuperoCrediti> fatture = deserializer.Deserialize<List<ElementoInserimentoFatturaRecuperoCrediti>>(Request.Form["fatture"]);
            long idRichiesta = DBHandler.InserisciRichiestaRecuperoCrediti(tipologiaRecupero,loggeduser.CodiceAzienda, partitaIvaCliente, ragioneSocialeCliente, fatture);
           
            return View("RecuperoCrediti");
        }

        [Authorize]
        public ActionResult InserisciRichiestaAnticipoFatture()
        {
            string partitaIvaCliente = Request.Form["clientePiva"];
            string tipologiaRecupero = Request.Form["tipologiaRecupero"];
            string ragioneSocialeCliente = Server.UrlDecode(Request.Form["clienteRagioneSociale"]);
            var deserializer = new JavaScriptSerializer();
            List<ElementoInserimentoFatturaRecuperoCrediti> fatture = deserializer.Deserialize<List<ElementoInserimentoFatturaRecuperoCrediti>>(Request.Form["fatture"]);
            //long idRichiesta = 
            DBHandler.InserisciAnticipoFatture(tipologiaRecupero, loggeduser.CodiceAzienda, partitaIvaCliente, ragioneSocialeCliente, fatture);

            return View("RecuperoCrediti");
        }

        [Authorize]
        /*NB VERRA' CREATO UN CONTROLLER BASE SU CUI ANDARE A INSERIRE QUESTO METODO E IL RETRIEVE DELLO USER*/
        public JsonResult GetPreferitiRecuperoCrediti(string chiave, string valore)
        {
            /*Ricevo parametri da chiamata POST AJAX*/
            string sidx = String.IsNullOrEmpty(Request.Form["sidx"]) ? "Esposizione" : Request.Form["sidx"];
            string sord = Request.Form["sord"];
            string piva = Request.Form["piva"];
            int rows = Convert.ToInt32(Request.Form["rows"]);
            int page = Convert.ToInt32(Request.Form["page"]);

            /*Lista preferiti per Operatore Loggato*/
            List<ElementoPreferiti> preferiti = DBHandler.GetPreferitiRecuperoCrediti(loggeduser.IdUser, loggeduser.IdRuolo, loggeduser.IdMercato, loggeduser.Ambiente);

            string searchField = !String.IsNullOrEmpty(Request.Form["searchField"]) ? Request.Form["searchField"] : String.Empty;
            string searchString = !String.IsNullOrEmpty(Request.Form["searchString"]) ? Request.Form["searchString"] : String.Empty;
            searchString = searchString.ToUpper();



            if (piva != String.Empty)
            {
                preferiti = preferiti.Where(p => p.PartitaIva == piva).ToList();
            }
            else
            {

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

                if (chiave == "giorni")
                {

                    switch (valore)
                    {
                        case "60":
                            preferiti = preferiti.Where(p => p.GG < 60).ToList();
                            break;
                        case "150":
                            preferiti = preferiti.Where(p => p.GG >= 60 && p.GG < 150).ToList();
                            break;
                        case "999":
                            preferiti = preferiti.Where(p => p.GG >= 150).ToList();
                            break;
                    }

                }

                if (chiave == "vendite")
                {
                    switch (valore)
                    {
                        case "20000":
                            preferiti = preferiti.Where(p => p.Vendite < 20000).ToList();
                            break;
                        case "100000":
                            preferiti = preferiti.Where(p => p.Vendite >= 20000 && p.Vendite < 100000).ToList();
                            break;
                        case "400000":
                            preferiti = preferiti.Where(p => p.Vendite >= 100000 && p.Vendite < 400000).ToList();
                            break;
                        case "999":
                            preferiti = preferiti.Where(p => p.Vendite >= 400000).ToList();
                            break;
                    }
                }

                if (chiave == "esposizione")
                {
                    switch (valore)
                    {
                        case "1000":
                            preferiti = preferiti.Where(p => p.Esposizione < 1000).ToList();
                            break;
                        case "5000":
                            preferiti = preferiti.Where(p => p.Esposizione >= 1000 && p.Esposizione < 5000).ToList();
                            break;
                        case "25000":
                            preferiti = preferiti.Where(p => p.Esposizione >= 5000 && p.Esposizione < 25000).ToList();
                            break;
                        case "100000":
                            preferiti = preferiti.Where(p => p.Esposizione >= 25000 && p.Esposizione < 100000).ToList();
                            break;
                        case "999":
                            preferiti = preferiti.Where(p => p.Esposizione >= 100000).ToList();
                            break;
                    }
                }
            }



            int recordTotali = preferiti.Count;
            int pagineTotali = recordTotali / rows;

            /*Ordino i preferiti*/
            if (sord == "desc")
            {
                switch (sidx)
                {
                    case "id":
                        preferiti = preferiti.OrderByDescending(ord => ord.PartitaIva).ToList();
                        break;
                    case "RagioneSociale":
                        preferiti = preferiti.OrderByDescending(ord => ord.RagioneSociale).ToList();
                        break;
                    case "Rating":
                        preferiti = preferiti.OrderByDescending(ord => ord.Rating).ToList();
                        break;
                    case "Fido":
                        preferiti = preferiti.OrderByDescending(ord => ord.Fido).ToList();
                        break;
                    case "Esposizione":
                        preferiti = preferiti.OrderByDescending(ord => ord.Esposizione).ToList();
                        break;
                    /*
                    case "GG":
                        preferiti = preferiti.OrderByDescending(ord => ord.GG).ToList();
                        break;
                    */
                    default:
                        preferiti = preferiti.OrderByDescending(ord => ord.Esposizione).ToList();
                        break;
                }
            }
            else
            {
                switch (sidx)
                {
                    case "id":
                        preferiti = preferiti.OrderBy(ord => ord.PartitaIva).ToList();
                        break;
                    case "RagioneSociale":
                        preferiti = preferiti.OrderBy(ord => ord.RagioneSociale).ToList();
                        break;
                    case "Rating":
                        preferiti = preferiti.OrderBy(ord => ord.Rating).ToList();
                        break;
                    case "Fido":
                        preferiti = preferiti.OrderBy(ord => ord.Fido).ToList();
                        break;
                    case "Esposizione":
                        preferiti = preferiti.OrderBy(ord => ord.Esposizione).ToList();
                        break;
                    /*
                    case "GG":
                        preferiti = preferiti.OrderBy(ord => ord.GG).ToList();
                        break;
                    */
                    default:
                        preferiti = preferiti.OrderBy(ord => ord.Esposizione).ToList();
                        break;
                }
            }


            /*Pagino i preferiti ottenuti*/
            preferiti = preferiti.Skip((page > 0 ? page - 1 : 0) * rows).Take(rows).ToList();

            /*Formattazione JSON per JqGrid*/
            var result = new { page = page, total = (recordTotali + rows - 1) / rows, records = recordTotali, rows = preferiti };

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [Authorize]        
        public ActionResult RecuperoCrediti()
        {
            if (loggeduser == null)
            {
                RedirectToAction("Index", "Home");
            }
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

            getUrlSearch();

            string titoloRC = "Tutti i Clienti";

            if (!String.IsNullOrEmpty(Request.QueryString["piva"]))
            {
                ViewBag.PartitaIva = Request.QueryString["piva"];
                ElementoAnagraficheOsservatorio aziendaO = DBHandler.GetAziendaOsservatorio(ViewBag.PartitaIva);                
                ViewBag.TitoloRC = "Analisi Portafoglio Recupero Crediti Cliente " + aziendaO.RagioneSociale;
            }
            if (ViewBag.Chiave != String.Empty)
            {
                string valore = ViewBag.Valore;
                string chiave = ViewBag.Chiave;
                
                if (chiave == "giorni")
                {
                    switch (valore)
                    {
                        case "60":
                            titoloRC = "Clienti Dilazione Pagamento sotto 60gg";
                            break;
                        case "150":
                            titoloRC = "Clienti Dilazione Pagamento tra 60gg e 150gg";
                            break;
                        case "999":
                            titoloRC = "Clienti Dilazione Pagamento oltre i 150gg";
                            break;                        
                    }
                }
                if (chiave == "vendite")
                {
                    switch (valore)
                    {
                        case "20000":
                            titoloRC = "Clienti Vendite sotto €20.000";
                            break;
                        case "100000":
                            titoloRC = "Clienti Vendite tra €20.000 e €100.000";
                            break;
                        case "400000":
                            titoloRC = "Clienti Vendite tra €100.000 e €400.000";
                            break;
                        case "999":
                            titoloRC = "Clienti Vendite oltre €400.000";
                            break;                        
                    }
                }
                if (chiave == "esposizione")
                {
                    switch (valore)
                    {
                        case "1000":
                            titoloRC = "Clienti Esposizione sotto €1.000";
                            break;
                        case "5000":
                            titoloRC = "Clienti Esposizione tra €1.000 e €5.000";
                            break;
                        case "25000":
                            titoloRC = "Clienti Esposizione tra €5.000 e €25.000";
                            break;
                        case "100000":
                            titoloRC = "Clienti Esposizione tra €25.000 e €100.000";
                            break;
                        case "999":
                            titoloRC = "Clienti Esposizione oltre €100.000";
                            break;                        
                    }
                }
                
            }

            ViewBag.TitoloRC = titoloRC;
            
            
            return View("RecuperoCrediti");
        }
        [Authorize]        
        public ActionResult PosizionamentoAziendale()
        {
            if (loggeduser == null)
            {
                RedirectToAction("Index", "Home");
            }
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
            return View("PosizionamentoAziendale");
        }
        [Authorize]        
        public ActionResult PortafoglioClienti()
        {
            if (loggeduser == null)
            {
                RedirectToAction("Index", "Home");
            }
            ViewBag.IdUser = loggeduser.IdUser;            
            ViewBag.ProvinceItaliane = ProvinceItaliane;
            ViewBag.Username = loggeduser.Username;
            ViewBag.Azienda = loggeduser.Azienda;
            ViewBag.Mercato = loggeduser.Mercato;
            ViewBag.Ambiente = loggeduser.Ambiente;
            ViewBag.IdUser = loggeduser.IdUser;
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
            return View("PortafoglioClienti");
        }

        
        [Authorize]
        public ActionResult MonitoraggioPreferiti()
        {
            if (loggeduser == null)
            {
                RedirectToAction("Index", "Home");
            }
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
            ViewBag.NumeroMonitoraggiReport = NumeroMonitoraggiReport;
            ViewBag.VerificaSuperamentoSogliaReport = VerificaSuperamentoSogliaReport;
            ViewBag.RicercaAbilitata = loggeduser.AbilitatoRicerca;
            if (Request.Browser.IsMobileDevice)
            {
                List<ElementoPreferiti> preferiti = DBHandler.GetPreferitiMonitoraggio(loggeduser.IdUser, loggeduser.IdRuolo, loggeduser.IdMercato, loggeduser.Ambiente);
                //preferiti = preferiti.OrderByDescending(d => d.Esposizione).Take(10).ToList();
                ViewBag.Preferiti = preferiti;
            }
            return View();
        }

        [Authorize]
        public ExcelResult GetMonitoraggiXLS()
        {

            List<ElementoPreferiti> preferiti = DBHandler.GetPreferitiMonitoraggio(loggeduser.IdUser, loggeduser.IdRuolo, loggeduser.IdMercato, loggeduser.Ambiente);
            preferiti = preferiti.OrderByDescending(or => or.DataVariazione).ToList();
            ExcelResult report = new ExcelResult(preferiti, "Monitoraggi.xls");
            return report;

        }

        [Authorize]
        public JsonResult GetPreferitiMonitoraggio()
        {
            /*Ricevo parametri da chiamata POST AJAX*/
            string sidx = String.IsNullOrEmpty(Request.Form["sidx"]) ? "Esposizione" : Request.Form["sidx"];
            string sord = Request.Form["sord"];
            int rows = Convert.ToInt32(Request.Form["rows"]);
            int page = Convert.ToInt32(Request.Form["page"]);
            string searchField = !String.IsNullOrEmpty(Request.Form["searchField"]) ? Request.Form["searchField"] : String.Empty;
            string searchString = !String.IsNullOrEmpty(Request.Form["searchString"]) ? Request.Form["searchString"] : String.Empty;
            searchString = searchString.ToUpper();

            /*Lista preferiti per Operatore Loggato*/
            List<ElementoPreferiti> preferiti = DBHandler.GetPreferitiMonitoraggio(loggeduser.IdUser, loggeduser.IdRuolo, loggeduser.IdMercato, loggeduser.Ambiente);

            int recordTotali = preferiti.Count;
            int pagineTotali = recordTotali / rows;


            if (searchField != String.Empty)
            {
                switch (searchField)
                {
                    case "id":
                        /*preferiti = preferiti.Where(p => p.PartitaIva.Contains(searchString)).ToList();*/
                        preferiti = preferiti.Where(p => p.PartitaIva == searchString).ToList();
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
                    case "Fido":
                        preferiti = preferiti.OrderByDescending(ord => ord.Fido).ToList();
                        break;
                    case "Rating":
                        preferiti = preferiti.OrderByDescending(ord => ord.Rating).ToList();
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
                    case "Fido":
                        preferiti = preferiti.OrderBy(ord => ord.Fido).ToList();
                        break;
                    case "Rating":
                        preferiti = preferiti.OrderBy(ord => ord.Rating).ToList();
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

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult GetNoteAccredito()
        {
            string piva = Request.Form["clientePiva"] != null ? Request.Form["clientePiva"] : "";
            var deserializer = new JavaScriptSerializer();
            List<ElementoInserimentoFatturaRecuperoCrediti> fatture = deserializer.Deserialize<List<ElementoInserimentoFatturaRecuperoCrediti>>(Request.Form["fatture"]);

            List<DateTime> dateFatture = fatture.Select(sel => DateTime.Parse(sel.DataDocumento)).ToList();

            DateTime dataPrima = dateFatture.OrderBy(o => o).FirstOrDefault().Date;


            List<ElementoFatturaRecuperoCrediti> listaRc = DBHandler.GetNoteCredito(loggeduser.IdUser, piva, loggeduser.Ambiente, dataPrima);
            return Json(listaRc, JsonRequestBehavior.DenyGet);
        }
        
        [Authorize]        
        public JsonResult GetFatture()
        {
            /*Ricevo parametri da chiamata POST AJAX*/
            string sidx = String.IsNullOrEmpty(Request.Form["sidx"]) ? "Esposizione" : Request.Form["sidx"];
            string sord = Request.Form["sord"];
            int rows = Convert.ToInt32(Request.Form["rows"]);
            int page = Convert.ToInt32(Request.Form["page"]);
            string piva = Request.Form["piva"] != null ? Request.Form["piva"] : "";
            Decimal esposizione = 0;
            Decimal.TryParse(Request.Form["esp"],out esposizione);
            string searchField = !String.IsNullOrEmpty(Request.Form["searchField"]) ? Request.Form["searchField"] : String.Empty;
            string searchString = !String.IsNullOrEmpty(Request.Form["searchString"]) ? Request.Form["searchString"] : String.Empty;
            searchString = searchString.ToUpper();



            List<ElementoFatturaRecuperoCrediti> listaRc = DBHandler.GetFattureRecuperoCrediti(loggeduser.IdUser, piva, loggeduser.Ambiente);

            if (searchField != String.Empty)
            {
                switch (searchField)
                {
                    case "NDoc":
                        listaRc = listaRc.Where(p => p.NDoc.Contains(searchString)).ToList();
                        break;
             
                }
            }

            /*Ordino Aziende Recupero Crediti*/

            if (sord == "desc")
            {
                switch (sidx)
                {
                    case "NDoc":
                        listaRc = listaRc.OrderByDescending(ord => ord.NDoc).ToList();
                        break;
                    case "DataFattura":
                        listaRc = listaRc.OrderByDescending(ord => ord.DataFattura).ToList();
                        break;

                    case "DataScadenza":
                        listaRc = listaRc.OrderByDescending(ord => ord.DataScadenza).ToList();
                        break;
                    case "Importo":
                        listaRc = listaRc.OrderByDescending(ord => ord.Importo).ToList();
                        break;                        
                    default:
                        listaRc = listaRc.OrderByDescending(ord => ord.DataFattura).ToList();
                        break;
                }
            }
            else
            {
                switch (sidx)
                {
                    case "NDoc":
                        listaRc = listaRc.OrderBy(ord => ord.NDoc).ToList();
                        break;
                    case "DataFattura":
                        listaRc = listaRc.OrderBy(ord => ord.DataFattura).ToList();
                        break;

                    case "DataScadenza":
                        listaRc = listaRc.OrderBy(ord => ord.DataScadenza).ToList();
                        break;
                    case "Importo":
                        listaRc = listaRc.OrderBy(ord => ord.Importo).ToList();
                        break;
                    default:
                        listaRc = listaRc.OrderBy(ord => ord.DataFattura).ToList();
                        break;
                }
            }

            int recordTotali = listaRc.Count;
            int pagineTotali = recordTotali / rows;

            /*NB LIMITO FATTURE A 18 MESI*/
            listaRc.Where(o => o.DataFattura >= DateTime.Now.AddMonths(-18));

            /*Pagino Aziende Recupero Crediti ottenute*/
            listaRc = listaRc.Skip((page > 0 ? page - 1 : 0) * rows).Take(rows).ToList();

            /*Formattazione JSON per JqGrid*/
            var result = new { page = page, total = (recordTotali + rows - 1) / rows, records = recordTotali, rows = listaRc };

            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}
