using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel.Channels;
using System.ServiceModel;
using System.Net;
using System.ServiceModel.Description;
using System.Threading; 
using CentraleRischiR2Library.BridgeClasses;
using CentraleRischiR2Library.CervedRetrieveReportServiceReference;
using CentraleRischiR2Library.CervedThreeStepProdServiceReference;
using CentraleRischiR2Library.CDSServiceReference;
using System.IO;
using System.IO.Compression;
using System.Xml.Linq;
using System.Configuration;
using Newtonsoft.Json;
using System.Net.Http;

namespace CentraleRischiR2Library
{
    public class CervedWSHandler 
    {

        public vs_aziende_osservamercati Azienda { get; set; }
        public string HostThreeStepWSAddress { get; set; }
        public string HostRetrieveReportWSAddress { get; set; }
        public string HostCdSReportWSAddress { get; set; }
        public string NetUser { get; set; }
        public string NetPassword { get; set; }
        public string ReportPath { get; set; }
        public string LogPath { get; set; }
        public company RetrievedCompany;



       


        public CervedWSHandler(
            vs_aziende_osservamercati azienda,
            string hostThreeStepWS, string hostRetreveReportWS, string oneThreeStepWS, string cdsThreeStepWS,string netUser, string netPassword,string reportPath,string logPath="")
        {
            Azienda = azienda;
            HostThreeStepWSAddress = hostThreeStepWS;
            HostRetrieveReportWSAddress = hostRetreveReportWS;
            HostCdSReportWSAddress = cdsThreeStepWS;
            NetUser = netUser;
            NetPassword = netPassword;
            ReportPath = reportPath;
            LogPath = logPath;
        }

        public void WriteLog(string message)
        {
            if (LogPath != String.Empty)
            {
                StreamWriter sw = new StreamWriter(String.Format("{0}log{1:yyyyMMdd}.log", LogPath, DateTime.Now), true);
                sw.WriteLine(DateTime.Now.ToString("g") + ": " + message);
                //Console.WriteLine(DateTime.Now.ToString("g") + ": " + message);
                sw.Flush();
                sw.Close();
            }
            else
            {
                Console.WriteLine(DateTime.Now.ToString("g") + ": " + message);
            }
        }


        public bool GetPurchasedReportCDS(long codiceRichiesta, int productCode, string partitaIva)
        {
            bool returnValue = false;
            WriteLog("Tentativo recupero Global Profile e Score Credit Decision per partitaIva " + partitaIva+ " codice Cerved " + codiceRichiesta);
            cdsRequestData requestObject = new cdsRequestData();
            cdsEvaluationData evaluationObject = new cdsEvaluationData();
            cdsAdditionalInputParameter additionalInputObject = new cdsAdditionalInputParameter();
            cdsCompany gpCompany = new cdsCompany
            {
                vatNumber = partitaIva
            };

            requestObject.cdsCompany = gpCompany;
            evaluationObject.operationType = cdsOperationType.EVP;
            evaluationObject.cdsId = codiceRichiesta;

            evaluationObject.documentType = "XML,PDF";
            evaluationObject.contextClear = cdsContextClear.INTEGRAL_CONTEXT;
            evaluationObject.outputType = "PDF";
            
            additionalInputObject.cdsInputKeyParam = cdsInputKeyParam.PRODUCT_CODE;
            additionalInputObject.value = "55220";


            AddressHeader addressHeader1 = AddressHeader.CreateAddressHeader("CdsWS", this.HostCdSReportWSAddress, 1);
            AddressHeader[] addressHeaders = new AddressHeader[1] { addressHeader1 };
            EndpointAddress endpointAddress = new EndpointAddress(
                new Uri(this.HostCdSReportWSAddress), addressHeaders);

            HttpsTransportBindingElement transportElement = new HttpsTransportBindingElement
            {
                AuthenticationScheme = AuthenticationSchemes.Basic,
                MaxReceivedMessageSize = 2147483647,
                MaxBufferPoolSize = 2147483647
            };

            TextOrMtomEncodingBindingElement bindingElement = new TextOrMtomEncodingBindingElement
            {
                MessageVersion = MessageVersion.Soap11
            };
            CustomBinding mtomBinding = new CustomBinding(bindingElement, transportElement)
            {
                OpenTimeout = new TimeSpan(0, 30, 0),
                CloseTimeout = new TimeSpan(0, 30, 0),
                SendTimeout = new TimeSpan(0, 30, 0)
            };

            CdsWSClient wCdsClient = new CdsWSClient(mtomBinding, endpointAddress);

            string usernameWS = String.Empty;
            string passwordWS = String.Empty;

            if (Azienda != null)
            {
                WriteLog("-- Ricerca da parte di " + Azienda.m01_ragionesociale + " codice " + Azienda.m01_codice);
                usernameWS =
                    Azienda.m01_net_user ?? this.NetUser;
                passwordWS =
                    Azienda.m01_net_password ?? this.NetPassword;
            }
            else
            {
                usernameWS = this.NetUser;
                passwordWS = this.NetPassword;
            }
            
            wCdsClient.ClientCredentials.UserName.UserName = usernameWS;
            wCdsClient.ClientCredentials.UserName.Password = passwordWS;
            WriteLog("-- Utilizzate credenziali  user: " + usernameWS + " pwd : " + passwordWS);



            evaluateExended extendedObject = new evaluateExended();

            List<cdsAdditionalInputParameter> parameters = new List<cdsAdditionalInputParameter>
            {
                additionalInputObject
            };

            extendedObject.cdsEvaluationData = evaluationObject;
            extendedObject.cdsRequestData = requestObject;
            extendedObject.cdsAdditionalInputParameters = parameters.ToArray();



            evaluateExendedResponse response = wCdsClient.evaluateExended(extendedObject);
            if (response != null)
            {
                WriteLog("-- Risposta CDWS per partitaIva: " + partitaIva + " codice Cerved: " + codiceRichiesta);
                cdsRequestInfo info = response.cdsResultExtended.cdsResponseExtended.cdsRequestInfo;

                if (info.resultCode == "OK")
                {
                    WriteLog("---- OK CDWS per partitaIva: " + partitaIva + " codice Cerved: " + codiceRichiesta);
                    string documentExtension = "";
                    List<cerved_check> monitoraggiSuStessaPiva = DBHandler.GetListaRichiesteCervedEsistentiSuPiva(partitaIva);
                    if (response.cdsResultExtended.cdsResponseExtended.outputFormatInformation.Length > 0)
                    {
                        documentExtension = response.cdsResultExtended.cdsResponseExtended.outputFormatInformation[0].format;
                        outputFormatInformation fileOutPut = response.cdsResultExtended.cdsResponseExtended.outputFormatInformation[0];

                        string reportFilePath =
                            documentExtension == "PDF" ?
                            ReportPath + "\\CDWS_" + codiceRichiesta + "." + documentExtension :

                            ReportPath + "\\" + codiceRichiesta + "." + documentExtension;
                        if (File.Exists(reportFilePath))
                        {
                            File.Delete(reportFilePath);
                        }
                        File.WriteAllBytes(reportFilePath, fileOutPut.bytes);
                        WriteLog("------ Salvato PDF Valutazione " + codiceRichiesta);

                        //foreach (cerved_check monitoraggio in monitoraggiSuStessaPiva)
                        //{
                        //    string reportFilePath =
                        //        documentExtension == "PDF" ?
                        //        ReportPath + "\\CDWS_" + monitoraggio.codice_cerved + "." + documentExtension :
                        //        ReportPath + "\\" + monitoraggio.codice_cerved + "." + documentExtension;
                        //    if (File.Exists(reportFilePath))
                        //    {
                        //        File.Delete(reportFilePath);
                        //    }
                        //    File.WriteAllBytes(reportFilePath, fileOutPut.bytes);
                        //    WriteLog("------ Salvato PDF Valutazione " + monitoraggio.codice_cerved);
                        //}
                        /*
                        if (documentExtension == "XML")
                        {
                            if (ParseXMLCDS(ReportPath + "\\" + codiceRichiesta + "." + documentExtension, partitaIva, codiceRichiesta.ToString()))
                                DBHandler.AggiornaEvasioneRichiesta(codiceRichiesta, );
                        }
                        */
                    }
                    if (response.cdsResultExtended.cdsResponseExtended.outputFormatInformation.Length > 1)
                    {
                        documentExtension = response.cdsResultExtended.cdsResponseExtended.outputFormatInformation[1].format;
                        outputFormatInformation fileOutPut = response.cdsResultExtended.cdsResponseExtended.outputFormatInformation[1];
                        //foreach (cerved_check monitoraggio in monitoraggiSuStessaPiva)
                        //{
                        //    string reportFilePath =
                        //        documentExtension == "PDF" ?
                        //        ReportPath + "\\CDWS_" + monitoraggio.codice_cerved + "." + documentExtension :
                        //        ReportPath + "\\" + monitoraggio.codice_cerved + "." + documentExtension;
                        //    if (File.Exists(reportFilePath))
                        //    {
                        //        File.Delete(reportFilePath);
                        //    }
                        //    File.WriteAllBytes(reportFilePath, fileOutPut.bytes);
                        //    WriteLog("------ Salvato XML Valutazione " + monitoraggio.codice_cerved);
                        //}
                        string reportFilePath =
                            documentExtension == "PDF" ?
                            ReportPath + "\\CDWS_" + codiceRichiesta + "." + documentExtension :
                            ReportPath + "\\" + codiceRichiesta + "." + documentExtension;
                        if (File.Exists(reportFilePath))
                        {
                            File.Delete(reportFilePath);
                        }
                        File.WriteAllBytes(reportFilePath, fileOutPut.bytes);
                        WriteLog("------ Salvato XML Valutazione " + codiceRichiesta);
                    }
                    returnValue = true;
                }
                else
                {
                    WriteLog("---- NOT YET: Documento non aggiornato per il recupero: stato " + info.resultCode);
                    
                }
                                
            }

            return returnValue;
        }

        /*
        public bool GetPurchasedReportGlobalRisk(long codiceRichiesta,int productCode,string partitaIva)
        {
            bool returnValue = false;


            
            retrieveReport report = new retrieveReport();
            retrieveReport reportXML = new retrieveReport();


            retrieveReportParameter reportParameter = new retrieveReportParameter();
            retrieveReportParameter reportParameterXML = new retrieveReportParameter();


            reportParameter.documentTypeSpecified = false;
            reportParameter.reportCode = codiceRichiesta;
            reportParameter.documentFormat = documentFormat.PDF;
            reportParameter.dataObject = false;

            reportParameterXML.documentTypeSpecified = false;
            reportParameterXML.reportCode = codiceRichiesta;
            reportParameterXML.documentFormat = documentFormat.XML;
            reportParameterXML.dataObject = false;

            report.retrieveReportParameter = reportParameter;
            reportXML.retrieveReportParameter = reportParameterXML;


            // WCF MTOM -> JBoss MTOM: using CustomBinding
            
            AddressHeader addressHeader1 = AddressHeader.CreateAddressHeader("retrieveReportWS", HostRetrieveReportWSAddress, 1);
            AddressHeader[]  addressHeaders = new AddressHeader[1] { addressHeader1 };
            EndpointAddress endpointAddress = new EndpointAddress(
                new Uri(HostRetrieveReportWSAddress), addressHeaders);

            HttpsTransportBindingElement  transportElement = new HttpsTransportBindingElement();
            transportElement.AuthenticationScheme = AuthenticationSchemes.Basic;
            transportElement.MaxReceivedMessageSize = 2147483647;
            transportElement.MaxBufferPoolSize = 2147483647;

            TextOrMtomEncodingBindingElement  bindingElement = new TextOrMtomEncodingBindingElement();
            bindingElement.MessageVersion = MessageVersion.Soap11;
            CustomBinding mtomBinding = new CustomBinding(bindingElement, transportElement);


            RetrieveReportWSClient wsReportRetrieveService = new RetrieveReportWSClient(mtomBinding, endpointAddress);

            wsReportRetrieveService.ClientCredentials.UserName.UserName = NetUser;
            wsReportRetrieveService.ClientCredentials.UserName.Password = NetPassword;



            try
            {
                report retrievedReport = wsReportRetrieveService.retrieveReport(report).report;

                if (retrievedReport != null && retrievedReport.document != null && retrievedReport.document.Length != 0)
                {

                    if (productCode.ToString() == "55220")
                    {
                        File.WriteAllBytes(ReportPath + "\\" + codiceRichiesta + ".pdf", retrievedReport.document);
                    }
                    
                    if (productCode.ToString() == "55210")
                    {
                        File.WriteAllBytes(ReportPath + "\\" + codiceRichiesta + ".pdf", retrievedReport.document);
                    }
                    
                    
                    returnValue = true;

                }
                else
                {
                    returnValue = false;
                }

                retrievedReport = wsReportRetrieveService.retrieveReport(reportXML).report;

                if (retrievedReport != null && retrievedReport.document != null && retrievedReport.document.Length != 0)
                {
                    if (productCode.ToString() == "55220")
                    {
                        File.WriteAllBytes(ReportPath + "\\" + codiceRichiesta + ".xml", retrievedReport.document);
                    }
                    if (productCode.ToString() == "55210")
                    {
                        File.WriteAllBytes(ReportPath + "\\risk_" + codiceRichiesta + ".xml", retrievedReport.document);
                        ParseXMLGlobalRisk(ReportPath + "\\risk_" + codiceRichiesta + ".xml", partitaIva);
                    }

                    returnValue = true;                    
                }
                else
                {
                    returnValue = false;
                }

                
            }
            catch (Exception)
            {}


            

            return returnValue;


        }

        public bool ParseXMLCDS(string piva,long idUtente, JsonSeralizespriv json)
        {
            bool returnValue = false;
            string cervedGroupScore = "";
            string cervedGroupScoreDescrizione = "";
            string coloreIntermedioCliente = "";
            string globalScore = "";
            string globalScoreDescrizione = "";
            string statoAttivita = "";
            string eventiNegativi = "";
            string ragioneSociale = "";
            string pec = ""; /*OK ??
            decimal fido = 0; /*OK ??
            string encodedReportContent;
           // XDocument document = XDocument.Load(reportPath);
            //WriteLog("Parsing XML Valutazione partitaIva " + piva + "  codice Cerved " + codiceRichiesta);
            try
            {


                pec =
                    document.Root.Elements("s2xData").FirstOrDefault()
                    .Elements("datiRichiesta")
                    .FirstOrDefault()
                    .Elements("contesto")
                    .Where(el => el.Element("nomeDato").Value == "infoanagrafica.mail")
                    .FirstOrDefault() != null ? document.Root.Elements("s2xData").FirstOrDefault()
                    .Elements("datiRichiesta")
                    .FirstOrDefault()
                    .Elements("contesto")
                    .Where(el => el.Element("nomeDato").Value == "infoanagrafica.mail")
                    .FirstOrDefault().Element("valore").Value : "";

                ragioneSociale =
                    document.Root.Elements("s2xData").FirstOrDefault()
                    .Elements("datiRichiesta")
                    .FirstOrDefault()
                    .Elements("anagrafica").FirstOrDefault()
                    .Element("denominazione").Value;

                cervedGroupScore =
                    document.Root.Elements("s2xData").FirstOrDefault()
                    .Elements("datiRichiesta")
                    .FirstOrDefault()
                    .Elements("contesto")
                    .Where(el => el.Element("nomeDato").Value == "regola.CGScore")
                    .FirstOrDefault().Element("valore").Value;

                cervedGroupScoreDescrizione =
                    document.Root.Elements("s2xData").FirstOrDefault()
                    .Elements("datiRichiesta")
                    .FirstOrDefault()
                    .Elements("contesto")
                    .Where(el => el.Element("nomeDato").Value == "regola.CGScore.descrizione")
                    .FirstOrDefault().Element("valore").Value;

                globalScore =
                    document.Root.Elements("s2xData").FirstOrDefault()
                    .Elements("datiRichiesta")
                    .FirstOrDefault()
                    .Elements("contesto")
                    .Where(el => el.Element("nomeDato").Value == "valutazione.Classe")
                    .FirstOrDefault().Element("valore").Value;

                globalScoreDescrizione =
                    document.Root.Elements("s2xData").FirstOrDefault()
                    .Elements("datiRichiesta")
                    .FirstOrDefault()
                    .Elements("contesto")
                    .Where(el => el.Element("nomeDato").Value == "valutazione.messaggioRatingCDS")
                    .FirstOrDefault().Element("valore").Value;


                coloreIntermedioCliente =
                    document.Root.Elements("s2xData").FirstOrDefault()
                    .Elements("datiRichiesta")
                    .FirstOrDefault()
                    .Elements("contesto")
                    .Where(el => el.Element("nomeDato").Value == "regola.coloreIntermedioCliente")
                    .FirstOrDefault().Element("valore").Value;

                /*COEFFICIENTE CLIENTE???

                statoAttivita =
                    document.Root.Elements("s2xData").FirstOrDefault()
                    .Elements("datiRichiesta")
                    .FirstOrDefault()
                    .Elements("contesto")
                    .Where(el => el.Element("nomeDato").Value == "infoanagrafica.statoImpresa")
                    .FirstOrDefault().Element("valore").Value;

                eventiNegativi =
                    document.Root.Elements("s2xData").FirstOrDefault()
                    .Elements("datiRichiesta")
                    .FirstOrDefault()
                    .Elements("contesto")
                    .Where(el => el.Element("nomeDato").Value == "regola.scorEventiNeg.descrizione")
                    .FirstOrDefault().Element("valore").Value;

                string fidoStr =
                    document.Root.Elements("s2xData").FirstOrDefault()
                    .Elements("datiRichiesta")
                    .FirstOrDefault()
                    .Elements("contesto")
                    .Where(el => el.Element("nomeDato").Value == "valutazione.fido")
                    .FirstOrDefault() != null ?
                    document.Root.Elements("s2xData").FirstOrDefault()
                    .Elements("datiRichiesta")
                    .FirstOrDefault()
                    .Elements("contesto")
                    .Where(el => el.Element("nomeDato").Value == "valutazione.fido")
                    .FirstOrDefault().Element("valore").Value : "0";

                fidoStr = fidoStr.Replace(".", ",");

                fidoStr = fidoStr == "ND" ? "0" : fidoStr;

                fido =
                    Convert.ToDecimal(fidoStr);


                encodedReportContent = document.Root.Elements("s2xData").FirstOrDefault()
                    .Elements("datiRichiesta").FirstOrDefault()
                    .Elements("infoDocumentoCerved").FirstOrDefault()
                    .Elements("documentoCerved").FirstOrDefault()
                    .Elements("dossier").FirstOrDefault() != null ?
                document.Root.Elements("s2xData").FirstOrDefault()
                    .Elements("datiRichiesta").FirstOrDefault()
                    .Elements("infoDocumentoCerved").FirstOrDefault()
                    .Elements("documentoCerved").FirstOrDefault()
                    .Elements("dossier").FirstOrDefault().Value : "";

                WriteLog("---- Elementi estratti; procedo a estrazione Global profile");
                if (encodedReportContent != String.Empty)
                {
                    string reportFolder = reportPath.Replace(codiceRichiesta + ".xml", "");
                    /*BASE 64 STANDARD
                    string gzipFileName = String.Format("{0}\\{1}.gzip", reportFolder, codiceRichiesta);
                    string pdfFileName = String.Format("{0}\\{1}.pdf", reportFolder, codiceRichiesta);
                    byte[] data = Convert.FromBase64String(encodedReportContent);
                    File.WriteAllBytes(gzipFileName, data);
                    using (Stream fd = File.Create(pdfFileName))
                    using (Stream fs = File.OpenRead(gzipFileName))
                    using (Stream csStream = new GZipStream(fs, CompressionMode.Decompress))
                    {
                        byte[] buffer = new byte[1024];
                        int nRead;
                        while ((nRead = csStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            fd.Write(buffer, 0, nRead);
                        }
                        fs.Close();
                        fd.Close();
                    }
                    
                    /*COPIO GLOBAL PROFILE SU ESISTENTI
                    List<cerved_check> monitoraggiSuStessaPiva = DBHandler.GetListaRichiesteCervedEsistentiSuPivaAttivi(piva);

                    foreach(cerved_check monitoraggioSimile in monitoraggiSuStessaPiva)
                    {

                        if (codiceRichiesta != monitoraggioSimile.codice_cerved)
                        {
                            File.Copy(String.Format("{0}\\{1}.pdf", reportFolder, codiceRichiesta), String.Format("{0}\\{1}.pdf", reportFolder, monitoraggioSimile.codice_cerved), true);
                        }
                            WriteLog("---- Tentativo Inserimento News per monitoraggio" + monitoraggioSimile.codice_cerved);
                        if (DBHandler.InserisciNewsReport(monitoraggioSimile.codice_cerved, piva, ragioneSociale, globalScore, globalScoreDescrizione, "", coloreIntermedioCliente, cervedGroupScore, cervedGroupScoreDescrizione, monitoraggioSimile.id_utente, eventiNegativi, fido, descrizione, dataSegnalazione, "STANDARD"))
                        {
                            WriteLog("---- OK Inserita News per monitoraggio" + monitoraggioSimile.codice_cerved);
                        }
                        else {
                            WriteLog("---- NOK Inserita News per monitoraggio" + monitoraggioSimile.codice_cerved);
                        }


                        List<string> additionalData = new List<string>
                        {
                            ragioneSociale,
                            piva,
                            monitoraggioSimile.codice_cerved,
                            reportFolder + "\\" + monitoraggioSimile.codice_cerved + ".pdf"
                        };

                        WriteLog("Invio Mail Nuovo Report per Codice" + monitoraggioSimile.codice_cerved + "utente "+ monitoraggioSimile.id_utente);

                        MailHandler.WarnMail(monitoraggioSimile.id_utente, "RECEIVED_REPORT", additionalData);

                        WriteLog("Mail Inviata");
                        
                    }
                    File.Delete(gzipFileName);
                    
                    WriteLog("---- OK Estratto, rimosso zip");
                }
            }
            catch (Exception ex)
            {
                WriteLog("-- NOK Parsing XML " + ex.Message);
            }
            finally
            {
                try
                {
                    DBHandler.AggiornaAnagraficaFidoEventiNegativiPecStato(piva, fido, eventiNegativi, pec, statoAttivita);
                    DBHandler.InserisciRating(piva, globalScore, globalScoreDescrizione, "", coloreIntermedioCliente, cervedGroupScore, cervedGroupScoreDescrizione);
                    //DBHandler.InserisciNewsReport(codiceRichiesta, piva, ragioneSociale, globalScore, globalScoreDescrizione, "", coloreIntermedioCliente, cervedGroupScore, cervedGroupScoreDescrizione, idUtente, eventiNegativi, fido,descrizione,dataSegnalazione,"STANDARD");
                    WriteLog("---- Aggiornata Anagrafica, inserito rating.");
                    returnValue = true;
                }
                catch (Exception)
                {
                    WriteLog("-- NOK Updating Dabase ");
                }
                
                

            }

            return returnValue;
        }*/

        /*NON USATO*/
        //public static bool ParseXMLGlobalProfile(string reportPath, string piva)
        //{
        //    bool returnValue = false;
        //    string cervedGroupScore = "";
        //    string statoAttivita = "";
        //    bool eventiNegativi = false;
        //    string pec = "";
        //    int fido = 0;
        //    XDocument document = XDocument.Load(reportPath);
        //    try
        //    {
        //        string averageGrantableCredit = document.Root.Elements("AverageGrantableCredit")
        //            .FirstOrDefault()
        //            .Elements("CreditLimit")
        //            .FirstOrDefault()
        //            .Elements("Value").FirstOrDefault().Value;

        //        fido = Convert.ToInt32(averageGrantableCredit);


        //        cervedGroupScore = document.Root.Elements("CervedGroupScore")
        //            .FirstOrDefault()
        //            .Elements("Score")
        //            .FirstOrDefault().Value;

        //        statoAttivita = document.Root.Elements("ItemInformation")
        //            .FirstOrDefault()
        //            .Elements("CompanySituation")
        //            .FirstOrDefault().Value;

        //        pec = document.Root.Elements("DeliveryAndSummaryDetails")
        //            .FirstOrDefault()
        //            .Elements("CertifiedEmail")
        //            .FirstOrDefault().Value;

        //        string eventiN =
        //            document.Root.Elements("Focus")
        //            .FirstOrDefault()
        //            .Elements("NegativeEventsCredit")
        //            .FirstOrDefault().Value;

        //        int numeroEventiN = 0;
        //        Int32.TryParse(eventiN, out numeroEventiN);
        //        eventiNegativi = numeroEventiN > 0;

        //    }

        //    catch (Exception) { }


        //    /*DBHandler.InserisciRating(piva, fido, cervedGroupScore, cervedGroupScore, pec, eventiNegativi, "55220");*/
        //    returnValue = true;

        //    return returnValue;

        //}

        /*
        public static bool ParseXMLGlobalRisk(string reportPath, string piva)
        {
            bool returnValue = false;
            string cervedGroupScore = "";
            string statoAttivita = "";                        
            string pec = "";
            bool eventiNegativi = false;
            
            XDocument document = XDocument.Load(reportPath);
            try
            {


                cervedGroupScore = 
                    document.Root.Elements("HistoricalRiskEvaluation")                        
                    .Descendants("Evaluations")
                    .Where(a => (string)a.Attribute("CurrentMonth") == "1")
                    .Select(a => (string)a.Element("Risk")).FirstOrDefault();

                if (cervedGroupScore == "7" || cervedGroupScore == "8")
                    eventiNegativi = true;


                statoAttivita = document.Root.Elements("DeliveryAndSummaryDetails")                    
                    .Descendants("CompanySituation")
                    .FirstOrDefault().Value;
                    
                    
                pec = document.Root.Elements("ItemInformation")
                    .FirstOrDefault()                    
                    .Elements("CertifiedEmail")
                    .FirstOrDefault().Value;

            }
            
            catch (Exception) { }
            

            DBHandler.InserisciRating(piva, 0, cervedGroupScore, statoAttivita, pec, eventiNegativi, "55210");
            returnValue = true;
            return returnValue;

        }

        */

        public List<company> SearchCompany(string partitaIva,string ragioneSociale,string provincia)
        {

            List<company> returnValue = new List<company>();

            searchCompany searchCompany = new searchCompany();
            searchCompanyParameters parameters = new searchCompanyParameters();

            WriteLog("Invocata Richiesta per partitaIva: " + partitaIva + " ragioneSociale:"+ragioneSociale +" provincia" + provincia);
            



            if (partitaIva.Trim() != String.Empty)
            {
                parameters.vatNumber = partitaIva;
            }

            if (ragioneSociale.Trim() != String.Empty)
            {
                parameters.companyName = ragioneSociale;
            }

            if (provincia.Trim() != String.Empty)
            {
                parameters.province = provincia;
            }


            
            searchCompany.@params = parameters;




            // ---------------------------------------------------------------


            // WCF MTOM -> JBoss MTOM: using CustomBinding
            //
            AddressHeader addressHeader1 = AddressHeader.CreateAddressHeader("retrieveReportWS", HostThreeStepWSAddress, 1);
            AddressHeader[] addressHeaders = new AddressHeader[1] { addressHeader1 };
            EndpointAddress endpointAddress = new EndpointAddress(
                new Uri(HostThreeStepWSAddress), addressHeaders);

            HttpsTransportBindingElement transportElement = new HttpsTransportBindingElement
            {
                AuthenticationScheme = AuthenticationSchemes.Basic,
                MaxReceivedMessageSize = 2147483647,
                MaxBufferPoolSize = 2147483647
            };

            TextOrMtomEncodingBindingElement bindingElement = new TextOrMtomEncodingBindingElement
            {
                MessageVersion = MessageVersion.Soap11
            };
            CustomBinding mtomBinding = new CustomBinding(bindingElement, transportElement)
            {
                OpenTimeout = new TimeSpan(0, 30, 0),
                CloseTimeout = new TimeSpan(0, 30, 0),
                SendTimeout = new TimeSpan(0, 30, 0)
            };


            ThreeStepWSClient wsRetrieve = new ThreeStepWSClient(mtomBinding, endpointAddress);

            


            string usernameWS = String.Empty;
            string passwordWS = String.Empty;

            if (Azienda != null)
            {
                WriteLog("-- Ricerca da parte di " + Azienda.m01_ragionesociale+" codice "+ Azienda.m01_codice);
                usernameWS =
                    Azienda.m01_net_user ?? this.NetUser;
                passwordWS =
                    Azienda.m01_net_password ?? this.NetPassword;
            }
            else {
                usernameWS = this.NetUser;
                passwordWS = this.NetPassword;
            }

            wsRetrieve.ClientCredentials.UserName.UserName = usernameWS;
            wsRetrieve.ClientCredentials.UserName.Password = passwordWS;



            WriteLog("-- Utilizzate credenziali  user: " + usernameWS+ " pwd : "+ passwordWS);

            searchCompanyResponse searchedCompany = wsRetrieve.searchCompany(searchCompany);

            if(searchedCompany.searchResult.companies != null)
                returnValue = searchedCompany.searchResult.companies.ToList();


            WriteLog("-- Trovate : " + returnValue.Count + " occorenze");
            

            return returnValue;

        }


        public string BuyGlobalProfile(string partitaIva, int productCode, long idLoggedUser,bool sendMail)
        {
            string returnValue = string.Empty;


            
            
            if(partitaIva.Any(x => char.IsLetter(x)))
            {
                WriteLog("-- Non è possibile richiedere monitoraggio su aziende estere.");
                return returnValue;
            }

            List<company> aziendeTrovate = SearchCompany(partitaIva, String.Empty, string.Empty);

            
            //if (aziendeTrovate.Count == 0)
            //{
            //    WriteLog("-- Partita Iva " + partitaIva + " non trovata in archivio Cerved! Impossibile acquistare rapporto.");
            //    return returnValue;
            //}

            ElementoOperatore aziendaUtente = DBHandler.GetAziendaUtente(idLoggedUser, null, "STANDARD");            

            /*SE RAPPORTO ESISTE GIA NON LO RICHIEDO A CERVED MA COPIO RIGA
             */            
            //if (DBHandler.VerificaEsistenzaRapportoAttivo(partitaIva))
            //{
            //    return DBHandler.CopiaRapportoUtente(partitaIva, idLoggedUser);
            //}            
            company azienda =
                aziendeTrovate[0];
            if (azienda.vatNumber == null) azienda.vatNumber = partitaIva;
            WriteLog("Richiesto acquisto global profile per piva: " + partitaIva + " da utente " + idLoggedUser);
            DBHandler.AggiornaAziendaOsservatorioAccodamentoRichiesta(azienda, idLoggedUser, productCode,partitaIva);
            WriteLog("-- Aggiornata anagrafica e preferiti per piva " + partitaIva);


            

            cdsRequestData requestObject = new cdsRequestData();
            cdsEvaluationData evaluationObject = new cdsEvaluationData();
            cdsAdditionalInputParameter additionalInputObject = new cdsAdditionalInputParameter();





            cdsCompany gpCompany = new cdsCompany
            {
                vatNumber = partitaIva
            };



            requestObject.cdsCompany = gpCompany;            
                evaluationObject.operationType = cdsOperationType.VAL;
                evaluationObject.grid = "GLOPROSSE";
                evaluationObject.urgency = 0;
                evaluationObject.documentType = "PDF,XML";

                /*CODICE OSSERVA PER TUTTI I SUPERADMIN*/    
                NavigationUser utenteLoggato = DBHandler.GetUtente(idLoggedUser);
                if (utenteLoggato != null)
                {
                    if (utenteLoggato.IdRuolo != 0)
                        evaluationObject.ndg = aziendaUtente.CodicePaylineCerved;
                    else
                        evaluationObject.ndg = "54359";
                }
                else
                    evaluationObject.ndg = aziendaUtente.CodicePaylineCerved;

                evaluationObject.contextClear = cdsContextClear.INTEGRAL_CONTEXT;
                /*evaluationObject.outputType = "PDF";*/
                /*evaluationObject.internalData = "assegniInsoluti1PresRiba=0, infoInterne=b, impPregiudizievoli=0.0, assegniInsoluti=0.0, scaduto=0.0, azioniLegali=NO, DSO=0.0, esposizione=0.0, solleciti=NO";*/
                additionalInputObject.cdsInputKeyParam = cdsInputKeyParam.PRODUCT_CODE;
                additionalInputObject.value = "55220";                
                evaluationObject.internalData = 
                    //DBHandler.GetDatiInterniInvioCerved(partitaIva, DateTime.Now, DateTime.Now.AddMonths(-12), 0);



            //   this.WriteLog("Richiesta Global Profile da utente " + utenteLoggato.Username+ " su piva "+ partitaIva+ " con Codice Fatturazione Cerved " + evaluationObject.ndg);
            
                AddressHeader addressHeader1 = AddressHeader.CreateAddressHeader("CdsWS", this.HostCdSReportWSAddress, 1);
                AddressHeader[] addressHeaders = new AddressHeader[1] { addressHeader1 };
                EndpointAddress endpointAddress = new EndpointAddress(
                    new Uri(this.HostCdSReportWSAddress), addressHeaders);



            HttpsTransportBindingElement transportElement = new HttpsTransportBindingElement
            {
                AuthenticationScheme = AuthenticationSchemes.Basic,
                MaxReceivedMessageSize = 2147483647,
                MaxBufferPoolSize = 2147483647
            };


            TextOrMtomEncodingBindingElement bindingElement = new TextOrMtomEncodingBindingElement
            {
                MessageVersion = MessageVersion.Soap11
            };
            CustomBinding mtomBinding = new CustomBinding(bindingElement, transportElement)
            {
                OpenTimeout = new TimeSpan(0, 30, 0),
                CloseTimeout = new TimeSpan(0, 30, 0),
                SendTimeout = new TimeSpan(0, 30, 0)
            };

            CdsWSClient wCdsClient = new CdsWSClient(mtomBinding, endpointAddress);

                string usernameWS = String.Empty;
                string passwordWS = String.Empty;

                

                if (Azienda != null)
                {
                    WriteLog("-- Ricerca da parte di " + Azienda.m01_ragionesociale + " codice " + Azienda.m01_codice);
                    usernameWS =
                        Azienda.m01_net_user ?? this.NetUser;
                    passwordWS =
                        Azienda.m01_net_password ?? this.NetPassword;
                }
                else
                {
                    usernameWS = this.NetUser;
                    passwordWS = this.NetPassword;
                }

                wCdsClient.ClientCredentials.UserName.UserName = usernameWS;
                wCdsClient.ClientCredentials.UserName.Password = passwordWS;
            
                WriteLog("-- Utilizzate credenziali  user: " + usernameWS + " pwd : " + passwordWS);

            evaluateExended extendedObject = new evaluateExended();

            List<cdsAdditionalInputParameter> parameters = new List<cdsAdditionalInputParameter>
            {
                additionalInputObject
            };

            extendedObject.cdsEvaluationData = evaluationObject;
                extendedObject.cdsRequestData = requestObject;
                extendedObject.cdsAdditionalInputParameters = parameters.ToArray();

            

                evaluateExendedResponse response = wCdsClient.evaluateExended(extendedObject);
                if (response != null)
                {
                
                    cdsResponseExtended responseEx = response.cdsResultExtended.cdsResponseExtended;
                    if (responseEx != null)
                    {
                        long codiceRichiesta = responseEx.cdsRequestInfo.requestId ?? 0;
                    
                        if (codiceRichiesta != 0)
                        {
                            returnValue = codiceRichiesta.ToString();
                            this.WriteLog("OK - recuperato con successo codice " + codiceRichiesta+" per partitaiva " + partitaIva);
                            DBHandler.AggiornaCodiceRichiesta(codiceRichiesta, partitaIva, idLoggedUser);                        
                            
                            if (sendMail)
                            {
                            List<string> additionalData = new List<string>
                            {
                                azienda.name,
                                partitaIva,
                                codiceRichiesta.ToString()
                            };
                            MailHandler.WarnMail(idLoggedUser, "PURCHASED_REPORT", additionalData);
                            }
                        }
                    }
                    else
                    {
                        if (response.cdsResultExtended.cdsError != null)
                        {
                        
                            this.WriteLog("NOK - Errore " + response.cdsResultExtended.cdsError.cdsErrorType + " " + response.cdsResultExtended.cdsError.additionalDescription);
                        
                        }
                    }
                }


            
            
            
            



            


            return returnValue;
        }


        //public string BuyGlobalProfile(string partitaIva,int productCode,long idLoggedUser)
        //{
        //    string returnValue = string.Empty;            

        //    List<company> aziendeTrovate = SearchCompany(partitaIva, String.Empty, string.Empty);

        //    if (aziendeTrovate.Count == 0)
        //        return returnValue;

        //    company azienda =
        //        aziendeTrovate[0];

        //    DBHandler.AggiornaAziendaOsservatorio(azienda,idLoggedUser,productCode);


        //    List<product> foundproducts = GetAvaliableProducts(azienda);
        //    //product prodotto = foundproducts.Where(pr => pr.productCode == 5220).FirstOrDefault();
        //    /*MODIFICATO INTRODOTTO CODICE PER */
        //    product prodotto = foundproducts.Where(pr => pr.productCode == productCode).FirstOrDefault();

        //    buyReport reportToBuy = new buyReport();
        //    buyReportParams reportParameters = new buyReportParams();
        //    reportParameters.company = RetrievedCompany;


        //    reportParameters.language = language.ITALIAN;
        //    //reportParameters.mail = "silvio.costabile@expertweb.it";
        //    reportParameters.deliveryChannel = deliveryChannel.WEB;
            

            

        //    reportParameters.urgency = prodotto.availableUrgencies[0];
        //    reportParameters.product = prodotto;


        //    reportParameters.reason = enquiryReason.WEBSERVICE_REQUEST;
        //    reportToBuy.buyReportParams = reportParameters;


        //    // WCF MTOM -> JBoss MTOM: using CustomBinding
        //    //
        //    AddressHeader addressHeader1 = AddressHeader.CreateAddressHeader("retrieveReportWS", this.HostThreeStepWSAddress, 1);
        //    AddressHeader[] addressHeaders = new AddressHeader[1] { addressHeader1 };
        //    EndpointAddress endpointAddress = new EndpointAddress(
        //        new Uri(this.HostThreeStepWSAddress), addressHeaders);

        //    HttpsTransportBindingElement transportElement = new HttpsTransportBindingElement();
        //    transportElement.AuthenticationScheme = AuthenticationSchemes.Basic;
        //    transportElement.MaxReceivedMessageSize = 2147483647;
        //    transportElement.MaxBufferPoolSize = 2147483647;

        //    TextOrMtomEncodingBindingElement bindingElement = new TextOrMtomEncodingBindingElement();
        //    bindingElement.MessageVersion = MessageVersion.Soap11;
        //    CustomBinding mtomBinding = new CustomBinding(bindingElement, transportElement);



        //    ThreeStepWSClient wsRetrieve = new ThreeStepWSClient(mtomBinding, endpointAddress);


        //    wsRetrieve.ClientCredentials.UserName.UserName = this.NetUser;
        //    wsRetrieve.ClientCredentials.UserName.Password = this.NetPassword;


        //    buyReportResponse response = wsRetrieve.buyReport(reportToBuy);

        //    long codiceRichiesta = response.buyResult.requestCode.HasValue ? response.buyResult.requestCode.Value : 0;
        //    if (codiceRichiesta != 0)
        //    {
        //        DBHandler.AggiornaCodiceRichiesta(codiceRichiesta, azienda,idLoggedUser);
        //    }

          
        //    returnValue = codiceRichiesta.ToString();

        //    return returnValue;
        //}
        
        //public string   BuyGlobalRisk(string partitaIva, int productCode, long idLoggedUser)
        //{
        //    string returnValue = string.Empty;

        //    List<company> aziendeTrovate = SearchCompany(partitaIva, String.Empty, string.Empty);

        //    if (aziendeTrovate.Count == 0)
        //        return returnValue;

        //    company azienda =
        //        aziendeTrovate[0];

        //    DBHandler.AggiornaAziendaOsservatorio(azienda, idLoggedUser,productCode);


        //    List<product> foundproducts = GetAvaliableProducts(azienda);
        //    //product prodotto = foundproducts.Where(pr => pr.productCode == 5220).FirstOrDefault();
        //    /*MODIFICATO INTRODOTTO CODICE PER */
        //    product prodotto = foundproducts.Where(pr => pr.productCode == productCode).FirstOrDefault();

        //    buyReport reportToBuy = new buyReport();
        //    buyReportParams reportParameters = new buyReportParams();
        //    reportParameters.company = RetrievedCompany;


        //    reportParameters.language = language.ITALIAN;
        //    //reportParameters.mail = "silvio.costabile@expertweb.it";
        //    reportParameters.deliveryChannel = deliveryChannel.WEB;




        //    reportParameters.urgency = prodotto.availableUrgencies[0];
        //    reportParameters.product = prodotto;


        //    reportParameters.reason = enquiryReason.WEBSERVICE_REQUEST;
        //    reportToBuy.buyReportParams = reportParameters;


        //    // WCF MTOM -> JBoss MTOM: using CustomBinding
        //    //
        //    AddressHeader addressHeader1 = AddressHeader.CreateAddressHeader("retrieveReportWS", this.HostThreeStepWSAddress, 1);
        //    AddressHeader[] addressHeaders = new AddressHeader[1] { addressHeader1 };
        //    EndpointAddress endpointAddress = new EndpointAddress(
        //        new Uri(this.HostThreeStepWSAddress), addressHeaders);

        //    HttpsTransportBindingElement transportElement = new HttpsTransportBindingElement();
        //    transportElement.AuthenticationScheme = AuthenticationSchemes.Basic;
        //    transportElement.MaxReceivedMessageSize = 2147483647;
        //    transportElement.MaxBufferPoolSize = 2147483647;

        //    TextOrMtomEncodingBindingElement bindingElement = new TextOrMtomEncodingBindingElement();
        //    bindingElement.MessageVersion = MessageVersion.Soap11;
        //    CustomBinding mtomBinding = new CustomBinding(bindingElement, transportElement);



        //    ThreeStepWSClient wsRetrieve = new ThreeStepWSClient(mtomBinding, endpointAddress);


        //    wsRetrieve.ClientCredentials.UserName.UserName = this.NetUser;
        //    wsRetrieve.ClientCredentials.UserName.Password = this.NetPassword;


        //    buyReportResponse response = wsRetrieve.buyReport(reportToBuy);

        //    long codiceRichiesta = response.buyResult.requestCode.HasValue ? response.buyResult.requestCode.Value : 0;
        //    if (codiceRichiesta != 0)
        //    {
        //        DBHandler.AggiornaCodiceRichiesta(codiceRichiesta, azienda.vatNumber, idLoggedUser);                
        //        GetPurchasedReportGlobalRisk(codiceRichiesta,productCode,partitaIva);
        //        DBHandler.AggiornaEvasioneRichiesta(codiceRichiesta.ToString(), idLoggedUser);
        //    }


        //    returnValue = codiceRichiesta.ToString();

        //    return returnValue;
        //}

        //public List<product> GetAvaliableProducts(company azienda)
        //{

        //    List<product> returnValue = new List<product>();


            

        //    retrieveCompanyProducts product = new retrieveCompanyProducts();

        //    product.company = azienda;



        //    // WCF MTOM -> JBoss MTOM: using CustomBinding
        //    //
        //    AddressHeader addressHeader1 = AddressHeader.CreateAddressHeader("retrieveReportWS", HostThreeStepWSAddress, 1);
        //    AddressHeader[] addressHeaders = new AddressHeader[1] { addressHeader1 };
        //    EndpointAddress endpointAddress = new EndpointAddress(
        //        new Uri(HostThreeStepWSAddress), addressHeaders);

        //    HttpsTransportBindingElement transportElement = new HttpsTransportBindingElement();
        //    transportElement.AuthenticationScheme = AuthenticationSchemes.Basic;
        //    transportElement.MaxReceivedMessageSize = 2147483647;
        //    transportElement.MaxBufferPoolSize = 2147483647;

        //    TextOrMtomEncodingBindingElement bindingElement = new TextOrMtomEncodingBindingElement();
        //    bindingElement.MessageVersion = MessageVersion.Soap11;
        //    CustomBinding mtomBinding = new CustomBinding(bindingElement, transportElement);



        //    ThreeStepWSClient wsRetrieve = new ThreeStepWSClient(mtomBinding, endpointAddress);


        //    wsRetrieve.ClientCredentials.UserName.UserName = NetUser;
        //    wsRetrieve.ClientCredentials.UserName.Password = NetPassword;


        //    retrieveCompanyProductsResponse searchedProducts = wsRetrieve.retrieveCompanyProducts(product);

        //    CompanyProductListResult resultProducts = searchedProducts.productListResult;

        //    returnValue = resultProducts.products.ToList();

        //    RetrievedCompany = resultProducts.company;
            


        //    return returnValue;

        //}



        
    }
}
