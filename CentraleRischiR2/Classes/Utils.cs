using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Reflection;
using System.Web.Configuration;
using System.Net;
using System.Net.Mail;
using System.IO;
using Osserva.CentraleRischi.Library;
using Osserva.CentraleRischi.Library.Import;
using log4net;

namespace CentraleRischiR2.Classes
{
    public class SendMail
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public void Send(String MailTo, String MailSubject, String Body, String Attachment)
        {

            string MailFrom = WebConfigurationManager.AppSettings["From"];
            string MailSmtpServer = WebConfigurationManager.AppSettings["SmtpServer"];


            System.Net.Mail.MailMessage mail = new System.Net.Mail.MailMessage();

            mail.From = new System.Net.Mail.MailAddress(MailFrom);
            mail.To.Add(new System.Net.Mail.MailAddress(MailTo));
            mail.Subject = MailSubject;
            mail.Priority = System.Net.Mail.MailPriority.High;

            mail.Body = Body;


            if (Attachment != String.Empty && Attachment != null)
            {
                System.Net.Mail.Attachment myAttachment = new System.Net.Mail.Attachment(Attachment);
                mail.Attachments.Add(myAttachment);
            }
            try
            {
                System.Net.Mail.SmtpClient server = new System.Net.Mail.SmtpClient();
                server.Host = MailSmtpServer;

                if (WebConfigurationManager.AppSettings["SmtpAuth"] == "True")
                {
                    server.Credentials = new NetworkCredential(WebConfigurationManager.AppSettings["SmtpUser"],
                                                               WebConfigurationManager.AppSettings["SmtpPassword"]
                                                              );

                }
                server.Send(mail);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }

        }

        public void Send(String MailFrom, String MailTo, String MailSubject, String Body, String Attachment)
        {


            string MailSmtpServer = WebConfigurationManager.AppSettings["SmtpServer"];


            System.Net.Mail.MailMessage mail = new System.Net.Mail.MailMessage();

            mail.From = new System.Net.Mail.MailAddress(MailFrom);
            mail.To.Add(new System.Net.Mail.MailAddress(MailTo));
            //  mail.Bcc.Add(new System.Net.Mail.MailAddress("salvatore.santonicola@osservamercati.it"));
            mail.Subject = MailSubject;
            mail.Priority = System.Net.Mail.MailPriority.High;
            if(Body != null)
            {
                mail.Body = Body;
            }
            else
            {
                mail.Body = "Empty";
            }
            if (Attachment != String.Empty && Attachment != null)
            {
                System.Net.Mail.Attachment myAttachment = new System.Net.Mail.Attachment(Attachment);
                mail.Attachments.Add(myAttachment);
            }
            try
            {
                System.Net.Mail.SmtpClient server = new System.Net.Mail.SmtpClient();
                server.Host = MailSmtpServer;

                if (WebConfigurationManager.AppSettings["SmtpAuth"] == "True")
                {
                    server.Credentials = new NetworkCredential(WebConfigurationManager.AppSettings["SmtpUser"],
                                                               WebConfigurationManager.AppSettings["SmtpPassword"]
                                                              );

                }
                server.Send(mail);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }

        }


        public void SendCustom(String MailTo, String MailCC, String MailBCC, String MailSubject, String Body, String Attachment)
        {


            string MailFrom = WebConfigurationManager.AppSettings["From"];
            string MailSmtpServer = WebConfigurationManager.AppSettings["SmtpServer"];
            System.Net.Mail.MailMessage mail = new System.Net.Mail.MailMessage();

            string[] toAddresses = MailTo.Split(';');
            foreach (string address in toAddresses)
            {
                if (address != String.Empty)
                    mail.To.Add(new System.Net.Mail.MailAddress(address));
            }

            string[] ccAddresses = MailCC.Split(';');
            foreach (string address in ccAddresses)
            {
                if (address != String.Empty)
                    mail.CC.Add(new System.Net.Mail.MailAddress(address));
            }

            string[] bCCAddresses = MailBCC.Split(';');
            foreach (string address in bCCAddresses)
            {
                if (address != String.Empty)
                    mail.Bcc.Add(new System.Net.Mail.MailAddress(address));
            }



            string template = @"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Strict//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd"">
                            <html xmlns=""http://www.w3.org/1999/xhtml"" xml:lang=""it"" lang=""it"">
	<head>
		<title></title>
		<meta http-equiv=""content-type"" content=""text/html;charset=utf-8"" />
		<style type=""text/css"">
			body {
				font-family: Trebuchet MS, sans-serif;
			}
			table {
				border-collapse: collapse;
				margin-bottom: 1.5em;
			}
			table, th, td {
				border: 1px solid rgb(147, 154, 160);
			}
			tr.alt {
				background-color: rgb(242, 243, 244);
			}
			th {
				background-color: rgb(232, 233, 234);
				color: rgb(249, 151, 28);
				padding: 0.2em;
			}
			td {
				padding: 0.5em;
			}
			address {
				border-top: 1px dotted rgb(147, 154, 160);
				margin: 1em 0;
			}
			address img {
				padding: 1em 0;
			}
		</style>
	</head>
	<body>    
		{%CONTENT%}
	<address>
		<img src=""cid:logoOsserva"" alt=""Logo Osserva S.r.l."" width=""142"" height=""49"" /><br />
		<strong>Osserva S.r.l.</strong><br /><br />Sede Legale: Via Giuseppe Ferrari 12, 00195 Roma<br />Sede Amministrativa: Via Giardini Nord 121/123 - 41043 Formigine(MO)<br />
		P.I. 03158470363<br />
		<a href=""mailto:info@osservamercati.it"">info@osservamercati.it</a>
	</address>
	</body>
</html>";

            template = template.Replace("{%CONTENT%}", Body);



            mail.From = new System.Net.Mail.MailAddress(MailFrom);

            //  mail.Bcc.Add(new System.Net.Mail.MailAddress("salvatore.santonicola@osservamercati.it"));
            mail.Subject = MailSubject;
            mail.Priority = System.Net.Mail.MailPriority.High;



            AlternateView htmlView = AlternateView.CreateAlternateViewFromString(template, null, "text/html");
            //LinkedResource logo = new LinkedResource(WebConfigurationManager.AppSettings["PathImg"] + "logo_osserva.gif");
            //logo.ContentId = "logoOsserva";
            //logo.ContentType = new ContentType("image/gif");
            //htmlView.LinkedResources.Add(logo);


            mail.AlternateViews.Add(htmlView);

            if (Attachment != String.Empty && Attachment != null)
            {
                if (File.Exists(Attachment))
                {
                    System.Net.Mail.Attachment myAttachment = new System.Net.Mail.Attachment(Attachment);
                    mail.Attachments.Add(myAttachment);
                }
            }
            try
            {
                System.Net.Mail.SmtpClient server = new System.Net.Mail.SmtpClient();
                server.Host = MailSmtpServer;
                if (WebConfigurationManager.AppSettings["SmtpAuth"] == "True")
                {
                    server.Credentials = new NetworkCredential(WebConfigurationManager.AppSettings["SmtpUser"],
                                                               WebConfigurationManager.AppSettings["SmtpPassword"]
                                                              );

                }


                server.Send(mail);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }

        }
    }

    public partial class Utils
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static bool Reimport(string codiceAzienda, int numeroMesi)
        {
            bool returnValue = false;
            DataTable tbAziende = new DataTable(); // DataTable di una o tutte le aziende

            tbAziende = DBController.GetAziende(codiceAzienda.ToString()); // codice azienda attivo
            // cicla tutte le aziende selezionate
            foreach (DataRow dr in tbAziende.Rows)
            {
                // scrive sul log e sulla console l'azienda corrente
                string nomeAzienda = String.Format("Azienda: {0}", dr["ragione_sociale"].ToString());
                nomeAzienda = dr["ragione_sociale"].ToString();

                Log.Info("start reimport" + " nome azienda=" + nomeAzienda + " numeroMesi= " + numeroMesi);

                string libNamespace = "Osserva.CentraleRischi.Library.Import."; // nome del namespace della libreria

                // nome completo della classe da utilizzare con la reflection (basato sul campo tipo_gestionale)
                string instanceFullName = String.Format(
                    libNamespace + "Import{0}", dr["classe_gestionale"].ToString().Trim());


                // viene utilizzata la reflection per instanziare la sottoclasse corretta di ImportBase
                ImportData daily = (ImportData)
                    Assembly.GetAssembly(typeof(ImportData)).CreateInstance(instanceFullName);

                // procede soltanto se la classe è stata istanziata correttamente
                if (daily != null)
                {
                    List<string> credentials = new List<string>(50);
                    bool gestionaleCorretto = true; // viene impostato a false in caso di gestionale non supportato (es. NULL)

                    // modifica l'elenco delle credenziali a seconda del tipo di gestionale
                    switch (dr["tipo_gestionale"].ToString().ToUpper())
                    {
                        case "MYSQL":
                            credentials.Add(WebConfigurationManager.AppSettings["ImportLogPath"]);
                            credentials.Add(dr["codice"].ToString());
                            credentials.Add(dr["server"].ToString());
                            credentials.Add(dr["db_mysql"].ToString());
                            credentials.Add(dr["db_user"].ToString());
                            credentials.Add(dr["db_password"].ToString());
                            credentials.Add(numeroMesi.ToString());

                            // aggiunge alle credenziali il flag per la modalità verbosa
                            credentials.Add("false");
                            /*Per ora il solo strafrutta */
                            //string encryptState = dr["id_centro"].ToString() == "8" ? "encrypt" : "noEncrypt";
                            string encryptState = dr["codice"].ToString() == "8117" ? "encrypt" : "noEncrypt";
                            credentials.Add(encryptState);
                            break;
                        case "FTP":
                            credentials.Add(WebConfigurationManager.AppSettings["ImportLogPath"]);
                            credentials.Add(dr["codice"].ToString());
                            credentials.Add(WebConfigurationManager.AppSettings["TxtPath"]);
                            credentials.Add(dr["cartella"].ToString());
                            credentials.Add(numeroMesi.ToString());

                            // aggiunge due credenziali vuote in modo da pareggiare l'indice della lista con i gestionali MySQL
                            credentials.Add(String.Empty);
                            credentials.Add(String.Empty);

                            // aggiunge alle credenziali il flag per la modalità verbosa
                            credentials.Add("false");
                            break;
                        default:
                            gestionaleCorretto = false;
                            break;
                    }

                    // procede con l'importazione soltanto se il gestionale è supportato (MYSQL o FTP)
                    if (gestionaleCorretto)
                    {
                        daily.mesiManuali = -numeroMesi;
                        daily.Import(credentials);

                        returnValue = true;
                        string MailSubject = "Reimport "+ "mesi richiesti "+daily.mesiManuali+" azienda = "+codiceAzienda;
                        SendMail sender = new SendMail();
                        string body = String.Format("Ricaricamento R2 {0} mesi su operatore {1} codice: {2} effettuato.", numeroMesi, nomeAzienda, codiceAzienda);
                        //sender.Send(WebConfigurationManager.AppSettings["MailToImport"].ToString(), "Ricaricamento operatore " + nomeAzienda + " codice " + codiceAzienda, body, String.Empty);
                        //sender.Send("admin@osservamecati.it", "info@mafcoit.it", MailSubject, null, null);
                    }
                    else
                    {
                        returnValue = false;
                    }
                }
            }
            /*Rimuovo anagrafiche errate/vuote */
            DBController.DeleteEmptyAnagrafiche();
            DBController.DeleteVenditeSaldiBeyondNow();

            return returnValue;
        }
    }
}