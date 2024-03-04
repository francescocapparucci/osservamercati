using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CentraleRischiR2Library.Report
{
    public class Report
    {
        public int IdCentro{get;set;}
        public DateTime DataInizio { get; set; }
        public DateTime DataFine { get; set; }
        public string PartitaIva { get; set; }
        public string CodiceAzienda { get; set; }
        public virtual Stream GetReport(int numeroAziende,int salto) { return null; }
        public virtual Stream GetReport(string codiceAzienda,int numeroAziende, int salto) { return null; }
    }
}


    
