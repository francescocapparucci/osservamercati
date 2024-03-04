using CentraleRischiR2.Classes;
using CentraleRischiR2.Models;
using CentraleRischiR2Library;
using CentraleRischiR2Library.BridgeClasses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Configuration;
using System.Web.Mvc;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using log4net;
using System.Text;

namespace CentraleRischiR2.Controllers
{
    public class ReportController : BaseController
    {
      
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private List<ElementoAnagraficheReport> anagCS;
        
      

        [Authorize]
        public JsonResult GetAnagraficheCS(string rs, string piva)
        {
            Log.Info("begin GetAnagraficheCs**");
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
            Log.Info("begin GetAnagCs - idUtente= " + idUtente.ToString());
            List<ElementoAnagraficheReport> returnValue = new List<ElementoAnagraficheReport>();
            List<ElementoAnagraficheReport> anagraficheInterne = new List<ElementoAnagraficheReport>();
            List<ElementoAnagraficheReport> anagraficheEsterne = new List<ElementoAnagraficheReport>();
            try { 
            using (DemoR2Entities context = new DemoR2Entities())
            {

                ElementoOperatore aziendaUtente = GetAziendaUtente(idUtente, context, ambiente);
                List<CompanyCS> anagCS = SearchCompany(partivaIva, ragioneSociale);
                Log.Debug("anagCs count " + anagCS.Count);

                    string partitaivabella = context.news_operatore.Select(a => a.rapporto).FirstOrDefault();
                 if (anagCS.Count > 0)
                {
                        anagraficheEsterne = anagCS.Select
                        (an => new ElementoAnagraficheReport
                        {
                            PartitaIva = an.vatNo != String.Empty ? an.vatNo :"",
                            EventiNegativi =
                                ""
                                ,
                            Fido = 0,
                            RagioneSociale = an.name != null ? an.name:"",
                            Indirizzo = an.address.street != String.Empty ? an.address.street:"",
                            Provincia = an.address.province != String.Empty ? an.address.province:"",
                            id = an.id != String.Empty ? an.id:"",
                            RatingDescrizione = ""
                                ,
                            Rapporto = context.news_operatore.Where(a => a.partita_iva == partivaIva).FirstOrDefault() != null?
                                context.news_operatore.Where(a => a.partita_iva == partivaIva).FirstOrDefault().rapporto:"",
                            Osservatorio = ""
                            ,
                            PEC = "",
                            StatoAttivita = an.status != null ? an.status:""
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
            }catch(Exception e)
            {
                Log.Error("Error : " + e.ToString());
            }
            Log.Info("end GetAnagCS");
            return returnValue;

        }



        private List<CompanyCS> SearchCompany(string partivaIva, string ragioneSociale)
        {
            Log.Info("begin SearchCompany filtri : partita Iva= "+partivaIva+" Ragione sociale= "+ragioneSociale);
            string token = LoginCS();
            return CompaniesListCS(token, partivaIva, ragioneSociale);
        }


        private  List<CompanyCS> CompaniesListCS(string token, string partitaIva, string ragioneSociale)
        {
            List<CompanyCS> retVal = new List<CompanyCS>();


            try
            {
                Log.Info("begin CompaniesListCS ");
                string URI = "https://connect.creditsafe.com/v1/companies";
                URI = URI + "?countries=" + "IT";
                if (!partitaIva.Equals(""))
                {
                    URI = URI + "&vatNo=" + partitaIva.ToString();
                }
                if (!ragioneSociale.Equals(""))
                {
                    URI = URI + "&name=" + ragioneSociale.ToString();
                }
                Log.Info("URI " + URI);
                var httpWebRequest2 = (HttpWebRequest)WebRequest.Create(URI);
                httpWebRequest2.ContentType = "application/json";
                httpWebRequest2.Method = "GET";
                httpWebRequest2.Headers.Add("Authorization", token);
                var httpResponse2 = (HttpWebResponse)httpWebRequest2.GetResponse();

                using (var streamReader = new StreamReader(httpResponse2.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    dynamic response = JsonConvert.DeserializeObject(result.ToString());

                    foreach (dynamic element in response.companies)
                    {
                            CompanyCS companyCS = new CompanyCS();
                            Addressview address = new Addressview();
                        try { companyCS.vatNo = element.vatNo[0]; } catch { companyCS.vatNo = ""; };
                            address.street = element.address.street;
                            address.province = element.address.province;
                            companyCS.address = address;
                            companyCS.status = element.status;
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

        public string LoginCS()
        {
            Log.Info("begin LoginCs");
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            string username = ConfigurationManager.AppSettings["UsernameWsCs"];
            string password = ConfigurationManager.AppSettings["PasswordWsCs"];
            var token = "";

            var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://connect.creditsafe.com/v1/authenticate");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            //WriteLog("username : " + username+"password : " +password);

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string json = "{\"username\":\"" + username + "\",\"password\":\"" + password + "\"}";
                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }
            try
            {
                WebResponse httpResponse = httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    dynamic response = JsonConvert.DeserializeObject(result.ToString());
                    token = response.token;
                }
            }
            catch (WebException ex)
            {
                Log.Info("errore LoginCs " + ex.ToString());
                using (WebResponse response = ex.Response)
                {
                    HttpWebResponse httpResponse2 = (HttpWebResponse)response;
                    //  using (Stream data = response.GetResponseStream())
                }
            }
            Log.Info("token : " + token.ToString());
            Log.Info("end LoginCs");
            return token;


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


        public string GetIdGlobalReport(string piva, string token)
        {
            string id = "";
            try
            {
                Log.Info("begin GlobalReportId ");
                string URI = "https://connect.creditsafe.com/v1/companies";
                URI = URI + "?countries=" + "IT&vatNo=" + piva;
                Log.Info("URI " + URI);
                Log.Info("token : " + token);
                var httpWebRequest2 = (HttpWebRequest)WebRequest.Create(URI);
                httpWebRequest2.ContentType = "application/json";
                httpWebRequest2.Method = "GET";
                httpWebRequest2.Headers.Add("Authorization", token);
                var httpResponse2 = (HttpWebResponse)httpWebRequest2.GetResponse();
                JsonSerializerSettings settings = new JsonSerializerSettings();
                settings.NullValueHandling = NullValueHandling.Ignore;
                using (var streamReader = new StreamReader(httpResponse2.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    dynamic response = JsonConvert.DeserializeObject(result.ToString(),settings);
                    
                    id = response.companies[0].id;
                }
                Log.Info("end GlobalReportId -- ID= " +id);
            }
            catch (WebException ex)
            {
                Log.Error("errore : " + ex.ToString());
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
            return id;
        }
     
        /*public ActionResult GetGlobalReportDirect(string piva)
        {
            ActionResult returnView = RedirectToAction("Index", "Home");
            string partitaIva = piva;
            string partitaIva2 = Request.Form["piva"];
            
            string filePath = WebConfigurationManager.AppSettings["ReportPath"];
            string dataoggi = DateTime.Now.ToString("yyyyMMdd");
            string extention = ".pdf";
            string fileName = partitaIva +"_"+ dataoggi;
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
                    token = LoginCS();
                    idCompany = GetIdGlobalReport(partitaIva, token);

                    //deserializzazione dinamica
                    json = CompanyComplete(idCompany, token);
               
                    newCO = true;
                    string URI = "https://connect.creditsafe.com/v1/companies/";
                      URI = URI + idCompany+"?template=complete";
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
                         StreamWriter objSW = new StreamWriter(filePath + fileName+extention, false, objEncoding);
                         objSW.Write(objSR.ReadToEnd());
                         objSW.Close();
                         objSR.Close();

                         var fileStream = new FileStream((filePath + fileName+extention), FileMode.Open);

                         var memoryStream = new MemoryStream();
                         fileStream.CopyTo(memoryStream);
                         fileStream.Close();
                         memoryStream.Position = 0;
                         if (!DBHandler.Insertreport(partitaIva, loggeduser.IdUser, JsonConvert.DeserializeObject(json.ToString()),fileName))
                         {
                             Log.Error("error insert report: ");
                         }
                         DBHandler.InserisciRichiesteReportFornitore(partitaIva, loggeduser.IdUser, loggeduser.Email, DateTime.Now, DateTime.Now.AddMonths(12), tipoReport, DBHandler.GetCentro(loggeduser.IdUser));
                         mercato = DBHandler.GetMercato(DBHandler.GetCentro(loggeduser.IdUser).IdMercato).Mercato;
                         idPortfoglio = PortfoglioCs.GetIdPortfoglio(token, mercato);
                         string retVal = CentraleRischiR2Library.PortfoglioCs.InsertCOMonitoraggio(token, idPortfoglio, idCompany, partitaIva);
                         Log.Info("inserimento portfolio = " + retVal + " User= " + loggeduser.IdUser);
                         DBHandler.InserisciChekMonitoraggio(loggeduser.IdUser, partitaIva, DateTime.Now, DateTime.Now);
                         if (newCO)
                         {
                             Log.Info("end GlobalReport nuova azienda** " + "user = " + loggeduser.IdUser);
                         }
                         else
                         {
                             Log.Info("end GlobalReport azienda gia in Monitoraggio** " + "user = " + loggeduser.IdUser);
                         }
                         return File(memoryStream, "application/pdf", fileName+extention);
                     }
                }
                catch (Exception ex)
                {
                    Log.Error("errore : " + ex.ToString());
                    return RedirectToAction("Index", "Home");
                }
        }*/


            /*
        [System.Web.Services.WebMethod()]
        [Authorize]
        [HttpPost]
        public ActionResult GetGlobalReport()
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
                    token = LoginCS();
                    idCompany = GetIdGlobalReport(partitaIva, token);

                    //deserializzazione dinamica
                    json = CompanyComplete(idCompany, token);
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

        public ActionResult ReportPdf(string id,string token)
        {
            string URI = "https://connect.creditsafe.com/v1/companies/";
            URI = URI + id;
            string filePath = WebConfigurationManager.AppSettings["ReportPath"];
            string fileName =id;

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
                    StreamWriter objSW = new StreamWriter(filePath+fileName, false, objEncoding);
                    objSW.Write(objSR.ReadToEnd());
                    objSW.Close();
                    objSR.Close();

                    var fileStream = new FileStream((filePath+fileName), FileMode.Open);

                    // dynamic result = streamReader.ReadToEnd();
                    var memoryStream = new MemoryStream();
                    fileStream.CopyTo(memoryStream);
                    fileStream.Close();
                    memoryStream.Position = 0;
                    return File(memoryStream, "application/pdf", filePath+fileName);
                }

            }
            catch (Exception ex)
            {
                Log.Error("errore : " + ex.ToString());
                return RedirectToAction("Index", "Home");
            }
        }

        public string CompanyComplete(string id, string token)
        {
            //CHIAMATA PER ID 

            string response = "";
            string URI = "https://connect.creditsafe.com/v1/companies/";
            URI = URI + id;
            URI = URI + "?template=complete";

            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(URI.ToString());
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "GET";
                httpWebRequest.Headers.Add("Authorization", token);
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var resultws = streamReader.ReadToEnd();
                    response = resultws;
                    //Log.Debug("result companycomplete = " + resultws);
                    //Log.Debug("response companycomplete = " + response.ToString());
                }
            }
            catch (Exception ex)
            {
                Log.Error("errore : " + ex.ToString());
            }

            return response;
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
            using (DemoR2Entities context = new DemoR2Entities())
            {
                ViewBag.ReportCount = context.richiesta_report.Where(rep => rep.id_utente == loggeduser.IdUser && rep.evasa == false).Count();
                loggeduser.ReportCount = ViewBag.ReportCount;
            }
            ViewBag.Ambiente = loggeduser.Ambiente;
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
