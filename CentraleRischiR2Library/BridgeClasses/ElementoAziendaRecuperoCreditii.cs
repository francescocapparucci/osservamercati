using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CentraleRischiR2Library.BridgeClasses
{
    public class ElementoAziendaRecuperoCrediti
    {
        public string idRC { get; set; }
        public string PartitaIva { get; set; }
        public string RagioneSocialeRC { get; set; }
        public DateTime DataRichiesta { get; set; }
        public DateTime DataAggiornamento { get; set; }
        public string StatoRichiesta { get; set; }
        public Decimal SommaFatture { get; set; }
        public Decimal SommaIncasso { get; set; }
        public long IdDettaglio { get; set; }        
    }
}
