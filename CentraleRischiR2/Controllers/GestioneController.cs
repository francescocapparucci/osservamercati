using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CentraleRischiR2Library;
using CentraleRischiR2Library.BridgeClasses;
using CentraleRischiR2.Classes;
using System.Configuration;
using System.Web.Configuration;
using System.Threading;

namespace CentraleRischiR2.Controllers
{
    public class GestioneController : BaseController
    {
        [Authorize]
        public override JsonResult ElencoOperatoriCentro(int idCentro)
        {
            string idAzienda = "";
            if (loggeduser.IdRuolo == 0)
            {
                idAzienda = "";
            }
            else
            {
                idAzienda = loggeduser.CodiceAzienda;
            }
            List<ElementoAggiornamento> returnValue = DBHandler.ElencoAggiornamentoOperatoriCentro(idCentro, idAzienda);

            return Json(returnValue, JsonRequestBehavior.AllowGet);
        }


        [Authorize]
        public JsonResult ImportazioneMesiAzienda(string codiceAzienda, int numeroMesi)
        {
            bool returnValue = true;

            ThreadStart parallelGrouping = new ThreadStart(() => { CentraleRischiR2.Classes.Utils.Reimport(codiceAzienda, numeroMesi); });
            Thread threadGrouping = new Thread(parallelGrouping);
            threadGrouping.Start();

            return Json(returnValue, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public ActionResult SalvaUtente(Models.User user)
        {

            if (user.IdRole == 2)
            {
                DBHandler.UpdateUtente(user);
                
                return RedirectToAction("GestioneUser", "Gestione");
            }

            if (ModelState.IsValid)
            {
                DBHandler.SalvaUtente(user);
            }
            return RedirectToAction("Gestione", "Gestione");
        }

        [Authorize]
        public ActionResult Gestione()
        {

            if (loggeduser == null)
            {
                RedirectToAction("Index", "Home");
            }
            ViewBag.ProvinceItaliane = ProvinceItaliane;
            ViewBag.Username = loggeduser.Username;
            ViewBag.CodiceAzienda = loggeduser.CodiceAzienda;
            ViewBag.Azienda = loggeduser.Azienda;
            ViewBag.Mercato = loggeduser.Mercato;
            ViewBag.Indirizzo = loggeduser.Indirizzo;
            ViewBag.Localita = loggeduser.Localita;
            ViewBag.Demo = loggeduser.Demo;
            ViewBag.ApprovatoPrivacy = loggeduser.ApprovatoPrivacy;
            ViewBag.DataApprovazionePrivacy = loggeduser.DataApprovazionePrivacy;
            ViewBag.Ambiente = loggeduser.Ambiente;
            ViewBag.IdRuolo = loggeduser.IdRuolo;
            ViewBag.NumeroMonitoraggiReportDisponibili = NumeroMonitoraggiReportDisponibili;
            ViewBag.VerificaSuperamentoSogliaReport = VerificaSuperamentoSogliaReport;
            Models.SearchUserModel suModel = new Models.SearchUserModel();
            if (loggeduser.IdRuolo == 2)
            {
                suModel.ElencoCentri = DBHandler.ElencoCentri(loggeduser.IdMercato);
            }
            else
            {
                suModel.ElencoCentri = DBHandler.ElencoCentri();
            }

            ViewBag.RicercaAbilitata = loggeduser.AbilitatoRicerca;
            ViewBag.IdUser = loggeduser.IdUser;
            if (Request.Browser.IsMobileDevice)
            {
                List<ElementoPreferiti> preferiti = DBHandler.GetPreferitiHome(loggeduser.IdUser, loggeduser.IdRuolo, loggeduser.IdMercato, loggeduser.Ambiente);
                preferiti = preferiti.OrderByDescending(ord => ord.DataVariazione).Take(10).ToList(); ;
                ViewBag.Preferiti = preferiti;
            }

            CentraleRischiR2.Models.Search search = new CentraleRischiR2.Models.Search();

            search.RagioneSociale = Request.Form["RagioneSociale"];
            if (!String.IsNullOrEmpty(Request.QueryString["piva"]))
                search.PartitaIva = Request.QueryString["piva"];
            else
                search.PartitaIva = Request.Form["PartitaIva"];
            search.Provincia = Request.Form["Provincia"];

            if (loggeduser.IdRuolo == 2)
            {
                suModel.ElencoUtenti = DBHandler.GetUtenti().Where(_ => _.IdUser == loggeduser.IdUser).Select(a => new Models.User() { Azienda = a.Azienda, Abilitato = a.Abilitato, AbilitatoRicerca = a.AbilitatoRicerca, AbilitatoNotturno = a.AbilitatoNotturno, Name = a.Name, Password = a.Password, IdUser = a.IdUser, IdAzienda = a.IdAzienda, Demo = a.Demo, IdRole = a.IdRuolo, Email = a.Email, NetUser = a.NetUser, NetPwd = a.NetPwd }).ToList();
                suModel.ElencoAziende = DBHandler.ElencoOperatori().Where(__ => __.m01_idazienda == loggeduser.IdAzienda).Select(a => new Models.Azienda { CodiceAzienda = a.m01_codice, RagioneSociale = a.m01_ragionesociale, IdAzienda = a.m01_idazienda }).ToList();
            }
            else
            {
                suModel.ElencoUtenti = DBHandler.GetUtenti().Select(a => new Models.User() { Azienda = a.Azienda, Abilitato = a.Abilitato, AbilitatoRicerca = a.AbilitatoRicerca, AbilitatoNotturno = a.AbilitatoNotturno, Name = a.Name, Password = a.Password, IdUser = a.IdUser, IdAzienda = a.IdAzienda, Demo = a.Demo, IdRole = a.IdRuolo, Email = a.Email, NetUser = a.NetUser, NetPwd = a.NetPwd }).ToList();
                suModel.ElencoAziende = DBHandler.ElencoOperatori().Select(a => new Models.Azienda { CodiceAzienda = a.m01_codice, RagioneSociale = a.m01_ragionesociale, IdAzienda = a.m01_idazienda }).ToList();
            }
            suModel.Search = search;
            return View(suModel);

        }
        [Authorize]
        public ActionResult GestioneUser()
        {

            if (loggeduser == null)
            {
                RedirectToAction("Index", "Home");
            }
            ViewBag.ProvinceItaliane = ProvinceItaliane;
            ViewBag.Username = loggeduser.Username;
            ViewBag.CodiceAzienda = loggeduser.CodiceAzienda;
            ViewBag.Azienda = loggeduser.Azienda;
            ViewBag.Mercato = loggeduser.Mercato;
            ViewBag.Indirizzo = loggeduser.Indirizzo;
            ViewBag.Localita = loggeduser.Localita;
            ViewBag.Demo = loggeduser.Demo;
            ViewBag.ApprovatoPrivacy = loggeduser.ApprovatoPrivacy;
            ViewBag.DataApprovazionePrivacy = loggeduser.DataApprovazionePrivacy;
            ViewBag.Ambiente = loggeduser.Ambiente;
            ViewBag.IdRuolo = loggeduser.IdRuolo;
            ViewBag.NumeroMonitoraggiReportDisponibili = NumeroMonitoraggiReportDisponibili;
            ViewBag.VerificaSuperamentoSogliaReport = VerificaSuperamentoSogliaReport;
            Models.SearchUserModel suModel = new Models.SearchUserModel();
            suModel.ElencoCentri = DBHandler.ElencoCentri();
            ViewBag.RicercaAbilitata = loggeduser.AbilitatoRicerca;
            ViewBag.IdUser = loggeduser.IdUser;
            if (Request.Browser.IsMobileDevice)
            {
                List<ElementoPreferiti> preferiti = DBHandler.GetPreferitiHome(loggeduser.IdUser, loggeduser.IdRuolo, loggeduser.IdMercato, loggeduser.Ambiente);
                preferiti = preferiti.OrderByDescending(ord => ord.DataVariazione).Take(10).ToList(); ;
                ViewBag.Preferiti = preferiti;
            }

            CentraleRischiR2.Models.Search search = new CentraleRischiR2.Models.Search();

            search.RagioneSociale = Request.Form["RagioneSociale"];
            if (!String.IsNullOrEmpty(Request.QueryString["piva"]))
                search.PartitaIva = Request.QueryString["piva"];
            else
                search.PartitaIva = Request.Form["PartitaIva"];
            search.Provincia = Request.Form["Provincia"];

            suModel.ElencoUtenti = DBHandler.GetUtenti().Where(__ => __.IdAzienda == loggeduser.IdAzienda).Select(a => new Models.User() { Azienda = a.Azienda, Abilitato = a.Abilitato, AbilitatoRicerca = a.AbilitatoRicerca, AbilitatoNotturno = a.AbilitatoNotturno, Name = a.Name, Password = a.Password, IdUser = a.IdUser, IdAzienda = a.IdAzienda, Demo = a.Demo, IdRole = a.IdRuolo, Email = a.Email, NetUser = a.NetUser, NetPwd = a.NetPwd }).ToList();
            suModel.Search = search;

            suModel.ElencoAziende = DBHandler.ElencoOperatori().Where(__ => __.m01_idazienda == loggeduser.IdAzienda).Select(a => new Models.Azienda { CodiceAzienda = a.m01_codice, RagioneSociale = a.m01_ragionesociale, IdAzienda = a.m01_idazienda }).ToList();

            return View(suModel);

        }

    }
}
