using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Configuration;
using Osserva.CentraleRischi.Library.Report;
using CentraleRischiR2Library;
using System.Net;

namespace ReportTester
{
    class Program
    {
        [Obsolete]
        static void Main(string[] args)
        {
            //  bbbbb();
            //  bonificaReportPdf();
            monitoInserimentoMassivo();

        }

        [Obsolete]
        private static void monitoInserimentoMassivo()
        {
            try { 
            List<string> ListCodice = new List<string>();
            List<string> ListPartiteIva = new List<string>();
            List<string> ListPartitaIvaAnag = new List<string>();
            
                int idCentro = 7;//inserire il centro desisderato
                string idportfolio = "394269";//inserire il portfoglio abbinato
            
                using (DemoR2Entities context = new DemoR2Entities())
            {
                ListCodice = context.m01_aziende.Where(vs => vs.m01_idcentro == idCentro).Select(vss=>vss.m01_codice).ToList();
                ListPartiteIva = context.m04_vendite.Where(ve => ListCodice.Contains(ve.m04_codiceazienda)).Select(vep=>vep.m04_partitaiva).ToList();
                ListPartitaIvaAnag = context.m02_anagrafica.Where(an =>an.m02_note != ""&& ListPartiteIva.Contains(an.m02_partitaiva)).Select(anp=>anp.m02_partitaiva).ToList();
            }
            string token = DBHandler.LoginCS(SecurityProtocolType.Tls12);
                Console.WriteLine(token);
                int i = 1;
            foreach (var element in ListPartitaIvaAnag) 
            { 
                string idCompany = DBHandler.GetIdGlobalReport(element, token);
                PortfoglioCs.InsertCOMonitoraggio(token, idportfolio, idCompany, element);
                    Console.WriteLine(i);
                    i++;
            }
            }catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public static void bbbbb()
        {
            string reportFile =
             $"{ConfigurationManager.AppSettings["ReportPath"]}report_{ConfigurationManager.AppSettings["IdCentro"]}_{ConfigurationManager.AppSettings["Rating"]}.xls";
            Report reportObj = new DeloitteReport();
            reportObj.DataInizio = DateTime.Now;
            reportObj.Rating = ConfigurationManager.AppSettings["Rating"];
            reportObj.IdCentro =
                Convert.ToInt16(
                ConfigurationManager.AppSettings["IdCentro"]);
            //reportObj.FasciaEsposizione = 10000;
            MemoryStream report = (MemoryStream)reportObj.GetReport("", 0, 0);
            if (report != null)
            {
                FileStream fileStream = File.Create(reportFile, (int)report.Length);
                //Initialize the bytes array with the stream length and then fill it with data
                byte[] bytesInStream = new byte[report.Length];
                report.Read(bytesInStream, 0, bytesInStream.Length);
                //Use write method to write to the file specified above
                fileStream.Write(bytesInStream, 0, bytesInStream.Length);
                fileStream.Close();
            }
        }

        public static void ControlloReportAnagrafica()
        {
            string newFile = "";
            string codiceCerved = "";
            DirectoryInfo di = new DirectoryInfo("C:\\R3\\test\\");
            IDictionary<string, DateTime> openWith = new Dictionary<string, DateTime>();
            List<string> nontrovatisuAnag = new List<string>();
            List<string> nontrovatisuPdf = new List<string>();

            using (DemoR2Entities context = new DemoR2Entities())
            {
                /*List<string> listaAnagrafiche = context.m02_anagrafica.Where(anag => anag.m02_note != "").Select(sel=>sel.m02_note).ToList();
            
                foreach (var element in di.GetFiles())
                {

                    if (!listaAnagrafiche.Contains(element.Name))
                    {
                        nontrovatisuAnag.Add(element.Name);
                        Console.WriteLine(element.Name);
                    }
                }
                Console.WriteLine(nontrovatisuAnag.Count.ToString());*/
                List<m02_anagrafica> anag = context.m02_anagrafica.Where(an => an.m02_note != "").ToList();
                Boolean appo = false;
                foreach (var el in anag)
                {
                    foreach (var element in di.GetFiles())
                    {
                        if (el.m02_note.Equals(element.Name))
                        {
                            appo = true;
                        }
                    }
                    Console.WriteLine("appo " + appo + " piva " + el.m02_note);
                }
                Console.WriteLine("EEEEEEEEEEE" + anag.Count);
            }
        }

        public static void bonificaReportPdf()
        {
            string newFile = "";
            string codiceCerved = "";
            DirectoryInfo di = new DirectoryInfo("C:\\R3\\test\\");
            IDictionary<string, DateTime> openWith = new Dictionary<string, DateTime>();
            foreach (var el in di.GetFiles())
            {
                foreach (var element in di.GetFiles())
                {

                    int subend = element.Name.IndexOf("_");
                    newFile = element.Name.Substring(0, subend);
                    if (newFile.Equals(el.Name.Substring(0, subend)))
                    {
                        Console.WriteLine(el.Name.ToString());

                        if (el.LastWriteTimeUtc < element.LastWriteTimeUtc)
                        {
                            el.Delete();
                            break;

                        }

                    }
                }
            }
        }
    }
}
