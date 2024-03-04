using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CentraleRischiR2Library
{
    public class PortfoglioCs
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static string GetIdPortfoglio(string token, string name)
        {
            string id = "";
            try
            {
                Log.Info("begin GetIdPortfoglio " + name);
                string URI = "https://connect.creditsafe.com/v1/monitoring/portfolios";
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
                    dynamic response = JsonConvert.DeserializeObject(result.ToString(), settings);
                    //Console.WriteLine(response);
                    foreach (dynamic appo in response.data.portfolios)
                    {
                        if (appo.name == name)
                        {
                            id = appo.portfolioId;
                        }
                    }
                    Log.Debug("id del portafoglio " + name + " = " + id);
                }
                Log.Info("end GlobalReportId -- ID= " + id);
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
                MailHandler.SendMail("support@osservamercati.it", "GetIdPortfoglio" + ex.ToString());
            }
            Log.Info("end idPortfoglio= " + id);
            return id;
        }

        public static Boolean InsertCOMonitoraggio(string token, string idPortafoglio, string idCompany, string piva)
        {
            Boolean retVal = true;
            Log.Info("begin InsertMonitoraggio **");
            Log.Debug("idportfoglio= " + idPortafoglio + " idcompany= " + idCompany + " partitaIva= " + piva);

            try
            {
                var httpWebRequest =
                (HttpWebRequest)WebRequest.Create("https://connect.creditsafe.com/v1/monitoring/portfolios/" + idPortafoglio + "/companies");

                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Headers.Add("Authorization", token);
                httpWebRequest.Method = "POST";

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    string json = "{\"id\":\"" + idCompany + "\",\"personalReference\":\"Some text\"," +
                        "\"includeBranches\": true,\"freeText\": \"" + piva + "\",\"personalLimit\": \"40\"}";
                    streamWriter.Write(json);
                    streamWriter.Flush();
                    streamWriter.Close();
                }
                WebResponse httpResponse = httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    dynamic response = JsonConvert.DeserializeObject(result.ToString());
                    Log.Info("inserimento monitoraggio azienda :" + piva + " portfolio " + idPortafoglio);
                }
            }
            catch (Exception ex)
            {
                Log.Error("error : " + ex.ToString());
                MailHandler.SendMail("support@osservamercati.it", "InsertCOMonitoraggio " + ex.ToString());
                Console.WriteLine(ex.ToString());
                return false;
            }
            return retVal;
        }
    }
}
