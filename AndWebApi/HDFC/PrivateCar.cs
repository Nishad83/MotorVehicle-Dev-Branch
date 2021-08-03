
namespace AndWebApi.HDFC
{
    #region namespace
    using AndWebApi.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Xml;
    using System.Xml.XPath;
    using System.Xml.Linq;
    using System.IO;
    using AndWebApi;
    using Controllers;
    using System.Web.Script.Serialization;
    using System.Net;
    using System.Text;
    using Newtonsoft.Json.Linq;
    using RestSharp;
    using Newtonsoft.Json;
    using AndApp.Utilities;
    using System.Security.Cryptography;
    using System.Web.Mvc;
    using System.Collections;
    using System.Configuration;
    using System.Net.Security;

    #endregion

    public class PrivateCar
    {
        string baseurl = ConfigurationManager.AppSettings["HDFCBASEURL"];
        string premiumcalcURL = ConfigurationManager.AppSettings["HDFCPREMIUMCALS"];
        string proposalgenerateURL = ConfigurationManager.AppSettings["HDFCPROPOSAL"];

        string merchantkey = "IAND INSURANCE BROKER";
        string Secretetoken = "pce7d5CDaVuvXasix7H6jw==";
        //string featureid = "S001";

        DAL.ANDAPPEntities ap = new DAL.ANDAPPEntities();

        #region  Quote Request - Response
        public Response GetQuoteRequest(Quotation model)
        {
            Response resModel = new Response();
            try
            {
                string PolicyType = "", path = "", filePath = "", json = "";
                dynamic jsonObj = null;
                int CpaYear = 0;
                dynamic jsonObject = string.Empty;
                string RtoLocationCode = string.Empty;
                string externalCNG = string.Empty;

                #region IDV 
                if (model.IDV != 0)
                {
                    if (model.CustomIDV != null)
                    {
                        var getidvvalues = model.CustomIDV.Where(x => x.CompanyName == "HDFC").FirstOrDefault();
                        if (model.IDV == 1)  //1 is for lowest idv of company
                        {
                            model.IDV = getidvvalues.MinIDV;
                        }
                        else if (model.IDV == 2) // 2 is for lowest idv of company
                        {
                            model.IDV = getidvvalues.MaxIDV;
                        }
                        else
                        {
                            if (model.IDV < getidvvalues.MinIDV)
                            {
                                model.IDV = getidvvalues.MinIDV;
                            }
                            else if (model.IDV > getidvvalues.MaxIDV)
                            {
                                model.IDV = getidvvalues.MaxIDV;
                            }
                        }
                    }
                }
                #endregion

                string RegistrationDate = DateTime.Now.ToString("yyyy-MM-dd");

                #region FOR NEW BUSINESS
                if (model.PolicyType == "NEW" || model.PolicyType == "New")
                {
                    PolicyType = "Comprehensive";
                    path = AppDomain.CurrentDomain.BaseDirectory;
                    filePath = Path.Combine(path, "JSON/HDFC/Quote_NEW.json");
                    json = File.ReadAllText(filePath);
                    jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
                }
                #endregion

                #region FOR ROLLOVER 
                if (model.PolicyType == "ROLLOVER" || model.PolicyType == "Rollover" || model.PolicyType == "RollOver")
                {
                    path = AppDomain.CurrentDomain.BaseDirectory;
                    filePath = Path.Combine(path, "JSON/HDFC/Quote.json");
                    json = File.ReadAllText(filePath);
                    jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);

                    string previouscompanyid = GetPreviousInsurerCompany(model.PreviousPolicyDetails.CompanyId, "prvins");

                    RegistrationDate = Convert.ToDateTime(model.VehicleDetails.RegistrationDate).ToString("yyyy-MM-dd");
                    string PrvPolicyEndDate = Convert.ToDateTime(model.PreviousPolicyDetails.PreviousPolicyEndDate).ToString("yyyy-MM-dd");

                    if (model.DontKnowPreviousInsurer == true)
                    {
                        PrvPolicyEndDate = DateTime.Now.AddDays(-2).ToString("yyyy-MM-dd");
                    }

                    int prvncb = Convert.ToInt32(model.PreviousPolicyDetails.PreviousNcbPercentage);
                    if (prvncb > 50)
                    {
                        model.PreviousPolicyDetails.PreviousNcbPercentage = "50";
                    }

                    jsonObj["PreviousPolicyEndDate"] = PrvPolicyEndDate;
                    jsonObj["IsPreviousClaim"] = model.PreviousPolicyDetails != null ? Convert.ToString(model.PreviousPolicyDetails.IsPreviousInsuranceClaimed).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                    jsonObj["PreviousNCBDiscountPercentage"] = model.PreviousPolicyDetails != null ? model.PreviousPolicyDetails.PreviousNcbPercentage : "0";
                    jsonObj["PreviousPolicyType"] = model.PreviousPolicyDetails != null ? model.PreviousPolicyDetails.PreviousPolicyType : "Comprehensive";
                    jsonObj["PreviousInsurerId"] = previouscompanyid;
                }
                #endregion

                #region Policy Type

                #region Comprehensive
                if (model.IsODOnly == false && model.IsThirdPartyOnly == false)
                {
                    PolicyType = "Comprehensive";
                }
                #endregion

                #region ODOnly
                if (model.IsODOnly == true)
                {
                    PolicyType = "ODOnly";
                    string EndDateTP = Convert.ToDateTime(model.PreviousTPPolicyDetails.PolicyEndDate).ToString("yyyy-MM-dd"); ;
                    jsonObj["TPExistingEndDate"] = EndDateTP;
                }
                #endregion

                #region ThirdParty
                if (model.IsThirdPartyOnly == true)
                {
                    PolicyType = "ThirdParty";
                }
                #endregion

                #endregion


                var rtocode = ap.COMPANY_WISE_RTO_MASTER.Where(x => x.companyid == 8 && x.andapp_rtoid == model.VehicleDetails.RtoId).FirstOrDefault();
                if (rtocode != null)
                {
                    RtoLocationCode = rtocode.rtolocationgrpcd;
                }
                else
                {
                    resModel.Status = Status.Fail;
                    resModel.ErrorMsg = "Rto is not provided by HDFC";
                    LogU.WriteLog("HDFC >> PrivateCar >> GetQuoteRequest >> " + resModel.ErrorMsg);
                    return resModel;
                }
                externalCNG = model.VehicleDetails.Fuel;
                if (model.CoverageDetails.IsBiFuelKit == true)
                {
                    externalCNG = "CNG";
                }

                jsonObj["PospCode"] = "";
                jsonObj["TypeOfBusiness"] = model.PolicyType;
                jsonObj["CustomerType"] = model.CustomerType;
                jsonObj["PolicyType"] = PolicyType;
                // jsonObj["CustomerStateCode"] = model.VehicleAddressDetails.State;
                jsonObj["RtoLocationCode"] = RtoLocationCode;
                jsonObj["VehicleMakeCode"] = model.VehicleDetails.MakeCode;
                jsonObj["VehicleModelCode"] = model.VehicleDetails.ModelCode;
                jsonObj["PurchaseRegistrationDate"] = RegistrationDate;
                jsonObj["RequiredIDV"] = model.IDV;

                #region INDIVIDUAL AddOnCovers
                if (model.CustomerType == "INDIVIDUAL" || model.CustomerType == "Individual")
                {
                    #region ODOnly
                    if (PolicyType == "ODOnly")
                    {
                        jsonObj["AddOnCovers"]["IsZeroDepCover"] = model.AddonCover != null ? Convert.ToString(model.AddonCover.IsZeroDeperation).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                        jsonObj["AddOnCovers"]["IsLossOfUse"] = model.AddonCover != null ? Convert.ToString(model.AddonCover.IsLossofpersonalBelonging).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                        jsonObj["AddOnCovers"]["IsEmergencyAssistanceCover"] = model.AddonCover != null ? Convert.ToString(model.AddonCover.IsRoadSideAssistance).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                        jsonObj["AddOnCovers"]["IsNoClaimBonusProtection"] = model.AddonCover != null ? Convert.ToString(model.AddonCover.IsNCBProtection).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                        jsonObj["AddOnCovers"]["IsCostOfConsumable"] = model.AddonCover != null ? Convert.ToString(model.AddonCover.IsConsumables).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                        jsonObj["AddOnCovers"]["IsReturntoInvoice"] = model.AddonCover != null ? Convert.ToString(model.AddonCover.IsReturntoInvoice).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                        jsonObj["AddOnCovers"]["IsEmergencyAssistanceWiderCover"] = model.AddonCover != null ? Convert.ToString(model.AddonCover.IsLossofKey).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                        jsonObj["AddOnCovers"]["IsEngineAndGearboxProtectorCover"] = model.AddonCover != null ? Convert.ToString(model.AddonCover.IsEngineProtector).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                        jsonObj["AddOnCovers"]["IsTyreSecureCover"] = model.AddonCover != null ? Convert.ToString(model.AddonCover.IsTyreCover).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                        jsonObj["AddOnCovers"]["IsZeroDepTakenLastYear"] = model.AddonCover != null ? !string.IsNullOrEmpty(model.AddonCover.NoOfCliamTaken) ? model.AddonCover.NoOfCliamTaken : "0" : "0";
                        jsonObj["AddOnCovers"]["NonelectricalAccessoriesIdv"] = model.CoverageDetails != null ? model.CoverageDetails.SINonElectricalAccessories : 0;
                        jsonObj["AddOnCovers"]["ElectricalAccessoriesIdv"] = model.CoverageDetails != null ? model.CoverageDetails.SIElectricalAccessories : 0;
                        jsonObj["AddOnCovers"]["LpgCngKitIdv"] = model.CoverageDetails != null ? model.CoverageDetails.BiFuelKitAmount : 0;
                        jsonObj["AddOnCovers"]["SelectedFuelType"] = externalCNG;
                    }
                    #endregion

                    #region ThirdParty
                    if (PolicyType == "ThirdParty")
                    {
                        jsonObj["AddOnCovers"]["IsTPPDDiscount"] = model.DiscountDetails != null ? Convert.ToString(model.DiscountDetails.IsTPPDRestrictedto6000).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                        jsonObj["AddOnCovers"]["IsExLpgCngKit"] = model.CoverageDetails != null ? Convert.ToString(model.CoverageDetails.IsBiFuelKit).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                        jsonObj["AddOnCovers"]["IsPAPaidDriver"] = model.CoverageDetails != null ? Convert.ToString(model.CoverageDetails.IsPACoverPaidDriver).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                        jsonObj["AddOnCovers"]["PAPaidDriverSumInsured"] = model.CoverageDetails != null ? model.CoverageDetails.PACoverPaidDriverAmount : 0;
                        jsonObj["AddOnCovers"]["IsPAUnnamedPassenger"] = model.CoverageDetails != null ? Convert.ToString(model.CoverageDetails.IsPACoverUnnamedPerson).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                        jsonObj["AddOnCovers"]["PAUnnamedPassengerNo"] = model.VehicleDetails != null ? model.VehicleDetails.SC : 0;
                        jsonObj["AddOnCovers"]["PAPerUnnamedPassengerSumInsured"] = model.CoverageDetails != null ? model.CoverageDetails.PACoverUnnamedPersonAmount : 0;
                        jsonObj["AddOnCovers"]["IsLLEmployee"] = Convert.ToString(model.CoverageDetails.IsEmployeeLiability).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO";
                        jsonObj["AddOnCovers"]["LLEmployeeNo"] = 1;
                        jsonObj["AddOnCovers"]["CpaYear"] = CpaYear;
                        jsonObj["AddOnCovers"]["IsLegalLiabilityDriver"] = Convert.ToString(model.CoverageDetails.IsLegalLiablityPaidDriver).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO";
                        jsonObj["AddOnCovers"]["LLPaidDriverNo"] = model.CoverageDetails != null ? model.CoverageDetails.NoOfLLPaidDriver : 0;

                    }
                    #endregion

                    #region Comprehensive
                    if (PolicyType == "Comprehensive")
                    {
                        jsonObj["AddOnCovers"]["IsZeroDepCover"] = model.AddonCover != null ? Convert.ToString(model.AddonCover.IsZeroDeperation).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                        jsonObj["AddOnCovers"]["IsLossOfUse"] = model.AddonCover != null ? Convert.ToString(model.AddonCover.IsLossofpersonalBelonging).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                        jsonObj["AddOnCovers"]["IsEmergencyAssistanceCover"] = model.AddonCover != null ? Convert.ToString(model.AddonCover.IsRoadSideAssistance).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                        jsonObj["AddOnCovers"]["IsNoClaimBonusProtection"] = model.AddonCover != null ? Convert.ToString(model.AddonCover.IsNCBProtection).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                        jsonObj["AddOnCovers"]["IsEmergencyAssistanceWiderCover"] = model.AddonCover != null ? Convert.ToString(model.AddonCover.IsLossofKey).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                        jsonObj["AddOnCovers"]["IsEngineAndGearboxProtectorCover"] = model.AddonCover != null ? Convert.ToString(model.AddonCover.IsEngineProtector).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                        jsonObj["AddOnCovers"]["IsCostOfConsumable"] = model.AddonCover != null ? Convert.ToString(model.AddonCover.IsConsumables).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                        jsonObj["AddOnCovers"]["IsReturntoInvoice"] = model.AddonCover != null ? Convert.ToString(model.AddonCover.IsReturntoInvoice).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                        jsonObj["AddOnCovers"]["IsTyreSecureCover"] = model.AddonCover != null ? Convert.ToString(model.AddonCover.IsTyreCover).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                        jsonObj["AddOnCovers"]["IsZeroDepTakenLastYear"] = model.AddonCover != null ? !string.IsNullOrEmpty(model.AddonCover.NoOfCliamTaken) ? model.AddonCover.NoOfCliamTaken : "0" : "0";
                        jsonObj["AddOnCovers"]["IsTPPDDiscount"] = model.DiscountDetails != null ? Convert.ToString(model.DiscountDetails.IsTPPDRestrictedto6000).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                        jsonObj["AddOnCovers"]["IsExLpgCngKit"] = model.CoverageDetails != null ? Convert.ToString(model.CoverageDetails.IsBiFuelKit).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                        jsonObj["AddOnCovers"]["NonelectricalAccessoriesIdv"] = model.CoverageDetails != null ? model.CoverageDetails.SINonElectricalAccessories : 0;
                        jsonObj["AddOnCovers"]["ElectricalAccessoriesIdv"] = model.CoverageDetails != null ? model.CoverageDetails.SIElectricalAccessories : 0;
                        jsonObj["AddOnCovers"]["LpgCngKitIdv"] = model.CoverageDetails != null ? model.CoverageDetails.BiFuelKitAmount : 0;
                        jsonObj["AddOnCovers"]["SelectedFuelType"] = externalCNG;
                        jsonObj["AddOnCovers"]["IsPAPaidDriver"] = model.CoverageDetails != null ? Convert.ToString(model.CoverageDetails.IsPACoverPaidDriver).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                        jsonObj["AddOnCovers"]["PAPaidDriverSumInsured"] = model.CoverageDetails != null ? model.CoverageDetails.PACoverPaidDriverAmount : 0;
                        jsonObj["AddOnCovers"]["IsPAUnnamedPassenger"] = model.CoverageDetails != null ? Convert.ToString(model.CoverageDetails.IsPACoverUnnamedPerson).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                        jsonObj["AddOnCovers"]["PAUnnamedPassengerNo"] = model.VehicleDetails != null ? model.VehicleDetails.SC : 0;
                        jsonObj["AddOnCovers"]["PAPerUnnamedPassengerSumInsured"] = model.CoverageDetails != null ? model.CoverageDetails.PACoverUnnamedPersonAmount : 0;
                        jsonObj["AddOnCovers"]["IsLegalLiabilityDriver"] = Convert.ToString(model.CoverageDetails.IsLegalLiablityPaidDriver).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO";
                        jsonObj["AddOnCovers"]["LLPaidDriverNo"] = model.CoverageDetails != null ? model.CoverageDetails.NoOfLLPaidDriver : 0;
                        jsonObj["AddOnCovers"]["IsLLEmployee"] = Convert.ToString(model.CoverageDetails.IsEmployeeLiability).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO";
                        jsonObj["AddOnCovers"]["LLEmployeeNo"] = 1;
                        jsonObj["AddOnCovers"]["CpaYear"] = CpaYear;
                    }
                    #endregion
                }
                #endregion

                #region CORPORATE AddOnCovers
                if (model.CustomerType == "CORPORATE" || model.CustomerType == "Corporate")
                {
                    #region ODOnly
                    if (PolicyType == "ODOnly")
                    {
                        jsonObj["AddOnCovers"]["IsEngineAndGearboxProtectorCover"] = model.AddonCover != null ? Convert.ToString(model.AddonCover.IsEngineProtector).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                        jsonObj["AddOnCovers"]["IsZeroDepCover"] = model.AddonCover != null ? Convert.ToString(model.AddonCover.IsZeroDeperation).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                        jsonObj["AddOnCovers"]["IsLossOfUse"] = model.AddonCover != null ? Convert.ToString(model.AddonCover.IsLossofpersonalBelonging).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                        jsonObj["AddOnCovers"]["IsEmergencyAssistanceCover"] = model.AddonCover != null ? Convert.ToString(model.AddonCover.IsRoadSideAssistance).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                        jsonObj["AddOnCovers"]["IsNoClaimBonusProtection"] = model.AddonCover != null ? Convert.ToString(model.AddonCover.IsNCBProtection).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                        jsonObj["AddOnCovers"]["IsCostOfConsumable"] = model.AddonCover != null ? Convert.ToString(model.AddonCover.IsConsumables).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                        jsonObj["AddOnCovers"]["IsReturntoInvoice"] = model.AddonCover != null ? Convert.ToString(model.AddonCover.IsReturntoInvoice).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                        jsonObj["AddOnCovers"]["IsEmergencyAssistanceWiderCover"] = model.AddonCover != null ? Convert.ToString(model.AddonCover.IsLossofKey).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                        jsonObj["AddOnCovers"]["IsTyreSecureCover"] = model.AddonCover != null ? Convert.ToString(model.AddonCover.IsTyreCover).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                        jsonObj["AddOnCovers"]["NonelectricalAccessoriesIdv"] = model.CoverageDetails != null ? model.CoverageDetails.SINonElectricalAccessories : 0;
                        jsonObj["AddOnCovers"]["ElectricalAccessoriesIdv"] = model.CoverageDetails != null ? model.CoverageDetails.SIElectricalAccessories : 0;
                        jsonObj["AddOnCovers"]["LpgCngKitIdv"] = model.CoverageDetails != null ? model.CoverageDetails.BiFuelKitAmount : 0;
                        jsonObj["AddOnCovers"]["SelectedFuelType"] = externalCNG;
                        jsonObj["AddOnCovers"]["IsZeroDepTakenLastYear"] = model.AddonCover != null ? !string.IsNullOrEmpty(model.AddonCover.NoOfCliamTaken) ? model.AddonCover.NoOfCliamTaken : "0" : "0";
                    }
                    #endregion

                    #region ThirdParty
                    if (PolicyType == "ThirdParty")
                    {
                        jsonObj["AddOnCovers"]["IsTPPDDiscount"] = model.DiscountDetails != null ? Convert.ToString(model.DiscountDetails.IsTPPDRestrictedto6000).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                        jsonObj["AddOnCovers"]["IsExLpgCngKit"] = model.CoverageDetails != null ? Convert.ToString(model.CoverageDetails.IsBiFuelKit).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                        jsonObj["AddOnCovers"]["IsPAPaidDriver"] = model.CoverageDetails != null ? Convert.ToString(model.CoverageDetails.IsPACoverPaidDriver).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                        jsonObj["AddOnCovers"]["PAPaidDriverSumInsured"] = model.CoverageDetails != null ? model.CoverageDetails.PACoverPaidDriverAmount : 0;
                        jsonObj["AddOnCovers"]["IsPAUnnamedPassenger"] = model.CoverageDetails != null ? Convert.ToString(model.CoverageDetails.IsPACoverUnnamedPerson).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                        jsonObj["AddOnCovers"]["PAUnnamedPassengerNo"] = model.VehicleDetails != null ? model.VehicleDetails.SC : 0;
                        jsonObj["AddOnCovers"]["PAPerUnnamedPassengerSumInsured"] = model.CoverageDetails != null ? model.CoverageDetails.PACoverUnnamedPersonAmount : 0;
                        jsonObj["AddOnCovers"]["IsLLEmployee"] = Convert.ToString(model.CoverageDetails.IsEmployeeLiability).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO";
                        jsonObj["AddOnCovers"]["LLEmployeeNo"] = 1;
                        jsonObj["AddOnCovers"]["IsLegalLiabilityDriver"] = Convert.ToString(model.CoverageDetails.IsLegalLiablityPaidDriver).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO";
                        jsonObj["AddOnCovers"]["LLPaidDriverNo"] = model.CoverageDetails != null ? model.CoverageDetails.NoOfLLPaidDriver : 0;
                    }
                    #endregion

                    #region Comprehensive
                    if (PolicyType == "Comprehensive")
                    {
                        jsonObj["AddOnCovers"]["IsZeroDepCover"] = model.AddonCover != null ? Convert.ToString(model.AddonCover.IsZeroDeperation).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO"; ;
                        jsonObj["AddOnCovers"]["IsEngineAndGearboxProtectorCover"] = model.AddonCover != null ? Convert.ToString(model.AddonCover.IsEngineProtector).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                        jsonObj["AddOnCovers"]["IsLossOfUse"] = model.AddonCover != null ? Convert.ToString(model.AddonCover.IsLossofpersonalBelonging).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                        jsonObj["AddOnCovers"]["IsEmergencyAssistanceCover"] = model.AddonCover != null ? Convert.ToString(model.AddonCover.IsRoadSideAssistance).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                        jsonObj["AddOnCovers"]["IsNoClaimBonusProtection"] = model.AddonCover != null ? Convert.ToString(model.AddonCover.IsNCBProtection).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                        jsonObj["AddOnCovers"]["IsEmergencyAssistanceWiderCover"] = model.AddonCover != null ? Convert.ToString(model.AddonCover.IsLossofKey).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                        jsonObj["AddOnCovers"]["IsCostOfConsumable"] = model.AddonCover != null ? Convert.ToString(model.AddonCover.IsConsumables).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                        jsonObj["AddOnCovers"]["IsReturntoInvoice"] = model.AddonCover != null ? Convert.ToString(model.AddonCover.IsReturntoInvoice).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                        jsonObj["AddOnCovers"]["IsTyreSecureCover"] = model.AddonCover != null ? Convert.ToString(model.AddonCover.IsTyreCover).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                        jsonObj["AddOnCovers"]["IsZeroDepTakenLastYear"] = model.AddonCover != null ? !string.IsNullOrEmpty(model.AddonCover.NoOfCliamTaken) ? model.AddonCover.NoOfCliamTaken : "0" : "0";
                        jsonObj["AddOnCovers"]["IsTPPDDiscount"] = model.DiscountDetails != null ? Convert.ToString(model.DiscountDetails.IsTPPDRestrictedto6000).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                        jsonObj["AddOnCovers"]["IsExLpgCngKit"] = model.CoverageDetails != null ? Convert.ToString(model.CoverageDetails.IsBiFuelKit).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                        jsonObj["AddOnCovers"]["NonelectricalAccessoriesIdv"] = model.CoverageDetails != null ? model.CoverageDetails.SINonElectricalAccessories : 0;
                        jsonObj["AddOnCovers"]["ElectricalAccessoriesIdv"] = model.CoverageDetails != null ? model.CoverageDetails.SIElectricalAccessories : 0;
                        jsonObj["AddOnCovers"]["LpgCngKitIdv"] = model.CoverageDetails != null ? model.CoverageDetails.BiFuelKitAmount : 0;
                        jsonObj["AddOnCovers"]["SelectedFuelType"] = externalCNG;
                        jsonObj["AddOnCovers"]["IsPAPaidDriver"] = model.CoverageDetails != null ? Convert.ToString(model.CoverageDetails.IsPACoverPaidDriver).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                        jsonObj["AddOnCovers"]["PAPaidDriverSumInsured"] = model.CoverageDetails != null ? model.CoverageDetails.PACoverPaidDriverAmount : 0;
                        jsonObj["AddOnCovers"]["IsPAUnnamedPassenger"] = model.CoverageDetails != null ? Convert.ToString(model.CoverageDetails.IsPACoverUnnamedPerson).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                        jsonObj["AddOnCovers"]["PAUnnamedPassengerNo"] = model.VehicleDetails != null ? model.VehicleDetails.SC : 0;
                        jsonObj["AddOnCovers"]["PAPerUnnamedPassengerSumInsured"] = model.CoverageDetails != null ? model.CoverageDetails.PACoverUnnamedPersonAmount : 0;
                        jsonObj["AddOnCovers"]["IsLegalLiabilityDriver"] = Convert.ToString(model.CoverageDetails.IsLegalLiablityPaidDriver).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO";
                        jsonObj["AddOnCovers"]["LLPaidDriverNo"] = model.CoverageDetails != null ? model.CoverageDetails.NoOfLLPaidDriver : 0;
                        jsonObj["AddOnCovers"]["IsLLEmployee"] = Convert.ToString(model.CoverageDetails.IsEmployeeLiability).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO";
                        jsonObj["AddOnCovers"]["LLEmployeeNo"] = 1;

                    }
                    #endregion
                }
                #endregion

                string requestjson = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);

                dynamic responsePremCalc = Webservice(requestjson.ToString(), baseurl, premiumcalcURL);
                if (responsePremCalc.StatusCode == HttpStatusCode.OK)
                {
                    resModel = GetQuoteResponse(responsePremCalc, model);
                }
                else
                {
                    resModel.Status = Status.Fail;
                    resModel.ErrorMsg = Convert.ToString(responsePremCalc.StatusCode);
                    LogU.WriteLog("HDFC >> PrivateCar >> GetQuoteRequest >> " + Convert.ToString(responsePremCalc.StatusCode));
                }
            }
            catch (Exception ex)
            {
                resModel.Status = Status.Fail;
                resModel.ErrorMsg = Convert.ToString(ex.Message);
                Console.Write(Convert.ToString(ex.Message));
                LogU.WriteLog("HDFC >> PrivateCar >> GetQuoteRequest >> " + Convert.ToString(ex.Message));
                // throw;
            }
            return resModel;
        }

        public Response GetQuoteResponse(dynamic responsePremCalc, Quotation model)
        {
            Response resModel = new Response();
            try
            {
                dynamic Response = (JObject)JsonConvert.DeserializeObject(responsePremCalc.Content.ToString());
                string responseMessage = Convert.ToString(Response.Message);

                #region QUOTE RESPONSE
                if (string.IsNullOrEmpty(responseMessage))
                {
                    dynamic responseData = Response.Data[0];
                    dynamic responseStatus = Response.Status;

                    PremiumBreakUpDetails PremiumBreakUp = new PremiumBreakUpDetails();
                    CompanyWiseRefference comReff = new CompanyWiseRefference();



                    comReff.applicationId = Convert.ToString(Response.UniqueRequestID);
                    double AddonPRemium = 0, TPPremium = 0, llemployee = 0;

                    #region Covers Details
                    var AddonsCoverDetails = responseData.AddOnCovers;
                    for (int i = 0; i < responseData.AddOnCovers.Count; i++)
                    {
                        string CoverName = Convert.ToString(responseData.AddOnCovers[i].CoverName);

                        if (CoverName == "PACoverOwnerDriver")
                        {
                            TPPremium += Convert.ToDouble(responseData.AddOnCovers[i].CoverPremium);
                            PremiumBreakUp.PACoverToOwnDriver = Convert.ToDouble(responseData.AddOnCovers[i].CoverPremium);
                            continue;
                        }

                        if (CoverName == "ZERODEP" && model.AddonCover.IsZeroDeperation == true)
                        {
                            PremiumBreakUp.ZeroDepPremium = Convert.ToDouble(responseData.AddOnCovers[i].CoverPremium);
                            AddonPRemium += Convert.ToDouble(responseData.AddOnCovers[i].CoverPremium);
                            continue;
                        }

                        if (CoverName == "EMERGASSIST" && model.AddonCover.IsRoadSideAssistance == true)
                        {
                            PremiumBreakUp.RSAPremium = Convert.ToDouble(responseData.AddOnCovers[i].CoverPremium);
                            AddonPRemium += Convert.ToDouble(responseData.AddOnCovers[i].CoverPremium);
                            continue;
                        }
                        if (CoverName == "LOSSUSEDOWN" && model.AddonCover.IsLossofpersonalBelonging == true)
                        {
                            PremiumBreakUp.LossOfPersonalBelongingPremium = Convert.ToDouble(responseData.AddOnCovers[i].CoverPremium);
                            AddonPRemium += Convert.ToDouble(responseData.AddOnCovers[i].CoverPremium);
                            continue;
                        }
                        if (CoverName == "NCBPROT" && model.AddonCover.IsNCBProtection == true)
                        {
                            PremiumBreakUp.NcbProtectorPremium = Convert.ToDouble(responseData.AddOnCovers[i].CoverPremium);
                            AddonPRemium += Convert.ToDouble(responseData.AddOnCovers[i].CoverPremium);
                            continue;
                        }
                        if (CoverName == "ENGEBOX" && model.AddonCover.IsEngineProtector == true)
                        {
                            PremiumBreakUp.EngineProtectorPremium = Convert.ToDouble(responseData.AddOnCovers[i].CoverPremium);
                            AddonPRemium += Convert.ToDouble(responseData.AddOnCovers[i].CoverPremium);
                            continue;
                        }
                        if (CoverName == "COSTCONS" && model.AddonCover.IsConsumables == true)
                        {
                            PremiumBreakUp.CostOfConsumablesPremium = Convert.ToDouble(responseData.AddOnCovers[i].CoverPremium);
                            AddonPRemium += Convert.ToDouble(responseData.AddOnCovers[i].CoverPremium);
                            continue;
                        }
                        if (CoverName == "RTI" && model.AddonCover.IsReturntoInvoice == true)
                        {
                            PremiumBreakUp.InvoicePriceCoverPremium = Convert.ToDouble(responseData.AddOnCovers[i].CoverPremium);
                            AddonPRemium += Convert.ToDouble(responseData.AddOnCovers[i].CoverPremium);
                            continue;
                        }
                        if (CoverName == "EMERGASSISTWIDER" && model.AddonCover.IsLossofKey == true)
                        {
                            PremiumBreakUp.KeyReplacementPremium = Convert.ToDouble(responseData.AddOnCovers[i].CoverPremium);
                            AddonPRemium += Convert.ToDouble(responseData.AddOnCovers[i].CoverPremium);
                            continue;
                        }
                        if (CoverName == "TYRESECURE" && model.AddonCover.IsTyreCover == true)
                        {
                            PremiumBreakUp.TyreProtect = Convert.ToDouble(responseData.AddOnCovers[i].CoverPremium);
                            AddonPRemium += Convert.ToDouble(responseData.AddOnCovers[i].CoverPremium);
                            continue;
                        }

                        if (CoverName == "LLPaidDriver")
                        {
                            PremiumBreakUp.LLToPaidDriver = Convert.ToDouble(responseData.AddOnCovers[i].CoverPremium);
                            continue;
                        }

                        if (CoverName == "LLEmployee")
                        {
                            PremiumBreakUp.LLToPaidEmployee = Convert.ToDouble(responseData.AddOnCovers[i].CoverPremium);
                            // llemployee += Convert.ToDouble(responseData.AddOnCovers[i].CoverPremium);
                            continue;
                        }

                        if (CoverName == "LpgCngKitIdvOD")
                        {
                            PremiumBreakUp.CNGLPGKitPremium = Convert.ToDouble(responseData.AddOnCovers[i].CoverPremium);
                            continue;
                        }
                        if (CoverName == "LpgCngKitIdvTP")
                        {
                            PremiumBreakUp.TPCNGLPGPremium = Convert.ToDouble(responseData.AddOnCovers[i].CoverPremium);
                            continue;
                        }

                        if (CoverName == "UnnamedPassenger")
                        {
                            PremiumBreakUp.PACoverToUnNamedPerson = Convert.ToDouble(responseData.AddOnCovers[i].CoverPremium);
                            continue;
                        }
                        if (CoverName == "PAPaidDriver")
                        {
                            PremiumBreakUp.PAToPaidDriver = Convert.ToDouble(responseData.AddOnCovers[i].CoverPremium);
                            continue;
                        }
                        if (CoverName == "ElectricalAccessoriesIdv")
                        {
                            PremiumBreakUp.ElecAccessoriesPremium = Convert.ToDouble(responseData.AddOnCovers[i].CoverPremium);
                            continue;
                        }
                        if (CoverName == "NonelectricalAccessoriesIdv")
                        {
                            PremiumBreakUp.NonElecAccessoriesPremium = Convert.ToDouble(responseData.AddOnCovers[i].CoverPremium);
                            continue;
                        }
                    }
                    #endregion

                    #region Calculation
                    resModel.IDV = Convert.ToDouble(responseData.VehicleIdv);
                    resModel.MinIDV = Convert.ToDouble(responseData.VehicleIdvMin);
                    resModel.MaxIDV = Convert.ToDouble(responseData.VehicleIdvMax);

                    resModel.PolicyStartDate = Convert.ToString(responseData.NewPolicyStartDate);
                    resModel.PolicyEndDate = Convert.ToString(responseData.NewPolicyEndDate);

                    resModel.TPStartDate = Convert.ToString(responseData.TPNewPolicyStartDate);
                    resModel.TPEndDate = Convert.ToString(responseData.TPNewPolicyEndDate);

                    double NetPrm = 0;
                    if (model.IsODOnly == true)
                    {
                        PremiumBreakUp.BasicODPremium = Convert.ToDouble(responseData.BasicODPremium);
                        PremiumBreakUp.BasicThirdPartyLiability = 0;
                        PremiumBreakUp.NCBDiscount = Convert.ToDouble(responseData.NewNcbDiscountAmount);
                        NetPrm = Convert.ToDouble(responseData.NetPremiumAmount);
                    }

                    else if (model.IsThirdPartyOnly == true)
                    {
                        PremiumBreakUp.BasicODPremium = 0;
                        PremiumBreakUp.BasicThirdPartyLiability = Convert.ToDouble(responseData.BasicTPPremium) - TPPremium;
                        NetPrm = Convert.ToDouble(responseData.NetPremiumAmount);
                    }
                    else
                    {
                        if (model.PolicyType.ToUpper() == "ROLLOVER")
                        {
                            PremiumBreakUp.NCBDiscount = Convert.ToDouble(responseData.NewNcbDiscountAmount);
                        }

                        PremiumBreakUp.BasicODPremium = Convert.ToDouble(responseData.BasicODPremium);
                        PremiumBreakUp.BasicThirdPartyLiability = Convert.ToDouble(responseData.BasicTPPremium) - TPPremium;
                        NetPrm = Convert.ToDouble(responseData.NetPremiumAmount);
                    }

                    PremiumBreakUp.NetAddonPremium = AddonPRemium;

                    double nP = NetPrm;
                    double GST = nP * .18;
                    double gstceiling = GST - Math.Round(GST);
                    if (gstceiling == 0.5)
                    {
                        PremiumBreakUp.ServiceTax = Math.Ceiling(GST);
                    }
                    else
                    {
                        PremiumBreakUp.ServiceTax = Math.Round(GST);
                    }
                   
                    double FP = Math.Round(nP + GST);

                    PremiumBreakUp.NetPremium = nP;
                    PremiumBreakUp.ServiceTax = GST;
                    resModel.FinalPremium = FP;

                    string planetype = "Comprehensive";
                    if (model.IsODOnly == true)
                    {
                        planetype = "ODOnly";
                    }
                    if (model.IsThirdPartyOnly == true)
                    {
                        planetype = "ThirdParty";
                    }
                    #endregion



                    resModel.isbreakin = Convert.ToBoolean(responseData.IsBreakin);
                    resModel.PlanName = planetype;
                    resModel.CompanyWiseRefference = comReff;
                    resModel.PremiumBreakUpDetails = PremiumBreakUp;
                    resModel.Status = Status.Success;
                    resModel.Product = Product.Motor;
                    resModel.SubProduct = SubProduct.PrivateCar;
                    resModel.CompanyName = Company.HDFC.ToString();

                    if (Convert.ToBoolean(responseData.IsBreakin) == true)
                    {
                        resModel.Status = Status.Fail;
                        resModel.ErrorMsg = Convert.ToString(responseData.BreakinMessage);
                        LogU.WriteLog("HDFC >> PrivateCar >> GetQuoteRequest >> " + Convert.ToString(responseData.BreakinMessage));
                        return resModel;
                    }
                }
                else
                {
                    resModel.Status = Status.Fail;
                    resModel.ErrorMsg = Convert.ToString(responseMessage);
                    LogU.WriteLog("HDFC >> PrivateCar >> GetQuoteRequest >> " + Convert.ToString(responseMessage));
                }
                #endregion
            }
            catch (Exception ex)
            {
                resModel.Status = Status.Fail;
                resModel.ErrorMsg = Convert.ToString(ex.Message);
                Console.Write(Convert.ToString(ex.Message));
                LogU.WriteLog("HDFC >> PrivateCar >> GetQuoteResponse >> " + Convert.ToString(ex.Message));
                //  throw;
            }

            return resModel;
        }
        #endregion

        #region Proposal Request - Response
        public Response GetProposalRequest(Quotation model)
        {
            Response resModel = new Response();

            string PolicyType = "", path = "", filePath = "", json = "";
            dynamic jsonObj = null;
            int CpaYear = 0;

            string RtoLocationCode = string.Empty;
            string externalCNG = model.VehicleDetails.Fuel;
            if (model.CoverageDetails.IsBiFuelKit == true)
            {
                externalCNG = "CNG";
            }

            var rtocode = ap.COMPANY_WISE_RTO_MASTER.Where(x => x.companyid == 8 && x.andapp_rtoid == model.VehicleDetails.RtoId).FirstOrDefault();
            if (rtocode != null)
            {
                RtoLocationCode = rtocode.rtolocationgrpcd;
            }
            else
            {
                resModel.Status = Status.Fail;
                resModel.ErrorMsg = "Rto is not provided by HDFC";
                LogU.WriteLog("HDFC >> PrivateCar >> GetProposalRequest >> " + resModel.ErrorMsg);
                return resModel;
            }

            #region POLICY TYPE
            #region Comprehensive
            if (model.IsODOnly == false && model.IsThirdPartyOnly == false)
            {
                PolicyType = "Comprehensive";
            }
            #endregion

            #region ODOnly
            if (model.IsODOnly == true)
            {
                PolicyType = "ODOnly";
            }
            #endregion

            #region ThirdParty
            if (model.IsThirdPartyOnly == true)
            {
                PolicyType = "ThirdParty";
            }
            #endregion

            #endregion

            #region FOR NEW 
            if (model.PolicyType == "NEW" || model.PolicyType == "New")
            {
                jsonObj = null;
                path = AppDomain.CurrentDomain.BaseDirectory;
                filePath = Path.Combine(path, "JSON/HDFC/Proposal_NEW.json");
                json = File.ReadAllText(filePath);
                jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
            }
            #endregion

            #region FOR ROLLOVER
            if (model.PolicyType == "ROLLOVER" || model.PolicyType == "ROllOver" || model.PolicyType == "Rollover")
            {
                jsonObj = null;


                string previouscompanyid = GetPreviousInsurerCompany(model.PreviousPolicyDetails.CompanyId, "prvins");

                #region Read Json File

                if (model.IsODOnly == true)
                {
                    path = AppDomain.CurrentDomain.BaseDirectory;
                    filePath = Path.Combine(path, "JSON/HDFC/ProposalOnlyIOD.json");
                    json = File.ReadAllText(filePath);
                    jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);

                    DateTime tpstartdate = Convert.ToDateTime(model.PreviousTPPolicyDetails.PolicyEndDate).AddYears(-3);
                    string tartdateTP = tpstartdate.ToString("yyyy-MM-dd");

                    string PrvPolicyEndDateTP = Convert.ToDateTime(model.PreviousTPPolicyDetails.PolicyEndDate).ToString("yyyy-MM-dd");
                    jsonObj["CustomerDetails"]["TPExisitingInsurerCode"] = previouscompanyid;
                    jsonObj["CustomerDetails"]["TPExisitingPolicyNumber"] = model.PreviousTPPolicyDetails.PolicyNo;
                    jsonObj["CustomerDetails"]["TPExistingStartDate"] = tartdateTP;
                    jsonObj["CustomerDetails"]["TPExistingEndDate"] = PrvPolicyEndDateTP;
                }
                else
                {
                    path = AppDomain.CurrentDomain.BaseDirectory;
                    filePath = Path.Combine(path, "JSON/HDFC/Proposal.json");
                    json = File.ReadAllText(filePath);
                    jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
                }

                int prvncb = Convert.ToInt32(model.PreviousPolicyDetails.PreviousNcbPercentage);
                if (prvncb > 50)
                {
                    model.PreviousPolicyDetails.PreviousNcbPercentage = "50";
                }

                string PrvPolicyEndDate = Convert.ToDateTime(model.PreviousPolicyDetails.PreviousPolicyEndDate).ToString("yyyy-MM-dd");
                jsonObj["ProposalDetails"]["PreviousInsurerCode"] = previouscompanyid;
                jsonObj["ProposalDetails"]["PreviousNcbDiscountPercentage"] = model.PreviousPolicyDetails != null ? model.PreviousPolicyDetails.PreviousNcbPercentage : "0";
                jsonObj["ProposalDetails"]["PreviousPolicyEndDate"] = PrvPolicyEndDate;
                jsonObj["ProposalDetails"]["PreviousPolicyNumber"] = model.PreviousPolicyDetails != null ? model.PreviousPolicyDetails.PreviousPolicyNo : string.Empty;
                jsonObj["ProposalDetails"]["PreviousPolicyType"] = model.PreviousPolicyDetails != null ? model.PreviousPolicyDetails.PreviousPolicyType : string.Empty;
                #endregion
            }
            #endregion

            #region INDIVIDUAL AddOnCovers
            if (model.CustomerType == "INDIVIDUAL" || model.CustomerType == "Individual")
            {
                #region ODOnly
                if (PolicyType == "ODOnly")
                {
                    jsonObj["ProposalDetails"]["AddOnCovers"]["IsEngineAndGearboxProtectorCover"] = model.AddonCover != null ? Convert.ToString(model.AddonCover.IsEngineProtector).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                    jsonObj["ProposalDetails"]["AddOnCovers"]["IsEmergencyAssistanceCover"] = model.AddonCover != null ? Convert.ToString(model.AddonCover.IsRoadSideAssistance).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                    jsonObj["ProposalDetails"]["AddOnCovers"]["IsEmergencyAssistanceWiderCover"] = model.AddonCover != null ? Convert.ToString(model.AddonCover.IsLossofKey).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                    jsonObj["ProposalDetails"]["AddOnCovers"]["IsZeroDepTakenLastYear"] = model.AddonCover != null ? !string.IsNullOrEmpty(model.AddonCover.NoOfCliamTaken) ? model.AddonCover.NoOfCliamTaken : "0" : "0";
                    jsonObj["ProposalDetails"]["AddOnCovers"]["IsZeroDepCover"] = model.AddonCover != null ? Convert.ToString(model.AddonCover.IsZeroDeperation).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                    jsonObj["ProposalDetails"]["AddOnCovers"]["IsLossOfUse"] = model.AddonCover != null ? Convert.ToString(model.AddonCover.IsLossofpersonalBelonging).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                    jsonObj["ProposalDetails"]["AddOnCovers"]["IsNoClaimBonusProtection"] = model.AddonCover != null ? Convert.ToString(model.AddonCover.IsNCBProtection).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                    jsonObj["ProposalDetails"]["AddOnCovers"]["IsCostOfConsumable"] = model.AddonCover != null ? Convert.ToString(model.AddonCover.IsConsumables).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                    jsonObj["ProposalDetails"]["AddOnCovers"]["IsReturntoInvoice"] = model.AddonCover != null ? Convert.ToString(model.AddonCover.IsReturntoInvoice).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                    jsonObj["ProposalDetails"]["AddOnCovers"]["IsTyreSecureCover"] = model.AddonCover != null ? Convert.ToString(model.AddonCover.IsTyreCover).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                    jsonObj["ProposalDetails"]["AddOnCovers"]["NonelectricalAccessoriesIdv"] = model.CoverageDetails != null ? model.CoverageDetails.SINonElectricalAccessories : 0;
                    jsonObj["ProposalDetails"]["AddOnCovers"]["ElectricalAccessoriesIdv"] = model.CoverageDetails != null ? model.CoverageDetails.SIElectricalAccessories : 0;
                    jsonObj["ProposalDetails"]["AddOnCovers"]["LpgCngKitIdv"] = model.CoverageDetails != null ? model.CoverageDetails.BiFuelKitAmount : 0;
                    jsonObj["ProposalDetails"]["AddOnCovers"]["SelectedFuelType"] = externalCNG;
                }
                #endregion

                #region ThirdParty
                if (PolicyType == "ThirdParty")
                {
                    jsonObj["ProposalDetails"]["AddOnCovers"]["IsTPPDDiscount"] = Convert.ToString(model.DiscountDetails.IsTPPDRestrictedto6000).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO";
                    jsonObj["ProposalDetails"]["AddOnCovers"]["IsExLpgCngKit"] = Convert.ToString(model.CoverageDetails.IsBiFuelKit).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO";
                    jsonObj["ProposalDetails"]["AddOnCovers"]["IsPAPaidDriver"] = Convert.ToString(model.CoverageDetails.IsPACoverPaidDriver).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO";
                    jsonObj["ProposalDetails"]["AddOnCovers"]["PAPaidDriverSumInsured"] = model.CoverageDetails.PACoverPaidDriverAmount;
                    jsonObj["ProposalDetails"]["AddOnCovers"]["IsPAUnnamedPassenger"] = Convert.ToString(model.CoverageDetails.IsPACoverUnnamedPerson).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO";
                    jsonObj["ProposalDetails"]["AddOnCovers"]["PAUnnamedPassengerNo"] = model.VehicleDetails != null ? model.VehicleDetails.SC : 0;
                    jsonObj["ProposalDetails"]["AddOnCovers"]["PAPerUnnamedPassengerSumInsured"] = model.CoverageDetails != null ? model.CoverageDetails.PACoverUnnamedPersonAmount : 0;
                    jsonObj["ProposalDetails"]["AddOnCovers"]["IsLLEmployee"] = Convert.ToString(model.CoverageDetails.IsEmployeeLiability).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO";
                    jsonObj["ProposalDetails"]["AddOnCovers"]["LLEmployeeNo"] = 1;
                    jsonObj["ProposalDetails"]["AddOnCovers"]["CpaYear"] = CpaYear;
                    jsonObj["ProposalDetails"]["AddOnCovers"]["IsLegalLiabilityDriver"] = Convert.ToString(model.CoverageDetails.IsLegalLiablityPaidDriver).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO";
                    jsonObj["ProposalDetails"]["AddOnCovers"]["LLPaidDriverNo"] = model.CoverageDetails != null ? model.CoverageDetails.NoOfLLPaidDriver : 0;
                }
                #endregion

                #region Comprehensive
                if (PolicyType == "Comprehensive")
                {
                    jsonObj["ProposalDetails"]["AddOnCovers"]["IsEngineAndGearboxProtectorCover"] = model.AddonCover != null ? Convert.ToString(model.AddonCover.IsEngineProtector).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                    jsonObj["ProposalDetails"]["AddOnCovers"]["IsZeroDepCover"] = model.AddonCover != null ? Convert.ToString(model.AddonCover.IsZeroDeperation).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                    jsonObj["ProposalDetails"]["AddOnCovers"]["IsLossOfUse"] = model.AddonCover != null ? Convert.ToString(model.AddonCover.IsLossofpersonalBelonging).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                    jsonObj["ProposalDetails"]["AddOnCovers"]["IsEmergencyAssistanceCover"] = model.AddonCover != null ? Convert.ToString(model.AddonCover.IsRoadSideAssistance).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                    jsonObj["ProposalDetails"]["AddOnCovers"]["IsEmergencyAssistanceWiderCover"] = model.AddonCover != null ? Convert.ToString(model.AddonCover.IsLossofKey).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                    jsonObj["ProposalDetails"]["AddOnCovers"]["IsNoClaimBonusProtection"] = model.AddonCover != null ? Convert.ToString(model.AddonCover.IsNCBProtection).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                    jsonObj["ProposalDetails"]["AddOnCovers"]["IsCostOfConsumable"] = model.AddonCover != null ? Convert.ToString(model.AddonCover.IsConsumables).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                    jsonObj["ProposalDetails"]["AddOnCovers"]["IsReturntoInvoice"] = model.AddonCover != null ? Convert.ToString(model.AddonCover.IsReturntoInvoice).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                    jsonObj["ProposalDetails"]["AddOnCovers"]["IsTyreSecureCover"] = model.AddonCover != null ? Convert.ToString(model.AddonCover.IsTyreCover).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                    jsonObj["ProposalDetails"]["AddOnCovers"]["IsZeroDepTakenLastYear"] = model.AddonCover != null ? !string.IsNullOrEmpty(model.AddonCover.NoOfCliamTaken) ? model.AddonCover.NoOfCliamTaken : "0" : "0";
                    jsonObj["ProposalDetails"]["AddOnCovers"]["IsTPPDDiscount"] = Convert.ToString(model.DiscountDetails.IsTPPDRestrictedto6000).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO";
                    jsonObj["ProposalDetails"]["AddOnCovers"]["IsExLpgCngKit"] = Convert.ToString(model.CoverageDetails.IsBiFuelKit).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO";
                    jsonObj["ProposalDetails"]["AddOnCovers"]["NonelectricalAccessoriesIdv"] = model.CoverageDetails != null ? model.CoverageDetails.SINonElectricalAccessories : 0;
                    jsonObj["ProposalDetails"]["AddOnCovers"]["ElectricalAccessoriesIdv"] = model.CoverageDetails != null ? model.CoverageDetails.SIElectricalAccessories : 0;
                    jsonObj["ProposalDetails"]["AddOnCovers"]["LpgCngKitIdv"] = model.CoverageDetails != null ? model.CoverageDetails.BiFuelKitAmount : 0;
                    jsonObj["ProposalDetails"]["AddOnCovers"]["SelectedFuelType"] = externalCNG;
                    jsonObj["ProposalDetails"]["AddOnCovers"]["IsPAPaidDriver"] = Convert.ToString(model.CoverageDetails.IsPACoverPaidDriver).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO";
                    jsonObj["ProposalDetails"]["AddOnCovers"]["PAPaidDriverSumInsured"] = model.CoverageDetails.PACoverPaidDriverAmount;
                    jsonObj["ProposalDetails"]["AddOnCovers"]["IsPAUnnamedPassenger"] = Convert.ToString(model.CoverageDetails.IsPACoverUnnamedPerson).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO";
                    jsonObj["ProposalDetails"]["AddOnCovers"]["PAUnnamedPassengerNo"] = model.VehicleDetails != null ? model.VehicleDetails.SC : 0;
                    jsonObj["ProposalDetails"]["AddOnCovers"]["PAPerUnnamedPassengerSumInsured"] = model.CoverageDetails.PACoverUnnamedPersonAmount;
                    jsonObj["ProposalDetails"]["AddOnCovers"]["IsLegalLiabilityDriver"] = Convert.ToString(model.CoverageDetails.IsLegalLiablityPaidDriver).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO";
                    jsonObj["ProposalDetails"]["AddOnCovers"]["LLPaidDriverNo"] = model.CoverageDetails != null ? model.CoverageDetails.NoOfLLPaidDriver : 0;
                    jsonObj["ProposalDetails"]["AddOnCovers"]["IsLLEmployee"] = model.CoverageDetails != null ? Convert.ToString(model.CoverageDetails.IsEmployeeLiability).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                    jsonObj["ProposalDetails"]["AddOnCovers"]["LLEmployeeNo"] = 1;
                    jsonObj["ProposalDetails"]["AddOnCovers"]["CpaYear"] = CpaYear;
                }
                #endregion
            }
            #endregion

            #region CORPORATE AddOnCovers
            if (model.CustomerType == "CORPORATE" || model.CustomerType == "Corporate")
            {
                #region ODOnly
                if (PolicyType == "ODOnly")
                {
                    jsonObj["ProposalDetails"]["AddOnCovers"]["IsEngineAndGearboxProtectorCover"] = model.AddonCover != null ? Convert.ToString(model.AddonCover.IsEngineProtector).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                    jsonObj["ProposalDetails"]["AddOnCovers"]["IsZeroDepCover"] = model.AddonCover != null ? Convert.ToString(model.AddonCover.IsZeroDeperation).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                    jsonObj["ProposalDetails"]["AddOnCovers"]["IsLossOfUse"] = model.AddonCover != null ? Convert.ToString(model.AddonCover.IsLossofpersonalBelonging).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                    jsonObj["ProposalDetails"]["AddOnCovers"]["IsEmergencyAssistanceCover"] = model.AddonCover != null ? Convert.ToString(model.AddonCover.IsRoadSideAssistance).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                    jsonObj["ProposalDetails"]["AddOnCovers"]["IsNoClaimBonusProtection"] = model.AddonCover != null ? Convert.ToString(model.AddonCover.IsNCBProtection).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                    jsonObj["ProposalDetails"]["AddOnCovers"]["IsCostOfConsumable"] = model.AddonCover != null ? Convert.ToString(model.AddonCover.IsConsumables).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                    jsonObj["ProposalDetails"]["AddOnCovers"]["IsReturntoInvoice"] = model.AddonCover != null ? Convert.ToString(model.AddonCover.IsReturntoInvoice).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                    jsonObj["ProposalDetails"]["AddOnCovers"]["IsEmergencyAssistanceWiderCover"] = model.AddonCover != null ? Convert.ToString(model.AddonCover.IsLossofKey).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                    jsonObj["ProposalDetails"]["AddOnCovers"]["IsTyreSecureCover"] = model.AddonCover != null ? Convert.ToString(model.AddonCover.IsTyreCover).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                    jsonObj["ProposalDetails"]["AddOnCovers"]["IsZeroDepTakenLastYear"] = model.AddonCover != null ? !string.IsNullOrEmpty(model.AddonCover.NoOfCliamTaken) ? model.AddonCover.NoOfCliamTaken : "0" : "0";
                    jsonObj["ProposalDetails"]["AddOnCovers"]["NonelectricalAccessoriesIdv"] = model.CoverageDetails != null ? model.CoverageDetails.SINonElectricalAccessories : 0;
                    jsonObj["ProposalDetails"]["AddOnCovers"]["ElectricalAccessoriesIdv"] = model.CoverageDetails != null ? model.CoverageDetails.SIElectricalAccessories : 0;
                    jsonObj["ProposalDetails"]["AddOnCovers"]["LpgCngKitIdv"] = model.CoverageDetails != null ? model.CoverageDetails.BiFuelKitAmount : 0;
                    jsonObj["ProposalDetails"]["AddOnCovers"]["SelectedFuelType"] = externalCNG;
                }
                #endregion

                #region ThirdParty
                if (PolicyType == "ThirdParty")
                {
                    jsonObj["ProposalDetails"]["AddOnCovers"]["IsTPPDDiscount"] = model.DiscountDetails != null ? Convert.ToString(model.DiscountDetails.IsTPPDRestrictedto6000).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                    jsonObj["ProposalDetails"]["AddOnCovers"]["IsExLpgCngKit"] = model.CoverageDetails != null ? Convert.ToString(model.CoverageDetails.IsBiFuelKit).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                    jsonObj["ProposalDetails"]["AddOnCovers"]["IsPAPaidDriver"] = model.CoverageDetails != null ? Convert.ToString(model.CoverageDetails.IsPACoverPaidDriver).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                    jsonObj["ProposalDetails"]["AddOnCovers"]["PAPaidDriverSumInsured"] = model.CoverageDetails != null ? model.CoverageDetails.PACoverPaidDriverAmount : 0;
                    jsonObj["ProposalDetails"]["AddOnCovers"]["IsPAUnnamedPassenger"] = model.CoverageDetails != null ? Convert.ToString(model.CoverageDetails.IsPACoverUnnamedPerson).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                    jsonObj["ProposalDetails"]["AddOnCovers"]["PAUnnamedPassengerNo"] = model.VehicleDetails != null ? model.VehicleDetails.SC : 0;
                    jsonObj["ProposalDetails"]["AddOnCovers"]["PAPerUnnamedPassengerSumInsured"] = model.CoverageDetails != null ? model.CoverageDetails.PACoverUnnamedPersonAmount : 0;
                    jsonObj["ProposalDetails"]["AddOnCovers"]["IsLLEmployee"] = model.CoverageDetails != null ? Convert.ToString(model.CoverageDetails.IsEmployeeLiability).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                    jsonObj["ProposalDetails"]["AddOnCovers"]["LLEmployeeNo"] = 1;
                    jsonObj["ProposalDetails"]["AddOnCovers"]["IsLegalLiabilityDriver"] = Convert.ToString(model.CoverageDetails.IsLegalLiablityPaidDriver).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO";
                    jsonObj["ProposalDetails"]["AddOnCovers"]["LLPaidDriverNo"] = model.CoverageDetails != null ? model.CoverageDetails.NoOfLLPaidDriver : 0;
                }
                #endregion

                #region Comprehensive
                if (PolicyType == "Comprehensive")
                {
                    jsonObj["ProposalDetails"]["AddOnCovers"]["IsLossOfUse"] = model.AddonCover != null ? Convert.ToString(model.AddonCover.IsLossofpersonalBelonging).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                    jsonObj["ProposalDetails"]["AddOnCovers"]["IsEmergencyAssistanceCover"] = model.AddonCover != null ? Convert.ToString(model.AddonCover.IsRoadSideAssistance).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                    jsonObj["ProposalDetails"]["AddOnCovers"]["IsNoClaimBonusProtection"] = model.AddonCover != null ? Convert.ToString(model.AddonCover.IsNCBProtection).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                    jsonObj["ProposalDetails"]["AddOnCovers"]["IsEmergencyAssistanceWiderCover"] = model.AddonCover != null ? Convert.ToString(model.AddonCover.IsLossofKey).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                    jsonObj["ProposalDetails"]["AddOnCovers"]["IsCostOfConsumable"] = model.AddonCover != null ? Convert.ToString(model.AddonCover.IsConsumables).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                    jsonObj["ProposalDetails"]["AddOnCovers"]["IsReturntoInvoice"] = model.AddonCover != null ? Convert.ToString(model.AddonCover.IsReturntoInvoice).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                    jsonObj["ProposalDetails"]["AddOnCovers"]["IsTyreSecureCover"] = model.AddonCover != null ? Convert.ToString(model.AddonCover.IsTyreCover).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                    jsonObj["ProposalDetails"]["AddOnCovers"]["IsEngineAndGearboxProtectorCover"] = model.AddonCover != null ? Convert.ToString(model.AddonCover.IsEngineProtector).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                    jsonObj["ProposalDetails"]["AddOnCovers"]["IsZeroDepCover"] = model.AddonCover != null ? Convert.ToString(model.AddonCover.IsZeroDeperation).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                    jsonObj["ProposalDetails"]["AddOnCovers"]["IsTPPDDiscount"] = model.CoverageDetails != null ? Convert.ToString(model.DiscountDetails.IsTPPDRestrictedto6000).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                    jsonObj["ProposalDetails"]["AddOnCovers"]["IsExLpgCngKit"] = model.CoverageDetails != null ? Convert.ToString(model.CoverageDetails.IsBiFuelKit).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                    jsonObj["ProposalDetails"]["AddOnCovers"]["IsZeroDepTakenLastYear"] = model.AddonCover != null ? !string.IsNullOrEmpty(model.AddonCover.NoOfCliamTaken) ? model.AddonCover.NoOfCliamTaken : "0" : "0";
                    jsonObj["ProposalDetails"]["AddOnCovers"]["NonelectricalAccessoriesIdv"] = model.CoverageDetails != null ? model.CoverageDetails.SINonElectricalAccessories : 0;
                    jsonObj["ProposalDetails"]["AddOnCovers"]["ElectricalAccessoriesIdv"] = model.CoverageDetails != null ? model.CoverageDetails.SIElectricalAccessories : 0;
                    jsonObj["ProposalDetails"]["AddOnCovers"]["LpgCngKitIdv"] = model.CoverageDetails != null ? model.CoverageDetails.BiFuelKitAmount : 0;
                    jsonObj["ProposalDetails"]["AddOnCovers"]["SelectedFuelType"] = externalCNG;
                    jsonObj["ProposalDetails"]["AddOnCovers"]["IsPAPaidDriver"] = model.CoverageDetails != null ? Convert.ToString(model.CoverageDetails.IsPACoverPaidDriver).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                    jsonObj["ProposalDetails"]["AddOnCovers"]["PAPaidDriverSumInsured"] = model.CoverageDetails != null ? model.CoverageDetails.PACoverPaidDriverAmount : 0;
                    jsonObj["ProposalDetails"]["AddOnCovers"]["IsPAUnnamedPassenger"] = model.CoverageDetails != null ? Convert.ToString(model.CoverageDetails.IsPACoverUnnamedPerson).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                    jsonObj["ProposalDetails"]["AddOnCovers"]["PAUnnamedPassengerNo"] = model.VehicleDetails != null ? model.VehicleDetails.SC : 0;
                    jsonObj["ProposalDetails"]["AddOnCovers"]["PAPerUnnamedPassengerSumInsured"] = model.CoverageDetails != null ? model.CoverageDetails.PACoverUnnamedPersonAmount : 0;
                    jsonObj["ProposalDetails"]["AddOnCovers"]["IsLegalLiabilityDriver"] = model.CoverageDetails != null ? Convert.ToString(model.CoverageDetails.IsLegalLiablityPaidDriver).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                    jsonObj["ProposalDetails"]["AddOnCovers"]["LLPaidDriverNo"] = model.CoverageDetails != null ? model.CoverageDetails.NoOfLLPaidDriver : 0;
                    jsonObj["ProposalDetails"]["AddOnCovers"]["IsLLEmployee"] = model.CoverageDetails != null ? Convert.ToString(model.CoverageDetails.IsEmployeeLiability).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
                    jsonObj["ProposalDetails"]["AddOnCovers"]["LLEmployeeNo"] = 1;

                }
                #endregion
            }
            #endregion

            #region Proposal Details
            jsonObj["ProposalDetails"]["ChassisNo"] = model.VehicleDetails != null ? model.VehicleDetails.ChassisNumber : string.Empty;
            jsonObj["ProposalDetails"]["CustomerType"] = model.CustomerType != null ? model.CustomerType : "Individual";
            jsonObj["ProposalDetails"]["EngineNo"] = model.VehicleDetails != null ? model.VehicleDetails.EngineNumber : string.Empty;

            string financreode = "0";
            if (model.VehicleDetails.IsVehicleLoan == true)
            {
                financreode = model.VehicleDetails.LoanCompanyName;
            }

            int CrYear = DateTime.Now.Year;
            int nomineeYear = Convert.ToDateTime(model.NomineeDateOfBirth).Year;

            int PAOwnerDriverNomineeAge = CrYear - nomineeYear;

            jsonObj["ProposalDetails"]["FinancierCode"] = financreode;
            jsonObj["ProposalDetails"]["IsPreviousClaim"] = model.PreviousPolicyDetails != null ? Convert.ToString(model.PreviousPolicyDetails.IsPreviousInsuranceClaimed).Equals("true", StringComparison.OrdinalIgnoreCase) ? "YES" : "NO" : "NO";
            jsonObj["ProposalDetails"]["PAOwnerDriverNomineeAge"] = PAOwnerDriverNomineeAge;
            jsonObj["ProposalDetails"]["PAOwnerDriverNomineeName"] = model.NomineeName != null ? model.NomineeName : "XX";
            jsonObj["ProposalDetails"]["PAOwnerDriverNomineeRelationship"] = model.NomineeRelationShip != null ? model.NomineeRelationShip : "Mother";

            jsonObj["ProposalDetails"]["PolicyType"] = PolicyType.ToString();

            string registrationDate = Convert.ToDateTime(model.VehicleDetails.RegistrationDate).ToString("yyyy-MM-dd");
            int manufactureYear = Convert.ToDateTime(model.VehicleDetails.ManufaturingDate).Year;
            jsonObj["ProposalDetails"]["PurchaseRegistrationDate"] = registrationDate;

            //jsonObj["ProposalDetails"]["QuoteNo"] = model.CompanyWiseRefference.QuoteNo;

            string registrationno = "NEW";

            if (model.PolicyType != "NEW" && model.PolicyType != "New")
            {
                string rgno = model.VehicleDetails != null ? !string.IsNullOrEmpty(model.VehicleDetails.RegistrationNumber) ? model.VehicleDetails.RegistrationNumber : "MH02AB9685" : "MH02AB9685";
                int rglenght = rgno.Length - 4;
                string subregno = rgno.Substring(4, rglenght);
                string part1 = rgno.Substring(0, 2);
                string part2 = rgno.Substring(2, 2);
                string part3 = subregno.Substring(0, subregno.Length - 4);
                string part4 = subregno.Substring(subregno.Length - 4);
                registrationno = part1 + "-" + part2 + "-" + part3 + "-" + part4;
                model.VehicleDetails.RegistrationNumber = registrationno;
            }
            else
            {
                model.VehicleDetails.RegistrationNumber = registrationno;
            }
            jsonObj["ProposalDetails"]["RequiredIdv"] = model.IDV != 0 ? model.IDV : 0;
            jsonObj["ProposalDetails"]["TaxAmount"] = model.PremiumDetails != null ? model.PremiumDetails.TaxAmount : "0";
            jsonObj["ProposalDetails"]["NetPremiumAmount"] = model.PremiumDetails != null ? model.PremiumDetails.NetPremiumAmount : "0";
            jsonObj["ProposalDetails"]["TotalPremiumAmount"] = model.PremiumDetails != null ? model.PremiumDetails.TotalPremiumAmount : "0";
            jsonObj["ProposalDetails"]["TypeOfBusiness"] = model.PolicyType;
            jsonObj["ProposalDetails"]["VehicleMakeCode"] = model.VehicleDetails.MakeCode;
            jsonObj["ProposalDetails"]["VehicleModelCode"] = model.VehicleDetails.ModelCode;
            jsonObj["ProposalDetails"]["YearofManufacture"] = manufactureYear;
            jsonObj["ProposalDetails"]["RegistrationNo"] = registrationno;
            jsonObj["ProposalDetails"]["RtoLocationCode"] = RtoLocationCode;

            jsonObj["UniqueRequestID"] = model.CompanyWiseRefference != null ? model.CompanyWiseRefference.applicationId : string.Empty;


            string dateofbirth = Convert.ToDateTime(model.ClientDetails.DateOfBirth).ToString("yyyy-MM-dd");


            if (model.CustomerAddressDetails.IsRegistrationAddressSame == false)
            {
                jsonObj["CustomerDetails"]["CorrespondenceAddress1"] = model.VehicleAddressDetails != null ? model.VehicleAddressDetails.Address1 : string.Empty;
                jsonObj["CustomerDetails"]["CorrespondenceAddress2"] = model.VehicleAddressDetails != null ? model.VehicleAddressDetails.Address2 : string.Empty;
                jsonObj["CustomerDetails"]["CorrespondenceAddress3"] = model.VehicleAddressDetails != null ? model.VehicleAddressDetails.Address2 : string.Empty;
                jsonObj["CustomerDetails"]["CorrespondenceAddressCitycode"] = model.VehicleAddressDetails != null ? model.VehicleAddressDetails.Citycode : string.Empty;
                jsonObj["CustomerDetails"]["CorrespondenceAddressCityName"] = model.VehicleAddressDetails != null ? model.VehicleAddressDetails.City : string.Empty;
                jsonObj["CustomerDetails"]["CorrespondenceAddressStateCode"] = model.VehicleAddressDetails != null ? model.VehicleAddressDetails.State : string.Empty;
                jsonObj["CustomerDetails"]["CorrespondenceAddressStateName"] = model.VehicleAddressDetails != null ? model.VehicleAddressDetails.Statecode : string.Empty;
                jsonObj["CustomerDetails"]["CorrespondenceAddressPincode"] = model.CustomerAddressDetails != null ? model.VehicleAddressDetails.Pincode : string.Empty;
            }
            else
            {
                jsonObj["CustomerDetails"]["CorrespondenceAddress1"] = model.CustomerAddressDetails != null ? model.CustomerAddressDetails.Address1 : string.Empty;
                jsonObj["CustomerDetails"]["CorrespondenceAddress2"] = model.CustomerAddressDetails != null ? model.CustomerAddressDetails.Address2 : string.Empty;
                jsonObj["CustomerDetails"]["CorrespondenceAddress3"] = model.CustomerAddressDetails != null ? model.CustomerAddressDetails.Address2 : string.Empty;
                jsonObj["CustomerDetails"]["CorrespondenceAddressCitycode"] = model.CustomerAddressDetails != null ? model.CustomerAddressDetails.Citycode : string.Empty;
                jsonObj["CustomerDetails"]["CorrespondenceAddressCityName"] = model.CustomerAddressDetails != null ? model.CustomerAddressDetails.City : string.Empty;
                jsonObj["CustomerDetails"]["CorrespondenceAddressStateCode"] = model.CustomerAddressDetails != null ? model.CustomerAddressDetails.State : string.Empty;
                jsonObj["CustomerDetails"]["CorrespondenceAddressStateName"] = model.CustomerAddressDetails != null ? model.CustomerAddressDetails.Statecode : string.Empty;
                jsonObj["CustomerDetails"]["CorrespondenceAddressPincode"] = model.CustomerAddressDetails != null ? model.CustomerAddressDetails.Pincode : string.Empty;
            }

            jsonObj["CustomerDetails"]["DateOfBirth"] = dateofbirth;
            jsonObj["CustomerDetails"]["EmailAddress"] = model.ClientDetails != null ? ((string.IsNullOrEmpty(model.ClientDetails.EmailId)) ? string.Empty : model.ClientDetails.EmailId) : string.Empty;
            jsonObj["CustomerDetails"]["Title"] = model.ClientDetails != null ? ((string.IsNullOrEmpty(model.ClientDetails.Salutation)) ? string.Empty : model.ClientDetails.Salutation) : string.Empty;
            jsonObj["CustomerDetails"]["FirstName"] = model.ClientDetails != null ? ((string.IsNullOrEmpty(model.ClientDetails.FirstName)) ? string.Empty : model.ClientDetails.FirstName) : string.Empty;
            jsonObj["CustomerDetails"]["Gender"] = model.ClientDetails != null ? ((string.IsNullOrEmpty(model.ClientDetails.Gender)) ? "MALE" : model.ClientDetails.Gender) : "MALE";
            jsonObj["CustomerDetails"]["GstInNo"] = model.ClientDetails != null ? ((string.IsNullOrEmpty(model.ClientDetails.GSTIN)) ? string.Empty : model.ClientDetails.GSTIN) : string.Empty;
            jsonObj["CustomerDetails"]["LastName"] = model.ClientDetails != null ? ((string.IsNullOrEmpty(model.ClientDetails.LastName)) ? string.Empty : model.ClientDetails.LastName) : string.Empty;
            jsonObj["CustomerDetails"]["MobileNumber"] = model.ClientDetails != null ? ((string.IsNullOrEmpty(model.ClientDetails.MobileNo)) ? string.Empty : model.ClientDetails.MobileNo) : string.Empty;
            jsonObj["CustomerDetails"]["PanCard"] = model.ClientDetails != null ? ((string.IsNullOrEmpty(model.ClientDetails.PanCardNo)) ? string.Empty : model.ClientDetails.PanCardNo) : string.Empty;
            jsonObj["CustomerDetails"]["PospCode"] = "";
            jsonObj["CustomerDetails"]["UidNo"] = "";
            jsonObj["CustomerDetails"]["OrganizationContactPersonName"] = model.ClientDetails != null ? model.ClientDetails.FirstName : string.Empty;
            jsonObj["CustomerDetails"]["OrganizationName"] = model.OrganizationName != null ? model.OrganizationName : string.Empty;
            #endregion

            string requestjson = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
            dynamic responsePremCalc = Webservice(requestjson.ToString(), baseurl, proposalgenerateURL);
            if (responsePremCalc.StatusCode == HttpStatusCode.OK)
            {
                resModel = GetPeopoaslResponse(responsePremCalc, model, requestjson);
            }
            else
            {
                resModel.Status = Status.Fail;
                resModel.ErrorMsg = Convert.ToString(responsePremCalc.StatusCode);
                LogU.WriteLog("HDFC >> PrivateCar >> GetProposalRequest >> " + Convert.ToString(responsePremCalc.StatusCode));
            }
            return resModel;
        }

        public Response GetPeopoaslResponse(dynamic responsePremCalc, Quotation model, string requestjson)
        {
            Response resModel = new Response();
            try
            {
                dynamic Response = (JObject)JsonConvert.DeserializeObject(responsePremCalc.Content.ToString());
                string responseMessage = Convert.ToString(Response.Message);

                string prpstr = Convert.ToString(Response);

                #region Proposal RESPONSE
                if (string.IsNullOrEmpty(responseMessage))
                {
                    dynamic responseData = Response.Data;
                    dynamic responseStatus = Response.Status;
                    PremiumBreakUpDetails PremiumBreakUp = new PremiumBreakUpDetails();
                    CompanyWiseRefference comReff = new CompanyWiseRefference();
                    comReff.applicationId = Convert.ToString(Response.UniqueRequestID);
                    comReff.QuoteNo = Convert.ToString(responseData.QuoteNo);
                    comReff.OrderNo = Convert.ToString(responseData.TransactionNo);
                  //  comReff.IsBreakin = Convert.ToInt32(responseData.IsBreakin);

                    PremiumBreakUp.NetPremium = Convert.ToDouble(model.PremiumDetails.NetPremiumAmount);
                    PremiumBreakUp.ServiceTax = Convert.ToDouble(model.PremiumDetails.TaxAmount);

                    resModel.FinalPremium = Convert.ToDouble(model.PremiumDetails.TotalPremiumAmount);

                    resModel.PolicyStartDate = Convert.ToString(responseData.NewPolicyStartDate);
                    resModel.PolicyEndDate = Convert.ToString(responseData.NewPolicyEndDate);

                    resModel.TPStartDate = Convert.ToString(responseData.TPNewPolicyStartDate);
                    resModel.TPEndDate = Convert.ToString(responseData.TPNewPolicyEndDate);


                    resModel.PremiumBreakUpDetails = PremiumBreakUp;

                    resModel.CompanyWiseRefference = comReff;
                    resModel.Status = Status.Success;
                    resModel.Product = Product.Motor;
                    resModel.SubProduct = SubProduct.PrivateCar;
                    resModel.CompanyName = Company.HDFC.ToString();
                    resModel.ComName = Company.HDFC.ToString();

                    string externalCNG = model.VehicleDetails.Fuel;
                    if (model.CoverageDetails.IsBiFuelKit == true)
                    {
                        externalCNG = "CNG";
                    }

                    ap.SP_REQUEST_RESPONSE_API_MASTER(model.enquiryid, 8, requestjson, prpstr);

                    string customeraddress = model.CustomerAddressDetails.Address1 + " " + model.CustomerAddressDetails.Address2 + " " + model.CustomerAddressDetails.Address3 + " " + model.CustomerAddressDetails.Pincode;
                    DateTime? tpstartdate = null, tpenddate = null;
                    string producttype = "Comprehensive";

                    if (!model.IsODOnly)
                    {
                        tpstartdate = Convert.ToDateTime(responseData.TPNewPolicyStartDate);
                        tpenddate = Convert.ToDateTime(responseData.TPNewPolicyEndDate);
                    }

                    if (model.IsODOnly)
                    {
                        producttype = "ODOnly";
                    }

                    if (model.IsThirdPartyOnly)
                    {
                        producttype = "ThirdParty";
                    }
                    int ncbper = 0;
                    if (model.PolicyType.ToUpper() != "NEW")
                    {
                        ncbper = model.CurrentNcb;
                    }

                    resModel.PlanName = producttype;

                    ap.SP_POLICYDETAILSMASTER("I", model.enquiryid, 8, model.pospid, model.CustomerType, model.PolicyType, producttype,
                        null, model.ClientDetails.FirstName, model.ClientDetails.MiddleName, model.ClientDetails.LastName,
                        customeraddress, model.ClientDetails.PanCardNo, model.ClientDetails.GSTIN, model.ClientDetails.EmailId,
                        Convert.ToDateTime(model.ClientDetails.DateOfBirth), model.ClientDetails.MobileNo, model.VehicleDetails.RtoId, null,
                       Convert.ToInt64(model.VehicleDetails.MakeCode), Convert.ToInt64(model.VehicleDetails.ModelCode),
                       model.VehicleDetails.VariantId,
                        Convert.ToDateTime(responseData.NewPolicyStartDate), Convert.ToDateTime(model.VehicleDetails.RegistrationDate),
                        Convert.ToDateTime(model.VehicleDetails.ManufaturingDate),
                        model.VehicleDetails.SC, model.VehicleDetails.RegistrationNumber, model.VehicleDetails.CC,
                        model.VehicleDetails.EngineNumber, model.VehicleDetails.ChassisNumber, model.VehicleDetails.Fuel,
                        null, Convert.ToDateTime(responseData.NewPolicyStartDate), Convert.ToDateTime(responseData.NewPolicyEndDate),
                        tpstartdate, tpenddate,
                        1, ncbper, Convert.ToDecimal(model.PremiumDetails.ncbDiscAmount), null,
                        model.IDV, Convert.ToDecimal(model.PremiumDetails.AddonPremium),
                        Convert.ToDecimal(model.PremiumDetails.OdPremiumAmount),
                        Convert.ToDecimal(model.PremiumDetails.TpPremiumAmount),
                        Convert.ToDecimal(model.PremiumDetails.NetPremiumAmount),
                        Convert.ToDecimal(model.PremiumDetails.TaxAmount),
                        Convert.ToDecimal(model.PremiumDetails.TotalPremiumAmount), false);
                }
                else
                {
                    resModel.Status = Status.Fail;
                    resModel.ErrorMsg = Convert.ToString(responseMessage);
                    LogU.WriteLog("HDFC >> PrivateCar >> GetProposalRequest >> " + Convert.ToString(responseMessage));
                }
                #endregion
            }
            catch (Exception ex)
            {
                resModel.Status = Status.Fail;
                resModel.ErrorMsg = Convert.ToString(ex.Message);
                Console.Write(Convert.ToString(ex.Message));
                LogU.WriteLog("HDFC >> PrivateCar >> GetProposalRequest >> " + Convert.ToString(ex.Message));
                //throw;
            }

            return resModel;
        }
        #endregion

        public IRestResponse Webservice(string jsonString, string baseurl, string url)
        {
            RestClient clientPremCalc = new RestClient(baseurl);
            var requestPremCalc = new RestRequest(url, Method.POST);
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(delegate { return true; });
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;

            requestPremCalc.AddHeader("MerchantKey", merchantkey);
            requestPremCalc.AddHeader("SecretToken", Secretetoken);

            requestPremCalc.RequestFormat = DataFormat.Json;
            requestPremCalc.AddJsonBody(jsonString);
            var responsePremCalc = clientPremCalc.Execute(requestPremCalc);

            return responsePremCalc;
        }
        public string GetPreviousInsurerCompany(long Companyid, string type)
        {
            string companyid = "0";
            if (type == "prvins")
            {
                var GetPrvCompanyId = (from db in ap.PREVIOUS_INSURER_MAPPING
                                       where db.companyid == 8 && db.previouscompanyid == Companyid
                                       select db).FirstOrDefault();
                if (GetPrvCompanyId != null)
                {
                    companyid = GetPrvCompanyId.inscompanycode;
                }
            }
            if (type == "prvinstp")
            {
                var GetPrvCompanyId = (from db in ap.PREVIOUS_INSURER_MAPPING
                                       where db.companyid == 8 && db.previouscompanyid == Companyid
                                       select db).FirstOrDefault();
                if (GetPrvCompanyId != null)
                {
                    companyid = GetPrvCompanyId.inscompanycode;
                }
            }
            return companyid;
        }
    }
}