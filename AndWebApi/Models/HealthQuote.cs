using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AndWebApi.Models
{
    public class HealthQuote
    {
        [Required(ErrorMessageResourceName = "partnerName", ErrorMessageResourceType = typeof(Validation))]
        public string partnerName { get; set; }
        public string paymenttype { get; set; }
        public string paymentdate { get; set; }
        public string policytype { get; set; }
        [Required(ErrorMessageResourceName = "PolicyStartDate", ErrorMessageResourceType = typeof(Validation))]
        public string startdate { get; set; }
        [Required(ErrorMessageResourceName = "PolicyEndDate", ErrorMessageResourceType = typeof(Validation))]
        public string enddate {get;set;}
        public int policyperiod {get;set;}
       
   
}
}