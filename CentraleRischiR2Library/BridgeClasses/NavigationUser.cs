using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace CentraleRischiR2Library.BridgeClasses
{
    [Serializable]
    public class NavigationUser 
    {
        public string Username {get;set; }
        public string Name { get; set; }
        public string NetUser { get; set; }
        public string NetPwd { get; set; }
        public string Password { get; set; }
        public int IdUser { get; set; }
        public int IdAzienda { get; set; }        
        public bool Demo{ get; set; }
        public string Azienda { get; set; }
        public string CodiceAzienda { get; set; }
        public string Mercato { get; set; }
        public string CittaMercato { get; set; }
        public int IdMercato { get; set; }
        public int IdRuolo { get; set; }
        public string Indirizzo { get; set; }
        public string Localita { get; set; }
        public string Email { get; set; }
        public bool ApprovatoPrivacy { get; set; }
        public bool Abilitato { get; set; }
        public bool AbilitatoNotturno { get; set; }
        public bool AbilitatoRicerca { get; set; }
        public int ReportCount { get; set; }
        public string Ambiente { get; set; }
        public DateTime? DataApprovazionePrivacy { get; set; }
        public string CodiceFinservice { get; set; }
        public string CodicePayLine { get; set; }    
        public string token { get; set; }
    }
}
