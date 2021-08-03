using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AndWebApi.Models
{
    public class PremiumDetails
    {
        /// <summary>
        /// get or set AddonPremium.
        /// </summary>
        public string AddonPremium { get; set; }

        /// <summary>
        /// get or set ncbDiscAmount.
        /// </summary>
        public string ncbDiscAmount { get; set; }
        /// <summary>
        /// get or set TaxAmount.
        /// </summary>
        public string TaxAmount { get; set; }
        /// <summary>
        /// get or set basicod.
        /// </summary>
        public string TpPremiumAmount { get; set; }

        /// <summary>
        /// get or set basicod.
        /// </summary>
        public string OdPremiumAmount { get; set; }
        /// <summary>
        /// get or set NetPremiumAmount.
        /// </summary>
        public string NetPremiumAmount { get; set; }
        /// <summary>
        /// get or set TotalPremiumAmount.
        /// </summary>
        public string TotalPremiumAmount { get; set; }
    }
}