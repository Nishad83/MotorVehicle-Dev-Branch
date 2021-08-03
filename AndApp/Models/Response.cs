
namespace AndApp.Models
{
    #region namespace
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    #endregion

    public class Response
    {
        /// <summary>
        /// get or set enquiry id.
        /// </summary>
        public string EnquiryId { get; set; }

        /// <summary>
        /// get or set status.
        /// </summary>
        public Status Status { get; set; }

        /// <summary>
        /// get or set error message.
        /// </summary>
        public string ErrorMsg { get; set; }

        /// <summary>
        /// get or set product.
        /// </summary>
        public Product Product { get; set; }

        /// <summary>
        /// get or set sub product.
        /// </summary>
        public SubProduct SubProduct { get; set; }

        /// <summary>
        /// get or set plan id.
        /// </summary>
        public int PlanId { get; set; }

        /// <summary>
        /// get or set plan name.
        /// </summary>
        public string PlanName { get; set; }

        /// <summary>
        /// get or set company name.
        /// </summary>
        public string CompanyName { get; set; }

        /// <summary>
        /// get or set insured declare value.
        /// </summary>
        public int IDV { get; set; }

        /// <summary>
        /// get or set minimum idv.
        /// </summary>
        public int MinIDV { get; set; }

        /// <summary>
        /// get or set maximum idv.
        /// </summary>
        public int MaxIDV { get; set; }

        /// <summary>
        /// get or set policy start date.
        /// </summary>
        public string PolicyStartDate { get; set; }

        /// <summary>
        /// get or set policy end date.
        /// </summary>
        public string PolicyEndDate { get; set; }

        /// <summary>
        /// get or set vehicle age.
        /// </summary>
        public int VehicleAge{ get; set; }

        /// <summary>
        /// get or set vehicle fuel type.
        /// </summary>
        public string FuelType { get; set; }

        /// <summary>
        /// get or set cubic capacity.
        /// </summary>
        public int CC { get; set; }

        /// <summary>
        /// get or set seating capacity. 
        /// </summary>
        public int SC { get; set; }

        /// <summary>
        /// get or set final premium.
        /// </summary>
        public int FinalPremium { get; set; }

        /// <summary>
        /// get or set policy number.
        /// </summary>
        public string PolicyNo { get; set; }

        /// <summary>
        /// get or set policy pdf url.
        /// </summary>
        public string PolicyPdfUrl { get; set; }

        /// <summary>
        /// get or set proposal number.
        /// </summary>
        public string ProposalNo { get; set; }

        /// <summary>
        /// get or set premium break up details.
        /// </summary>
        public PremiumBreakUpDetails PremiumBreakUpDetails { get; set; }
        public CompanyWiseRefference CompanyWiseDetail { get; set; }
        /// <summary>
        /// get or set is breakin.
        /// </summary>
        public bool isbreakin { get; set; }

        public AddonCover FreeAddonCover { get; set; }
    }
}