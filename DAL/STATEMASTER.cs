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
    
    public partial class STATEMASTER
    {
        public long stateid { get; set; }
        public string statecode { get; set; }
        public string statename { get; set; }
        public Nullable<long> zoneid { get; set; }
        public Nullable<System.DateTime> effectivefrom { get; set; }
        public Nullable<System.DateTime> effectiveto { get; set; }
        public Nullable<bool> status { get; set; }
        public string createdby { get; set; }
        public Nullable<System.DateTime> createdon { get; set; }
        public string updatedby { get; set; }
        public Nullable<System.DateTime> updatedon { get; set; }
    }
}
