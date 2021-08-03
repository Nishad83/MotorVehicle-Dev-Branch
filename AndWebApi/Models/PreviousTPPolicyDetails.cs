using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AndWebApi.Models
{
    public class PreviousTPPolicyDetails
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
        public string CompanyName { get; set; }

        /// <summary>
        /// get or set previous policy number.
        /// </summary>
        public string PolicyNo { get; set; }

        public string PolicyEndDate { get; set; }

        /// <summary>
        /// get or set Company Address.
        /// </summary>
        public string CompanyAddress { get; set; }
        public RequestType RequestType { get; set; }
    }
}