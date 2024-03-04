using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Configuration;
using CentraleRischiR2Library;
using System.IO;
using System.Xml.Linq;

namespace CheckCervedUpdate
{
    class Program
    {
        static void WriteLog(string row)
        {
            Console.WriteLine(row + "\n");
            StreamWriter sw = new StreamWriter(String.Format("{0}log{1:yyyyMMdd}.log", ConfigurationManager.AppSettings["PathLog"], DateTime.Now), true);
            sw.WriteLine(DateTime.Now.ToString("g") + ":" + row);
            sw.Flush();
            sw.Close();
        }
        

        static void Main(string[] args)
        {

            
            
            WriteLog("DOWNLOAD AGGIORNAMENTI -----" + DateTime.Now);
            string uri = ConfigurationManager.AppSettings["FtpServerOut"].ToString();
            List<String> fileIn = new List<String>();
            FtpWebRequest request = (FtpWebRequest)FtpWebRequest.Create(uri);
            request.Credentials = new NetworkCredential(ConfigurationManager.AppSettings["FtpUser"].ToString(), ConfigurationManager.AppSettings["FtpPassword"].ToString());
            request.Method = WebRequestMethods.Ftp.ListDirectory;
            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            Stream responseStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(responseStream);
            string line = reader.ReadLine();            
            while (line != null)
            {
                WriteLog("--- file " + line);
                fileIn.Add(line);
                line = reader.ReadLine();

            }
            reader.Close();
            responseStream.Close();
            response.Close();
            if (line == null)
                WriteLog("Nessun file da scaricare");
            foreach (String fileName in fileIn)
            {
                if (fileName != String.Empty)
                {
                    
                    WebClient oFtp = new WebClient();
                    WriteLog("-- Tentativo download " + fileName);
                    string downloadedFileName = ConfigurationManager.AppSettings["PathIn"] + fileName.Replace("OUT/", "");
                    oFtp.Credentials = new NetworkCredential(ConfigurationManager.AppSettings["FtpUser"].ToString(), ConfigurationManager.AppSettings["FtpPassword"].ToString());
                    oFtp.DownloadFile(ConfigurationManager.AppSettings["FtpServer"] + "/" + fileName, downloadedFileName);

                    
                    FtpWebRequest deleteRequest = (FtpWebRequest)WebRequest.Create(ConfigurationManager.AppSettings["FtpServer"] + "/" + fileName);
                    deleteRequest.Credentials = new NetworkCredential(ConfigurationManager.AppSettings["FtpUser"].ToString(), ConfigurationManager.AppSettings["FtpPassword"].ToString());

                    deleteRequest.Method = WebRequestMethods.Ftp.DeleteFile;
                    FtpWebResponse deleteResponse = (FtpWebResponse)deleteRequest.GetResponse();
                    WriteLog("-- Rimozione da server remoto " + fileName);                    
                    
                }
            }            
            
            //int productCode = Convert.ToInt32(ConfigurationManager.AppSettings["ProductCode"]);
            /*ESAMINO CARTELLA DI RICEZIONE*/
            Console.WriteLine("ESAME CARTELLA RICEZIONE ---------------------------> " + ConfigurationManager.AppSettings["PathIn"]);
            DirectoryInfo directoryIn = new DirectoryInfo(ConfigurationManager.AppSettings["PathIn"]);
            /*file di testo per news*/
            foreach (FileInfo fileAgg in directoryIn.GetFiles("*.txt"))
            {
                    WriteLog("Esamino file txt " + fileAgg.FullName);
                    StreamReader re = File.OpenText(fileAgg.FullName);      
                    string input = null;

                    // mostra una barra di avanzamento
                    long fileSize = fileAgg.Length;
                    int contatore = 0;
                    /*PASSAGGIO PER RENDERE UNIVOCI I DATI CERVED*/
                    List<string> righeDistinct = new List<string>();
                    while ((input = re.ReadLine()) != null)
                    {
                        righeDistinct.Add(input);
                    }
                    //NON PIU DISTINCT!!! E' presente il codice operatore!!!
                    //righeDistinct = righeDistinct.Distinct().ToList();
                                        
                    foreach(string rigaDistinct in righeDistinct)
                    {
                        WriteLog("riga "+ contatore + " file " + fileAgg.FullName);
                        if (contatore == 0) { contatore++; continue; };
                        /*INDIVIDUO PARAMETRI ESTRAIBILI DAL FILE TXT*/
                        String[] pars = new String[50];
                        pars = rigaDistinct.Split(';');
                        DateTime dataAggiornamento = Convert.ToDateTime(pars[5]);

                        string codiceCervedOperatore = pars[0].Trim();
                        int intCodiceCervedOperatore = 0;
                        Int32.TryParse(codiceCervedOperatore, out intCodiceCervedOperatore);
                                                

                        string descrizioneNews = pars[8];
                        string ragioneSociale = pars[2];
                        string partitaIva = pars[4];
                        string codiceCerved = pars[4];
                        DateTime dataSegnalazione = DateTime.Parse(pars[9]);
                        WriteLog("Segnalazione per piva " + partitaIva + " " + dataSegnalazione + " " + descrizioneNews + " relativa a codice oepratore "+ codiceCervedOperatore);
                        /*EFFETTUO SEMPRE NUOVA RICHIESTA DOCUMENTO(PRENDO MONITORAGGIO PIU RECENTE CHE SOVRASCRIVE GLI ALTRI)*/
                        using (DemoR2Entities context = new DemoR2Entities())
                        {

                            if(intCodiceCervedOperatore != 0){
                                WriteLog("Presente il codice operatore: " + intCodiceCervedOperatore.ToString());
                                
                                m05_rating ratingPiuRecente = 
         context.m05_rating.Where(rating => rating.m05_partitaiva == partitaIva).OrderByDescending(ord => ord.m05_dtriferimento).FirstOrDefault();
                                m02_anagrafica anag = context.m02_anagrafica.Where(rating => rating.m02_partitaiva == partitaIva).FirstOrDefault();

                                WriteLog("dopo trovato rating");
                               cerved_check richiesta = 
                                    context.m01_aziende.Where(r => r.m01_codice_payline_cerved == codiceCervedOperatore)
                                    .Join(context.m10_utenti,
                                    a => a.m01_idazienda,
                                    b => b.m10_idazienda,
                                    (a,b) => b)
                                    .Join(context.cerved_check.Where(cc => cc.evaso== true && cc.partita_iva == partitaIva),
                                    a => a.m10_iduser,
                                    b => b.id_utente,
                                    (a,b) => b).FirstOrDefault();

                               WriteLog("dopo trovata richiesta per piva " + partitaIva);
                                if(richiesta == null)
                                    WriteLog("richiesta NULL");
                                if (ratingPiuRecente == null)
                                    WriteLog("rating NULL");
                                if (anag == null)
                                    WriteLog("anag NULL");

                                bool result = false;

                                if (partitaIva.Trim() != String.Empty && richiesta != null && anag != null && ratingPiuRecente != null)
                                {
                                    /*result =
                                    DBHandler.InserisciNewsReport(
                                                    richiesta.codice_cerved,
                                                    partitaIva, ragioneSociale,
                                                    ratingPiuRecente.m05_stato,
                                                    ratingPiuRecente.m05_stato_semaforo,
                                                    ratingPiuRecente.m05_stato_osserva,
                                                    ratingPiuRecente.m05_stato_semaforo_osserva,
                                                    ratingPiuRecente.m05_stato_cerved, ratingPiuRecente.m05_stato_semaforo_cerved, richiesta.id_utente,
                                                    anag.m02_eventi_negativi, anag.m02_fido.HasValue ? anag.m02_fido.Value : 0, descrizioneNews,
                                                    dataSegnalazione,
                                                    "STANDARD"
                                            );*/
                                }
                                WriteLog("inserita news");

                                if (ConfigurationManager.AppSettings["SendMail"] == "True" && result)
                                {
                                    WriteLog("INVIO MAIL conseguente");
                                    List<string> additionalData = new List<string>();

                                    additionalData.Add(ragioneSociale);
                                    additionalData.Add(partitaIva); 
                                    additionalData.Add(richiesta.codice_cerved);
                                    additionalData.Add(descrizioneNews);

                                    MailHandler.WarnMail(richiesta.id_utente, "UPDATE_MONITORING", additionalData);
                                }
                                else {
                                    WriteLog("EVITO MAIL");
                                }                                

                            }
                            else
                            {
                                cerved_check monitoraggioAttivoPiuRecente = context.cerved_check.Where(c => c.partita_iva == partitaIva && c.evaso == true)
                                                                                    .OrderByDescending(c => c.dt_aggiornamento).FirstOrDefault();
                                m05_rating ratingPiuRecente = context.m05_rating.Where(rating => rating.m05_partitaiva == partitaIva).OrderByDescending(ord => ord.m05_dtriferimento).FirstOrDefault();


                                m02_anagrafica anag = context.m02_anagrafica.Where(rating => rating.m02_partitaiva == partitaIva).FirstOrDefault();

                                if (monitoraggioAttivoPiuRecente != null && ratingPiuRecente != null && anag != null)
                                {


                                    List<cerved_check> monitoraggiSuStessaPiva =
                                        DBHandler.GetListaRichiesteCervedEsistentiSuPivaAttivi(monitoraggioAttivoPiuRecente.partita_iva);

                                    foreach (cerved_check monitoraggioSimile in monitoraggiSuStessaPiva)
                                    {


                                        WriteLog("dopo trovata richiesta per piva " + partitaIva);
                                        if (monitoraggioSimile == null)
                                            WriteLog("monitoraggio simile NULL");
                                        if (ratingPiuRecente == null)
                                            WriteLog("rating NULL");
                                        if (anag == null)
                                            WriteLog("anag NULL");

                                        bool result = false;


                                        if (partitaIva.Trim() != String.Empty && monitoraggioSimile != null && ratingPiuRecente != null && anag != null)
                                        {
                                            /*result =
                                                DBHandler.InserisciNewsReport(
                                                        monitoraggioSimile.codice_cerved,
                                                        partitaIva, ragioneSociale,
                                                        ratingPiuRecente.m05_stato,
                                                        ratingPiuRecente.m05_stato_semaforo,
                                                        ratingPiuRecente.m05_stato_osserva,
                                                        ratingPiuRecente.m05_stato_semaforo_osserva,
                                                        ratingPiuRecente.m05_stato_cerved, ratingPiuRecente.m05_stato_semaforo_cerved, monitoraggioSimile.id_utente,
                                                        anag.m02_eventi_negativi, anag.m02_fido.HasValue ? anag.m02_fido.Value : 0, descrizioneNews,
                                                        dataSegnalazione,
                                                        "STANDARD"
                                                );*/
                                        }
                                        if (ConfigurationManager.AppSettings["SendMail"] == "True" && result)
                                        {
                                            WriteLog("INVIO MAIL conseguente");
                                            List<string> additionalData = new List<string>();

                                            additionalData.Add(ragioneSociale);
                                            additionalData.Add(partitaIva);
                                            additionalData.Add(monitoraggioSimile.codice_cerved);
                                            additionalData.Add(descrizioneNews);

                                            MailHandler.WarnMail(monitoraggioSimile.id_utente, "UPDATE_MONITORING", additionalData);
                                        }
                                        else
                                        {
                                            WriteLog("EVITO MAIL");
                                        }

                                    }
                                }

                            }                      
      
                                      
                            

                            
                        }
                        contatore++;
                     
                    }
                    re.Close();

                    if (File.Exists(ConfigurationManager.AppSettings["PathBackup"] + fileAgg.Name))
                    {
                        File.Delete(ConfigurationManager.AppSettings["PathBackup"] + fileAgg.Name);
                    }
                    fileAgg.MoveTo(ConfigurationManager.AppSettings["PathBackup"] + fileAgg.Name);
            }
            /*file PDF */
            WriteLog("Processing PDF");
            using (DemoR2Entities context = new DemoR2Entities())
            {
                foreach (FileInfo filePDF in directoryIn.GetFiles("*.pdf"))
                {
                    WriteLog("FILE " + filePDF.Name);
                    string[] fileNameParts = filePDF.Name.Split('_');
                    string partitaIva = fileNameParts[0];
                    //per test if (partitaIva != "01966540401") continue;
                    string sCodiceOperatore = fileNameParts[1];
                    int codiceOperatore = 0;
                    Int32.TryParse(sCodiceOperatore, out codiceOperatore);
                    if (codiceOperatore != 0)
                    {
                        WriteLog("Codice operatore :" + codiceOperatore);
                        if (File.Exists(ConfigurationManager.AppSettings["ReportPath"] + partitaIva+"_"+codiceOperatore+".pdf")) {
                            WriteLog("File già presente: elimino come " + ConfigurationManager.AppSettings["ReportPath"] + partitaIva+"_"+codiceOperatore+".pdf");
                            File.Delete(ConfigurationManager.AppSettings["ReportPath"] + partitaIva + "_" + codiceOperatore + ".pdf");
                        }
                        WriteLog("move file a destinazione report " + ConfigurationManager.AppSettings["ReportPath"] + partitaIva + "_" + codiceOperatore + ".pdf");
                        filePDF.MoveTo(ConfigurationManager.AppSettings["ReportPath"] + partitaIva+"_"+codiceOperatore+".pdf");
                    }                         
                    else {
                        //WriteLog("Codice operatore assente: ciclo su tutti i monitoraggi attivi");
                         cerved_check monitoraggioAttivoPiuRecente = context.cerved_check.Where(c => c.partita_iva == partitaIva && c.evaso == true)
                                                                                    .OrderByDescending(c => c.dt_aggiornamento).FirstOrDefault();
                         m05_rating ratingPiuRecente = context.m05_rating.Where(rating => rating.m05_partitaiva == partitaIva).OrderByDescending(ord => ord.m05_dtriferimento).FirstOrDefault();

                         m02_anagrafica anag = context.m02_anagrafica.Where(rating => rating.m02_partitaiva == partitaIva).FirstOrDefault();

                         if (monitoraggioAttivoPiuRecente != null && ratingPiuRecente != null && anag != null)
                         {


                             List<cerved_check> monitoraggiSuStessaPiva =
                                 DBHandler.GetListaRichiesteCervedEsistentiSuPivaAttivi(monitoraggioAttivoPiuRecente.partita_iva);
                             WriteLog("---trovati " + monitoraggiSuStessaPiva.Count);                             
                             foreach (cerved_check monitoraggioSimile in monitoraggiSuStessaPiva)
                             {
                                 m01_aziende operatoreMonitoraggio =
                                     context.m10_utenti.Where(ut => ut.m10_iduser == monitoraggioSimile.id_utente)
                                     .Join(context.m01_aziende,
                                     a => a.m10_idazienda,
                                     b => b.m01_idazienda,
                                     (a, b) => b).FirstOrDefault();
                                 if (operatoreMonitoraggio != null)
                                 {
                                     WriteLog("file " + ConfigurationManager.AppSettings["ReportPath"] + partitaIva + "_" + operatoreMonitoraggio.m01_codice_payline_cerved + ".pdf");
                                     if (!File.Exists(ConfigurationManager.AppSettings["ReportPath"] + partitaIva + "_" + operatoreMonitoraggio.m01_codice_payline_cerved + ".pdf"))
                                     {
                                         WriteLog("move file a destinazione " + ConfigurationManager.AppSettings["ReportPath"] + partitaIva + "_" + operatoreMonitoraggio.m01_codice_payline_cerved + ".pdf");
                                         filePDF.MoveTo(ConfigurationManager.AppSettings["ReportPath"] + partitaIva + "_" + operatoreMonitoraggio.m01_codice_payline_cerved + ".pdf");
                                     }
                                 }
                                 
                             }                              
                     }
                }           
                
                /*spezzo il nome file in piva. codice utente e data*/
                /*prendop solo piva e codice utente*/
                /*se codice utente manca creo per tutti quelli che hanno monitoraggio*/
                
                if (File.Exists(ConfigurationManager.AppSettings["PathBackup"] + filePDF.Name))
                {
                    File.Delete(ConfigurationManager.AppSettings["PathBackup"] + filePDF.Name);
                }
                filePDF.CopyTo(ConfigurationManager.AppSettings["PathBackup"] + filePDF.Name);
                }   
                }
            /*file xml per */
            foreach (FileInfo fileAgg in directoryIn.GetFiles("*.xml"))
            {
                try{
                                
                    using (DemoR2Entities context = new DemoR2Entities())
                    {
                        XDocument document = XDocument.Load(fileAgg.FullName);

                        string partitaIva = fileAgg.Name.Substring(0, fileAgg.Name.IndexOf("_"));

                        m02_anagrafica anagraficaOss = context.m02_anagrafica.Where(a => a.m02_partitaiva == partitaIva).FirstOrDefault();
                        if (anagraficaOss != null)
                        {
                            var datiAnagraficiReport =
                                    document.Root
                                    .Elements("CompanyBriefHeading").FirstOrDefault();

                            var address =
                                document.Root
                                    .Elements("CompanyBriefHeading").Elements("Address").FirstOrDefault();

                            var ssRea =
                                document.Root   
                                    .Elements("CompanyBriefHeading").Elements("ReaCode").FirstOrDefault();

                            anagraficaOss.m02_ragionesociale =
                                datiAnagraficiReport.Elements("CompanyName").FirstOrDefault() != null ? datiAnagraficiReport.Elements("CompanyName").FirstOrDefault().Value :
                                anagraficaOss.m02_ragionesociale;

                            anagraficaOss.m02_stato_attivita =
                                datiAnagraficiReport.Elements("CompanySituation").FirstOrDefault() != null ? datiAnagraficiReport.Elements("CompanySituation").FirstOrDefault().Value :
                                anagraficaOss.m02_stato_attivita;


                            if(address != null)
                            {
                                anagraficaOss.m02_cap = address.Elements("PostCode").FirstOrDefault() != null ? address.Elements("PostCode").FirstOrDefault().Value :
                                    anagraficaOss.m02_cap;
                                anagraficaOss.m02_comune =
                                    address.Elements("Municipality").FirstOrDefault() != null ? address.Elements("Municipality").FirstOrDefault().Value :
                                    anagraficaOss.m02_comune;
                                anagraficaOss.m02_indirizzo =
                                    address.Elements("Street").FirstOrDefault() != null ? address.Elements("Street").FirstOrDefault().Value :
                                    anagraficaOss.m02_indirizzo;
                                anagraficaOss.m02_prefisso = address.Elements("Province").FirstOrDefault() != null ? address.Elements("Province").FirstOrDefault().Attribute("Code").Value :
                                    anagraficaOss.m02_prefisso;
                                
                            }

                            if (ssRea != null)
                            {
                                anagraficaOss.m02_nrea = ssRea.Elements("REANo").FirstOrDefault() != null ? ssRea.Elements("REANo").FirstOrDefault().Value :
                                    anagraficaOss.m02_nrea;
                                anagraficaOss.m02_cciaa = ssRea.Elements("CoCProvinceCode").FirstOrDefault() != null ? ssRea.Elements("CoCProvinceCode").FirstOrDefault().Value :
                                    anagraficaOss.m02_cciaa;
                            }

                                



                        }
                        var dettagliReport =
                        document.Root
                            .Elements("Synthesis").FirstOrDefault()
                            .Elements("MonetReportOutput").FirstOrDefault()
                            .Elements("MonetReportDettaglio");

                        foreach(var dettaglioReport in dettagliReport)
                        {
                            string descrizioneEvento = dettaglioReport.Elements("descrizioneEvento").FirstOrDefault().Value;
                            
                            switch (descrizioneEvento)
                            { 
                                case "Variazione PEC" :

                                    var rOut = document.Root
                                                    .Elements("Synthesis").FirstOrDefault()
                                                    .Elements("MonetReportOutput");
                                    if (rOut != null)
                                    {
                                        var dettagli = rOut.Elements("MonetReportDettaglio");
                                        foreach (var dettaglio in dettagli)
                                        {
                                            if (dettaglio.Element("descrizioneEvento") != null)
                                            {
                                                if (dettaglio.Element("descrizioneEvento").Value == "Variazione PEC")
                                                { 
                                                   var valorePec = 
                                                       dettaglio.Elements("InfoDatiAzienda").FirstOrDefault() != null ? 
                                                    dettaglio.Elements("InfoDatiAzienda").FirstOrDefault()
                                                    .Elements("VariazioneAzienda").FirstOrDefault()
                                                    .Element("nuovoValore").Value : "";
                                                   anagraficaOss.m02_pec = valorePec != "" ? valorePec : anagraficaOss.m02_pec;
                                                }
                                            }
                                        }

                                    
                                    }

                                break;                                
                            }
                            context.SaveChanges();
                            
                        }
                        fileAgg.MoveTo(ConfigurationManager.AppSettings["PathBackup"] + fileAgg.Name);
                    }
                    
                    
                }
                catch(Exception)
	            {
		 
	            }

             
            }
            
        }
    }
}
