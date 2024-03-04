using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using CentraleRischiR2Library;


namespace InvioMassiveWSCerved
{
    class Program
    {
        static void Main(string[] args)
        {
            /*Seleziona i "preferiti" di un'operatore e li invia*/            
            using (DemoR2Entities context = new DemoR2Entities())
            {

                List<cerved_check> listaInevase =
                    context.cerved_check.Where(a => a.codice_cerved == "P" && a.evaso == false).OrderBy(o => o.id_utente).ToList();

                

                foreach(cerved_check inevasa in listaInevase)
                {

                    vs_aziende_osservamercati aziendaUtente = context.m10_utenti.Where(ut => ut.m10_iduser == inevasa.id_utente)
                        .Join(context.vs_aziende_osservamercati,
                        a => a.m10_idazienda, 
                        b => b.m01_idazienda,
                        (a,b) => b).FirstOrDefault(); 
                    
                    CervedWSHandler cHandler = new CervedWSHandler(
                            aziendaUtente,
                            ConfigurationManager.AppSettings["ThreeStepWSUrl"],
                            ConfigurationManager.AppSettings["RetrieveReportWSUrl"],
                            "",
                            ConfigurationManager.AppSettings["CDSStepWSUrl"],
                            ConfigurationManager.AppSettings["NETUSERCDS"],
                            ConfigurationManager.AppSettings["NETPASSWORDCDS"],
                            ConfigurationManager.AppSettings["ReportPath"],
                            ConfigurationManager.AppSettings["LogWSPath"]);
                                        
                            cHandler.WriteLog("-- Tentativo acquisto massive per operatore " + aziendaUtente.m01_codice + " su partitaIva " + inevasa.partita_iva);
                            string codiceRichiesta = cHandler.BuyGlobalProfile(inevasa.partita_iva, Convert.ToInt32(ConfigurationManager.AppSettings["GP_PRODUCTCODE"]), inevasa.id_utente,false);
                            cHandler.WriteLog("-- Recuperato Codice Cerved " + codiceRichiesta + " per operatore " + aziendaUtente.m01_codice + " su partitaIva " + inevasa.partita_iva);

                }

            }
        }
    }
}
