using DocumentFormat.OpenXml.Drawing.Charts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CentraleRischiR2Library.BridgeClasses
{

    public class ReportCs
    {
        public int idUtente { get; set; }
        public string RagioneSociale { get; set; }
        public string PartitaIva { get; set; }
        public string Referente { get; set; }
        public int idCentro { get; set; }
        public string emailUtente { get; set; }
        public string TipoRichiesta { get; set; }
        public DateTime? DataRichiesta { get; set; }
       
    }
}
