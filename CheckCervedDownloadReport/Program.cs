using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Net.Mail;
using System.IO;
using System.Xml.Linq;
using CentraleRischiR2Library;
using CentraleRischiR2Library.BridgeClasses;
using CentraleRischiR2Library.CervedRetrieveReportServiceReference;
using CentraleRischiR2Library.CervedThreeStepProdServiceReference;



namespace InsertDownloadReport
{
    class Program
    {

        public static void WriteLog(string message)
        {

            StreamWriter sw = new StreamWriter(String.Format("{0}log{1:yyyyMMdd}.log", ConfigurationManager.AppSettings["LogWSPath"].ToString(), DateTime.Now), true);
            sw.WriteLine(DateTime.Now.ToString("g") + ": " + message);
            Console.WriteLine(DateTime.Now.ToString("g") + ": " + message);
            sw.Flush();
            sw.Close();
                        
        }

        static void Main(string[] args)
        {

            WriteLog("CERVED WS CHECK - Recupero codici Richiesta...");
            

            using (DemoR2Entities context = new DemoR2Entities())
            {

                List<cerved_check> reportDownload =
                    context.cerved_check.Where(check => check.evaso == false && check.codice_cerved != "P")
                    .Join(context.m10_utenti.Where(u => u.m10_attivo == true && u.m10_attivo_rircerca == true),
                    a => a.id_utente,
                    b => b.m10_iduser,
                    (a,b) => a)
                    .ToList();

                WriteLog("-- " + reportDownload.Count + " Codici Richiesti recuperati......OK");
                foreach (cerved_check aziendaCheck in reportDownload)
                {
                    

                    try
                    {


                        vs_aziende_osservamercati aziendaUtente = context.m10_utenti.Where(ut => ut.m10_iduser == aziendaCheck.id_utente)
                        .Join(context.vs_aziende_osservamercati,
                        a => a.m10_idazienda,
                        b => b.m01_idazienda,
                        (a, b) => b).FirstOrDefault();

                        CervedWSHandler cervedHandler = new CervedWSHandler(aziendaUtente,ConfigurationManager.AppSettings["ThreeStepWSUrl"].ToString(), ConfigurationManager.AppSettings["RetrieveReportWSUrl"].ToString(), "", ConfigurationManager.AppSettings["CDSStepWSUrl"].ToString(), ConfigurationManager.AppSettings["NETUSERCDS"].ToString(), ConfigurationManager.AppSettings["NETPASSWORDCDS"].ToString(), ConfigurationManager.AppSettings["ReportPath"].ToString(), ConfigurationManager.AppSettings["LogWSPath"].ToString());
                        m02_anagrafica azienda = context.m02_anagrafica.Where(a => a.m02_partitaiva == aziendaCheck.partita_iva).FirstOrDefault();

                        WriteLog(" --- Richiesta per azienda RAGIONE SOCIALE :" + azienda.m02_ragionesociale + " PIVA : " + azienda.m02_partitaiva + " CODICE CERVED : " + aziendaCheck.codice_cerved);

                        if (cervedHandler.GetPurchasedReportCDS(Convert.ToInt32(aziendaCheck.codice_cerved), 55220, azienda.m02_partitaiva))
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            WriteLog(" --- Report Scaricato OK");
                            Console.ForegroundColor = ConsoleColor.Black;
                            DBHandler.AggiornaEvasioneRichiesta(aziendaCheck.codice_cerved,aziendaCheck.id_utente);
                            if (cervedHandler.ParseXMLCDS(cervedHandler.ReportPath + "\\" + aziendaCheck.codice_cerved + ".xml", aziendaCheck.partita_iva, aziendaCheck.codice_cerved, aziendaCheck.id_utente,"Nuovo Report",DateTime.Now))
                            {                                
                                WriteLog(" --- File XML Letto     OK");                                
                            }
                            else
                            {                                
                                WriteLog(" --- File XML Letto     NOK");                                
                            }
                            /*
                            List<string> additionalData = new List<string>();

                            additionalData.Add(azienda.m02_ragionesociale);
                            additionalData.Add(azienda.m02_partitaiva);
                            additionalData.Add(aziendaCheck.codice_cerved);
                            additionalData.Add(cervedHandler.ReportPath + "\\" + aziendaCheck.codice_cerved + ".pdf");


                            MailHandler.WarnMail(aziendaCheck.id_utente, "RECEIVED_REPORT", additionalData);
                            */
                            

                            /*
                            SmtpClient smtpClient = new SmtpClient(ConfigurationManager.AppSettings["SmtpServer"]);

                            MailMessage message = new MailMessage();

                            message.Subject = "Report Global Profile Azienda " + azienda.m02_ragionesociale + " PartitaIva " + azienda.m02_partitaiva;
                                
                            message.Body =
                                "In allegato il rapporto informativo Global Profile per " + azienda.m02_ragionesociale + " PartitaIva " + azienda.m02_partitaiva;

                            message.From = new MailAddress(ConfigurationManager.AppSettings["From"]);



                            m10_utenti utente = context.m10_utenti.Where(u => u.m10_iduser == aziendaCheck.id_utente).FirstOrDefault();


                            if (utente != null)
                            {
                                foreach (string address in utente.m10_email.Split(';'))
                                    message.To.Add(new MailAddress(address));                                
                            }
                            


                            if (ConfigurationManager.AppSettings["To"] != String.Empty)
                                foreach (string address in ConfigurationManager.AppSettings["To"].Split(';'))
                                    message.To.Add(new MailAddress(address));

                            
                            try
                            {

                                message.Attachments.Add(new Attachment(cervedHandler.ReportPath + "\\" + aziendaCheck.codice_cerved + ".pdf"));

                                smtpClient.Send(message);
                                
                                WriteLog(" --- Mail Inviata OK");
                                
                            }
                            catch (Exception ex)
                            {
                                
                                WriteLog(" --- Mail Inviata NOK !!! " + ex.InnerException.Message);
                                
                            }
                            */
                        }
                        
                    }
                    catch(Exception ex){                        
                        WriteLog(" --- Mail Inviata     NOK !!! -- " +  ex.Message);                        
                    }
                }

                

            }
            
        }
    }
}
