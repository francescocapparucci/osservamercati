using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CentraleRischiR2Library.BridgeClasses
{
    public class ElementoPreferiti
    {
        public string id { get; set; }
        public bool Letto { get; set; }
        public string PartitaIva { get; set; }
        public string RagioneSociale { get; set; }
        public decimal Esposizione { get; set; }
        public string EsposizioneString { get; set; }
        public double Vendite { get; set; }
        public string Rating { get; set; }
        public string RatingDescrizione { get; set; }
        public string Rapporto { get; set; }
        public string Rapportonew { get; set; }
        public string Aggiornamento { get; set; }
        public string CodiceRapporto { get; set; }
        public Decimal? Fido { get; set; }
        /*public byte DSO { get; set; }*/
        public bool AnomaliaVendite { get; set; }
        public double? GG { get; set; }
        public string Camerale { get; set; }
        public DateTime? DataVariazione { get; set; }
        public DateTime? DataAggiornamento { get; set; }
        public string EventiNegativi { get; set; }
        public string Osservatorio { get; set; }
        public string DescrizioneVariazione { get; set; }
        public string UtenteNews { get; set; }
    }
}
