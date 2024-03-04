using CentraleRischiR2Library;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace CentraleRischiR2.Service
{
    public class ApiService
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        public Boolean CallSendMail(string mailTo, string subject, string body)
        {
            Boolean retVal = false;
            Log.Debug("Start invio mail Java");
            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create("http://localhost/sendMail");
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";

                //WriteLog("username : " + username+"password : " +password);
                {
                }
                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    string json = "{\"subject\":\"" + subject + "\",\"mailto\":\"" + mailTo + "\",\"body\":\"" + body + "\"}";
                    streamWriter.Write(json);
                    streamWriter.Flush();
                    streamWriter.Close();
                }
                WebResponse httpResponse = httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    dynamic response = JsonConvert.DeserializeObject(result.ToString());
                    retVal = true;
                }
            }
            catch (WebException ex)
            {
                return retVal;
               
            }
            return retVal;
        }
    }
}