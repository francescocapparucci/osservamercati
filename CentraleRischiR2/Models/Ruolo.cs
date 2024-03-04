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
    public class Ruolo
    {                
        public int IdRole
        { get; set; }
        public string Descrizione
        { get; set; }
    }
}
