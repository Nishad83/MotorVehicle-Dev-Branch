using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AndApp.Models
{
    public class CarDetails
    {
        public string policytype { get; set; }
        public int stateid { get; set; }
        public string cityname { get; set; }

        public string rtocode { get; set; }
        public int makeid { get; set; }
        public int modelid { get; set; }
        public int variantid { get; set; }
        public string fueltype { get; set; }
        public int manufacturingyear { get; set; }
        public string regno { get; set; }

    
public string policystartdate { get; set; }
        public string policyenddate { get; set; }
        public string existingpolicyexpired { get; set; }
        public string claimtaken { get; set; }
    }
}