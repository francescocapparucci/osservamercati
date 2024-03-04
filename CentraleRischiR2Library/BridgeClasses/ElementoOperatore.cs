using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CentraleRischiR2Library.BridgeClasses
{
    public class ElementoOperatore
    {
        public int Id { get; set; }
        public int IdCentro { get; set; }        
        public string PartitaIva { get; set; }
        public string RagioneSociale { get; set; }
        public string CodiceAzienda  { get; set; }
        public string CodiceFinservice { get; set; }
        public string CodicePaylineCerved { get; set; }
        public int SogliaReport { get; set; }
        public string Ambiente { get; set; }        
    }
}