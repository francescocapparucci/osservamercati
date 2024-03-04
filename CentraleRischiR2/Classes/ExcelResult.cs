using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CentraleRischiR2Library;
using CentraleRischiR2Library.BridgeClasses;
using ClosedXML.Excel;

namespace CentraleRischiR2.Classes
{
    public class ExcelResult : ActionResult
    {

        public string FileName { get; set; }
        public string BufferText { get; set; }

        public ExcelResult() { }

        


        
        public ExcelResult(List<ElementoFatturaRecuperoCrediti> elementiRC, string fileName)
        {
            FileName = fileName;
            BufferText = "<table>";
            switch (fileName)
            {
                case "RichiesteRecuperoCrediti.xls":
                    BufferText += "<tr><td colspan='5'><b>Riepilogo Richieste Recupero Crediti</b></td></tr>";
                    BufferText += "<table><tr><td><b>Data Richiesta</b></td><td><b>Numero Richiesta</b></td><td><b>Partita Iva</b></td><td><b>Ragione Sociale</b></td><td><b>Data Documento</b></td><td><b>Numero Documento</b></td><td><b>Importo</b></td><td><b>Stato</b></td></tr>";
                    foreach (ElementoFatturaRecuperoCrediti rigaRC in elementiRC)
                    {                        
                        BufferText +=
                            String.Format("<tr><td>{0}</td><td>{1}</td><td>-{2}-</td><td>{3}</td><td>{4:dd/MM/yyyy}</td><td>{5}</td><td>{6}</td><td>{7}</td></tr>",
                            rigaRC.DataRichiesta,rigaRC.IdRichiesta,rigaRC.PartitaIva,rigaRC.RagioneSociale,rigaRC.DataFattura,rigaRC.NDoc,rigaRC.ImportoDecimal,rigaRC.StatoRichiesta);
                            
                    }
                break;
            }
            BufferText += "</table>";
        }

        public ExcelResult(List<ElementoPreferiti> preferitiHome, string fileName)
        {
            FileName = fileName;
            BufferText = "<table>";
            switch (fileName)
            {
                case "NewsPreferiti.xls":
                    
                    BufferText += "<table><tr><td colspan='7'><b>Riepilogo News Variazioni su Monitoraggi</b></td></tr>";
                    BufferText += "<tr><td><b>Partita Iva</b></td><td><b>Ragione Sociale</b></td><td><b>Esposizione</b></td><td><b>Fido</b></td><td><b>Rating</b></td><td><b>Data Aggiornamento</b></td><td><b>Descrizione Variazione</b></td></tr>";

                    foreach (ElementoPreferiti rigaPreferiti in preferitiHome)
                    {

                        string stato = string.Empty;
                        switch (rigaPreferiti.Rating)
                        {
                            case "1":
                                stato = "Verde 1";
                                break;
                            case "2":
                                stato = "Verde 2";
                                break;
                            case "3":
                                stato = "Verde 3";
                                break;
                            case "4":
                                stato = "Giallo";
                                break;
                            case "5":
                                stato = "Rosso";
                                break;
                            default:
                                stato = "N.D.";
                                break;
                        }

                        BufferText +=
                            String.Format("<tr><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td><td>{4}</td><td>{5}</td><td>{6}</td></tr>",
                            rigaPreferiti.PartitaIva,
                            rigaPreferiti.RagioneSociale,
                            rigaPreferiti.Esposizione,
                            rigaPreferiti.Fido,                            
                            stato,
                            rigaPreferiti.DataAggiornamento,                                                         
                            rigaPreferiti.DescrizioneVariazione);
                    }
                    
                    break;

                    case "Monitoraggi.xls":
                        BufferText += "<tr><td colspan='6'><b>Riepilogo Monitoraggi Attivi</b></td></tr>";
                        BufferText += "<table><tr><td><b>Partita Iva</b></td><td><b>Ragione Sociale</b></td><td><b>Esposizione</b></td><td><b>Valutazione</b></td><td><b>Eventi Negativi</b></td><td><b>Fido</b></td></tr>";

                        foreach (ElementoPreferiti rigaPreferiti in preferitiHome)
                        {

                            string stato = string.Empty;
                            switch (rigaPreferiti.Rating)
                            {
                                case "1":
                                    stato = "Verde 1";
                                    break;
                                case "2":
                                    stato = "Verde 2";
                                    break;
                                case "3":
                                    stato = "Verde 3";
                                    break;
                                case "4":
                                    stato = "Giallo";
                                    break;
                                case "5":
                                    stato = "Rosso";
                                    break;
                                default:
                                    stato = "N.D.";
                                    break;
                            }

                            BufferText +=
                                String.Format("<tr><td>- {0} -</td><td>{1}</td><td>{2}</td><td>{3}</td><td>{4}</td><td>{5}</td><td>{5}</td></tr>", rigaPreferiti.PartitaIva, rigaPreferiti.RagioneSociale,  rigaPreferiti.Esposizione, stato,rigaPreferiti.EventiNegativi,rigaPreferiti.Fido);
                    }
                    break;
            }
            BufferText += "</table>";
        
        }

        public override void ExecuteResult(ControllerContext context)
        {
            context.HttpContext.Response.Buffer = true;
            context.HttpContext.Response.Clear();
            context.HttpContext.Response.AddHeader("content-disposition", "attachment; filename=" + FileName);
            context.HttpContext.Response.ContentType = "application/vnd.ms-excel";
            context.HttpContext.Response.Write(BufferText);
        }
    }
}
