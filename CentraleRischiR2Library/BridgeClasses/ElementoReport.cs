using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CentraleRischiR2Library.BridgeClasses
{
    public class ElementoReport
    {
        public string PartitaIva { get; set; }
        public string RagioneSociale { get; set; }
        public decimal RichEffettuate { get; set; }
        public string meseRiferimento { get; set; }
        public string annoRiferimento { get; set; }
        public string DataRichiesta { get; set; }
        public string DaFatturare { get; set; }
        public string Residui { get; set; }
    }
}
