

namespace AndApp.Models
{
    #region namespace
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    #endregion
    public class PremiumBreakUpDetails 
    {
        /// <summary>
        /// get or set basic od premium.
        /// </summary>
        public double BasicODPremium { get; set; }
         
        /// <summary>
        /// getor set electrical accessories premium.
        /// </summary>
        public double ElecAccessoriesPremium { get; set; }

        /// <summary>
        /// get or set non-electrical accessories premium.
        /// </summary>
        public double NonElecAccessoriesPremium { get; set; }

        /// <summary>
        /// get or set fiber glass tank premium.
        /// </summary>
        public double FiberGlassTankPremium { get; set; }

        /// <summary>
        /// get or set cng lpg kit premium.
        /// </summary>
        public double CNGLPGKitPremium { get; set; }

        /// <summary>
        /// get or set loading premium.
        /// </summary>
        public double LoadingPremium { get; set; }

        /// <summary>
        /// get or set aai discount.
        /// </summary>
        public double AAIDiscount { get; set; }

        /// <summary>
        /// get or set anti theft discount.
        /// </summary>
        public double AntiTheftDiscount { get; set; }

        /// <summary>
        /// get or set ncb discount.
        /// </summary>
        public double NCBDiscount { get; set; }

        /// <summary>
        /// get or set loading discount.
        /// </summary>
        public double LoadingDiscount { get; set; }

        /// <summary>
        /// get or set loading discount percentage.
        /// </summary>
        public double LoadingDiscountPercentage { get; set; }

        /// <summary>
        /// get or set voluntary discount.
        /// </summary>
        public double VoluntaryDiscount { get; set; }

        /// <summary>
        /// get or set occupation discount.
        /// </summary>
        public double OccupationDiscount { get; set; }

        /// <summary>
        /// get or set other discount. 
        /// </summary>
        public double OtherDiscount { get; set; }

        /// <summary>
        /// get or set current ncb value.
        /// </summary>
        public double CurrentNCB { get; set; }

        /// <summary>
        /// get or set basic third party liability.
        /// </summary>
        public double BasicThirdPartyLiability { get; set; }

        /// <summary>
        /// get or set restrict liability.
        /// </summary>
        public double RestrictLiability { get; set; }

        /// <summary>
        /// get or set Tp cng lpg premium.
        /// </summary>
        public double TPCNGLPGPremium { get; set; }

        /// <summary>
        /// get or set pa cover to unnamed person. 
        /// </summary>
        public double PACoverToUnNamedPerson { get; set; }
        /// <summary>
        /// get or set pa cover to named person. 
        /// </summary>
        public double PACoverToNamedPerson { get; set; }
        /// <summary>
        /// get or set pa cover to own driver.
        /// </summary>
        public double PACoverToOwnDriver { get; set; }

        /// <summary>
        /// get or set ll to paid driver.
        /// </summary>
        public double LLToPaidDriver { get; set; }
       
        /// <summary>
        /// get or set ll to unnamedPax.
        /// </summary>
        public double LLTounnamedPax { get; set; }

        /// <summary>
        /// get or set pa to paid driver.
        /// </summary>
        public double PAToPaidDriver { get; set; }

        /// <summary>
        /// get or set ll to paid employee.
        /// </summary>
        public double LLToPaidEmployee { get; set; }

        /// <summary>
        /// get or set zero dep premium.
        /// </summary>
        public double ZeroDepPremium { get; set; }

        /// <summary>
        /// get or set rsa premium. 
        /// </summary>
        public double RSAPremium { get; set; }

        /// <summary>
        ///  get or set key replacement premium.
        /// </summary>
        public double KeyReplacementPremium { get; set; }

        /// <summary>
        /// get or set loss of personal belonging premium.
        /// </summary>
        public double LossOfPersonalBelongingPremium { get; set; }

        /// <summary>
        /// get or set cost of consumables premium.
        /// </summary>
        public double CostOfConsumablesPremium { get; set; }

        /// <summary>
        /// get or set engine protector premium.  
        /// </summary>
        public double EngineProtectorPremium { get; set; }

        /// <summary>
        /// get or set invoice price cover premium. 
        /// </summary>
        public double InvoicePriceCoverPremium { get; set; }

        /// <summary>
        /// get or set ncb protector premium.
        /// </summary>
        public double NcbProtectorPremium { get; set; }

        /// <summary>
        /// get or set emergency assistance premium.
        /// </summary>
        public double EmergencyAssistancePremium { get; set; }

        /// <summary>
        /// get or set net od premium.
        /// </summary>
        public double NetODPremium { get; set; }

        /// <summary>
        /// get set net discount.
        /// </summary>
        public double NetDiscount { get; set; }

        /// <summary>
        /// get or set net tp premium.
        /// </summary>
        public double NetTPPremium { get; set; }

        /// <summary>
        /// get or set net addon cover premium.
        /// </summary>
        public double NetAddonPremium { get; set; }

        /// <summary>
        /// get or set net premium.
        /// </summary>
        public double NetPremium { get; set; }

        /// <summary>
        /// get or set service tax.
        /// </summary>
        public double ServiceTax { get; set; }

        /// <summary>
        /// get or set TyreProtect.
        /// </summary>
        public double TyreProtect { get; set; }
        /// <summary>
        /// get or set RimProtectionPremium.
        /// </summary>
        public double RimProtectionPremium { get; set; }
        /// <summary>
        /// get or set HospitalCashCoverPremium.
        /// </summary>
        public double HospitalCashCoverPremium { get; set; }
        /// <summary>
        /// get or set AmbulanceChargesPremium.
        /// </summary>
        public double AmbulanceChargesPremium { get; set; }
        /// <summary>
        /// get or set MedicalExpensesPremium.
        /// </summary>
        public double MedicalExpensesPremium { get; set; }
        /// <summary>
        /// get or set HydrostaticLockCoverPremium.
        /// </summary>
        public double HydrostaticLockCoverPremium { get; set; }


    }
}