using CentraleRischiR2Library;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpdatePreferiti
{
    // BATCH CHE ESEGUE PER L ID AZIENDA UNA RICERCA DI TUTTE LE ANAGRAFICHE E LE INSERISCE NELLA TABELLA M_15_PREFERITI
    class Program
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        static void Main(string[] args)
        {
            Log.Info("begin Update Prefertit***");
            using (DemoR2Entities context = new DemoR2Entities())
            {
                List<m01_aziende> aziendeRecupero =
                context.m01_aziende.Where(_=>_.m01_attivo_r2==true).ToList();

                foreach (var codiceAzienda in aziendeRecupero)
                {
                     
                    m10_utenti utente = DBHandler.GetUtenteAziendaImportazionepreferiti(codiceAzienda.m01_idazienda, context);
                    if (utente != null)
                    {
                        Log.Info("Azienda= " + codiceAzienda.m01_ragionesociale+" Utente= "+utente.m10_iduser);
                        List<string> partitaIvaMancanti =
                           context.m04_vendite.Where(ven => ven.m04_codiceazienda == codiceAzienda.m01_codice
                               && ven.m04_partitaiva != codiceAzienda.m01_partitaiva)
                           .Select(a => a.m04_partitaiva)
                           .Except(context.m15_preferiti.Where(pref => pref.m15_iduser == utente.m10_iduser).Select(a => a.m15_partitaiva))
                           .Distinct().ToList();
                        Log.Info("Trovate " + partitaIvaMancanti.Count + " Partite Iva da Inserire Nuove");
                        int i = 1;
                        foreach (string partitaIvaMancante in partitaIvaMancanti)
                        {
                            m02_anagrafica anagrafica = context.m02_anagrafica.Where(a => a.m02_partitaiva == partitaIvaMancante).FirstOrDefault();
                            if(anagrafica == null)
                            {
                                continue;
                            }
                            Log.Info("Partita Iva= " + partitaIvaMancante+" Ragione Sociale= "+anagrafica.m02_ragionesociale);
                            m15_preferiti rigaPreferito =
                                context.m15_preferiti.Where(pref => pref.m15_iduser == utente.m10_iduser
                                                                        && pref.m15_partitaiva == partitaIvaMancante).FirstOrDefault();

                            if (rigaPreferito == null)
                            {
                                Console.WriteLine("preferito per partita iva : " + partitaIvaMancante + " e Id User : " + utente.m10_iduser + " non trovati " + i);
                            }
                            m15_preferiti nuovoPreferito = new m15_preferiti();
                            nuovoPreferito.m15_iduser = utente.m10_iduser;
                            nuovoPreferito.m15_partitaiva = partitaIvaMancante;
                            context.m15_preferiti.AddObject(nuovoPreferito);
                            Console.WriteLine("aggiunto n° " + i + " di :" + partitaIvaMancanti.Count);
                            i++;
                            context.SaveChanges();
                        }
                        int z = i - 1;
                        Log.Info("Inserite " + z + " Partite iva");
                    }
                }

            }

        }
    }
}
