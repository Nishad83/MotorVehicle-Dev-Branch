namespace AndWebApi.Models
{
    #region namespace
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using static App_Start.CustomValidators;
    #endregion
    public class VehicleDetails 
    {
        /// <summary>
        /// get or ser variant id.
        /// </summary>
        public int VariantId { get; set; }

        /// <summary>
        /// get or set rto id.
        /// </summary>
        public int RtoId { get; set; }

        /// <summary>
        /// get or set rto zone.
        /// </summary>
        public string RtoZone { get; set; }

        /// <summary>
        /// get or set purchase date.
        /// </summary>
        public string PurchaseDate { get; set; }

        /// <summary>
        /// get or set manufaturing date.
        /// </summary>
        public string ManufaturingDate { get; set; }

        /// <summary>
        /// get or set registration date.
        /// </summary>
        public string RegistrationDate { get; set; }

        /// <summary>
        /// get or set registration no.
        /// </summary>
        public string RegistrationNumber { get; set; }

        /// <summary>
        /// get or set engine no.
        /// </summary>
        public string EngineNumber { get; set; }

        /// <summary>
        /// get or set chassis no.
        /// </summary>
        public string ChassisNumber { get; set; }

        /// <summary>
        /// get or set make name.
        /// </summary>
        public string MakeName { get; set; }

        /// <summary>
        /// get or set model name.
        /// </summary>
        public string ModelName { get; set; }

        /// <summary>
        /// get or set variant name.
        /// </summary>
        public string VariantName { get; set; }

        /// <summary>
        /// get or set make code.
        /// </summary>
        public string MakeCode { get; set; }

        /// <summary>
        /// get or  set model code.
        /// </summary>
        public string ModelCode { get; set; }

        /// <summary>
        /// get or set variant code.
        /// </summary>
        public string VariantCode { get; set; }

        /// <summary>
        /// get or set bi fuel type like lpg or cng.
        /// </summary>
        public string BiFuelType { get; set; }

        /// <summary>
        /// get or set is vehicle loan.
        /// </summary>
        public bool IsVehicleLoan { get; set; }

        /// <summary>
        /// get or set loan company name.     
        /// </summary>
        public string LoanCompanyName { get; set; }

        /// <summary>
        /// get or set loan amount.
        /// </summary>
        public string LoanAmount { get; set; }

        /// <summary>
        /// get or set is valid puc.
        /// </summary>
        public bool IsValidPUC { get; set; }

        /// <summary>
        /// get or set puc number.
        /// </summary>
        public string PUCNumber { get; set; }

        /// <summary>
        /// get or set puc start date.
        /// </summary>
        public string PUCStartDate { get; set; }

        /// <summary>
        /// get or set puc end date.
        /// </summary>
        public string PUCEndDate { get; set; }

        /// <summary>
        /// get or set cubic capacity.
        /// </summary>
        public int CC { get; set; }

        /// <summary>
        /// get or set seating capacity.  
        /// </summary>
        public int SC { get; set; }

        /// <summary>
        /// get or set ex-showroom price.
        /// </summary>
        public int ExShowroomPrice { get; set; }

        /// <summary>
        /// get or set request type. 
        /// </summary>
        public RequestType RequestType { get; set; }

        public string Segment { get; set; }

        public string Fuel { get; set; }

        public string BodyType { get; set; }

        public string VehicleColor { get; set; }
        /// <summary>
        /// get or set loan city.
        /// </summary>
        public string LoanCity { get; set; }


    }
}