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
    
    public partial class VW_POLICYDETAILSMASTER
    {
        public long pdid { get; set; }
        public string enquiryid { get; set; }
        public Nullable<long> companyid { get; set; }
        public Nullable<long> pospid { get; set; }
        public string custmertype { get; set; }
        public string policytype { get; set; }
        public string producttype { get; set; }
        public Nullable<bool> isrenewable { get; set; }
        public string customerfirstname { get; set; }
        public string customermiddlename { get; set; }
        public string customerlastname { get; set; }
        public string customeraddress { get; set; }
        public string customerpancardno { get; set; }
        public string customergstno { get; set; }
        public string customeremailid { get; set; }
        public Nullable<System.DateTime> customerdob { get; set; }
        public string customermobileno { get; set; }
        public Nullable<long> rtoid { get; set; }
        public Nullable<long> zoneid { get; set; }
        public Nullable<long> makeid { get; set; }
        public Nullable<long> modelid { get; set; }
        public Nullable<long> variantid { get; set; }
        public Nullable<System.DateTime> issuedate { get; set; }
        public Nullable<System.DateTime> registrationdate { get; set; }
        public Nullable<System.DateTime> manufacturdate { get; set; }
        public Nullable<int> seatingcapacity { get; set; }
        public string registrationno { get; set; }
        public Nullable<long> cubiccapacity { get; set; }
        public string engingno { get; set; }
        public string chassisno { get; set; }
        public string fueltype { get; set; }
        public string policyno { get; set; }
        public Nullable<System.DateTime> policystartdate { get; set; }
        public Nullable<System.DateTime> policyenddate { get; set; }
        public Nullable<System.DateTime> tpstartdate { get; set; }
        public Nullable<System.DateTime> tpenddate { get; set; }
        public Nullable<int> tenure { get; set; }
        public Nullable<int> ncdpercentage { get; set; }
        public Nullable<decimal> ncdpercentagevalue { get; set; }
        public Nullable<decimal> otherdiscount { get; set; }
        public Nullable<decimal> idv { get; set; }
        public Nullable<decimal> addonpremium { get; set; }
        public Nullable<decimal> odpremium { get; set; }
        public Nullable<decimal> tppremium { get; set; }
        public Nullable<decimal> netpremium { get; set; }
        public Nullable<decimal> gstvalue { get; set; }
        public Nullable<decimal> finalpremium { get; set; }
        public Nullable<bool> paymentstatus { get; set; }
        public Nullable<bool> status { get; set; }
        public string createdby { get; set; }
        public Nullable<System.DateTime> createdon { get; set; }
        public string updatedby { get; set; }
        public Nullable<System.DateTime> updatedon { get; set; }
        public string companyname { get; set; }
        public string shortname { get; set; }
        public string makename { get; set; }
        public string modelname { get; set; }
        public string variantname { get; set; }
    }
}