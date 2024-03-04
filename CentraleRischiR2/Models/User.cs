using CentraleRischiR2Library;
using CentraleRischiR2Library.BridgeClasses;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Configuration;

namespace CentraleRischiR2.Models
{
    public class User
    {        
        public bool AbilitatoRicerca
        { get; set; }
        public bool AbilitatoNotturno
        { get; set; }
        public string Azienda
        { get; set; }

        [Required]
        [Display(Name = "Utente")]
        public string Name
        { get;set; }

        public int IdAzienda
        { get; set; }

        public int IdUser
        { get; set; }

        public bool Demo
        { get; set; }

        public int IdRole
        { get; set; }

        public string NetUser
        { get; set; }

        public string NetPwd
        { get; set; }


        public bool Abilitato
        { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password
        { get;set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email")]
        public string Email
        { get; set; }

        [Display(Name = "Ricordiami su questo computer")]
        public bool RememberMe { get; set; }

        /// <summary>
        /// Checks if user with given password exists in the database
        /// </summary>
        /// <param name="_username">User name</param>
        /// <param name="_password">User password</param>
        /// <returns>True if user exist and password is correct</returns>
        public bool IsValid(string _name, string _password)
        {
                bool returnValue = false;
            NavigationUser loggedUser = DBHandler.LogUser(_name, _password, WebConfigurationManager.AppSettings["AMBIENTE"]);
            if(loggedUser != null)
            {
                HttpContext.Current.Session["LoggedUser"] = loggedUser;
                returnValue = true;
            }
            return returnValue;
        }
    }
}