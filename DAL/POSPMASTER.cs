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
    
    public partial class POSPMASTER
    {
        public long pospid { get; set; }
        public string pospcode { get; set; }
        public string tmpcode { get; set; }
        public Nullable<System.DateTime> tmpcodecreatedon { get; set; }
        public string salutation { get; set; }
        public string firstname { get; set; }
        public string middelname { get; set; }
        public string lastname { get; set; }
        public string gender { get; set; }
        public Nullable<System.DateTime> dateofbirth { get; set; }
        public Nullable<bool> isnameverified { get; set; }
        public string mobileno { get; set; }
        public string emailid { get; set; }
        public string adharcardno { get; set; }
        public Nullable<bool> isadharfrontverified { get; set; }
        public Nullable<bool> isadharbacktverified { get; set; }
        public string pancardno { get; set; }
        public Nullable<bool> ispancardverified { get; set; }
        public string education { get; set; }
        public Nullable<bool> iseducationverified { get; set; }
        public Nullable<bool> isdocverified { get; set; }
        public string docverifiedby { get; set; }
        public Nullable<System.DateTime> docverifiedon { get; set; }
        public string addressline1 { get; set; }
        public string addressline2 { get; set; }
        public string area { get; set; }
        public Nullable<long> stateid { get; set; }
        public Nullable<long> cityid { get; set; }
        public string pincode { get; set; }
        public Nullable<bool> isirdaverified { get; set; }
        public string irdaverifiedby { get; set; }
        public Nullable<System.DateTime> irdaverifiedon { get; set; }
        public Nullable<System.DateTime> createdon { get; set; }
        public Nullable<bool> status { get; set; }
        public string otp { get; set; }
        public Nullable<System.DateTime> otpupdatedon { get; set; }
        public Nullable<long> reffredbyid { get; set; }
        public string reffredtype { get; set; }
        public Nullable<bool> isdocreupload { get; set; }
        public Nullable<System.DateTime> docreuploadon { get; set; }
        public string beneficiaryname { get; set; }
        public string accountno { get; set; }
        public string ifsc { get; set; }
        public Nullable<bool> isdocupload { get; set; }
        public Nullable<System.DateTime> docuploadon { get; set; }
        public string secretkey { get; set; }
        public string aadharcardnoEncrypted { get; set; }
        public string pancardnoEncrypted { get; set; }
        public string aadharcarfrontimage { get; set; }
        public string aadharcardbackimage { get; set; }
        public string pancardimage { get; set; }
        public string educationimage { get; set; }
    }
}
