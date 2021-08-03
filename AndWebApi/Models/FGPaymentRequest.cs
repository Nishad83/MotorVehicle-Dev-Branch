using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AndWebApi.Models
{
    public class FGPaymentRequest
    {
        public string PaymentUrl { get; set; }

        public int PaymentOption { get; set; }

        public string TransactionID { get; set; }

        public string ResponseURL { get; set; }

        public string ProposalNumber { get; set; }

        public double PremiumAmount { get; set; }

        public string UserIdentifier { get; set; }

        public string UserId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Mobile { get; set; }

        public string Email { get; set; }
    }
}