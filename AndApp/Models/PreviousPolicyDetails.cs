
namespace AndApp.Models
{
    #region namespace
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    #endregion
    public class PreviousPolicyDetails 
    {
        /// <summary>
        /// get or set previous company id.
        /// </summary>
        public int CompanyId { get; set; }

        /// <summary>
        /// get or set previous company code.
        /// </summary>
        public string CompanyCode { get; set; }

        /// <summary>
        /// get or set previous company name.
        /// </summary>
        public string PreviousCompanyName { get; set; }

        /// <summary>
        /// get or set previous policy number.
        /// </summary>
        public string PreviousPolicyNo { get; set; }

        /// <summary>
        /// get or set previous insurance claimed.
        /// </summary>
        public bool IsPreviousInsuranceClaimed { get; set; }

        /// <summary>
        /// get or set previous policy end date.
        /// </summary>
        public string PreviousPolicyEndDate { get; set; }

        /// <summary>
        /// get or set previous policy start date.
        /// </summary>
        public string PreviousPolicyStartDate { get; set; }

        /// <summary>
        /// get or set previous ncb percentage.
        /// </summary>
        public string PreviousNcbPercentage { get; set; }

        /// <summary>
        /// get or set previous policy type.
        /// </summary>
        public string PreviousPolicyType { get; set; }
        /// <summary>
        /// get or set previous Company Address.
        /// </summary>
        public string PreviousCompanyAddress { get; set; }
        public RequestType RequestType { get; set; }
    }
}