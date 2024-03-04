using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CentraleRischiR2Library;
using CentraleRischiR2Library.BridgeClasses;


namespace CentraleRischiR2.Models
{
    public class Search
    {

        [Display(Name = "Ragione Sociale:")]
        public string RagioneSociale
        { get; set; }
        [Display(Name = "Partita Iva:")]
        public string PartitaIva
        { get; set; }

        [Display(Name = "Provincia:")]
        public string Provincia
        { get; set; }

        public IEnumerable<SelectListItem> ProvinceItaliane
        {
            get
            {
                var dictionaryProvince = DBHandler.ElencoProvince();
                dictionaryProvince.Add("Key", "Value");
                return new SelectList(dictionaryProvince, "Key", "Value");
            }
        }
    }
}
