using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AndApp.Models
{
    public class PaymentRequest
    {
        public string enquiryno { get; set; }
        public string quotationno { get; set; }
        public Company CompanyName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string EmailId { get; set; }
        public string MobileNo { get; set; }
        public double NetPremium { get; set; }
        public double ServiceTax { get; set; }
        public double FinalPremium { get; set; }
        public CompanyWiseRefference CompanyDetail { get; set; }
    }
}