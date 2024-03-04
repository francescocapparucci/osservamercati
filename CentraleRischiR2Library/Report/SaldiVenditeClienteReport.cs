using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClosedXML.Excel;
using System.IO;

namespace CentraleRischiR2Library.Report
{
    ///*
    //public class SaldiVenditeClienteReport : Report
    //{
    //    public override Stream GetReport(int numeroAziende, int salto)
    //    {


    //        XLWorkbook wb = new XLWorkbook();
    //        //FileInfo infoCSV = new FileInfo("C:\\Users\\acodeluppi\\Desktop\\csvreport.csv");
    //        //FileInfo infoCSVNEW = new FileInfo("C:\\Users\\acodeluppi\\Desktop\\report_new.csv");            
    //        //FileInfo infoCSVNEW = new FileInfo(String.Format("{0}\\report_cliente.csv", ConfigurationManager.AppSettings["PathReport"]));
    //        //StreamReader re = File.OpenText(infoCSV.FullName);





    //        DateTime dataInizioRicercaSaldo = DataInizio.AddMonths(-7);
    //        DateTime dataBuffer = DataInizio;

    //        //DateTime dataInizioCruscotto = Convert.ToDateTime(ConfigurationManager.AppSettings["DataInizioCruscotto"]);



    //        //int idCentro = Convert.ToInt32(ConfigurationManager.AppSettings["IdCentro"]);

    //        using (OsservaCentraleRischiEntities context = new OsservaCentraleRischiEntities())
    //        {
    //            /*COMPOSIZIONE COLONNE */
    //            context.CommandTimeout = 30000;
    //            //wr.Write("PIVA OPERATORE;");
    //            //wr.Write("RAGIONE SOCIALE OPERATORE;");
    //            while (dataBuffer < DataFine)
    //            {

    //                m02_anagrafica aziendaCliente =
    //                    context.m02_anagrafica.Where((o) => o.m02_partitaiva == PartitaIva).ToList()[0];

    //                DateTime dataUltimoGiornoMeseCorrente = new DateTime(dataBuffer.Year, dataBuffer.Month, 1).AddMonths(1).AddDays(-1);


    //                IXLWorksheet ws = wb.Worksheets.Add(String.Format("MENSILITA {0:MM-yyyy}", dataBuffer));
    //                ws.Cell("A1").Value = String.Format("VENDITE SALDI CLIENTE " + aziendaCliente.m02_ragionesociale + " MENSILITA {0:MM-yyyy}", dataBuffer);
    //                ws.Cell("A2").Value = "MERCATO";
    //                ws.Cell("B2").Value = "OPERATORE";
    //                ws.Cell("C2").Value = "VENDITE";
    //                ws.Cell("D2").Value = "SALDI";


    //                /*
    //                ws.Cell("A1").Value = 
    //                wr.Write(String.Format("VENDITE {0:MM-yyyy};", dataBuffer));
    //                wr.Write(String.Format("SALDI {0:MM-yyyy};", dataBuffer));
    //                //dataBuffer = dataBuffer.AddMonths(1);
    //                */

    //                /*SELEZIONE AZIENDE CHE HANNO COMMERCIATO CON IL CLIENTE NELLA MENSILITA INDICATA*/



    //                List<m01_aziende> aziendeVenditeMese =
    //                    context.m01_aziende.Join(
    //                        context.m04_vendite.Where(o => o.m04_dtdocvendita.Year == dataBuffer.Year && o.m04_dtdocvendita.Month == dataBuffer.Month && o.m04_partitaiva == PartitaIva),
    //                        e => e.m01_codice,
    //                        o => o.m04_codiceazienda,
    //                        (e, o) => e).ToList();

    //                List<m01_aziende> aziendeSaldiMese =
    //                    context.m01_aziende.Join(
    //                        context.m03_saldi.Where(o => o.m03_dtsaldo >= dataInizioRicercaSaldo && o.m03_dtsaldo <= dataUltimoGiornoMeseCorrente && o.m03_partitaiva == PartitaIva),
    //                        e => e.m01_codice,
    //                        o => o.m03_codiceazienda,
    //                        (e, o) => e).ToList();


    //                List<m01_aziende> aziende = aziendeSaldiMese.Union(aziendeVenditeMese).Distinct().Skip(salto).ToList();



    //                int riga = 3;


    //                foreach (m01_aziende azienda in aziende)
    //                {



    //                    m00_centri centro = context.m00_centri.Where(o => o.m00_idcentro == azienda.m01_idcentro).ToList()[0];


    //                    /*VENDITE AZIENDA*/
    //                    double? importoVenditaMese = 0;
    //                    try
    //                    {
    //                        importoVenditaMese =
    //                            context.m04_vendite.Where((o) => o.m04_codiceazienda == azienda.m01_codice &&
    //                            (o.m04_dtdocvendita.Year == dataBuffer.Year && o.m04_dtdocvendita.Month == dataBuffer.Month) && o.m04_partitaiva == PartitaIva)
    //                            .Sum((o) => o.m04_importo);
    //                    }
    //                    catch (Exception ex)
    //                    {
    //                        Console.WriteLine(ex.Message);
    //                    }


    //                    /*SALDO AZIENDA */
    //                    decimal saldo = 0;

    //                    try
    //                    {
    //                        var listaDistinctPiva =
    //                                context.m03_saldi
    //                        .Where((o) => o.m03_codiceazienda == azienda.m01_codice && (o.m03_dtsaldo >= dataInizioRicercaSaldo && o.m03_dtsaldo <= dataUltimoGiornoMeseCorrente) && o.m03_partitaiva == PartitaIva && o.m03_saldo >= 0)
    //                        .Select((o) => o.m03_codiceazienda).Distinct();
    //                        var bufferExpression =
    //                        listaDistinctPiva.Select((codiceOperatore) =>
    //                            new
    //                            {
    //                                codiceOperatore,
    //                                primoSaldo =

    //                                context.m03_saldi
    //                                .Where((o) => o.m03_codiceazienda == codiceOperatore &&
    //                                (o.m03_dtsaldo >= dataInizioRicercaSaldo && o.m03_dtsaldo <= dataUltimoGiornoMeseCorrente) && o.m03_partitaiva == PartitaIva && o.m03_saldo >= 0)
    //                                    .OrderByDescending((o) => o.m03_dtsaldo)
    //                                .FirstOrDefault() != null ?

    //                                context.m03_saldi
    //                                .Where((o) => o.m03_codiceazienda == codiceOperatore &&
    //                                (o.m03_dtsaldo >= dataInizioRicercaSaldo && o.m03_dtsaldo <= dataUltimoGiornoMeseCorrente) && o.m03_partitaiva == PartitaIva && o.m03_saldo >= 0)
    //                                .OrderByDescending((o) => o.m03_dtsaldo)
    //                                .FirstOrDefault().m03_saldo :
    //                                0
    //                            }
    //                                ).ToList();
    //                        saldo = bufferExpression.Sum((o) => o.primoSaldo) != null ? bufferExpression.Sum((o) => o.primoSaldo) : 0;

    //                    }
    //                    catch (Exception ex)
    //                    {
    //                        Console.WriteLine(ex.Message);
    //                    }

    //                    if (saldo != 0 || importoVenditaMese != 0)
    //                    {

    //                        ws.Cell("A" + riga).Value = centro.m00_centro;
    //                        ws.Cell("B" + riga).Value = azienda.m01_ragionesociale;
    //                        ws.Cell("C" + riga).Value = importoVenditaMese;
    //                        ws.Cell("D" + riga).Value = saldo;
    //                        riga++;

    //                    }



    //                }
    //                ws.Columns().AdjustToContents();
    //                dataBuffer = dataBuffer.AddMonths(1);






    //            }


    //            MemoryStream returnValue = new MemoryStream();
    //            wb.SaveAs(returnValue);
    //            returnValue.Position = 0;

    //            return returnValue;

    //        }

    //    }
    //}
}

