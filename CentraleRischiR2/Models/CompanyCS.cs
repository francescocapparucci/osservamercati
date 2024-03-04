using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CentraleRischiR2.Models
{
    public class CompanyCS
    {
        public string id { get; set; }
        public string country { get; set; }
        public string regNo { get; set; }
        public string vatNo { get; set; }
        public string safeNo { get; set; }
        public string name { get; set; }
        public string rapporto { get; set; }
        public Int16 valutazione { get; set; }
        public Addressview address { get; set; }
        public string status { get; set; }
        public string officeType { get; set; }
        public string type { get; set; }
    }
}