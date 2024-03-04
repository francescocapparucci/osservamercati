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
    public class SearchUserModel
    {
        public List<Azienda> ElencoAziende { get; set; }
        public List<Ruolo> ElencoRuoli { get {
                return new List<Ruolo>() {
                    new Ruolo {IdRole=0,Descrizione="SuperAdmin"},
                    new Ruolo {IdRole=1,Descrizione="Admin"},
                    new Ruolo {IdRole=2,Descrizione="User"}
                };
            }
        }
        public List<User> ElencoUtenti { get; set; }        
        public List<ElementoMercato> ElencoCentri { get; set; }
        public Models.Search Search { get; set; } 
    }
}