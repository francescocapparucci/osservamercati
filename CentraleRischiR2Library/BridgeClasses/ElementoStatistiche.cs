using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CentraleRischiR2Library.BridgeClasses
{
    public class ElementoStatistiche
    {
        public string id { get; set; }
        public string PartitaIva { get; set; }
        public string RagioneSociale { get; set; }
        public decimal Esposizione { get; set; }
        public decimal Vendite { get; set; }
        public decimal Incassi { get; set; }
        public long Giorni { get; set; }
        public string Rating { get; set; }        
        
    }
}
