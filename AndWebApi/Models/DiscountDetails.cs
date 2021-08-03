namespace AndWebApi.Models
{
    #region namespace
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    #endregion

    public class DiscountDetails 
    {

        /// <summary>
        /// get or set member of auto mobile association Expiry Date.
        /// </summary>
        public string AutomobileAssociationMemberExpiryDate { get; set; }
        /// <summary>
        /// get or set voluntary excess. 
        /// </summary>
        public bool IsVoluntaryExcess { get; set; }

        /// <summary>
        /// get or set anti theft device.
        /// </summary>
        public bool IsAntiTheftDevice { get; set; }

        /// <summary>
        /// get or set member of auto mobile association.
        /// </summary>
        public bool IsMemberOfAutomobileAssociation { get; set; }

        /// <summary>
        /// get or set TPPD restricted.
        /// </summary>
        public bool IsTPPDRestrictedto6000 { get; set; }

        /// <summary>
        /// get or set vehicle use for handicap.
        /// </summary>
        public bool IsUseForHandicap { get; set; }

        /// <summary>
        /// get or set association name.
        /// </summary>
        public string AssociationName { get; set; }

        /// <summary>
        /// get or set member ship no.
        /// </summary>
        public string MembershipNumber { get; set; }

        /// <summary>
        /// get or set voluntary excess amount.
        /// </summary>
        public int VoluntaryExcessAmount { get; set; }

        public RequestType RequestType { get; set; }
    }
}