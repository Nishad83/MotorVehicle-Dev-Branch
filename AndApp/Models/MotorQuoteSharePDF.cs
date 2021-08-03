using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AndApp.Models
{
    public class MotorQuoteSharePDF
    {
        public double? gst { get; set; }

        public double? TotalDiscount { get; set; }
        public double? TotalAddon { get; set; }
        public string QuoteNo { get; set; }
        public string CompanyName { get; set; }
        public string ManufactureYear { get; set; }
        public string IDV { get; set; }
        public string netprm { get; set; }
        public string finalprm { get; set; }
        public string Model { get; set; }
        public string VariantName { get; set; }
        public string Fuel { get; set; }
        public string NCBRate { get; set; }
        public string PolicyType { get; set; }
        public string RegNo { get; set; }

        //basic Cover
        public double? BasicODPremium { get; set; }
        public double? EleAccPremium { get; set; }
        public double? NonEleAccPremium { get; set; }
        public double? CngLpgKitPremium { get; set; }

        //TP Premium
        public double? BasicThirdPartyLiability { get; set; }
        public double? PACoverToOwnDriver { get; set; }
        public double? PAToPaidDriver { get; set; }
        public double? TPCNGLPGPremium { get; set; }
        public double? PACoverToUnNamedPerson { get; set; }
        public double? LLToPaidDriver { get; set; }
        public double? LLToPaidEmployee { get; set; }


        //POSP details
        public string PospCode { get; set; }
        public string PospName { get; set; }
        public string PospMobile { get; set; }
        public string PospEmail { get; set; }
        public List<AddonPremium> lstAddonPremiums { get; set; }

    }
    public class AddonPremium
    {
        public string AddonName { get; set; }
        public string AddonVal { get; set; }

    }
}