namespace AndApp.Models
{
    #region namespace
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Web;
    using static AndWebApi.App_Start.CustomValidators;
    #endregion

    /// <summary>
    /// This class contains properties for getting quotation of insurance company.    
    /// </summary>
    public class Quotation
    {
        public long pospid { get; set; }
        /// <summary>
        /// get or set policy type like new,rollover etc.
        /// </summary>
        [Required(ErrorMessageResourceType = typeof(Validation), ErrorMessageResourceName = "PolicyType")]
        public string PolicyType { get; set; }

        /// <summary>
        /// get or set policy start date.
        /// </summary>
        // [RequiredIf("RequestType", RequestType.Fullquote, ErrorMessageResourceType = typeof(Validation), ErrorMessageResourceName = "PolicyStartDate")]
        //[RequiredIf(PolicyStartDate,]
        public string PolicyStartDate { get; set; }

        /// <summary>
        /// get or set policy end date.
        /// </summary>
        [Required(ErrorMessageResourceType = typeof(Validation), ErrorMessageResourceName = "PolicyEndDate")]
        public string PolicyEndDate { get; set; }

        /// <summary>
        /// get or set plan name.
        /// </summary>
        [RequiredIf("RequestType", RequestType.Fullquote, ErrorMessageResourceName = "PlanName", ErrorMessageResourceType = typeof(Validation))]
        public string PlanName { get; set; }


        /// <summary>
        /// get or set plan id.
        /// </summary>
       // [RequiredIf("RequestType", RequestType.Fullquote, ErrorMessageResourceType = typeof(Validation), ErrorMessageResourceName = "PlanId")]
        public string PlanId { get; set; }

        /// <summary>
        /// get or set company name.
        /// </summary>
        [RequiredIf("RequestType", RequestType.Fullquote, ErrorMessageResourceType = typeof(Validation), ErrorMessageResourceName = "CompanyName")]
        public Company CompanyName { get; set; }

        /// <summary>
        /// get or set customer type like individual or organization.
        /// </summary>
        [Required(ErrorMessageResourceType = typeof(Validation), ErrorMessageResourceName = "CustomerType")]
        public string CustomerType { get; set; }

        /// <summary>
        /// get or set organization name.
        /// </summary>
        [RequiredIf("CustomerType", "Organization", ErrorMessageResourceType = typeof(Validation), ErrorMessageResourceName = "OrganizationName")]
        public string OrganizationName { get; set; }

        /// <summary>
        /// get or set nominee name.
        /// </summary>
        [RequiredIf("RequestType", RequestType.Fullquote, ErrorMessageResourceType = typeof(Validation), ErrorMessageResourceName = "NomineeName")]
        public string NomineeName { get; set; }

        /// <summary>
        /// get or set nominee date of birth.
        /// </summary>
        [RequiredIf("RequestType", RequestType.Fullquote, ErrorMessageResourceType = typeof(Validation), ErrorMessageResourceName = "NomineeDateOfBirth")]
        public string NomineeDateOfBirth { get; set; }

        /// <summary>
        /// get or set nominee relation ship.
        /// </summary>
        [RequiredIf("RequestType", RequestType.Fullquote, ErrorMessageResourceType = typeof(Validation), ErrorMessageResourceName = "NomineeRelationShip")]
        public string NomineeRelationShip { get; set; }

        /// <summary>
        /// get or set nominee gender.
        /// </summary>
        [RequiredIf("RequestType", RequestType.Fullquote, ErrorMessageResourceType = typeof(Validation), ErrorMessageResourceName = "NomineeGender")]
        public string NomineeGender { get; set; }

        /// <summary>
        /// get or set appointee name.
        /// </summary>
        public string AppointeeName { get; set; }

        /// <summary>
        /// get or set appointee relation ship.
        /// </summary>
        public string AppointeeRelationShip { get; set; }

        /// <summary>
        /// get or set is owner changed.
        /// </summary>
        public bool IsOwnerChanged { get; set; }

        /// <summary>
        /// get or set don't know previous insurer. 
        /// </summary>
        public bool DontKnowPreviousInsurer { get; set; }

        /// <summary>
        /// get or set is third party only.
        /// </summary>
        public bool IsThirdPartyOnly { get; set; }

        /// <summary>
        /// get or set is od only policy.
        /// </summary>
        public bool IsODOnly { get; set; }

        /// <summary>
        /// get or set is valid licence.
        /// </summary>
        public bool IsValidLicence { get; set; }

        /// <summary>
        /// get or set idv amount.
        /// </summary>
        public int IDV { get; set; }

        public string IDV_MinOrMax { get; set; }
        /// <summary>
        /// get or set request type. 
        /// </summary>
        public RequestType RequestType { get; set; }

        /// <summary>
        /// get or set vehicle details. 
        /// </summary>
        public VehicleDetails VehicleDetails { get; set; }

        /// <summary>
        /// get or set vehicle address details.
        /// </summary>
        public VehicleAddressDetails VehicleAddressDetails { get; set; }

        /// <summary>
        /// get or set previous policy details.
        /// </summary>
        public PreviousPolicyDetails PreviousPolicyDetails { get; set; }

        /// <summary>
        /// get or set previous policy PreviousTPPolicyDetails.
        /// </summary>
        public PreviousTPPolicyDetails PreviousTPPolicyDetails { get; set; }
     

        /// <summary>
        /// get or set customer address details.
        /// </summary>
        public CustomerAddressDetails CustomerAddressDetails { get; set; }

        /// <summary>
        /// get or set client details.
        /// </summary>
        public ClientDetails ClientDetails { get; set; }

        /// <summary>
        /// get or set discount details.
        /// </summary>
        public DiscountDetails DiscountDetails { get; set; }

        /// <summary>
        /// get or set coverage details .
        /// </summary>
        public CoverageDetails CoverageDetails { get; set; }

        /// <summary>
        /// get or set current ncb.
        /// </summary>
        public int CurrentNcb { get; set; }

        /// <summary>
        /// get or set add on cover details.
        /// </summary>
        public AddonCover AddonCover { get; set; }

        public List<CustomIDV> CustomIDV { get; set; }
        public CompanyWiseRefference CompanyWiseRefference { get;set;}

        public PremiumDetails PremiumDetails { get; set; }

        public int Tennure { get; set; }
        /// <summary>
        /// get or set appointee DateOfBirth.
        /// </summary>
        public string AppointeeDateOfBirth { get; set; }

        public string enquiryid { get; set; }
    }
}