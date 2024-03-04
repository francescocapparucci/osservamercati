using CentraleRischiR2.Classes;
using CentraleRischiR2.Models;
using CentraleRischiR2Library;
using CentraleRischiR2Library.BridgeClasses;
using Microsoft.Office.Interop.Excel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Configuration;
using System.Web.Mvc;
using ClosedXML.Excel;
using System.Web;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using log4net;
using System.Net.Http;
using System.Diagnostics;

namespace CentraleRischiR2.Controllers
{
    public class UploadController : BaseController
    {
        [Authorize]
        public ActionResult Upload()
        {
            return View();
        }
        [Authorize]
        public JsonResult UploadResult()
        {
           List<ReportCs> returnValue = null;
            if (loggeduser == null)
            {
                RedirectToAction("Index", "Home");
            }
            string sidx = String.IsNullOrEmpty(Request.Form["sidx"]) ? "DataAggiornamento" : Request.Form["sidx"];
            string sord = Request.Form["sord"];
            int rows = Convert.ToInt32(Request.Form["rows"]);
            int page = Convert.ToInt32(Request.Form["page"]);
            string searchField = !String.IsNullOrEmpty(Request.Form["searchField"]) ? Request.Form["searchField"] : String.Empty;
            string searchString = !String.IsNullOrEmpty(Request.Form["searchString"]) ? Request.Form["searchString"] : String.Empty;
            searchString = searchString.ToUpper();
            int idUser = loggeduser.IdUser;
            using (DemoR2Entities context = new DemoR2Entities())
            {
                IQueryable<vs_aziende_osservamercati> reportElem = from vs_aziende_osservamercati e in context.vs_aziende_osservamercati select e;

                var bufferValue =
                        reportElem
                        .Select((appoggio) =>
                            new ReportCs
                            {
                                PartitaIva = appoggio.m01_partitaiva,
                                RagioneSociale = appoggio.m01_ragionesociale
                            });
                returnValue = bufferValue.ToList();
                
            }
            if (searchField != String.Empty)
            {
                switch (searchField)
                {
                    case "id":
                        returnValue = returnValue.Where(p => p.PartitaIva.Contains(searchString)).ToList();
                        break;
                    case "RagioneSociale":
                        returnValue = returnValue.Where(p => p.RagioneSociale.Contains(searchString)).ToList();
                        break;
                }
            }
            returnValue.OrderByDescending(or => or.RagioneSociale).ToList();
            ViewBag.Preferiti = returnValue;
            int recordTotali = returnValue.Count();
            int pagineTotali = recordTotali / rows;
            //returnValue.Skip((page > 0 ? page - 1 : 0) * rows).Take(rows).ToList();

            /*Formattazione JSON per JqGrid*/
            var result = new { page = 1, total = 1, records = recordTotali, rows = returnValue };

            return Json(result, JsonRequestBehavior.AllowGet);

        }
    }

}