namespace AndApp.Models
{
    #region namespace
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Web;
    using static App_Start.CustomValidators;
    #endregion

    public class ClientDetails 
    {
        /// <summary>
        /// get or set first name.
        /// </summary>
        [RequiredIf("RequestType", RequestType.Fullquote, ErrorMessageResourceType = typeof(Validation), ErrorMessageResourceName = "FirstName")]
        public string FirstName { get; set; }

        /// <summary>
        /// get or set middle name.
        /// </summary>
        public string MiddleName { get; set; }

        /// <summary>
        /// get or set last name.
        /// </summary>
        [RequiredIf("RequestType", RequestType.Fullquote, ErrorMessageResourceType = typeof(Validation), ErrorMessageResourceName = "LastName")]
        public string LastName { get; set; }

        /// <summary>
        /// get or set date of birth.
        /// </summary>
        [RequiredIf("RequestType", RequestType.Fullquote, ErrorMessageResourceType = typeof(Validation), ErrorMessageResourceName = "DateOfBirth")]
        public string DateOfBirth { get; set; }

        /// <summary>
        /// get or set gender. 
        /// </summary>
        [RequiredIf("RequestType", RequestType.Fullquote, ErrorMessageResourceType = typeof(Validation), ErrorMessageResourceName = "Gender")]
        public string Gender { get; set; }

        /// <summary>
        /// get or set email id.
        /// </summary>
        [RequiredIf("RequestType", RequestType.Fullquote, ErrorMessageResourceType = typeof(Validation), ErrorMessageResourceName = "EmailId")]
       // [RegularExpression(@"\b[A-Z0-9._%-]+@[A-Z0-9.-]+\.[A-Z]{2,4}\b", ErrorMessageResourceType = typeof(Validation), ErrorMessageResourceName = "RegExEmail")]
        public string EmailId { get; set; }

        /// <summary>
        /// get or set mobile no.
        /// </summary>
        [RequiredIf("RequestType", RequestType.Fullquote, ErrorMessageResourceType = typeof(Validation), ErrorMessageResourceName = "MobileNo")]
        public string MobileNo { get; set; }

        /// <summary>
        /// get or set marital status.
        /// </summary>
        [RequiredIf("RequestType", RequestType.Fullquote, ErrorMessageResourceType = typeof(Validation), ErrorMessageResourceName = "MaritalStatus")]
        public string MaritalStatus { get; set; }

        /// <summary>
        /// get or set salutation.
        /// </summary>
        [RequiredIf("RequestType", RequestType.Fullquote, ErrorMessageResourceType = typeof(Validation), ErrorMessageResourceName = "Salutation")]
        public string Salutation { get; set; }

        /// <summary>
        /// get or set occupation.
        /// </summary>
        [RequiredIf("RequestType", RequestType.Fullquote, ErrorMessageResourceType = typeof(Validation), ErrorMessageResourceName = "Occupation")]
        public string Occupation { get; set; }

        /// <summary>
        /// get or set pan card no.
        /// </summary>
        public string PanCardNo { get; set; }

        /// <summary>
        /// get or set gstin number.
        /// </summary>
        public string GSTIN { get; set; }

        /// <summary>
        /// get or set aadhar card no.
        /// </summary>
        public string AadharNo { get; set; }

        public RequestType RequestType { get; set; }

    }
}