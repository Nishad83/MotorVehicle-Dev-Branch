using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AndWebApi.Models
{
    public class FGPaymentResponse
    {
        public string WS_P_ID { get; set; }

        public string TID { get; set; }

        public string PGID { get; set; }

        public double Premium { get; set; }

        public string Response { get; set; }
    }
}