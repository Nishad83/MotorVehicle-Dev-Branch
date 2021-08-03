using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AndApp.Models
{
    public class CompanyWiseRefference { 
        /// <summary>
        /// get or set applicationId.
        /// </summary>
        public string applicationId { get; set; }

        public string CorrelationId { get; set; }
        /// <summary>
        /// get or set Quote NO.
        /// </summary>
        public string QuoteNo { get; set; }
        /// <summary>
        /// get or set Order NO.
        /// </summary>
        public string OrderNo { get; set; }

        public string QuoteId { get; set; }

        public string LoadingDiscount { get; set; }
    }
}