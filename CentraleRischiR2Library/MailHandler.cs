using System;
using System.Collections.Generic;
using CentraleRischiR2Library.BridgeClasses;
using System.Net.Mail;
using System.Net.Mime;
using System.IO;
using System.Configuration;
using log4net;
using System.Net;
using AegisImplicitMail;
using System.ComponentModel;
using Newtonsoft.Json;

namespace CentraleRischiR2Library
{
    public class MailHandler
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static bool SendMail(long idUtente, string warningType, string piva, string ragioneSociale, string mercato, string mailTo,string body, string centro)
        {
            bool retVal= true;
            string mailBody = String.Empty;
            string mailSubject = String.Empty;
            string mailFrom = String.Empty;
            string customMail = String.Empty;
            SmtpClient smtpClient = new SmtpClient("smtps.osservamercati.it");
            smtpClient.Port=465;
            
            try
            {
                
                MailMessage message = new MailMessage();
                smtpClient.EnableSsl = true;
                smtpClient.UseDefaultCredentials = false; // [3] Changed this
                smtpClient.Credentials = new NetworkCredential("admin@osservamercati.it", "8su@2UmC");

                switch (warningType)
                {
                    case "RECEIVED_REPORT":
                        mailSubject = "Piattaforma OSSERVAMERCATI - Evasione Rapporto - Da Operatore " + idUtente;
                        mailBody += @"<p>Gentile Mercato,<br /> E' stato richiesto della Piattaforma OSSERVAMERCATI per il MERCATO:" + mercato + " dall OPERATORE: " + idUtente + " il rapporto informativo richiesto sulla PARTITA IVA: " + piva + ";<br /><br />" +
                                                    "Cordiali saluti,<br />" +
                                                    "<b>Osserva Srl</b><br /><br />" +
                                                    "<a href='mailto:info@osservamercati.it'>info@osservamercati.it</a><br />" +
                                                    "<a href='http://www.osservamercati.it'>http://www.osservamercati.it</a>" + piva + ragioneSociale;

                        //message.Attachments.Add(new Attachment(additionalData[3]));
                        break;
                    case "INVIO_DATI":
                         mailSubject = " Report invio dati Aziende Mercato di "+centro;
                         mailBody = body;
                         smtpClient.Send("support@osservamercati.it", mailTo, mailSubject, body);
                        break;
                }
                if (mailBody.Equals(""))
                {
                    mailBody = "Trasmissioni tutte ok";
                }
                message.Subject = mailSubject;
                message.Body = mailBody;
                message.From = new MailAddress("support@osservamercati.it");
                message.To.Add(mailTo);
               // message.To.Add("snardone1@virgilio.it");
                //message.IsBodyHtml = true;
                //Log.Info("email pronta all'invio from: " + ConfigurationManager.AppSettings["From"] + " To : " +  mailTo+ " partita iva richiesta= " + piva + " ragione sociale= ");
                
                smtpClient.Send(message);
            }
            catch (Exception ex)
            {
                Log.Error("error :" + ex.ToString());
                smtpClient.Send("admin@osservamercati.it", "info@mafcoit.it", "error send Mail", ex.ToString());
                retVal = false;
            }
            return retVal;
        }

        static public Boolean SendMail(string mailTo, string body)
        {
            Boolean retVal = false;
            Log.Info("START SendMail ****");
            //TEST per non mandare mail a tutti ma indirizzo unico // mailTo = "francesco.capparucci@gmail.com";//prova per test
            var httpWebRequest = (HttpWebRequest)WebRequest.Create("http://localhost:8009/sendMail");
            httpWebRequest.ContentType = "application/json; charset=utf-8";
            httpWebRequest.Method = "POST";
            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string json = "{\"subject\":\"" + "Controllo Invio Dati Operatori Osserva" + "\",\"mailto\":\"" + mailTo + "\",\"body\":\"" + body + "\"}";
                Log.Debug("json invio " + json);
                streamWriter.Write(json.ToString());
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
                }
                retVal = true;
            }
            catch (WebException ex)
            {
                Log.Error("errore invio dati " + ex.ToString());
                using (WebResponse response = ex.Response)
                {
                    HttpWebResponse httpResponse2 = (HttpWebResponse)response;
                }
            }
            return retVal;
        }

        //Call back function
        public static bool SendMailAruba(long idUtente, string warningType, string piva, string ragioneSociale, string mercato, string[] mailTo, string body, string centro)
        {
            bool retVal = true;
            var mail = "support@osservamercati.it";
            var host = "smtps.aruba.it";
            var user = "admin@osservamercati.it";
            var pass = "8su@2UmC";

            var mymessage = new MimeMailMessage();
            mymessage.From = new MimeMailAddress(mail);
            mymessage.IsBodyHtml = true;
            foreach(string element in mailTo)
            {
                mymessage.To.Add(element);
            }
            mymessage.Subject = " Report invio dati Aziende Mercato di " + centro;
            mymessage.Body = body;
            var mailer = new MimeMailer("smtps.aruba.it", 465);
            mailer.User = user;
            mailer.Password = pass;
            mailer.SslType = SslMode.Ssl;
            mailer.AuthenticationMode = AuthenticationType.Base64;
            if(mymessage.Body.Equals(""))
            {
                mymessage.Body = "Trasmissioni tutte ok";
            }
           
            mailer.SendMailAsync(mymessage);
            
            return retVal;
        }

        public static bool WarnMail(long idUtente,string warningType,List<string> additionalData)
        {
            bool returnValue = false;
            string mailBody= String.Empty;
            string mailSubject= String.Empty;
            string mailFrom = String.Empty;
            string customMail = String.Empty;

            NavigationUser loggedUser = DBHandler.GetUtente(idUtente);

            MailMessage message = new MailMessage();

            Log.Info("RICHIESTO INVIO MAIL UTENTE - " + loggedUser.Azienda + " per " + warningType);
            

            switch(warningType)
            {

                    case "RC_SOLLECITO":
                    mailSubject = "Piattaforma OSSERVAMERCATI - Sollecito Pagamento " + additionalData[1];
                    mailBody = additionalData[2];
                    customMail = "andrea.codeluppi@osservamercati.it";
                    break;

                    case "RC_INSERIMENTO":
                    mailSubject = "Piattaforma OSSERVAMERCATI - Richiesta Recupero Crediti per " + additionalData[0] + " - PIVA " + additionalData[1] + "  - RIF. " + additionalData[2];
                    mailBody = String.Format(@"<p>Gentile Cliente,<br />
La Piattaforma OSSERVAMERCATI ha preso in gestione la sua richiesta di Recupero Crediti su: {0} - PIVA {1} RIF. {2}<br /><br />
Le ricordiamo che siamo a disposizione per qualsiasi informazione.<br /><br />
Cordiali saluti,<br />
<b>Osserva Srl</b><br /><br />
<a href='mailto:info@osservamercati.it'>info@osservamercati.it</a><br />
<a href='http://www.osservamercati.it'>http://www.osservamercati.it</a>", additionalData[0], additionalData[1], additionalData[2]);
                    
                break;
                case "PURCHASED_REPORT":
                    mailSubject = "Piattaforma OSSERVAMERCATI - Richiesta Rapporto " + additionalData[0] + " - PIVA " + additionalData[1] + "  - RIF. " + additionalData[2];
                    mailBody = String.Format(@"<p>Gentile Cliente,<br />
La Piattaforma OSSERVAMERCATI ha preso in gestione la sua richiesta di evasione del rapporto informativo richiesto su: {0} - PIVA {1} RIF. {2}<br /><br />
Riceverá una comunicazione appena il rapporto sará disponibile nella sezione NEWS PREFERITI sul sito <a href='http://www.osservamercati.it'>http://www.osservamercati.it</a><br /><br />
Le ricordiamo che siamo a disposizione per qualsiasi informazione.<br /><br />
Cordiali saluti,<br />
<b>Osserva Srl</b><br /><br />
<a href='mailto:info@osservamercati.it'>info@osservamercati.it</a><br />
<a href='http://www.osservamercati.it'>http://www.osservamercati.it</a>", additionalData[0], additionalData[1], additionalData[2]);
                    
                break;

                case "RECEIVED_REPORT":
                mailSubject = "Piattaforma OSSERVAMERCATI - Evasione Rapporto - " + additionalData[0] + " - PIVA " + additionalData[1] ;
                mailBody = String.Format(@"<p>Gentile Cliente,<br />
Nella sezione è stato richiesto della Piattaforma OSSERVAMERCATI il rapporto informativo richiesto su: {0} - PIVA {1};<br /><br />
La ringraziamo per la preferenza accordataci e le auguriamo Buona Giornata.<br /><br />
Cordiali saluti,<br />
<b>Osserva Srl</b><br /><br />
<a href='mailto:info@osservamercati.it'>info@osservamercati.it</a><br />
<a href='http://www.osservamercati.it'>http://www.osservamercati.it</a>", additionalData[0], additionalData[1]);
                //message.Attachments.Add(new Attachment(additionalData[3]));

                break;

                case "UPDATE_MONITORING":
                mailSubject = "Piattaforma OSSERVAMERCATI - Aggiornamento Monitoraggio - " + additionalData[0] + " - PIVA " + additionalData[1] + "  - RIF. " + additionalData[2];
                mailBody = String.Format(@"<p>Gentile Cliente,<br />
Il servizio monitoraggio Cerved Group ha rilevato variazioni a carico di: {0} - PIVA {1} RIF. {2};<br /><br />
<b>{3}</b><br /><br />
Il rapporto e tutte le eventuali altre segnalazioni possono essere consultate nel sito <a href='http://www.osservamercati.it'>http://www.osservamercati.it</a><br /><br />
Le ricordiamo che siamo a disposizione per qualsiasi informazione.<br /><br />
Cordiali saluti,<br />
<b>Osserva Srl</b><br /><br />
<a href='mailto:info@osservamercati.it'>info@osservamercati.it</a><br />
<a href='http://www.osservamercati.it'>http://www.osservamercati.it</a>", additionalData[0], additionalData[1], additionalData[2], additionalData[3]);

                break;
            }

            FileInfo template = new FileInfo(ConfigurationManager.AppSettings["HtmlTemplatePath"] + "\\template.html");
            StreamReader stream = template.OpenText();
            string htmlTemplate = stream.ReadToEnd();
            stream.Close();
            
            string htmlBody = htmlTemplate.Replace("{%TITLE%}", mailSubject).Replace("{%CONTENT%}", mailBody);

            AlternateView htmlView = AlternateView.CreateAlternateViewFromString(htmlBody, null, "text/html");

            // aggiunge un allegato al messaggio HTML
            LinkedResource logo = new LinkedResource(ConfigurationManager.AppSettings["HtmlTemplatePath"] + "\\logo_osserva.gif");
            logo.ContentId = "logoOsserva";
            logo.ContentType = new ContentType("image/gif");
            htmlView.LinkedResources.Add(logo);

            SmtpClient smtpClient = new SmtpClient(ConfigurationManager.AppSettings["SmtpServer"]);
            message.Subject = mailSubject;
            message.Body = mailBody;
            message.From = new MailAddress(ConfigurationManager.AppSettings["From"]);

            message.AlternateViews.Add(htmlView);

            if (customMail != String.Empty)
            {
                foreach (string address in customMail.Split(';'))
                    message.To.Add(new MailAddress(address));
            }
            else
            {
                if (loggedUser != null)
                {
                    foreach (string address in loggedUser.Email.Split(';'))
                        message.To.Add(new MailAddress(address));
                }
            }

            if(ConfigurationManager.AppSettings["BCC"] != null)
            {
                foreach (string address in ConfigurationManager.AppSettings["BCC"].Split(';'))
                    message.Bcc.Add(new MailAddress(address));          
            }


            try{
                smtpClient.Send(message);
                Log.Info("INVIO EFFETTUATO Ok su smtp " + ConfigurationManager.AppSettings["SmtpServer"] + " da " + message.From.Address + " a to: " + message.To[0] + " e bcc: " + message.Bcc[0]);
                returnValue = true;
            }
            catch(Exception e)
            {
                Log.Error("ERRORE - " + e.InnerException);
            }
            

            return returnValue;
        }
    }
}
