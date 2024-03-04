using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CentraleRischiR2Library.BridgeClasses
{
    public class ElementoValutazione
    {
        public string id { get; set; }       
        public string PartitaIva { get; set; }
        public DateTime DataRiferimento { get; set; }
        public DateTime DataFineValidita { get; set; }
        public string ValutazioneGlobale { get; set; }
        public string ValutazioneGlobaleSemaforo { get; set; }
        public string ValutazioneCerved { get; set; }
        public string ValutazioneCervedSemaforo { get; set; }
        public string ValutazioneOsservaSemaforo { get; set; }
        public string EventiNegativi { get; set; }
        public decimal Fido { get; set; }        
    }
}
