using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Configuration;
using Osserva.CentraleRischi.Library;
using Osserva.CentraleRischi.Library.Report;
using CentraleRischiR2Library;
using System.Net.Mail;

namespace CheckRequestedReports
{
    class Program
    {
        static void Main(string[] args)
        {

            Console.WriteLine("Verifica generazione report...");
            using (DemoR2Entities context = new DemoR2Entities())
            {
                List<richiesta_report> richiesteInevase = context.richiesta_report.Where(richiesta => richiesta.evasa == false).ToList();
                foreach (richiesta_report richiesta in richiesteInevase)
                {
                    bool generatedReport = false;
                    string email = richiesta.email_utente;
                    string subject = "";
                    string body = "";
                    string reportFile = "";
                    switch (richiesta.tipo_richiesta)
                    {
                        case "REPORT_FORNITORI":
                            subject = "Report Fornitore su Partita Iva " + richiesta.chiave.Trim();
                            body = "In allegato il Report Fornitore su Partita Iva " + richiesta.chiave.Trim();
                            string partitaIva = richiesta.chiave.Trim();
                            int idCentro = richiesta.id_centro ?? 0;
                            DateTime dataInizio = richiesta.data_inizio ?? DateTime.Now;
                            DateTime dataFine = richiesta.data_fine ?? DateTime.Now;
                            Report reportObj = new SaldiVenditeClienteReport
                            {
                                DataInizio = dataInizio,
                                DataFine = dataFine,
                                IdCentro = idCentro,
                                PartitaIva = partitaIva
                            };
                            MemoryStream report = (MemoryStream)reportObj.GetReport(1000, 0);
                            if (report != null)
                            {
                                reportFile = ConfigurationManager.AppSettings["PathReport"] + partitaIva + ".xls";
                                FileStream fileStream = File.Create(reportFile, (int)report.Length);
                                //Initialize the bytes array with the stream length and then fill it with data
                                byte[] bytesInStream = new byte[report.Length];
                                report.Read(bytesInStream, 0, bytesInStream.Length);
                                //Use write method to write to the file specified above
                                fileStream.Write(bytesInStream, 0, bytesInStream.Length);
                                fileStream.Close();
                                generatedReport = true;
                            }
                            break;
                        case "REPORT_RP":
                            subject = "Report Risk Predictor su Operatore Codice " + richiesta.chiave.Trim();
                            body = "In allegato il Report Risk Predictor su Operatore Codice " + richiesta.chiave.Trim();
                            string codice = richiesta.chiave.Trim();                            
                            dataInizio = richiesta.data_inizio ?? DateTime.Now;
                            dataFine = richiesta.data_fine ?? DateTime.Now;
                            reportObj = new RiskPredictorReport();
                            reportObj.DataInizio = dataInizio;
                            reportObj.DataFine = dataFine;                            
                            reportObj.CodiceAzienda = codice;
                            report = (MemoryStream)reportObj.GetReport(codice, 1000, 0);
                            if (report != null)
                            {
                                reportFile = ConfigurationManager.AppSettings["PathReport"] + codice + "_RP.xls";
                                FileStream fileStream = File.Create(reportFile, (int)report.Length);
                                //Initialize the bytes array with the stream length and then fill it with data
                                byte[] bytesInStream = new byte[report.Length];
                                report.Read(bytesInStream, 0, bytesInStream.Length);
                                //Use write method to write to the file specified above
                                fileStream.Write(bytesInStream, 0, bytesInStream.Length);
                                fileStream.Close();
                                generatedReport = true;
                            }
                            break;
                        case "REPORT_CRC":
                            var centro = context.m00_centri.Where(c => c.m00_idcentro == richiesta.id_centro.Value).FirstOrDefault();
                            subject = $"Report CRC su Operatore Codice {richiesta.chiave.Trim()} Mercato {centro.m00_localita}";
                            body = $"In allegato il Report CRC su Operatore Codice {richiesta.chiave.Trim()} Mercato {centro.m00_localita}";
                            codice = richiesta.chiave.Trim();
                            dataInizio = richiesta.data_inizio ?? DateTime.Now;
                            dataFine = richiesta.data_fine ?? DateTime.Now;
                            reportObj = new CRCReport();
                            reportObj.DataInizio = dataInizio;
                            reportObj.DataFine = dataFine;
                            reportObj.CodiceAzienda = codice;
                            reportObj.IdCentro = richiesta.id_centro ?? 0;
                            report = (MemoryStream)reportObj.GetReport(codice, 1000, 0);
                            
                            if (report != null)
                            {
                                reportFile = $"{ConfigurationManager.AppSettings["PathReport"]}{codice}_{reportObj.IdCentro}_CRC.xls";
                                FileStream fileStream = File.Create(reportFile, (int)report.Length);
                                //Initialize the bytes array with the stream length and then fill it with data
                                byte[] bytesInStream = new byte[report.Length];
                                report.Read(bytesInStream, 0, bytesInStream.Length);
                                //Use write method to write to the file specified above
                                fileStream.Write(bytesInStream, 0, bytesInStream.Length);
                                fileStream.Close();
                                generatedReport = true;
                            }
                            break;
                        case "REPORT_GLOBALE":
                            subject = "Report Globale su Operatore Codice " + richiesta.chiave.Trim();
                            body = "In allegato il Report Globale su Operatore Codice " + richiesta.chiave.Trim();
                            codice = richiesta.chiave.Trim();
                            dataInizio = richiesta.data_inizio.HasValue ? richiesta.data_inizio.Value : DateTime.Now;
                            dataFine = richiesta.data_fine.HasValue ? richiesta.data_fine.Value : DateTime.Now;
                            reportObj = new ReportGlobaleAzienda();
                            reportObj.DataInizio = DateTime.Now;
                            reportObj.DataFine = DateTime.Now;
                            reportObj.CodiceAzienda = codice;
                            report = (MemoryStream)reportObj.GetReport(codice, 1000, 0);
                            if (report != null)
                            {
                                reportFile = ConfigurationManager.AppSettings["PathReport"] + codice + "_GLOBALE.xls";
                                FileStream fileStream = File.Create(reportFile, (int)report.Length);
                                //Initialize the bytes array with the stream length and then fill it with data
                                byte[] bytesInStream = new byte[report.Length];
                                report.Read(bytesInStream, 0, bytesInStream.Length);
                                //Use write method to write to the file specified above
                                fileStream.Write(bytesInStream, 0, bytesInStream.Length);
                                fileStream.Close();
                                generatedReport = true;
                            }
                            break;

                        case "REPORT_CN":
                            subject = "Report Classifica Nazionale";
                            body = "In allegato il Report Classifica Nazionale su " + richiesta.chiave.Trim()+ " aziende";
                            codice = richiesta.chiave.Trim();
                            dataInizio = richiesta.data_inizio.HasValue ? richiesta.data_inizio.Value : DateTime.Now;
                            dataFine = richiesta.data_fine ?? DateTime.Now;
                            reportObj = new CNReport
                            {
                                DataInizio = dataInizio,
                                DataFine = dataFine
                            };
                            int numeroAziende = Convert.ToInt32(codice);
                            report = (MemoryStream)reportObj.GetReport(codice, numeroAziende, 0);
                            if (report != null)
                            {
                                reportFile = ConfigurationManager.AppSettings["PathReport"] + codice + "_CN.xls";
                                FileStream fileStream = File.Create(reportFile, (int)report.Length);
                                //Initialize the bytes array with the stream length and then fill it with data
                                byte[] bytesInStream = new byte[report.Length];
                                report.Read(bytesInStream, 0, bytesInStream.Length);
                                //Use write method to write to the file specified above
                                fileStream.Write(bytesInStream, 0, bytesInStream.Length);
                                fileStream.Close();
                                generatedReport = true;
                            }
                            break;
                    }
                    if (generatedReport)
                    { 
                        SmtpClient smtpClient = new SmtpClient(ConfigurationManager.AppSettings["SmtpServer"]);

                        MailMessage message = new MailMessage
                        {
                            Body =
                            body,

                            Subject = subject,

                            From = new MailAddress(ConfigurationManager.AppSettings["From"])
                        };


                        foreach (string address in email.Split(';'))
                                message.To.Add(new MailAddress(address));                                
                                                                                                                
                            try
                            {

                                message.Attachments.Add(new Attachment(reportFile));

                                smtpClient.Send(message);
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine(" --- Mail Inviata OK");
                                Console.ForegroundColor = ConsoleColor.Black;
                                richiesta.evasa = true;
                                richiesta.data_evasione = DateTime.Now;
                                context.SaveChanges();
                            }
                            catch (Exception ex)
                            {
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine(" --- Mail Inviata NOK !!! " + ex.InnerException.Message);
                                Console.ForegroundColor = ConsoleColor.Black;
                            }

                    }
                    
                    
                }

            }
                
            }
        }
    }

