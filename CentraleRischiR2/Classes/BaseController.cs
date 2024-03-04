using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CentraleRischiR2Library;
using CentraleRischiR2Library.BridgeClasses;

namespace CentraleRischiR2.Classes
{
    public class BaseController : Controller
    {
        protected NavigationUser loggeduser
        {
            get
            {
                return (NavigationUser)Session["LoggedUser"];
            }
        }

        protected int NumeroMonitoraggiReport
        {
            get
            {
                return DBHandler.NumeroMonitoraggiReport(loggeduser.IdUser);
            }

        }

        protected int NumeroMonitoraggiReportDisponibili
        {
            get
            {
                return DBHandler.NumeroMonitoraggiReportDisponibili(loggeduser.IdUser, loggeduser.Ambiente);
            }

        }

        protected bool VerificaSuperamentoSogliaReport
        {
            get
            {
                return DBHandler.VerificaSuperamentoSogliaReport(loggeduser.IdUser, loggeduser.IdRuolo, loggeduser.Ambiente);
            }

        }
        protected SelectList ProvinceItaliane{
            get{
                var dictionaryProvince = DBHandler.ElencoProvince();
                dictionaryProvince.Add("Key", "Value");
                return new SelectList(dictionaryProvince, "Key", "Value");
            }
        }

        

        [Authorize]

        public ActionResult GetFattureDettaglioInvioRC()
        {
            /*Ricevo parametri da chiamata POST AJAX*/

            long idRichiesta = Convert.ToInt32(Request.Form["identRichiesta"]);


            /*Lista Aziende Recupero Crediti per Operatore Loggato*/
            List<ElementoFatturaRecuperoCrediti> listaRc = DBHandler.GetFattureDettaglioInvioRC(idRichiesta);

            string strReturn = "<table style='border:0px;width:100%'>";
            strReturn += "<tr><td style='text-align:left'><b>Data Documento</b></td><td style='text-align:left'><b>Data Scadenza</b></td><td style='text-align:left'><b>N. Doc.</b></td><td style='text-align:left'><b>Importo</b></td></tr>";
            foreach (ElementoFatturaRecuperoCrediti fattura in listaRc)
            {
                strReturn += String.Format("<tr><td style='text-align:left'>{0:dd/MM/yyyy}</td><td style='text-align:left'>{1:dd/MM/yyyy}</td><td style='text-align:left'>{2}</td><td style='text-align:left'>{3}</td></tr>", fattura.DataFattura, fattura.DataScadenza, fattura.NDoc, fattura.ImportoDecimal);
            }
            strReturn += "</table>";

            return Content(strReturn);
        }

        [Authorize]
        public JsonResult GetRichiesteRecuperoCrediti()
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
            List<ElementoAziendaRecuperoCrediti> listaRc = DBHandler.GetRichiesteRecuperoCreditiHome(loggeduser.IdUser, loggeduser.IdRuolo, loggeduser.IdMercato, loggeduser.Ambiente);

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
                    case "SommaIncasso":
                        listaRc = listaRc.OrderByDescending(ord => ord.SommaIncasso).ToList();
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
                    case "SommaIncasso":
                        listaRc = listaRc.OrderByDescending(ord => ord.SommaIncasso).ToList();
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
        public virtual JsonResult ElencoOperatoriCentro(int idCentro)
        {
            List<vs_aziende_osservamercati> returnValue = new List<vs_aziende_osservamercati>();
            returnValue = DBHandler.ElencoOperatoriCentro(idCentro);
            return Json(returnValue, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult GetAnagraficaPivaOsservatorioAC(string query = "")
        {
            List<ElementoAnagraficheRicerca> returnValue = new List<ElementoAnagraficheRicerca>();

            returnValue = DBHandler.GetAnagraficaOsservatorioAC(query, "");

            return Json(returnValue, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult GetAnagraficaRagSocOsservatorioAC(string query = "")
        {
            List<ElementoAnagraficheRicerca> returnValue = new List<ElementoAnagraficheRicerca>();

            returnValue = DBHandler.GetAnagraficaOsservatorioAC("", query);

            return Json(returnValue, JsonRequestBehavior.AllowGet);
        }
        
    }
}
