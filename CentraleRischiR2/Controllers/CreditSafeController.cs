using CentraleRischiR2.Classes;
using CentraleRischiR2.Models;
using CentraleRischiR2Library;
using CentraleRischiR2Library.BridgeClasses;
using Microsoft.Office.Interop.Excel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Configuration;
using System.Web.Mvc;
using ClosedXML.Excel;
using System.Web;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using log4net;
using System.Net.Http;
using System.Diagnostics;
using System.Text;

namespace CentraleRischiR2.Controllers
{
    public class CreditSafeController : BaseController
    {

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private List<ElementoAnagraficheReport> anagCS;

        

        [Authorize]
        public JsonResult GetAnagraficheCS(string rs, string piva)
        {
            Log.Info("GetAnagraficheCs Start ----- P.iva : "+piva);
            string sidx = String.IsNullOrEmpty(Request.Form["sidx"]) ? "Esposizione" : Request.Form["sidx"];
            string sord = Request.Form["sord"];
            int rows = Convert.ToInt32(Request.Form["rows"]);
            int page = Convert.ToInt32(Request.Form["page"]);

            if (rs != String.Empty || piva != String.Empty)
            {
                //Log.Debug("GetAnagCS  piva=" + piva + " rs=" + rs + " IdUser=" + loggeduser.IdUser + " Ambiente=" + loggeduser.Ambiente + "azienda=" + loggeduser.IdAzienda + " mercato=" + loggeduser.IdMercato);
                anagCS = GetAnagCS(piva, rs, loggeduser.IdUser, loggeduser.Ambiente);
            }

            int recordTotali = anagCS.Count;
            int pagineTotali = recordTotali / rows;


            /*Ordino anagrafiche*/

            if (sord == "desc")
            {

                switch (sidx)
                {
                    case "id":
                        anagCS = anagCS.OrderByDescending(ord => ord.PartitaIva).ToList();
                        break;
                    case "RagioneSociale":
                        anagCS = anagCS.OrderByDescending(ord => ord.RagioneSociale).ToList();
                        break;
                    case "PartitaIva":
                        anagCS = anagCS.OrderByDescending(ord => ord.PartitaIva).ToList();
                        break;
                    case "Provincia":
                        anagCS = anagCS.OrderByDescending(ord => ord.Provincia).ToList();
                        break;
                    case "Indirizzo":
                        anagCS = anagCS.OrderByDescending(ord => ord.Indirizzo).ToList();
                        break;
                    case "Rating":
                        anagCS = anagCS.OrderByDescending(ord => ord.Rating).ToList();
                        break;
                    case "PEC":
                        anagCS = anagCS.OrderByDescending(ord => ord.PEC).ToList();
                        break;
                    default:
                        anagCS = anagCS.OrderByDescending(ord => ord.RagioneSociale).ToList();
                        break;
                }
            }
            else
            {
                switch (sidx)
                {
                    case "id":
                        anagCS = anagCS.OrderBy(ord => ord.PartitaIva).ToList();
                        break;
                    case "RagioneSociale":
                        anagCS = anagCS.OrderBy(ord => ord.RagioneSociale).ToList();
                        break;
                    case "PartitaIva":
                        anagCS = anagCS.OrderBy(ord => ord.PartitaIva).ToList();
                        break;
                    case "Provincia":
                        anagCS = anagCS.OrderBy(ord => ord.Provincia).ToList();
                        break;
                    case "Indirizzo":
                        anagCS = anagCS.OrderBy(ord => ord.Indirizzo).ToList();
                        break;
                    case "Rating":
                        anagCS = anagCS.OrderBy(ord => ord.Rating).ToList();
                        break;
                    case "PEC":
                        anagCS = anagCS.OrderBy(ord => ord.PEC).ToList();
                        break;
                    default:
                        anagCS = anagCS.OrderBy(ord => ord.RagioneSociale).ToList();
                        break;
                }
            }

            /*Pagino i preferiti ottenuti*/
            var result = new { page = 1, total = 1, records = anagCS, rows = anagCS };

            /*Formattazione JSON per JqGrid*/
            Log.Info("end GetAnagraficheCS");
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public List<ElementoAnagraficheReport> GetAnagCS(string partivaIva, string ragioneSociale, int idUtente, string ambiente)
        {
            Log.Info("GetAnagCs start ---- idUtente : " + idUtente.ToString());
            List<ElementoAnagraficheReport> returnValue = new List<ElementoAnagraficheReport>();
            List<ElementoAnagraficheReport> anagraficheInterne = new List<ElementoAnagraficheReport>();
            List<ElementoAnagraficheReport> anagraficheEsterne = new List<ElementoAnagraficheReport>();
            try
            {
                using (DemoR2Entities context = new DemoR2Entities())
                {
                    ElementoOperatore aziendaUtente = GetAziendaUtente(idUtente, context, ambiente);
                    List<CompanyCS> anagCS = SearchCompany(partivaIva, ragioneSociale);
                    Log.Debug("anagCs count " + anagCS.Count);

                    string partitaivabella = context.news_operatore.Select(a => a.rapporto).FirstOrDefault();
                    if (anagCS.Count > 0 && !partivaIva.Equals("01704320595"))
                    {
                        anagraficheEsterne = anagCS.Select
                        (an => new ElementoAnagraficheReport
                        {
                            PartitaIva = an.vatNo != String.Empty ? an.vatNo : "",
                            EventiNegativi =
                                ""
                                ,
                            Fido = 0,
                            RagioneSociale = an.name != null ? an.name : "",
                            Indirizzo = an.address.street != String.Empty ? an.address.street : "",
                            Provincia = an.address.province != String.Empty ? an.address.province : "",
                            id = an.id != String.Empty ? an.id : "",
                            RatingDescrizione = ""
                                ,
                            Rapporto = an.rapporto != "" ? an.rapporto : "",
                            Rapportonew = context.news_operatore.Where(op => op.partita_iva.Equals(an.vatNo))
                                                                .Select(sel => sel.aggiornamento).FirstOrDefault() == true
                                                                 ? "1" : "0",
                            Rating = Convert.ToString(an.valutazione),
                            Osservatorio = ""
                            ,
                            PEC = "",
                            StatoAttivita = an.status != null ? an.status : ""
                        }
                        )
                        .ToList();
                    }
                    List<m02_anagrafica> anagraficaInterna = new List<m02_anagrafica>();
                    if (partivaIva != String.Empty)
                    {
                        anagraficaInterna = context.m02_anagrafica.Where(
                                p => p.m02_partitaiva.Contains(partivaIva) && p.m02_stato_validazione_cerved == 1).ToList();
                    }
                    else
                    {
                        anagraficaInterna = context.m02_anagrafica.Where(
                                p => p.m02_ragionesociale.Contains(ragioneSociale) && p.m02_stato_validazione_cerved == 1).ToList();
                    }
                    if (anagraficaInterna.Count > 0)
                    {
                        anagraficheInterne =
                             anagraficaInterna.Select
                             (an => new ElementoAnagraficheReport
                             {
                                 StatoAttivita = an.m02_stato_attivita,
                                 PartitaIva = an.m02_partitaiva,
                                 EventiNegativi =
                                     context.m02_anagrafica.Where(anag => anag.m02_partitaiva == an.m02_ragionesociale).FirstOrDefault() != null ?
                                     context.m02_anagrafica.Where(anag => anag.m02_partitaiva == an.m02_ragionesociale).FirstOrDefault().m02_eventi_negativi : "",
                                 Fido = context.m02_anagrafica.Where(ana => ana.m02_partitaiva == an.m02_partitaiva).FirstOrDefault().m02_fido.Value,
                                 RagioneSociale = an.m02_ragionesociale,
                                 Indirizzo = an.m02_indirizzo,
                                 Provincia = an.m02_prefisso,
                                 Rating = context.m05_rating
                 .Where(rat => rat.m05_partitaiva == an.m02_partitaiva && (rat.m05_dtriferimento <= DateTime.Now && rat.m05_dtfinevalidita >= DateTime.Now)).OrderByDescending(rating => rating.m05_dtriferimento).FirstOrDefault() != null ?
                     context.m05_rating
                 .Where(rat => rat.m05_partitaiva == an.m02_partitaiva && (rat.m05_dtriferimento <= DateTime.Now && rat.m05_dtfinevalidita >= DateTime.Now)).OrderByDescending(rating => rating.m05_dtriferimento).FirstOrDefault().m05_stato : "",
                                 RatingDescrizione = context.m05_rating
                 .Where(rat => rat.m05_partitaiva == an.m02_partitaiva && (rat.m05_dtriferimento <= DateTime.Now && rat.m05_dtfinevalidita >= DateTime.Now)).FirstOrDefault() != null ?
                     context.m05_rating
                 .Where(rat => rat.m05_partitaiva == an.m02_partitaiva && (rat.m05_dtriferimento <= DateTime.Now && rat.m05_dtfinevalidita >= DateTime.Now)).FirstOrDefault().m05_stato : ""
                                 ,
                                 Rapporto = context.news_operatore.Where(a => a.partita_iva == an.m02_partitaiva).FirstOrDefault() != null ?
                                    context.news_operatore.Where(a => a.partita_iva == an.m02_partitaiva).FirstOrDefault().rapporto : "",
                                 Osservatorio = context.m02_anagrafica.Where(anag => anag.m02_partitaiva == an.m02_partitaiva)
                             .Join(context.m04_vendite.Where(v => v.m04_partitaiva == an.m02_partitaiva && v.m04_codiceazienda == aziendaUtente.CodiceAzienda),
                             a => a.m02_partitaiva,
                             b => b.m04_partitaiva,
                             (a, b) => a).FirstOrDefault() != null ?
                             an.m02_partitaiva : "",
                                 PEC =
                                 context.m02_anagrafica.Where(anag => anag.m02_partitaiva == an.m02_ragionesociale).FirstOrDefault() != null ?
                                 context.m02_anagrafica.Where(anag => anag.m02_partitaiva == an.m02_ragionesociale).FirstOrDefault().m02_pec : ""
                             }
                             ).ToList();
                    }
                    returnValue.AddRange(anagraficheInterne);
                    returnValue.AddRange(anagraficheEsterne.Where(anE => !anagraficheInterne.Any(anI => anI.PartitaIva == anE.PartitaIva)));
                }
            }
            catch (Exception e)
            {
                Log.Error("Error : " + e.ToString());
            }
            Log.Info("end GetAnagCS");
            return returnValue;
        }

        [Obsolete]
        private List<CompanyCS> SearchCompany(string partivaIva, string ragioneSociale)
        {
            Log.Info("begin SearchCompany filtri : partita Iva= " + partivaIva + " Ragione sociale= " + ragioneSociale);
            string token = DBHandler.LoginCS(SecurityProtocolType.Tls12);
            return CompaniesListCS(token, partivaIva, ragioneSociale);
        }


        private List<CompanyCS> CompaniesListCS(string token, string piva, string ragioneSociale)
        {
            Log.Info(" CompaniesListCS start ----- P.iva : "+piva);
            List<CompanyCS> retVal = new List<CompanyCS>();
            string rapporto = "";
            string dataRapporto = "";
            string nation =  "IT";
            if(!piva.Equals("") && piva.Length < 15)
            {
                if (!piva.Substring(0, 1).Any(char.IsDigit))
                {
                    nation = piva.Substring(0, 2);
                    //piva = piva.Substring(2);
                }
            }
            try
            {
                string URI = "https://connect.creditsafe.com/v1/companies";
                URI = URI + "?countries=" +nation;
                if (!piva.Equals(""))
                {
                    URI = URI + "&vatNo=" + piva.ToString();
                }
                if (!ragioneSociale.Equals(""))
                {
                    URI = URI + "&name=" + ragioneSociale.ToString();
                }
                Log.Info("URI " + URI);
                URI = URI + "&pageSize=200&page=1";
                var httpWebRequest2 = (HttpWebRequest)WebRequest.Create(URI);
                httpWebRequest2.ContentType = "application/json";
                httpWebRequest2.Method = "GET";
                httpWebRequest2.Headers.Add("Authorization", token);
                var httpResponse2 = (HttpWebResponse)httpWebRequest2.GetResponse();
                IQueryable<news_operatore> newsPreferiti = Enumerable.Empty<news_operatore>().AsQueryable();
                using (var streamReader = new StreamReader(httpResponse2.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    dynamic response = JsonConvert.DeserializeObject(result.ToString());

                    foreach (dynamic element in response.companies)
                    {
                        CompanyCS companyCS = new CompanyCS();
                        Addressview address = new Addressview();
                        try
                        {
                            for (int i = 0; i <= 50; i++)
                            {
                                string pivalist = element.vatNo[i];
                                if (piva.Contains(piva))
                                {
                                    companyCS.vatNo = element.vatNo[i];
                                    break;
                                }
                            }
                        }
                        catch { companyCS.vatNo = "NotFound"; }

                        address.street = element.address.street;
                        address.province = element.address.province;
                        companyCS.address = address;
                        companyCS.status = element.status;
                        companyCS.rapporto = "";
                        using (DemoR2Entities context = new DemoR2Entities())
                        {
                            try
                            {
                                companyCS.rapporto = context.m02_anagrafica
                                                     .Where(a => a.m02_partitaiva.Equals(companyCS.vatNo))
                                                     .Select(b => b.m02_note)
                                                     .FirstOrDefault();

                            }
                            catch { companyCS.rapporto = ""; }
                            if (companyCS.rapporto == null)
                            {
                                companyCS.rapporto = "";
                            }
                            try
                            {
                                companyCS.valutazione = Convert.ToInt16(context.m02_anagrafica
                                                .Where(_ => _.m02_partitaiva == companyCS.vatNo)
                                                .Select(_ => _.m02_stato_validazione_cerved).FirstOrDefault());
                            }
                            catch { companyCS.valutazione = 0; }
                        }
                        //valutazione se il rapporto pdf sia effettivamente l'ultimo vado a controllare se ci sono state news report dopo la data del rapporto
                        //se ci sono lo metto come lente di ingrandimento cosi da poterlo scaricare altrimento con icona pdf perche è il piu aggiornato
                        string rapportoAnag = rapporto.Trim();

                        if (rapportoAnag != "")
                        {
                            List<string> listarapporti = new List<string>();
                            int subend = rapportoAnag.IndexOf("_");
                            dataRapporto = rapportoAnag.Substring(subend + 1, 8);
                            dataRapporto = dataRapporto.Substring(0, 4) + "/" + dataRapporto.Substring(4, 2) + "/" + dataRapporto.Substring(6, 2);

                            Boolean resultDate = false;
                            using (DemoR2Entities context = new DemoR2Entities())
                            {
                                Log.Debug("*data rapporto* " + dataRapporto + " partita iva=" + companyCS.vatNo);
                                listarapporti = context.news_operatore.Where(_ => _.partita_iva == companyCS.vatNo && _.rapporto != "").Select(sel => sel.rapporto).ToList();
                                Log.Debug("*listaa news operatore* " + listarapporti);
                                foreach (string el in listarapporti)
                                {
                                    Log.Debug("* el *" + el);
                                    string dataNewsOp = el.Substring(subend + 1, 8);
                                    dataNewsOp = dataNewsOp.Substring(0, 4) + "/" + dataNewsOp.Substring(4, 2) + "/" + dataNewsOp.Substring(6, 2);
                                    Log.Debug("* if data rapporto con elemento news* " + dataRapporto + " elemento news= " + el.Substring(subend + 1, 8));

                                    if (DateTime.Compare(DateTime.Parse(dataRapporto), DateTime.Parse(dataNewsOp)) < 0)
                                    {
                                        resultDate = true;
                                    }
                                }
                            }
                            if (resultDate)
                            {
                                companyCS.rapporto = rapporto;
                            }
                        }
                        companyCS.name = element.name;
                        companyCS.id = element.id;
                        retVal.Add(companyCS);
                    }
                }
            }
            catch (WebException ex)
            {
                using (WebResponse response = ex.Response)
                {
                    HttpWebResponse httpResponse2 = (HttpWebResponse)response;
                    using (Stream data = response.GetResponseStream())
                    using (var reader = new StreamReader(data))
                    {
                        // text is the response body
                        string text = reader.ReadToEnd();
                    }
                }
            }
            return retVal;
        }



        public ElementoOperatore GetAziendaUtente(long idUtente, DemoR2Entities context, string ambiente)
        {
            bool contextToBeClosed = false;
            if (context == null)
            {
                context = new DemoR2Entities();
                contextToBeClosed = true;
            }
            ElementoOperatore returnValue = new ElementoOperatore();
            if (ambiente == "STANDARD")
            {
                returnValue = context.vs_aziende_osservamercati.Join(context.m10_utenti.Where(id => id.m10_iduser == idUtente),
                                                            a => a.m01_idazienda,
                                                            b => b.m10_idazienda,
                                                            (a, b) => a)
                                                            .Select(an => new ElementoOperatore { Id = an.m01_idazienda, IdCentro = an.m01_idcentro.Value, RagioneSociale = an.m01_ragionesociale, Ambiente = ambiente, CodiceAzienda = an.m01_codice, SogliaReport = an.m01_soglia_report.Value, CodiceFinservice = an.m01_codice_rc, CodicePaylineCerved = an.m01_codice_payline_cerved }).FirstOrDefault();
            }
            else
            {
                returnValue = context.vs_aziende_osservacrediti.Join(context.m10_utenti.Where(id => id.m10_iduser == idUtente),
                                                            a => a.m01_idazienda,
                                                            b => b.m10_idazienda,
                                                            (a, b) => a)
                                                            .Select(an => new ElementoOperatore { Id = an.m01_idazienda, IdCentro = an.m01_idcentro.Value, RagioneSociale = an.m01_ragionesociale, Ambiente = ambiente, CodiceAzienda = an.m01_codice, SogliaReport = an.m01_soglia_report.Value, CodiceFinservice = an.m01_codice_rc, CodicePaylineCerved = an.m01_codice_payline_cerved }).FirstOrDefault();
            }
            if (contextToBeClosed)
            {
                context = null;
            }
            return returnValue;
        }

        [Obsolete]
        public ActionResult GetGlobalReportDirect(string piva)
        {
            Log.Info("GetGlobalReportDirect Start ----- P.iva : " + piva);
            ActionResult returnView = RedirectToAction("Index", "Home");
            string partitaIva = piva;
            string partitaIva2 = Request.Form["piva"];

            string filePath = WebConfigurationManager.AppSettings["ReportPath"];
            string dataoggi = DateTime.Now.ToString("yyyyMMdd");
            string extention = ".pdf";
            string fileName = partitaIva + "_" + dataoggi + extention;
            string token = "";
            string idCompany = "";
            bool newCO = false;
            string idPortfoglio = "";
            string mercato = "";
            string json = "";
            //Log.Debug("partita iva ********* = " + partitaIva);

            string tipoReport = "nuovaRichiesta";
            Log.Info("begin GlobalReport partita iva= " + partitaIva + " utente = " + loggeduser.IdUser);
            try
            {

                /*prendo il token*/
                token = DBHandler.LoginCS(SecurityProtocolType.Tls12);

                if (partitaIva.Contains("-"))
                {
                    idCompany = partitaIva;
                }
                else
                {
                    /*prendo idcompany*/
                    idCompany = CentraleRischiR2Library.DBHandler.GetIdGlobalReport(partitaIva, token);
                }
                Log.Debug("id company = " + idCompany);

                /*lettura api*/
                json = CentraleRischiR2Library.DBHandler.CompanyComplete(idCompany, token);
                dynamic jsondy = JsonConvert.DeserializeObject(json.ToString());
                fileName = jsondy.report.alternateSummary.vatRegistrationNumber + "_" + dataoggi + extention;

                /* pdf report  */
                var httpResponse = (HttpWebResponse)CentraleRischiR2Library.DBHandler.CompanyReportPdf(idCompany, token);
                newCO = true;
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    Encoding objEncoding = Encoding.Default;
                    StreamReader objSR = new StreamReader(httpResponse.GetResponseStream(), objEncoding);
                    StreamWriter objSW = new StreamWriter(filePath + fileName, false, objEncoding);
                    objSW.Write(objSR.ReadToEnd());
                    objSW.Close();
                    objSR.Close();

                    var fileStream = new FileStream((filePath + fileName), FileMode.Open);

                    var memoryStream = new MemoryStream();
                    fileStream.CopyTo(memoryStream);
                    fileStream.Close();
                    memoryStream.Position = 0;

                    /*if (!DBHandler.Insertreport(partitaIva, loggeduser.IdUser, JsonConvert.DeserializeObject(json.ToString()),fileName))
                         {
                             Log.Error("error insert report: ");
                    
                    }*/
                    string ragioneSociale = "";
                    string globalScore = "";
                    Int16 globalScoreInt = 0;
                    string pec = "";
                    string eventiNegativi = "";
                    decimal fido = 50m;
                    string fidoStr = "";
                    string statoAttivita = "";
                    dynamic jsonDeserialize = JsonConvert.DeserializeObject(json.ToString());
                    try { fidoStr = (string)jsonDeserialize.report.creditScore.currentCreditRating.creditLimit.value; } catch { fidoStr = ""; }
                    fidoStr = fidoStr.Replace(".", ",");
                    fidoStr = fidoStr == "ND" ? "0" : fidoStr;
                    fido = Convert.ToDecimal(fidoStr);
                    try { statoAttivita = (string)jsonDeserialize.report.companySummary.companyStatus.status; } catch { statoAttivita = "non presente"; }
                    try { ragioneSociale = (string)jsonDeserialize.report.alternateSummary.businessName; } catch { ragioneSociale = "ragione sociale non presente"; }
                    try { globalScore = (string)jsonDeserialize.report.creditScore.currentCreditRating.providerValue.value;}catch{globalScore = "0";}
                    try { pec = (string)jsonDeserialize.report.contactInformation.emailAddresses[0]; } catch { pec = "pec non presente"; }
                    try { globalScoreInt = Int16.Parse(globalScore); } catch { }


                    /*upd anag exists*/
                    if (!DBHandler.AggiornaAnagraficaFidoEventiNegativiPecStato(piva, ragioneSociale, globalScoreInt, fido, eventiNegativi, pec, statoAttivita, fileName))
                    {
                        /*cre anag*/
                        DBHandler.InsertAnagrafica(JsonConvert.DeserializeObject(json.ToString()), piva, pec, fileName);
                        /*upd anag*/
                        DBHandler.AggiornaAnagraficaFidoEventiNegativiPecStato(piva, ragioneSociale, globalScoreInt, fido, eventiNegativi, pec, statoAttivita, fileName);
                    }
                    DBHandler.InserisciRichiesteReportFornitore(partitaIva, loggeduser.IdUser, loggeduser.Email, DateTime.Now, DateTime.Now.AddMonths(12), tipoReport, DBHandler.GetCentro(loggeduser.IdUser));
                    mercato = DBHandler.GetMercato(DBHandler.GetCentro(loggeduser.IdUser).IdMercato).Mercato;
                    idPortfoglio = PortfoglioCs.GetIdPortfoglio(token, mercato);
                    if (CentraleRischiR2Library.PortfoglioCs.InsertCOMonitoraggio(token, idPortfoglio, idCompany, partitaIva))
                    {
                        Log.Info("Azienda " + piva + " inserita correttamente nel portfolio di " + idPortfoglio);
                    }
                    else
                    {
                        Log.Info("Azienda gia presente nel portfolio " + idPortfoglio);
                    }
                    DBHandler.InserisciChekMonitoraggio(loggeduser.IdUser, partitaIva, DateTime.Now, DateTime.Now);
                    using (DemoR2Entities context = new DemoR2Entities())
                    {
                        List<news_operatore> aggiornamentoList = context.news_operatore.Where(wr => wr.partita_iva.Equals(partitaIva)).ToList();
                        foreach (news_operatore el in aggiornamentoList)
                        {
                            el.aggiornamento = true;
                            context.SaveChanges();
                        }
                    }
                    if (newCO)
                    {
                        Log.Info("end GlobalReport nuova azienda** " + "user = " + loggeduser.IdUser);
                    }
                    else
                    {
                        Log.Info("end GlobalReport azienda gia in Monitoraggio** " + "user = " + loggeduser.IdUser);
                    }
                    return File(memoryStream, "application/pdf", fileName);
                }
            }
            catch (Exception ex)
            {
                Log.Error("errore : " + ex.ToString());
                MailHandler.SendMail("support@osservamercati.it", "GetGlobalReportDirect " + ex.ToString());
                return RedirectToAction("Index", "Home");
            }
        }



        [System.Web.Services.WebMethod()]
        [Authorize]
        [HttpPost]
        /*public ActionResult GetGlobalReport()
        {
            string partitaIva = Request.Form["piva"];
            string filePath = WebConfigurationManager.AppSettings["ReportPath"];
            string dataoggi = DateTime.Now.ToString("yyyyMMdd");
            string extention = ".pdf";
            string fileName = partitaIva + "_" + dataoggi + extention;
            string token = "";
            string idCompany = "";
            bool newCO = false;
            string idPortfoglio = "";
            string mercato = "";
            string json = "";

            string tipoReport ="richiestaGiaEffettuata"; //sono i report che sono gia presenti nella pancia di Osserva 
            if (!System.IO.File.Exists(filePath + fileName))
            {
                tipoReport = "nuovaRichiesta";
                Log.Info("START GlobalReport partita iva= " + partitaIva +" utente = "+loggeduser.IdUser);
                try
                {
                    token = CentraleRischiR2Library.DBHandler.LoginCS(SecurityProtocolType.Tls12);
                    idCompany = CentraleRischiR2Library.DBHandler.GetIdGlobalReport(partitaIva, token);

                    //deserializzazione dinamica
                    json = CentraleRischiR2Library.DBHandler.CompanyComplete(idCompany, token);
                    //Log.Debug("global report = " + json.ToString());
                }
                catch (Exception e)
                {
                    Log.Error("error : " + e.ToString());
                }
                newCO = true;
            }
            if (!DBHandler.Insertreport(partitaIva, loggeduser.IdUser, JsonConvert.DeserializeObject(json.ToString()),fileName))
            {
                Log.Error("error insert report: ");
            }
            DBHandler.InserisciRichiesteReportFornitore(partitaIva, loggeduser.IdUser, loggeduser.Email, DateTime.Now, DateTime.Now.AddMonths(12), tipoReport, DBHandler.GetCentro(loggeduser.IdUser));
            mercato = DBHandler.GetMercato(DBHandler.GetCentro(loggeduser.IdUser).IdMercato).Mercato;
            idPortfoglio = PortfoglioCs.GetIdPortfoglio(token, mercato);
            string retVal = CentraleRischiR2Library.PortfoglioCs.InsertCOMonitoraggio(token, idPortfoglio, idCompany, partitaIva);
            Log.Info("inserimento portfolio = " + retVal +" User= "+loggeduser.IdUser);
            DBHandler.InserisciChekMonitoraggio(loggeduser.IdUser, partitaIva, DateTime.Now, DateTime.Now);
            if (newCO)
            {
                Log.Info("end GlobalReport nuova azienda** " + "user = " + loggeduser.IdUser);
            }else
            {
                Log.Info("end GlobalReport azienda gia in Monitoraggio** "+"user = "+loggeduser.IdUser);
            }

            string URI = "https://connect.creditsafe.com/v1/companies/";
            URI = URI + idCompany+"?template=complete";

            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(URI.ToString());
                httpWebRequest.ContentType = "application/pdf";
                httpWebRequest.Method = "GET";
                httpWebRequest.Accept = "application/pdf";
                httpWebRequest.Headers.Add("Authorization", token);

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    Encoding objEncoding = Encoding.Default;
                    StreamReader objSR = new StreamReader(httpResponse.GetResponseStream(), objEncoding);
                    StreamWriter objSW = new StreamWriter(filePath + fileName, false, objEncoding);
                    objSW.Write(objSR.ReadToEnd());
                    objSW.Close();
                    objSR.Close();

                    var fileStream = new FileStream((filePath + fileName), FileMode.Open);

                    var memoryStream = new MemoryStream();
                    fileStream.CopyTo(memoryStream);
                    fileStream.Close();
                    memoryStream.Position = 0;
                    return File(memoryStream, "application/pdf", filePath + fileName);
                }
            }
            catch (Exception ex)
            {
                Log.Error("errore : " + ex.ToString());
                return RedirectToAction("Index", "Home");
            }
        }*/

        public static void OpenAndAddToSpreadsheetStream(Stream stream)
        {
            // Open a SpreadsheetDocument based on a stream.
            SpreadsheetDocument spreadsheetDocument =
                SpreadsheetDocument.Open(stream, true);

            // Add a new worksheet.
            WorksheetPart newWorksheetPart = spreadsheetDocument.WorkbookPart.AddNewPart<WorksheetPart>();
            newWorksheetPart.Worksheet = new DocumentFormat.OpenXml.Spreadsheet.Worksheet(new SheetData());
            newWorksheetPart.Worksheet.Save();

            DocumentFormat.OpenXml.Spreadsheet.Sheets sheets = spreadsheetDocument.WorkbookPart.Workbook.GetFirstChild<DocumentFormat.OpenXml.Spreadsheet.Sheets>();
            string relationshipId = spreadsheetDocument.WorkbookPart.GetIdOfPart(newWorksheetPart);

            // Get a unique ID for the new worksheet.
            uint sheetId = 1;
            if (sheets.Elements<Sheet>().Count() > 0)
            {
                sheetId = sheets.Elements<Sheet>().Select(s => s.SheetId.Value).Max() + 1;
            }

            // Give the new worksheet a name.
            string sheetName = "Sheet" + sheetId;

            // Append the new worksheet and associate it with the workbook.
            Sheet sheet = new Sheet() { Id = relationshipId, SheetId = sheetId, Name = sheetName };
            sheets.Append(sheet);
            spreadsheetDocument.WorkbookPart.Workbook.Save();

            // Close the document handle.
            spreadsheetDocument.Close();

            // Caller must close the stream.
        }

        public ActionResult ReportPdf(string id, string token)
        {
            string URI = "https://connect.creditsafe.com/v1/companies/";
            URI = URI + id;
            string filePath = WebConfigurationManager.AppSettings["ReportPath"];
            string fileName = id;

            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(URI.ToString());
                httpWebRequest.ContentType = "application/pdf";
                httpWebRequest.Method = "GET";
                httpWebRequest.Accept = "application/pdf";
                httpWebRequest.Headers.Add("Authorization", token);

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    Encoding objEncoding = Encoding.Default;
                    StreamReader objSR = new StreamReader(httpResponse.GetResponseStream(), objEncoding);
                    StreamWriter objSW = new StreamWriter(filePath + fileName, false, objEncoding);
                    objSW.Write(objSR.ReadToEnd());
                    objSW.Close();
                    objSR.Close();

                    var fileStream = new FileStream((filePath + fileName), FileMode.Open);

                    // dynamic result = streamReader.ReadToEnd();
                    var memoryStream = new MemoryStream();
                    fileStream.CopyTo(memoryStream);
                    fileStream.Close();
                    memoryStream.Position = 0;
                    return File(memoryStream, "application/pdf", filePath + fileName);
                }

            }
            catch (Exception ex)
            {
                Log.Error("errore : " + ex.ToString());
                MailHandler.SendMail("support@osservamercati.it", ex.ToString());
                return RedirectToAction("Index", "Home");
            }
        }



        public Microsoft.Office.Interop.Excel.Worksheet FormatCellTitle(Microsoft.Office.Interop.Excel.Worksheet worKsheeT, string col, string row, string colFrom, string colTo)
        {
            ((Microsoft.Office.Interop.Excel.Range)worKsheeT.Cells[(colFrom + row), (colTo + row)]).Merge();

            ((Microsoft.Office.Interop.Excel.Range)worKsheeT.Cells[col, row]).BorderAround2();
            ((Microsoft.Office.Interop.Excel.Range)worKsheeT.Cells[col, row]).HorizontalAlignment("center");
            ((Microsoft.Office.Interop.Excel.Range)worKsheeT.Cells[col, row]).Font.Italic = true;
            return worKsheeT;
        }
        public Microsoft.Office.Interop.Excel.Worksheet FormatCellData(Microsoft.Office.Interop.Excel.Worksheet worKsheeT, string col, string row)
        {
            ((Microsoft.Office.Interop.Excel.Range)worKsheeT.Cells[col, row]).BorderAround2();
            ((Microsoft.Office.Interop.Excel.Range)worKsheeT.Cells[col, row]).HorizontalAlignment("center");
            ((Microsoft.Office.Interop.Excel.Range)worKsheeT.Cells[col, row]).Font.Italic = true;
            return worKsheeT;
        }


        public string bonificaNull(string valueString)
        {
            string retval;
            if (!valueString.Equals(""))
            {
                retval = valueString;
            }
            else
            {
                retval = "";
            }
            return retval;
        }

        public System.Data.DataTable ExportToExcel()
        {
            System.Data.DataTable table = new System.Data.DataTable();
            table.Columns.Add("ID", typeof(int));
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("Sex", typeof(string));
            table.Columns.Add("Subject1", typeof(int));
            table.Columns.Add("Subject2", typeof(int));
            table.Columns.Add("Subject3", typeof(int));
            table.Columns.Add("Subject4", typeof(int));
            table.Columns.Add("Subject5", typeof(int));
            table.Columns.Add("Subject6", typeof(int));
            table.Rows.Add(1, "Amar", "M", 78, 59, 72, 95, 83, 77);
            table.Rows.Add(2, "Mohit", "M", 76, 65, 85, 87, 72, 90);
            table.Rows.Add(3, "Garima", "F", 77, 73, 83, 64, 86, 63);
            table.Rows.Add(4, "jyoti", "F", 55, 77, 85, 69, 70, 86);
            table.Rows.Add(5, "Avinash", "M", 87, 73, 69, 75, 67, 81);
            table.Rows.Add(6, "Devesh", "M", 92, 87, 78, 73, 75, 72);
            return table;
        }

     
        

        [Authorize]
        [HttpPost]
        public ViewResult Search()
        {
            if (loggeduser == null)
            {
                RedirectToAction("Index", "Home");
            }
            CentraleRischiR2.Models.Search search = new CentraleRischiR2.Models.Search();

            search.RagioneSociale = Request.Form["RagioneSociale"];
            if (!String.IsNullOrEmpty(Request.QueryString["piva"]))
            {
                search.PartitaIva = Request.QueryString["piva"];
            }
            else
            {
                search.PartitaIva = Request.Form["PartitaIva"];
            }

            search.Provincia = Request.Form["Provincia"];

            ViewBag.Username = loggeduser.Username;
            ViewBag.Azienda = loggeduser.Azienda;
            ViewBag.Mercato = loggeduser.Mercato;
            ViewBag.Ambiente = loggeduser.Ambiente;
            ViewBag.ReportCount = loggeduser.ReportCount;
            ViewBag.ProvinceItaliane = ProvinceItaliane;
            ViewBag.IdRuolo = loggeduser.IdRuolo;
            ViewBag.Demo = loggeduser.Demo;
            ViewBag.NumeroMonitoraggiReportDisponibili = NumeroMonitoraggiReportDisponibili;
            ViewBag.VerificaSuperamentoSogliaReport = VerificaSuperamentoSogliaReport;
            ViewBag.RicercaAbilitata = loggeduser.AbilitatoRicerca;



            return View(search);
        }

        [Authorize]
        [HttpPost]
        public ActionResult SearchMobile()
        {
            if (loggeduser == null)
            {
                RedirectToAction("Index", "Home");
            }

            CentraleRischiR2.Models.Search search = new CentraleRischiR2.Models.Search();

            search.RagioneSociale = Request.Form["RagioneSociale"];
            search.PartitaIva = Request.Form["PartitaIva"];
            search.Provincia = Request.Form["Provincia"];

            ViewBag.Username = loggeduser.Username;
            ViewBag.Azienda = loggeduser.Azienda;
            ViewBag.Mercato = loggeduser.Mercato;
            ViewBag.ProvinceItaliane = ProvinceItaliane;
            ViewBag.Demo = loggeduser.Demo;

            

            return View("Search", search);
        }


    }
}
