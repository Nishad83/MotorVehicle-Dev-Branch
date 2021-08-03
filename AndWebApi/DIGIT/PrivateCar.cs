namespace AndWebApi.DIGIT
{
    #region namespace
    using AndApp;
    using AndWebApi.Models;
    using AndApp.Utilities;
    using Controllers;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Web;
    using DAL;
    using System.Configuration;
    #endregion

    public class PrivateCar
    {
        DefaultController control = new DefaultController();

        public string GetEnquiryId(int length)
        {
            string result = string.Empty;
            Random random = new Random();
            try
            {
                var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                result = new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());

            }
            catch (Exception ex)
            {
                LogU.WriteLog("DIGIT >> PrivateCar >> GetEnquiryId >> " + Convert.ToString(ex.Message));
                throw;
            }
            return result;
        }
        public Response GetQuoteRequest(Quotation model)
        {
            Response resModel = new Response();
            try
            {
                if (model.IDV != 0)
                {
                    if (model.CustomIDV != null)
                    {
                        var getidvvalues = model.CustomIDV.Where(x => x.CompanyName == "DIGIT").FirstOrDefault();
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
                            else
                            {

                            }
                        }
                    }
                }
                if (model.PolicyType.Equals("New"))
                {
                    model.PolicyEndDate = Convert.ToDateTime(model.PolicyStartDate).AddYears(3).AddDays(-1).ToString("yyyy-MM-dd");

                }
                else
                {
                    model.PolicyEndDate = Convert.ToDateTime(model.PolicyStartDate).AddYears(1).AddDays(-1).ToString("yyyy-MM-dd");
                }
                string DIGIT_enquiryid = GetEnquiryId(50);
                string apiUrl = ConfigurationManager.AppSettings["DigitQuote"];
                var httpWebRequest = (HttpWebRequest)WebRequest.CreateHttp(apiUrl);

                //var httpWebRequest = (HttpWebRequest)WebRequest.CreateHttp("https://preprod-qnb.godigit.com/digit/motor-insurance/services/integration/v2/quickquote?isUserSpecialDiscountOpted=false");
                httpWebRequest.ContentType = "application/json ; charset=utf-8";

                httpWebRequest.Method = "POST";
                httpWebRequest.Headers["Authorization"] = "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes("84606359:digit123"));

                string path = AppDomain.CurrentDomain.BaseDirectory;
                string filePath = Path.Combine(path, "JSON/DIGIT/Quote.json");
                string json = File.ReadAllText(filePath);

                dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
                jsonObj["enquiryId"] = DIGIT_enquiryid;
                jsonObj["contract"]["insuranceProductCode"] = model.IsODOnly == true ? "20103" : model.IsThirdPartyOnly == true ? "20102" : "20101";
                jsonObj["contract"]["subInsuranceProductCode"] = model.PolicyType.Equals("New") ? "31" : "PB";           //PB for single year renewal
                jsonObj["contract"]["startDate"] = Convert.ToDateTime(model.PolicyStartDate).ToString("yyyy-MM-dd");
                jsonObj["contract"]["endDate"] = model.PolicyEndDate;
                jsonObj["contract"]["policyHolderType"] = (model.CustomerType.Equals("Individual") ? "INDIVIDUAL" : model.CustomerType);           //INDIVIUAL

                //Coverage Details
                jsonObj["contract"]["coverages"]["thirdPartyLiability"]["isTPPD"] = model.DiscountDetails.IsTPPDRestrictedto6000;
                jsonObj["contract"]["coverages"]["accessories"]["cng"]["selection"] = model.CoverageDetails.IsBiFuelKit;
                jsonObj["contract"]["coverages"]["accessories"]["electrical"]["selection"] = model.CoverageDetails.IsElectricalAccessories;
                jsonObj["contract"]["coverages"]["accessories"]["nonElectrical"]["selection"] = model.CoverageDetails.IsNonElectricalAccessories;
                jsonObj["contract"]["coverages"]["accessories"]["cng"]["insuredAmount"] = model.CoverageDetails.BiFuelKitAmount;
                jsonObj["contract"]["coverages"]["accessories"]["electrical"]["insuredAmount"] = model.CoverageDetails.SIElectricalAccessories;
                jsonObj["contract"]["coverages"]["accessories"]["nonElectrical"]["insuredAmount"] = model.CoverageDetails.SINonElectricalAccessories;
                jsonObj["contract"]["coverages"]["thirdPartyLiability"]["isTPPD"] = model.IsThirdPartyOnly;
                if (model.CoverageDetails.IsLegalLiablityPaidDriver == true)
                {
                    jsonObj["contract"]["coverages"]["legalLiability"]["paidDriverLL"]["selection"] = model.CoverageDetails.IsLegalLiablityPaidDriver;
                    jsonObj["contract"]["coverages"]["legalLiability"]["paidDriverLL"]["insuredCount"] = model.CoverageDetails.NoOfLLPaidDriver;
                }
                if (model.CoverageDetails.IsPACoverUnnamedPerson == true)
                {
                    jsonObj["contract"]["coverages"]["unnamedPA"]["unnamedPax"]["selection"] = model.CoverageDetails.IsPACoverUnnamedPerson;
                    jsonObj["contract"]["coverages"]["unnamedPA"]["unnamedPax"]["insuredAmount"] = model.CoverageDetails.NumberofPersonsUnnamed;
                }
                jsonObj["contract"]["coverages"]["legalLiability"]["employeesLL"]["selection"] = model.CoverageDetails.IsEmployeeLiability;
                jsonObj["contract"]["coverages"]["personalAccident"]["selection"] = model.CoverageDetails.IsPACoverForOwnerDriver;
                //addons
                if (model.AddonCover != null)
                {
                    jsonObj["contract"]["coverages"]["addons"]["partsDepreciation"]["selection"] = model.AddonCover.IsZeroDeperation;
                    jsonObj["contract"]["coverages"]["addons"]["engineProtection"]["selection"] = model.AddonCover.IsEngineProtector;
                    jsonObj["contract"]["coverages"]["addons"]["roadSideAssistance"]["selection"] = model.AddonCover.IsRoadSideAssistance;
                    jsonObj["contract"]["coverages"]["addons"]["tyreProtection"]["selection"] = model.AddonCover.IsTyreCover;
                    jsonObj["contract"]["coverages"]["addons"]["rimProtection"]["selection"] = model.AddonCover.IsRimProtectionCover;
                    jsonObj["contract"]["coverages"]["addons"]["returnToInvoice"]["selection"] = model.AddonCover.IsReturntoInvoice;
                    jsonObj["contract"]["coverages"]["addons"]["consumables"]["selection"] = model.AddonCover.IsConsumables;
                    jsonObj["contract"]["coverages"]["addons"]["keyAndLockProtect"]["selection"] = model.AddonCover.IsLossofKey;
                    jsonObj["contract"]["coverages"]["addons"]["personalBelonging"]["selection"] = model.AddonCover.IsLossofpersonalBelonging;
                }
                // vechicle details
                jsonObj["vehicle"]["isVehicleNew"] = model.PolicyType.Equals("New") ? true : false;
                jsonObj["vehicle"]["vehicleMaincode"] = model.VehicleDetails.VariantCode;
                jsonObj["vehicle"]["licensePlateNumber"] = (string.IsNullOrEmpty(model.VehicleDetails.RegistrationNumber) ? "GJ01" : model.VehicleDetails.RegistrationNumber);
                jsonObj["vehicle"]["vehicleIdentificationNumber"] = model.VehicleDetails.ChassisNumber;
                jsonObj["vehicle"]["engineNumber"] = model.VehicleDetails.EngineNumber;
                jsonObj["vehicle"]["manufactureDate"] = Convert.ToDateTime(model.VehicleDetails.ManufaturingDate).ToString("yyyy-MM-dd");
                jsonObj["vehicle"]["registrationDate"] = Convert.ToDateTime(model.VehicleDetails.RegistrationDate).ToString("yyyy-MM-dd");

                jsonObj["vehicle"]["vehicleIDV"]["idv"] = model.IDV.ToString();



                //previousInsurer
                if (model.PolicyType != "New" && model.DontKnowPreviousInsurer == false)
                {
                    jsonObj["previousInsurer"]["isPreviousInsurerKnown"] = !model.DontKnowPreviousInsurer;
                    jsonObj["previousInsurer"]["previousInsurerCode"] = GetDigitPrevCompany(model.PreviousPolicyDetails.CompanyId);
                    jsonObj["previousInsurer"]["previousPolicyNumber"] = model.PreviousPolicyDetails.PreviousPolicyNo;
                    jsonObj["previousInsurer"]["previousPolicyExpiryDate"] = Convert.ToDateTime(model.PreviousPolicyDetails.PreviousPolicyEndDate).ToString("yyyy-MM-dd");
                    if (model.PreviousPolicyDetails.PreviousPolicyType == "TP")
                        jsonObj["previousInsurer"]["isClaimInLastYear"] = true;
                    else
                        jsonObj["previousInsurer"]["isClaimInLastYear"] = model.PreviousPolicyDetails.IsPreviousInsuranceClaimed;
                    jsonObj["previousInsurer"]["originalPreviousPolicyType"] = model.PreviousPolicyDetails.PreviousPolicyType == "OD" ? "1OD_0TP" : model.PreviousPolicyDetails.PreviousPolicyType == "TP" ? "0OD_1TP" : "1OD_3TP";
                    if (model.PreviousPolicyDetails.IsPreviousInsuranceClaimed == false)
                    {
                        var ncb = Convert.ToInt16(model.PreviousPolicyDetails.PreviousNcbPercentage);
                        jsonObj["previousInsurer"]["previousNoClaimBonus"] = ncb == 0 ? "ZERO" : ncb == 20 ? "TWENTY" : ncb == 25 ? "TWENTY_FIVE" : ncb == 35 ? "THIRTY_FIVE" : ncb == 45 ? "FORTY_FIVE" : "FIFTY";
                    }
                    if (model.IsODOnly)
                    {
                        // jsonObj["previousInsurer"]["currentThirdPartyPolicy"]["isCurrentThirdPartyPolicyActive"] = ;
                        jsonObj["previousInsurer"]["currentThirdPartyPolicy"]["currentThirdPartyPolicyInsurerCode"] = model.PreviousTPPolicyDetails != null ? model.PreviousTPPolicyDetails.CompanyId.ToString() : null;
                        jsonObj["previousInsurer"]["currentThirdPartyPolicy"]["currentThirdPartyPolicyNumber"] = model.PreviousTPPolicyDetails != null ? model.PreviousTPPolicyDetails.PolicyNo.ToString() : null;
                        jsonObj["previousInsurer"]["currentThirdPartyPolicy"]["currentThirdPartyPolicyStartDateTime"] = model.PreviousTPPolicyDetails != null ? Convert.ToDateTime(model.PreviousTPPolicyDetails.PolicyEndDate).AddYears(-3).ToString("yyyy-MM-dd") : null;
                        jsonObj["previousInsurer"]["currentThirdPartyPolicy"]["currentThirdPartyPolicyExpiryDateTime"] = model.PreviousTPPolicyDetails != null ? Convert.ToDateTime(model.PreviousTPPolicyDetails.PolicyEndDate).ToString("yyyy-MM-dd") : null;
                    }
                    // jsonObj["previousInsurer"]["currentThirdPartyPolicy"] = model.PreviousPolicyDetails.;
                }
                if (model.PolicyType != "New" && model.DontKnowPreviousInsurer == true)
                {
                    jsonObj["previousInsurer"]["isPreviousInsurerKnown"] = !model.DontKnowPreviousInsurer;
                    jsonObj["motorQuestions"]["selfInspection"] = true;
                }

                jsonObj["pospInfo"]["isPOSP"] = false;
                jsonObj["pincode"] = "380054";
                string requestjson = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(requestjson);
                    streamWriter.Flush();
                    streamWriter.Close();
                }
                //File.WriteAllText("D:\\Trupti\\And\\AndApp\\AndWebApi\\JSON\\ICICI\\Quote.json", output);
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                StreamReader reader = new StreamReader(httpResponse.GetResponseStream());
                var result = reader.ReadToEnd();
                var policytype = model.IsODOnly == true ? "OD" : model.IsThirdPartyOnly ? "TP" : "Comp";
                resModel = GetQuoteResponse(result, policytype);
                if (model.IsThirdPartyOnly)
                {
                    resModel.PlanName = "Third Party Plan";
                }
                else if (model.IsODOnly)
                {
                    resModel.PlanName = "Own Damage Plan";
                }
                else
                {
                    resModel.PlanName = "Comprehesive Plan";
                }
                resModel.PolicyStartDate = model.PolicyStartDate;
                resModel.PolicyEndDate = model.PolicyEndDate;
            }
            catch (Exception ex)
            {
                resModel.Status = Status.Fail;
                resModel.ErrorMsg = ex.ToString();
                // throw;
            }
            return resModel;

        }
        public Response GetQuoteResponse(string res, string policytype)
        {
            Response resModel = new Response();
            PremiumBreakUpDetails premiumdata = new PremiumBreakUpDetails();
            CompanyWiseRefference compreff = new CompanyWiseRefference();

            try
            {
                resModel.Status = Status.Success;
                var data = (JObject)JsonConvert.DeserializeObject(res);
                resModel.EnquiryId = control.GenerateEnquiryId();
                compreff.QuoteNo = data["enquiryId"].Value<string>();
                resModel.CompanyWiseRefference = compreff;
                resModel.PolicyStartDate = data["contract"]["startDate"].Value<string>();
                resModel.PolicyEndDate = data["contract"]["endDate"].Value<string>();

                //resModel.FinalPremium = Math.Round(Convert.ToDouble(data["grossPremium"].Value<string>().Substring(4)), 0);
                //premiumdata.ServiceTax = Convert.ToDouble(data["serviceTax"]["totalTax"].Value<string>().Substring(4));

                if (policytype != "TP")
                {
                    //var withzerodep = Convert.ToDouble(data["contract"]["coverages"]["ownDamage"]["withZeroDepNetPremium"].Value<string>().Substring(4));
                    //var withoutzerodep = Convert.ToDouble(data["contract"]["coverages"]["ownDamage"]["withoutZeroDepNetPremium"].Value<string>().Substring(4));
                    //var netodzerodep = withzerodep - withoutzerodep;
                    if (data["contract"]["coverages"]["addons"]["partsDepreciation"]["selection"].Value<string>() == "True")
                    {
                        premiumdata.ZeroDepPremium = Math.Round(Convert.ToDouble(data["contract"]["coverages"]["addons"]["partsDepreciation"]["netPremium"].Value<string>().Substring(4)), 0);
                        //premiumdata.ZeroDepPremium = Math.Round((premiumdata.ZeroDepPremium + netodzerodep), 0);
                        var discouuntwithzerodep = data["contract"]["coverages"]["ownDamage"]["discount"]["discountsWithZeroDep"];
                        if (discouuntwithzerodep[0]["discountType"].Value<string>() == "NCB_DISCOUNT")
                            premiumdata.NCBDiscount = Math.Round(Convert.ToDouble(discouuntwithzerodep[0]["discountAmount"].Value<string>().Substring(4)), 0);
                    }
                    else
                    {
                        var discountarray = data["contract"]["coverages"]["ownDamage"]["discount"]["discountsWithoutZeroDep"];
                        if (discountarray[0]["discountType"].Value<string>() == "NCB_DISCOUNT")
                            premiumdata.NCBDiscount = Math.Round(Convert.ToDouble(discountarray[0]["discountAmount"].Value<string>().Substring(4)), 0);
                    }
                    if (data["contract"]["coverages"]["addons"]["engineProtection"]["selection"].Value<string>() == "True")
                        premiumdata.EngineProtectorPremium = Math.Round(Convert.ToDouble(data["contract"]["coverages"]["addons"]["engineProtection"]["netPremium"].Value<string>().Substring(4)), 0);
                    if (data["contract"]["coverages"]["addons"]["rimProtection"]["selection"].Value<string>() == "True")
                        premiumdata.RimProtectionPremium = Math.Round(Convert.ToDouble(data["contract"]["coverages"]["addons"]["rimProtection"]["netPremium"].Value<string>().Substring(4)), 0);
                    if (data["contract"]["coverages"]["addons"]["returnToInvoice"]["selection"].Value<string>() == "True")
                        premiumdata.InvoicePriceCoverPremium = Math.Round(Convert.ToDouble(data["contract"]["coverages"]["addons"]["returnToInvoice"]["netPremium"].Value<string>().Substring(4)), 0);
                    if (data["contract"]["coverages"]["addons"]["tyreProtection"]["selection"].Value<string>() == "True")
                        premiumdata.TyreProtect = Math.Round(Convert.ToDouble(data["contract"]["coverages"]["addons"]["tyreProtection"]["netPremium"].Value<string>().Substring(4)), 0);
                    if (data["contract"]["coverages"]["addons"]["roadSideAssistance"]["selection"].Value<string>() == "True")
                        premiumdata.RSAPremium = Math.Round(Convert.ToDouble(data["contract"]["coverages"]["addons"]["roadSideAssistance"]["netPremium"].Value<string>().Substring(4)), 0);
                    if (data["contract"]["coverages"]["addons"]["consumables"]["selection"].Value<string>() == "True")
                        premiumdata.CostOfConsumablesPremium = Math.Round(Convert.ToDouble(data["contract"]["coverages"]["addons"]["consumables"]["netPremium"].Value<string>().Substring(4)), 0);
                    if (data["contract"]["coverages"]["addons"]["personalBelonging"]["selection"].Value<string>() == "True")
                        premiumdata.LossOfPersonalBelongingPremium = Math.Round(Convert.ToDouble(data["contract"]["coverages"]["addons"]["personalBelonging"]["netPremium"].Value<string>().Substring(4)), 0);
                    if (data["contract"]["coverages"]["addons"]["keyAndLockProtect"]["selection"].Value<string>() == "True")
                        premiumdata.KeyReplacementPremium = Math.Round(Convert.ToDouble(data["contract"]["coverages"]["addons"]["keyAndLockProtect"]["netPremium"].Value<string>().Substring(4)), 0);
                    // var discountarray = data["contract"]["coverages"]["ownDamage"]["discount"]["discountsWithoutZeroDep"];
                    // if (discountarray[0]["discountType"].Value<string>() == "NCB_DISCOUNT")
                    //     premiumdata.NCBDiscount = Math.Round(Convert.ToDouble(discountarray[0]["discountAmount"].Value<string>().Substring(4)), 0);
                    // var discouuntwithzerodep = data["contract"]["coverages"]["ownDamage"]["discount"]["discountsWithZeroDep"];
                    //if (discouuntwithzerodep[0]["discountType"].Value<string>() == "NCB_DISCOUNT")
                    //  premiumdata.LoadingDiscount = Math.Round(Convert.ToDouble(discouuntwithzerodep[0]["discountAmount"].Value<string>().Substring(4)), 0);
                    premiumdata.BasicODPremium = Math.Round(Convert.ToDouble(data["contract"]["coverages"]["ownDamage"]["netPremium"].Value<string>().Substring(4)), 0);
                    premiumdata.NetAddonPremium = Math.Round((premiumdata.ZeroDepPremium + premiumdata.EngineProtectorPremium + premiumdata.RimProtectionPremium + premiumdata.InvoicePriceCoverPremium + premiumdata.TyreProtect + premiumdata.RSAPremium + premiumdata.CostOfConsumablesPremium + premiumdata.LossOfPersonalBelongingPremium + premiumdata.KeyReplacementPremium), 0);

                }
                if (policytype != "OD")
                {
                    if (data["contract"]["coverages"]["personalAccident"]["selection"].Value<string>() == "True")
                        premiumdata.PACoverToOwnDriver = Math.Round(Convert.ToDouble(data["contract"]["coverages"]["personalAccident"]["netPremium"].Value<string>().Substring(4)), 0);
                    if (data["contract"]["coverages"]["legalLiability"]["employeesLL"]["selection"].Value<string>() == "True")
                        premiumdata.LLToPaidEmployee = Math.Round(Convert.ToDouble(data["contract"]["coverages"]["legalLiability"]["employeesLL"]["netPremium"].Value<string>().Substring(4)), 0);
                    if (data["contract"]["coverages"]["legalLiability"]["paidDriverLL"]["selection"].Value<string>() == "True")
                        premiumdata.LLToPaidDriver = Math.Round(Convert.ToDouble(data["contract"]["coverages"]["legalLiability"]["paidDriverLL"]["netPremium"].Value<string>().Substring(4)), 0);
                    if (data["contract"]["coverages"]["legalLiability"]["unnamedPaxLL"]["selection"].Value<string>() == "True")
                        premiumdata.LLTounnamedPax = Math.Round(Convert.ToDouble(data["contract"]["coverages"]["legalLiability"]["unnamedPaxLL"]["netPremium"].Value<string>().Substring(4)), 0);
                    if (data["contract"]["coverages"]["unnamedPA"]["unnamedPax"]["selection"].Value<string>() == "True")
                        premiumdata.PACoverToUnNamedPerson = Math.Round(Convert.ToDouble(data["contract"]["coverages"]["unnamedPA"]["unnamedPax"]["netPremium"].Value<string>().Substring(4)), 0);
                    premiumdata.BasicThirdPartyLiability = Math.Round(Convert.ToDouble(data["contract"]["coverages"]["thirdPartyLiability"]["netPremium"].Value<string>().Substring(4)), 0);
                    var tpamount = premiumdata.LLToPaidEmployee + premiumdata.LLToPaidDriver + premiumdata.LLTounnamedPax + premiumdata.PACoverToOwnDriver;
                    premiumdata.NetTPPremium = premiumdata.BasicThirdPartyLiability + tpamount;

                }
                premiumdata.ElecAccessoriesPremium = 0;
                premiumdata.NonElecAccessoriesPremium = 0;
                premiumdata.CNGLPGKitPremium = 0;

                // var ncbdiff = premiumdata.LoadingDiscount - premiumdata.NCBDiscount;
                var netpremium = Convert.ToDouble(data["netPremium"].Value<string>().Substring(4));
                //  premiumdata.NetPremium = Math.Round(netpremium - premiumdata.NetAddonPremium - premiumdata.LLToPaidEmployee + ncbdiff, 0);
                premiumdata.NetPremium = Math.Round(netpremium, 0);
                premiumdata.ServiceTax = Math.Round(Convert.ToDouble(premiumdata.NetPremium * .18), 0);
                resModel.FinalPremium = Math.Round(Convert.ToDouble(data["grossPremium"].Value<string>().Substring(4)), 0);


                resModel.PremiumBreakUpDetails = premiumdata;
                //  premiumdata.NetDiscount = Convert.ToDouble(data["contract"]["coverages"]["discount"]["netPremium"].Value<string>().Substring(4));
                resModel.IDV = Convert.ToInt32(data["vehicle"]["vehicleIDV"]["idv"].Value<string>());
                resModel.MaxIDV = Convert.ToInt32(data["vehicle"]["vehicleIDV"]["maximumIdv"].Value<string>());
                resModel.MinIDV = Convert.ToInt32(data["vehicle"]["vehicleIDV"]["minimumIdv"].Value<string>());
                resModel.Product = Product.Motor;
                resModel.SubProduct = SubProduct.PrivateCar;
                resModel.CompanyName = Company.DIGIT.ToString();
            }
            catch (Exception ex)
            {
                resModel.Status = Status.Fail;
                resModel.ErrorMsg = ex.ToString();
                throw;
            }
            return resModel;
        }
        public Response GetProposalRequest(Quotation model)
        {
            Response resModel = new Response();

            try
            {
                string DIGIT_enquiryid = GetEnquiryId(50);
                string apiUrl = ConfigurationManager.AppSettings["DigitProposal"];
                var httpWebRequest = (HttpWebRequest)WebRequest.CreateHttp(apiUrl);

                //var httpWebRequest = (HttpWebRequest)WebRequest.CreateHttp("https://preprod-qnb.godigit.com/digit/motor-insurance/services/integration/v2/quote?isUserSpecialDiscountOpted=false");
                httpWebRequest.ContentType = "application/json ; charset=utf-8";

                httpWebRequest.Method = "POST";
                httpWebRequest.Headers["Authorization"] = "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes("84606359:digit123"));

                string path = AppDomain.CurrentDomain.BaseDirectory;
                string filePath = Path.Combine(path, "JSON/DIGIT/Quote.json");
                string json = File.ReadAllText(filePath);
                dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
                jsonObj["enquiryId"] = DIGIT_enquiryid;
                jsonObj["contract"]["insuranceProductCode"] = model.IsODOnly == true ? "20103" : model.IsThirdPartyOnly == true ? "20102" : "20101";
                jsonObj["contract"]["subInsuranceProductCode"] = model.PolicyType.Equals("New") ? "31" : "PB";           //PB for single year renewal
                jsonObj["contract"]["startDate"] = Convert.ToDateTime(model.PolicyStartDate).ToString("yyyy-MM-dd");
                jsonObj["contract"]["endDate"] = Convert.ToDateTime(model.PolicyEndDate).ToString("yyyy-MM-dd");
                jsonObj["contract"]["policyHolderType"] = (model.CustomerType.Equals("Individual") ? "INDIVIDUAL" : model.CustomerType);           //INDIVIUAL

                //Coverage Details
                jsonObj["contract"]["coverages"]["thirdPartyLiability"]["isTPPD"] = model.DiscountDetails.IsTPPDRestrictedto6000;
                jsonObj["contract"]["coverages"]["accessories"]["cng"]["selection"] = model.CoverageDetails.IsBiFuelKit;
                jsonObj["contract"]["coverages"]["accessories"]["electrical"]["selection"] = model.CoverageDetails.IsElectricalAccessories;
                jsonObj["contract"]["coverages"]["accessories"]["nonElectrical"]["selection"] = model.CoverageDetails.IsNonElectricalAccessories;
                jsonObj["contract"]["coverages"]["accessories"]["cng"]["insuredAmount"] = model.CoverageDetails.BiFuelKitAmount;
                jsonObj["contract"]["coverages"]["accessories"]["electrical"]["insuredAmount"] = model.CoverageDetails.SIElectricalAccessories;
                jsonObj["contract"]["coverages"]["accessories"]["nonElectrical"]["insuredAmount"] = model.CoverageDetails.SINonElectricalAccessories;
                jsonObj["contract"]["coverages"]["thirdPartyLiability"]["isTPPD"] = model.IsThirdPartyOnly;
                if (model.CoverageDetails.IsLegalLiablityPaidDriver == true)
                {
                    jsonObj["contract"]["coverages"]["legalLiability"]["paidDriverLL"]["selection"] = model.CoverageDetails.IsLegalLiablityPaidDriver;
                    jsonObj["contract"]["coverages"]["legalLiability"]["paidDriverLL"]["insuredCount"] = model.CoverageDetails.NoOfLLPaidDriver;
                }
                if (model.CoverageDetails.IsPACoverUnnamedPerson == true)
                {
                    jsonObj["contract"]["coverages"]["unnamedPA"]["unnamedPax"]["selection"] = model.CoverageDetails.IsPACoverUnnamedPerson;
                    jsonObj["contract"]["coverages"]["unnamedPA"]["unnamedPax"]["insuredAmount"] = model.CoverageDetails.NumberofPersonsUnnamed;
                }
                jsonObj["contract"]["coverages"]["legalLiability"]["employeesLL"]["selection"] = model.CoverageDetails.IsEmployeeLiability;
                jsonObj["contract"]["coverages"]["personalAccident"]["selection"] = model.CoverageDetails.IsPACoverForOwnerDriver;

                //addons
                if (model.AddonCover != null)
                {
                    jsonObj["contract"]["coverages"]["addons"]["partsDepreciation"]["selection"] = model.AddonCover.IsZeroDeperation;
                    jsonObj["contract"]["coverages"]["addons"]["engineProtection"]["selection"] = model.AddonCover.IsEngineProtector;
                    jsonObj["contract"]["coverages"]["addons"]["roadSideAssistance"]["selection"] = model.AddonCover.IsRoadSideAssistance;
                    jsonObj["contract"]["coverages"]["addons"]["tyreProtection"]["selection"] = model.AddonCover.IsTyreCover;
                    jsonObj["contract"]["coverages"]["addons"]["rimProtection"]["selection"] = model.AddonCover.IsRimProtectionCover;
                    jsonObj["contract"]["coverages"]["addons"]["returnToInvoice"]["selection"] = model.AddonCover.IsReturntoInvoice;
                    jsonObj["contract"]["coverages"]["addons"]["consumables"]["selection"] = model.AddonCover.IsConsumables;
                    jsonObj["contract"]["coverages"]["addons"]["keyAndLockProtect"]["selection"] = model.AddonCover.IsLossofKey;
                    jsonObj["contract"]["coverages"]["addons"]["personalBelonging"]["selection"] = model.AddonCover.IsLossofpersonalBelonging;
                }
                // vechicle details
                jsonObj["vehicle"]["isVehicleNew"] = model.PolicyType.Equals("New") ? true : false;
                jsonObj["vehicle"]["vehicleMaincode"] = model.VehicleDetails.VariantCode;
                jsonObj["vehicle"]["licensePlateNumber"] = (string.IsNullOrEmpty(model.VehicleDetails.RegistrationNumber) ? "GJ01" : model.VehicleDetails.RegistrationNumber);
                jsonObj["vehicle"]["vehicleIdentificationNumber"] = model.VehicleDetails.ChassisNumber;
                jsonObj["vehicle"]["engineNumber"] = model.VehicleDetails.EngineNumber;
                jsonObj["vehicle"]["manufactureDate"] = Convert.ToDateTime(model.VehicleDetails.ManufaturingDate).ToString("yyyy-MM-dd");
                jsonObj["vehicle"]["registrationDate"] = Convert.ToDateTime(model.VehicleDetails.RegistrationDate).ToString("yyyy-MM-dd");

                jsonObj["vehicle"]["vehicleIDV"]["idv"] = model.IDV.ToString();

                //previousInsurer
                if (model.PolicyType != "New" && model.DontKnowPreviousInsurer == false)
                {
                    jsonObj["previousInsurer"]["isPreviousInsurerKnown"] = !model.DontKnowPreviousInsurer;
                    jsonObj["previousInsurer"]["previousInsurerCode"] = GetDigitPrevCompany(model.PreviousPolicyDetails.CompanyId);
                    jsonObj["previousInsurer"]["previousPolicyNumber"] = model.PreviousPolicyDetails.PreviousPolicyNo;
                    jsonObj["previousInsurer"]["previousPolicyExpiryDate"] = Convert.ToDateTime(model.PreviousPolicyDetails.PreviousPolicyEndDate).ToString("yyyy-MM-dd");
                    jsonObj["previousInsurer"]["isClaimInLastYear"] = model.PreviousPolicyDetails.IsPreviousInsuranceClaimed;
                    jsonObj["previousInsurer"]["originalPreviousPolicyType"] = model.PreviousPolicyDetails.PreviousPolicyType == "OD" ? "1OD_0TP" : model.PreviousPolicyDetails.PreviousPolicyType == "TP" ? "0OD_1TP" : "1OD_3TP";
                    if (model.PreviousPolicyDetails.IsPreviousInsuranceClaimed == false)
                    {
                        var ncb = Convert.ToInt16(model.PreviousPolicyDetails.PreviousNcbPercentage);
                        jsonObj["previousInsurer"]["previousNoClaimBonus"] = ncb == 0 ? "ZERO" : ncb == 20 ? "TWENTY" : ncb == 25 ? "TWENTY_FIVE" : ncb == 35 ? "THIRTY_FIVE" : ncb == 45 ? "FORTY_FIVE" : "FIFTY";
                    }
                    if (model.IsODOnly)
                    {
                        // jsonObj["previousInsurer"]["currentThirdPartyPolicy"]["isCurrentThirdPartyPolicyActive"] = ;
                        jsonObj["previousInsurer"]["currentThirdPartyPolicy"]["currentThirdPartyPolicyInsurerCode"] = model.PreviousTPPolicyDetails != null ? GetDigitPrevCompany(model.PreviousTPPolicyDetails.CompanyId) : null;
                        jsonObj["previousInsurer"]["currentThirdPartyPolicy"]["currentThirdPartyPolicyNumber"] = model.PreviousTPPolicyDetails != null ? model.PreviousTPPolicyDetails.PolicyNo.ToString() : null;
                        jsonObj["previousInsurer"]["currentThirdPartyPolicy"]["currentThirdPartyPolicyStartDateTime"] = model.PreviousTPPolicyDetails != null ? Convert.ToDateTime(model.PreviousTPPolicyDetails.PolicyEndDate).AddYears(-3).ToString("yyyy-MM-dd") : null;
                        jsonObj["previousInsurer"]["currentThirdPartyPolicy"]["currentThirdPartyPolicyExpiryDateTime"] = model.PreviousTPPolicyDetails != null ? Convert.ToDateTime(model.PreviousTPPolicyDetails.PolicyEndDate).ToString("yyyy-MM-dd") : null;
                    }
                    // jsonObj["previousInsurer"]["currentThirdPartyPolicy"] = model.PreviousPolicyDetails.;
                }
                if (model.PolicyType != "New" && model.DontKnowPreviousInsurer == true)
                {
                    jsonObj["previousInsurer"]["isPreviousInsurerKnown"] = !model.DontKnowPreviousInsurer;
                    jsonObj["motorQuestions"]["selfInspection"] = true;
                    jsonObj["motorBreakIn"]["isBreakin"] = true;
                }
                jsonObj["pospInfo"]["isPOSP"] = false;

                if (model.RequestType.ToString() == "Proposal")
                {//address
                    jsonObj["persons"][0]["personType"] = model.CustomerType.Equals("Individual") ? "INDIVIDUAL" : model.CustomerType;
                    jsonObj["persons"][0]["addresses"][0]["flatNumber"] = (model.CustomerAddressDetails.Address2.Length > 5) ? model.CustomerAddressDetails.Address2.Substring(0, 5) : model.CustomerAddressDetails.Address2;
                    jsonObj["persons"][0]["addresses"][0]["streetNumber"] = model.CustomerAddressDetails.Address1.Length > 10 ? model.CustomerAddressDetails.Address1.Substring(0, 10) : model.CustomerAddressDetails.Address1;
                    jsonObj["persons"][0]["addresses"][0]["street"] = model.CustomerAddressDetails.Address1;
                    jsonObj["persons"][0]["addresses"][0]["district"] = model.CustomerAddressDetails.Address3;
                    jsonObj["persons"][0]["addresses"][0]["state"] = "27";// static model.CustomerAddressDetails.State;
                    jsonObj["persons"][0]["addresses"][0]["city"] = model.CustomerAddressDetails.City;
                    jsonObj["persons"][0]["addresses"][0]["pincode"] = model.CustomerAddressDetails.Pincode;
                    //communication
                    jsonObj["persons"][0]["communications"][0]["communicationType"] = "MOBILE";
                    jsonObj["persons"][0]["communications"][0]["communicationId"] = model.ClientDetails.MobileNo;
                    jsonObj["persons"][0]["communications"][1]["communicationType"] = "EMAIL";

                    jsonObj["persons"][0]["communications"][1]["communicationId"] = model.ClientDetails.EmailId;
                    //clientDetail
                    jsonObj["persons"][0]["firstName"] = model.ClientDetails.FirstName;
                    jsonObj["persons"][0]["middleName"] = model.ClientDetails.MiddleName;
                    jsonObj["persons"][0]["lastName"] = model.ClientDetails.LastName;
                    jsonObj["persons"][0]["dateOfBirth"] = Convert.ToDateTime(model.ClientDetails.DateOfBirth).ToString("yyyy-MM-dd");
                    var a = Convert.ToDateTime(model.ClientDetails.DateOfBirth).ToString("yyyy-MM-dd");
                    jsonObj["persons"][0]["gender"] = (model.ClientDetails.Gender.Equals("Male") ? "MALE" : "FEMALE");
                    //nominee detail
                    var nomname = model.NomineeName.Split(' ').Length;
                    jsonObj["nominee"]["firstName"] = nomname > 1 ? model.NomineeName.Split(' ').First() : model.NomineeName;//.Split(' ').First();
                    jsonObj["nominee"]["middleName"] = nomname > 2 ? model.NomineeName.Split(' ')[1] : "";// model.NomineeName.Split(' ')[1];
                    jsonObj["nominee"]["lastName"] = nomname > 1 ? model.NomineeName.Split(' ').Last() : model.NomineeName;// model.NomineeName.Split(' ').Last();
                    jsonObj["nominee"]["dateOfBirth"] = Convert.ToDateTime(model.NomineeDateOfBirth).ToString("yyyy-MM-dd");
                    jsonObj["nominee"]["relation"] = model.NomineeRelationShip;

                }
                string requestjson = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(requestjson);
                    streamWriter.Flush();
                    streamWriter.Close();
                }
                //File.WriteAllText("D:\\Trupti\\And\\AndApp\\AndWebApi\\JSON\\ICICI\\Quote.json", output);
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                StreamReader reader = new StreamReader(httpResponse.GetResponseStream());
                var result = reader.ReadToEnd();
                var policytype = model.IsODOnly == true ? "OD" : model.IsThirdPartyOnly ? "TP" : "Comp";
                resModel = GetProposalResponse(result, policytype);

                if (!resModel.Status.Equals("Fail"))
                {
                    if (model.IsThirdPartyOnly)
                    {
                        resModel.IDV = 0;
                    }

                }
                else
                {
                    resModel.IDV = 0;
                }

                if (model.IsThirdPartyOnly)
                {
                    resModel.PlanName = "Third Party Plan";
                }
                else if (model.IsODOnly)
                {
                    resModel.PlanName = "Own Damage Plan";
                }
                else
                {
                    resModel.PlanName = "Comprehesive Plan";
                }
                resModel.PolicyStartDate = model.PolicyStartDate;
                resModel.PolicyEndDate = model.PolicyEndDate;
                ANDAPPEntities entity = new ANDAPPEntities();
                var data = (JObject)JsonConvert.DeserializeObject(result);


                entity.SP_REQUEST_RESPONSE_API_MASTER(model.enquiryid, 6, requestjson, Convert.ToString(data));
                entity.SP_Payment_Parameter(model.enquiryid, 6, "applicationId", resModel.CompanyWiseRefference.applicationId);
                entity.SP_Payment_Parameter(model.enquiryid, 6, "policyno", resModel.PolicyNo);

                string customeraddress = model.CustomerAddressDetails.Address1 + " " + model.CustomerAddressDetails.Address2 + " " + model.CustomerAddressDetails.Address3 + " " + model.CustomerAddressDetails.Pincode;
                DateTime? tpstartdate = null, tpenddate = null;
                string producttype = "Comprehensive";


                //if (!model.IsODOnly)
                //{
                //    tpstartdate = Convert.ToDateTime(responseData.TPNewPolicyStartDate);
                //    tpenddate = Convert.ToDateTime(responseData.TPNewPolicyEndDate);
                //}

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
                entity.SP_POLICYDETAILSMASTER("I", model.enquiryid, 6, model.pospid, model.CustomerType, model.PolicyType, producttype,
                    null, model.ClientDetails.FirstName, model.ClientDetails.MiddleName, model.ClientDetails.LastName,
                    customeraddress, model.ClientDetails.PanCardNo, model.ClientDetails.GSTIN, model.ClientDetails.EmailId,
                    Convert.ToDateTime(model.ClientDetails.DateOfBirth), model.ClientDetails.MobileNo, model.VehicleDetails.RtoId, null,
                   Convert.ToInt64(model.VehicleDetails.MakeCode), Convert.ToInt64(model.VehicleDetails.ModelCode),
                   model.VehicleDetails.VariantId,
                    Convert.ToDateTime(resModel.PolicyStartDate), Convert.ToDateTime(model.VehicleDetails.RegistrationDate),
                    Convert.ToDateTime(model.VehicleDetails.ManufaturingDate),
                    model.VehicleDetails.SC, model.VehicleDetails.RegistrationNumber, model.VehicleDetails.CC,
                    model.VehicleDetails.EngineNumber, model.VehicleDetails.ChassisNumber, model.VehicleDetails.Fuel,
                    null, Convert.ToDateTime(resModel.PolicyStartDate), Convert.ToDateTime(resModel.PolicyEndDate),
                    tpstartdate, tpenddate,
                    1, ncbper, Convert.ToDecimal(model.PremiumDetails.ncbDiscAmount), null,
                    model.IDV, Convert.ToDecimal(model.PremiumDetails.AddonPremium),
                    Convert.ToDecimal(model.PremiumDetails.OdPremiumAmount),
                    Convert.ToDecimal(model.PremiumDetails.TpPremiumAmount),
                    Convert.ToDecimal(resModel.PremiumBreakUpDetails.NetPremium),
                    Convert.ToDecimal(resModel.PremiumBreakUpDetails.ServiceTax),
                    Convert.ToDecimal(resModel.FinalPremium), false);
                //entity.SP_POLICYDETAILSMASTER("I", model.enquiryid, 6, model.pospid, model.CustomerType, model.PolicyType,
                //         model.ClientDetails.FirstName, model.ClientDetails.MiddleName, model.ClientDetails.LastName, model.ClientDetails.EmailId,
                //         Convert.ToDateTime(model.ClientDetails.DateOfBirth), customeraddress, model.ClientDetails.PanCardNo,
                //         model.ClientDetails.GSTIN, model.ClientDetails.MobileNo, Convert.ToInt64(model.VehicleDetails.MakeCode),
                //         Convert.ToInt64(model.VehicleDetails.ModelCode), model.VehicleDetails.VariantId, model.VehicleDetails.RegistrationNumber,
                //         model.VehicleDetails.EngineNumber, model.VehicleDetails.ChassisNumber, resModel.PolicyNo, Convert.ToDateTime(resModel.PolicyStartDate),
                //         Convert.ToDateTime(resModel.PolicyEndDate), tpstartdate, tpenddate, 1,
                //         Convert.ToDecimal(model.IDV), Convert.ToDecimal(model.PremiumDetails.OdPremiumAmount),
                //        Convert.ToDecimal(model.PremiumDetails.TpPremiumAmount),
                //        Convert.ToDecimal(resModel.PremiumBreakUpDetails.NetPremium),
                //        Convert.ToDecimal(resModel.PremiumBreakUpDetails.ServiceTax),
                //         Convert.ToDecimal(resModel.FinalPremium), false);
            }

            catch (WebException ex)
            {
                resModel.Status = Status.Fail;
                using (WebResponse response = ex.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)response;
                    Console.WriteLine("Error code: {0}", httpResponse.StatusCode);
                    using (Stream data = response.GetResponseStream())
                    using (var reader = new StreamReader(data))
                    {
                        string text = reader.ReadToEnd();
                        var datas = (JObject)JsonConvert.DeserializeObject(text);
                        var ab = datas["error"]["validationMessages"].First();
                        string errormessage = datas["error"]["validationMessages"].First().Value<string>();
                        resModel.ErrorMsg = errormessage;
                        LogU.WriteLog("DIGIT >> PrivateCar >> GetProposalRequest >> " + Convert.ToString(errormessage));
                    }
                }
            }

            return resModel;
        }
        public Response GetProposalResponse(string res, string policytype)
        {
            Response resModel = new Response();
            try
            {
                resModel.Status = Status.Success;
                var data = (JObject)JsonConvert.DeserializeObject(res);
                string applicationid = data["applicationId"].Value<string>();
                resModel.EnquiryId = control.GenerateEnquiryId();
                resModel.PolicyStartDate = data["contract"]["startDate"].Value<string>();
                resModel.PolicyEndDate = data["contract"]["endDate"].Value<string>();
                resModel.PolicyNo = data["policyNumber"].Value<string>();

                resModel.FinalPremium = Math.Round(Convert.ToDouble(data["grossPremium"].Value<string>().Substring(4)), 0);
                var idv = data["vehicle"]["vehicleIDV"]["idv"].Value<string>();

                PremiumBreakUpDetails premiumdata = new PremiumBreakUpDetails();
                premiumdata.ServiceTax = Convert.ToDouble(data["serviceTax"]["totalTax"].Value<string>().Substring(4));
                if (policytype != "TP")
                {
                    if (data["contract"]["coverages"]["addons"]["partsDepreciation"]["selection"].Value<string>() == "True")
                    {
                        premiumdata.ZeroDepPremium = Convert.ToDouble(data["contract"]["coverages"]["addons"]["partsDepreciation"]["netPremium"].Value<string>().Substring(4));
                        var discouuntwithzerodep = data["contract"]["coverages"]["ownDamage"]["discount"]["discountsWithZeroDep"];
                        if (discouuntwithzerodep.First != null)
                            if (discouuntwithzerodep[0]["discountType"].Value<string>() == "NCB_DISCOUNT")
                                premiumdata.NCBDiscount = Convert.ToDouble(discouuntwithzerodep[0]["discountAmount"].Value<string>().Substring(4));
                    }
                    else
                    {
                        var discountarray = data["contract"]["coverages"]["ownDamage"]["discount"]["discountsWithoutZeroDep"];
                        if (discountarray.First != null)
                            if (discountarray[0]["discountType"].Value<string>() == "NCB_DISCOUNT")
                                premiumdata.NCBDiscount = Convert.ToDouble(discountarray[0]["discountAmount"].Value<string>().Substring(4));
                    }
                    if (data["contract"]["coverages"]["addons"]["engineProtection"]["selection"].Value<string>() == "True")
                        premiumdata.EngineProtectorPremium = Convert.ToDouble(data["contract"]["coverages"]["addons"]["engineProtection"]["netPremium"].Value<string>().Substring(4));
                    if (data["contract"]["coverages"]["addons"]["rimProtection"]["selection"].Value<string>() == "True")
                        premiumdata.RimProtectionPremium = Convert.ToDouble(data["contract"]["coverages"]["addons"]["rimProtection"]["netPremium"].Value<string>().Substring(4));
                    if (data["contract"]["coverages"]["addons"]["returnToInvoice"]["selection"].Value<string>() == "True")
                        premiumdata.InvoicePriceCoverPremium = Convert.ToDouble(data["contract"]["coverages"]["addons"]["returnToInvoice"]["netPremium"].Value<string>().Substring(4));
                    if (data["contract"]["coverages"]["addons"]["tyreProtection"]["selection"].Value<string>() == "True")
                        premiumdata.TyreProtect = Convert.ToDouble(data["contract"]["coverages"]["addons"]["tyreProtection"]["netPremium"].Value<string>().Substring(4));
                    if (data["contract"]["coverages"]["addons"]["roadSideAssistance"]["selection"].Value<string>() == "True")
                        premiumdata.RSAPremium = Convert.ToDouble(data["contract"]["coverages"]["addons"]["roadSideAssistance"]["netPremium"].Value<string>().Substring(4));
                    if (data["contract"]["coverages"]["addons"]["consumables"]["selection"].Value<string>() == "True")
                        premiumdata.CostOfConsumablesPremium = Convert.ToDouble(data["contract"]["coverages"]["addons"]["consumables"]["netPremium"].Value<string>().Substring(4));
                    if (data["contract"]["coverages"]["addons"]["personalBelonging"]["selection"].Value<string>() == "True")
                        premiumdata.LossOfPersonalBelongingPremium = Convert.ToDouble(data["contract"]["coverages"]["addons"]["personalBelonging"]["netPremium"].Value<string>().Substring(4));
                    if (data["contract"]["coverages"]["addons"]["keyAndLockProtect"]["selection"].Value<string>() == "True")
                        premiumdata.KeyReplacementPremium = Convert.ToDouble(data["contract"]["coverages"]["addons"]["keyAndLockProtect"]["netPremium"].Value<string>().Substring(4));
                    premiumdata.BasicODPremium = Convert.ToDouble(data["contract"]["coverages"]["ownDamage"]["netPremium"].Value<string>().Substring(4));
                    premiumdata.NetAddonPremium = Math.Round((premiumdata.ZeroDepPremium + premiumdata.EngineProtectorPremium + premiumdata.RimProtectionPremium + premiumdata.InvoicePriceCoverPremium + premiumdata.TyreProtect + premiumdata.RSAPremium + premiumdata.CostOfConsumablesPremium + premiumdata.LossOfPersonalBelongingPremium + premiumdata.KeyReplacementPremium), 2);
                    premiumdata.NetODPremium = premiumdata.BasicODPremium;
                }
                if (policytype != "OD")
                {
                    if (data["contract"]["coverages"]["personalAccident"]["selection"].Value<string>() == "True")
                        premiumdata.PACoverToOwnDriver = Convert.ToDouble(data["contract"]["coverages"]["personalAccident"]["netPremium"].Value<string>().Substring(4));
                    if (data["contract"]["coverages"]["legalLiability"]["employeesLL"]["selection"].Value<string>() == "True")
                        premiumdata.LLToPaidEmployee = Convert.ToDouble(data["contract"]["coverages"]["legalLiability"]["employeesLL"]["netPremium"].Value<string>().Substring(4));
                    if (data["contract"]["coverages"]["legalLiability"]["paidDriverLL"]["selection"].Value<string>() == "True")
                        premiumdata.LLToPaidDriver = Convert.ToDouble(data["contract"]["coverages"]["legalLiability"]["paidDriverLL"]["netPremium"].Value<string>().Substring(4));
                    if (data["contract"]["coverages"]["legalLiability"]["unnamedPaxLL"]["selection"].Value<string>() == "True")
                        premiumdata.LLTounnamedPax = Convert.ToDouble(data["contract"]["coverages"]["legalLiability"]["unnamedPaxLL"]["netPremium"].Value<string>().Substring(4));
                    if (data["contract"]["coverages"]["unnamedPA"]["unnamedPax"]["selection"].Value<string>() == "True")
                        premiumdata.PACoverToUnNamedPerson = Convert.ToDouble(data["contract"]["coverages"]["unnamedPA"]["unnamedPax"]["netPremium"].Value<string>().Substring(4));
                    premiumdata.BasicThirdPartyLiability = Convert.ToDouble(data["contract"]["coverages"]["thirdPartyLiability"]["netPremium"].Value<string>().Substring(4));
                    var tpamount = premiumdata.LLToPaidEmployee + premiumdata.LLToPaidDriver + premiumdata.LLTounnamedPax + premiumdata.PACoverToOwnDriver;
                    premiumdata.NetTPPremium = premiumdata.BasicThirdPartyLiability + tpamount;

                }
                premiumdata.NetPremium = Math.Round(Convert.ToDouble(data["netPremium"].Value<string>().Substring(4)), 0);
                resModel.PremiumBreakUpDetails = premiumdata;

                resModel.Product = Product.Motor;
                resModel.SubProduct = SubProduct.PrivateCar;
                resModel.CompanyName = Company.DIGIT.ToString();
                CompanyWiseRefference companydata = new CompanyWiseRefference();
                companydata.applicationId = applicationid;
                companydata.QuoteNo = data["policyNumber"].Value<string>();
                resModel.CompanyWiseRefference = companydata;
            }
            catch (Exception ex)
            {
                resModel.Status = Status.Fail;
                resModel.ErrorMsg = ex.Message.ToString();
                Console.Write(Convert.ToString(ex.Message));
                LogU.WriteLog("DIGIT >> PrivateCar >> GetProposalRequest >> " + Convert.ToString(ex.Message));
                throw;
            }
            return resModel;
        }
        public string GetPaymentParameter(PaymentRequest model)
        {
            string result;
            try
            {
                string apiUrl = ConfigurationManager.AppSettings["DigitPayment"];
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                //var httpWebRequest = (HttpWebRequest)WebRequest.CreateHttp(" https://preprod-digitpaymentgateway.godigit.com/DigitPaymentGateway/rest/insertPaymentOnline/EB/Motor");
                var httpWebRequest = (HttpWebRequest)WebRequest.CreateHttp(apiUrl);
                httpWebRequest.ContentType = "application/json ; charset=utf-8";

                httpWebRequest.Method = "POST";
                httpWebRequest.Headers["Authorization"] = ("E0EV6I53I54YK6LMH1AO3C7C8NHPSCO8");

                string path = AppDomain.CurrentDomain.BaseDirectory;
                string filePath = Path.Combine(path, "JSON/DIGIT/Payment.json");
                string json = File.ReadAllText(filePath);
                dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
                jsonObj["applicationId"] = model.CompanyDetail.applicationId;
                jsonObj["successReturnUrl"] = ConfigurationManager.AppSettings["DIGITPaymentSuccess"] + model.CompanyDetail.applicationId + "&&policyno=" + model.CompanyDetail.QuoteNo;
                jsonObj["cancelReturnUrl"] = ConfigurationManager.AppSettings["DIGITPaymentFail"];
                string requestjson = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(requestjson);
                    streamWriter.Flush();
                    streamWriter.Close();
                }
                //File.WriteAllText("D:\\Trupti\\And\\AndApp\\AndWebApi\\JSON\\ICICI\\Quote.json", output);
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                StreamReader reader = new StreamReader(httpResponse.GetResponseStream());
                result = reader.ReadToEnd();
            }
            catch (Exception ex)
            {

                result = "200";
                throw;
            }
            return result;
        }
        public string GeneratePolicyPDF(string applicationid)
        {
            string pdfpath;
            string result;
            try
            {
                string apiUrl = ConfigurationManager.AppSettings["DigitPDF"];

                //var httpWebRequest = (HttpWebRequest)WebRequest.CreateHttp(" https://preprod-pdfgeneration.godigit.com/PDFGeneration/rest/digit/generatePolicy");
                var httpWebRequest = (HttpWebRequest)WebRequest.CreateHttp(apiUrl);

                httpWebRequest.ContentType = "application/json ; charset=utf-8";

                httpWebRequest.Method = "POST";
                httpWebRequest.Headers["Authorization"] = ("E0EV6I53I54YK6LMH1AO3C7C8NHPSCO8");
                string requestjson = "{\"policyId\":\"" + applicationid + "\"}";
                //   string requestjson = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(requestjson);
                    streamWriter.Flush();
                    streamWriter.Close();
                }
                //File.WriteAllText("D:\\Trupti\\And\\AndApp\\AndWebApi\\JSON\\ICICI\\Quote.json", output);
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                StreamReader reader = new StreamReader(httpResponse.GetResponseStream());
                result = reader.ReadToEnd();
                var data = (JObject)JsonConvert.DeserializeObject(result);
                pdfpath = data["schedulePath"].Value<string>();

            }
            catch (Exception ex)
            {
                result = ex.ToString();
                throw;
            }
            return pdfpath;
        }
        public string GetDigitPrevCompany(int compid)
        {
            ANDAPPEntities entity = new ANDAPPEntities();
            var companycode = entity.PREVIOUS_INSURER_MAPPING.Where(x => x.companyid == 6 && x.previouscompanyid == 12).FirstOrDefault();
            if (companycode != null)
                return companycode.inscompanycode.ToString();
            else
                return "ONA";             // ONA for other companies from master
        }
        public string GetPolicyStatus(string Policyno)
        {
            var result = "";
            var response = "";
            try
            {

                string apiUrl = ConfigurationManager.AppSettings["DigitSearchPolicy"] + Policyno;

                var httpWebRequest = (HttpWebRequest)WebRequest.CreateHttp(apiUrl);

                httpWebRequest.ContentType = "application/json ; charset=utf-8";

                httpWebRequest.Method = "GET";
                httpWebRequest.Headers["Authorization"] = "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes("84606359:digit123"));

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                StreamReader reader = new StreamReader(httpResponse.GetResponseStream());
                result = reader.ReadToEnd();
                var data = (JObject)JsonConvert.DeserializeObject(result);
                var policystatus = data["policyStatus"].Value<string>();
                var policystate = data["policyState"].Value<string>();
                response = "PolicyStatus=" + policystatus + ",policystate=" + policystate;
            }
            catch (Exception ex)
            {

                result = ex.ToString();
                throw;
            }
            return response;
        }

    }
}