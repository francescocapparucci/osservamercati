using ClosedXML.Excel;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Web;

namespace CentraleRischiR2.Classes
{
    public class BuildExcel
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public void buildExcelMain(dynamic json, string filePath, string fileName)
        {
            Log.Info("begin buildExcelMain ");
            ExcelCs reportCs = new ExcelCs();
            XLWorkbook wb = new XLWorkbook();
            IXLWorksheet worKsheeTAz = wb.Worksheets.Add("RapportoCs");
            worKsheeTAz.Cells("A1").Style.Font.FontSize = 26;
            worKsheeTAz.Cells("A1").Style.Fill.BackgroundColor = XLColor.Red;
            worKsheeTAz.Cells("A1").Value = "REPORT CREDIT SAFE";
            worKsheeTAz.Cell("A1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worKsheeTAz.Range("A1:B1").Merge();
            worKsheeTAz.Columns("A").Width = 50;
            worKsheeTAz.Columns("B").Width = 40;

            worKsheeTAz.Range("A2:B2").Merge();
            worKsheeTAz.Cells("A2").Style.Font.FontSize = 14;
            worKsheeTAz.Cells("A2").Style.Fill.BackgroundColor = XLColor.BlueGreen;
            worKsheeTAz.Cells("A2").Value = "AZIENDA";
            worKsheeTAz.Cell("A2").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            worKsheeTAz.Cells("A3").Value = "CODICE CREDITSAFE";
           // Log.Debug("B3 " + json.report.companyId);
            try { worKsheeTAz.Cells("B3").Value = (string) json.report.companyId; } catch { worKsheeTAz.Cells("B3").Value = "codice mancante"; }

            worKsheeTAz.Cells("A4").Value = "RAGIONE SOCIALE";
            try { worKsheeTAz.Cells("B4").Value = (string) json.report.alternateSummary.legalForm; } catch { worKsheeTAz.Cells("B4").Value = "ragione sociale assente"; }

            worKsheeTAz.Cells("A5").Value = "PARTITA IVA";
            try { worKsheeTAz.Cells("B5").Value = (string) json.report.alternateSummary.vatRegistrationNumber; } catch { worKsheeTAz.Cells("B5").Value = "partita iva assente"; }


            worKsheeTAz.Range("A6:B6").Merge();
            worKsheeTAz.Cells("A6").Style.Font.FontSize = 14;
            worKsheeTAz.Cells("A6").Style.Fill.BackgroundColor = XLColor.BlueGreen;
            worKsheeTAz.Cells("A6").Value = "MERITO CREDITIZIO";
            worKsheeTAz.Cell("A6").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            worKsheeTAz.Cells("A7").Value = "PUNTEGGIO DI OGGI";
            try { worKsheeTAz.Cells("B7").Value = (string) json.report.creditScore.currentCreditRating.providerValue.value; } catch { worKsheeTAz.Cells("B7").Value = "punteggio aseente"; }

            worKsheeTAz.Cells("A8").Value = "CREDITO DI OGGI";
            if (Convert.ToInt32((string) json.report.creditScore.currentCreditRating.creditLimit.value) > 71)
            {
                worKsheeTAz.Cells("B8").Style.Fill.BackgroundColor = XLColor.Green;
            }else if (Convert.ToInt32((string) json.report.creditScore.currentCreditRating.creditLimit.value) <= 71
                   && Convert.ToInt32((string) json.report.creditScore.currentCreditRating.creditLimit.value) > 51)
            {
                worKsheeTAz.Cells("B8").Style.Fill.BackgroundColor = XLColor.BrightGreen;
            }else if (Convert.ToInt32((string) json.report.creditScore.currentCreditRating.creditLimit.value) < 51
                       && Convert.ToInt32((string) json.report.creditScore.currentCreditRating.creditLimit.value) > 29)
            {
                worKsheeTAz.Cells("B8").Style.Fill.BackgroundColor = XLColor.Yellow;
            }else if (Convert.ToInt32((string) json.report.creditScore.currentCreditRating.creditLimit.value) <= 29
                           && Convert.ToInt32((string) json.report.creditScore.currentCreditRating.creditLimit.value) > 20)
            {
                worKsheeTAz.Cells("B8").Style.Fill.BackgroundColor = XLColor.Coral;
            }else
            {
                worKsheeTAz.Cells("B8").Style.Fill.BackgroundColor = XLColor.Red;
            }
            try { worKsheeTAz.Cells("B8").Value = (string) json.report.creditScore.currentCreditRating.creditLimit.value; } catch { worKsheeTAz.Cells("B8").Value = "credito assente"; }

            worKsheeTAz.Cells("A9").Value = "DATA MERITO CREDITIZIO";
            worKsheeTAz.Cells("B9").Value = (string) json.report.creditScore.latestRatingChangeDate ;

            worKsheeTAz.Cells("A10").Value = "STATO";
            try { worKsheeTAz.Cells("B10").Value = (string) json.report.companySummary.companyStatus.status; } catch { worKsheeTAz.Cells("B10").Value = "stato assente"; }

            worKsheeTAz.Cells("A11").Value = "PROTESTI";
            try { worKsheeTAz.Cells("B11").Value = (string) json.report.additionalInformation.shareholdingCompanies[0].hasProtesti; } catch { worKsheeTAz.Cells("B11").Value = "assente"; }

            worKsheeTAz.Cells("A12").Value = "IMPORTO DEI PROTESTI";
            try { worKsheeTAz.Cells("B12").Value = (string) json.report.additionalInformation.shareholders[0].sharesStockNumber; } catch { worKsheeTAz.Cells("B12").Value = "assente"; }

            worKsheeTAz.Range("A13:B13").Merge();
            worKsheeTAz.Cells("A13").Style.Font.FontSize = 14;
            worKsheeTAz.Cells("A13").Style.Fill.BackgroundColor = XLColor.BlueGreen;
            worKsheeTAz.Cells("A13").Value = "INDIRIZZO";
            worKsheeTAz.Cell("A13").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            worKsheeTAz.Cells("A14").Value = "PROVINCIA";
            try { worKsheeTAz.Cells("B14").Value = (string) json.report.alternateSummary.province; } catch { worKsheeTAz.Cells("B14").Value = "provincia assente"; }

            worKsheeTAz.Cells("A15").Value = "INDIRIZZO EMAIL";
            try { worKsheeTAz.Cells("B15").Value = (string) json.report.contactInformation.emailAddresses[0]; } catch { worKsheeTAz.Cells("B15").Value = "mail mancante"; }

            worKsheeTAz.Cells("A16").Value = "TELEFONO";
            try { worKsheeTAz.Cells("B16").Value = (string) json.report.alternateSummary.telephone; } catch { worKsheeTAz.Cells("B16").Value = "telefono assente"; }

            worKsheeTAz.Range("A17:B17").Merge();
            worKsheeTAz.Cells("A17").Style.Font.FontSize = 14;
            worKsheeTAz.Cells("A17").Style.Fill.BackgroundColor = XLColor.BlueGreen;
            worKsheeTAz.Cells("A17").Value = "DATI SOCIETARI";
            worKsheeTAz.Cell("A17").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            worKsheeTAz.Cells("A18").Value = "NUMERO ADDETTI";
            try { worKsheeTAz.Cells("B18").Value = (string) json.report.alternateSummary.numberOfEmployees; } catch { worKsheeTAz.Cells("B18").Value = "numero addetti assenti"; }

            worKsheeTAz.Cells("A19").Value = "STATO SOCIETA'";
            try { worKsheeTAz.Cells("B19").Value = (string) json.report.companySummary.companyStatus.status; } catch { worKsheeTAz.Cells("B19").Value = "stato societa assente"; }

            worKsheeTAz.Cells("A20").Value = "CAPITALE SOCIALE";
            try { worKsheeTAz.Cells("B20").Value = (string) json.report.alternateSummary.shareCapital; }catch{worKsheeTAz.Cells("B20").Value = "assente";}

            worKsheeTAz.Cells("A21").Value = "DATA DI COSTITUZIONE";
            try { worKsheeTAz.Cells("B21").Value = (string) json.report.alternateSummary.incorporationDate; } catch { worKsheeTAz.Cells("B21").Value = "seente"; }

            worKsheeTAz.Cells("A22").Value = "DATA ISCRIZIONE REA";
            try { worKsheeTAz.Cells("B22").Value = (string) json.report.alternateSummary.reaInscriptionDate; }catch{ worKsheeTAz.Cells("B22").Value = "assente"; }

            worKsheeTAz.Range("A23:B23").Merge();
            worKsheeTAz.Cells("A23").Style.Font.FontSize = 14;
            worKsheeTAz.Cells("A23").Style.Fill.BackgroundColor = XLColor.BlueGreen;
            worKsheeTAz.Cells("A23").Value = "DATI FINANZIARI";
            worKsheeTAz.Cell("A23").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            worKsheeTAz.Cells("A24").Value = "ULTIMA DATA CHIUSURA DELL'ANNO CONTABILE";
            try { worKsheeTAz.Cells("B24").Value = (string) json.report.additionalInformation.misc.latestYearEndOfAccounts; } catch { worKsheeTAz.Cells("B24").Value = ""; }

            worKsheeTAz.Cells("A25").Value = " TOTALE VALORE DELLA PRODUZIONE";
            try { worKsheeTAz.Cells("B25").Value = (string) json.report.companySummary.latestTurnoverFigure.value; } catch { worKsheeTAz.Cells("B25").Value = "assente"; }

            worKsheeTAz.Cells("A26").Value = " UTILE(PERDITA) DELL'ESERCIZIO";
            worKsheeTAz.Cells("B26").Value = "";

            worKsheeTAz.Cells("A27").Value = " TOTALE VALORE DELLA PRODUZIONE";
            try { worKsheeTAz.Cells("B27").Value = (string) json.report.companySummary.latestTurnoverFigure.value; }catch { worKsheeTAz.Cells("B27").Value = "assente"; }

            worKsheeTAz.Range("A28:B28").Merge();
            worKsheeTAz.Cells("A28").Style.Font.FontSize = 14;
            worKsheeTAz.Cells("A28").Style.Fill.BackgroundColor = XLColor.BlueGreen;
            worKsheeTAz.Cells("A28").Value = "DIRIGENTE";
            worKsheeTAz.Cell("A28").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            worKsheeTAz.Cells("A29").Value = "NOME";
            try { worKsheeTAz.Cells("B29").Value = (string) json.report.directors.currentDirectors.name; } catch { worKsheeTAz.Cells("B29").Value = ""; }


            worKsheeTAz.Cells("A30").Value = "COGNOME";
            try { worKsheeTAz.Cells("B30").Value = (string) json.report.directors.currentDirectors[0].surname; } catch { worKsheeTAz.Cells("B30").Value = ""; }

            worKsheeTAz.Cells("A31").Value = "CODICE FISCALE";
            try { worKsheeTAz.Cells("B31").Value = (string) json.report.directors.currentDirectors[0].id; } catch { worKsheeTAz.Cells("B31").Value = ""; }

            worKsheeTAz.Cells("A32").Value = "POSIZIONE";
            try { worKsheeTAz.Cells("B32").Value = (string) json.report.directors.currentDirectors[0].positions[0].positionName; } catch { worKsheeTAz.Cells("B32").Value = ""; }

            worKsheeTAz.Cells("A33").Value = "DATA INCARICO";
            try { worKsheeTAz.Cells("B33").Value = (string) json.report.directors.currentDirectors[0].positions[0].dateAppointed; } catch { worKsheeTAz.Cells("B33").Value = ""; }

            worKsheeTAz.Cells("A34").Value = "TIPO DURATA INCARICO";
            try { worKsheeTAz.Cells("B34").Value = (string) json.report.directors.currentDirectors[0].positions[0].apptDurationType; } catch { worKsheeTAz.Cells("B34").Value = ""; }

            for (int z = 1; z < 35; z++)
            {
                worKsheeTAz.Cells("A" + z).Style.Font.Bold = true;
            }

            for (int i = 2; i < 35; i++)
            {
                string test = "B" + i.ToString();
                worKsheeTAz.Cell(test).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }
            Log.Info("end buildExcelMain = " + wb.ToString());
            saveExcelMain(wb, filePath, fileName);
            // Process.Start(filePath + fileName);

        }




        private void saveExcelMain(XLWorkbook wb, string filePath, string fileName)
        {
            Log.Info("begin saveExcelMain");
            wb.SaveAs(filePath + fileName);
            Log.Info("SAVE SUCCESFULL ! ! ! ");
        }
    }
}
            
        
    

