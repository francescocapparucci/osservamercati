using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml;
using System.Configuration;
using System.Text;


namespace ParserXMLReportCerved
{
    class Program
    {
        static void Main(string[] args)
        {
            string reportPath =
                String.Format("{0}{1}", ConfigurationManager.AppSettings["ReportPath"], ConfigurationManager.AppSettings["ReportTest"]);
            XDocument document = XDocument.Load(reportPath);
            /*
             LETTURA FIDO
            string averageGrantableCredit = document.Root.Elements("AverageGrantableCredit")
                .FirstOrDefault()
                .Elements("CreditLimit")
                .FirstOrDefault()
                .Elements("Value").FirstOrDefault().Value;
                Console.WriteLine(averageGrantableCredit);
            */

            foreach (XElement element in document.Root.Elements())
            {
                Console.WriteLine("-----------------------------------------------------------------");
                Console.WriteLine(element);
                Console.WriteLine("-----------------------------------------------------------------");
                Console.ReadLine();
            }
            
            Console.ReadLine();

        }
    }
}
