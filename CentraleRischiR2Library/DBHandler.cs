using CentraleRischiR2Library.BridgeClasses;
using CentraleRischiR2Library.CervedThreeStepProdServiceReference;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Objects.SqlClient;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Configuration;

namespace CentraleRischiR2Library
{
    public class DBHandler
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static Boolean sendPassword(int iduser, string email)
        {
            Log.Info("*** Start sendPassword");
            Boolean retVal = false;
            string password = "";
            try
            {
                using (DemoR2Entities context = new DemoR2Entities())
                {
                    password = context.m10_utenti.Where(ut => ut.m10_iduser == iduser).Select(sel => sel.m10_pass).FirstOrDefault() != null ?
                                context.m10_utenti.Where(ut => ut.m10_iduser == iduser).Select(sel => sel.m10_pass).FirstOrDefault() : "";
                    if (password != "")
                    {
                        retVal = true;
                    }
                }
                string body = "La password per il vostro sistema Osservatorio è : " + password;
                MailHandler.SendMail(email, body);
            }
            catch (Exception e)
            {
                Log.Error("errore : " + e.ToString());
            }
            return retVal;
        }

        public static List<ElementoFatturaRecuperoCrediti> GetVenditeScadute(string codieAzienda)
        {
            List<ElementoFatturaRecuperoCrediti> returnValue = new List<ElementoFatturaRecuperoCrediti>();
            using (DemoR2Entities context = new DemoR2Entities())
            {
                returnValue = context.m04_vendite
                    .Where(az => az.m04_codiceazienda == codieAzienda && az.m04_chiusura == false && az.m04_importo > 0)
                    .Join(context.m02_anagrafica,
                        rj => rj.m04_partitaiva,
                        lj => lj.m02_partitaiva,
                        (rj, lj) => new ElementoFatturaRecuperoCrediti
                        {
                            PartitaIva = rj.m04_partitaiva,
                            RagioneSociale = lj.m02_ragionesociale,
                            DataFattura = rj.m04_dtdocvendita,
                            DataScadenza = rj.m04_dtscadenza.Value,
                            Importo = rj.m04_importo.Value,
                            NDoc = rj.m04_numdoc,
                            EventiNegativi = lj.m02_eventi_negativi,
                            Rating =
                                context.m05_rating
                                .Where(rat => rat.m05_partitaiva == rj.m04_partitaiva).OrderByDescending(rating => rating.m05_dtriferimento).FirstOrDefault() != null ?
                                context.m05_rating
                                .Where(rat => rat.m05_partitaiva == rj.m04_partitaiva).OrderByDescending(rating => rating.m05_dtriferimento).FirstOrDefault().m05_stato : "",
                            RatingDescrizione =
                                context.m05_rating
                                .Where(rat => rat.m05_partitaiva == rj.m04_partitaiva).FirstOrDefault() != null ?
                                context.m05_rating
                                .Where(rat => rat.m05_partitaiva == rj.m04_partitaiva).FirstOrDefault().m05_stato_semaforo : ""
                        }

                        ).OrderBy(scad => scad.DataScadenza).Take(500).ToList();
            }
            return returnValue;

        }

        public static void InviaPassword(string name)
        {
            //
        }

        public static List<ElementoReportRating> GetDistribuzioneAziende(m10_utenti utente, m01_aziende azienda)
        {
            List<ElementoReportRating> returnValue = new List<ElementoReportRating>();
            using (DemoR2Entities context = new DemoR2Entities())
            {
                returnValue =
                    GetPreferitiMonitoraggio(utente.m10_iduser, utente.m10_idruolo.Value, azienda.m01_idcentro.Value, "STANDARD").Where(az => az.Esposizione > 0)
                    .GroupBy(gr => gr.Rating).Select(gruppo => new ElementoReportRating { Rating = gruppo.Key, Companies = gruppo.Count() }).OrderBy(o => o.Rating).ToList();

            }
            return returnValue;

        }

        public static List<ElementoFatturaRecuperoCrediti> GetVenditeScadute30giorni(string codieAzienda)
        {
            List<ElementoFatturaRecuperoCrediti> returnValue = new List<ElementoFatturaRecuperoCrediti>();
            DateTime data30giorniFa = DateTime.Now.AddDays(-30);
            using (DemoR2Entities context = new DemoR2Entities())
            {
                returnValue = context.m04_vendite
                    .Where(az => az.m04_codiceazienda == codieAzienda && az.m04_chiusura == false && az.m04_importo > 0 && az.m04_dtscadenza >= data30giorniFa)
                    .Join(context.m02_anagrafica,
                        rj => rj.m04_partitaiva,
                        lj => lj.m02_partitaiva,
                        (rj, lj) => new ElementoFatturaRecuperoCrediti
                        {
                            PartitaIva = rj.m04_partitaiva,
                            RagioneSociale = lj.m02_ragionesociale,
                            DataFattura = rj.m04_dtdocvendita,
                            DataScadenza = rj.m04_dtscadenza.Value,
                            Importo = rj.m04_importo.Value,
                            NDoc = rj.m04_numdoc,
                            EventiNegativi = lj.m02_eventi_negativi,
                            Rating =
                                context.m05_rating
                                .Where(rat => rat.m05_partitaiva == rj.m04_partitaiva).OrderByDescending(rating => rating.m05_dtriferimento).FirstOrDefault() != null ?
                                context.m05_rating
                                .Where(rat => rat.m05_partitaiva == rj.m04_partitaiva).OrderByDescending(rating => rating.m05_dtriferimento).FirstOrDefault().m05_stato : "",
                            RatingDescrizione =
                                context.m05_rating
                                .Where(rat => rat.m05_partitaiva == rj.m04_partitaiva).FirstOrDefault() != null ?
                                context.m05_rating
                                .Where(rat => rat.m05_partitaiva == rj.m04_partitaiva).FirstOrDefault().m05_stato_semaforo : ""
                        }

                        ).OrderBy(scad => scad.DataScadenza).ToList();
            }
            return returnValue;

        }

        public static Double SommaVenditeEmesseXGiorni(string codieAzienda, int numeroGiorni)
        {
            Double returnValue = 0;
            DateTime dataXgiorniFa = DateTime.Now.AddDays(-numeroGiorni);
            using (DemoR2Entities context = new DemoR2Entities())
            {
                try
                {
                    returnValue = context.m04_vendite
                        .Where(az => az.m04_codiceazienda == codieAzienda && az.m04_importo > 0 && az.m04_dtdocvendita >= dataXgiorniFa)
                        .Join(context.m02_anagrafica,
                            rj => rj.m04_partitaiva,
                            lj => lj.m02_partitaiva,
                            (rj, lj) => new ElementoFatturaRecuperoCrediti
                            {
                                PartitaIva = rj.m04_partitaiva,
                                RagioneSociale = lj.m02_ragionesociale,
                                DataFattura = rj.m04_dtdocvendita,
                                DataScadenza = rj.m04_dtscadenza.Value,
                                Importo = rj.m04_importo.Value,
                                NDoc = rj.m04_numdoc,
                                EventiNegativi = lj.m02_eventi_negativi,
                                Rating =
                                    context.m05_rating
                                    .Where(rat => rat.m05_partitaiva == rj.m04_partitaiva).OrderByDescending(rating => rating.m05_dtriferimento).FirstOrDefault() != null ?
                                    context.m05_rating
                                    .Where(rat => rat.m05_partitaiva == rj.m04_partitaiva).OrderByDescending(rating => rating.m05_dtriferimento).FirstOrDefault().m05_stato : "",
                                RatingDescrizione =
                                    context.m05_rating
                                    .Where(rat => rat.m05_partitaiva == rj.m04_partitaiva).FirstOrDefault() != null ?
                                    context.m05_rating
                                    .Where(rat => rat.m05_partitaiva == rj.m04_partitaiva).FirstOrDefault().m05_stato_semaforo : ""
                            }


                            ).Sum(s => s.Importo);
                }
                catch (Exception e)
                {
                    Log.Error("error : " + e.ToString());
                }
            }
            return returnValue;

        }

        public static Double SommaVenditeIncassateXGiorni(string codieAzienda, int numeroGiorni)
        {
            Double returnValue = 0;
            DateTime dataXgiorniFa = DateTime.Now.AddDays(-numeroGiorni);
            using (DemoR2Entities context = new DemoR2Entities())
            {
                try
                {
                    returnValue = context.m04_vendite
                        .Where(az => az.m04_codiceazienda == codieAzienda && az.m04_chiusura == true && az.m04_importo > 0 && az.m04_dtchiusura >= dataXgiorniFa)
                        .Join(context.m02_anagrafica,
                            rj => rj.m04_partitaiva,
                            lj => lj.m02_partitaiva,
                            (rj, lj) => new ElementoFatturaRecuperoCrediti
                            {
                                PartitaIva = rj.m04_partitaiva,
                                RagioneSociale = lj.m02_ragionesociale,
                                DataFattura = rj.m04_dtdocvendita,
                                DataScadenza = rj.m04_dtscadenza.Value,
                                Importo = rj.m04_importo.Value,
                                NDoc = rj.m04_numdoc,
                                EventiNegativi = lj.m02_eventi_negativi,
                                Rating =
                                    context.m05_rating
                                    .Where(rat => rat.m05_partitaiva == rj.m04_partitaiva).OrderByDescending(rating => rating.m05_dtriferimento).FirstOrDefault() != null ?
                                    context.m05_rating
                                    .Where(rat => rat.m05_partitaiva == rj.m04_partitaiva).OrderByDescending(rating => rating.m05_dtriferimento).FirstOrDefault().m05_stato : "",
                                RatingDescrizione =
                                    context.m05_rating
                                    .Where(rat => rat.m05_partitaiva == rj.m04_partitaiva).FirstOrDefault() != null ?
                                    context.m05_rating
                                    .Where(rat => rat.m05_partitaiva == rj.m04_partitaiva).FirstOrDefault().m05_stato_semaforo : ""
                            }

                            ).Sum(s => s.Importo);
                }
                catch (Exception e)
                {
                    Log.Error("error : " + e.ToString());
                }
            }
            return returnValue;

        }

        public static Double SommaVenditeScaduteXGiorni(string codieAzienda, int numeroGiorni)
        {
            Double returnValue = 0;
            DateTime dataXgiorniFa = DateTime.Now.AddDays(-numeroGiorni);
            using (DemoR2Entities context = new DemoR2Entities())
            {
                returnValue = context.m04_vendite
                    .Where(az => az.m04_codiceazienda == codieAzienda && az.m04_chiusura == false && az.m04_importo > 0 && az.m04_dtscadenza >= dataXgiorniFa)
                    .Join(context.m02_anagrafica,
                        rj => rj.m04_partitaiva,
                        lj => lj.m02_partitaiva,
                        (rj, lj) => new ElementoFatturaRecuperoCrediti
                        {
                            PartitaIva = rj.m04_partitaiva,
                            RagioneSociale = lj.m02_ragionesociale,
                            DataFattura = rj.m04_dtdocvendita,
                            DataScadenza = rj.m04_dtscadenza.Value,
                            Importo = rj.m04_importo.Value,
                            NDoc = rj.m04_numdoc,
                            EventiNegativi = lj.m02_eventi_negativi,
                            Rating =
                                context.m05_rating
                                .Where(rat => rat.m05_partitaiva == rj.m04_partitaiva).OrderByDescending(rating => rating.m05_dtriferimento).FirstOrDefault() != null ?
                                context.m05_rating
                                .Where(rat => rat.m05_partitaiva == rj.m04_partitaiva).OrderByDescending(rating => rating.m05_dtriferimento).FirstOrDefault().m05_stato : "",
                            RatingDescrizione =
                                context.m05_rating
                                .Where(rat => rat.m05_partitaiva == rj.m04_partitaiva).FirstOrDefault() != null ?
                                context.m05_rating
                                .Where(rat => rat.m05_partitaiva == rj.m04_partitaiva).FirstOrDefault().m05_stato_semaforo : ""
                        }

                        ).Sum(s => s.Importo);
            }
            return returnValue;

        }






        public static bool ApprovatoPrivacy(string codiceAzienda, string ambiente)
        {
            bool returnValue = false;
            using (DemoR2Entities context = new DemoR2Entities())
            {
                if (ambiente == "STANDARD")
                {
                    vs_aziende_osservamercati azienda = context.vs_aziende_osservamercati.Where(az => az.m01_codice == codiceAzienda && az.m01_attivo_r2 == true).FirstOrDefault();
                    if (azienda != null)
                    {
                        returnValue = azienda.m01_approvato_privacy.HasValue ? azienda.m01_approvato_privacy.Value : false;
                    }
                }
                else
                {
                    vs_aziende_osservacrediti azienda = context.vs_aziende_osservacrediti.Where(az => az.m01_codice == codiceAzienda && az.m01_attivo_r2 == true).FirstOrDefault();
                    if (azienda != null)
                    {
                        returnValue = azienda.m01_approvato_privacy.HasValue ? azienda.m01_approvato_privacy.Value : false;
                    }
                }
            }
            return returnValue;
        }

        public static bool ApprovaPrivacy(string codiceAzienda)
        {
            bool returnValue = false;
            using (DemoR2Entities context = new DemoR2Entities())
            {
                m01_aziende azienda = context.m01_aziende.Where(az => az.m01_codice == codiceAzienda).FirstOrDefault();
                if (azienda != null)
                {
                    azienda.m01_approvato_privacy = true;
                    azienda.m01_data_approvazione_privacy = DateTime.Now;
                    returnValue = true;
                    context.SaveChanges();
                }
            }
            return returnValue;
        }


        public static NavigationUser LogUser(string username, string password, string ambiente = "STANDARD")
        {
            NavigationUser returnValue = null;

            using (DemoR2Entities context = new DemoR2Entities())
            {
                var bufferValue = context.m10_utenti.Where(ut => ut.m10_user == username && ut.m10_pass == password && ut.m10_attivo.Value == true)
                    .FirstOrDefault();

                if (bufferValue != null)
                {
                    if (ambiente == "STANDARD")
                    {
                        returnValue =
                            context.m01_aziende.Where(az => az.m01_idazienda == bufferValue.m10_idazienda).
                            Join(context.m00_centri,
                            a => a.m01_idcentro,
                            b => b.m00_idcentro,
                            (a, b) => new NavigationUser
                            {
                                Azienda = a.m01_ragionesociale,
                                CittaMercato = b.m00_localita,
                                CodiceAzienda = a.m01_codice,
                                IdMercato = b.m00_idcentro,
                                IdAzienda = a.m01_idazienda,
                                IdRuolo = bufferValue.m10_idruolo.HasValue ? bufferValue.m10_idruolo.Value : 2,
                                IdUser = bufferValue.m10_iduser,
                                Mercato = b.m00_centro,
                                Username = bufferValue.m10_user,
                                Indirizzo = a.m01_indirizzo,
                                Localita = a.m01_pv,
                                Email = bufferValue.m10_email,
                                Ambiente = ambiente,
                                ApprovatoPrivacy = a.m01_approvato_privacy.HasValue ? a.m01_approvato_privacy.Value : false,
                                Abilitato = bufferValue.m10_attivo_notturno.HasValue ? bufferValue.m10_attivo_notturno.Value : false,
                                AbilitatoNotturno = bufferValue.m10_attivo_notturno.HasValue ? bufferValue.m10_attivo_notturno.Value : false,
                                AbilitatoRicerca = bufferValue.m10_attivo_rircerca.HasValue ? bufferValue.m10_attivo_rircerca.Value : false,
                                DataApprovazionePrivacy = a.m01_data_approvazione_privacy,
                                CodiceFinservice = a.m01_codice_rc != null ? a.m01_codice_rc : String.Empty,
                                Demo = bufferValue.m10_demo.HasValue ? bufferValue.m10_demo.Value : false

                            }).ToList().FirstOrDefault();
                    }
                    else
                    {
                        returnValue =
                            context.vs_aziende_osservacrediti.Where(az => az.m01_idazienda == bufferValue.m10_idazienda).
                            Join(context.m00_centri,
                            a => a.m01_idcentro,
                            b => b.m00_idcentro,
                            (a, b) => new NavigationUser
                            {
                                Azienda = a.m01_ragionesociale,
                                CittaMercato = b.m00_localita,
                                CodiceAzienda = a.m01_codice,
                                IdMercato = b.m00_idcentro,
                                IdRuolo = bufferValue.m10_idruolo.HasValue ? bufferValue.m10_idruolo.Value : 2,
                                IdUser = bufferValue.m10_iduser,
                                Mercato = b.m00_centro,
                                Username = bufferValue.m10_user,
                                Indirizzo = a.m01_indirizzo,
                                Localita = a.m01_pv,
                                Email = bufferValue.m10_email,
                                Ambiente = ambiente,
                                ApprovatoPrivacy = a.m01_approvato_privacy.HasValue ? a.m01_approvato_privacy.Value : false,
                                DataApprovazionePrivacy = a.m01_data_approvazione_privacy,
                                CodiceFinservice = a.m01_codice_rc,
                                CodicePayLine = a.m01_codice_payline_cerved != null ? a.m01_codice_payline_cerved : String.Empty,
                                Demo = bufferValue.m10_demo.HasValue ? bufferValue.m10_demo.Value : false
                            }).ToList().FirstOrDefault();

                    }

                    /*Effettuo Log dell'aavvenuto accesso*/
                    if (returnValue != null)
                    {
                        m24_accessi nuovoAccesso = new m24_accessi();
                        nuovoAccesso.m24_iduser = returnValue.IdUser;
                        nuovoAccesso.m24_data_accesso = DateTime.Now;
                        context.m24_accessi.AddObject(nuovoAccesso);
                        context.SaveChanges();
                    }

                }

            }
            return returnValue;
        }

        public static NavigationUser ChangeUserAdmin(int idUtente, string ambiente = "STANDARD")
        {
            NavigationUser returnValue = null;

            using (DemoR2Entities context = new DemoR2Entities())
            {
                var bufferValue = context.m10_utenti.Where(ut => ut.m10_iduser == idUtente && ut.m10_attivo.Value == true)
                    .FirstOrDefault();

                if (bufferValue != null)
                {
                    if (ambiente == "STANDARD")
                    {
                        returnValue =
                            context.vs_aziende_osservamercati.Where(az => az.m01_idazienda == bufferValue.m10_idazienda).
                            Join(context.m00_centri,
                            a => a.m01_idcentro,
                            b => b.m00_idcentro,
                            (a, b) => new NavigationUser
                            {
                                Azienda = a.m01_ragionesociale,
                                CittaMercato = b.m00_localita,
                                CodiceAzienda = a.m01_codice,
                                IdMercato = b.m00_idcentro,
                                IdRuolo = bufferValue.m10_idruolo.HasValue ? bufferValue.m10_idruolo.Value : 2,
                                IdUser = bufferValue.m10_iduser,
                                Mercato = b.m00_centro,
                                Username = bufferValue.m10_user,
                                Indirizzo = a.m01_indirizzo,
                                Localita = a.m01_pv,
                                Email = bufferValue.m10_email,
                                Ambiente = ambiente,
                                ApprovatoPrivacy = a.m01_approvato_privacy.HasValue ? a.m01_approvato_privacy.Value : false,
                                Abilitato = bufferValue.m10_attivo_notturno.HasValue ? bufferValue.m10_attivo_notturno.Value : false,
                                AbilitatoNotturno = bufferValue.m10_attivo_notturno.HasValue ? bufferValue.m10_attivo_notturno.Value : false,
                                AbilitatoRicerca = bufferValue.m10_attivo_rircerca.HasValue ? bufferValue.m10_attivo_rircerca.Value : false,
                                DataApprovazionePrivacy = a.m01_data_approvazione_privacy,
                                CodiceFinservice = a.m01_codice_rc != null ? a.m01_codice_rc : String.Empty,
                                CodicePayLine = a.m01_codice_payline_cerved != null ? a.m01_codice_payline_cerved : String.Empty,
                                Demo = bufferValue.m10_demo.HasValue ? bufferValue.m10_demo.Value : false

                            }).ToList().FirstOrDefault();
                    }
                    else
                    {
                        returnValue =
                            context.vs_aziende_osservacrediti.Where(az => az.m01_idazienda == bufferValue.m10_idazienda).
                            Join(context.m00_centri,
                            a => a.m01_idcentro,
                            b => b.m00_idcentro,
                            (a, b) => new NavigationUser
                            {
                                Azienda = a.m01_ragionesociale,
                                CittaMercato = b.m00_localita,
                                CodiceAzienda = a.m01_codice,
                                IdMercato = b.m00_idcentro,
                                IdRuolo = bufferValue.m10_idruolo.HasValue ? bufferValue.m10_idruolo.Value : 2,
                                IdUser = bufferValue.m10_iduser,
                                Mercato = b.m00_centro,
                                Username = bufferValue.m10_user,
                                Indirizzo = a.m01_indirizzo,
                                Localita = a.m01_pv,
                                Email = bufferValue.m10_email,
                                Ambiente = ambiente,
                                ApprovatoPrivacy = a.m01_approvato_privacy.HasValue ? a.m01_approvato_privacy.Value : false,
                                DataApprovazionePrivacy = a.m01_data_approvazione_privacy,
                                CodiceFinservice = a.m01_codice_rc,
                                CodicePayLine = a.m01_codice_payline_cerved != null ? a.m01_codice_payline_cerved : String.Empty,
                                Demo = bufferValue.m10_demo.HasValue ? bufferValue.m10_demo.Value : false
                            }).ToList().FirstOrDefault();

                    }

                }

            }
            return returnValue;
        }


        public static Dictionary<string, string> ElencoProvince()
        {

            Dictionary<string, string> returnValue = new Dictionary<string, string>();

            using (DemoR2Entities context = new DemoR2Entities())
            {
                returnValue = context.provincia.ToDictionary(a => a.targa, a => a.provincia1);

            }
            return returnValue;

        }

        public static Dictionary<string, string> ElencoAziendeSuperAdmin()
        {

            Dictionary<string, string> returnValue = new Dictionary<string, string>();

            using (DemoR2Entities context = new DemoR2Entities())
            {
                returnValue = context.m10_utenti.Where(u => u.m10_attivo == true)
                    .Join(context.vs_aziende_osservamercati,
                    a => a.m10_idazienda,
                    b => b.m01_idazienda,
                    (a, b) => new { IdUtente = SqlFunctions.StringConvert((decimal)a.m10_iduser).Trim(), RagioneSociale = b.m01_ragionesociale + " (utente " + a.m10_user + ")" }).OrderBy(a => a.RagioneSociale).ToDictionary(a => a.IdUtente, a => a.RagioneSociale);

            }
            return returnValue;

        }

        public static Dictionary<string, string> ElencoMercati()
        {
            Dictionary<string, string> returnValue = new Dictionary<string, string>();

            using (DemoR2Entities context = new DemoR2Entities())
            {
                returnValue = context.m00_centri.ToDictionary(a => a.m00_idcentro.ToString(), a => a.m00_localita);
            }
            return returnValue;
        }

        /*
    public static SelectList ElencoProvince()
    {


        List<SelectListItem> returnValue = new List<SelectListItem>();

        using (DemoR2Entities context = new DemoR2Entities())
        {
            returnValue = context.provincia
                .Select(pr => new SelectListItem { Value = pr.targa, Text = pr.provincia1 }).ToList();
        }
        return new SelectList(returnValue, "Value", "Text");
        
    }
    
    public static SelectList ElencoMercati()
        {
            List<SelectListItem> returnValue = new List<SelectListItem>();

            using (DemoR2Entities context = new DemoR2Entities())
            {
                returnValue = context.m00_centri
                    .Select(pr => new SelectListItem { Value = SqlFunctions.StringConvert((double)pr.m00_idcentro), Text = pr.m00_localita }).ToList();
            }
            return new SelectList(returnValue, "Value", "Text");
        }
        */
        public static List<ElementoMercato> ElencoCentri()
        {
            List<ElementoMercato> returnValue = new List<ElementoMercato>();

            NavigationUser user = new NavigationUser();
            using (DemoR2Entities context = new DemoR2Entities())
            {
                returnValue = context.m00_centri
                    .Select(pr => new ElementoMercato { Citta = pr.m00_localita, IdMercato = pr.m00_idcentro, NomeMercato = pr.m00_centro }).ToList();
            }
            return returnValue;
        }
        public static List<ElementoMercato> ElencoCentri(int idMercato)
        {
            List<ElementoMercato> returnValue = new List<ElementoMercato>();

            NavigationUser user = new NavigationUser();
            using (DemoR2Entities context = new DemoR2Entities())
            {
                returnValue = context.m00_centri.Where(_ => _.m00_idcentro == idMercato)
                    .Select(pr => new ElementoMercato { Citta = pr.m00_localita, IdMercato = pr.m00_idcentro, NomeMercato = pr.m00_centro }).ToList();
            }
            return returnValue;
        }

        public static List<vs_aziende_osservamercati> ElencoOperatoriCentro(int idCentro)
        {
            List<vs_aziende_osservamercati> returnValue = new List<vs_aziende_osservamercati>();

            using (DemoR2Entities context = new DemoR2Entities())
            {
                returnValue = context.vs_aziende_osservamercati.Where(om => om.m01_idcentro == idCentro).ToList();

            }
            return returnValue;
        }
        static public void UpdateUtente(dynamic utente)
        {
            int id = 0;
            try
            {
                int idAzienda = utente.IdAzienda;
                using (DemoR2Entities context = new DemoR2Entities())
                {
                    m10_utenti utenteRec = new m10_utenti();
                    id = Convert.ToInt32(utente.IdUser);
                    utenteRec = context.m10_utenti.Where(m => m.m10_iduser == id).FirstOrDefault();

                    if (utente.Email != "" || utente.Email != null)
                        utenteRec.m10_email = utente.Email;
                    utenteRec.m10_pass = utente.Password;
                    context.SaveChanges();
                }
                DataTable dt = new DataTable();
                String sSql = String.Empty;
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString))
                {
                    sSql = "update OsservaCentraleRischi.dbo.m10_utenti " +
                           "set m10_pass='" + utente.Password + "' , m10_email='" + utente.Email + "' where m10_user= '" + utente.Name + "'";

                    SqlCommand cmd = new SqlCommand(sSql, conn);
                    cmd.Connection.Open();
                    cmd.ExecuteNonQuery();
                    Log.Info("righe update " + cmd.ExecuteNonQuery().ToString());

                    DBHandler.CloseConn(conn);
                }
            }
            catch (Exception e)
            {
                Log.Error("Update Utente : " + e.ToString());
            }

        }

        static public void SalvaUtente(dynamic utente)
        {
            int idAzienda = utente.IdAzienda;
            using (DemoR2Entities context = new DemoR2Entities())
            {
                m10_utenti utenteRec = new m10_utenti();
                if (utente.IdUser != 0)
                {
                    int id = Convert.ToInt32(utente.IdUser);
                    utenteRec =
                        context.m10_utenti.Where(m => m.m10_iduser == id).FirstOrDefault();
                }

                utenteRec.m10_idazienda = idAzienda;
                utenteRec.m10_user = utente.Name;
                utenteRec.m10_email = utente.Email;
                utenteRec.m10_pass = utente.Password;
                utenteRec.m10_attivo = utente.Abilitato;
                utenteRec.m10_attivo_notturno = utente.AbilitatoNotturno;
                utenteRec.m10_attivo_rircerca = utente.AbilitatoRicerca;
                utenteRec.m10_demo = utente.Demo;
                utenteRec.m10_idruolo = utente.IdRole;

                if (utente.IdUser == 0)
                {
                    context.m10_utenti.AddObject(utenteRec);
                }

                context.SaveChanges();

            }
        }

        public static List<vs_aziende_osservamercati> ElencoOperatori()
        {
            List<vs_aziende_osservamercati> returnValue = new List<vs_aziende_osservamercati>();
            using (DemoR2Entities context = new DemoR2Entities())
            {
                returnValue = context.vs_aziende_osservamercati.OrderBy(o => o.m01_ragionesociale).ToList();
            }
            return returnValue;
        }


        public static List<ElementoAggiornamento> ElencoAggiornamentoOperatoriCentro(int idCentro, string idAzienda)
        {
            List<ElementoAggiornamento> returnValue = new List<ElementoAggiornamento>();

            using (DemoR2Entities context = new DemoR2Entities())
            {
                if (!idAzienda.Equals(""))
                {
                    returnValue = context.vs_aziende_osservamercati
                    .Where(om => om.m01_idcentro == idCentro && om.m01_codice.Equals(idAzienda))
                    .Select(a => new ElementoAggiornamento()
                    {
                        Codice = a.m01_codice,
                        PartitaIva = a.m01_partitaiva,
                        RagioneSociale = a.m01_ragionesociale,
                        TipoGestionale = a.m01_tipo_gestionale,
                        Gestionale = a.m01_gestionale,
                        UltimoSaldo = context.m03_saldi.Where(s => s.m03_codiceazienda == a.m01_codice)
                                       .OrderByDescending(ord => ord.m03_dtsaldo).FirstOrDefault() != null ?
                                       context.m03_saldi.Where(s => s.m03_codiceazienda == a.m01_codice)
                                       .OrderByDescending(ord => ord.m03_dtsaldo).FirstOrDefault().m03_dtsaldo : DateTime.MinValue,
                        UltimaVendita = context.m04_vendite.Where(s => s.m04_codiceazienda == a.m01_codice)
                            .OrderByDescending(ord => ord.m04_dtdocvendita).FirstOrDefault() != null ?
                            context.m04_vendite.Where(s => s.m04_codiceazienda == a.m01_codice)
                            .OrderByDescending(ord => ord.m04_dtdocvendita).FirstOrDefault().m04_dtdocvendita : DateTime.MinValue,
                        UltimaChiusura = context.m04_vendite.Where(s => s.m04_codiceazienda == a.m01_codice)
                            .OrderByDescending(ord => ord.m04_dtchiusura).FirstOrDefault() != null ?
                            context.m04_vendite.Where(s => s.m04_codiceazienda == a.m01_codice)
                            .OrderByDescending(ord => ord.m04_dtchiusura).FirstOrDefault().m04_dtchiusura : DateTime.MinValue
                    }).ToList();
                }
                else
                {
                    returnValue = context.vs_aziende_osservamercati
                    .Where(om => om.m01_idcentro == idCentro)
                    .Select(a => new ElementoAggiornamento()
                    {
                        Codice = a.m01_codice,
                        PartitaIva = a.m01_partitaiva,
                        RagioneSociale = a.m01_ragionesociale,
                        TipoGestionale = a.m01_tipo_gestionale,
                        Gestionale = a.m01_gestionale,
                        UltimoSaldo = context.m03_saldi.Where(s => s.m03_codiceazienda == a.m01_codice)
                                       .OrderByDescending(ord => ord.m03_dtsaldo).FirstOrDefault() != null ?
                                       context.m03_saldi.Where(s => s.m03_codiceazienda == a.m01_codice)
                                       .OrderByDescending(ord => ord.m03_dtsaldo).FirstOrDefault().m03_dtsaldo : DateTime.MinValue,
                        UltimaVendita = context.m04_vendite.Where(s => s.m04_codiceazienda == a.m01_codice)
                            .OrderByDescending(ord => ord.m04_dtdocvendita).FirstOrDefault() != null ?
                            context.m04_vendite.Where(s => s.m04_codiceazienda == a.m01_codice)
                            .OrderByDescending(ord => ord.m04_dtdocvendita).FirstOrDefault().m04_dtdocvendita : DateTime.MinValue,
                        UltimaChiusura = context.m04_vendite.Where(s => s.m04_codiceazienda == a.m01_codice)
                            .OrderByDescending(ord => ord.m04_dtchiusura).FirstOrDefault() != null ?
                            context.m04_vendite.Where(s => s.m04_codiceazienda == a.m01_codice)
                            .OrderByDescending(ord => ord.m04_dtchiusura).FirstOrDefault().m04_dtchiusura : DateTime.MinValue
                    }).ToList();
                }
            }
            return returnValue;
        }



        public static List<ElementoReportFasce> GetReportFasceGiorni(int idUtente, int idRuolo, int idCentro, string ambiente)
        {
            List<ElementoReportFasce> returnValue = new List<ElementoReportFasce>();
            string stringaActualDSO = string.Format("{0:yyyy-MM}", DateTime.Now);
            DateTime now = DateTime.Today;
            var lastDateofCurrentMonth = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month));

            DateTime dataTemp = DateTime.Now.AddYears(-1);
            DateTime limiteDataDoc = new DateTime(dataTemp.Year, dataTemp.Month, 1);

            using (DemoR2Entities context = new DemoR2Entities())
            {
                IQueryable<m15_preferiti> preferiti = Enumerable.Empty<m15_preferiti>().AsQueryable();
                switch (idRuolo)
                {
                    /*CASO SUPERADMIN PRENDE TUTTI I PREFERITI DI TUTTI GLI UTENTI*/
                    case 0:
                        preferiti = context.m15_preferiti;
                        break;
                    /*CASO ADMIN PRENDE TUTTI I PREFERITI DI TUTTI GLI UTENTI DEL MERCATO*/
                    case 1:
                        if (ambiente == "STANDARD")
                        {
                            preferiti = context.m15_preferiti.Join(
                                context.m10_utenti,
                                a => a.m15_iduser,
                                b => b.m10_idazienda,
                                (a, b) => new { Preferito = a, idAzienda = b.m10_idazienda })
                                .Join(context.vs_aziende_osservamercati.Where(az => az.m01_idcentro == idCentro),
                                a => a.idAzienda,
                                b => b.m01_idazienda,
                                (a, b) => a.Preferito);
                        }
                        else
                        {
                            preferiti = context.m15_preferiti.Join(
                                context.m10_utenti,
                                a => a.m15_iduser,
                                b => b.m10_idazienda,
                                (a, b) => new { Preferito = a, idAzienda = b.m10_idazienda })
                                .Join(context.vs_aziende_osservacrediti.Where(az => az.m01_idcentro == idCentro),
                                a => a.idAzienda,
                                b => b.m01_idazienda,
                                (a, b) => a.Preferito);
                        }
                        break;
                    case 2:
                        /*CASO USER PRENDE TUTTI I PREFERITI DI DELL'AZIENDA DI RIFERIMENTO DELL'UTENTE LOGGATO*/
                        preferiti = context.m15_preferiti.Where(id => id.m15_iduser == idUtente);
                        break;

                }

                ElementoOperatore aziendaUtente = GetAziendaUtente(idUtente, context, ambiente);
                int mese = DateTime.Now.Month;
                int anno = DateTime.Now.Year;
                var buffer =
                    preferiti
                    .Select(sel =>
                        new
                        {
                            PartitaIva = sel.m15_partitaiva,
                            Esposizone =
                            context.m03_saldi.Where(sal => sal.m03_codiceazienda == aziendaUtente.CodiceAzienda && sal.m03_partitaiva == sel.m15_partitaiva).OrderByDescending(saldo => saldo.m03_dtsaldo).FirstOrDefault() != null ?
                            context.m03_saldi.Where(sal => sal.m03_codiceazienda == aziendaUtente.CodiceAzienda && sal.m03_partitaiva == sel.m15_partitaiva).OrderByDescending(saldo => saldo.m03_dtsaldo).FirstOrDefault().m03_saldo : 0,
                            Giorni =
                                context.m04_vendite
                                    .Where(vend => vend.m04_codiceazienda == aziendaUtente.CodiceAzienda && vend.m04_partitaiva == sel.m15_partitaiva && vend.m04_dtdocvendita >= limiteDataDoc && vend.m04_importo > 0 && (vend.m04_chiusura.HasValue ? vend.m04_chiusura.Value == false : false))
                                    .Select(selez => new { DifferenzaVendita = System.Data.Objects.EntityFunctions.DiffDays(selez.m04_dtdocvendita, DateTime.Now) })
                                    .Average(av => av.DifferenzaVendita)
                        })
                        .Where(elab => elab.Giorni != null)
                        .Distinct().Where(esp => esp.Esposizone > 0).ToList();
                /*< 30*/
                ElementoReportFasce elementoReport = new ElementoReportFasce();
                elementoReport.Fascia = "Sotto i 60 Giorni";
                elementoReport.Companies = buffer.Where(b => b.Giorni < 60).Count();
                returnValue.Add(elementoReport);
                /*TRA 30 e 150*/
                elementoReport = new ElementoReportFasce();
                elementoReport.Fascia = "Tra i 60 e i 150 Giorni";
                elementoReport.Companies = buffer.Where(b => b.Giorni >= 60 && b.Giorni < 150).Count();
                returnValue.Add(elementoReport);
                /*SOPRA 150*/
                elementoReport = new ElementoReportFasce();
                elementoReport.Fascia = "Oltre i 150 giorni";
                elementoReport.Companies = buffer.Where(b => b.Giorni > 150).Count();
                returnValue.Add(elementoReport);

            }


            return returnValue;

        }

        public static List<ElementoReportFasce> GetReportFasceVendite(int idUtente, int idRuolo, int idCentro, string ambiente)
        {
            List<ElementoReportFasce> returnValue = new List<ElementoReportFasce>();
            DateTime dataTemp = DateTime.Now.AddYears(-1);
            DateTime limiteDataDoc = new DateTime(dataTemp.Year, dataTemp.Month, 1);
            using (DemoR2Entities context = new DemoR2Entities())
            {

                IQueryable<m15_preferiti> preferiti = Enumerable.Empty<m15_preferiti>().AsQueryable();
                switch (idRuolo)
                {
                    /*CASO SUPERADMIN PRENDE TUTTI I PREFERITI DI TUTTI GLI UTENTI*/
                    case 0:
                        preferiti = context.m15_preferiti;
                        break;
                    /*CASO ADMIN PRENDE TUTTI I PREFERITI DI TUTTI GLI UTENTI DEL MERCATO*/
                    case 1:
                        if (ambiente == "STANDARD")
                        {
                            preferiti = context.m15_preferiti.Join(
                                context.m10_utenti,
                                a => a.m15_iduser,
                                b => b.m10_idazienda,
                                (a, b) => new { Preferito = a, idAzienda = b.m10_idazienda })
                                .Join(context.vs_aziende_osservamercati.Where(az => az.m01_idcentro == idCentro),
                                a => a.idAzienda,
                                b => b.m01_idazienda,
                                (a, b) => a.Preferito);
                        }
                        else
                        {
                            preferiti = context.m15_preferiti.Join(
                             context.m10_utenti,
                             a => a.m15_iduser,
                             b => b.m10_idazienda,
                             (a, b) => new { Preferito = a, idAzienda = b.m10_idazienda })
                             .Join(context.vs_aziende_osservacrediti.Where(az => az.m01_idcentro == idCentro),
                             a => a.idAzienda,
                             b => b.m01_idazienda,
                             (a, b) => a.Preferito);
                        }
                        break;
                    case 2:
                        /*CASO USER PRENDE TUTTI I PREFERITI DI DELL'AZIENDA DI RIFERIMENTO DELL'UTENTE LOGGATO*/
                        preferiti = context.m15_preferiti.Where(id => id.m15_iduser == idUtente);
                        break;

                }

                ElementoOperatore aziendaUtente = GetAziendaUtente(idUtente, context, ambiente);

                var buffer =
                    preferiti
                    .Select(sel =>
                        new
                        {
                            PartitaIva = sel.m15_partitaiva,
                            Esposizone =
                            context.m03_saldi.Where(sal => sal.m03_codiceazienda == aziendaUtente.CodiceAzienda && sal.m03_partitaiva == sel.m15_partitaiva).OrderByDescending(saldo => saldo.m03_dtsaldo).FirstOrDefault() != null ?
                            context.m03_saldi.Where(sal => sal.m03_codiceazienda == aziendaUtente.CodiceAzienda && sal.m03_partitaiva == sel.m15_partitaiva).OrderByDescending(saldo => saldo.m03_dtsaldo).FirstOrDefault().m03_saldo : 0,
                            Vendite =
                            context.m04_vendite
                                .Where(ven => ven.m04_codiceazienda == aziendaUtente.CodiceAzienda && ven.m04_partitaiva == sel.m15_partitaiva && ven.m04_dtdocvendita >= limiteDataDoc && ven.m04_importo > 0).FirstOrDefault() != null ?
                                context.m04_vendite.Where(ven => ven.m04_codiceazienda == aziendaUtente.CodiceAzienda && ven.m04_dtdocvendita >= limiteDataDoc && ven.m04_partitaiva == sel.m15_partitaiva && ven.m04_importo > 0).Sum(som => som.m04_importo) : 0

                        }).Distinct().Where(esp => esp.Esposizone > 0).ToList();
                /*< 20.000*/
                ElementoReportFasce elementoReport = new ElementoReportFasce();
                elementoReport.Fascia = "Sotto i 20.000";
                elementoReport.Companies = buffer.Where(b => b.Vendite < 20000).Count();
                returnValue.Add(elementoReport);
                /*Tra i 20.000 e i 100.000*/
                elementoReport = new ElementoReportFasce();
                elementoReport.Fascia = "Tra i 20.000 e i 100.000";
                elementoReport.Companies = buffer.Where(b => b.Vendite >= 20000 && b.Vendite < 100000).Count();
                returnValue.Add(elementoReport);
                /*Tra i 100.000 e i 400.000*/
                elementoReport = new ElementoReportFasce();
                elementoReport.Fascia = "Tra i 100.000 e i 400.000";
                elementoReport.Companies = buffer.Where(b => b.Vendite >= 100000 && b.Vendite < 400000).Count();
                returnValue.Add(elementoReport);
                /*Tra i 100.000 e i 400.000*/
                elementoReport = new ElementoReportFasce();
                elementoReport.Fascia = "Sopra i 400.000";
                elementoReport.Companies = buffer.Where(b => b.Vendite >= 400000).Count();
                returnValue.Add(elementoReport);

            }


            return returnValue;

        }

        public static DataSet getDatiNazione(String ragsoc, String codfis, string connectionString)
        {

            codfis = codfis != String.Empty ? codfis : "";
            DataSet ds = new DataSet();
            try
            {
                using (SqlConnection conn = OpenSqlConn(connectionString))
                {
                    SqlCommand cmd = new SqlCommand("getDatiNazione", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ragsoc", ragsoc);
                    cmd.Parameters.AddWithValue("@codfis", codfis);
                    cmd.CommandTimeout = 0;

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(ds);
                    CloseConn(conn);
                }
            }
            catch (Exception e)
            {
                Log.Error("error : " + e.ToString());

            }

            return ds;
        }

        public static DataSet getDatiAzienda(String ragsoc, String codfis, string codiceAzienda, string connectionString)
        {

            codfis = codfis != String.Empty ? codfis : "";
            DataSet ds = new DataSet();
            try
            {
                SqlCommand cmd = null;
                using (SqlConnection conn = OpenSqlConn(connectionString))
                {
                    cmd = new SqlCommand("getDatiAziendaR3", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ragsoc", ragsoc);
                    cmd.Parameters.AddWithValue("@codfis", codfis);
                    cmd.Parameters.AddWithValue("@codAz", codiceAzienda);
                    cmd.CommandTimeout = 0;

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(ds);
                    CloseConn(conn);
                }
            }
            catch (Exception e)
            {
                Log.Error("error : " + e.ToString());

            }

            return ds;
        }

        public static bool VerificaPermessiUpdate(int idUtente, string partitaIva, int idRuolo)
        {
            bool returnValue = false;
            if (idRuolo == 0)
                return true;
            try
            {
                using (DemoR2Entities context = new DemoR2Entities())
                {
                    cerved_check rigaCheck =
                        context.cerved_check.Where(check => check.id_utente == idUtente && check.partita_iva == partitaIva)
                        .OrderByDescending(d => d.dt_inserimento)
                        .FirstOrDefault();
                    returnValue = rigaCheck != null;
                }

            }
            catch (Exception e)
            {
                Log.Error("error : " + e.ToString());
            }
            return returnValue;
        }

        public static bool VerificaPermessiReport(string codiceCerved, int idUtente, int idRuolo, int idMercato)
        {
            bool returnValue = false;
            codiceCerved = codiceCerved.Replace("CDWS_", "");
            if (idRuolo == 0)
                return true;
            try
            {
                using (DemoR2Entities context = new DemoR2Entities())
                {
                    cerved_check rigaCheck =
                        context.cerved_check.Where(check => check.id_utente == idUtente && check.codice_cerved == codiceCerved)
                        .OrderByDescending(d => d.dt_inserimento)
                        .FirstOrDefault();
                    returnValue = rigaCheck != null;
                }

            }
            catch (Exception e)
            {
                Log.Error("error : " + e.ToString());
            }
            return returnValue;
        }

        public static double TrovaVenditeMese(string partitaIva, int mese, int anno, int idCentro)
        {
            double returnValue = 0;
            try
            {
                using (DemoR2Entities context = new DemoR2Entities())
                {
                    returnValue =
                    idCentro == 0 ?
                        context.m04_vendite.Where((o) => o.m04_partitaiva.IndexOf(partitaIva) > -1 &&
                            (
                            o.m04_dtdocvendita.Month == mese &&
                            o.m04_dtdocvendita.Year == anno
                            )
                            ).Sum((o) => o.m04_importo).HasValue ?
                            context.m04_vendite.Where((o) => o.m04_partitaiva.IndexOf(partitaIva) > -1 &&
                            (
                            o.m04_dtdocvendita.Month == mese &&
                            o.m04_dtdocvendita.Year == anno
                            )
                            ).Sum((o) => o.m04_importo).Value : 0 :

                            context.m04_vendite.Where((o) => o.m04_partitaiva.IndexOf(partitaIva) > -1 &&
                            (
                            o.m04_dtdocvendita.Month == mese &&
                            o.m04_dtdocvendita.Year == anno
                            )
                            )
                            .Join(context.vs_aziende_osservamercati.Where(az => az.m01_idcentro == idCentro),
                            a => a.m04_codiceazienda,
                            b => b.m01_codice,
                            (a, b) => a)
                            .Sum((o) => o.m04_importo).HasValue ?
                            context.m04_vendite.Where((o) => o.m04_partitaiva.IndexOf(partitaIva) > -1 &&
                            (
                            o.m04_dtdocvendita.Month == mese &&
                            o.m04_dtdocvendita.Year == anno
                            )
                            ).Sum((o) => o.m04_importo).Value : 0;

                }
            }
            catch (Exception e)
            {
                Log.Error("error : " + e.ToString());
            }

            return returnValue;

        }

        public static Decimal TrovaEsposizioneMese(string partitaIva, DateTime dataInizioRicercaSaldi, int meseCorrente, int annoCorrente, int idCentro)
        {

            Decimal returnValue = 0;

            DateTime dataUltimoGiornoMeseCorrente = new DateTime(annoCorrente, meseCorrente, 1).AddMonths(1).AddDays(-1);
            decimal saldo = 0;
            using (DemoR2Entities context = new DemoR2Entities())
            {

                try
                {
                    var listaDistinctPiva =
                        idCentro == 0 ?
                            context.m03_saldi
                    .Where((o) => (o.m03_dtsaldo >= dataInizioRicercaSaldi && o.m03_dtsaldo <= dataUltimoGiornoMeseCorrente)
                                                && o.m03_partitaiva == partitaIva && o.m03_saldo >= 0)
                                                .Select((o) => o.m03_codiceazienda).Distinct() :
                        context.m03_saldi

                    .Where((o) => (o.m03_dtsaldo >= dataInizioRicercaSaldi && o.m03_dtsaldo <= dataUltimoGiornoMeseCorrente)

                                                && o.m03_partitaiva == partitaIva && o.m03_saldo >= 0)
                                                .Join(context.vs_aziende_osservamercati.Where(az => az.m01_idcentro == idCentro),
                    a => a.m03_codiceazienda,
                    b => b.m01_codice,
                    (a, b) => a)
                    .Select((o) => o.m03_codiceazienda).Distinct();

                    var bufferExpression =
                    listaDistinctPiva.Select((codiceOperatore) =>
                        new
                        {
                            codiceOperatore,
                            primoSaldo =

                            context.m03_saldi
                            .Where((o) => o.m03_codiceazienda == codiceOperatore &&
                            (o.m03_dtsaldo >= dataInizioRicercaSaldi && o.m03_dtsaldo <= dataUltimoGiornoMeseCorrente) && o.m03_partitaiva == partitaIva && o.m03_saldo >= 0)
                                .OrderByDescending((o) => o.m03_dtsaldo)
                            .FirstOrDefault() != null ?

                            context.m03_saldi
                            .Where((o) => o.m03_codiceazienda == codiceOperatore &&
                            (o.m03_dtsaldo >= dataInizioRicercaSaldi && o.m03_dtsaldo <= dataUltimoGiornoMeseCorrente) && o.m03_partitaiva == partitaIva && o.m03_saldo >= 0)
                            .OrderByDescending((o) => o.m03_dtsaldo)
                            .FirstOrDefault().m03_saldo :
                            0
                        }
                            ).ToList();

                    saldo = bufferExpression.Sum((o) => o.primoSaldo);

                }
                catch (Exception e)
                {
                    Log.Error("error : " + e.ToString());
                }
            }

            returnValue = saldo;

            return returnValue;
        }

        public static DataSet GetDatiCentro(String idcentro, String ragsoc, String codfis, string connectionString)
        {
            codfis = codfis != String.Empty ? codfis : "";
            DataSet ds = new DataSet();
            try
            {
                using (SqlConnection conn = DBHandler.OpenSqlConn(connectionString))
                {
                    SqlCommand cmd = new SqlCommand("getDatiCentro", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@idcentro", idcentro);
                    cmd.Parameters.AddWithValue("@ragsoc", ragsoc);
                    cmd.Parameters.AddWithValue("@codfis", codfis);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(ds);
                    CloseConn(conn);
                }
            }
            catch (Exception e)
            {
                Log.Error("error : " + e.ToString());

            }
            return ds;
        }

        public static ElementoAnagraficheOsservatorio GetAziendaOsservatorio(String piva)
        {
            ElementoAnagraficheOsservatorio returnValue = new ElementoAnagraficheOsservatorio();

            try
            {
                using (DemoR2Entities context = new DemoR2Entities())
                {
                    List<m02_anagrafica> anagrafica =
                        context.m02_anagrafica.Where(an => an.m02_partitaiva == piva).ToList();

                    if (anagrafica.Count > 0)
                    {
                        returnValue = anagrafica.Select(an => new ElementoAnagraficheOsservatorio
                        {
                            PartitaIva = an.m02_partitaiva,
                            RagioneSociale = an.m02_ragionesociale
                        }
                        ).FirstOrDefault();
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error("error : " + e.ToString());

            }
            return returnValue;
        }

        public static ElementoAnagraficheOsservatorio getAziendeOperatori(String piva, int mercato)
        {
            ElementoAnagraficheOsservatorio returnValue = new ElementoAnagraficheOsservatorio();

            try
            {
                using (DemoR2Entities context = new DemoR2Entities())
                {
                    List<vs_aziende_osservamercati> operatori =
                        context.vs_aziende_osservamercati.Where(an => an.m01_partitaiva == piva && an.m01_idcentro == mercato).ToList();

                    if (operatori.Count > 0)
                    {
                        returnValue = operatori.Select(an => new ElementoAnagraficheOsservatorio
                        {
                            PartitaIva = an.m01_partitaiva,
                            RagioneSociale = an.m01_ragionesociale
                        }
                        ).FirstOrDefault();
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error("error : " + e.ToString());

            }
            return returnValue;
        }


        public static SqlConnection OpenSqlConn(string connectionString)
        {

            SqlConnection conn = new SqlConnection(connectionString);
            try
            {
                conn.Open();

            }
            catch (Exception e)
            {
                Log.Error("error : " + e.ToString());

            }
            return conn;
        }

        public static void CloseConn(SqlConnection conn)
        {
            try
            {
                conn.Close();
            }
            catch (Exception e)
            {
                Log.Error("error : " + e.ToString());

            }
        }
        public static List<ElementoReportFasce> GetReportFasceEsposizione(int idUtente, int idRuolo, int idCentro, string ambiente)
        {
            List<ElementoReportFasce> returnValue = new List<ElementoReportFasce>();
            using (DemoR2Entities context = new DemoR2Entities())
            {
                IQueryable<m15_preferiti> preferiti = Enumerable.Empty<m15_preferiti>().AsQueryable();
                switch (idRuolo)
                {
                    /*CASO SUPERADMIN PRENDE TUTTI I PREFERITI DI TUTTI GLI UTENTI*/
                    case 0:
                        preferiti = context.m15_preferiti;
                        break;
                    /*CASO ADMIN PRENDE TUTTI I PREFERITI DI TUTTI GLI UTENTI DEL MERCATO*/
                    case 1:
                        if (ambiente == "STANDARD")
                        {
                            preferiti = context.m15_preferiti.Join(
                                context.m10_utenti,
                                a => a.m15_iduser,
                                b => b.m10_idazienda,
                                (a, b) => new { Preferito = a, idAzienda = b.m10_idazienda })
                                .Join(context.vs_aziende_osservamercati.Where(az => az.m01_idcentro == idCentro),
                                a => a.idAzienda,
                                b => b.m01_idazienda,
                                (a, b) => a.Preferito);
                        }
                        else
                        {
                            preferiti = context.m15_preferiti.Join(
                            context.m10_utenti,
                            a => a.m15_iduser,
                            b => b.m10_idazienda,
                            (a, b) => new { Preferito = a, idAzienda = b.m10_idazienda })
                            .Join(context.vs_aziende_osservacrediti.Where(az => az.m01_idcentro == idCentro),
                            a => a.idAzienda,
                            b => b.m01_idazienda,
                            (a, b) => a.Preferito);
                        }
                        break;
                    case 2:
                        /*CASO USER PRENDE TUTTI I PREFERITI DI DELL'AZIENDA DI RIFERIMENTO DELL'UTENTE LOGGATO*/
                        preferiti = context.m15_preferiti.Where(id => id.m15_iduser == idUtente);
                        break;

                }


                ElementoOperatore aziendaUtente = GetAziendaUtente(idUtente, context, ambiente);
                var buffer =
                    preferiti
                    .Select(sel =>
                        new
                        {
                            PartitaIva = sel.m15_partitaiva,
                            Esposizone =
                            context.m03_saldi.Where(sal => sal.m03_codiceazienda == aziendaUtente.CodiceAzienda && sal.m03_partitaiva == sel.m15_partitaiva).OrderByDescending(saldo => saldo.m03_dtsaldo).FirstOrDefault() != null ?
                            context.m03_saldi.Where(sal => sal.m03_codiceazienda == aziendaUtente.CodiceAzienda && sal.m03_partitaiva == sel.m15_partitaiva).OrderByDescending(saldo => saldo.m03_dtsaldo).FirstOrDefault().m03_saldo : 0
                        }).Distinct().Where(esp => esp.Esposizone > 0).ToList();


                /*< 1.000*/
                ElementoReportFasce elementoReport = new ElementoReportFasce();
                elementoReport.Fascia = "Sotto i 1.000";
                elementoReport.Companies = buffer.Where(b => b.Esposizone < 1000).Count();
                returnValue.Add(elementoReport);
                /*Tra i 1.000 e i 5.000*/
                elementoReport = new ElementoReportFasce();
                elementoReport.Fascia = "Tra i 1.000 e i 5.000";
                elementoReport.Companies = buffer.Where(b => b.Esposizone >= 1000 && b.Esposizone < 5000).Count();
                returnValue.Add(elementoReport);
                /*Tra i 5.000 e i 25.000*/
                elementoReport = new ElementoReportFasce();
                elementoReport.Fascia = "Tra i 5.000 e i 25.000";
                elementoReport.Companies = buffer.Where(b => b.Esposizone >= 5000 && b.Esposizone < 25000).Count();
                returnValue.Add(elementoReport);
                /*Tra i 25.000 e i 100.000*/
                elementoReport = new ElementoReportFasce();
                elementoReport.Fascia = "Tra i 25.000 e i 100.000";
                elementoReport.Companies = buffer.Where(b => b.Esposizone >= 25000 && b.Esposizone < 100000).Count();
                returnValue.Add(elementoReport);
                /*Sopra 100.000*/
                elementoReport = new ElementoReportFasce();
                elementoReport.Fascia = "Sopra i 100.000";
                elementoReport.Companies = buffer.Where(b => b.Esposizone >= 100000).Count();
                returnValue.Add(elementoReport);

            }


            return returnValue;

        }
        public static HttpWebResponse CompanyReportPdf(string id, string token)
        {

            Log.Info("CompanyReportPdf Start ----- id : " + id);

            string URI = "https://connect.creditsafe.com/v1/companies/";
            URI = URI + id;
            if (id.Substring(0, 2).Equals("IT"))
            {
                URI = URI + "?template=complete";
            }
            Log.Info("CompanyReportPdf request : " + URI.ToString());
            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(URI.ToString());
                httpWebRequest.ContentType = "application/pdf";
                httpWebRequest.Method = "GET";
                httpWebRequest.Accept = "application/pdf";
                httpWebRequest.Headers.Add("Authorization", token);
                return (HttpWebResponse)httpWebRequest.GetResponse();
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                MailHandler.SendMail("support@osservamercati.it", "CompanyReportPdf" + ex.ToString());
                return null;
            }
        }

        public static string CompanyComplete(string id, string token)
        {
            Log.Info("CompanyComplete Start ----id :" + id);
            string response = "";
            string URI = "https://connect.creditsafe.com/v1/companies/";
            URI = URI + id;
            if (id.Substring(0, 2).Equals("IT"))
            {
                URI = URI + "?template=complete";
            }
            try
            {
                Log.Info("CompanyComplete request : " + URI.ToString());
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(URI.ToString());
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "GET";
                httpWebRequest.Headers.Add("Authorization", token);
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var resultws = streamReader.ReadToEnd();
                    response = resultws;
                    //Log.Debug("result companycomplete = " + resultws);
                    //Log.Debug("response companycomplete = " + response.ToString());
                }
            }
            catch (Exception ex)
            {
                Log.Error("errore : " + ex.ToString());
            }
            return response;
        }
        public static string GetIdGlobalReport(string piva, string token)
        {
            Log.Info("GetIdGlobalReport Start ---- P.iva : " + piva);
            string id = "";
            string nation = "IT";
            if (piva.Length < 12)
            {
                if (!piva.Substring(0, 1).Any(char.IsDigit))
                {
                    nation = piva.Substring(0, 2);
                    piva = piva.Substring(2);
                }
            }

            try
            {
                string URI = "https://connect.creditsafe.com/v1/companies";
                URI = URI + "?countries=" + nation + "&vatNo=" + piva;
                Log.Info("URI " + URI);
                Log.Info("token : " + token);
                var httpWebRequest2 = (HttpWebRequest)WebRequest.Create(URI);
                httpWebRequest2.ContentType = "application/json";
                httpWebRequest2.Method = "GET";
                httpWebRequest2.Headers.Add("Authorization", token);
                var httpResponse2 = (HttpWebResponse)httpWebRequest2.GetResponse();
                JsonSerializerSettings settings = new JsonSerializerSettings();
                settings.NullValueHandling = NullValueHandling.Ignore;
                using (var streamReader = new StreamReader(httpResponse2.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    dynamic response = JsonConvert.DeserializeObject(result.ToString(), settings);

                    try { id = response.companies[0].id; } catch { id = null; }
                }
                Log.Info("end GlobalReportId -- ID= " + id);
            }
            catch (WebException ex)
            {
                Log.Error("errore : " + ex.ToString());
                using (WebResponse response = ex.Response)
                {
                    HttpWebResponse httpResponse2 = (HttpWebResponse)response;
                    using (Stream data = response.GetResponseStream())
                    using (var reader = new StreamReader(data))
                    {
                        // text is the response body
                        string text = reader.ReadToEnd();
                    }
                }
            }
            return id;
        }

        [Obsolete]
        public static string LoginCS(SecurityProtocolType protocolloTrasporto)
        {
            Log.Info("begin LoginCs");
            ServicePointManager.SecurityProtocol = protocolloTrasporto;
            string username = ConfigurationManager.AppSettings["UsernameWsCs"];
            string password = ConfigurationManager.AppSettings["PasswordWsCs"];
            var token = "";

            var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://connect.creditsafe.com/v1/authenticate");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            //WriteLog("username : " + username+"password : " +password);

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string json = "{\"username\":\"" + username + "\",\"password\":\"" + password + "\"}";
                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }
            try
            {
                WebResponse httpResponse = httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    dynamic response = JsonConvert.DeserializeObject(result.ToString());
                    token = response.token;
                }
            }
            catch (WebException ex)
            {
                Log.Info("errore LoginCs " + ex.ToString());
                using (WebResponse response = ex.Response)
                {
                    HttpWebResponse httpResponse2 = (HttpWebResponse)response;
                    //  using (Stream data = response.GetResponseStream())
                }
            }
            Log.Info("token : " + token.ToString());
            Log.Info("end LoginCs");
            return token;


        }
        public static List<ElementoReport> GetListReportOpe(int idUtente, int idCentro)
        {
            List<ElementoReport> reportElement = new List<ElementoReport>();
            ElementoReport elemento = new ElementoReport();
            int countMese = 1;
            using (DemoR2Entities context = new DemoR2Entities())
            {
                try
                {
                    List<richiesta_report> rich = context.richiesta_report.Where(rr => rr.id_utente == idUtente).ToList();
                    List<richiesta_report> result = rich.OrderBy(_ => _.data_richiesta).ToList();
                    int primomese = Int16.Parse(result[0].data_richiesta.Month.ToString());
                    elemento.meseRiferimento = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(primomese);
                    elemento.annoRiferimento = result[0].data_richiesta.Year.ToString();
                    elemento.RagioneSociale = context.m10_utenti.Where(ute => ute.m10_iduser == idUtente).Select(sel => sel.m10_nomeutente).FirstOrDefault();
                    elemento.RichEffettuate = countMese;
                    int contReport = Convert.ToInt32(context.m10_utenti
                                          .Where(_ => _.m10_iduser == idUtente)
                                          .Select(sel => sel.m10_num_report)
                                          .FirstOrDefault() - elemento.RichEffettuate);

                    elemento.Residui = contReport > 0 ? contReport.ToString() : "0";
                    int i = 1;
                    for (int ii = 1; ii < result.Count; ii++)
                    {
                        if (Int16.Parse(result[ii].data_richiesta.Month.ToString()) == primomese)
                        {
                            elemento = new ElementoReport();
                            primomese = Int16.Parse(result[ii].data_richiesta.Month.ToString());
                            elemento.meseRiferimento = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(primomese);
                            elemento.annoRiferimento = result[ii].data_richiesta.Year.ToString();
                            elemento.RagioneSociale = context.m10_utenti.Where(ute => ute.m10_iduser == idUtente).Select(sel => sel.m10_nomeutente).FirstOrDefault();
                            countMese++;
                            elemento.RichEffettuate = countMese;
                        }
                        else
                        {
                            reportElement.Add(elemento);
                            countMese = 0;
                            elemento = new ElementoReport();
                            primomese = Int16.Parse(result[ii].data_richiesta.Month.ToString());
                            elemento.meseRiferimento = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(primomese);
                            elemento.annoRiferimento = result[ii].data_richiesta.Year.ToString();
                            elemento.RagioneSociale = context.m10_utenti.Where(ute => ute.m10_iduser == idUtente).Select(sel => sel.m10_nomeutente).FirstOrDefault();
                            countMese++;
                            elemento.RichEffettuate = countMese;
                        }
                        contReport = Convert.ToInt32(context.m10_utenti
                                          .Where(_ => _.m10_iduser == idUtente)
                                          .Select(sel => sel.m10_num_report)
                                          .FirstOrDefault() - elemento.RichEffettuate);

                        elemento.Residui = contReport > 0 ? contReport.ToString() : "0";
                        /*                    
                                            elemento.RichEffettuate = context.richiesta_report.Where(ric => ric.id_utente == el.id_utente).Count();
                                            elemento.PartitaIva =
                                                            context.vs_aziende_osservamercati.Where(
                                                                                                     az => az.m01_idazienda == (context.m10_utenti.Where
                                                                                                        (ut => ut.m10_iduser == el.id_utente).Select
                                                                                                        (_ => _.m10_idazienda).FirstOrDefault())
                                                                                                   ).Select(aa => aa.m01_partitaiva).FirstOrDefault();
                                            elemento.RagioneSociale =
                                                           context.vs_aziende_osservamercati.Where(
                                                                                                    az => az.m01_idazienda == (context.m10_utenti.Where
                                                                                                       (ut => ut.m10_iduser == el.id_utente).Select
                                                                                                       (_ => _.m10_idazienda).FirstOrDefault())
                                                                                                  ).Select(aa => aa.m01_ragionesociale).FirstOrDefault();
                                            elemento.DataRichiesta = "30";//WebConfigurationManager.AppSettings["richiesteReport"];
                                            elemento.Residui = (int.Parse(elemento.DataRichiesta) - elemento.RichEffettuate).ToString();
                                            reportElement.Add(elemento);*/
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e.ToString());
                }
                reportElement.Add(elemento);
            }
            return reportElement;
        }

        public static List<ElementoReport> GetListReport(int idUtente, int idCentro)
        {
            List<ElementoReport> reportElement = new List<ElementoReport>();
            using (DemoR2Entities context = new DemoR2Entities())
            {
                List<richiesta_report> rich = context.richiesta_report.Where(rr => rr.id_centro == idCentro).ToList();
                List<richiesta_report> result = rich.GroupBy(_ => _.id_utente).Select(rr => rr.First()).ToList();
                foreach (richiesta_report el in result)
                {
                    ElementoReport elemento = new ElementoReport();
                    elemento.RichEffettuate = context.richiesta_report.Where(ric => ric.id_utente == el.id_utente).Count();
                    elemento.PartitaIva =
                                    context.vs_aziende_osservamercati.Where(
                                                                             az => az.m01_idazienda == (context.m10_utenti.Where
                                                                                (ut => ut.m10_iduser == el.id_utente).Select
                                                                                (_ => _.m10_idazienda).FirstOrDefault())
                                                                           ).Select(aa => aa.m01_partitaiva).FirstOrDefault();
                    elemento.RagioneSociale =
                                   context.vs_aziende_osservamercati.Where(
                                                                            az => az.m01_idazienda == (context.m10_utenti.Where
                                                                               (ut => ut.m10_iduser == el.id_utente).Select
                                                                               (_ => _.m10_idazienda).FirstOrDefault())
                                                                          ).Select(aa => aa.m01_ragionesociale).FirstOrDefault();
                    elemento.DataRichiesta = Convert.ToString(context.m10_utenti.Where(_ => _.m10_iduser == idUtente).Select(sel => sel.m10_num_report).FirstOrDefault());

                    int contReport = Convert.ToInt32(context.m10_utenti
                                          .Where(_ => _.m10_iduser == el.id_utente)
                                          .Select(sel => sel.m10_num_report)
                                          .FirstOrDefault() - elemento.RichEffettuate);
                    elemento.Residui = contReport > 0 ? contReport.ToString() : "0";

                    if (contReport - elemento.RichEffettuate < 0)
                    {
                        elemento.DaFatturare = Convert.ToString(elemento.RichEffettuate - contReport);
                    }
                    else
                    {
                        elemento.DaFatturare = "0";
                    }
                    reportElement.Add(elemento);
                }

            }
            return reportElement;
        }

        public static List<ElementoPreferiti> GetPreferitiRecuperoCrediti(int idUtente, int idRuolo, int idCentro, string ambiente)
        {
            List<ElementoPreferiti> returnValue = new List<ElementoPreferiti>();
            DateTime now = DateTime.Now;
            var lastDateofCurrentMonth = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month));


            DateTime dataTemp = DateTime.Now.AddYears(-1);
            DateTime limiteDataDoc = new DateTime(dataTemp.Year, dataTemp.Month, 1);

            string stringaActualDSO = string.Format("{0:yyyy-MM}", DateTime.Now);
            IQueryable<m15_preferiti> preferiti = Enumerable.Empty<m15_preferiti>().AsQueryable();
            List<string> prefPiva = new List<string>();
            ElementoOperatore aziendaUtente = new ElementoOperatore();
            using (DemoR2Entities context = new DemoR2Entities())
            {

                /*Determinazione dei preferiti di riferimento*/
                switch (idRuolo)
                {
                    /*CASO SUPERADMIN PRENDE TUTTI I PREFERITI DI TUTTI GLI UTENTI*/
                    case 0:
                        preferiti = context.m15_preferiti;
                        break;
                    /*CASO ADMIN PRENDE TUTTI I PREFERITI DI TUTTI GLI UTENTI DEL MERCATO*/
                    case 1:
                        if (ambiente == "STANDARD")
                        {
                            preferiti = context.m15_preferiti.Join(
                                context.m10_utenti,
                                a => a.m15_iduser,
                                b => b.m10_idazienda,
                                (a, b) => new { Preferito = a, idAzienda = b.m10_idazienda })
                                .Join(context.vs_aziende_osservamercati.Where(az => az.m01_idcentro == idCentro),
                                a => a.idAzienda,
                                b => b.m01_idazienda,
                                (a, b) => a.Preferito);
                        }
                        else
                        {
                            preferiti = context.m15_preferiti.Join(
                                context.m10_utenti,
                                a => a.m15_iduser,
                                b => b.m10_idazienda,
                                (a, b) => new { Preferito = a, idAzienda = b.m10_idazienda })
                                .Join(context.vs_aziende_osservacrediti.Where(az => az.m01_idcentro == idCentro),
                                a => a.idAzienda,
                                b => b.m01_idazienda,
                                (a, b) => a.Preferito);
                        }
                        break;
                    case 2:
                        /*CASO USER PRENDE TUTTI I PREFERITI DI DELL'AZIENDA DI RIFERIMENTO DELL'UTENTE LOGGATO*/
                        aziendaUtente = GetAziendaUtente(idUtente, context, ambiente);
                        prefPiva = context.m04_vendite.Where(wr => wr.m04_dtscadenza > DateTime.Now
                                                             && wr.m04_codiceazienda == aziendaUtente.CodiceAzienda).Select(sel => sel.m04_partitaiva).ToList();
                        preferiti = context.m15_preferiti.Where(id => id.m15_iduser == idUtente && prefPiva.Contains(id.m15_partitaiva));
                        break;

                }


                /*RIMUOVERE O CAMBIARE! AGGIUNTO PASSAGGIO PER VERIFICA ANAGRAFICA VUOTA O NON TROVATA */
                var bufferValue =
                        preferiti
                        .Join(
                        context.m02_anagrafica.Where(an => an.m02_ragionesociale != String.Empty),
                        a => a.m15_partitaiva,
                        b => b.m02_partitaiva,
                        (a, b) => a)
                        .Select((clienteSaldo) =>
                            new ElementoPreferiti
                            {

                                id = clienteSaldo.m15_partitaiva,
                                PartitaIva = clienteSaldo.m15_partitaiva,
                                Rating = SqlFunctions.StringConvert((double)context.m02_anagrafica
                                                    .Where(_ => _.m02_partitaiva == clienteSaldo.m15_partitaiva)
                                                    .Select(_ => _.m02_stato_validazione_cerved).FirstOrDefault()),
                                RagioneSociale = context.m02_anagrafica.Where(anag => anag.m02_partitaiva == clienteSaldo.m15_partitaiva).FirstOrDefault() != null ? context.m02_anagrafica.Where(anag => anag.m02_partitaiva == clienteSaldo.m15_partitaiva).FirstOrDefault().m02_ragionesociale : "",
                                Vendite =
                                context.m04_vendite.Where(saldo => saldo.m04_partitaiva == clienteSaldo.m15_partitaiva && saldo.m04_dtdocvendita >= limiteDataDoc && saldo.m04_codiceazienda == aziendaUtente.CodiceAzienda && saldo.m04_importo > 0 && saldo.m04_dtscadenza < DateTime.Now)
                                                 .Sum(somma => somma.m04_importo).HasValue ?
                                context.m04_vendite.Where(saldo => saldo.m04_partitaiva == clienteSaldo.m15_partitaiva && saldo.m04_dtdocvendita >= limiteDataDoc && saldo.m04_codiceazienda == aziendaUtente.CodiceAzienda && saldo.m04_importo > 0 && saldo.m04_dtscadenza < DateTime.Now)
                                                 .Sum(somma => somma.m04_importo).Value : 0
                                                 ,
                                Esposizione =
                                context.m03_saldi.Where(saldo => saldo.m03_partitaiva == clienteSaldo.m15_partitaiva && saldo.m03_codiceazienda == aziendaUtente.CodiceAzienda)
                                                 .OrderByDescending(saldo => saldo.m03_dtsaldo).FirstOrDefault() != null ?
                                                 context.m03_saldi.Where(saldo => saldo.m03_partitaiva == clienteSaldo.m15_partitaiva && saldo.m03_codiceazienda == aziendaUtente.CodiceAzienda)
                                                 .OrderByDescending(saldo => saldo.m03_dtsaldo).FirstOrDefault().m03_saldo : 0,

                                GG =
                                context.m04_vendite
                                    .Where(vend => vend.m04_codiceazienda == aziendaUtente.CodiceAzienda && vend.m04_partitaiva == clienteSaldo.m15_partitaiva && vend.m04_dtdocvendita >= limiteDataDoc && vend.m04_importo > 0 && (vend.m04_chiusura.HasValue ? vend.m04_chiusura.Value == false : false))
                                   .Select(selez => new { DifferenzaVendita = System.Data.Objects.EntityFunctions.DiffDays(selez.m04_dtdocvendita, DateTime.Now) })
                                    .Average(av => av.DifferenzaVendita)


                            }).Distinct().Where(esp => esp.Esposizione > 0);

                returnValue = bufferValue.ToList();
            }
            return returnValue;
        }

        public static List<ElementoFatturaRecuperoCrediti> GetRiepilogoRecuperoCrediti(int idUtente, int idRuolo, int idCentro, string ambiente)
        {
            List<ElementoFatturaRecuperoCrediti> returnValue = new List<ElementoFatturaRecuperoCrediti>();



            using (DemoR2Entities context = new DemoR2Entities())
            {
                IQueryable<m26_richiesta_recupero_crediti> richiesteRC = Enumerable.Empty<m26_richiesta_recupero_crediti>().AsQueryable();


                switch (idRuolo)
                {
                    /*CASO SUPERADMIN PRENDE TUTTI I PREFERITI DI TUTTI GLI UTENTI*/
                    case 0:
                        richiesteRC = context.m26_richiesta_recupero_crediti;
                        break;
                    default:
                        ElementoOperatore aziendaUtente = GetAziendaUtente(idUtente, context, ambiente);
                        int codiceAzienda = Convert.ToInt32(aziendaUtente.CodiceAzienda);
                        /*CASO USER PRENDE TUTTI I PREFERITI DI DELL'AZIENDA DI RIFERIMENTO DELL'UTENTE LOGGATO*/
                        richiesteRC = context.m26_richiesta_recupero_crediti.Where(rc => rc.m26_codice_azienda == codiceAzienda);
                        break;

                }
                if (richiesteRC != Enumerable.Empty<m26_richiesta_recupero_crediti>().AsQueryable())
                {
                    returnValue = richiesteRC.Join(context.m27_fattura_richiesta,
                        a => a.m26_id,
                        b => b.m27_id_richiesta,
                        (a, b) => new ElementoFatturaRecuperoCrediti
                        {
                            DataFattura = b.m27_data_documento,
                            DataRichiesta = a.m26_data_inserimento,
                            DataScadenza = b.m27_data_scadenza_documento.Value,
                            IdRichiesta = a.m26_id,
                            StatoRichiesta = a.m26_stato_richiesta,
                            ImportoDecimal = b.m27_importo_documento,
                            NDoc = b.m27_numero_documento,
                            PartitaIva = a.m26_partita_iva,
                            RagioneSociale = a.m26_ragione_sociale
                        }
                    ).ToList();
                }



            }

            return returnValue;



        }

        public static SqlCommand GetSqlCommand(string cmdText, SqlConnection conn)
        {
            SqlCommand comm = new SqlCommand(cmdText, conn);

            // imposta il timeout del comando prendendo il valore corrispondente dall' App.config
            int commandTimeout = 0;
            /*
            try
            {
                commandTimeout = Int32.Parse(ConfigurationManager.AppSettings["CommandTimeout"]);
            }
            catch (Exception)
            {
                commandTimeout = 50000;
            }
            */
            comm.CommandTimeout = commandTimeout;

            return comm;
        }

        public static List<ReportAiende> GetReportList(string piva, string meseRif, int idRuolo, int idUserLog)
        {
            int meseRifInt = 0;
            List<ReportAiende> returnValue = new List<ReportAiende>();
            List<richiesta_report> listaRagSocRich = new List<richiesta_report>();
            using (DemoR2Entities context = new DemoR2Entities())
            {
                int month = DateTime.ParseExact(meseRif, "MMMM", CultureInfo.CurrentCulture).Month;
                int idazienda = context.vs_aziende_osservamercati.Where(_ => _.m01_partitaiva.Equals(piva)).Select(az => az.m01_idazienda).FirstOrDefault();
                int idUser = context.m10_utenti.Where(ut => ut.m10_idazienda == idazienda).Select(_ => _.m10_iduser).FirstOrDefault();

                if (idRuolo == 2)
                {
                    DateTime anno = DateTime.Parse("01/01/" + piva);
                    string data = "01/" + month + "/" + piva;
                    DateTime dataDa = DateTime.Parse(data);
                    DateTime dataA = dataDa.AddMonths(1);

                    listaRagSocRich = context.richiesta_report.Where(a => a.id_utente == idUserLog && a.data_richiesta >= dataDa && a.data_richiesta < dataA).ToList();

                }
                else
                {
                    listaRagSocRich = context.richiesta_report.Where(a => a.id_utente == idUser).ToList();
                }

                foreach (richiesta_report el in listaRagSocRich)
                {

                    ReportAiende retVal = new ReportAiende();
                    retVal.RagioneSociale = context.m02_anagrafica.Where(an => an.m02_partitaiva.Equals(el.chiave)).Select(a => a.m02_ragionesociale).FirstOrDefault();
                    retVal.Fatturato = el.evasa == false ? "no" : "si";
                    retVal.partitaIva = el.chiave;
                    retVal.DataRichiesta = el.data_richiesta;
                    retVal.TipoRichiesta = el.tipo_richiesta;
                    returnValue.Add(retVal);
                }
            }
            return returnValue;
        }
        public static List<ReportCs> GetReportallCs(int idUser, int idMercato)
        {
            List<ReportCs> returnValue = new List<ReportCs>();
            ReportCs appo = new ReportCs();
            List<ReportCs> returnValueNorm = new List<ReportCs>();
            List<ReportCs> returnValueOk = new List<ReportCs>();
            using (DemoR2Entities context = new DemoR2Entities())
            {
                IQueryable<richiesta_report> reportElem = from richiesta_report e in context.richiesta_report select e;
                IQueryable<m10_utenti> reportElemUtente = from m10_utenti e in context.m10_utenti select e;
                IQueryable<vs_aziende_osservamercati> reportElemazienda = from vs_aziende_osservamercati e in context.vs_aziende_osservamercati select e;
                int numAz = reportElemazienda.Count();
                reportElem = context.richiesta_report;
                int idAzienda = 0;
                int idCentro = 0;
                int contatore = 0;

                List<string> vs_aziende = context.vs_aziende_osservamercati.Where(aa => aa.m01_idcentro == idMercato).Select(piva => piva.m01_partitaiva).ToList();
                List<m02_anagrafica> m02anagrafica = context.m02_anagrafica.Where(aaaa => vs_aziende.Contains(aaaa.m02_partitaiva)).ToList();
                foreach (string el in vs_aziende)
                {
                    List<richiesta_report> richiesteReport = context.richiesta_report.Where(a => a.chiave == el).ToList();
                    contatore = richiesteReport.Count;

                    foreach (richiesta_report elem in richiesteReport)
                    {


                        appo.idUtente = Convert.ToInt32(elem.id_utente);
                        appo.idCentro = Convert.ToInt32(elem.id_centro);
                        appo.emailUtente = elem.email_utente;
                        appo.TipoRichiesta = elem.chiave;
                        appo.DataRichiesta = elem.data_richiesta;
                        appo.Referente = context.m02_anagrafica.Where(a => a.m02_partitaiva == elem.chiave).FirstOrDefault().m02_ragionesociale;
                        returnValue.Add(appo);
                    }
                }


                foreach (m10_utenti ut in reportElemUtente)
                {
                    if (idUser == ut.m10_iduser)
                    {
                        idAzienda = (int)ut.m10_idazienda;
                    }
                }
                foreach (vs_aziende_osservamercati az in reportElemazienda)
                {
                    if (idAzienda == az.m01_idazienda)
                    {
                        idCentro = (int)az.m01_idcentro;
                    }
                }
                foreach (ReportCs x in returnValue)
                {
                    foreach (m10_utenti v in reportElemUtente)
                    {
                        if (v.m10_iduser == x.idUtente && x.idCentro == idCentro)
                        {
                            foreach (vs_aziende_osservamercati b in reportElemazienda)
                            {
                                if (b.m01_idazienda == v.m10_idazienda)
                                {
                                    if (!b.m01_ragionesociale.Contains("DEMO"))
                                    {
                                        if (idCentro == b.m01_idcentro)
                                        {
                                            x.PartitaIva = b.m01_partitaiva;
                                            x.RagioneSociale = b.m01_ragionesociale;
                                            returnValueNorm.Add(x);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                returnValueNorm = returnValueNorm.OrderByDescending(o => o.DataRichiesta).ToList();

            }
            return returnValueNorm;

        }

        public static SqlConnection OpenSqlConn()
        {
            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionStringR3"].ConnectionString);
            try
            {
                conn.Open();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
            return conn;
        }
        public static List<ElementoPreferiti> GetPreferitiHome(int idUtente, int idRuolo, int idCentro, string ambiente)
        {
            int idAzienda = 0;
            string ragioneSociale = "";
            using (DemoR2Entities context = new DemoR2Entities())
            {
                idAzienda = (int)context.m10_utenti.Where(_ => _.m10_iduser == idUtente).Select(__ => __.m10_idazienda).FirstOrDefault();
                ragioneSociale = context.vs_aziende_osservamercati.Where(rag => rag.m01_idazienda == idAzienda).Select(sel => sel.m01_ragionesociale).FirstOrDefault();
            }
            List<ElementoPreferiti> returnValue = new List<ElementoPreferiti>();
            string stringaActualDSO = string.Format("{0:yyyy-MM}", DateTime.Now);
            DateTime oneMonthAgo = DateTime.Now.AddMonths(-1);
            Log.Debug("START Get PreferitiHome ----- idUtente=" + idUtente + " idRuolo=" + idRuolo +
                " idCentro=" + idCentro + " ambiente=" + ambiente + " data da -- " + oneMonthAgo.ToString());

            try
            {
                using (DemoR2Entities context = new DemoR2Entities())
                {
                    IQueryable<news_operatore> newsPreferiti = Enumerable.Empty<news_operatore>().AsQueryable();
                    IQueryable<m02_anagrafica> m02anagrafica = Enumerable.Empty<m02_anagrafica>().AsQueryable();

                    /*Determinazione dei preferiti di riferimento*/

                    switch (idRuolo)
                    {
                        /*CASO SUPERADMIN PRENDE TUTTI I PREFERITI DI TUTTI GLI UTENTI*/
                        case 0:
                            newsPreferiti = context.news_operatore;
                            var bufferValue =
                                newsPreferiti
                                .Where(a => a.data_aggiornamento >= oneMonthAgo)
                                .Select((newsPreferito) =>
                                    new ElementoPreferiti
                                    {
                                        Osservatorio = newsPreferito.osservatorio ? newsPreferito.partita_iva : "",
                                        EventiNegativi = newsPreferito.eventi_negativi,
                                        DataVariazione = newsPreferito.data_variazione,
                                        DataAggiornamento = newsPreferito.data_aggiornamento,
                                        id = newsPreferito.partita_iva,
                                        PartitaIva = newsPreferito.partita_iva,
                                        Rating = SqlFunctions.StringConvert((double)context.m02_anagrafica
                                                    .Where(_ => _.m02_partitaiva == newsPreferito.partita_iva)
                                                    .Select(_ => _.m02_stato_validazione_cerved).FirstOrDefault()),
                                        Fido =
                                            context.m02_anagrafica.Where(an => an.m02_partitaiva == newsPreferito.partita_iva).FirstOrDefault().m02_fido
                                        ,
                                        Rapporto = context.m02_anagrafica.Where(a => a.m02_partitaiva == newsPreferito.partita_iva).Select(b => b.m02_note).FirstOrDefault(),
                                        RagioneSociale = newsPreferito.ragione_sociale,
                                        Esposizione = newsPreferito.esposizione,
                                        Aggiornamento =
                                           newsPreferito.riepilogo_variazione == true ?
                                        context.m10_utenti.Where(ut => ut.m10_iduser == newsPreferito.id_user).Join(context.vs_aziende_osservamercati,
                                        a => a.m10_idazienda,
                                        b => b.m01_idazienda,
                                        (a, b) => new { Codice = newsPreferito.partita_iva }).FirstOrDefault().Codice : "",
                                        CodiceRapporto = newsPreferito.valutazione.Value != 0 ? newsPreferito.rapporto : "",
                                        Letto = newsPreferito.letto,
                                        DescrizioneVariazione = newsPreferito.descrizione,
                                        UtenteNews = context.m10_utenti.Where(ut => ut.m10_iduser == newsPreferito.id_user).FirstOrDefault().m10_user
                                    });
                            returnValue = bufferValue.ToList();
                            break;
                        /*CASO ADMIN PRENDE TUTTI I PREFERITI DI TUTTI GLI UTENTI DEL MERCATO*/
                        case 1:
                            DateTime dataoggi = DateTime.Now;
                            DateTime dataInizioRicercaSaldo = dataoggi.AddMonths(-13);

                            newsPreferiti = context.news_operatore.Join(
                                context.m10_utenti,
                                a => a.id_user,
                                b => b.m10_iduser,
                                (a, b) => new { Preferito = a, idAzienda = b.m10_idazienda })
                                .Join(context.vs_aziende_osservamercati.Where(az => az.m01_idcentro == idCentro),
                                a => a.idAzienda,
                                b => b.m01_idazienda,
                                (a, b) => a.Preferito);
                            bufferValue =
                               newsPreferiti
                               .Where(a => a.data_aggiornamento >= oneMonthAgo)
                               .GroupBy(gr => new
                               {
                                   gr.data_variazione,
                                   gr.descrizione,
                                   gr.eventi_negativi,
                                   gr.partita_iva,
                                   gr.ragione_sociale,
                                   gr.valutazione,
                                   gr.fido
                               })
                               .Select((newsPreferitoGr) =>
                                   new ElementoPreferiti
                                   {
                                       Osservatorio = newsPreferitoGr.Key.partita_iva,
                                       EventiNegativi = newsPreferitoGr.Key.eventi_negativi,
                                       DataVariazione = newsPreferitoGr.Key.data_variazione,
                                       DataAggiornamento = newsPreferitoGr.Max(_ => _.data_aggiornamento),
                                       id = "0",
                                       PartitaIva = newsPreferitoGr.Key.partita_iva,
                                       Rating = SqlFunctions.StringConvert((double)context.m02_anagrafica
                                                    .Where(_ => _.m02_partitaiva == newsPreferitoGr.Key.partita_iva)
                                                    .Select(_ => _.m02_stato_validazione_cerved).FirstOrDefault()),
                                       Fido =
                                            context.m02_anagrafica.Where(an => an.m02_partitaiva == newsPreferitoGr.Key.partita_iva).FirstOrDefault().m02_fido
                                        ,
                                       Rapporto = context.m02_anagrafica.Where(a => a.m02_partitaiva == newsPreferitoGr.Key.partita_iva).Select(b => b.m02_note).FirstOrDefault(),

                                       RagioneSociale = newsPreferitoGr.Key.ragione_sociale,
                                       Esposizione = context.vs_aziende_osservamercati.Where(_ => _.m01_idcentro == idCentro)

                                            .Select(sel => new
                                            {
                                                CODICE = sel,
                                                UltimaEsposizione =
                                                        context.m03_saldi.Where(saldo => saldo.m03_partitaiva == newsPreferitoGr.Key.partita_iva && saldo.m03_codiceazienda == sel.m01_codice && (saldo.m03_dtsaldo >= dataInizioRicercaSaldo && saldo.m03_dtsaldo <= dataoggi))
                                                        .OrderByDescending(ord => ord.m03_dtsaldo).FirstOrDefault() != null ?
                                                        context.m03_saldi.Where(saldo => saldo.m03_partitaiva == newsPreferitoGr.Key.partita_iva && saldo.m03_codiceazienda == sel.m01_codice && (saldo.m03_dtsaldo >= dataInizioRicercaSaldo && saldo.m03_dtsaldo <= dataoggi))
                                                        .OrderByDescending(ord => ord.m03_dtsaldo).FirstOrDefault().m03_saldo : 0
                                            }).Sum(s => s.UltimaEsposizione),
                                       Aggiornamento = newsPreferitoGr.Key.partita_iva,
                                       Letto = false,
                                       DescrizioneVariazione = newsPreferitoGr.Key.descrizione

                                   });
                            returnValue = bufferValue.ToList();


                            break;
                        default:
                            /*CASO USER PRENDE TUTTI I PREFERITI DI DELL'AZIENDA DI RIFERIMENTO DELL'UTENTE LOGGATO*/
                            newsPreferiti = context.news_operatore.Where(id => id.id_user == idUtente);
                            bufferValue =
                                newsPreferiti
                                .Where(a => a.data_aggiornamento >= oneMonthAgo && !a.partita_iva.Equals("01704320595"))
                                .Select((newsPreferito) =>
                                    new ElementoPreferiti
                                    {
                                        Osservatorio = newsPreferito.osservatorio ? newsPreferito.partita_iva : "",
                                        EventiNegativi = newsPreferito.eventi_negativi,
                                        DataVariazione = newsPreferito.data_variazione,
                                        DataAggiornamento = newsPreferito.data_aggiornamento,
                                        id = newsPreferito.partita_iva,
                                        PartitaIva = newsPreferito.partita_iva,
                                        Rating = SqlFunctions.StringConvert((double)context.m02_anagrafica
                                                    .Where(_ => _.m02_partitaiva == newsPreferito.partita_iva)
                                                    .Select(_ => _.m02_stato_validazione_cerved).FirstOrDefault()),
                                        Fido =
                                            context.m02_anagrafica.Where(an => an.m02_partitaiva == newsPreferito.partita_iva).FirstOrDefault().m02_fido
                                        ,
                                        Rapportonew = context.news_operatore.Where(op => op.partita_iva.Equals(newsPreferito.partita_iva))
                                                    .Select(sel => sel.aggiornamento).FirstOrDefault() == true ? "1" : "0",
                                        Rapporto = context.m02_anagrafica.Where(a => a.m02_partitaiva == newsPreferito.partita_iva).Select(b => b.m02_note).FirstOrDefault(),
                                        RagioneSociale = newsPreferito.ragione_sociale,
                                        Esposizione = newsPreferito.esposizione,
                                        //Rapporto = newsPreferito.rapporto != "" ? newsPreferito.rapporto : "",
                                        Aggiornamento =
                                           newsPreferito.riepilogo_variazione == true ?
                                        context.m10_utenti.Where(ut => ut.m10_iduser == newsPreferito.id_user).Join(context.vs_aziende_osservamercati,
                                        a => a.m10_idazienda,
                                        b => b.m01_idazienda,
                                        (a, b) => new { Codice = newsPreferito.partita_iva }).FirstOrDefault().Codice : "",
                                        CodiceRapporto = newsPreferito.valutazione.Value != 0 ? newsPreferito.rapporto : "",
                                        Letto = newsPreferito.letto,
                                        DescrizioneVariazione = newsPreferito.descrizione,
                                        UtenteNews = context.m10_utenti.Where(ut => ut.m10_iduser == newsPreferito.id_user).FirstOrDefault().m10_user
                                    });
                            returnValue = bufferValue.ToList();
                            break;
                    }


                }

            }
            catch (Exception e)
            {
                Log.Error("errore :" + e.ToString());
            }
            Log.Debug("Totale preferiti = " + returnValue.Count);
            return returnValue;
        }

        public static List<ElementoPreferiti> GetPreferitiMonitoraggio(int idUtente, int idRuolo, int idCentro, string ambiente)
        {
            List<ElementoPreferiti> returnValue = new List<ElementoPreferiti>();

            IQueryable<m15_preferiti> preferiti = Enumerable.Empty<m15_preferiti>().AsQueryable();
            List<int> idAziende = new List<int>();
            List<string> idUtenti = new List<string>();

            string stringaActualDSO = string.Format("{0:yyyy-MM}", DateTime.Now);
            using (DemoR2Entities context = new DemoR2Entities())
            {
                /*Determinazione dei preferiti di riferimento*/
                switch (idRuolo)
                {
                    /*CASO SUPERADMIN PRENDE TUTTI I PREFERITI DI TUTTI GLI UTENTI*/
                    case 0:
                        preferiti = context.m15_preferiti;
                        break;
                    /*CASO ADMIN PRENDE TUTTI I PREFERITI DI TUTTI GLI UTENTI DEL MERCATO*/
                    case 1:

                        try
                        {
                            idAziende = context.vs_aziende_osservamercati.Where(az => az.m01_idcentro == idCentro)
                                                  .Select(sel => sel.m01_idazienda)
                                                  .ToList();
                            foreach (int i in idAziende)
                            {
                                int quest = context.m10_utenti.Where(wrr => wrr.m10_idazienda == i).Select(ssss => ssss.m10_iduser).FirstOrDefault();
                                if (quest > 0)
                                {
                                    idUtenti.Add(quest.ToString());
                                    Console.WriteLine(quest);
                                }
                            }
                            preferiti = context.m15_preferiti.Where(pr => idUtenti.Contains(SqlFunctions.StringConvert((double)pr.m15_iduser)));
                            break;

                        }
                        catch (EntityException e)
                        {
                            Console.WriteLine(e.ToString());
                        }
                        break;
                    case 2:
                        /*CASO USER PRENDE TUTTI I PREFERITI DI DELL'AZIENDA DI RIFERIMENTO DELL'UTENTE LOGGATO*/
                        preferiti = context.m15_preferiti.Where(id => id.m15_iduser == idUtente);
                        break;
                }
                // string newFile = "";
                // string codiceCerved = "";
                // string rapporto = "";

                DirectoryInfo di = new DirectoryInfo(WebConfigurationManager.AppSettings["ReportPath"]);
                IDictionary<string, DateTime> openWith = new Dictionary<string, DateTime>();
                ElementoOperatore aziendaUtente = GetAziendaUtente(idUtente, context, ambiente);
                string stringaData = String.Format("{0:yyyy-MM}", DateTime.Now);
                int idAzienda = (int)context.m10_utenti.Where(ut => ut.m10_iduser.Equals(idUtente)).Select(a => a.m10_idazienda).FirstOrDefault();
                int codiceAzienda = Convert.ToInt32(aziendaUtente.CodiceAzienda);
                /*RIMUOVERE O CAMBIARE! AGGIUNTO PASSAGGIO PER VERIFICA ANAGRAFICA VUOTA O NON TROVATA */
                IQueryable<news_operatore> newsPreferiti = Enumerable.Empty<news_operatore>().AsQueryable();

                if (idRuolo == 1)
                {
                    newsPreferiti = context.news_operatore;
                    var bufferValue =
                            preferiti
                            .Join(
                            context.m02_anagrafica.Where(an => an.m02_ragionesociale != String.Empty),
                            a => a.m15_partitaiva,
                            b => b.m02_partitaiva,
                            (a, b) => a)
                            .Select((clienteSaldo) =>
                                new ElementoPreferiti
                                {
                                    Osservatorio =
                            context.m02_anagrafica.Where(anag => anag.m02_partitaiva == clienteSaldo.m15_partitaiva)
                            .Join(context.m04_vendite.Where(v => v.m04_partitaiva == clienteSaldo.m15_partitaiva),
                            a => a.m02_partitaiva,
                            b => b.m04_partitaiva,
                            (a, b) => a).FirstOrDefault() != null ?
                            clienteSaldo.m15_partitaiva : "",
                                    EventiNegativi =
                                    context.m02_anagrafica.Where(an => an.m02_partitaiva == clienteSaldo.m15_partitaiva).FirstOrDefault().m02_eventi_negativi
                                    ,
                                    PartitaIva = clienteSaldo.m15_partitaiva,
                                    Vendite =

                                    context.m04_vendite.Where(vendita =>
                                        (vendita.m04_dtdocvendita.Year == DateTime.Now.Year && vendita.m04_dtdocvendita.Month == DateTime.Now.Month) &&
                                        vendita.m04_partitaiva == clienteSaldo.m15_partitaiva).Sum(s => s.m04_importo).HasValue ?
                                    context.m04_vendite.Where(vendita =>
                                        (vendita.m04_dtdocvendita.Year == DateTime.Now.Year && vendita.m04_dtdocvendita.Month == DateTime.Now.Month) &&
                                        vendita.m04_partitaiva == clienteSaldo.m15_partitaiva).Sum(s => s.m04_importo).Value : 0
                                    ,
                                    Rating = SqlFunctions.StringConvert((double)context.m02_anagrafica
                                                        .Where(_ => _.m02_partitaiva == clienteSaldo.m15_partitaiva)
                                                        .Select(_ => _.m02_stato_validazione_cerved).FirstOrDefault()) != null ? SqlFunctions.StringConvert((double)context.m02_anagrafica
                                                        .Where(_ => _.m02_partitaiva == clienteSaldo.m15_partitaiva)
                                                        .Select(_ => _.m02_stato_validazione_cerved).FirstOrDefault()) : "",
                                    Rapportonew = context.news_operatore.Where(op => op.partita_iva.Equals(clienteSaldo.m15_partitaiva))
                                                        .Select(sel => sel.aggiornamento).FirstOrDefault() == true ? "1" : "0",
                                    Fido =
                                    context.m02_anagrafica.Where(an => an.m02_partitaiva == clienteSaldo.m15_partitaiva).FirstOrDefault().m02_fido
                                    ,
                                    RagioneSociale = context.m02_anagrafica.Where(anag => anag.m02_partitaiva == clienteSaldo.m15_partitaiva)
                                    .FirstOrDefault() != null ? context.m02_anagrafica.Where(anag => anag.m02_partitaiva == clienteSaldo.m15_partitaiva)
                                        .FirstOrDefault().m02_ragionesociale : "",

                                    Esposizione =
                                    context.s03_AggregazioneSaldoAziende.Where(agg => agg.PartitaIva.Equals(clienteSaldo.m15_partitaiva)
                                                                               )
                                                                              .OrderByDescending(_ => _.Mese_rif)
                                                                              .FirstOrDefault() != null ? (decimal)
                                                                        context.s03_AggregazioneSaldoAziende.Where(agg => agg.PartitaIva
                                                                        .Equals(clienteSaldo.m15_partitaiva)
                                                                              ).OrderByDescending(_ => _.Mese_rif)
                                                                        .FirstOrDefault().Importo : 0,
                                    Rapporto = context.m02_anagrafica.Where(a => a.m02_partitaiva == clienteSaldo.m15_partitaiva)
                                                                     .Select(b => b.m02_note).FirstOrDefault(),
                                    CodiceRapporto =
                                    idRuolo == 0 ?
                                    (context.cerved_check.Where(check => check.partita_iva == clienteSaldo.m15_partitaiva).OrderByDescending(d => d.dt_inserimento).FirstOrDefault() != null ?
                                        (context.cerved_check.Where(check => check.partita_iva == clienteSaldo.m15_partitaiva).OrderByDescending(d => d.dt_inserimento).FirstOrDefault().evaso == true ?
                                        context.cerved_check.Where(check => check.partita_iva == clienteSaldo.m15_partitaiva).OrderByDescending(d => d.dt_inserimento).FirstOrDefault().codice_cerved : "P"
                                        ) : "") :
                                    (context.cerved_check.Where(check => check.partita_iva == clienteSaldo.m15_partitaiva).OrderByDescending(d => d.dt_inserimento).FirstOrDefault() != null ?
                                        (context.cerved_check.Where(check => check.partita_iva == clienteSaldo.m15_partitaiva).OrderByDescending(d => d.dt_inserimento).FirstOrDefault().evaso == true ?
                                        context.cerved_check.Where(check => check.partita_iva == clienteSaldo.m15_partitaiva).OrderByDescending(d => d.dt_inserimento).FirstOrDefault().codice_cerved : "P"
                                        ) : ""),
                                    AnomaliaVendite = false,
                                    Camerale = String.Empty
                                }).Distinct().Where(wre => wre.Esposizione > 0);

                    returnValue = bufferValue.ToList();
                }
                else
                {
                    newsPreferiti = context.news_operatore;
                    var bufferValue =
                            preferiti
                            .Join(
                            context.m02_anagrafica.Where(an => an.m02_ragionesociale != String.Empty),
                            a => a.m15_partitaiva,
                            b => b.m02_partitaiva,
                            (a, b) => a)
                            .Select((clienteSaldo) =>
                                new ElementoPreferiti
                                {
                                    Osservatorio =
                            context.m02_anagrafica.Where(anag => anag.m02_partitaiva == clienteSaldo.m15_partitaiva)
                            .Join(context.m04_vendite.Where(v => v.m04_partitaiva == clienteSaldo.m15_partitaiva && v.m04_codiceazienda == aziendaUtente.CodiceAzienda),
                            a => a.m02_partitaiva,
                            b => b.m04_partitaiva,
                            (a, b) => a).FirstOrDefault() != null ?
                            clienteSaldo.m15_partitaiva : "",
                                    EventiNegativi =
                                    context.m02_anagrafica.Where(an => an.m02_partitaiva == clienteSaldo.m15_partitaiva).FirstOrDefault().m02_eventi_negativi
                                    ,
                                    PartitaIva = clienteSaldo.m15_partitaiva,
                                    Vendite =

                                    context.m04_vendite.Where(vendita =>
                                        (vendita.m04_dtdocvendita.Year == DateTime.Now.Year && vendita.m04_dtdocvendita.Month == DateTime.Now.Month) &&
                                        vendita.m04_partitaiva == clienteSaldo.m15_partitaiva && vendita.m04_codiceazienda == aziendaUtente.CodiceAzienda).Sum(s => s.m04_importo).HasValue ?
                                    context.m04_vendite.Where(vendita =>
                                        (vendita.m04_dtdocvendita.Year == DateTime.Now.Year && vendita.m04_dtdocvendita.Month == DateTime.Now.Month) &&
                                        vendita.m04_partitaiva == clienteSaldo.m15_partitaiva && vendita.m04_codiceazienda == aziendaUtente.CodiceAzienda).Sum(s => s.m04_importo).Value : 0
                                    ,
                                    Rating = SqlFunctions.StringConvert((double)context.m02_anagrafica
                                                        .Where(_ => _.m02_partitaiva == clienteSaldo.m15_partitaiva)
                                                        .Select(_ => _.m02_stato_validazione_cerved).FirstOrDefault()) != null ? SqlFunctions.StringConvert((double)context.m02_anagrafica
                                                        .Where(_ => _.m02_partitaiva == clienteSaldo.m15_partitaiva)
                                                        .Select(_ => _.m02_stato_validazione_cerved).FirstOrDefault()) : "",
                                    Rapportonew = context.news_operatore.Where(op => op.partita_iva.Equals(clienteSaldo.m15_partitaiva))
                                                        .Select(sel => sel.aggiornamento).FirstOrDefault() == true ? "1" : "0",
                                    Fido =
                                    context.m02_anagrafica.Where(an => an.m02_partitaiva == clienteSaldo.m15_partitaiva).FirstOrDefault().m02_fido
                                    ,
                                    RagioneSociale = context.m02_anagrafica.Where(anag => anag.m02_partitaiva == clienteSaldo.m15_partitaiva).FirstOrDefault() != null ? context.m02_anagrafica.Where(anag => anag.m02_partitaiva == clienteSaldo.m15_partitaiva).FirstOrDefault().m02_ragionesociale : "",

                                    Esposizione =
                                    context.s03_AggregazioneSaldoAziende.Where(agg => agg.PartitaIva.Equals(clienteSaldo.m15_partitaiva)
                                                                               && agg.IdAzienda == codiceAzienda).OrderByDescending(_ => _.Mese_rif)
                                                                        .FirstOrDefault() != null ? (decimal)
                                                                        context.s03_AggregazioneSaldoAziende.Where(agg => agg.PartitaIva.Equals(clienteSaldo.m15_partitaiva)
                                                                               && agg.IdAzienda == codiceAzienda).OrderByDescending(_ => _.Mese_rif)
                                                                        .FirstOrDefault().Importo : 0,

                                    /*Esposizione =
                                    context.m03_saldi.Where(saldo => saldo.m03_partitaiva == clienteSaldo.m15_partitaiva && saldo.m03_codiceazienda == aziendaUtente.CodiceAzienda)
                                                     .OrderByDescending(saldo => saldo.m03_dtsaldo).FirstOrDefault() != null ?
                                                     context.m03_saldi.Where(saldo => saldo.m03_partitaiva == clienteSaldo.m15_partitaiva && saldo.m03_codiceazienda == aziendaUtente.CodiceAzienda)
                                                     .OrderByDescending(saldo => saldo.m03_dtsaldo).FirstOrDefault().m03_saldo : 0,*/

                                    Rapporto = context.m02_anagrafica.Where(a => a.m02_partitaiva == clienteSaldo.m15_partitaiva).Select(b => b.m02_note).FirstOrDefault(),
                                    // Rapporto = 
                                    //     context.news_operatore.Where(a=>a.partita_iva == clienteSaldo.m15_partitaiva).FirstOrDefault() != null ?
                                    //     context.news_operatore.Where(a => a.partita_iva == clienteSaldo.m15_partitaiva).FirstOrDefault().rapporto : ""
                                    // ,
                                    CodiceRapporto =
                                    idRuolo == 0 ?
                                    (context.cerved_check.Where(check => check.partita_iva == clienteSaldo.m15_partitaiva).OrderByDescending(d => d.dt_inserimento).FirstOrDefault() != null ?
                                        (context.cerved_check.Where(check => check.partita_iva == clienteSaldo.m15_partitaiva).OrderByDescending(d => d.dt_inserimento).FirstOrDefault().evaso == true ?
                                        context.cerved_check.Where(check => check.partita_iva == clienteSaldo.m15_partitaiva).OrderByDescending(d => d.dt_inserimento).FirstOrDefault().codice_cerved : "P"
                                        ) : "") :
                                    (context.cerved_check.Where(check => check.partita_iva == clienteSaldo.m15_partitaiva && check.id_utente == idUtente).OrderByDescending(d => d.dt_inserimento).FirstOrDefault() != null ?
                                        (context.cerved_check.Where(check => check.partita_iva == clienteSaldo.m15_partitaiva && check.id_utente == idUtente).OrderByDescending(d => d.dt_inserimento).FirstOrDefault().evaso == true ?
                                        context.cerved_check.Where(check => check.partita_iva == clienteSaldo.m15_partitaiva && check.id_utente == idUtente).OrderByDescending(d => d.dt_inserimento).FirstOrDefault().codice_cerved : "P"
                                        ) : ""),
                                    AnomaliaVendite = false,
                                    Camerale = String.Empty
                                }).Distinct().Where(wre => wre.Esposizione > 0);

                    returnValue = bufferValue.ToList();
                }




            }
            return returnValue;

        }



        public static List<ElementoFatturaRecuperoCrediti> GetFattureDettaglioInvioRC(long idRichiesta)
        {
            List<ElementoFatturaRecuperoCrediti> returnValue = new List<ElementoFatturaRecuperoCrediti>();
            using (DemoR2Entities context = new DemoR2Entities())
            {
                returnValue =
                    context.m27_fattura_richiesta.Where(fat => fat.m27_id_richiesta == idRichiesta).Select(
                         f => new ElementoFatturaRecuperoCrediti { DataFattura = f.m27_data_documento, DataScadenza = f.m27_data_scadenza_documento.Value, ImportoDecimal = f.m27_importo_documento, NDoc = f.m27_numero_documento }

                    ).ToList();
            }
            return returnValue;
        }

        public static List<ElementoFatturaRecuperoCrediti> GetFattureRecuperoCrediti(int idUtente, string piva, string ambiente)
        {
            List<ElementoFatturaRecuperoCrediti> returnValue = new List<ElementoFatturaRecuperoCrediti>();
            using (DemoR2Entities context = new DemoR2Entities())
            {

                ElementoOperatore aziendaUtente = GetAziendaUtente(idUtente, context, ambiente);
                string codiceAzienda = aziendaUtente.CodiceAzienda;

                DateTime now = DateTime.Today;
                var lastDateofCurrentMonth = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month));

                DateTime dataTemp = DateTime.Now.AddYears(-1);
                DateTime dataNow = DateTime.Now;
                DateTime limiteDataDoc = new DateTime(dataTemp.Year, dataTemp.Month, 1);

                var bufferValue =
                    context.m04_vendite
                    .Where(vend => vend.m04_codiceazienda == codiceAzienda && vend.m04_partitaiva == piva && vend.m04_dtdocvendita >= limiteDataDoc
                                   && vend.m04_importo > 0
                                   && vend.m04_dtscadenza >= dataNow
                                   //todo && vend.m04_anticipoFatt== 0
                                   && (vend.m04_chiusura.HasValue ? vend.m04_chiusura.Value == false : false))
                        .Select((fatturaRC) =>
                            new ElementoFatturaRecuperoCrediti
                            {
                                PartitaIva = fatturaRC.m04_partitaiva,
                                RagioneSociale = context.m02_anagrafica.Where(p => p.m02_partitaiva == piva).FirstOrDefault() != null ?
                                context.m02_anagrafica.Where(p => p.m02_partitaiva == piva).FirstOrDefault().m02_ragionesociale : "",
                                DataFattura = fatturaRC.m04_dtdocvendita,
                                DataScadenza = fatturaRC.m04_dtscadenza.HasValue ? fatturaRC.m04_dtscadenza.Value : fatturaRC.m04_dtdocvendita,
                                Importo = fatturaRC.m04_importo.HasValue ? fatturaRC.m04_importo.Value : 0,
                                NDoc = fatturaRC.m04_numdoc,
                                GGScadutoAdOggi = System.Data.Objects.EntityFunctions.DiffDays(DateTime.Now, fatturaRC.m04_dtdocvendita).HasValue ? System.Data.Objects.EntityFunctions.DiffDays(DateTime.Now, fatturaRC.m04_dtdocvendita).Value : 0,
                                Rating =
                                            SqlFunctions.StringConvert((double)context.m02_anagrafica.Where(aa => aa.m02_partitaiva.Equals(fatturaRC.m04_partitaiva))
                                                                                .Select(sel => sel.m02_stato_validazione_cerved).FirstOrDefault())
                                ,
                                RatingDescrizione =
                                context.m05_rating
                                .Where(rat => rat.m05_partitaiva == fatturaRC.m04_partitaiva && (rat.m05_dtriferimento <= fatturaRC.m04_dtdocvendita && rat.m05_dtfinevalidita >= fatturaRC.m04_dtdocvendita)).FirstOrDefault() != null ?
                                context.m05_rating
                                .Where(rat => rat.m05_partitaiva == fatturaRC.m04_partitaiva && (rat.m05_dtriferimento <= fatturaRC.m04_dtdocvendita && rat.m05_dtfinevalidita >= fatturaRC.m04_dtdocvendita)).FirstOrDefault().m05_stato_semaforo : ""
                            }).ToList();

                int contatore = 0;
                foreach (var elemento in bufferValue)
                {

                    elemento.id = contatore;
                    contatore++;
                }
                returnValue = bufferValue;
            }

            return returnValue;
        }

        public static List<ElementoFatturaRecuperoCrediti> GetNoteCredito(int idUtente, string piva, string ambiente, DateTime dataPrima)
        {
            List<ElementoFatturaRecuperoCrediti> returnValue = new List<ElementoFatturaRecuperoCrediti>();
            using (DemoR2Entities context = new DemoR2Entities())
            {

                ElementoOperatore aziendaUtente = GetAziendaUtente(idUtente, context, ambiente);
                string codiceAzienda = aziendaUtente.CodiceAzienda;

                DateTime now = DateTime.Today;
                var lastDateofCurrentMonth = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month));

                DateTime dataTemp = DateTime.Now.AddYears(-2);
                DateTime limiteDataDoc = new DateTime(dataTemp.Year, dataTemp.Month, 1);

                var bufferValue =
                    context.m04_vendite
                    .Where(vend => vend.m04_codiceazienda == codiceAzienda && vend.m04_partitaiva == piva && vend.m04_dtdocvendita >= limiteDataDoc && vend.m04_importo < 0 && vend.m04_dtdocvendita >= dataPrima)
                        .Select((fatturaRC) =>
                            new ElementoFatturaRecuperoCrediti
                            {
                                PartitaIva = fatturaRC.m04_partitaiva,
                                RagioneSociale = context.m02_anagrafica.Where(p => p.m02_partitaiva == piva).FirstOrDefault() != null ?
                                context.m02_anagrafica.Where(p => p.m02_partitaiva == piva).FirstOrDefault().m02_ragionesociale : "",
                                DataFattura = fatturaRC.m04_dtdocvendita,
                                DataScadenza = fatturaRC.m04_dtscadenza.HasValue ? fatturaRC.m04_dtscadenza.Value : fatturaRC.m04_dtdocvendita,
                                Importo = fatturaRC.m04_importo.HasValue ? fatturaRC.m04_importo.Value : 0,
                                NDoc = fatturaRC.m04_numdoc,
                                GGScadutoAdOggi = System.Data.Objects.EntityFunctions.DiffDays(DateTime.Now, fatturaRC.m04_dtdocvendita).HasValue ? System.Data.Objects.EntityFunctions.DiffDays(DateTime.Now, fatturaRC.m04_dtdocvendita).Value : 0,
                                Rating =
                                context.m05_rating
                                .Where(rat => rat.m05_partitaiva == fatturaRC.m04_partitaiva && (rat.m05_dtriferimento <= fatturaRC.m04_dtdocvendita && rat.m05_dtfinevalidita >= fatturaRC.m04_dtdocvendita)).OrderByDescending(rating => rating.m05_dtriferimento).FirstOrDefault() != null ?
                                context.m05_rating
                                .Where(rat => rat.m05_partitaiva == fatturaRC.m04_partitaiva && (rat.m05_dtriferimento <= fatturaRC.m04_dtdocvendita && rat.m05_dtfinevalidita >= fatturaRC.m04_dtdocvendita)).OrderByDescending(rating => rating.m05_dtriferimento).FirstOrDefault().m05_stato : "",
                                RatingDescrizione =
                                context.m05_rating
                                .Where(rat => rat.m05_partitaiva == fatturaRC.m04_partitaiva && (rat.m05_dtriferimento <= fatturaRC.m04_dtdocvendita && rat.m05_dtfinevalidita >= fatturaRC.m04_dtdocvendita)).FirstOrDefault() != null ?
                                context.m05_rating
                                .Where(rat => rat.m05_partitaiva == fatturaRC.m04_partitaiva && (rat.m05_dtriferimento <= fatturaRC.m04_dtdocvendita && rat.m05_dtfinevalidita >= fatturaRC.m04_dtdocvendita)).FirstOrDefault().m05_stato_semaforo : ""
                            }).ToList();

                int contatore = 0;
                foreach (var elemento in bufferValue)
                {

                    elemento.id = contatore;
                    elemento.StrDataFattura = String.Format("{0:dd/MM/yyyy}", elemento.DataFattura);
                    contatore++;
                }
                returnValue = bufferValue.OrderByDescending(ord => ord.DataFattura).ToList();
            }

            return returnValue;
        }

        public static ElementoGraficoMercato AggiungiRating(ElementoGraficoMercato input, string partitaIva)
        {
            List<ElementoGraficoMercato> returnValue = new List<ElementoGraficoMercato>();
            using (DemoR2Entities context = new DemoR2Entities())
            {


                int anno = Convert.ToInt32(input.Mensilita.Split('-')[0].ToString());
                int mese = Convert.ToInt32(input.Mensilita.Split('-')[1].ToString());

                DateTime dataCheck = new DateTime(anno, mese, 1);

                m05_rating ratingCheck = context.m05_rating.Where(r => r.m05_partitaiva == partitaIva && (r.m05_dtriferimento <= dataCheck && dataCheck <= r.m05_dtfinevalidita)).FirstOrDefault();
                string stato = "N.D";
                if (ratingCheck != null)
                {
                    int rating = Convert.ToInt32(ratingCheck.m05_stato);
                    if (rating < 10)
                    {
                        switch (rating)
                        {
                            case 0:
                                stato = "<img src='../content/images/sfera_grigia.gif' alt='Grigia' title='Grigia' />";
                                break;
                            case 1:
                                stato = "<img src='../content/images/sfera_rossa.gif' alt='Rossa' title='Rossa' />";
                                break;
                            case 2:
                                stato = "<img src='../content/images/sfera_gialla.gif' alt='Gialla' title='Gialla' />";
                                break;
                            case 3:
                                stato = "<img src='../content/images/sfera_grigia.gif' alt='Grigia' title='Grigia' />";
                                break;
                            case 4:
                                stato = "<img src='../content/images/sfera_verde.gif' alt='Verde' title='Verde' />";
                                break;
                            case 5:
                                stato = "<img src='../content/images/sfera_nera.gif' alt='Nera' title='Nera' />";
                                break;
                            case 6:
                                stato = "<img src='../content/images/sfera_nera.gif' alt='Nera' title='Nera' />";
                                break;
                            case 7:
                                stato = "<img src='../content/images/sfera_rossa.gif' alt='Rossa' title='Rossa' />";
                                break;
                            case 8:
                                stato = "<img src='../content/images/sfera_gialla.gif' alt='Gialla' title='Gialla' />";
                                break;
                        }
                    }
                    else
                    {
                        if (rating < 30)
                            stato = "<img src='../content/images/sfera_rossa.gif' alt='Rossa' title='Rossa' />";
                        if (rating >= 30 && rating > 60)
                            stato = "<img src='../content/images/sfera_gialla.gif' alt='Gialla' title='Gialla' />";
                        if (rating >= 60)
                            stato = "<img src='../content/images/sfera_verde.gif' alt='Verde' title='Verde' />";
                    }

                    input.Rating = stato;
                }

            }

            return input;
        }



        public static List<ElementoGraficoMercato> AggiungiReportRating(List<ElementoGraficoMercato> input, string partitaIva, NavigationUser loggedUser)
        {
            List<ElementoGraficoMercato> returnValue = new List<ElementoGraficoMercato>();
            using (DemoR2Entities context = new DemoR2Entities())
            {
                foreach (ElementoGraficoMercato el in input)
                {

                    int anno = Convert.ToInt32(el.Mensilita.Split('-')[0].ToString());
                    int mese = Convert.ToInt32(el.Mensilita.Split('-')[1].ToString());

                    DateTime dataCheck = new DateTime(anno, mese, 1);

                    m05_rating ratingCheck = context.m05_rating.Where(r => r.m05_partitaiva == partitaIva && (r.m05_dtriferimento <= dataCheck && dataCheck <= r.m05_dtfinevalidita)).FirstOrDefault();

                    string codiceCerved =
                    loggedUser.IdRuolo == 0 ?
                                (context.cerved_check.Where(check => check.partita_iva == partitaIva).OrderByDescending(d => d.dt_inserimento).FirstOrDefault() != null ?
                                    (context.cerved_check.Where(check => check.partita_iva == partitaIva).OrderByDescending(d => d.dt_inserimento).FirstOrDefault().evaso == true ?
                                    context.cerved_check.Where(check => check.partita_iva == partitaIva).OrderByDescending(d => d.dt_inserimento).FirstOrDefault().codice_cerved : "P"
                                    ) : "") :
                                (context.cerved_check.Where(check => check.partita_iva == partitaIva && check.id_utente == loggedUser.IdUser).OrderByDescending(d => d.dt_inserimento).FirstOrDefault() != null ?
                                    (context.cerved_check.Where(check => check.partita_iva == partitaIva && check.id_utente == loggedUser.IdUser).OrderByDescending(d => d.dt_inserimento).FirstOrDefault().evaso == true ?
                                    context.cerved_check.Where(check => check.partita_iva == partitaIva && check.id_utente == loggedUser.IdUser).OrderByDescending(d => d.dt_inserimento).FirstOrDefault().codice_cerved : "P"
                                    ) : "");

                    if (DBHandler.VerificaPermessiReport("", loggedUser.IdUser, loggedUser.IdRuolo, loggedUser.IdMercato))
                    {

                    }



                    /*cerved_check cervedCheck = context*/
                    string stato = "N.D";
                    if (ratingCheck != null)
                    {
                        if (ratingCheck.m05_stato != "ND")
                        {
                            int rating = Convert.ToInt32(ratingCheck.m05_stato);


                            if (rating <= 5)
                            {
                                switch (rating)
                                {
                                    case 1:
                                        stato = "<img src='../content/images/sfera_verde.gif' alt='Verde 1' title='Verde 1' />";
                                        break;
                                    case 2:
                                        stato = "<img src='../content/images/sfera_verde.gif' alt='Verde 2' title='Verde 2' />";
                                        break;
                                    case 3:
                                        stato = "<img src='../content/images/sfera_verde.gif' alt='Verde 3' title='Verde 3' />";
                                        break;
                                    case 4:
                                        stato = "<img src='../content/images/sfera_gialla.gif' alt='Giallo' title='Giallo' />";
                                        break;
                                    case 5:
                                        stato = "<img src='../content/images/sfera_rossa.gif' alt='Rosso' title='Rosso' />";
                                        break;


                                }
                            }
                            else
                            {
                                if (rating < 30)
                                    stato = "<img src='../content/images/sfera_rossa.gif' alt='Rossa' title='Rossa' />";
                                if (rating >= 30 && rating > 60)
                                    stato = "<img src='../content/images/sfera_gialla.gif' alt='Gialla' title='Gialla' />";
                                if (rating >= 60)
                                    stato = "<img src='../content/images/sfera_verde.gif' alt='Verde' title='Verde' />";
                            }
                        }

                        el.Rating = stato;
                        el.Report = partitaIva;
                    }



                    returnValue.Add(el);

                }

            }

            return returnValue;
        }


        public static List<ElementoAziendaRecuperoCrediti> GetRichiesteRecuperoCreditiHome(int idUtente, int idRuolo, int idCentro, string ambiente)
        {
            List<ElementoAziendaRecuperoCrediti> returnValue = new List<ElementoAziendaRecuperoCrediti>();

            using (DemoR2Entities context = new DemoR2Entities())
            {
                ElementoOperatore aziendaUtente = GetAziendaUtente(idUtente, context, ambiente);

                int codiceAzienda = Convert.ToInt32(aziendaUtente.CodiceAzienda);

                IQueryable<ElementoAziendaRecuperoCrediti> bufferValue = Enumerable.Empty<ElementoAziendaRecuperoCrediti>().AsQueryable();

                switch (idRuolo)
                {
                    /*CASO SUPERADMIN Prendo tutte le richieste di recupero crediti*/
                    case 0:
                        bufferValue =
                            context.m26_richiesta_recupero_crediti
                                .Select((clienteRC) =>
                                    new ElementoAziendaRecuperoCrediti
                                    {
                                        idRC = clienteRC.m26_partita_iva,
                                        PartitaIva = clienteRC.m26_partita_iva,
                                        RagioneSocialeRC = clienteRC.m26_ragione_sociale,
                                        StatoRichiesta = clienteRC.m26_stato_richiesta,
                                        DataAggiornamento = clienteRC.m26_data_ultima_modifica,
                                        DataRichiesta = clienteRC.m26_data_inserimento,
                                        SommaFatture =
                                          context.m27_fattura_richiesta.Where(fatRic => fatRic.m27_id_richiesta == clienteRC.m26_id)
                                          .Sum(fat => fat.m27_importo_documento),
                                        SommaIncasso = clienteRC.m26_incasso,
                                        IdDettaglio = clienteRC.m26_id
                                    });
                        break;
                    /*CASO ADMIN Prendo tutte le richieste di recupero crediti del mercato*/
                    case 1:

                        bufferValue =
                            context.m26_richiesta_recupero_crediti
                                .Join(context.vs_aziende_osservamercati.Where(az => az.m01_idcentro == idCentro),
                                a => a.m26_codice_azienda.ToString(),
                                b => b.m01_codice,
                                (a, b) =>
                                    new ElementoAziendaRecuperoCrediti
                                    {
                                        idRC = a.m26_partita_iva,
                                        PartitaIva = a.m26_partita_iva,
                                        RagioneSocialeRC = a.m26_ragione_sociale,
                                        StatoRichiesta = a.m26_stato_richiesta,
                                        DataRichiesta = a.m26_data_inserimento,
                                        SommaIncasso = a.m26_incasso,
                                        SommaFatture =
                                          context.m27_fattura_richiesta.Where(fatRic => fatRic.m27_id_richiesta == a.m26_id)
                                          .Sum(fat => fat.m27_importo_documento),
                                        IdDettaglio = a.m26_id
                                    });
                        break;
                    /*CASO USER Prendo tutte le richieste di recupero crediti del mercato*/
                    case 2:
                        bufferValue =
                            context.m26_richiesta_recupero_crediti
                                .Where(id => id.m26_codice_azienda == codiceAzienda)
                                .Select((clienteRC) =>
                                    new ElementoAziendaRecuperoCrediti
                                    {
                                        idRC = clienteRC.m26_partita_iva,
                                        PartitaIva = clienteRC.m26_partita_iva,
                                        RagioneSocialeRC = clienteRC.m26_ragione_sociale,
                                        StatoRichiesta = clienteRC.m26_stato_richiesta,
                                        DataRichiesta = clienteRC.m26_data_inserimento,
                                        SommaIncasso = clienteRC.m26_incasso,
                                        SommaFatture =
                                          context.m27_fattura_richiesta.Where(fatRic => fatRic.m27_id_richiesta == clienteRC.m26_id)
                                          .Sum(fat => fat.m27_importo_documento),
                                        IdDettaglio = clienteRC.m26_id
                                    });
                        break;
                }
                try
                {
                    returnValue = bufferValue.ToList();
                }
                catch (Exception e)
                {
                    Log.Error(e.ToString());
                }
            }

            return returnValue;

        }



        public static vs_aziende_osservamercati GetAziendaUtenteOM(long idUtente)
        {
            vs_aziende_osservamercati returnValue = new vs_aziende_osservamercati();
            using (DemoR2Entities context = new DemoR2Entities())
            {
                returnValue = context.m10_utenti.Where(ut => ut.m10_iduser == idUtente)
                        .Join(context.vs_aziende_osservamercati,
                        a => a.m10_idazienda,
                        b => b.m01_idazienda,
                        (a, b) => b).FirstOrDefault();
            }
            return returnValue;
        }

        public static ElementoOperatore GetAziendaUtente(long idUtente, DemoR2Entities context, string ambiente)
        {
            bool contextToBeClosed = false;
            if (context == null)
            {
                context = new DemoR2Entities();
                contextToBeClosed = true;
            }
            ElementoOperatore returnValue = new ElementoOperatore();
            if (ambiente == "STANDARD")
            {
                returnValue = context.vs_aziende_osservamercati.Join(context.m10_utenti.Where(id => id.m10_iduser == idUtente),
                                                            a => a.m01_idazienda,
                                                            b => b.m10_idazienda,
                                                            (a, b) => a)
                                                            .Select(an => new ElementoOperatore { Id = an.m01_idazienda, IdCentro = an.m01_idcentro.Value, RagioneSociale = an.m01_ragionesociale, Ambiente = ambiente, CodiceAzienda = an.m01_codice, SogliaReport = an.m01_soglia_report.Value, CodiceFinservice = an.m01_codice_rc, CodicePaylineCerved = an.m01_codice_payline_cerved }).FirstOrDefault();
            }
            else
            {
                returnValue = context.vs_aziende_osservacrediti.Join(context.m10_utenti.Where(id => id.m10_iduser == idUtente),
                                                            a => a.m01_idazienda,
                                                            b => b.m10_idazienda,
                                                            (a, b) => a)
                                                            .Select(an => new ElementoOperatore { Id = an.m01_idazienda, IdCentro = an.m01_idcentro.Value, RagioneSociale = an.m01_ragionesociale, Ambiente = ambiente, CodiceAzienda = an.m01_codice, SogliaReport = an.m01_soglia_report.Value, CodiceFinservice = an.m01_codice_rc, CodicePaylineCerved = an.m01_codice_payline_cerved }).FirstOrDefault();
            }
            if (contextToBeClosed)
            {
                context = null;
            }
            return returnValue;
        }

        public static ElementoOperatore GetAziendaCodice(string codiceAzienda, DemoR2Entities context, string ambiente)
        {
            bool contextToBeClosed = false;
            if (context == null)
            {
                context = new DemoR2Entities();
                contextToBeClosed = true;
            }
            ElementoOperatore returnValue = new ElementoOperatore();
            if (ambiente == "STANDARD")
            {
                returnValue = context.vs_aziende_osservamercati.Where(az => az.m01_codice == codiceAzienda)
                    .Select(an => new ElementoOperatore { Id = an.m01_idazienda, IdCentro = an.m01_idcentro.Value, RagioneSociale = an.m01_ragionesociale, Ambiente = ambiente, CodiceAzienda = an.m01_codice, SogliaReport = an.m01_soglia_report.Value, CodiceFinservice = an.m01_codice_rc, CodicePaylineCerved = an.m01_codice_payline_cerved }).FirstOrDefault();
            }
            else
            {
                returnValue = returnValue = context.vs_aziende_osservamercati.Where(az => az.m01_codice == codiceAzienda)
                    .Select(an => new ElementoOperatore { Id = an.m01_idazienda, IdCentro = an.m01_idcentro.Value, RagioneSociale = an.m01_ragionesociale, Ambiente = ambiente, CodiceAzienda = an.m01_codice, SogliaReport = an.m01_soglia_report.Value, CodiceFinservice = an.m01_codice_rc, CodicePaylineCerved = an.m01_codice_payline_cerved }).FirstOrDefault();
            }
            if (contextToBeClosed)
            {
                context = null;
            }
            return returnValue;
        }

        public static m10_utenti GetUtenteAzienda(string codiceAzienda, DemoR2Entities context)
        {
            return context.m10_utenti
                .Where(az => az.m10_demo == false && az.m10_idruolo == 2 && az.m10_attivo == true)
                .Join(context.vs_aziende_osservamercati.Where(az => az.m01_codice == codiceAzienda),
                a => a.m10_idazienda,
                b => b.m01_idazienda,
                (a, b) => a).FirstOrDefault();

        }

        public static m10_utenti GetUtenteAziendaImportazioneWS(string codiceAzienda, DemoR2Entities context)
        {
            return context.m10_utenti
                .Where(az => az.m10_demo == false && az.m10_idruolo == 2 && az.m10_attivo == true && az.m10_attivo_notturno == true)
                .Join(context.vs_aziende_osservamercati.Where(az => az.m01_codice == codiceAzienda),
                a => a.m10_idazienda,
                b => b.m01_idazienda,
                (a, b) => a).FirstOrDefault();

        }

        public static m10_utenti GetUtenteAziendaImportazionepreferiti(int codiceAzienda, DemoR2Entities context)
        {
            return context.m10_utenti
                .Where(az => az.m10_demo == false && az.m10_idruolo == 2 && az.m10_attivo == true && az.m10_idazienda == codiceAzienda).FirstOrDefault();

        }

        public static NavigationUser GetUtente(long idUtente)
        {
            NavigationUser returnValue = null;

            using (DemoR2Entities context = new DemoR2Entities())
            {
                returnValue =
                    context.m10_utenti.Where(ut => ut.m10_iduser == idUtente)
                    .Select(u => new NavigationUser
                    {
                        Username = u.m10_nomeutente,
                        Email = u.m10_email,
                        IdRuolo = u.m10_idruolo.Value,
                        IdAzienda = u.m10_idazienda.Value
                    })
                    .FirstOrDefault();
            }
            return returnValue;
        }

        public static NavigationUser GetCentro(long idUtente)
        {
            NavigationUser returnValue = null;
            int idAzienda = GetUtente(idUtente).IdAzienda;

            using (DemoR2Entities context = new DemoR2Entities())
            {

                returnValue = context.vs_aziende_osservamercati.Where(az => az.m01_idazienda == idAzienda)
                    .Select(u => new NavigationUser
                    {
                        IdMercato = u.m01_idcentro.Value

                    }).FirstOrDefault();

            }
            return returnValue;
        }

        public static NavigationUser GetMercato(int idMercato)
        {
            NavigationUser returnValue = null;

            using (DemoR2Entities context = new DemoR2Entities())
            {

                returnValue = context.m00_centri.Where(mr => mr.m00_idcentro == idMercato)
                    .Select(u => new NavigationUser
                    {
                        Mercato = u.m00_centro

                    }).FirstOrDefault();

            }
            return returnValue;
        }

        public static List<NavigationUser> GetUtenti()
        {
            List<NavigationUser> returnValue = null;

            using (DemoR2Entities context = new DemoR2Entities())
            {
                returnValue =
                    context.m10_utenti
                    .Select(u => new NavigationUser
                    {
                        NetUser = context.vs_aziende_osservamercati.Where(az => az.m01_idazienda == u.m10_idazienda).FirstOrDefault() != null ?
                            context.vs_aziende_osservamercati.Where(az => az.m01_idazienda == u.m10_idazienda).FirstOrDefault().m01_net_user : "",
                        NetPwd = context.vs_aziende_osservamercati.Where(az => az.m01_idazienda == u.m10_idazienda).FirstOrDefault() != null ?
                            context.vs_aziende_osservamercati.Where(az => az.m01_idazienda == u.m10_idazienda).FirstOrDefault().m01_net_password : "",
                        Name = u.m10_user,
                        Username = u.m10_nomeutente,
                        Password = u.m10_pass,
                        Email = u.m10_email,
                        IdRuolo = u.m10_idruolo.HasValue ? u.m10_idruolo.Value : 0,
                        Demo = u.m10_demo.HasValue ? u.m10_demo.Value : false,
                        IdAzienda = u.m10_idazienda.HasValue ? u.m10_idazienda.Value : 0,
                        Azienda = context.vs_aziende_osservamercati.Where(az => az.m01_idazienda == u.m10_idazienda).FirstOrDefault() != null ?
                            context.vs_aziende_osservamercati.Where(az => az.m01_idazienda == u.m10_idazienda).FirstOrDefault().m01_ragionesociale : "",
                        IdUser = u.m10_iduser,
                        Abilitato = u.m10_attivo.HasValue ? u.m10_attivo.Value : false,
                        AbilitatoNotturno = u.m10_attivo_notturno.HasValue ? u.m10_attivo_notturno.Value : false,
                        AbilitatoRicerca = u.m10_attivo_rircerca.HasValue ? u.m10_attivo_rircerca.Value : false,
                    })
                    .OrderBy(ord => ord.Username)
                    .ToList();
            }
            return returnValue;
        }


        public static bool AggiornaCodiceRichiesta(long codiceRichiesta, string partitaIva, long idLoggedUser)
        {
            bool returnValue = false;
            using (DemoR2Entities context = new DemoR2Entities())
            {
                cerved_check aziendaUpdateCodice =
                    context.cerved_check.Where(an => an.partita_iva == partitaIva && an.id_utente == idLoggedUser)
                    .OrderByDescending(d => d.dt_inserimento)
                    .FirstOrDefault();
                if (aziendaUpdateCodice != null)
                {
                    aziendaUpdateCodice.codice_cerved = codiceRichiesta.ToString();
                    context.SaveChanges();
                }
            }
            return returnValue;

        }


        public static List<m02_anagrafica> AnagrafichePayline(string codiceAzienda)
        {
            List<m02_anagrafica> returnValue = new List<m02_anagrafica>();
            List<m02_anagrafica> bufferValue = new List<m02_anagrafica>();
            DateTime dataTemp = DateTime.Now.AddMonths(-13);
            DateTime dataInizio = new DateTime(dataTemp.Year, dataTemp.Month, 1);
            using (DemoR2Entities context = new DemoR2Entities())
            {
                bufferValue =
                    context.m04_vendite.Where(ven => ven.m04_codiceazienda == codiceAzienda && ven.m04_dtdocvendita >= dataInizio)
                        .Select(sel => sel.m04_partitaiva)
                        .Distinct()
                        .Join(context.m02_anagrafica,
                        a => a,
                        b => b.m02_partitaiva,
                        (a, b) => b).ToList();
            }

            foreach (var valore in bufferValue)
            {
                var result = 0;
                if (int.TryParse(valore.m02_partitaiva.Substring(0, 1), out result))
                {
                    returnValue.Add(valore);
                }

            }

            return returnValue;
        }

        public static List<m04_vendite> MovimentiPayLine(string codiceAzienda)
        {
            List<m04_vendite> returnValue = new List<m04_vendite>();
            List<m04_vendite> bufferValue = new List<m04_vendite>();
            DateTime dataTemp = DateTime.Now.AddMonths(-13);
            DateTime dataInizio = new DateTime(dataTemp.Year, dataTemp.Month, 1);
            using (DemoR2Entities context = new DemoR2Entities())
            {
                bufferValue =
                    context.m04_vendite.Where(ven => ven.m04_codiceazienda == codiceAzienda && ven.m04_dtdocvendita >= dataInizio).ToList();
            }

            foreach (var valore in bufferValue)
            {
                var result = 0;
                if (valore.m04_partitaiva != String.Empty)
                {
                    if (int.TryParse(valore.m04_partitaiva.Substring(0, 1), out result))
                    {
                        returnValue.Add(valore);
                    }
                }
            }
            Console.WriteLine("****** " + returnValue.Count);
            return returnValue;
        }


        public static bool AggiornaAnagraficaFidoEventiNegativiPecStato(string partitaIva, string ragioneSociale, Int16 valutazione, decimal fido,
                                                                        string eventiNegativi, string pec, string statoAzienda, string filename)
        {
            Log.Info("START AggiornaAnagraficaFidoEventiNegativiPecStato patritaIva= " + partitaIva);

            bool returnValue = false;
            using (DemoR2Entities context = new DemoR2Entities())
            {
                m02_anagrafica anag =
                    context.m02_anagrafica.Where(an => an.m02_partitaiva == partitaIva).FirstOrDefault();
                if (anag != null)
                {
                    if (valutazione != 0) { anag.m02_stato_validazione_cerved = valutazione; } else { anag.m02_stato_validazione_cerved = 0; }
                    if (!ragioneSociale.Equals("")) { anag.m02_ragionesociale = ragioneSociale; }
                    if (fido != 0) { anag.m02_fido = fido; }
                    if (eventiNegativi != null) { anag.m02_eventi_negativi = eventiNegativi; }
                    if (!pec.Equals("")) { anag.m02_pec = pec; }
                    if (statoAzienda != null) { anag.m02_stato_attivita = statoAzienda; }
                    anag.m02_updadm = true;
                    if (!filename.Equals(""))
                    {
                        anag.m02_note = filename;
                    }
                    else
                    {
                        Log.Error("errore filename null");
                    }
                    context.SaveChanges();
                    returnValue = true;
                }
            }
            return returnValue;
        }

        public static string AggiornaAnagraficaFidoEventiNegativiPecStatoMonitoraggio(string partitaIva, string ragioneSociale, Int16 valutazione, decimal fido,
                                                                        string eventiNegativi, string pec, string statoAzienda, string filename)
        {
            Log.Info("START AggiornaAnagraficaFidoEventiNegativiPecStato patritaIva= " + partitaIva);

            string returnValue = "";
            using (DemoR2Entities context = new DemoR2Entities())
            {
                m02_anagrafica anag =
                    context.m02_anagrafica.Where(an => an.m02_ragionesociale.Contains(ragioneSociale)).FirstOrDefault();
                if (anag != null)
                {
                    if (valutazione != 0) { anag.m02_stato_validazione_cerved = valutazione; }
                    if (!ragioneSociale.Equals("")) { anag.m02_ragionesociale = ragioneSociale; }
                    if (fido != 0) { anag.m02_fido = fido; }
                    if (eventiNegativi != null || "".Equals(eventiNegativi)) { anag.m02_eventi_negativi = eventiNegativi; }
                    if (!pec.Equals("")) { anag.m02_pec = pec; }
                    if (statoAzienda != null || "".Equals(statoAzienda)) { anag.m02_stato_attivita = statoAzienda; }
                    anag.m02_updadm = true;
                    if (!filename.Equals(""))
                    {
                        anag.m02_note = filename;
                    }
                    else
                    {
                        Log.Error("errore filename null");
                    }
                    context.SaveChanges();
                    Log.Debug("AggiornaAnagraficaFidoEventiNegativiPecStatoMonitoraggio Ok per partita iva= " + anag.m02_partitaiva + " rating= " + valutazione + " fido= " + fido);
                    Console.WriteLine("Salvataggio Succesfull Piva= " + anag.m02_partitaiva + " rating= " + valutazione + " fido= " + fido);

                    returnValue = anag.m02_partitaiva;
                }
            }
            return returnValue;
        }
        public static bool AggiornaEvasioneRichiesta(string codiceRichiesta, long idLoggedUser)
        {
            bool returnValue = false;
            using (DemoR2Entities context = new DemoR2Entities())
            {
                cerved_check aziandaUpdateCodice =
                    context.cerved_check.Where(an => an.codice_cerved == codiceRichiesta && an.id_utente == idLoggedUser)
                    .OrderByDescending(d => d.dt_inserimento)
                    .FirstOrDefault();
                if (aziandaUpdateCodice != null)
                {
                    aziandaUpdateCodice.dt_aggiornamento = DateTime.Now;
                    aziandaUpdateCodice.evaso = true;
                    context.SaveChanges();
                }
            }
            return returnValue;

        }


        public static bool InserisciChekMonitoraggio(int idUser, string partitaIva, DateTime dataAggiornamento, DateTime dataInserimento)
        {
            Log.Info("begin InserisciChekMonitoraggio**");
            Log.Debug("partita iva= " + partitaIva + " idUser= " + idUser);
            bool returnValue = false;
            try
            {
                using (DemoR2Entities context = new DemoR2Entities())
                {
                    cerved_check nuovoMonitoraggio;
                    nuovoMonitoraggio = new cerved_check();
                    nuovoMonitoraggio.id_utente = idUser;
                    nuovoMonitoraggio.partita_iva = partitaIva;
                    nuovoMonitoraggio.dt_aggiornamento = dataAggiornamento;
                    nuovoMonitoraggio.dt_inserimento = dataInserimento;
                    returnValue = true;
                    context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Log.Error("error InserisciChekMonitoraggio " + e.ToString());
            }
            Log.Info("end InserisciChekMonitoraggio**");
            return returnValue;
        }

        public static bool Insertreport(string piva, long idUtente, dynamic json, string filename)
        {
            Log.Info("START InsertReport** " + "partita iva=" + piva + " idutente=" + idUtente);

            string ignored = JsonConvert.SerializeObject(json,
            Formatting.Indented,
            new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore });

            bool sw = false;
            bool returnValue = false;
            string cervedGroupScore = "";
            string cervedGroupScoreDescrizione = "";
            string coloreIntermedioCliente = "";
            string globalScore = "";
            string globalScoreDescrizione = "";
            string statoAttivita = "";
            string eventiNegativi = "";
            string ragioneSociale = "";
            string fidoStr = "";
            string mercato = "";
            string mailTo = "";
            string pec = ""; /*OK ??*/
            decimal fido = 50m;

            try
            {
                try { pec = (string)json.report.contactInformation.emailAddresses[0]; } catch { pec = "pec non presente"; }
                try { ragioneSociale = (string)json.report.alternateSummary.businessName; } catch { ragioneSociale = "ragione sociale non presente"; }
                try { cervedGroupScore = (string)json.report.creditScore.currentCreditRating.providerValue.value; } catch { cervedGroupScore = "0"; }
                try { cervedGroupScoreDescrizione = (string)json.report.companySummary.creditRating.commonDescription; } catch { cervedGroupScoreDescrizione = "non presente"; }
                try { globalScoreDescrizione = (string)json.report.companySummary.creditRating.commonDescription; } catch { globalScoreDescrizione = "non presente"; }
                try { globalScore = (string)json.report.creditScore.currentCreditRating.providerValue.value; } catch { globalScore = "0"; }

                if (globalScore.Contains("No"))
                {
                    globalScore = "0";
                }

                /*COEFFICIENTE CLIENTE???*/

                try { statoAttivita = (string)json.report.companySummary.companyStatus.status; } catch { statoAttivita = "non presente"; }
                try { eventiNegativi = (string)json.report.additionalInformation.shareholdingCompanies[0].hasPrejudicials; } catch { eventiNegativi = "non presente"; }
                try { fidoStr = (string)json.report.creditScore.currentCreditRating.creditLimit.value; } catch { fidoStr = ""; }
                fidoStr = fidoStr.Replace(".", ",");
                fidoStr = fidoStr == "ND" ? "0" : fidoStr;
                fido = Convert.ToDecimal(fidoStr);

                if (idUtente == 1363)
                {
                    mercato = "CAAB BOLOGNA";
                    mailTo = ConfigurationManager.AppSettings["ToBologna"].ToString();
                }
                else
                {
                    if (DBHandler.GetCentro(idUtente).IdMercato == 8)
                    {
                        mercato = "MOF Fondi";
                        mailTo = ConfigurationManager.AppSettings["ToFondi"].ToString();
                    }
                    else if (DBHandler.GetCentro(idUtente).IdMercato == 7)
                    {
                        mercato = "CAAB BOLOGNA";
                        mailTo = ConfigurationManager.AppSettings["ToBologna"].ToString();
                    }
                    else
                    {
                        sw = true;
                        mercato = "ADMIN";
                        mailTo = ConfigurationManager.AppSettings["To"].ToString();
                    }
                }
                /*if (!sw)
                {
                    if (MailHandler.SendMail(idUtente, "RECEIVED_REPORT", piva, ragioneSociale, mercato, mailTo)) { Log.Info("Mail Inviata"); }
                }*/
            }
            catch (Exception ex)
            {
                Log.Error("InsertReport -- error : insert report : " + ex.Message);
                sw = true;
            }
            finally
            {
                try
                {
                    if (!DBHandler.AggiornaAnagraficaFidoEventiNegativiPecStato(piva, ragioneSociale, Int16.Parse(globalScore), fido, eventiNegativi, pec, statoAttivita, filename))
                    {
                        DBHandler.InsertAnagrafica(json, piva, pec, filename);
                        DBHandler.AggiornaAnagraficaFidoEventiNegativiPecStato(piva, ragioneSociale, Int16.Parse(globalScore), fido, eventiNegativi, pec, statoAttivita, filename);
                    }
                    if (!sw)
                    {
                        using (DemoR2Entities context = new DemoR2Entities())
                        {
                            var newsEsistente = context.news_operatore
                                .Where(news => news.partita_iva == piva).ToList();

                            if (newsEsistente.Count > 0)
                            {
                                DBHandler.AggiornaNewsReport(piva, filename, globalScore, fido, ragioneSociale);
                                Console.WriteLine("*******AGGIORNATA ANAG******* " + piva);
                            }
                            else
                            {
                                DBHandler.InserisciNewsReport(piva, ragioneSociale, statoAttivita, int.Parse(globalScore), "", "", "", idUtente, eventiNegativi, fido, "NuovoReport", DateTime.Now, "STANDARD");
                                Console.WriteLine("*******INSERITA ANAG******* " + piva);
                            }
                        }
                    }
                    DBHandler.InserisciRating(piva, globalScore, globalScoreDescrizione, "", coloreIntermedioCliente, cervedGroupScore, cervedGroupScoreDescrizione);
                    Log.Info("InsertReport -- Aggiornata Anagrafica, inserito rating.");
                    returnValue = true;
                }
                catch (Exception e)
                {
                    Log.Error("InsertReport -- KO Updating Dabase " + e.ToString());
                }
            }

            return returnValue;
        }
        public static void AggiornaNewsReport(string piva, string fileName, string globalScore, decimal fido, string ragioneSociale)
        {
            Log.Info("START AggiornaNewsReport partitaIva= " + piva);
            Console.WriteLine("Aggiorno le news per la PartitaIva=" + piva);
            using (DemoR2Entities context = new DemoR2Entities())
            {
                var newsEsistente = context.news_operatore
                    .Where(news => news.partita_iva == piva).ToList();


                if (newsEsistente != null)
                {
                    foreach (news_operatore element in newsEsistente)
                    {
                        //   short s;
                        element.rapporto = fileName;
                        element.valutazione = short.Parse(globalScore);
                        element.ragione_sociale = ragioneSociale;
                        element.fido = fido;
                        context.SaveChanges();
                    }
                    Console.WriteLine("Aggiornate n° " + newsEsistente.Count + " news");
                }
            }
        }
        public static void AggiornaNewsReport(string fileName, string piva)
        {
            Console.WriteLine("Aggiorno le news per la PartitaIva=" + fileName);
            using (DemoR2Entities context = new DemoR2Entities())
            {
                var newsEsistente = context.news_operatore
                    .Where(news => news.partita_iva == piva).ToList();


                if (newsEsistente != null)
                {
                    foreach (news_operatore element in newsEsistente)
                    {
                        element.rapporto = fileName;
                        context.SaveChanges();
                    }
                    Console.WriteLine("Aggiornate n° " + newsEsistente.Count + " news");
                }
            }
        }
        public static void AggiornaNewsReport(string piva, string fileName, string globalScore, decimal fido)
        {
            Log.Info("START AggiornaNewsReport partitaIva= " + piva);
            Console.WriteLine("Aggiorno le news per la PartitaIva=" + piva);
            using (DemoR2Entities context = new DemoR2Entities())
            {
                var newsEsistente = context.news_operatore
                    .Where(news => news.partita_iva == piva).ToList();


                if (newsEsistente != null)
                {
                    foreach (news_operatore element in newsEsistente)
                    {
                        //   short s;
                        element.rapporto = fileName;
                        element.valutazione = short.Parse(globalScore);
                        element.fido = fido;
                        context.SaveChanges();
                    }
                    Console.WriteLine("Aggiornate n° " + newsEsistente.Count + " news");
                }
            }
        }
        public static void InsertAnagrafica(dynamic json, string piva, string pec, string filename)
        {
            Log.Info("start InsertAnagrafica Partita Iva= " + piva + " file Name= " + filename);

            string telefono = "";
            string prefisso = "";
            string codiceFiscale = "";
            string indirizzo = "";
            string comune = "";
            string eventiNegativi = "";
            string descAttivita = "";
            string descRea = "";
            string ragioneSociale = "";
            string nrea = "";
            string note = "";
            decimal fido = 50m;
            string globalScore = "";

            string ignored = JsonConvert.SerializeObject(json,
            Formatting.Indented,
            new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore });

            try { globalScore = Int16.Parse(Convert.ToString(json.report.creditScore.currentCreditRating.providerValue.value)); } catch { globalScore = "0"; }
            try { ragioneSociale = (string)json.report.alternateSummary.businessName; } catch { ragioneSociale = "ragione sociale non presente"; }
            try { indirizzo = (string)json.report.alternateSummary.address; } catch { indirizzo = ""; }
            try { codiceFiscale = (string)json.report.alternateSummary.taxCode; } catch { codiceFiscale = ""; }
            try { comune = (string)json.report.alternateSummary.taxCode; } catch { comune = ""; }
            try { eventiNegativi = (string)json.report.additionalInformation.shareholdingCompanies[0].hasPrejudicials; } catch { eventiNegativi = "non presente"; }
            try { nrea = (string)json.report.alternateSummary.companyRegistrationNumber; } catch { nrea = ""; }
            try { descAttivita = (string)json.report.additionalInformation.activities.companyActivityDescription; } catch { descAttivita = ""; }
            try { descRea = (string)json.report.alternateSummary.reaInscriptionDateription; } catch { descRea = ""; }
            try { telefono = (string)json.report.alternateSummary.telephone; } catch { telefono = ""; }
            try { note = filename; } catch { note = ""; }
            try { Int16.Parse(globalScore); } catch { globalScore = "0"; }
            using (DemoR2Entities context = new DemoR2Entities())
            {
                m02_anagrafica anagraficaUpdate = new m02_anagrafica();

                anagraficaUpdate.m02_ragionesociale = ragioneSociale;
                // anagraficaUpdate.m02_cap = azienda.addresses.Length > 0 ? azienda.addresses[0].zipCode : "";
                anagraficaUpdate.m02_codicefiscale = codiceFiscale;
                anagraficaUpdate.m02_comune = comune;
                anagraficaUpdate.m02_indirizzo = indirizzo;
                anagraficaUpdate.m02_nazione = " ";
                anagraficaUpdate.m02_dtupdate = DateTime.Now;
                anagraficaUpdate.m02_stato_validazione_cerved = Int16.Parse(globalScore);
                anagraficaUpdate.m02_updadm = true;
                anagraficaUpdate.m02_note = "---";
                anagraficaUpdate.m02_telefono = telefono;
                anagraficaUpdate.m02_prefisso = prefisso;
                anagraficaUpdate.m02_note = note;
                anagraficaUpdate.m02_partitaiva = piva;

                anagraficaUpdate.m02_fido = fido;
                anagraficaUpdate.m02_pec = pec;
                anagraficaUpdate.m02_eventi_negativi = eventiNegativi;
                anagraficaUpdate.m02_nrea = nrea;
                anagraficaUpdate.m02_cciaa = comune;
                anagraficaUpdate.m02_descrizioneAttivita = descAttivita;
                anagraficaUpdate.m02_iscrizioneCCIAA = descRea;
                context.m02_anagrafica.AddObject(anagraficaUpdate);
                context.SaveChanges();
                Log.Info("Inserimento avvenuto con successo");
            }
        }

        public static bool InserisciRichiesteReportFornitore(string partitaIva, int idUser, string emailUser, DateTime dataInizio, DateTime dataFine, string tipoReport, NavigationUser centro)
        {
            Log.Info("begin InserisciRichiesteReportFornitore**");
            Log.Debug("partita iva " + partitaIva + " iduser " + idUser.ToString());
            bool returnValue = false;
            try
            {
                using (DemoR2Entities context = new DemoR2Entities())
                {
                    richiesta_report nuovaRichiesta = null;
                    nuovaRichiesta = new richiesta_report();
                    nuovaRichiesta.tipo_richiesta = "REPORT_GLOBALE";
                    nuovaRichiesta.evasa = false;
                    nuovaRichiesta.data_richiesta = DateTime.Now;
                    nuovaRichiesta.email_utente = emailUser;
                    nuovaRichiesta.id_utente = idUser;
                    nuovaRichiesta.chiave = partitaIva;
                    nuovaRichiesta.id_centro = centro.IdMercato;
                    context.richiesta_report.AddObject(nuovaRichiesta);
                    returnValue = true;
                    context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Log.Error("error InserisciRichiesteReportFornitore " + e.ToString());
            }
            Log.Info("end InserisciRichiesteReportFornitore**");
            return returnValue;
        }


        public static bool InserisciRichiesteReportGlobale(string codiceOperatore, int idUser, string emailUser)
        {
            bool returnValue = false;
            using (DemoR2Entities context = new DemoR2Entities())
            {
                richiesta_report nuovaRichiesta =
                context.richiesta_report.Where(richiesta => richiesta.chiave == codiceOperatore && richiesta.id_utente == idUser && richiesta.tipo_richiesta == "REPORT_GLOBALE" && richiesta.evasa == false).FirstOrDefault();
                if (nuovaRichiesta == null)
                {
                    nuovaRichiesta = new richiesta_report();
                    nuovaRichiesta.tipo_richiesta = "REPORT_GLOBALE";
                    nuovaRichiesta.evasa = false;
                    nuovaRichiesta.data_richiesta = DateTime.Now;
                    nuovaRichiesta.email_utente = emailUser;
                    nuovaRichiesta.id_utente = idUser;
                    nuovaRichiesta.chiave = codiceOperatore;
                    nuovaRichiesta.id_centro = 0;
                    context.richiesta_report.AddObject(nuovaRichiesta);
                    returnValue = true;
                    context.SaveChanges();

                }



            }
            return returnValue;
        }

        public static bool InserisciRichiesteReportRP(string codiceOperatore, int idUser, string emailUser, DateTime dataInizio, DateTime dataFine)
        {
            bool returnValue = false;
            using (DemoR2Entities context = new DemoR2Entities())
            {
                richiesta_report nuovaRichiesta =
                context.richiesta_report.Where(richiesta => richiesta.chiave == codiceOperatore && richiesta.id_utente == idUser && richiesta.tipo_richiesta == "REPORT_RP" && richiesta.evasa == false).FirstOrDefault();
                if (nuovaRichiesta == null)
                {
                    nuovaRichiesta = new richiesta_report();
                    nuovaRichiesta.tipo_richiesta = "REPORT_RP";
                    nuovaRichiesta.evasa = false;
                    nuovaRichiesta.data_richiesta = DateTime.Now;
                    nuovaRichiesta.email_utente = emailUser;
                    nuovaRichiesta.id_utente = idUser;
                    nuovaRichiesta.chiave = codiceOperatore;
                    //nuovaRichiesta.data_inizio = dataInizio;
                    nuovaRichiesta.data_fine = dataFine;
                    nuovaRichiesta.id_centro = 0;
                    context.richiesta_report.AddObject(nuovaRichiesta);
                    returnValue = true;
                    context.SaveChanges();

                }



            }
            return returnValue;
        }

        public static bool InserisciRichiesteReportCN(string codice, int idUser, string emailUser, DateTime dataInizio, DateTime dataFine)
        {
            bool returnValue = false;
            using (DemoR2Entities context = new DemoR2Entities())
            {
                richiesta_report nuovaRichiesta =
                context.richiesta_report.Where(richiesta => richiesta.chiave == codice && richiesta.id_utente == idUser && richiesta.tipo_richiesta == "REPORT_CN" && richiesta.evasa == false).FirstOrDefault();
                if (nuovaRichiesta == null)
                {
                    nuovaRichiesta = new richiesta_report();
                    nuovaRichiesta.tipo_richiesta = "REPORT_CN";
                    nuovaRichiesta.evasa = false;
                    nuovaRichiesta.data_richiesta = DateTime.Now;
                    nuovaRichiesta.email_utente = emailUser;
                    nuovaRichiesta.id_utente = idUser;
                    nuovaRichiesta.chiave = codice;
                    // nuovaRichiesta.data_inizio = dataInizio;
                    nuovaRichiesta.data_fine = dataFine;
                    nuovaRichiesta.id_centro = 0;
                    context.richiesta_report.AddObject(nuovaRichiesta);
                    returnValue = true;
                    context.SaveChanges();

                }



            }
            return returnValue;
        }

        public static bool InserisciVendita(string codiceAzienda, string partitaIva, string codiceFiscale, DateTime dataDocVendita, string numDoc, string tipoDoc, DateTime dataScadenza, Double importo)
        {
            bool returnValue = false;
            using (DemoR2Entities context = new DemoR2Entities())
            {
                try
                {
                    m04_vendite vendita = new m04_vendite();

                    vendita.m04_chiusura = false;
                    vendita.m04_codiceazienda = codiceAzienda;
                    vendita.m04_codicefiscale = codiceFiscale;
                    vendita.m04_dtchiusura = null;
                    vendita.m04_dtrifvendita = dataDocVendita;
                    vendita.m04_dtdocvendita = dataDocVendita;
                    vendita.m04_dtscadenza = dataScadenza;
                    vendita.m04_dtupdate = DateTime.Now;
                    vendita.m04_importo = importo;
                    vendita.m04_numdoc = numDoc;
                    vendita.m04_partitaiva = partitaIva;
                    vendita.m04_tipodoc = tipoDoc;
                    vendita.m04_alimentazione = "M";

                    context.m04_vendite.AddObject(vendita);

                    context.SaveChanges();

                    returnValue = true;
                }
                catch (Exception e)
                {
                    Log.Error("error : " + e.ToString());
                }
            }
            return returnValue;
        }

        public static bool ChiudiVendita(string codiceAzienda, string partitaIva, DateTime dataDocVendita, string numDoc, string tipoDoc, Double importo)
        {
            bool returnValue = false;
            using (DemoR2Entities context = new DemoR2Entities())
            {
                try
                {
                    m04_vendite vendita =
                        context.m04_vendite.Where(ven =>
                            ven.m04_codiceazienda == codiceAzienda &&
                            ven.m04_dtdocvendita == dataDocVendita &&
                            ven.m04_importo == importo &&
                            ven.m04_numdoc == numDoc).FirstOrDefault();

                    if (vendita != null)
                    {
                        vendita.m04_dtchiusura = DateTime.Now;
                        vendita.m04_chiusura = true;
                        vendita.m04_alimentazione_chiusura = "M";
                        context.SaveChanges();
                        returnValue = true;
                    }
                }
                catch (Exception e)
                {
                    Log.Error("error : " + e.ToString());
                }
            }
            return returnValue;
        }

        public static bool InserisciFattureRichiestaRecuperoCrediti(int idRichiesta, List<ElementoInserimentoFatturaRecuperoCrediti> fatture)
        {

            bool returnValue = false;
            try
            {
                using (DemoR2Entities context = new DemoR2Entities())
                {
                    foreach (ElementoInserimentoFatturaRecuperoCrediti fattura in fatture)
                    {
                        m27_fattura_richiesta nuovaFattura = new m27_fattura_richiesta();

                        nuovaFattura.m27_numero_documento = fattura.NumeroDocumento;
                        nuovaFattura.m27_data_documento = Convert.ToDateTime(fattura.DataDocumento);
                        nuovaFattura.m27_data_scadenza_documento = Convert.ToDateTime(fattura.DataScadenzaDocumento);
                        Decimal importoDocumento = Convert.ToDecimal(fattura.ImportoDocumento.Replace("---", "0").Replace("€", "").Replace(".", ""));
                        nuovaFattura.m27_importo_documento = importoDocumento;
                        nuovaFattura.m27_id_richiesta = idRichiesta;


                        context.m27_fattura_richiesta.AddObject(nuovaFattura);
                    }
                    context.SaveChanges();
                    /* todo m04_vendite vendita = new m04_vendite();
                    foreach (ElementoInserimentoFatturaRecuperoCrediti fattura in fatture)
                    {
                        DateTime datascad = Convert.ToDateTime(fattura.DataDocumento);
                        vendita = context.m04_vendite.Where(ve => ve.m04_numdoc.Equals(fattura.NumeroDocumento)
                                                             && ve.m04_dtscadenza == datascad).FirstOrDefault();
                        vendita.m04_anticipoFatt = 1;
                        context.SaveChanges();
                    }*/
                }
            }
            catch (SqlException sql)
            {
                Log.Error(sql.ToString());
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
            return returnValue;
        }

        public static long InserisciRichiestaRecuperoCrediti(string tipologiaRecupero, string codiceAzienda, String partitaIva, string ragioneSociale, List<ElementoInserimentoFatturaRecuperoCrediti> fatture)
        {
            long returnValue = 0;
            using (DemoR2Entities context = new DemoR2Entities())
            {
                m10_utenti utenteAzienda = GetUtenteAzienda(codiceAzienda, context);

                m26_richiesta_recupero_crediti nuovaRichiesta = new m26_richiesta_recupero_crediti();
                nuovaRichiesta.m26_codice_azienda = Convert.ToInt32(codiceAzienda);
                nuovaRichiesta.m26_ragione_sociale = ragioneSociale.Trim();
                nuovaRichiesta.m26_partita_iva = partitaIva.Trim();
                nuovaRichiesta.m26_data_ultima_modifica = DateTime.Now;
                nuovaRichiesta.m26_data_inserimento = DateTime.Now;
                nuovaRichiesta.m26_codice_richiedente = "0";
                nuovaRichiesta.m26_tipo_richiesta = tipologiaRecupero;
                nuovaRichiesta.m26_inviato = false;
                nuovaRichiesta.m26_chiusa = false;
                nuovaRichiesta.m26_incasso = 0;
                nuovaRichiesta.m26_stato_richiesta = "PENDING";


                context.m26_richiesta_recupero_crediti.AddObject(nuovaRichiesta);

                context.SaveChanges();
                int idRichiesta = nuovaRichiesta.m26_id;
                InserisciFattureRichiestaRecuperoCrediti(idRichiesta, fatture);

                returnValue = idRichiesta;
                List<string> additionalData = new List<string>();
                additionalData.Add(ragioneSociale);
                additionalData.Add(partitaIva);
                additionalData.Add(idRichiesta.ToString());

                MailHandler.WarnMail(utenteAzienda.m10_iduser, "RC_INSERIMENTO", additionalData);

            }

            return returnValue;

        }

        public static Boolean CreaTracciatoCsv(string partitaIva, List<ElementoInserimentoFatturaRecuperoCrediti> fatture)
        {
            Boolean returnValue = false;

            try
            {
                string filePath = CreaFile(partitaIva);
                if (filePath.Equals(""))
                {
                    Log.Error("file non creato errore ");
                    returnValue = false;
                }
                else if (ScriviFileTracciato(fatture, filePath).Equals("1"))
                {
                    returnValue = true;
                }
            }
            catch (IOException e)
            {
                Log.Error(e.ToString());
                return returnValue;
            }
            return returnValue;
        }

        private static string ScriviFileTracciato(List<ElementoInserimentoFatturaRecuperoCrediti> fatture, string filePath)
        {
            string returValue = "";
            try
            {
                var csv = new StringBuilder();

                foreach (ElementoInserimentoFatturaRecuperoCrediti el in fatture)
                {
                    string first = el.NumeroDocumento.ToString();
                    string second = el.DataDocumento.ToString();
                    var newLine = string.Format("{0},{1},{2}", first, second, "0");
                    csv.AppendLine(newLine);
                }
                File.WriteAllText(filePath, csv.ToString());
                returValue = "1";
            }
            catch (IOException e)
            {
                Log.Error("file error : " + e.ToString());
                return returValue;
            }
            return returValue;
        }

        private static string CreaFile(string partitaIva)
        {
            string path = "";
            try
            {
                string fileName =
                            String.Format("{0}{1}.csv",
                                               partitaIva,
                                               DateTime.Now.Year);
                //path = WebConfigurationManager.AppSettings["PathAnticipoFatture"]; //*PRODUZIONE*
                path = @"C:\Users\francesco\Desktop\anticipo fatture";             //*TEST*
                if (!Directory.Exists(path))
                {
                    DirectoryInfo di = Directory.CreateDirectory(path);
                }
                path += fileName;
                var myFile = File.Create(path);
                myFile.Close();


            }
            catch (IOException e)
            {
                Console.WriteLine(e.ToString());
            }
            return path;
        }

        public static Boolean InserisciAnticipoFatture(string tipologiaRecupero, string codiceAzienda, String partitaIva, string ragioneSociale, List<ElementoInserimentoFatturaRecuperoCrediti> fatture)
        {
            Boolean returnValue = false;

            returnValue = CreaTracciatoCsv(partitaIva, fatture);

            try
            {
                using (DemoR2Entities context = new DemoR2Entities())
                {
                    m10_utenti utenteAzienda = GetUtenteAzienda(codiceAzienda, context);

                    m26_richiesta_recupero_crediti nuovaRichiesta = new m26_richiesta_recupero_crediti();
                    nuovaRichiesta.m26_codice_azienda = Convert.ToInt32(codiceAzienda);
                    nuovaRichiesta.m26_ragione_sociale = ragioneSociale.Trim();
                    nuovaRichiesta.m26_partita_iva = partitaIva.Trim();
                    nuovaRichiesta.m26_data_ultima_modifica = DateTime.Now;
                    nuovaRichiesta.m26_data_inserimento = DateTime.Now;
                    nuovaRichiesta.m26_codice_richiedente = "0";
                    nuovaRichiesta.m26_tipo_richiesta = "AntFatt";
                    nuovaRichiesta.m26_inviato = false;
                    nuovaRichiesta.m26_data_invio = null;
                    nuovaRichiesta.m26_chiusa = false;
                    nuovaRichiesta.m26_incasso = 0;
                    nuovaRichiesta.m26_stato_richiesta = "RICHIESTA INVIATA";

                    context.m26_richiesta_recupero_crediti.AddObject(nuovaRichiesta);
                    context.SaveChanges();

                    int idRichiesta = nuovaRichiesta.m26_id;
                    InserisciFattureRichiestaRecuperoCrediti(idRichiesta, fatture);
                    string bodyMail = " sono state inviate il giorno " + DateTime.Now + " le fatture n° ";

                    foreach (var el in fatture)
                    {
                        bodyMail += " " + el.NumeroDocumento + " lo stato della richiesta per le citate fatture è : " + nuovaRichiesta.m26_stato_richiesta;
                    }
                    //MailHandler.SendMail(utenteAzienda.m10_email, bodyMail);
                    /*if(! MailHandler.SendMail("francesco.capparucci@gmail.com", bodyMail))
                    {
                        Log.Error("email non inviata");
                    }*/


                    /*
                    returnValue = idRichiesta;
                    List<string> additionalData = new List<string>();
                    additionalData.Add(ragioneSociale);
                    additionalData.Add(partitaIva);
                    additionalData.Add(idRichiesta.ToString());

                    MailHandler.WarnMail(utenteAzienda.m10_iduser, "RC_INSERIMENTO", additionalData);
                    */
                }
            }

            catch (SqlException e)
            {
                Log.Error(e.ToString());
            }
            catch (Exception ee)
            {
                Log.Error(ee.ToString());
            }
            return returnValue;
        }

        public static bool InserisciNewsReport(string partitaIva, string ragioneSociale, string stato, int statoSemaforo,
                                               string statoOsserva, string statoCerved,
                                               string statoSemaforoCerved, long idUser, string eventiNegativi, decimal fido,
                                               string descrizione, DateTime dataSegnalazione, string ambiente)
        {


            DirectoryInfo di = new DirectoryInfo(WebConfigurationManager.AppSettings["ReportPath"]);
            IDictionary<string, DateTime> openWith = new Dictionary<string, DateTime>();
            //string partitaIvaFile = "";
            Boolean aggiornamentoPdf = false;
            bool returnValue = false;

            try
            {
                if (statoOsserva.Equals("true"))
                    aggiornamentoPdf = true;
                /* foreach (var element in di.GetFiles())
                 {
                     int subend = element.Name.IndexOf("_");
                     partitaIvaFile = element.Name.Substring(0, subend);
                     if (partitaIvaFile.Equals(partitaIva))
                     {
                         string dataFile = element.Name.Substring(subend+1,8);
                         dataFile = dataFile.Substring(0, 4)+"/"+dataFile.Substring(4, 2)+"/"+dataFile.Substring(6, 2);

                         if (DateTime.Now > DateTime.Parse(dataFile))
                         {
                             aggiornamentoPdf = true;
                             break;
                         }

                     }
                 }*/


                if (descrizione == "") return returnValue;

                using (DemoR2Entities context = new DemoR2Entities())
                {
                    ElementoOperatore aziendaUtente = GetAziendaUtente(idUser, context, ambiente);
                    var newsEsistente = context.news_operatore
                        .Where(news => news.id_user == idUser &&
                               news.descrizione == descrizione &&
                               news.partita_iva == partitaIva &&
                               (news.data_variazione.Year == dataSegnalazione.Year && news.data_variazione.Month == dataSegnalazione.Month && news.data_variazione.Day == dataSegnalazione.Day)
                               ).FirstOrDefault();


                    if (newsEsistente == null && aziendaUtente != null && partitaIva != String.Empty)
                    {
                        news_operatore news = new news_operatore();
                        news.id_user = idUser;
                        news.data_aggiornamento = DateTime.Now;
                        news.data_variazione = dataSegnalazione;
                        news.riepilogo_variazione = descrizione != "Nuovo Report" ? true : false;
                        news.descrizione = descrizione;
                        news.ragione_sociale = ragioneSociale;
                        news.partita_iva = partitaIva;
                        news.aggiornamento = false;
                        news.descrizione = descrizione;
                        if (!fido.Equals("") || fido == 0)
                            news.fido = fido;
                        news.esposizione =
                            context.m03_saldi.Where(saldo => saldo.m03_partitaiva == partitaIva && saldo.m03_codiceazienda == aziendaUtente.CodiceAzienda)
                                                         .OrderByDescending(saldo => saldo.m03_dtsaldo).FirstOrDefault() != null ?
                                                         context.m03_saldi.Where(saldo => saldo.m03_partitaiva == partitaIva && saldo.m03_codiceazienda == aziendaUtente.CodiceAzienda)
                                                         .OrderByDescending(saldo => saldo.m03_dtsaldo).FirstOrDefault().m03_saldo : 0;

                        news.eventi_negativi = eventiNegativi;
                        news.letto = false;
                        if (stato.Contains("Rated"))
                        {
                            stato = "ND";
                        }
                        try { news.valutazione = Convert.ToInt16(stato); } catch { news.valutazione = 0; }
                        news.rapporto = partitaIva + "_" + DateTime.Now.ToString("yyyyMMdd");
                        news.valutazione = Convert.ToInt16(statoSemaforo);
                        news.osservatorio =
                            context.m04_vendite.Where(ven => ven.m04_codiceazienda == aziendaUtente.CodiceAzienda && ven.m04_partitaiva == partitaIva).Count() > 0;

                        context.news_operatore.AddObject(news);
                        context.SaveChanges();
                        Log.Debug("Salvataggio succesfull pIva= " + partitaIva + " User= " + idUser + " Azienda= " + aziendaUtente.CodiceAzienda);
                        Console.WriteLine("Salvataggio succesfull pIva= " + partitaIva + " User= " + idUser + " Azienda= " + aziendaUtente.CodiceAzienda);
                        returnValue = true;
                    }
                }
                return returnValue;
            }
            catch (Exception e)
            {
                Log.Error(e);
                return returnValue;
            }
        }

        public static bool InserisciNewsReportDirect(string partitaIva, string ragioneSociale, string stato, string statoSemaforo,
                                               string statoOsserva, string statoCerved,
                                               string statoSemaforoCerved, long idUser, string eventiNegativi, decimal fido,
                                               string descrizione, DateTime dataSegnalazione, string ambiente)
        {
            bool returnValue = false;


            using (DemoR2Entities context = new DemoR2Entities())
            {
                try
                {
                    news_operatore news = new news_operatore();
                    news.rapporto = partitaIva;
                    context.news_operatore.AddObject(news);
                    context.SaveChanges();
                    returnValue = true;
                }
                catch (Exception e)
                {
                    Log.Error("error : " + e.ToString());
                }

            }
            return returnValue;
        }

        public static bool InserisciRating(String partitaIva, string stato, string statoSemaforo, string statoOsserva, string statoSemaforoOsserva, string statoCerved, string statoSemaforoCerved)
        {
            Log.Debug("START  InserisciRating  partitaIva= " + partitaIva + " stato " + stato + " statosemaforo " + statoSemaforo + " statoosserva " + statoOsserva);
            bool returnValue = false;
            using (DemoR2Entities context = new DemoR2Entities())
            {
                m05_rating rating = new m05_rating();
                rating.m05_stato = stato;
                rating.m05_stato_semaforo = statoSemaforo;

                rating.m05_stato_cerved = statoCerved;
                rating.m05_stato_semaforo_cerved = statoSemaforoCerved;

                rating.m05_stato_osserva = statoOsserva;
                rating.m05_stato_semaforo_osserva = statoSemaforoOsserva;
                rating.m05_partitaiva = partitaIva;
                rating.m05_dtfinevalidita = DateTime.Now.AddYears(1);
                rating.m05_dtriferimento = DateTime.Now;
                context.m05_rating.AddObject(rating);
                context.SaveChanges();
                returnValue = true;
            }
            Log.Info("insert ok ");
            return returnValue;

        }

        public static bool VerificaEsistenzaRapportoAttivo(string partitaIva)
        {
            bool returnValue = false;
            using (DemoR2Entities context = new DemoR2Entities())
            {
                returnValue =
                    context.cerved_check.Where(ch => ch.partita_iva == partitaIva && ch.codice_cerved != "P")
                    .OrderByDescending(d => d.dt_inserimento)
                    .ToList().Count > 0;
            }
            return returnValue;
        }

        public static string CopiaRapportoUtente(string partitaIva, long idLoggedUser)
        {
            string returnValue = "";
            using (DemoR2Entities context = new DemoR2Entities())
            {
                cerved_check cervedCheckToCopy =
                    context.cerved_check.Where(ch => ch.partita_iva == partitaIva &&
                        ch.id_utente != idLoggedUser).OrderByDescending(or => or.dt_inserimento).FirstOrDefault();
                if (cervedCheckToCopy != null)
                {
                    cerved_check newCervedCheck = new cerved_check();
                    newCervedCheck.codice_cerved = cervedCheckToCopy.codice_cerved;
                    newCervedCheck.dt_aggiornamento = DateTime.Now;
                    newCervedCheck.dt_inserimento = DateTime.Now;
                    newCervedCheck.evaso = true;
                    newCervedCheck.id_utente = idLoggedUser;
                    newCervedCheck.partita_iva = partitaIva;
                    context.cerved_check.AddObject(newCervedCheck);
                    context.SaveChanges();
                    returnValue = cervedCheckToCopy.codice_cerved;
                }

            }
            return returnValue;
        }

        public static bool AggiornaAziendaOsservatorioAccodamentoRichiesta(company azienda, long idLoggedUser, int productCode, string partitaIva)
        {
            bool returnValue = false;
            using (DemoR2Entities context = new DemoR2Entities())
            {
                /*Se esiste aggiorno dati Osservatorio Crediti*/
                if (azienda.vatNumber == null) return returnValue;
                m02_anagrafica anagraficaUpdate =
                context.m02_anagrafica.Where(an => an.m02_partitaiva == partitaIva).FirstOrDefault();
                if (anagraficaUpdate != null)
                {
                    string ragSoc = azienda.name.Trim().Length >= 200 ? azienda.name.Trim().Substring(0, 150) : azienda.name.Trim();
                    anagraficaUpdate.m02_ragionesociale = ragSoc;
                    anagraficaUpdate.m02_cap = azienda.addresses.Length > 0 ? azienda.addresses[0].zipCode : "";
                    anagraficaUpdate.m02_codicefiscale = azienda.fiscalCode;
                    anagraficaUpdate.m02_comune = azienda.addresses.Length > 0 ? azienda.addresses[0].town : "";
                    //anagraficaUpdate.m02_indirizzo = azienda.addresses.Length > 0 ? String.Format("{0}", azienda.addresses[0].street.Trim()) : " ";
                    anagraficaUpdate.m02_nazione = " ";
                    anagraficaUpdate.m02_dtupdate = DateTime.Now;
                    anagraficaUpdate.m02_stato_validazione_cerved = 1;
                    anagraficaUpdate.m02_updadm = true;
                    anagraficaUpdate.m02_note = "---";
                    anagraficaUpdate.m02_telefono = azienda.addresses.Length > 0 ? azienda.addresses[0].phoneNumber : "";
                    anagraficaUpdate.m02_prefisso = azienda.addresses.Length > 0 ? azienda.addresses[0].province : "";

                    anagraficaUpdate.m02_fido = -1;
                    anagraficaUpdate.m02_pec = "";
                    anagraficaUpdate.m02_eventi_negativi = "Non rilevate";
                    anagraficaUpdate.m02_nrea = azienda.reaNumber.HasValue ? azienda.reaNumber.ToString() : "";
                    anagraficaUpdate.m02_cciaa = azienda.reaProvince;
                    anagraficaUpdate.m02_descrizioneAttivita = azienda.cAtecoDescription;
                    anagraficaUpdate.m02_iscrizioneCCIAA = azienda.registrationDate.HasValue ? azienda.registrationDate.Value.ToShortDateString() : "";
                }
                else
                {
                    m02_anagrafica anag = new m02_anagrafica();
                    string ragSoc = azienda.name.Trim().Length >= 200 ? azienda.name.Trim().Substring(0, 150) : azienda.name.Trim();
                    anag.m02_ragionesociale = ragSoc;
                    anag.m02_partitaiva = azienda.vatNumber != null ? azienda.vatNumber.Trim() : "";
                    anag.m02_indirizzo = azienda.addresses.Length > 0 ? String.Format("{0}", azienda.addresses[0].street.Trim()) : " ";
                    anag.m02_stato_validazione_cerved = 1;
                    anag.m02_cap = azienda.addresses.Length > 0 ? azienda.addresses[0].zipCode : "";
                    anag.m02_codicefiscale = azienda.fiscalCode;
                    anag.m02_comune = azienda.addresses.Length > 0 ? azienda.addresses[0].town : "";
                    anag.m02_indirizzo = azienda.addresses.Length > 0 ? String.Format("{0}, {1} {2}", azienda.addresses[0].street, azienda.addresses[0].street, azienda.addresses[0].street) : "";
                    anag.m02_nazione = azienda.addresses.Length > 0 ? String.Format("{0}, {1} {2}", azienda.addresses[0].street, azienda.addresses[0].country, azienda.addresses[0].country) : "";
                    anag.m02_dtupdate = DateTime.Now;
                    anag.m02_note = "";
                    anag.m02_telefono = azienda.addresses.Length > 0 ? azienda.addresses[0].phoneNumber : "";
                    anag.m02_prefisso = azienda.addresses.Length > 0 ? azienda.addresses[0].province : "";
                    anag.m02_fido = -1;
                    anag.m02_pec = "";
                    anag.m02_eventi_negativi = "Non rilevate";
                    anag.m02_updadm = true;
                    anag.m02_nrea = azienda.reaNumber.HasValue ? azienda.reaNumber.ToString() : "";
                    anag.m02_cciaa = azienda.reaProvince;
                    anag.m02_descrizioneAttivita = azienda.cAtecoDescription;
                    anag.m02_iscrizioneCCIAA = azienda.registrationDate.HasValue ? azienda.registrationDate.Value.ToShortDateString() : "";
                    context.m02_anagrafica.AddObject(anag);

                }
                context.SaveChanges();

                cerved_check anagraficaCUpdate =
                context.cerved_check.Where(an => an.partita_iva == partitaIva && an.id_utente == idLoggedUser
                )
                .OrderByDescending(d => d.dt_inserimento)
                .FirstOrDefault();
                if (anagraficaCUpdate != null)
                {

                    anagraficaCUpdate.codice_cerved = productCode.ToString() == "55220" ? "P" : "PR";
                    anagraficaCUpdate.evaso = false;
                    anagraficaCUpdate.dt_aggiornamento = DateTime.Now;
                }
                else
                {
                    anagraficaCUpdate = new cerved_check();
                    anagraficaCUpdate.dt_inserimento = DateTime.Now;
                    anagraficaCUpdate.dt_aggiornamento = DateTime.Now;
                    anagraficaCUpdate.partita_iva = partitaIva;
                    anagraficaCUpdate.id_utente = idLoggedUser;
                    anagraficaCUpdate.codice_cerved = productCode.ToString() == "55220" ? "P" : "PR";
                    context.cerved_check.AddObject(anagraficaCUpdate);
                    anagraficaCUpdate.evaso = false;
                }
                context.SaveChanges();
                m15_preferiti preferitoInserimento = context.m15_preferiti.Where(pref => pref.m15_partitaiva == azienda.vatNumber && pref.m15_iduser == idLoggedUser).FirstOrDefault();
                if (preferitoInserimento == null)
                {
                    preferitoInserimento = new m15_preferiti();
                    preferitoInserimento.m15_iduser = Convert.ToInt32(idLoggedUser);
                    preferitoInserimento.m15_partitaiva = partitaIva;
                    context.m15_preferiti.AddObject(preferitoInserimento);
                }
                context.SaveChanges();


                returnValue = true;
            }
            return returnValue;

        }

        public static ElementoGraficoMercato GetActual(int idCentro, string partitaIva)
        {
            ElementoGraficoMercato returnValue = new ElementoGraficoMercato();

            returnValue.DataMensilita = DateTime.Now;
            returnValue.Mensilita = String.Format("{0:yyyy}-{0:MM}", DateTime.Now);

            if (partitaIva != String.Empty)
            {
                returnValue = AggiungiRating(returnValue, partitaIva);
            }

            using (DemoR2Entities context = new DemoR2Entities())
            {


                return returnValue;
            }
        }
        public static List<ElementoAnagraficheRicerca> GetAnagraficaOsservatorioAC(string chiavePiva = "", string chiaveRagSoc = "")
        {
            List<ElementoAnagraficheRicerca> returnValue = new List<ElementoAnagraficheRicerca>();
            using (DemoR2Entities context = new DemoR2Entities())
            {
                if (chiavePiva != string.Empty)
                {
                    returnValue = context.m02_anagrafica.Where(a => a.m02_partitaiva.Contains(chiavePiva))
                                                        .Take(100)
                                                        .Select(a => new ElementoAnagraficheRicerca { PartitaIva = a.m02_partitaiva, RagioneSociale = a.m02_ragionesociale })
                                                        .OrderBy(a => a.PartitaIva).ToList();
                }

                if (chiaveRagSoc != string.Empty)
                {
                    returnValue = context.m02_anagrafica.Where(a => a.m02_ragionesociale.Contains(chiaveRagSoc))
                                                        .Take(100)
                                                        .Select(a => new ElementoAnagraficheRicerca { PartitaIva = a.m02_partitaiva, RagioneSociale = a.m02_ragionesociale })
                                                        .OrderBy(a => a.PartitaIva).ToList();
                }

            }
            return returnValue;

        }

        public static List<cerved_check> GetListaRichiesteCervedEsistentiSuPivaAttivi(string partitaIva)
        {
            List<cerved_check> returnValue = new List<cerved_check>();
            using (DemoR2Entities context = new DemoR2Entities())
            {
                returnValue =
                    context.cerved_check.Where(cc => cc.partita_iva == partitaIva && cc.evaso == true).Distinct().ToList();

            }
            return returnValue;

        }

        public static List<cerved_check> GetListaRichiesteCervedEsistentiSuPiva(string partitaIva)
        {
            List<cerved_check> returnValue = new List<cerved_check>();
            using (DemoR2Entities context = new DemoR2Entities())
            {
                returnValue =
                    context.cerved_check.Where(cc => cc.partita_iva == partitaIva).Distinct().ToList();

            }
            return returnValue;

        }


        public static bool VerificaSuperamentoSogliaReport(int idUtente, int idRuolo, string ambiente)
        {
            bool returnValue = false;
            if (idRuolo == 0) return returnValue;
            using (DemoR2Entities context = new DemoR2Entities())
            {
                ElementoOperatore aziendaUtente = GetAziendaUtente(idUtente, context, ambiente);
                int numeroMonitoraggi =
                    NumeroMonitoraggiReport(idUtente);
                returnValue = numeroMonitoraggi >= aziendaUtente.SogliaReport;
            }
            return returnValue;
        }

        public static int NumeroMonitoraggiReport(int idUtente)
        {
            int returnValue = 0;
            using (DemoR2Entities context = new DemoR2Entities())
            {
                returnValue =
                    context.cerved_check.Where(che => che.id_utente == idUtente).Count();

            }
            return returnValue;
        }

        public static int NumeroMonitoraggiReportDisponibili(int idUtente, string soglia)
        {
            int returnValue = 0;
            using (DemoR2Entities context = new DemoR2Entities())
            {
                ElementoOperatore aziendaUtente = GetAziendaUtente(idUtente, context, soglia);
                returnValue =
                    aziendaUtente.SogliaReport
                    -
                    context.cerved_check.Where(che => che.id_utente == idUtente).Count();
            }
            return returnValue;
        }
        public static bool VerificaEGestioneRichiestaAnaloga(string partitaIva, int idUtente)
        {
            bool returnValue = false;
            using (DemoR2Entities context = new DemoR2Entities())
            {

            }
            return returnValue;

        }

        public static ElementoValutazione GetDettaglioValutazione(string partitaIva)
        {
            ElementoValutazione returnValue = new ElementoValutazione();
            using (DemoR2Entities context = new DemoR2Entities())
            {
                returnValue = context.m05_rating.Where(rat => rat.m05_partitaiva == partitaIva).OrderByDescending(b => b.m05_dtriferimento).Take(1)
                .Join(context.m02_anagrafica,
                a => a.m05_partitaiva,
                b => b.m02_partitaiva,
                (a, b) => new ElementoValutazione
                {
                    id = a.m05_partitaiva,
                    PartitaIva = a.m05_partitaiva,
                    DataRiferimento = a.m05_dtriferimento,
                    ValutazioneGlobale = a.m05_stato,
                    ValutazioneGlobaleSemaforo = a.m05_stato_semaforo,
                    ValutazioneCerved = a.m05_stato_cerved,
                    ValutazioneCervedSemaforo = a.m05_stato_semaforo_cerved,
                    ValutazioneOsservaSemaforo = a.m05_stato_semaforo_osserva,
                    EventiNegativi = b.m02_eventi_negativi,
                    Fido = b.m02_fido.HasValue ? b.m02_fido.Value : -1
                }).FirstOrDefault();
            }
            return returnValue;
        }
        public static ElementoAnagraficheReport GetDettaglioAnagrafica(string partitaIva)
        {
            ElementoAnagraficheReport returnValue = new ElementoAnagraficheReport();
            using (DemoR2Entities context = new DemoR2Entities())
            {
                returnValue = context.m02_anagrafica.Where(an => an.m02_partitaiva == partitaIva)
                .Select(anag => new
                    ElementoAnagraficheReport
                {
                    id = anag.m02_partitaiva,
                    PartitaIva = anag.m02_partitaiva,
                    PEC = anag.m02_pec,
                    Comune = anag.m02_comune,
                    Provincia = anag.m02_prefisso,
                    RagioneSociale = anag.m02_ragionesociale,
                    NREA = anag.m02_nrea,
                    CCIA = anag.m02_cciaa,
                    DescrizioneAttivita = anag.m02_descrizioneAttivita,
                    EventiNegativi = "",
                    Fido = -1,
                    Indirizzo = anag.m02_indirizzo,
                    iscrizioneCCIA = anag.m02_iscrizioneCCIAA,
                    Cap = anag.m02_cap,
                    StatoAttivita = anag.m02_stato_attivita
                }).FirstOrDefault();

            }
            return returnValue;
        }
    }

}
