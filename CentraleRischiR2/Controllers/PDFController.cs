using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel.Channels;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Threading;
using System.Web.Mvc;
using System.IO;
using System.Web.Configuration;
using CentraleRischiR2Library;
using CentraleRischiR2.Classes;
using CentraleRischiR2Library.BridgeClasses;
using log4net;

namespace CentraleRischiR2.Controllers
{
    public class PDFController : BaseController
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public ActionResult Check(string id)
        {
            ActionResult returnView = RedirectToAction("Index", "Home");
            try
            {
                id = id.Trim();

               
                if (id != "")
                {
                    string newFile = "";
                    string codiceCerved = "";

                    DirectoryInfo di = new DirectoryInfo(WebConfigurationManager.AppSettings["ReportPath"]);
                    IDictionary<string, DateTime> openWith = new Dictionary<string, DateTime>();
                    foreach (var element in di.GetFiles())
                    {

                        int subend = element.Name.IndexOf("_");
                        newFile = element.Name.Substring(0, subend);
                        if (newFile.Equals(id.Substring(0, subend)))
                        {
                            if (openWith.Count == 0)
                            {
                                openWith.Add(newFile, element.LastWriteTimeUtc);
                                codiceCerved = element.Name;
                            }
                            else
                            {
                                if (openWith[element.Name.Substring(0, subend)] < element.LastWriteTimeUtc)
                                {
                                    openWith.Clear();
                                    openWith.Add(newFile, element.LastWriteTimeUtc);
                                    codiceCerved = element.Name;
                                }
                            }
                        }
                    }

                    int idUtente = loggeduser.IdUser;
                    if (System.IO.File.Exists(String.Format("{0}{1}", WebConfigurationManager.AppSettings["ReportPath"], codiceCerved)))
                    {
                        returnView =
                           new FileStreamResult(new FileStream(String.Format("{0}{1}", WebConfigurationManager.AppSettings["ReportPath"], codiceCerved), FileMode.Open), "application/pdf");
                    }
                }
            }
            catch(Exception e)
            {
                Log.Error("errore : " + e.ToString());
            }
            return returnView;
        }

        public ActionResult CheckUpdate(string id)
        {
            id = id.Trim();
            ActionResult returnView = RedirectToAction("Index", "Home");
            if (id != "")
           {
               if (loggeduser != null)
                {
                    int idUtente = loggeduser.IdUser;
                    if (System.IO.File.Exists(String.Format("{0}{1}", WebConfigurationManager.AppSettings["ReportPathUpdate"], id)))
                    {
                        returnView =
                            new FileStreamResult(new FileStream(String.Format("{0}{1}.pdf", WebConfigurationManager.AppSettings["ReportPathUpdate"], id), FileMode.Open), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                    }
                }
            }
            return returnView;
        }

    }
}
