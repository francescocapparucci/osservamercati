using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CentraleRischiR2.Models
{
    public class Addressview
    {
        public string simpleValue { get; set; }
        public string street { get; set; }
        public string city { get; set; }
        public string province { get; set; }
        public string postCode { get; set; }
        public string houseNo { get; set; }
    }
}