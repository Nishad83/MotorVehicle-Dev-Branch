using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AndApp.Models
{
    public class CommonModels
    {
        public class UserSessionDetails
        {
            public long userid { get; set; }
            public string username { get; set; }
            public string salutation { get; set; }
            public string mobileno { get; set; }
            public string pospcode { get; set; }
            public string emailid { get; set; }
             public string accountno { get; set; }
            public string beneficiaryname { get; set; }
            public string ifsc { get; set; }
            public long? stateid { get; set; }
            public long? cityid { get; set; }

            public string pincode { get; set; }
            public string addressline1 { get; set; }
            public string addressline2 { get; set; }

            public string area { get; set; }
        }
    }
}
