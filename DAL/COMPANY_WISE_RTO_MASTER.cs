//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DAL
{
    using System;
    using System.Collections.Generic;
    
    public partial class COMPANY_WISE_RTO_MASTER
    {
        public long rtoid { get; set; }
        public string rto_loc_code { get; set; }
        public string rto_loc_name { get; set; }
        public string rtolocationgrpcd { get; set; }
        public string rto_zone { get; set; }
        public Nullable<long> companyid { get; set; }
        public Nullable<int> andapp_rtoid { get; set; }
    }
}