using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Configuration;
using CentraleRischiR2Library;
using CentraleRischiR2.Classes;
using System.Data;
using CentraleRischiR2Library.BridgeClasses;
using Newtonsoft.Json;
using Newtonsoft.Json.Utilities;
using Newtonsoft.Json.Linq;
using log4net;
using System.Data.SqlClient;
using System.Text;

namespace CentraleRischiR2.Controllers
{
    public class MercatoController : BaseController
    {
        
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public  DateTime dataMassima = DateTime.Now.AddMonths(-14);

       [Authorize]
        public ActionResult Index()
        {

            ViewBag.NumeroMonitoraggiReportDisponibili = NumeroMonitoraggiReportDisponibili;
            ViewBag.VerificaSuperamentoSogliaReport = VerificaSuperamentoSogliaReport;
            return View();
        }

        protected SelectList MercatiDisponibili
        {
            get
            {
                var dictionaryMercati = DBHandler.ElencoMercati();
                dictionaryMercati.Add("Key", "Value");
                return new SelectList(dictionaryMercati, "Key", "Value");
            }
        }



        //
        // GET: /Mercato/
        [Authorize]
        public ActionResult Andamento()
        {
            if (loggeduser == null)
            {
                RedirectToAction("Index", "Home");
            }
            ViewBag.IdUser = loggeduser.IdUser;
            ViewBag.RicercaAbilitata = loggeduser.AbilitatoRicerca;
            ViewBag.NumeroMonitoraggiReportDisponibili = NumeroMonitoraggiReportDisponibili;
            ViewBag.VerificaSuperamentoSogliaReport = VerificaSuperamentoSogliaReport;
            ViewBag.ProvinceItaliane = ProvinceItaliane;
            ViewBag.ElencoMercati = DBHandler.ElencoMercati();
            ViewBag.Username = loggeduser.Username;
            ViewBag.Azienda = loggeduser.Azienda;
            using (DemoR2Entities context = new DemoR2Entities())
            {
                ViewBag.ReportCount = context.richiesta_report.Where(rep => rep.id_utente == loggeduser.IdUser && rep.evasa == false).Count();
                loggeduser.ReportCount = ViewBag.ReportCount;
            }
            ViewBag.Mercato = loggeduser.Mercato;
            ViewBag.Demo = loggeduser.Demo;
            ViewBag.Ambiente = loggeduser.Ambiente;
            ViewBag.IdRuolo = loggeduser.IdRuolo;
            ViewBag.PartitaIva = !String.IsNullOrEmpty(Request.QueryString["piva"]) ? Request.QueryString["piva"] : "";
            /* if (loggeduser.IdRuolo == 0)
             {
                 ViewBag.Andamento = !String.IsNullOrEmpty(Request.QueryString["andamento"]) ? Request.QueryString["andamento"] : "nazione";
             }
             else
             {
                 ViewBag.Andamento = !String.IsNullOrEmpty(Request.QueryString["andamento"]) ? Request.QueryString["andamento"] : "mercato";
             }*/
            ViewBag.Andamento = !String.IsNullOrEmpty(Request.QueryString["andamento"]) ? Request.QueryString["andamento"] : "mercato";
            ViewBag.PartitaIva = !String.IsNullOrEmpty(Request.QueryString["piva"]) ? Request.QueryString["piva"] : "";

            ViewBag.IdMercato = !String.IsNullOrEmpty(Request.Form["IdMercato"]) ? Request.Form["IdMercato"] : "0";

            int numeroMesi = Convert.ToInt32(WebConfigurationManager.AppSettings["NumeroMesiAndamento"]);

            List<ElementoGraficoMercato> elementiTabella = new List<ElementoGraficoMercato>();
            List<ElementoGraficoMercato> elementiGrafico = new List<ElementoGraficoMercato>();


            string andamento = ViewBag.Andamento;

            if (ViewBag.PartitaIva != String.Empty)
            {

                switch (andamento)
                {
                    case "mercato":
                        elementiTabella = GetTabellaMercato("", ViewBag.PartitaIva, ViewBag.IdMercato, numeroMesi);
                        break;
                    case "nazione":
                        elementiTabella = GetTabellaNazione("", ViewBag.PartitaIva, numeroMesi);
                        break;
                    case "azienda":
                        elementiTabella = GetTabellaAzienda("", ViewBag.PartitaIva, numeroMesi);
                        break;
                }
                /*
                if (ViewBag.Andamento == "mercato")
                {
                    elementiTabella = GetTabellaMercato("", ViewBag.PartitaIva, ViewBag.IdMercato,numeroMesi);                                        
                }
                else
                {
                    elementiTabella = GetTabellaNazione("", ViewBag.PartitaIva,numeroMesi);                                        
                }
                */
                ElementoAnagraficheOsservatorio aziendaO = DBHandler.GetAziendaOsservatorio(ViewBag.PartitaIva);
                ViewBag.RagioneSociale = aziendaO != null ? aziendaO.RagioneSociale : String.Empty;
                elementiTabella = DBHandler.AggiungiReportRating(elementiTabella, ViewBag.PartitaIva, loggeduser);

            }
            else
            {
                switch (andamento)
                {
                    case "mercato":
                        elementiTabella = GetTabellaMercato("", ViewBag.PartitaIva, ViewBag.IdMercato, numeroMesi);
                        break;
                    case "nazione":
                        elementiTabella = GetTabellaNazione("", ViewBag.PartitaIva, numeroMesi);
                        break;
                    case "azienda":
                        elementiTabella = GetTabellaAzienda("", ViewBag.PartitaIva, numeroMesi);
                        break;
                }
                /*
                if (ViewBag.Andamento == "mercato")
                {
                    elementiTabella = GetTabellaMercato("", "", ViewBag.IdMercato);
                }
                else
                {
                    elementiTabella = GetTabellaNazione();
                }
                */
            }
            ViewBag.ElementiTabella = elementiTabella.Take(numeroMesi - 1).OrderBy(o => o.Mensilita);
            ElementoGraficoMercato actual = elementiTabella.OrderBy(o => o.Mensilita).LastOrDefault();
            
            string partitaIva = Request.QueryString["piva"];
            ViewBag.ActualReport = "";
            if (actual != null)
            {
                ViewBag.ActualVendite = actual.Vendite;
                ViewBag.ActualSaldi = actual.Saldi;
                ViewBag.ActualIncassi = actual.Incassi;
                ViewBag.ActualGiorni = actual.Giorni;
            }
            else
            {
                ViewBag.ActualVendite = 0;
                ViewBag.ActualSaldi = 0;
                ViewBag.ActualIncassi = 0;
                ViewBag.ActualGiorni = 0;
            }
            using (DemoR2Entities context = new DemoR2Entities())
            {
                m02_anagrafica anagrafica = context.m02_anagrafica.Where(piva => piva.m02_partitaiva
                                       .Equals(partitaIva)).FirstOrDefault();
                if (anagrafica != null)
                {
                    ViewBag.ActualReport = anagrafica.m02_note;
                    ViewBag.ActualRating = anagrafica.m02_stato_validazione_cerved;
                }
                else ViewBag.ActualRating = 101;
            }

            return View();
        }

        //
        // GET: /Mercato/
        [Authorize]
        public ActionResult Posizionamento()
        {
            ViewBag.IdUser = loggeduser.IdUser;
            ViewBag.ProvinceItaliane = ProvinceItaliane;
            ViewBag.Username = loggeduser.Username;
            ViewBag.Azienda = loggeduser.Azienda;
            ViewBag.Mercato = loggeduser.Mercato;
            ViewBag.PartitaIva = !String.IsNullOrEmpty(Request.QueryString["piva"]) ? Request.QueryString["piva"] : "";
            ViewBag.Ambiente = loggeduser.Ambiente;
            ViewBag.IdRuolo = loggeduser.IdRuolo;
            ViewBag.Demo = loggeduser.Demo;
            ViewBag.NumeroMonitoraggiReportDisponibili = NumeroMonitoraggiReportDisponibili;
            ViewBag.VerificaSuperamentoSogliaReport = VerificaSuperamentoSogliaReport;

            return View();
        }

        private int getMonthDate(String mese_rif)
        {
            int giorni = 0;
            String cur_mese = String.Format("{0:yyyy-MM}", DateTime.Now);
            if (mese_rif != cur_mese)
            {
                giorni = System.DateTime.DaysInMonth(Convert.ToInt16(mese_rif.Substring(0, 4)), Convert.ToInt16(mese_rif.Substring(5, 2)));
            }
            else
            {
                giorni = DateTime.Now.Day;
            }
            return giorni;
        }

        private List<ElementoGraficoMercato> GetTabellaMercato(string ragioneSociale = "", string partitaIva = "", string idMercato = "0", int numeroMesi = 12)
        {
            List<string> listaAziendeCentro = GetListAziendeMercato(loggeduser.IdMercato);
            string piva = partitaIva.Trim();
            string idMercatoRicerca = idMercato != "0" ? idMercato.ToString() : loggeduser.IdMercato.ToString();
            List<ElementoGraficoMercato> elementiGraficoMercato = new List<ElementoGraficoMercato>();
            List<string> listappoggioPivaTotali = new List<string>();
            using (DemoR2Entities context = new DemoR2Entities())
            {
                listappoggioPivaTotali = context.s02_AggregazioneVendite.Where(wr => wr.IdCentro == loggeduser.IdMercato)
                                                                                     .Select(sel => sel.PartitaIva).ToList();
            }
             //   if (partitaIva == "01600480303") return elementiGraficoMercato;

            DataSet datiMercato = DBHandler.GetDatiCentro(idMercatoRicerca, ragioneSociale, partitaIva, WebConfigurationManager.ConnectionStrings["ConnectionStringR3"].ToString());

            datiMercato.Tables[0].TableName = "Importi";
            datiMercato.Tables[1].TableName = "Esposizione";

            DataTable dtEsposizione = datiMercato.Tables[1];
            DataTable dtVendite = datiMercato.Tables[0];


            datiMercato.Tables[0].PrimaryKey = new DataColumn[] { datiMercato.Tables[0].Columns["mese_rif"] };
            datiMercato.Tables[1].PrimaryKey = new DataColumn[] { datiMercato.Tables[1].Columns["mese_rif"] };

            /*CAMBIARE*/
            //int esposizionePrecedente = Convert.ToInt32(Math.Round(Convert.ToDecimal(datiMercato.Tables[1].Rows[0]["t03_esposizione_centro"])));
            DateTime dtInizio = new DateTime(DateTime.Now.AddMonths(-numeroMesi).Year, DateTime.Now.AddMonths(-numeroMesi).Month, 1);
            String primoMese = String.Format("{0:yyyy-MM}", new DateTime(DateTime.Now.AddMonths(-numeroMesi).Year, DateTime.Now.AddMonths(-numeroMesi).Month, 1));
            String mese_rif = String.Format("{0:yyyy-MM}", dtInizio);
            int venditeMesePrecedente = 0;

            // prendo la prima esposizione
            int esposizionePrecedente = 0;

            using (DemoR2Entities context = new DemoR2Entities())
            {
                if (partitaIva.Equals(""))
                {
                    esposizionePrecedente = Convert.ToInt32(context.s03_AggregazioneSaldoAziende.Where(sa => sa.Mese_rif.Equals(mese_rif)
                      && sa.IdCentro == loggeduser.IdMercato).Sum(sum => sum.Importo));
                }
                else
                {
                    esposizionePrecedente = Convert.ToInt32(context.s03_AggregazioneSaldoAziende.Where(sa => sa.Mese_rif.Equals(mese_rif)
                    && sa.IdCentro == loggeduser.IdMercato && sa.PartitaIva.Equals(partitaIva)).Sum(sum => sum.Importo));
                }
            }
            DateTime dtBuffer = dtInizio;
            DateTime dtFine = DateTime.Now;

            while (dtBuffer <= dtFine)
            {
                Dictionary<string, int> resultSaldi = new Dictionary<string, int>();
                string stringaData = String.Format("{0:yyyy-MM}", dtBuffer);
                //DataRow row = datiMercato.Tables[0].Rows.Find(stringaData);

                ElementoGraficoMercato elemento = new ElementoGraficoMercato();
                elemento.Mensilita = stringaData;
                elemento.DataMensilita = Convert.ToDateTime(String.Format("{0}-01", stringaData));
                elemento.Saldi = elemento.Vendite = 0;
                DateTime dataDA = Convert.ToDateTime(stringaData);
                DateTime dataA = dataDA.AddMonths(1);

                using (DemoR2Entities context = new DemoR2Entities())
                {
                    if (partitaIva.Equals("")) {
                        elemento.Vendite = Convert.ToInt32(context.m04_vendite.Where(ve => ve.m04_dtdocvendita >= dataDA
                                                                                       && ve.m04_dtdocvendita < dataA
                                                                                       && listaAziendeCentro.Contains(ve.m04_codiceazienda)
                                                                                       && listappoggioPivaTotali.Contains(ve.m04_partitaiva)
                                                                                      ).Sum(sum => sum.m04_importo));
                        elemento.Saldi = Convert.ToInt32(context.s03_AggregazioneSaldoAziende.Where(ag => ag.IdCentro == loggeduser.IdMercato
                                                                                                    && ag.Mese_rif.Equals(stringaData)
                                                                                                    ).Sum(sum => sum.Importo));
                    }
                    else
                    {
                        var autorizzato = 2;
                        if (loggeduser.IdRuolo != 1)
                        {
                            string codiceAzienda = loggeduser.CodiceAzienda;
                            autorizzato = context.m04_vendite.Where(a => a.m04_partitaiva.Equals(partitaIva) && a.m04_codiceazienda.Equals(codiceAzienda)).Count();
                        }

                        if (autorizzato > 0)
                        {
                            elemento.Vendite = Convert.ToInt32(context.m04_vendite.Where(ve => ve.m04_dtdocvendita >= dataDA
                                                                                                          && ve.m04_dtdocvendita < dataA
                                                                                                          && listaAziendeCentro.Contains(ve.m04_codiceazienda)
                                                                                                          && ve.m04_partitaiva.Equals(partitaIva)
                                                                                                         ).Sum(sum => sum.m04_importo));
                            elemento.Saldi = Convert.ToInt32(context.s03_AggregazioneSaldoAziende.Where(ag => ag.IdCentro == loggeduser.IdMercato
                                                                                                   && ag.Mese_rif.Equals(stringaData)
                                                                                                   && ag.PartitaIva.Equals(partitaIva)).Sum(sum => sum.Importo));
                        }
                        else
                        {
                            foreach(string el in listappoggioPivaTotali)
                            {
                                if (el.Trim().Equals(partitaIva))
                                {
                                    elemento.Vendite = Convert.ToInt32(context.m04_vendite.Where(ve => ve.m04_dtdocvendita >= dataDA
                                                                                                        && ve.m04_dtdocvendita < dataA
                                                                                                        && listaAziendeCentro.Contains(ve.m04_codiceazienda)
                                                                                                        && ve.m04_partitaiva.Equals(partitaIva)
                                                                                                       ).Sum(sum => sum.m04_importo));
                                    elemento.Saldi = Convert.ToInt32(context.s03_AggregazioneSaldoAziende.Where(ag => ag.IdCentro == loggeduser.IdMercato
                                                                                                       && ag.Mese_rif.Equals(stringaData)
                                                                                                       && ag.PartitaIva.Equals(partitaIva)).Sum(sum => sum.Importo));
                                    break;
                                }
                            }
                        }
                        elemento.RatingInt = calRationg(stringaData, partitaIva);
                    }
                }
                using (DemoR2Entities context = new DemoR2Entities())
                {
                    try
                    {
                        int totale = 0;
                        int vendite = elemento.Vendite;
                        int esposizione = elemento.Saldi;
                        DateTime dataMesePrecDa = dataDA;
                        DateTime dataMesePrecA = dataA;
                        DateTime dataOggi = DateTime.Now;

                        for (int i = 1; i < 13; i++)
                        {
                            totale = esposizione - vendite;
                            if (totale > 0)
                            {
                                if (stringaData.Equals(dataOggi.Year.ToString() + "-" + dataOggi.Month.ToString()))
                                {
                                    elemento.Giorni += DateTime.Now.Day;
                                    dataOggi = DateTime.Now.AddMonths(12);
                                }
                                else
                                {
                                    elemento.Giorni += System.DateTime.DaysInMonth(dataMesePrecDa.Year, dataMesePrecDa.Month);

                                }
                                esposizione = totale;
                                dataMesePrecDa = dataDA.AddMonths(-i);
                                dataMesePrecA = dataMesePrecDa.AddMonths(1);

                                if (partitaIva.Equals(""))
                                {
                                    vendite = Convert.ToInt32(context.m04_vendite.Where(ve => ve.m04_dtdocvendita >= dataMesePrecDa
                                                                                                              && ve.m04_dtdocvendita < dataMesePrecA
                                                                                                              && listaAziendeCentro.Contains(ve.m04_codiceazienda)
                                                                                                             ).Sum(sum => sum.m04_importo));
                                }
                                else
                                {
                                    vendite = Convert.ToInt32(context.m04_vendite.Where(ve => ve.m04_dtdocvendita >= dataMesePrecDa
                                                                                                          && ve.m04_dtdocvendita < dataMesePrecA
                                                                                                          && listaAziendeCentro.Contains(ve.m04_codiceazienda)
                                                                                                         && ve.m04_partitaiva.Equals(partitaIva)
                                                                                                          ).Sum(sum => sum.m04_importo));
                                }
                            }
                            else
                            {
                                int parzialeGiorni = System.DateTime.DaysInMonth(dataMesePrecDa.Year, dataMesePrecA.Month);
                                if (stringaData.Equals(dataOggi.Year.ToString() + "-" + dataOggi.Month.ToString()))
                                {
                                    elemento.Giorni += esposizione / (vendite / System.DateTime.DaysInMonth(dataDA.Year, dataDA.Month));
                                }
                                else
                                {
                                    elemento.Giorni += esposizione / (vendite / parzialeGiorni);
                                }
                                break;
                            }
                        }
                    }catch(DivideByZeroException e)
                    {
                        Log.Error(e.Message);
                    }
                }
                elemento.Incassi = esposizionePrecedente + elemento.Vendite - elemento.Saldi;
                elementiGraficoMercato.Add(elemento);
                esposizionePrecedente = elemento.Saldi;

                dtBuffer = dtBuffer.AddMonths(1);
            }

            /*CREO SCHEMA GIORNI
            DataTable tbWork = new DataTable();
            tbWork = new DataTable("dtFinale");
            tbWork.Columns.Add("mese_rif", typeof(String));
            /*1
            DateTime dtInizioSchema = dtInizio.AddMonths(1);
            for (int i = 0; i < numeroMesi; i++)
            {
                DateTime dtMeseRif = dtInizioSchema.AddMonths(i);
                tbWork.Columns.Add(String.Format("{0:yyyy-MM}", dtMeseRif), typeof(String));

            }

            DataView dvVendite = new DataView(datiMercato.Tables[0], "mese_rif<>'" + primoMese + "'", "mese_rif asc", DataViewRowState.CurrentRows);

            foreach (DataColumn dc in tbWork.Columns)
            {
                if (dc.ColumnName != "mese_rif")
                {
                    Decimal vendite = 0;
                    Decimal esposizione = 0;
                    using (DemoR2Entities context = new DemoR2Entities())
                    {
                        //drEsposizione = dtEsposizione.Rows.Find(dc.ColumnName);
                        //drEsposizione = context.s03_AggregazioneSaldoAziende.Where(ag => ag.Mese_rif.Equals(dc.ColumnName)).Select(sel=>sel.Importo).FirstOrDefault();
                        //DataRow drVendite = dtVendite.Rows.Find(dc.ColumnName);
                        //DataRow drVendite = 
                    }



                    //if (drVendite != null)
                    //{
                    //vendite = Convert.ToDecimal(drVendite[1DateTime dataDA = Convert.ToDateTime(stringaData);

                    DateTime dataDA = Convert.ToDateTime(dc.ColumnName + "-01");
                    DateTime dataA = dataDA.AddMonths(1);

                    using (DemoR2Entities context = new DemoR2Entities())
                    {
                        if (partitaIva.Equals(""))
                        {
                            vendite = Convert.ToInt32(context.m04_vendite.Where(ve => ve.m04_dtdocvendita >= dataDA
                                                                                      && ve.m04_dtdocvendita < dataA
                                                                                      && ve.m04_codiceazienda.Equals(loggeduser.CodiceAzienda)).Sum(sum => sum.m04_importo));
                        }
                        else
                        {
                            vendite = Convert.ToInt32(context.m04_vendite.Where(ve => ve.m04_dtdocvendita >= dataDA
                                                                                    && ve.m04_dtdocvendita < dataA
                                                                                    && ve.m04_codiceazienda.Equals(loggeduser.CodiceAzienda)
                                                                                    && ve.m04_partitaiva.Equals(partitaIva)).Sum(sum => sum.m04_importo));
                        }
                    }
                    //if (drEsposizione != null)
                    //{
                    //esposizione = Convert.ToDecimal(drEsposizione[1]);
                    using (DemoR2Entities context = new DemoR2Entities())
                    {
                        int appo = Convert.ToInt32(context.s03_AggregazioneSaldoAziende.Where(sa => sa.Mese_rif.Equals(dc.ColumnName)).Select(sel => sel.Importo).FirstOrDefault());
                        esposizione = Convert.ToDecimal(appo);
                    }
                    //}

                    DataRow drWork = tbWork.NewRow();
                    drWork["mese_rif"] = dc.ColumnName;

                    Decimal valore = esposizione - vendite;

                    if (valore < 0) valore = 0;

                    drWork[dc.ColumnName] = valore;


                    tbWork.Rows.Add(drWork);
                }
            }

            for (int count = 1; count < numeroMesi; count++)
            {

                for (int i = count; i >= 1; i--)
                {
                    string mese_Rif_interno = tbWork.Columns[i].ColumnName;

                    tbWork.Rows[count].BeginEdit();


                    Decimal vendite_mese = 0;

                    DateTime dataDA = Convert.ToDateTime(mese_Rif_interno + "-01");
                    DateTime dataA = dataDA.AddMonths(1);

                    using (DemoR2Entities context = new DemoR2Entities())
                    {
                        if (partitaIva.Equals(""))
                        {
                            vendite_mese = Convert.ToInt32(context.m04_vendite.Where(ve => ve.m04_dtdocvendita >= dataDA
                                                                                      && ve.m04_dtdocvendita < dataA
                                                                                      && ve.m04_codiceazienda.Equals(loggeduser.CodiceAzienda)).Sum(sum => sum.m04_importo));
                        }
                        else
                        {
                            vendite_mese = Convert.ToInt32(context.m04_vendite.Where(ve => ve.m04_dtdocvendita >= dataDA
                                                                                    && ve.m04_dtdocvendita < dataA
                                                                                    && ve.m04_codiceazienda.Equals(loggeduser.CodiceAzienda)
                                                                                    && ve.m04_partitaiva.Equals(partitaIva)).Sum(sum => sum.m04_importo));
                        }
                    }

                    /*if (dtVendite.Rows.Find(tbWork.Columns[i].ColumnName) != null)
                    {sssss
                        vendite_mese = Convert.ToDecimal(dtVendite.Rows.Find(tbWork.Columns[i].ColumnName)[1]);
                    }

                    Decimal residuo_mese = Convert.ToDecimal(tbWork.Rows[count][i + 1]);
                    Decimal valore = residuo_mese - vendite_mese;
                    if (valore < 0) valore = 0;
                    tbWork.Rows[count][i] = valore;

                    tbWork.Rows[count].EndEdit();
                }
            }

            DataTable tbWorkGiorni = new DataTable();
            tbWorkGiorni = new DataTable("dtFinale");
            tbWorkGiorni.Columns.Add("mese_rif", typeof(String));

            for (int i = 0; i < numeroMesi; i++)
            {
                DateTime dtMeseRif = dtInizioSchema.AddMonths(i);
                tbWorkGiorni.Columns.Add(String.Format("{0:yyyy-MM}", dtMeseRif), typeof(String));

            }

            foreach (DataColumn dc in tbWorkGiorni.Columns)
            {
                if (dc.ColumnName != "mese_rif")
                {
                    DataRow drWork = tbWorkGiorni.NewRow();
                    drWork["mese_rif"] = dc.ColumnName;
                    tbWorkGiorni.Rows.Add(drWork);
                }

            }
            int countSaldi = 13;
            for (int count = 0; count <= numeroMesi - 1; count++)
            {
                for (int i = 0; i <= count; i++)
                {
                    int giorni = 0;
                    tbWorkGiorni.Rows[count].BeginEdit();

                    Decimal vendite_mese = 0;
                    Decimal esposizione_mese = 0;


                    //if (dtVendite.Rows.Find(tbWorkGiorni.Columns[i + 1].ColumnName) != null)
                    //{
                    //vendite_mese = Convert.ToDecimal(dtVendite.Rows.Find(tbWorkGiorni.Columns[i + 1].ColumnName)[1]);

                    string data = tbWorkGiorni.Columns[i + 1].ColumnName.ToString() + "-01";
                    DateTime dataDa = DateTime.Parse(data);
                    DateTime dataA = dataDa.AddMonths(1);



                    using (DemoR2Entities context = new DemoR2Entities())
                    {
                        if (partitaIva.Equals(""))
                        {
                            vendite_mese = Convert.ToInt32(context.m04_vendite.Where(ve => ve.m04_dtdocvendita >= dataDa
                                                                                             && ve.m04_dtdocvendita < dataA
                                                                                             && ve.m04_codiceazienda.Equals(loggeduser.CodiceAzienda)
                                                                                            ).Sum(sum => sum.m04_importo));
                        }
                        else
                        {
                            vendite_mese = Convert.ToInt32(context.m04_vendite.Where(ve => ve.m04_dtdocvendita >= dataDa
                                                                                             && ve.m04_dtdocvendita < dataA
                                                                                             && ve.m04_codiceazienda.Equals(loggeduser.CodiceAzienda)
                                                                                             && ve.m04_partitaiva.Equals(partitaIva)).Sum(sum => sum.m04_importo));
                        }
                    }
                    //}

                    //if (dtEsposizione.Rows.Find(tbWorkGiorni.Columns[i + 1].ColumnName) != null)
                    //{
                    //esposizione_mese = Convert.ToDecimal(dtEsposizione.Rows.Find(tbWorkGiorni.Columns[i + 1].ColumnName)[1]);
                    string mese_rifInterno = tbWorkGiorni.Columns[i + 1].ColumnName;
                    int idAziendainteno = Convert.ToInt32(loggeduser.CodiceAzienda);
                    int appo = 0;
                    using (DemoR2Entities context = new DemoR2Entities())

                    {
                        if (partitaIva.Equals(""))
                        {
                            appo = Convert.ToInt32(context.s03_AggregazioneSaldoAziende.Where(sa => sa.Mese_rif.Equals(mese_rifInterno) && sa.IdAzienda == idAziendainteno)
                                                                                   .Select(sel => sel.Importo).FirstOrDefault());
                        }
                        else
                        {
                            appo = Convert.ToInt32(context.s03_AggregazioneSaldoAziende.Where(sa => sa.Mese_rif.Equals(mese_rifInterno)
                                                                                              && sa.IdAzienda == idAziendainteno
                                                                                              && sa.PartitaIva.Equals(partitaIva))
                                                                                       .Select(sel => sel.Importo).FirstOrDefault());
                        }
                        esposizione_mese = Convert.ToDecimal(appo);
                    }
                    //}

                    Decimal residuo_mese = Convert.ToDecimal(tbWork.Rows[count][i + 1]);
                    Decimal residuo_mese_succ = 0;

                    if ((i < numeroMesi - 1) && (tbWork.Rows[count][i + 2] != DBNull.Value))
                    {
                        residuo_mese_succ = Convert.ToDecimal(tbWork.Rows[count][i + 2]);
                    }

                    if (residuo_mese > 0)
                    {
                        giorni = getMonthDate(tbWorkGiorni.Columns[i + 1].ColumnName);
                    }
                    else
                    {
                        if (vendite_mese > 0)
                        {
                            giorni = Convert.ToInt32(residuo_mese_succ / vendite_mese * getMonthDate(tbWorkGiorni.Columns[i + 1].ColumnName));
                        }
                    }

                    if ((i == 0) && (residuo_mese > 0))
                    {
                        if (vendite_mese > 0)
                        {
                            giorni = giorni + Convert.ToInt32(residuo_mese / vendite_mese * getMonthDate(tbWorkGiorni.Columns[i + 1].ColumnName));
                        }
                    }




                    if ((esposizione_mese <= vendite_mese) && (i == count))
                    {
                        if (vendite_mese > 0)
                        {
                            giorni = giorni + Convert.ToInt32(esposizione_mese / vendite_mese * getMonthDate(tbWorkGiorni.Columns[i + 1].ColumnName));
                        }

                    }
                    tbWorkGiorni.Rows[count][i + 1] = giorni;

                }
            }

            try
            {
                for (int j = 0; j < tbWorkGiorni.Rows.Count; j++)
                {
                    int somma = 0;
                    for (int i = 0; i < tbWorkGiorni.Columns.Count; i++)
                    {
                        int valore = 0;
                        Int32.TryParse(tbWorkGiorni.Rows[j][i].ToString(), out valore);
                        somma += valore;

                    }
                    elementiGraficoMercato[j + 1].Giorni = somma <= 365 ? somma : 365;

                }
            }
            
            catch (Exception e) 
            {
                Log.Error(e.ToString());
            }

            /*CAMBIARE*/
            int skipInterval = 1;
            elementiGraficoMercato = elementiGraficoMercato.OrderBy(o => o.DataMensilita).Skip(skipInterval).ToList();

            return elementiGraficoMercato;
        }

        [Authorize]
        public List<ElementoGraficoMercato> GetTabellaNazione(string ragioneSociale = "", string partitaIva = "", int numeroMesi = 12)
        {
            List<ElementoGraficoMercato> elementiGraficoMercato = new List<ElementoGraficoMercato>();
            List<string> listappoggioPivaTotali = new List<string>();
            using (DemoR2Entities context = new DemoR2Entities())
            {
                listappoggioPivaTotali = context.s02_AggregazioneVendite.Where(wr => wr.IdCentro == loggeduser.IdMercato)
                                                                                     .Select(sel => sel.PartitaIva).ToList();
            }
           // if (partitaIva == "01600480303") return elementiGraficoMercato;

            DataSet datiMercato = DBHandler.getDatiNazione(ragioneSociale, partitaIva, WebConfigurationManager.ConnectionStrings["ConnectionStringR3"].ToString());

            datiMercato.Tables[0].TableName = "Importi";
            datiMercato.Tables[1].TableName = "Esposizione";

            DataTable dtEsposizione = datiMercato.Tables[1];
            DataTable dtVendite = datiMercato.Tables[0];


            datiMercato.Tables[0].PrimaryKey = new DataColumn[] { datiMercato.Tables[0].Columns["mese_rif"] };
            datiMercato.Tables[1].PrimaryKey = new DataColumn[] { datiMercato.Tables[1].Columns["mese_rif"] };
            /*CAMBIARE*/
            //int esposizionePrecedente = Convert.ToInt32(Math.Round(Convert.ToDecimal(datiMercato.Tables[1].Rows[0]["t03_esposizione_centro"])));
            DateTime dtInizio = new DateTime(DateTime.Now.AddMonths(-numeroMesi).Year, DateTime.Now.AddMonths(-numeroMesi).Month, 1);
            String primoMese = String.Format("{0:yyyy-MM}", new DateTime(DateTime.Now.AddMonths(-numeroMesi).Year, DateTime.Now.AddMonths(-numeroMesi).Month, 1));
            String mese_rif = String.Format("{0:yyyy-MM}", dtInizio);
            int venditeMesePrecedente = 0;

            // prendo la prima esposizione
            DataRow drEsposizione = datiMercato.Tables[1].Rows.Find(mese_rif);
            int esposizionePrecedente = 0;
            using (DemoR2Entities context = new DemoR2Entities())
            {
                if (partitaIva.Equals(""))
                {
                    esposizionePrecedente = Convert.ToInt32(context.s03_AggregazioneSaldoAziende.Where(sa => sa.Mese_rif.Equals(mese_rif))
                                                                                            .Sum(sum => sum.Importo));
                }
                else
                {
                    esposizionePrecedente = Convert.ToInt32(context.s03_AggregazioneSaldoAziende.Where(sa => sa.Mese_rif.Equals(mese_rif) && sa.PartitaIva.Equals(partitaIva))
                                                                                         .Sum(sum => sum.Importo));
                }
            }
            DateTime dtBuffer = dtInizio;
            DateTime dtFine = DateTime.Now;

            while (dtBuffer <= dtFine)
            {

                /*foreach (DataRow row in datiMercato.Tables[0].Rows)
                {
                 */
                string stringaData = String.Format("{0:yyyy-MM}", dtBuffer);
                //DataRow row = datiMercato.Tables[0].Rows.Find(stringaData);

                ElementoGraficoMercato elemento = new ElementoGraficoMercato();
                elemento.Mensilita = stringaData;
                elemento.DataMensilita = Convert.ToDateTime(String.Format("{0}-01", stringaData));
                elemento.Saldi = elemento.Vendite = 0;
                DateTime dataDA = Convert.ToDateTime(stringaData);
                DateTime dataA = dataDA.AddMonths(1);

                using (DemoR2Entities context = new DemoR2Entities())
                {
                    if (partitaIva.Equals(""))
                    {
                        elemento.Vendite = Convert.ToInt32(context.m04_vendite.Where(ve => ve.m04_dtdocvendita >= dataDA
                                                                                       && ve.m04_dtdocvendita < dataA
                                                                               //        && listappoggioPivaTotali.Contains(ve.m04_partitaiva)
                                                                                      ).Sum(sum => sum.m04_importo));
                        elemento.Saldi = Convert.ToInt32(context.s03_AggregazioneSaldoAziende.Where(ag => ag.Mese_rif.Equals(stringaData)
                                                                                                    ).Sum(sum => sum.Importo));
                    }
                    else
                    {
                        var autorizzato = 2;
                        if (loggeduser.IdRuolo != 1)
                        {
                            string codiceAzienda = loggeduser.CodiceAzienda;
                            autorizzato = context.m04_vendite.Where(a => a.m04_partitaiva.Equals(partitaIva) && a.m04_codiceazienda.Equals(codiceAzienda)).Count();
                        }

                        if (autorizzato > 0)
                        {
                            elemento.Vendite = Convert.ToInt32(context.m04_vendite.Where(ve => ve.m04_dtdocvendita >= dataDA
                                                                                                          && ve.m04_dtdocvendita < dataA
                                                                                                          && ve.m04_partitaiva.Equals(partitaIva)
                                                                                                         ).Sum(sum => sum.m04_importo));
                            elemento.Saldi = Convert.ToInt32(context.s03_AggregazioneSaldoAziende.Where(ag => ag.Mese_rif.Equals(stringaData)
                                                                                                   && ag.PartitaIva.Equals(partitaIva)).Sum(sum => sum.Importo));
                        }
                        else
                        {
                            foreach (string el in listappoggioPivaTotali)
                            {
                                if (el.Trim().Equals(partitaIva))
                                {
                                    elemento.Vendite = Convert.ToInt32(context.m04_vendite.Where(ve => ve.m04_dtdocvendita >= dataDA
                                                                                                        && ve.m04_dtdocvendita < dataA
                                                                                                        && ve.m04_partitaiva.Equals(partitaIva)
                                                                                                       ).Sum(sum => sum.m04_importo));
                                    elemento.Saldi = Convert.ToInt32(context.s03_AggregazioneSaldoAziende.Where(ag => ag.Mese_rif.Equals(stringaData)
                                                                                                       && ag.PartitaIva.Equals(partitaIva)).Sum(sum => sum.Importo));
                                    break;
                                }
                            }
                        }
                        elemento.RatingInt = calRationg(stringaData, partitaIva);
                    }
                }

                using (DemoR2Entities context = new DemoR2Entities())
                {
                    try
                    {
                        int totale = 0;
                        int vendite = elemento.Vendite;
                        int esposizione = elemento.Saldi;
                        DateTime dataMesePrecDa = dataDA;
                        DateTime dataMesePrecA = dataA;

                        for (int i = 1; i < 13; i++)
                        {
                            totale = esposizione - vendite;
                            if (totale > 0)
                            {
                                elemento.Giorni += System.DateTime.DaysInMonth(dataMesePrecDa.Year, dataMesePrecDa.Month);
                                esposizione = totale;
                                dataMesePrecDa = dataDA.AddMonths(-i);
                                dataMesePrecA = dataMesePrecDa.AddMonths(1);

                                if (partitaIva.Equals(""))
                                {
                                    vendite = Convert.ToInt32(context.m04_vendite.Where(ve => ve.m04_dtdocvendita >= dataMesePrecDa
                                                                                       && ve.m04_dtdocvendita < dataMesePrecA
                                                                                    //   && listappoggioPivaTotali.Contains(ve.m04_partitaiva)
                                                                                      ).Sum(sum => sum.m04_importo));
                                }
                                else
                                {
                                    vendite = Convert.ToInt32(context.m04_vendite.Where(ve => ve.m04_dtdocvendita >= dataMesePrecDa
                                                                                                          && ve.m04_dtdocvendita < dataMesePrecA
                                                                                                          && ve.m04_partitaiva.Equals(partitaIva)
                                                                                                         ).Sum(sum => sum.m04_importo));
                                }
                            }
                            else
                            {
                                int parzialeGiorni = System.DateTime.DaysInMonth(dataMesePrecDa.Year, dataMesePrecA.Month);
                                elemento.Giorni += esposizione / (vendite / parzialeGiorni);
                                break;
                            }
                        }
                    }
                    catch (DivideByZeroException e)
                    {
                        Log.Error(e.Message);
                    }

                }
                venditeMesePrecedente = elemento.Vendite;
                elemento.Incassi = esposizionePrecedente + elemento.Vendite - elemento.Saldi;
                elementiGraficoMercato.Add(elemento);
                esposizionePrecedente = elemento.Saldi;

                /*using (DemoR2Entities context = new DemoR2Entities())
                {
                    if (partitaIva.Equals(""))
                    {
                        try
                        {
                            int totaleGioni = Convert.ToInt32(context.s04_AggregazioneGiorniRitardo.Where(agg => agg.Mese_rif.Equals(stringaData)).Sum(sum => sum.giorniRitardo));
                            elemento.Giorni = totaleGioni / context.s04_AggregazioneGiorniRitardo.Where(agg => agg.Mese_rif.Equals(stringaData)).Count();
                        }
                        catch
                        {
                            elemento.Giorni = 0;
                        }
                    }
                    else
                    {
                        try
                        {
                            int totaleGioni = Convert.ToInt32(context.s04_AggregazioneGiorniRitardo.Where(agg => agg.Mese_rif.Equals(stringaData)
                                                                                     && agg.PartitaIva.Equals(partitaIva)).Sum(sum => sum.giorniRitardo));
                            elemento.Giorni = totaleGioni / context.s04_AggregazioneGiorniRitardo.Where(agg => agg.Mese_rif.Equals(stringaData) && agg.PartitaIva.Equals(partitaIva)).Count();
                        }
                        catch
                        {
                            elemento.Giorni = 0;
                        }
                    }

                }*/

                dtBuffer = dtBuffer.AddMonths(1);
            }


            /*CREO SCHEMA GIORNI
            DataTable tbWork = new DataTable();
            tbWork = new DataTable("dtFinale");
            tbWork.Columns.Add("mese_rif", typeof(String));
            /*1
            DateTime dtInizioSchema = dtInizio.AddMonths(1);
            for (int i = 0; i < numeroMesi; i++)
            {
                DateTime dtMeseRif = dtInizioSchema.AddMonths(i);
                tbWork.Columns.Add(String.Format("{0:yyyy-MM}", dtMeseRif), typeof(String));

            }

            DataView dvVendite = new DataView(datiMercato.Tables[0], "mese_rif<>'" + primoMese + "'", "mese_rif asc", DataViewRowState.CurrentRows);

            foreach (DataColumn dc in tbWork.Columns)
            {
                if (dc.ColumnName != "mese_rif")
                {
                    Decimal vendite = 0;
                    Decimal esposizione = 0;

                    drEsposizione = dtEsposizione.Rows.Find(dc.ColumnName);
                    DataRow drVendite = dtVendite.Rows.Find(dc.ColumnName);

                    if (drVendite != null)
                    {
                        vendite = Convert.ToDecimal(drVendite[1]);
                    }
                    if (drEsposizione != null)
                    {
                        esposizione = Convert.ToDecimal(drEsposizione[1]);
                    }

                    DataRow drWork = tbWork.NewRow();
                    drWork["mese_rif"] = dc.ColumnName;

                    Decimal valore = esposizione - vendite;

                    if (valore < 0) valore = 0;

                    drWork[dc.ColumnName] = valore;


                    tbWork.Rows.Add(drWork);
                }
            }

            for (int count = 1; count < numeroMesi; count++)
            {
                for (int i = count; i >= 1; i--)
                {
                    tbWork.Rows[count].BeginEdit();


                    Decimal vendite_mese = 0;
                    if (dtVendite.Rows.Find(tbWork.Columns[i].ColumnName) != null)
                    {
                        vendite_mese = Convert.ToDecimal(dtVendite.Rows.Find(tbWork.Columns[i].ColumnName)[1]);
                    }

                    Decimal residuo_mese = Convert.ToDecimal(tbWork.Rows[count][i + 1]);
                    Decimal valore = residuo_mese - vendite_mese;
                    if (valore < 0) valore = 0;
                    tbWork.Rows[count][i] = valore;

                    tbWork.Rows[count].EndEdit();
                }
            }

            DataTable tbWorkGiorni = new DataTable();
            tbWorkGiorni = new DataTable("dtFinale");
            tbWorkGiorni.Columns.Add("mese_rif", typeof(String));

            for (int i = 0; i < numeroMesi; i++)
            {
                DateTime dtMeseRif = dtInizioSchema.AddMonths(i);
                tbWorkGiorni.Columns.Add(String.Format("{0:yyyy-MM}", dtMeseRif), typeof(String));

            }

            foreach (DataColumn dc in tbWorkGiorni.Columns)
            {
                if (dc.ColumnName != "mese_rif")
                {
                    DataRow drWork = tbWorkGiorni.NewRow();
                    drWork["mese_rif"] = dc.ColumnName;
                    tbWorkGiorni.Rows.Add(drWork);
                }

            }

            for (int count = 0; count <= numeroMesi - 1; count++)
            {
                for (int i = 0; i <= count; i++)
                {
                    int giorni = 0;
                    tbWorkGiorni.Rows[count].BeginEdit();

                    Decimal vendite_mese = 0;
                    Decimal esposizione_mese = 0;


                    if (dtVendite.Rows.Find(tbWorkGiorni.Columns[i + 1].ColumnName) != null)
                    {
                        vendite_mese = Convert.ToDecimal(dtVendite.Rows.Find(tbWorkGiorni.Columns[i + 1].ColumnName)[1]);
                    }

                    if (dtEsposizione.Rows.Find(tbWorkGiorni.Columns[i + 1].ColumnName) != null)
                    {
                        esposizione_mese = Convert.ToDecimal(dtEsposizione.Rows.Find(tbWorkGiorni.Columns[i + 1].ColumnName)[1]);
                    }

                    Decimal residuo_mese = Convert.ToDecimal(tbWork.Rows[count][i + 1]);
                    Decimal residuo_mese_succ = 0;

                    if ((i < numeroMesi - 1) && (tbWork.Rows[count][i + 2] != DBNull.Value))
                    {
                        residuo_mese_succ = Convert.ToDecimal(tbWork.Rows[count][i + 2]);
                    }

                    if (residuo_mese > 0)
                    {
                        giorni = getMonthDate(tbWorkGiorni.Columns[i + 1].ColumnName);
                    }
                    else
                    {
                        if (vendite_mese > 0)
                        {
                            giorni = Convert.ToInt32(residuo_mese_succ / vendite_mese * getMonthDate(tbWorkGiorni.Columns[i + 1].ColumnName));
                        }
                    }

                    if ((i == 0) && (residuo_mese > 0))
                    {
                        if (vendite_mese > 0)
                        {
                            giorni = giorni + Convert.ToInt32(residuo_mese / vendite_mese * getMonthDate(tbWorkGiorni.Columns[i + 1].ColumnName));
                        }
                    }




                    if ((esposizione_mese <= vendite_mese) && (i == count))
                    {
                        if (vendite_mese > 0)
                        {
                            giorni = giorni + Convert.ToInt32(esposizione_mese / vendite_mese * getMonthDate(tbWorkGiorni.Columns[i + 1].ColumnName));
                        }

                    }


                    tbWorkGiorni.Rows[count][i + 1] = giorni;

                }
            }

            try
            {
                for (int j = 0; j < tbWorkGiorni.Rows.Count; j++)
                {
                    int somma = 0;
                    for (int i = 0; i < tbWorkGiorni.Columns.Count; i++)
                    {
                        int valore = 0;
                        Int32.TryParse(tbWorkGiorni.Rows[j][i].ToString(), out valore);
                        somma += valore;

                    }
                    elementiGraficoMercato[j + 1].Giorni = somma <= 365 ? somma : 365;

                }
            }
            catch { }

            /*CAMBIARE*/
            int skipInterval = 1;
            elementiGraficoMercato = elementiGraficoMercato.OrderBy(o => o.DataMensilita).Skip(skipInterval).ToList();



            return elementiGraficoMercato;


        }

        [Authorize]
        public List<ElementoGraficoMercato> GetTabellaAzienda(string ragioneSociale = "", string partitaIva = "", int numeroMesi = 12)
        {
            List<ElementoGraficoMercato> elementiGraficoMercato = new List<ElementoGraficoMercato>();

            //if (partitaIva == "01600480303") return elementiGraficoMercato;

            DataSet datiMercato = DBHandler.getDatiAzienda(ragioneSociale, partitaIva, loggeduser.CodiceAzienda, WebConfigurationManager.ConnectionStrings["ConnectionStringR3"].ToString());

            datiMercato.Tables[0].TableName = "Importi";
            datiMercato.Tables[1].TableName = "Esposizione";

            DataTable dtEsposizione = datiMercato.Tables[1];
            DataTable dtVendite = datiMercato.Tables[0];


            datiMercato.Tables[0].PrimaryKey = new DataColumn[] { datiMercato.Tables[0].Columns["mese_rif"] };
            datiMercato.Tables[1].PrimaryKey = new DataColumn[] { datiMercato.Tables[1].Columns["mese_rif"] };
            /*CAMBIARE*/
            //int esposizionePrecedente = Convert.ToInt32(Math.Round(Convert.ToDecimal(datiMercato.Tables[1].Rows[0]["t03_esposizione_centro"])));
            DateTime dtInizio = new DateTime(DateTime.Now.AddMonths(-numeroMesi).Year, DateTime.Now.AddMonths(-numeroMesi).Month, 1);
            String primoMese = String.Format("{0:yyyy-MM}", new DateTime(DateTime.Now.AddMonths(-numeroMesi).Year, DateTime.Now.AddMonths(-numeroMesi).Month, 1));
            String mese_rif = String.Format("{0:yyyy-MM}", dtInizio);

            // prendo la prima esposizione
            DataRow drEsposizione = datiMercato.Tables[1].Rows.Find(mese_rif);
            int esposizionePrecedente = 0;
            int venditeMesePrecedente = 0;
            int codiceAzienda = Convert.ToInt32(loggeduser.CodiceAzienda);
            using (DemoR2Entities context = new DemoR2Entities())
            {
                if (partitaIva.Equals(""))
                {
                    esposizionePrecedente = Convert.ToInt32(context.s03_AggregazioneSaldoAziende.Where(sa => sa.Mese_rif.Equals(mese_rif)
                      && sa.IdCentro == loggeduser.IdMercato
                      && sa.IdAzienda == codiceAzienda).Sum(sum => sum.Importo));
                }
                else
                {
                    esposizionePrecedente = Convert.ToInt32(context.s03_AggregazioneSaldoAziende.Where(sa => sa.Mese_rif.Equals(mese_rif)
                      && sa.IdCentro == loggeduser.IdMercato
                      && sa.IdAzienda == codiceAzienda && sa.PartitaIva.Equals(partitaIva)).Sum(sum => sum.Importo));
                }

            }
            using (DemoR2Entities context = new DemoR2Entities())
            {
                DateTime dataFine = dtInizio.AddMonths(1);
                if (partitaIva.Equals(""))
                {
                    venditeMesePrecedente = Convert.ToInt32(context.m04_vendite.Where(ve => ve.m04_dtdocvendita >= dtInizio
                                                                             && ve.m04_dtdocvendita < dataFine
                                                                             && ve.m04_codiceazienda.Equals(loggeduser.CodiceAzienda)).Sum(sum => sum.m04_importo));
                }
                else
                {
                    venditeMesePrecedente = Convert.ToInt32(context.m04_vendite.Where(ve => ve.m04_dtdocvendita >= dtInizio
                                                                            && ve.m04_dtdocvendita < dataFine
                                                                            && ve.m04_codiceazienda.Equals(loggeduser.CodiceAzienda)
                                                                            && ve.m04_partitaiva.Equals(partitaIva)).Sum(sum => sum.m04_importo));
                }
            }


            DateTime dtBuffer = dtInizio;
            DateTime dtFine = DateTime.Now;

            while (dtBuffer <= dtFine)
            {
               
                string stringaData = String.Format("{0:yyyy-MM}", dtBuffer);
                //DataRow row = datiMercato.Tables[0].Rows.Find(stringaData);

                ElementoGraficoMercato elemento = new ElementoGraficoMercato();
                elemento.Mensilita = stringaData;
                elemento.DataMensilita = Convert.ToDateTime(String.Format("{0}-01", stringaData));
                elemento.Saldi = elemento.Vendite = 0;
                DateTime dataDA = Convert.ToDateTime(stringaData);
                DateTime dataA = dataDA.AddMonths(1);
                using (DemoR2Entities context = new DemoR2Entities())
                {
                    if (partitaIva.Equals(""))
                    {
                        elemento.Vendite = Convert.ToInt32(context.m04_vendite.Where(ve => ve.m04_dtdocvendita >= dataDA
                                                                                 && ve.m04_dtdocvendita < dataA
                                                                                 && ve.m04_codiceazienda.Equals(loggeduser.CodiceAzienda)).Sum(sum => sum.m04_importo));
                        elemento.Saldi = Convert.ToInt32(context.s03_AggregazioneSaldoAziende.Where(ag => ag.IdCentro == loggeduser.IdMercato
                                                            && ag.Mese_rif.Equals(stringaData)
                                                            && ag.IdAzienda == codiceAzienda).Sum(sum => sum.Importo));
                    }
                    else
                    {
                        elemento.Vendite = Convert.ToInt32(context.m04_vendite.Where(ve => ve.m04_dtdocvendita >= dataDA
                                                                                && ve.m04_dtdocvendita < dataA
                                                                                && ve.m04_codiceazienda.Equals(loggeduser.CodiceAzienda)
                                                                                && ve.m04_partitaiva.Equals(partitaIva)).Sum(sum => sum.m04_importo));
                        elemento.Saldi = Convert.ToInt32(context.s03_AggregazioneSaldoAziende.Where(ag => ag.IdCentro == loggeduser.IdMercato
                                                            && ag.Mese_rif.Equals(stringaData)
                                                            && ag.IdAzienda == codiceAzienda
                                                            && ag.PartitaIva.Equals(partitaIva)).Sum(sum => sum.Importo));

                        elemento.RatingInt = calRationg(stringaData, partitaIva);
                    }
                }
               
                using (DemoR2Entities context = new DemoR2Entities()) 
                {
                    try
                    {
                        int totale = 0;
                        int vendite = elemento.Vendite;
                        int esposizione = elemento.Saldi;
                        DateTime dataMesePrecDa = dataDA;
                        DateTime dataMesePrecA = dataA;

                        for (int i = 1; i < 13; i++)    
                        {
                            totale = esposizione - vendite;
                            if (totale > 0)
                            {
                                elemento.Giorni += System.DateTime.DaysInMonth(dataMesePrecDa.Year, dataMesePrecA.Month);
                                esposizione = totale;
                                dataMesePrecDa = dataDA.AddMonths(-i);
                                dataMesePrecA = dataMesePrecDa.AddMonths(1);

                                if (partitaIva.Equals(""))
                                {
                                    vendite = Convert.ToInt32(context.m04_vendite.Where(ve => ve.m04_dtdocvendita >= dataMesePrecDa
                                                                                       && ve.m04_dtdocvendita < dataMesePrecA
                                                                                       && ve.m04_codiceazienda.Equals(loggeduser.CodiceAzienda)).Sum(sum => sum.m04_importo));
                                }
                                else
                                {
                                    vendite = Convert.ToInt32(context.m04_vendite.Where(ve => ve.m04_dtdocvendita >= dataMesePrecDa
                                                                                && ve.m04_dtdocvendita < dataMesePrecA
                                                                                && ve.m04_codiceazienda.Equals(loggeduser.CodiceAzienda)
                                                                                && ve.m04_partitaiva.Equals(partitaIva)).Sum(sum => sum.m04_importo));
                                }
                            }
                            else
                            {
                                int parzialeGiorni = System.DateTime.DaysInMonth(dataMesePrecDa.Year, dataMesePrecA.Month);
                                elemento.Giorni += esposizione / (vendite / parzialeGiorni);
                                break;
                            }
                        }
                    }
                    catch (DivideByZeroException e)
                    {
                        Log.Error(e.Message);
                    }
                }
                
                venditeMesePrecedente = elemento.Vendite;
                elemento.Incassi = esposizionePrecedente + elemento.Vendite - elemento.Saldi;
                elementiGraficoMercato.Add(elemento);
                esposizionePrecedente = elemento.Saldi;

                dtBuffer = dtBuffer.AddMonths(1);
            }

            //CREAZINE GIORNI 



            /*CREO SCHEMA GIORNI
            DataTable tbWork = new DataTable();
            tbWork = new DataTable("dtFinale");
            tbWork.Columns.Add("mese_rif", typeof(String));
            /*1
            DateTime dtInizioSchema = dtInizio.AddMonths(1);
            for (int i = 0; i < numeroMesi; i++)
            {
                DateTime dtMeseRif = dtInizioSchema.AddMonths(i);
                tbWork.Columns.Add(String.Format("{0:yyyy-MM}", dtMeseRif), typeof(String));

            }

            DataView dvVendite = new DataView(datiMercato.Tables[0], "mese_rif<>'" + primoMese + "'", "mese_rif asc", DataViewRowState.CurrentRows);

            foreach (DataColumn dc in tbWork.Columns)
            {
                if (dc.ColumnName != "mese_rif")
                {
                    Decimal vendite = 0;
                    Decimal esposizione = 0;
                    using (DemoR2Entities context = new DemoR2Entities())
                    {
                        //drEsposizione = dtEsposizione.Rows.Find(dc.ColumnName);
                        //drEsposizione = context.s03_AggregazioneSaldoAziende.Where(ag => ag.Mese_rif.Equals(dc.ColumnName)).Select(sel=>sel.Importo).FirstOrDefault();
                        //DataRow drVendite = dtVendite.Rows.Find(dc.ColumnName);
                        //DataRow drVendite = 
                    }
                        
                    

                    //if (drVendite != null)
                    //{
                        //vendite = Convert.ToDecimal(drVendite[1DateTime dataDA = Convert.ToDateTime(stringaData);

                        DateTime dataDA = Convert.ToDateTime(dc.ColumnName+"-01");
                        DateTime dataA = dataDA.AddMonths(1);

                        using (DemoR2Entities context = new DemoR2Entities())
                        {
                            if (partitaIva.Equals(""))
                            {
                               vendite = Convert.ToInt32(context.m04_vendite.Where(ve => ve.m04_dtdocvendita >= dataDA
                                                                                         && ve.m04_dtdocvendita < dataA
                                                                                         && ve.m04_codiceazienda.Equals(loggeduser.CodiceAzienda)).Sum(sum => sum.m04_importo));
                            }
                            else
                            {
                                vendite = Convert.ToInt32(context.m04_vendite.Where(ve => ve.m04_dtdocvendita >= dataDA
                                                                                        && ve.m04_dtdocvendita < dataA
                                                                                        && ve.m04_codiceazienda.Equals(loggeduser.CodiceAzienda)
                                                                                        && ve.m04_partitaiva.Equals(partitaIva)).Sum(sum => sum.m04_importo));
                            }
                        }
                    //if (drEsposizione != null)
                    //{
                        //esposizione = Convert.ToDecimal(drEsposizione[1]);
                        using (DemoR2Entities context = new DemoR2Entities())
                        {
                            int appo = Convert.ToInt32(context.s03_AggregazioneSaldoAziende.Where(sa => sa.Mese_rif.Equals(dc.ColumnName)).Select(sel => sel.Importo).FirstOrDefault());
                            esposizione = Convert.ToDecimal(appo);
                        }
                    //}

                    DataRow drWork = tbWork.NewRow();
                    drWork["mese_rif"] = dc.ColumnName;

                    Decimal valore = esposizione - vendite;

                    if (valore < 0) valore = 0;

                    drWork[dc.ColumnName] = valore;


                    tbWork.Rows.Add(drWork);
                }
            }

            for (int count = 1; count < numeroMesi; count++)
            {
                
                for (int i = count; i >= 1; i--)
                {
                    string mese_Rif_interno = tbWork.Columns[i].ColumnName;

                    tbWork.Rows[count].BeginEdit();


                    Decimal vendite_mese = 0;

                    DateTime dataDA = Convert.ToDateTime(mese_Rif_interno + "-01");
                    DateTime dataA = dataDA.AddMonths(1);

                    using (DemoR2Entities context = new DemoR2Entities())
                    {
                        if (partitaIva.Equals(""))
                        {
                            vendite_mese = Convert.ToInt32(context.m04_vendite.Where(ve => ve.m04_dtdocvendita >= dataDA
                                                                                      && ve.m04_dtdocvendita < dataA
                                                                                      && ve.m04_codiceazienda.Equals(loggeduser.CodiceAzienda)).Sum(sum => sum.m04_importo));
                        }
                        else
                        {
                            vendite_mese = Convert.ToInt32(context.m04_vendite.Where(ve => ve.m04_dtdocvendita >= dataDA
                                                                                    && ve.m04_dtdocvendita < dataA
                                                                                    && ve.m04_codiceazienda.Equals(loggeduser.CodiceAzienda)
                                                                                    && ve.m04_partitaiva.Equals(partitaIva)).Sum(sum => sum.m04_importo));
                        }
                    }

                    /*if (dtVendite.Rows.Find(tbWork.Columns[i].ColumnName) != null)
                    {sssss
                        vendite_mese = Convert.ToDecimal(dtVendite.Rows.Find(tbWork.Columns[i].ColumnName)[1]);
                    }

                    Decimal residuo_mese = Convert.ToDecimal(tbWork.Rows[count][i + 1]);
                    Decimal valore = residuo_mese - vendite_mese;
                    if (valore < 0) valore = 0;
                    tbWork.Rows[count][i] = valore;

                    tbWork.Rows[count].EndEdit();
                }
            }

            DataTable tbWorkGiorni = new DataTable();
            tbWorkGiorni = new DataTable("dtFinale");
            tbWorkGiorni.Columns.Add("mese_rif", typeof(String));

            for (int i = 0; i < numeroMesi; i++)
            {
                DateTime dtMeseRif = dtInizioSchema.AddMonths(i);
                tbWorkGiorni.Columns.Add(String.Format("{0:yyyy-MM}", dtMeseRif), typeof(String));

            }

            foreach (DataColumn dc in tbWorkGiorni.Columns)
            {
                if (dc.ColumnName != "mese_rif")
                {
                    DataRow drWork = tbWorkGiorni.NewRow();
                    drWork["mese_rif"] = dc.ColumnName;
                    tbWorkGiorni.Rows.Add(drWork);
                }

            }
            int countSaldi = 13;
            for (int count = 0; count <= numeroMesi - 1; count++)
            {
                for (int i = 0; i <= count; i++)
                {
                    int giorni = 0;
                    tbWorkGiorni.Rows[count].BeginEdit();

                    Decimal vendite_mese = 0;
                    Decimal esposizione_mese = 0;


                    //if (dtVendite.Rows.Find(tbWorkGiorni.Columns[i + 1].ColumnName) != null)
                    //{
                        //vendite_mese = Convert.ToDecimal(dtVendite.Rows.Find(tbWorkGiorni.Columns[i + 1].ColumnName)[1]);
                       
                        string data = tbWorkGiorni.Columns[i + 1].ColumnName.ToString() + "-01";
                        DateTime dataDa = DateTime.Parse(data);
                        DateTime dataA = dataDa.AddMonths(1);
                    


                        using (DemoR2Entities context = new DemoR2Entities()) {
                            if (partitaIva.Equals(""))
                            {
                            vendite_mese= Convert.ToInt32(context.m04_vendite.Where(ve => ve.m04_dtdocvendita >= dataDa
                                                                                            && ve.m04_dtdocvendita < dataA
                                                                                            && ve.m04_codiceazienda.Equals(loggeduser.CodiceAzienda)
                                                                                            ).Sum(sum => sum.m04_importo));
                            }
                            else
                            {
                            vendite_mese= Convert.ToInt32(context.m04_vendite.Where(ve => ve.m04_dtdocvendita >= dataDa
                                                                                            && ve.m04_dtdocvendita < dataA
                                                                                            && ve.m04_codiceazienda.Equals(loggeduser.CodiceAzienda)
                                                                                            && ve.m04_partitaiva.Equals(partitaIva)).Sum(sum => sum.m04_importo));
                            }
                        }
                    //}

                    //if (dtEsposizione.Rows.Find(tbWorkGiorni.Columns[i + 1].ColumnName) != null)
                    //{
                    //esposizione_mese = Convert.ToDecimal(dtEsposizione.Rows.Find(tbWorkGiorni.Columns[i + 1].ColumnName)[1]);
                    string mese_rifInterno = tbWorkGiorni.Columns[i + 1].ColumnName;
                    int idAziendainteno = Convert.ToInt32(loggeduser.CodiceAzienda);
                    int appo = 0;
                    using (DemoR2Entities context = new DemoR2Entities())
                      
                        {
                            if (partitaIva.Equals(""))
                            {
                                appo = Convert.ToInt32(context.s03_AggregazioneSaldoAziende.Where(sa => sa.Mese_rif.Equals(mese_rifInterno) && sa.IdAzienda == idAziendainteno)
                                                                                       .Select(sel => sel.Importo).FirstOrDefault());
                            }
                            else
                            {
                                appo = Convert.ToInt32(context.s03_AggregazioneSaldoAziende.Where(sa => sa.Mese_rif.Equals(mese_rifInterno) 
                                                                                                  && sa.IdAzienda == idAziendainteno 
                                                                                                  && sa.PartitaIva.Equals(partitaIva))
                                                                                           .Select(sel => sel.Importo).FirstOrDefault());
                            }
                            esposizione_mese = Convert.ToDecimal(appo);
                        }
                    //}

                    Decimal residuo_mese = Convert.ToDecimal(tbWork.Rows[count][i + 1]);
                    Decimal residuo_mese_succ = 0;

                    if ((i < numeroMesi - 1) && (tbWork.Rows[count][i + 2] != DBNull.Value))
                    {
                        residuo_mese_succ = Convert.ToDecimal(tbWork.Rows[count][i + 2]);
                    }

                    if (residuo_mese > 0)
                    {
                        giorni = getMonthDate(tbWorkGiorni.Columns[i + 1].ColumnName);
                    }
                    else
                    {
                        if (vendite_mese > 0)
                        {
                            giorni = Convert.ToInt32(residuo_mese_succ / vendite_mese * getMonthDate(tbWorkGiorni.Columns[i + 1].ColumnName));
                        }
                    }

                    if ((i == 0) && (residuo_mese > 0))
                    {
                        if (vendite_mese > 0)
                        {
                            giorni = giorni + Convert.ToInt32(residuo_mese / vendite_mese * getMonthDate(tbWorkGiorni.Columns[i + 1].ColumnName));
                        }
                    }




                    if ((esposizione_mese <= vendite_mese) && (i == count))
                    {
                        if (vendite_mese > 0)
                        {
                            giorni = giorni + Convert.ToInt32(esposizione_mese / vendite_mese * getMonthDate(tbWorkGiorni.Columns[i + 1].ColumnName));
                        }

                    }
                    tbWorkGiorni.Rows[count][i + 1] = giorni;

                }
            }

            try
            {
                for (int j = 0; j < tbWorkGiorni.Rows.Count; j++)
                {
                    int somma = 0;
                    for (int i = 0; i < tbWorkGiorni.Columns.Count; i++)
                    {
                        int valore = 0;
                        Int32.TryParse(tbWorkGiorni.Rows[j][i].ToString(), out valore);
                        somma += valore;

                    }
                    elementiGraficoMercato[j + 1].Giorni = somma <= 365 ? somma : 365;

                }
            }
            catch { }*/

            /*CAMBIARE*/
            int skipInterval = 1;
            elementiGraficoMercato = elementiGraficoMercato.OrderBy(o => o.DataMensilita).Skip(skipInterval).ToList();

            return elementiGraficoMercato;
        }

        private int calRationg(string stringaData, string partitaIva)
        {
            int ratingInt = 101;
            DataTable dt = new DataTable();
            String sSql = String.Empty;
            try
            {
                using (SqlConnection conn = DBHandler.OpenSqlConn())
                {
                    //String partitaIvazz = "02447440617";//partitaIva
                    sSql = "select m02_stato_validazione_cerved_" + stringaData.Substring(5, 2)
                            + " as rating from [OsservaR3.0].dbo.m02_anagrafica" +
                            " where m02_partitaiva='" + partitaIva.Trim() + "'";

                    SqlCommand cmd = DBHandler.GetSqlCommand(sSql, conn);
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandTimeout = 0;
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                    DBHandler.CloseConn(conn);

                    foreach (DataRow rating in dt.Rows)
                    {
                        if (rating["rating"] != null)
                        {
                            string appo = rating["rating"].ToString();
                            if (appo.Equals(""))
                            {
                                return ratingInt = 101;
                            }
                            else
                            {
                                return ratingInt = Convert.ToInt32(appo);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                return ratingInt;
            }
            return ratingInt;
        }

        [Authorize]
        public JsonResult GetMensilitaTabella()
        {
            List<string> returnValue = new List<string>();

            returnValue.Add("");

            DateTime dataBuffer = DateTime.Now.AddMonths(-17);

            while (dataBuffer < DateTime.Now)
            {
                returnValue.Add(String.Format("{0:yyyy}-{0:MM}", dataBuffer));
                dataBuffer = dataBuffer.AddMonths(1);
            }

            returnValue.Add(String.Format("ACTUAL {0:yyyy}-{0:MM}", dataBuffer));

            return Json(returnValue, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult GetReportMercato(string ragioneSociale = "", string partitaIva = "", int idMercato = 0)
        {
            List<string> listaAziendeCentro = GetListAziendeMercato(loggeduser.IdMercato);
            List<string> listappoggioPivaTotali = new List<string>();
            using (DemoR2Entities context = new DemoR2Entities())
            {
                listappoggioPivaTotali = context.s02_AggregazioneVendite.Where(wr => wr.IdCentro == loggeduser.IdMercato)
                                                                                     .Select(sel => sel.PartitaIva).ToList();
            }
            string idMercatoRicerca = idMercato != 0 ? idMercato.ToString() : loggeduser.IdMercato.ToString();
            List<ElementoGraficoMercato> elementiGraficoMercato = new List<ElementoGraficoMercato>();
   //         if (partitaIva == "01600480303") return Json(elementiGraficoMercato, JsonRequestBehavior.AllowGet);
            DataSet datiMercato = DBHandler.GetDatiCentro(idMercatoRicerca, ragioneSociale, partitaIva, WebConfigurationManager.ConnectionStrings["ConnectionStringR3"].ToString());

            datiMercato.Tables[0].TableName = "Importi";
            datiMercato.Tables[1].TableName = "Esposizione";

            int numeroMesi = Convert.ToInt32(WebConfigurationManager.AppSettings["NumeroMesiAndamento"]);


            datiMercato.Tables[0].PrimaryKey = new DataColumn[] { datiMercato.Tables[0].Columns["mese_rif"] };
            datiMercato.Tables[1].PrimaryKey = new DataColumn[] { datiMercato.Tables[1].Columns["mese_rif"] };
            /*CAMBIARE*/
            //int esposizionePrecedente = Convert.ToInt32(Math.Round(Convert.ToDecimal(datiMercato.Tables[1].Rows[0]["t03_esposizione_centro"])));
            DateTime dtInizio = new DateTime(DateTime.Now.AddMonths(-numeroMesi).Year, DateTime.Now.AddMonths(-numeroMesi).Month, 1);
            String mese_rif = String.Format("{0:yyyy-MM}", dtInizio);

            // prendo la prima esposizione
            DataRow drEsposizione = datiMercato.Tables[1].Rows.Find(mese_rif);
            int esposizionePrecedente = 0;
            using (DemoR2Entities context = new DemoR2Entities())
            {
                if (partitaIva.Equals(""))
                {
                    esposizionePrecedente = Convert.ToInt32(context.s03_AggregazioneSaldoAziende.Where(sa => sa.Mese_rif.Equals(mese_rif)
                      && sa.IdCentro == loggeduser.IdMercato).Sum(sum => sum.Importo));
                }
                else
                {
                    esposizionePrecedente = Convert.ToInt32(context.s03_AggregazioneSaldoAziende.Where(sa => sa.Mese_rif.Equals(mese_rif)
                    && sa.IdCentro == loggeduser.IdMercato && sa.PartitaIva.Equals(partitaIva)).Sum(sum => sum.Importo));
                }
            }
            DateTime dtBuffer = dtInizio;
            DateTime dtFine = DateTime.Now;

            while (dtBuffer <= dtFine)
            {

                string stringaData = String.Format("{0:yyyy-MM}", dtBuffer);
                //DataRow row = datiMercato.Tables[0].Rows.Find(stringaData);

                ElementoGraficoMercato elemento = new ElementoGraficoMercato();
                elemento.Mensilita = stringaData;
                elemento.DataMensilita = Convert.ToDateTime(String.Format("{0}-01", stringaData));
                elemento.Saldi = elemento.Vendite = 0;
                DateTime dataDA = Convert.ToDateTime(stringaData);
                DateTime dataA = dataDA.AddMonths(1);

                using (DemoR2Entities context = new DemoR2Entities())
                {
                    if (partitaIva.Equals(""))
                    {
                        elemento.Vendite = Convert.ToInt32(context.m04_vendite.Where(ve => ve.m04_dtdocvendita >= dataDA
                                                                                       && ve.m04_dtdocvendita < dataA
                                                                                       && listaAziendeCentro.Contains(ve.m04_codiceazienda)
                                                                                       && listappoggioPivaTotali.Contains(ve.m04_partitaiva)
                                                                                      ).Sum(sum => sum.m04_importo));
                        elemento.Saldi = Convert.ToInt32(context.s03_AggregazioneSaldoAziende.Where(ag => ag.IdCentro == loggeduser.IdMercato
                                                                                                    && ag.Mese_rif.Equals(stringaData)
                                                                                                    ).Sum(sum => sum.Importo));
                    }
                    else
                    {
                        var autorizzato = 2;
                        if (loggeduser.IdRuolo != 1)
                        {
                            string codiceAzienda = loggeduser.CodiceAzienda;
                            autorizzato = context.m04_vendite.Where(a => a.m04_partitaiva.Equals(partitaIva) && a.m04_codiceazienda.Equals(codiceAzienda)).Count();
                        }

                        if (autorizzato > 0)
                        {
                            elemento.Vendite = Convert.ToInt32(context.m04_vendite.Where(ve => ve.m04_dtdocvendita >= dataDA
                                                                                                          && ve.m04_dtdocvendita < dataA
                                                                                                          && listaAziendeCentro.Contains(ve.m04_codiceazienda)
                                                                                                          && ve.m04_partitaiva.Equals(partitaIva)
                                                                                                         ).Sum(sum => sum.m04_importo));
                            elemento.Saldi = Convert.ToInt32(context.s03_AggregazioneSaldoAziende.Where(ag => ag.IdCentro == loggeduser.IdMercato
                                                                                                   && ag.Mese_rif.Equals(stringaData)
                                                                                                   && ag.PartitaIva.Equals(partitaIva)).Sum(sum => sum.Importo));
                        }
                        else
                        {
                            foreach (string el in listappoggioPivaTotali)
                            {
                                if (el.Trim().Equals(partitaIva))
                                {
                                    elemento.Vendite = Convert.ToInt32(context.m04_vendite.Where(ve => ve.m04_dtdocvendita >= dataDA
                                                                                                        && ve.m04_dtdocvendita < dataA
                                                                                                        && listaAziendeCentro.Contains(ve.m04_codiceazienda)
                                                                                                        && ve.m04_partitaiva.Equals(partitaIva)
                                                                                                       ).Sum(sum => sum.m04_importo));
                                    elemento.Saldi = Convert.ToInt32(context.s03_AggregazioneSaldoAziende.Where(ag => ag.IdCentro == loggeduser.IdMercato
                                                                                                       && ag.Mese_rif.Equals(stringaData)
                                                                                                       && ag.PartitaIva.Equals(partitaIva)).Sum(sum => sum.Importo));
                                    break;
                                }
                            }
                        }
                    }
                }
                elemento.Giorni = 0;
                elemento.Incassi = esposizionePrecedente + elemento.Vendite - elemento.Saldi;
                elementiGraficoMercato.Add(elemento);
                esposizionePrecedente = elemento.Saldi;

                dtBuffer = dtBuffer.AddMonths(1);
            }

            int skipInterval = 1;
            elementiGraficoMercato = elementiGraficoMercato.OrderBy(o => o.DataMensilita).Skip(skipInterval).Take(numeroMesi - 1).ToList();

            return Json(elementiGraficoMercato, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult GetReportNazione(string ragioneSociale = "", string partitaIva = "")
        {

            List<ElementoGraficoMercato> elementiGraficoMercato = new List<ElementoGraficoMercato>();
      //      if (partitaIva == "01600480303") return Json(elementiGraficoMercato, JsonRequestBehavior.AllowGet);
            DataSet datiMercato = DBHandler.getDatiNazione(ragioneSociale, partitaIva, WebConfigurationManager.ConnectionStrings["ConnectionStringR3"].ToString());
            List<string> listappoggioPivaTotali = new List<string>();
            using (DemoR2Entities context = new DemoR2Entities())
            {
                listappoggioPivaTotali = context.s02_AggregazioneVendite.Where(wr => wr.IdCentro == loggeduser.IdMercato)
                                                                                     .Select(sel => sel.PartitaIva).ToList();
            }
            datiMercato.Tables[0].TableName = "Importi";
            datiMercato.Tables[1].TableName = "Esposizione";

            int numeroMesi = Convert.ToInt32(WebConfigurationManager.AppSettings["NumeroMesiAndamento"]);

            datiMercato.Tables[0].PrimaryKey = new DataColumn[] { datiMercato.Tables[0].Columns["mese_rif"] };
            datiMercato.Tables[1].PrimaryKey = new DataColumn[] { datiMercato.Tables[1].Columns["mese_rif"] };
            /*CAMBIARE*/
            //int esposizionePrecedente = Convert.ToInt32(Math.Round(Convert.ToDecimal(datiMercato.Tables[1].Rows[0]["t03_esposizione_centro"])));
            DateTime dtInizio = new DateTime(DateTime.Now.AddMonths(-numeroMesi).Year, DateTime.Now.AddMonths(-numeroMesi).Month, 1);
            String mese_rif = String.Format("{0:yyyy-MM}", dtInizio);

            // prendo la prima esposizione
            DataRow drEsposizione = datiMercato.Tables[1].Rows.Find(mese_rif);
            int esposizionePrecedente = 0;
            using (DemoR2Entities context = new DemoR2Entities())
            {
                if (partitaIva.Equals(""))
                {
                    esposizionePrecedente = Convert.ToInt32(context.s03_AggregazioneSaldoAziende.Where(sa => sa.Mese_rif.Equals(mese_rif))
                                                                                            .Sum(sum => sum.Importo));
                }
                else
                {
                    esposizionePrecedente = Convert.ToInt32(context.s03_AggregazioneSaldoAziende.Where(sa => sa.Mese_rif.Equals(mese_rif) && sa.PartitaIva.Equals(partitaIva))
                                                                                         .Sum(sum => sum.Importo));
                }
            }
            DateTime dtBuffer = dtInizio;
            DateTime dtFine = DateTime.Now;

            while (dtBuffer <= dtFine)
            {

                string stringaData = String.Format("{0:yyyy-MM}", dtBuffer);
                //DataRow row = datiMercato.Tables[0].Rows.Find(stringaData);

                ElementoGraficoMercato elemento = new ElementoGraficoMercato();
                elemento.Mensilita = stringaData;
                elemento.DataMensilita = Convert.ToDateTime(String.Format("{0}-01", stringaData));
                elemento.Saldi = elemento.Vendite = 0;
                DateTime dataDA = Convert.ToDateTime(stringaData);
                DateTime dataA = dataDA.AddMonths(1);

                using (DemoR2Entities context = new DemoR2Entities())
                {
                    if (partitaIva.Equals(""))
                    {
                        elemento.Vendite = Convert.ToInt32(context.m04_vendite.Where(ve => ve.m04_dtdocvendita >= dataDA
                                                                                       && ve.m04_dtdocvendita < dataA
                        //                                                               && listappoggioPivaTotali.Contains(ve.m04_partitaiva)
                                                                                      ).Sum(sum => sum.m04_importo));
                        elemento.Saldi = Convert.ToInt32(context.s03_AggregazioneSaldoAziende.Where(ag => ag.Mese_rif.Equals(stringaData)
                                                                                                    ).Sum(sum => sum.Importo));
                    }
                    else
                    {
                        var autorizzato = 2;
                        if (loggeduser.IdRuolo != 1)
                        {
                            string codiceAzienda = loggeduser.CodiceAzienda;
                            autorizzato = context.m04_vendite.Where(a => a.m04_partitaiva.Equals(partitaIva) && a.m04_codiceazienda.Equals(codiceAzienda)).Count();
                        }

                        if (autorizzato > 0)
                        {
                            elemento.Vendite = Convert.ToInt32(context.m04_vendite.Where(ve => ve.m04_dtdocvendita >= dataDA
                                                                                                          && ve.m04_dtdocvendita < dataA
                                                                                                          && ve.m04_partitaiva.Equals(partitaIva)
                                                                                                         ).Sum(sum => sum.m04_importo));
                            elemento.Saldi = Convert.ToInt32(context.s03_AggregazioneSaldoAziende.Where(ag => ag.Mese_rif.Equals(stringaData)
                                                                                                   && ag.PartitaIva.Equals(partitaIva)).Sum(sum => sum.Importo));
                        }
                        else
                        {
                            foreach (string el in listappoggioPivaTotali)
                            {
                                if (el.Trim().Equals(partitaIva))
                                {
                                    elemento.Vendite = Convert.ToInt32(context.m04_vendite.Where(ve => ve.m04_dtdocvendita >= dataDA
                                                                                                        && ve.m04_dtdocvendita < dataA
                                                                                                        && ve.m04_partitaiva.Equals(partitaIva)
                                                                                                       ).Sum(sum => sum.m04_importo));
                                    elemento.Saldi = Convert.ToInt32(context.s03_AggregazioneSaldoAziende.Where(ag => ag.Mese_rif.Equals(stringaData)
                                                                                                       && ag.PartitaIva.Equals(partitaIva)).Sum(sum => sum.Importo));
                                    break;
                                }
                            }
                        }
                    }
                }
                elemento.Giorni = 0;
                elemento.Incassi = esposizionePrecedente + elemento.Vendite - elemento.Saldi;
                elementiGraficoMercato.Add(elemento);
                esposizionePrecedente = elemento.Saldi;

                dtBuffer = dtBuffer.AddMonths(1);
            }
            int skipInterval = 1;
            elementiGraficoMercato = elementiGraficoMercato.OrderBy(o => o.DataMensilita).Skip(skipInterval).Take(numeroMesi - 1).ToList();

            return Json(elementiGraficoMercato, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult GetReportAzienda(string ragioneSociale = "", string partitaIva = "")
        {


            List<ElementoGraficoMercato> elementiGraficoMercato = new List<ElementoGraficoMercato>();
          //  if (partitaIva == "01600480303") return Json(elementiGraficoMercato, JsonRequestBehavior.AllowGet);
            DataSet datiMercato = DBHandler.getDatiAzienda(ragioneSociale, partitaIva, loggeduser.CodiceAzienda, WebConfigurationManager.ConnectionStrings["ConnectionStringR3"].ToString());

            datiMercato.Tables[0].TableName = "Importi";
            datiMercato.Tables[1].TableName = "Esposizione";

            int numeroMesi = Convert.ToInt32(WebConfigurationManager.AppSettings["NumeroMesiAndamento"]);

            datiMercato.Tables[0].PrimaryKey = new DataColumn[] { datiMercato.Tables[0].Columns["mese_rif"] };
            datiMercato.Tables[1].PrimaryKey = new DataColumn[] { datiMercato.Tables[1].Columns["mese_rif"] };
            /*CAMBIARE*/
            //int esposizionePrecedente = Convert.ToInt32(Math.Round(Convert.ToDecimal(datiMercato.Tables[1].Rows[0]["t03_esposizione_centro"])));
            DateTime dtInizio = new DateTime(DateTime.Now.AddMonths(-numeroMesi).Year, DateTime.Now.AddMonths(-numeroMesi).Month, 1);
            String mese_rif = String.Format("{0:yyyy-MM}", dtInizio);

            // prendo la prima esposizione
            DataRow drEsposizione = datiMercato.Tables[1].Rows.Find(mese_rif);
            int esposizionePrecedente = 0;
            int codiceAzienda = Convert.ToInt32(loggeduser.CodiceAzienda);
            using (DemoR2Entities context = new DemoR2Entities())
            {
                if (partitaIva.Equals(""))
                {
                    esposizionePrecedente = Convert.ToInt32(context.s03_AggregazioneSaldoAziende.Where(sa => sa.Mese_rif.Equals(mese_rif)
                      && sa.IdCentro == loggeduser.IdMercato
                      && sa.IdAzienda == codiceAzienda).Sum(sum => sum.Importo));
                }
                else
                {
                    esposizionePrecedente = Convert.ToInt32(context.s03_AggregazioneSaldoAziende.Where(sa => sa.Mese_rif.Equals(mese_rif)
                      && sa.IdCentro == loggeduser.IdMercato
                      && sa.IdAzienda == codiceAzienda && sa.PartitaIva.Equals(partitaIva)).Sum(sum => sum.Importo));
                }

            }
            DateTime dtBuffer = dtInizio;
            DateTime dtFine = DateTime.Now;

            while (dtBuffer <= dtFine)
            {

                string stringaData = String.Format("{0:yyyy-MM}", dtBuffer);
                //DataRow row = datiMercato.Tables[0].Rows.Find(stringaData);

                ElementoGraficoMercato elemento = new ElementoGraficoMercato();
                elemento.Mensilita = stringaData;
                elemento.DataMensilita = Convert.ToDateTime(String.Format("{0}-01", stringaData));
                elemento.Saldi = elemento.Vendite = 0;
                DateTime dataDA = Convert.ToDateTime(stringaData);
                DateTime dataA = dataDA.AddMonths(1);
                using (DemoR2Entities context = new DemoR2Entities())
                {
                    if (partitaIva.Equals(""))
                    {
                        elemento.Vendite = Convert.ToInt32(context.m04_vendite.Where(ve => ve.m04_dtdocvendita >= dataDA
                                                                                 && ve.m04_dtdocvendita < dataA
                                                                                 && ve.m04_codiceazienda.Equals(loggeduser.CodiceAzienda)).Sum(sum => sum.m04_importo));
                        elemento.Saldi = Convert.ToInt32(context.s03_AggregazioneSaldoAziende.Where(ag => ag.IdCentro == loggeduser.IdMercato
                                                            && ag.Mese_rif.Equals(stringaData)
                                                            && ag.IdAzienda == codiceAzienda).Sum(sum => sum.Importo));
                    }
                    else
                    {
                        elemento.Vendite = Convert.ToInt32(context.m04_vendite.Where(ve => ve.m04_dtdocvendita >= dataDA
                                                                                && ve.m04_dtdocvendita < dataA
                                                                                && ve.m04_codiceazienda.Equals(loggeduser.CodiceAzienda)
                                                                                && ve.m04_partitaiva.Equals(partitaIva)).Sum(sum => sum.m04_importo));
                        elemento.Saldi = Convert.ToInt32(context.s03_AggregazioneSaldoAziende.Where(ag => ag.IdCentro == loggeduser.IdMercato
                                                            && ag.Mese_rif.Equals(stringaData)
                                                            && ag.IdAzienda == codiceAzienda
                                                            && ag.PartitaIva.Equals(partitaIva)).Sum(sum => sum.Importo));
                    }
                }

                

                elemento.Giorni = 0;
                elemento.Incassi = esposizionePrecedente + elemento.Vendite - elemento.Saldi;
                elementiGraficoMercato.Add(elemento);
                esposizionePrecedente = elemento.Saldi;

                dtBuffer = dtBuffer.AddMonths(1);
            }

            /*
            foreach (DataRow row in datiMercato.Tables[0].Rows)
            {
                if (Convert.ToDateTime(String.Format("{0}-01", row["mese_rif"].ToString())) >= dtInizio)
                {
                    try
                    {
                        ElementoGraficoMercato elemento = new ElementoGraficoMercato();
                        elemento.Mensilita = row["mese_rif"].ToString();
                        elemento.DataMensilita = Convert.ToDateTime(String.Format("{0}-01", row["mese_rif"].ToString()));
                        elemento.Vendite = Convert.ToInt32(Math.Round(Convert.ToDouble(row["t02_importo_centro"].ToString())));
                        elemento.Saldi = Convert.ToInt32(Math.Round(Convert.ToDouble(datiMercato.Tables[1].Rows.Find(row["mese_rif"].ToString())["t03_esposizione_centro"])));

                        elemento.Incassi = esposizionePrecedente + elemento.Vendite - elemento.Saldi;
                        elementiGraficoMercato.Add(elemento);

                        esposizionePrecedente = elemento.Saldi;
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
            */
            int skipInterval = 1;
            elementiGraficoMercato = elementiGraficoMercato.OrderBy(o => o.DataMensilita).Skip(skipInterval).Take(numeroMesi - 1).ToList();

            return Json(elementiGraficoMercato, JsonRequestBehavior.AllowGet);
        }

        public List<string> GetListAziendeMercato(int idCentro)
        {
            List<string> retVal = new List<string>();
            try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                builder.DataSource = "osservaweb.cr2zvcfmn3xy.eu-west-1.rds.amazonaws.com";
                builder.UserID = "root";
                builder.Password = "Ma4c01t!";
                builder.InitialCatalog = "OsservaCentraleRischi";

                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    connection.Open();
                    StringBuilder sb = new StringBuilder();
                    sb.Append("SELECT m01_codice ");
                    sb.Append("FROM m01_aziende ");
                    sb.Append("WHERE m01_idcentro= "+idCentro);

                    String sql = sb.ToString();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                retVal.Add(reader.GetString(0));
                            }
                        }
                    }
                }
                return retVal;
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
                return retVal;
            }
        }



    }
}
