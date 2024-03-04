using CentraleRischiR2.Classes;
using CentraleRischiR2.Models;
using CentraleRischiR2Library;
using CentraleRischiR2Library.BridgeClasses;
using Microsoft.Office.Interop.Excel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Configuration;
using System.Web.Mvc;
using ClosedXML.Excel;
using System.Web;

namespace CentraleRischiR2.Classes
{
    public class ExcelCs
    {
        /*public MemoryStream ReportExcelCs(JsonSeralizespriv json, string piva)
            {
               // string filePath = WebConfigurationManager.AppSettings["ReportPath"];
                string filePath = WebConfigurationManager.AppSettings["ReportPath_Local"]; 
                XLWorkbook wb = new XLWorkbook();
                IXLWorksheet worKsheeTAz = wb.Worksheets.Add("RapportoCs");

                worKsheeTAz.Cells("A1").Style.Font.Bold = true;
                worKsheeTAz.Cells("A1").Style.Font.FontSize = 26;
                worKsheeTAz.Cells("A1").Style.Fill.BackgroundColor = XLColor.Red;
                worKsheeTAz.Cells("A1").Value = "REPORT CREDIT SAFE";
                worKsheeTAz.Cell("A1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worKsheeTAz.Range("A1:B1").Merge();
                worKsheeTAz.Columns("A").Width = 50;
                worKsheeTAz.Columns("B").Width = 40;

                worKsheeTAz.Range("A2:B2").Merge();
                worKsheeTAz.Cells("A2").Style.Font.Bold = true;
                worKsheeTAz.Cells("A2").Style.Font.FontSize = 14;
                worKsheeTAz.Cells("A2").Style.Fill.BackgroundColor = XLColor.BlueGreen;
                worKsheeTAz.Cells("A2").Value = "AZIENDA";
                worKsheeTAz.Cell("A2").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worKsheeTAz.Cells("A3").Style.Font.Bold = true;
                worKsheeTAz.Cells("A3").Value = "CODICE CREDITSAFE";
                worKsheeTAz.Cells("B3").Value = json.report.companyId;

                worKsheeTAz.Cells("A4").Style.Font.Bold = true;
                worKsheeTAz.Cells("A4").Value = "RAGIONE SOCIALE";
                worKsheeTAz.Cells("B4").Value = json.report.alternateSummary.legalForm;

                worKsheeTAz.Cells("A5").Style.Font.Bold = true;
                worKsheeTAz.Cells("A5").Value = "PARTITA IVA";
                worKsheeTAz.Cells("B5").Value = json.report.alternateSummary.vatRegistrationNumber;


                worKsheeTAz.Range("A6:B6").Merge();
                worKsheeTAz.Cells("A6").Style.Font.Bold = true;
                worKsheeTAz.Cells("A6").Style.Font.FontSize = 14;
                worKsheeTAz.Cells("A6").Style.Fill.BackgroundColor = XLColor.BlueGreen;
                worKsheeTAz.Cells("A6").Value = "MERITO CREDITIZIO";
                worKsheeTAz.Cell("A6").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worKsheeTAz.Cells("A7").Style.Font.Bold = true;
                worKsheeTAz.Cells("A7").Value = "PUNTEGGIO DI OGGI";
                worKsheeTAz.Cells("B7").Value = json.report.creditScore.currentCreditRating.providerValue.value;

                worKsheeTAz.Cells("A8").Style.Font.Bold = true;
                worKsheeTAz.Cells("A8").Value = "CREDITO DI OGGI";
                if (Convert.ToInt32(json.report.creditScore.currentCreditRating.creditLimit.value) < 30)
                {
                    worKsheeTAz.Cells("B8").Style.Fill.BackgroundColor = XLColor.Red;
                }
                else if (Convert.ToInt32(json.report.creditScore.currentCreditRating.creditLimit.value) > 30
                       && Convert.ToInt32(json.report.creditScore.currentCreditRating.creditLimit.value) < 60)
                {

                    worKsheeTAz.Cells("B8").Style.Fill.BackgroundColor = XLColor.Orange;
                }
                else
                {
                    worKsheeTAz.Cells("B8").Style.Fill.BackgroundColor = XLColor.Green;
                }
                worKsheeTAz.Cells("B8").Value = json.report.creditScore.currentCreditRating.creditLimit.value;

                worKsheeTAz.Cells("A9").Style.Font.Bold = true;
                worKsheeTAz.Cells("A9").Value = "DATA MERITO CREDITIZIO";
                worKsheeTAz.Cells("B9").Value = json.report.creditScore.latestRatingChangeDate;

                worKsheeTAz.Cells("A10").Style.Font.Bold = true;
                worKsheeTAz.Cells("A10").Value = "STATO";
                worKsheeTAz.Cells("B10").Value = json.report.companySummary.companyStatus.status;

                worKsheeTAz.Cells("A11").Style.Font.Bold = true;
                worKsheeTAz.Cells("A11").Value = "PROTESTI";
                worKsheeTAz.Cells("B11").Value = json.report.additionalInformation.shareholdingCompanies[0].hasProtesti;

                worKsheeTAz.Cells("A12").Style.Font.Bold = true;
                worKsheeTAz.Cells("A12").Value = "IMPORTO DEI PROTESTI";
                worKsheeTAz.Cells("B12").Value = json.report.additionalInformation.shareholders[0].sharesStockNumber;

                worKsheeTAz.Range("A13:B13").Merge();
                worKsheeTAz.Cells("A13").Style.Font.Bold = true;
                worKsheeTAz.Cells("A13").Style.Font.FontSize = 14;
                worKsheeTAz.Cells("A13").Style.Fill.BackgroundColor = XLColor.BlueGreen;
                worKsheeTAz.Cells("A13").Value = "INDIRIZZO";
                worKsheeTAz.Cell("A13").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worKsheeTAz.Cells("A14").Style.Font.Bold = true;
                worKsheeTAz.Cells("A14").Value = "INDIRIZZO COMPLETO";
                worKsheeTAz.Cells("B14").Value = json.report.alternateSummary.address;

                worKsheeTAz.Cells("A14").Style.Font.Bold = true;
                worKsheeTAz.Cells("A14").Value = "PROVINCIA";
                worKsheeTAz.Cells("B14").Value = json.report.alternateSummary.province;

                worKsheeTAz.Cells("A15").Style.Font.Bold = true;
                worKsheeTAz.Cells("A15").Value = "INDIRIZZO @MAIL";
                worKsheeTAz.Cells("B15").Value = json.report.contactInformation.emailAddresses;

                worKsheeTAz.Cells("A16").Style.Font.Bold = true;
                worKsheeTAz.Cells("A16").Value = "TELEFONO";
                worKsheeTAz.Cells("B16").Value = json.report.alternateSummary.telephone;

                worKsheeTAz.Range("A17:B17").Merge();
                worKsheeTAz.Cells("A17").Style.Font.Bold = true;
                worKsheeTAz.Cells("A17").Style.Font.FontSize = 14;
                worKsheeTAz.Cells("A17").Style.Fill.BackgroundColor = XLColor.BlueGreen;
                worKsheeTAz.Cells("A17").Value = "DATI SOCIETARI";
                worKsheeTAz.Cell("A17").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worKsheeTAz.Cells("A18").Style.Font.Bold = true;
                worKsheeTAz.Cells("A18").Value = "NUMERO ADDETTI";
                worKsheeTAz.Cells("B18").Value = json.report.alternateSummary.numberOfEmployees;

                worKsheeTAz.Cells("A19").Style.Font.Bold = true;
                worKsheeTAz.Cells("A19").Value = "STATO SOCIETA'";
                worKsheeTAz.Cells("B19").Value = json.report.companySummary.companyStatus.status;

                worKsheeTAz.Cells("A20").Style.Font.Bold = true;
                worKsheeTAz.Cells("A20").Value = "CAPITALE SOCIALE";
                worKsheeTAz.Cells("B20").Value = json.report.alternateSummary.shareCapital;

                worKsheeTAz.Cells("A21").Style.Font.Bold = true;
                worKsheeTAz.Cells("A21").Value = "DATA DI COSTITUZIONE";
                worKsheeTAz.Cells("B21").Value = json.report.alternateSummary.incorporationDate;

                worKsheeTAz.Cells("A22").Style.Font.Bold = true;
                worKsheeTAz.Cells("A22").Value = "DATA ISCRIZIONE REA";
                worKsheeTAz.Cells("B22").Value = json.report.alternateSummary.reaInscriptionDate;

                worKsheeTAz.Range("A23:B23").Merge();
                worKsheeTAz.Cells("A23").Style.Font.Bold = true;
                worKsheeTAz.Cells("A23").Style.Font.FontSize = 14;
                worKsheeTAz.Cells("A23").Style.Fill.BackgroundColor = XLColor.BlueGreen;
                worKsheeTAz.Cells("A23").Value = "DATI FINANZIARI";
                worKsheeTAz.Cell("A23").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worKsheeTAz.Cells("A24").Style.Font.Bold = true;
                worKsheeTAz.Cells("A24").Value = "ULTIMA DATA CHIUSURA DELL'ANNO CONTABILE";
                worKsheeTAz.Cells("B24").Value = json.report.additionalInformation.misc.latestYearEndOfAccounts;

                worKsheeTAz.Cells("A25").Style.Font.Bold = true;
                worKsheeTAz.Cells("A25").Value = " TOTALE VALORE DELLA PRODUZIONE";
                worKsheeTAz.Cells("B25").Value = json.report.companySummary.latestTurnoverFigure.value;

                worKsheeTAz.Cells("A26").Style.Font.Bold = true;
                worKsheeTAz.Cells("A26").Value = " UTILE(PERDITA) DELL'ESERCIZIO";
                worKsheeTAz.Cells("B26").Value = "";

                worKsheeTAz.Cells("A27").Style.Font.Bold = true;
                worKsheeTAz.Cells("A27").Value = " TOTALE VALORE DELLA PRODUZIONE";
                worKsheeTAz.Cells("B27").Value = json.report.companySummary.latestTurnoverFigure.value;

                worKsheeTAz.Range("A28:B28").Merge();
                worKsheeTAz.Cells("A28").Style.Font.Bold = true;
                worKsheeTAz.Cells("A28").Style.Font.FontSize = 14;
                worKsheeTAz.Cells("A28").Style.Fill.BackgroundColor = XLColor.BlueGreen;
                worKsheeTAz.Cells("A28").Value = "DIRIGENTE";
                worKsheeTAz.Cell("A28").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worKsheeTAz.Cells("A29").Style.Font.Bold = true;
                worKsheeTAz.Cells("A29").Value = "NOME";
                worKsheeTAz.Cells("B29").Value = json.report.directors.currentDirectors[0].name;

                worKsheeTAz.Cells("A30").Style.Font.Bold = true;
                worKsheeTAz.Cells("A30").Value = "COGNOME";
                worKsheeTAz.Cells("B30").Value = json.report.directors.currentDirectors[0].surname;

                worKsheeTAz.Cells("A31").Style.Font.Bold = true;
                worKsheeTAz.Cells("A31").Value = "CODICE FISCALE";
                worKsheeTAz.Cells("B31").Value = json.report.directors.currentDirectors[0].id;

                worKsheeTAz.Cells("A32").Style.Font.Bold = true;
                worKsheeTAz.Cells("A32").Value = "POSIZIONE";
                worKsheeTAz.Cells("B32").Value = json.report.directors.currentDirectors[0].positions[0].positionName;

                worKsheeTAz.Cells("A33").Style.Font.Bold = true;
                worKsheeTAz.Cells("A33").Value = "DATA INCARICO";
                worKsheeTAz.Cells("B33").Value = json.report.directors.currentDirectors[0].positions[0].dateAppointed;

                worKsheeTAz.Cells("A34").Style.Font.Bold = true;
                worKsheeTAz.Cells("A34").Value = "TIPO DURATA INCARICO";
                worKsheeTAz.Cells("B34").Value = json.report.directors.currentDirectors[0].positions[0].apptDurationType;

                for (int i = 2; i < 35; i++)
                {
                    string test = "B" + i.ToString();
                    worKsheeTAz.Cell(test).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }

        }
        public static MemoryStream SaveWorkbookToMemoryStream(XLWorkbook workbook)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                workbook.SaveAs(stream, new SaveOptions { EvaluateFormulasBeforeSaving = false, GenerateCalculationChain = false, ValidatePackage = false });
                return stream;
            }
        }*/

    }
}