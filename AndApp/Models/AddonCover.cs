using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AndApp.Models
{
    public class AddonCover
    {
        /// <summary>
        /// get or set is zero deperation.
        /// </summary>
        public bool IsZeroDeperation { get; set; }

        /// <summary>
        /// get or set is emergency cover.
        /// </summary>
        public bool IsEmergencyCover { get; set; }

        /// <summary>
        /// get or set is consumables.
        /// </summary>
        public bool IsConsumables { get; set; }

        /// <summary>
        /// get or set is tyre cover.
        /// </summary>
        public bool IsTyreCover { get; set; }

        /// <summary>
        /// get or set is ncb protection.
        /// </summary>
        public bool IsNCBProtection { get; set; }

        /// <summary>
        /// get or set is engine protector.
        /// </summary>
        public bool IsEngineProtector { get; set; }

        /// <summary>
        /// get or set is return to invoice.
        /// </summary>
        public bool IsReturntoInvoice { get; set; }

        /// <summary>
        /// get or set is loss of key.
        /// </summary>
        public bool IsLossofKey { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsLossofpersonalBelonging { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsRoadSideAssistance { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsPassengerAssistcover { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsHydrostaticLockCover { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsHospitalCashCover { get; set; }

      
        /// <summary>
        /// 
        /// </summary>
        public bool IsRimProtectionCover { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool IsMedicalExpensesSelected { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool IsAmbulanceChargesSelected { get; set; }
    }
}