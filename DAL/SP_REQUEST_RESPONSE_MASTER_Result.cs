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
    
    public partial class SP_REQUEST_RESPONSE_MASTER_Result
    {
        public long reqid { get; set; }
        public string enquiryid { get; set; }
        public Nullable<int> companyid { get; set; }
        public string request { get; set; }
        public Nullable<System.DateTime> requestedon { get; set; }
        public string response { get; set; }
        public Nullable<System.DateTime> respondedon { get; set; }
        public Nullable<bool> status { get; set; }
    }
}
