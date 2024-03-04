using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CentraleRischiR2Library.BridgeClasses
{
    public class ElementoFatturaRecuperoCrediti
    {
        public int id { get; set; }
        public string PartitaIva { get; set; }
        public string CodiceOperatore { get; set; }
        public string RagioneSocialeOperatore { get; set; }
        public string RagioneSociale { get; set; }
        public DateTime DataFattura { get; set; }
        public string StrDataFattura { get; set; }
        public string NDoc { get; set; }
        public DateTime DataScadenza { get; set; }
        public Double Importo { get; set; }
        public Decimal ImportoDecimal { get; set; }
        public int GGScaduto { get; set; }
        public int GGScadutoAdOggi { get; set; }
        public string Rating { get; set; }
        public string RatingDescrizione { get; set; }        
        /*per riepilogo*/
        public int IdRichiesta { get; set; }
        public DateTime DataRichiesta { get; set; }
        public string StatoRichiesta { get; set; }
        public string EventiNegativi { get; set; }

    }
}
