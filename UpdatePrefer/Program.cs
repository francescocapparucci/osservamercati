using CentraleRischiR2Library;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpdatePrefer
{
    class Program
    {
        
        static void Main(string[] args)
        {
            // Boolean retval = Allineamento_M02_News();
            bonifica_news_nuovi_id();
            //Console.WriteLine("END OF " + retval);
        }

        //batch che legge tutti i record per uno user id e esegue update di ogni row se presente di un altro userid aggiorna, 
        // altrimenti mette tutto in default
        public static void  bonifica_news_nuovi_id()
        {
            int id_user = 1369;
            int contatore = 0;
            using (DemoR2Entities context = new DemoR2Entities())
            {
                List<news_operatore> listope = context.news_operatore.Where(a => a.id_user == id_user).ToList();
                foreach(news_operatore el in listope)
                {
                    contatore++;
                    Console.WriteLine("n " + contatore + " di " + listope.Count);
                    List<news_operatore> appo = context.news_operatore.Where(a => a.partita_iva.Equals(el.partita_iva) && a.id_user != id_user).ToList();
                    appo = appo.OrderByDescending(ord => ord.data_aggiornamento).ToList();
                    
                    if (appo.Count >0)
                    {
                        Console.WriteLine("trovata corrispondenza su news_operatore valutazione= " + appo[0].valutazione + " rapporto= " + appo[0].rapporto);
                        el.valutazione = appo[0].valutazione;
                        el.rapporto = appo[0].rapporto;
                        el.fido = appo[0].fido;

                    }
                    else
                    {
                        el.valutazione = null;
                        el.rapporto = "";
                        el.fido = 0;
                    }
                    Console.WriteLine("salvo contesto " + el.partita_iva + " rapporto= " + el.rapporto + " valutazione= " 
                                      + el.valutazione + " fido= " + el.fido);
                    context.SaveChanges();
                }
            }
        }
        public static void bonifica_news()
        {

            using (DemoR2Entities context = new DemoR2Entities())
            {
                List<string> listaPartiteIvaAggiornate = new List<string>();
                List<string> listaPartiteIvaNonPresntiInAnag = new List<string>();
                int ii = 0;
                DirectoryInfo di = new DirectoryInfo("C:\\R3\\ReportCs");
                IDictionary<string, DateTime> openWith = new Dictionary<string, DateTime>();
                string pivaDaFIle = "";
                Console.WriteLine("FILE " + di.GetFiles().Count());
                foreach (var element in di.GetFiles())
                {
                    int subend = element.Name.IndexOf("_");
                    pivaDaFIle = element.Name.Substring(0, subend);
                    m02_anagrafica piva_m02 = context.m02_anagrafica.Where(a => a.m02_partitaiva.Equals(pivaDaFIle)).FirstOrDefault();
                    if (piva_m02 != null)
                    {
                        Console.WriteLine("m02 is not null");
                        // if (piva_m02.m02_note.Equals(""))
                        // {
                        Console.WriteLine("note is null");
                        //   piva_m02.m02_note = element.ToString();
                        news_operatore anag = context.news_operatore.Where(a => a.partita_iva.Equals(pivaDaFIle)).FirstOrDefault();
                        if (anag != null)
                        {
                            DateTime data = context.news_operatore.Where(a => a.partita_iva.Equals(pivaDaFIle)).Max(z => z.data_aggiornamento);
                            piva_m02.m02_fido = context.news_operatore.Where(a => a.partita_iva.Equals(pivaDaFIle) && a.data_aggiornamento == data).Select(b => b.fido).FirstOrDefault();
                            Console.WriteLine("aggiornato " + element.ToString());
                            listaPartiteIvaAggiornate.Add(element.ToString());
                            context.SaveChanges();
                        }

                        // }
                        //else
                        // {
                        //     Console.WriteLine("note is not null "+piva_m02.m02_note);
                        // }
                    }
                    else
                    {
                        Console.WriteLine("non aggiornato per m02 null=" + element.ToString());
                        listaPartiteIvaNonPresntiInAnag.Add(element.ToString());
                    }
                }
                string appo = "";


                //AGGIORNAMENTO DEL CAMPO NOTE DI M="_ANAGRAFICHE DA NEWS_RAPPORTO 
                /*       List<string> review = (context.news_operatore).Distinct().Select(a => a.partita_iva).ToList();
               review = review.Distinct().ToList();
               foreach (var element in review)
               {
                  ii++;
                  news_operatore test = context.news_operatore.Where(c => c.partita_iva.Equals(element)).FirstOrDefault();
                  if (!test.rapporto.Equals(""))
                  {
                       m02_anagrafica testanag = context.m02_anagrafica.Where(z => z.m02_partitaiva == test.partita_iva).FirstOrDefault();
                       if (!testanag.m02_note.Equals(test.rapporto))
                       {
                           testanag.m02_note = test.rapporto;
                           context.SaveChanges();
                           Console.WriteLine("SALVATO " + testanag.m02_partitaiva);
                       }
                       else
                       {
                           Console.WriteLine("GIA PRESENTE IN ANAGRAFICA " + ii);
                       }
                   }
                   else
                   {
                       Console.WriteLine("RAPPORTO NULL SU NEWS " + ii);

                   }


               }*/

                /* foreach(var el in review)
                {
                    List<m02_anagrafica> appoList = context.m02_anagrafica.Where(anag => anag.m02_partitaiva == el.partita_iva).ToList();
                    Console.WriteLine("trovati n" + appoList.Count + " elementi in anagrafica");
                    int z = 1;
                    foreach (var elem in appoList)
                    {
                        if(el.fido != elem.m02_fido)
                        {
                            elem.m02_fido = el.fido;
                            Console.WriteLine("AGGIORNATI " + z + " di " + appoList.Count);
                        }
                        elem.m02_note = el.rapporto;
                        context.SaveChanges();
                        Console.WriteLine(z);
                        z++;
                    }
                    ii++;
                    Console.WriteLine(ii +" di "+review.Count);
                }*/
            }
        }

        //BATCH PER LA BONIFICA DELLA TABELLA NEWS_REPORT LEGGE TUTTI I DATI CONTENTENTI "_" E RIALLINEA TUTTI I RECORD CON I NUOVI RISULATATI QUELLI CON IL CAMPO RAPPORTO SPORCO LI RIPORTA A ""
        public static Boolean Allineamento_M02_News()
        {
            Boolean retval = false;
            using (DemoR2Entities context = new DemoR2Entities())
            {
                int i = 0;
                List<m02_anagrafica> allAnag = context.m02_anagrafica.ToList();
                foreach (m02_anagrafica anag in allAnag)
                {
                    i++;
                    Console.WriteLine("record " + i +" di "+allAnag.Count);
                    news_operatore newop = context.news_operatore.Where(a => a.partita_iva == anag.m02_partitaiva).FirstOrDefault();
                    if (newop != null)
                    {
                        if (newop.fido != anag.m02_fido)
                        {
                            Console.WriteLine("TROVATA NEWS FIDO= " + newop.fido + " ANAGRAFICA FIDO= " + anag.m02_fido);
                            anag.m02_fido = newop.fido;
                            context.SaveChanges();
                        }
                    }
                    else if (anag.m02_note.Equals("")&& anag.m02_fido!=0)
                    {
                        Console.WriteLine("ANAG SOLITARIA FIDO= " + anag.m02_fido);
                        anag.m02_fido = 0;
                        context.SaveChanges();
                    }
                }
            }
            return retval;
        }
    }
}

     

