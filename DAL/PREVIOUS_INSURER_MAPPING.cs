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
    
    public partial class PREVIOUS_INSURER_MAPPING
    {
        public long id { get; set; }
        public Nullable<long> previouscompanyid { get; set; }
        public string inscompanycode { get; set; }
        public string inscompanyname { get; set; }
        public string inscompanyshortname { get; set; }
        public Nullable<long> companyid { get; set; }
    }
}
