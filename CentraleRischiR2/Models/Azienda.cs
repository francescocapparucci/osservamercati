using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using CentraleRischiR2Library;
using CentraleRischiR2Library.BridgeClasses;

namespace CentraleRischiR2.Models
{
    public class Azienda
    {
        public string RagioneSociale
        { get; set; }
        public string CodiceAzienda
        { get; set; }
        public int IdAzienda
        { get; set; }
    }
}