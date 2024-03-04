using DocumentFormat.OpenXml.Drawing.Charts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CentraleRischiR2Library.BridgeClasses
{

    public class ReportAiende

    {
        public string partitaIva { get; set; }
        public string RagioneSociale { get; set; }
        public string Fatturato { get; set; }
        public DateTime? DataRichiesta { get; set; }
        public string TipoRichiesta { get; set; }
               
    }
}
