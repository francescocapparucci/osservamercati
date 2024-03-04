using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CentraleRischiR2Library.BridgeClasses
{
    public class ElementoAggiornamento
    {
        public string Codice { get; set; }
        public string PartitaIva { get; set; }
        public string RagioneSociale { get; set; }
        public string TipoGestionale { get; set; }
        public string Gestionale { get; set; }
        public DateTime UltimoSaldo { get; set; }
        public DateTime UltimaVendita { get; set; }
        public DateTime? UltimaChiusura { get; set; }        
    }
}
