using System;

namespace AndApp.Models
{
    public class SearchCriteria
    {

        #region Quotation Search Criteria
        public class QuotaionSearchCriteria
        {
            public int variantid { get; set; }
            public int? makeid { get; set; }

            public int? modelid { get; set; }

            public string insurancename { get; set; }

            public string registrationno { get; set; }

            public DateTime? fromdate { get; set; }

            public DateTime? todate { get; set; }

            public int status { get; set; }

            public string quotationno { get; set; }

             public string policyno { get; set; }
            public string paymentstatus { get; set; }
        }
        #endregion
    }

}


