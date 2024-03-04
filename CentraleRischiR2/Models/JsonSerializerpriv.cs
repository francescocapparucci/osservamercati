using System;

namespace CentraleRischiR2.Classes
{

    public class JsonSeralizespriv
    {
        public string orderId { get; set; }
        public string companyId { get; set; }
        public DateTime dateOfOrder { get; set; }
        public string language { get; set; }
        public string userId { get; set; }
        public object chargeRef { get; set; }
        public Report report { get; set; }
    }

    public class Report
    {
        public string companyId { get; set; }
        public string language { get; set; }
        public Companysummary companySummary { get; set; }
        public Alternatesummary alternateSummary { get; set; }
        public Companyidentification companyIdentification { get; set; }
        public Creditscore creditScore { get; set; }
        public Contactinformation contactInformation { get; set; }
        public Sharecapitalstructure shareCapitalStructure { get; set; }
        public Directors directors { get; set; }
        public Otherinformation otherInformation { get; set; }
        public Additionalinformation additionalInformation { get; set; }
    }

    public class Companysummary
    {
        public string businessName { get; set; }
        public string country { get; set; }
        public string companyNumber { get; set; }
        public string companyRegistrationNumber { get; set; }
        public Mainactivity mainActivity { get; set; }
        public Companystatus companyStatus { get; set; }
        public Latestturnoverfigure latestTurnoverFigure { get; set; }
        public Creditrating creditRating { get; set; }
    }

    public class Mainactivity
    {
        public string code { get; set; }
        public string description { get; set; }
        public string classification { get; set; }
    }

    public class Companystatus
    {
        public string status { get; set; }
        public string description { get; set; }
    }

    public class Latestturnoverfigure
    {
        public float value { get; set; }
    }

    public class Creditrating
    {
        public string commonValue { get; set; }
        public string commonDescription { get; set; }
        public Creditlimit creditLimit { get; set; }
        public Providervalue providerValue { get; set; }
        public string providerDescription { get; set; }
    }

    public class Creditlimit
    {
        public string value { get; set; }
    }

    public class Providervalue
    {
        public string maxValue { get; set; }
        public string minValue { get; set; }
        public string value { get; set; }
    }

    public class Alternatesummary
    {
        public string province { get; set; }
        public string vatRegistrationNumber { get; set; }
        public string address { get; set; }
        public string businessName { get; set; }
        public string legalForm { get; set; }
        public string taxCode { get; set; }
        public string numberOfEmployees { get; set; }
        public string telephone { get; set; }
        public string publicRegisterSection { get; set; }
        public string shareCapital { get; set; }
        public DateTime incorporationDate { get; set; }
        public DateTime latestUpdateOnIc { get; set; }
        public DateTime reaInscriptionDate { get; set; }
        public string hqType { get; set; }
    }

    public class Companyidentification
    {
        public Basicinformation basicInformation { get; set; }
        public Activityclassification[] activityClassifications { get; set; }
    }

    public class Basicinformation
    {
        public string businessName { get; set; }
        public string registeredCompanyName { get; set; }
        public string companyRegistrationNumber { get; set; }
        public string country { get; set; }
        public string vatRegistrationNumber { get; set; }
        public DateTime companyRegistrationDate { get; set; }
        public DateTime operationsStartDate { get; set; }
        public Legalform legalForm { get; set; }
        public Companystatus1 companyStatus { get; set; }
        public Principalactivity principalActivity { get; set; }
        public Contactaddress contactAddress { get; set; }
    }

    public class Legalform
    {
        public string providerCode { get; set; }
        public string description { get; set; }
    }

    public class Companystatus1
    {
        public string status { get; set; }
        public string description { get; set; }
    }

    public class Principalactivity
    {
        public string code { get; set; }
        public string description { get; set; }
    }

    public class Contactaddress
    {
        public string simpleValue { get; set; }
        public string street { get; set; }
        public string houseNumber { get; set; }
        public string city { get; set; }
        public string postalCode { get; set; }
        public string province { get; set; }
        public string telephone { get; set; }
        public string country { get; set; }
    }

    public class Activityclassification
    {
        public string classification { get; set; }
        public Activity[] activities { get; set; }
    }

    public class Activity
    {
        public string code { get; set; }
        public string description { get; set; }
    }

    public class Creditscore
    {
        public Currentcreditrating currentCreditRating { get; set; }
        public Previouscreditrating previousCreditRating { get; set; }
        public DateTime latestRatingChangeDate { get; set; }
    }

    public class Currentcreditrating
    {
        public string commonValue { get; set; }
        public string commonDescription { get; set; }
        public Creditlimit1 creditLimit { get; set; }
        public Providervalue1 providerValue { get; set; }
        public string providerDescription { get; set; }
    }

    public class Creditlimit1
    {
        public string value { get; set; }
    }

    public class Providervalue1
    {
        public string maxValue { get; set; }
        public string minValue { get; set; }
        public string value { get; set; }
    }

    public class Previouscreditrating
    {
        public string commonValue { get; set; }
        public string commonDescription { get; set; }
        public Creditlimit2 creditLimit { get; set; }
        public Providervalue2 providerValue { get; set; }
        public string providerDescription { get; set; }
    }

    public class Creditlimit2
    {
        public string value { get; set; }
    }

    public class Providervalue2
    {
        public string maxValue { get; set; }
        public string minValue { get; set; }
        public string value { get; set; }
    }

    public class Contactinformation
    {
        public Mainaddress mainAddress { get; set; }
        public string[] emailAddresses { get; set; }
    }

    public class Mainaddress
    {
        public string simpleValue { get; set; }
        public string street { get; set; }
        public string houseNumber { get; set; }
        public string city { get; set; }
        public string province { get; set; }
        public string telephone { get; set; }
        public string country { get; set; }
    }

    public class Sharecapitalstructure
    {
        public Issuedsharecapital issuedShareCapital { get; set; }
        public string numberOfSharesIssued { get; set; }
        public Shareholder[] shareHolders { get; set; }
    }

    public class Issuedsharecapital
    {
        public string currency { get; set; }
        public string value { get; set; }
    }

    public class Shareholder
    {
        public string id { get; set; }
        public string name { get; set; }
        public string percentSharesHeld { get; set; }
    }

    public class Directors
    {
        public Currentdirector[] currentDirectors { get; set; }
    }

    public class Currentdirector
    {
        public string id { get; set; }
        public string name { get; set; }
        public Address address { get; set; }
        public string firstNames { get; set; }
        public string surname { get; set; }
        public string gender { get; set; }
        public DateTime dateOfBirth { get; set; }
        public string directorType { get; set; }
        public Position[] positions { get; set; }
        public Additionaldata additionalData { get; set; }
    }

    public class Address
    {
        public string simpleValue { get; set; }
    }

    public class Additionaldata
    {
        public string birthProvince { get; set; }
        public bool hasPrejudicials { get; set; }
        public bool hasProtesti { get; set; }
    }

    public class Position
    {
        public DateTime dateAppointed { get; set; }
        public string positionName { get; set; }
        public string apptDurationType { get; set; }
        public string authority { get; set; }
    }

    public class Otherinformation
    {
        public Employeesinformation[] employeesInformation { get; set; }
    }

    public class Employeesinformation
    {
        public string numberOfEmployees { get; set; }
    }

    public class Additionalinformation
    {
        public Misc misc { get; set; }
        public Ratinghistory[] ratingHistory { get; set; }
        public Creditlimithistory[] creditLimitHistory { get; set; }
        public Shareholder1[] shareholders { get; set; }
        public Shareholderssummary shareholdersSummary { get; set; }
        public Shareholdingcompany[] shareholdingCompanies { get; set; }
        public Commentary[] commentaries { get; set; }
        public Transfer[] transfers { get; set; }
        public Localbranch[] localBranches { get; set; }
        public Workforceinformation workforceInformation { get; set; }
        public Auditors auditors { get; set; }
        public Capitalfigures capitalFigures { get; set; }
        public Activities activities { get; set; }
    }

    public class Misc
    {
        public string taxCode { get; set; }
        public string hqTypeCode { get; set; }
        public string hqType { get; set; }
        public string chamberOfCommerce { get; set; }
        public string province { get; set; }
        public string registerStatus { get; set; }
        public string publicRegisterSection { get; set; }
        public string businessStatus { get; set; }
        public DateTime incorporationDate { get; set; }
        public DateTime closureDate { get; set; }
        public string socialDescription { get; set; }
        public string powerCategory { get; set; }
        public string negativeRating { get; set; }
        public string numberOfCompaniesInActivityCode { get; set; }
        public string numberOfCancelledCompaniesInActivityCode { get; set; }
        public string latestYearEndOfAccounts { get; set; }
    }

    public class Shareholderssummary
    {
        public DateTime proceedingsDate { get; set; }
        public string registrationProvince { get; set; }
        public string currencyCode { get; set; }
        public string sharesNumber { get; set; }
    }

    public class Workforceinformation
    {
        public string accountsController { get; set; }
        public Companyadministrationform[] companyAdministrationForms { get; set; }
        public Companyemployee[] companyEmployees { get; set; }
    }

    public class Companyadministrationform
    {
        public string administrationType { get; set; }
        public string minimumNumberofDirectors { get; set; }
        public string maximumNumberofDirectors { get; set; }
    }

    public class Companyemployee
    {
        public DateTime latestEmployeeInformationDate { get; set; }
        public string employeeInformationYear { get; set; }
        public string q1NumberSEworkers { get; set; }
        public string q1NumberEmployedWorkers { get; set; }
        public string q1TotalNumberofWorkers { get; set; }
    }

    public class Auditors
    {
        public DateTime auditorsBoardValidityStartDate { get; set; }
        public string numberofPeriodsBoardRemainsinOffice { get; set; }
        public string auditorsBoardTerm { get; set; }
        public string numberofStatutoryAuditors { get; set; }
        public string numberofAssistantAuditors { get; set; }
        public string numberofMembersInTheBoardinOffice { get; set; }
    }

    public class Capitalfigures
    {
        public string capitalCurrency { get; set; }
        public string deliberatedShareCapital { get; set; }
        public string subscribedShareCapital { get; set; }
        public string paidInShareCapital { get; set; }
        public string numberofSharesintheShareCapital { get; set; }
        public float shareUnitValue { get; set; }
        public string shareUnitCurrency { get; set; }
    }

    public class Activities
    {
        public Activitylist[] activityList { get; set; }
    }

    public class Activitylist
    {
        public string codeType { get; set; }
        public string code { get; set; }
        public string type { get; set; }
        public string description { get; set; }
    }

    public class Ratinghistory
    {
        public DateTime date { get; set; }
        public string localRating { get; set; }
        public string commonValue { get; set; }
        public string commonDescription { get; set; }
    }

    public class Creditlimithistory
    {
        public DateTime date { get; set; }
        public string value { get; set; }
    }

    public class Shareholder1
    {
        public string sharesTaxCode { get; set; }
        public string sharesStocksFaceValue { get; set; }
        public string sharesCurrencyCode { get; set; }
        public string sharesStockNumber { get; set; }
    }

    public class Shareholdingcompany
    {
        public string taxCode { get; set; }
        public bool hasProtesti { get; set; }
        public bool hasPrejudicials { get; set; }
    }

    public class Commentary
    {
        public string commentaryText { get; set; }
        public string positiveOrNegative { get; set; }
    }

    public class Transfer
    {
        public DateTime proceedingsDate { get; set; }
        public string chamberofCommerceRegistered { get; set; }
        public string transferNumber { get; set; }
        public DateTime filedDate { get; set; }
        public DateTime protocolDate { get; set; }
        public string transferSubject { get; set; }
        public Companiesinvolved[] companiesInvolved { get; set; }
    }

    public class Companiesinvolved
    {
        public string transfereeOrTransferor { get; set; }
        public string taxCode { get; set; }
        public string name { get; set; }
        public string documentName { get; set; }
    }

    public class Localbranch
    {
        public string cciaAandNREA { get; set; }
        public DateTime openingDate { get; set; }
        public string branchNumber { get; set; }
    }


}