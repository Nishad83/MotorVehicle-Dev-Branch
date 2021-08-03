
namespace AndWebApi.Models
{
    #region namespace
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    #endregion
    public class CoverageDetails
    {
        /// <summary>
        /// get or set is electrical accessories.  
        /// </summary>
        public bool IsElectricalAccessories { get; set; }

        /// <summary>
        ///  get or set is non-electrical accessories.
        /// </summary>
        public bool IsNonElectricalAccessories { get; set; }

        /// <summary>
        /// get or set number of SIElectricalAccessories
        /// </summary>
        public int SIElectricalAccessories { get; set; }

        /// <summary>
        /// get or set number of SINonElectricalAccessories
        /// </summary>
        public int SINonElectricalAccessories { get; set; }
        /// <summary>
        ///  get or set is bi-fuel kit.
        /// </summary>
        public bool IsBiFuelKit { get; set; }

        /// <summary>
        ///  get or set bi-fuel kit value.
        /// </summary>
        public int BiFuelKitAmount { get; set; }

        /// <summary>
        /// get or set fiber glass.
        /// </summary>
        public bool IsFiberGlassFuelTank { get; set; }

        /// <summary>
        /// get or set ll to paid driver.
        /// </summary>
        public bool IsLegalLiablityPaidDriver { get; set; }

        /// <summary>
        /// get or set number of legal liablity paid driver.
        /// </summary>
        public int NoOfLLPaidDriver { get; set; }

        /// <summary>
        /// get or set employee liability.
        /// </summary>
        public bool IsEmployeeLiability { get; set; }

        /// <summary>
        /// get or set pa cover paid driver.
        /// </summary>
        public bool IsPACoverPaidDriver { get; set; }

        /// <summary>
        /// get or set pa cover paid driver amount.
        /// </summary>
        public int PACoverPaidDriverAmount { get; set; }

        /// <summary>
        /// get or set pa cover unnamed person.
        /// </summary>
        public bool IsPACoverUnnamedPerson { get; set; }

        /// <summary>
        /// get or set pa cover unnamed person amount.
        /// </summary>
        public int PACoverUnnamedPersonAmount { get; set; }
        /// <summary>
        /// get or set number of unnamed Person
        /// </summary>
        public int NumberofPersonsUnnamed { get; set; }
        /// <summary>
        /// get or set pa cover named person.
        /// </summary>
        public bool IsPACoverForNamedPersons { get; set; }

        /// <summary>
        /// get or set pa cover unnamed person amount.
        /// </summary>
        public int CapitalSumInsuredPerPersonNamed { get; set; }
        /// <summary>
        /// get or set number of l named person.
        /// </summary>
        public int NumberofPersonsNamed { get; set; }


        /// <summary>
        /// get or set pa cover OwnDriver.
        /// </summary>
        public bool IsPACoverForOwnerDriver { get; set; }
        /// <summary>
        /// get or set list of electrical accessories details.
        /// </summary>
        public List<ElectricalAccessoriesDetails> ElectricalAccessoriesDetails { get; set; }

        /// <summary>
        /// get or set list of non-electrical accessories details.
        /// </summary>
        public List<NonElectricalAccessoriesDetails> NonElectricalAccessoriesDetails { get; set; }
        public RequestType RequestType { get; set; }

        /// <summary>
        /// get or set is LL Employee.
        /// </summary>
        public bool IsLLEmployee { get; set; }

        /// <summary>
        /// get or set LL Employee No.
        /// </summary>
        public int LLEmployeeNo { get; set; }
    }

    public class ElectricalAccessoriesDetails
    {
        /// <summary>
        /// get or set amount.
        /// </summary>
        public int Amount { get; set; }
        
        /// <summary>
        /// get or set name.
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// get or set year.
        /// </summary>
        public int Year { get; set; } 
    }

    public class NonElectricalAccessoriesDetails
    {
        /// <summary>
        /// get or set amount.
        /// </summary>
        public int Amount { get; set; }

        /// <summary>
        /// get or set name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// get or set year.
        /// </summary>
        public int Year { get; set; }
    }
}