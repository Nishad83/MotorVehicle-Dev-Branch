using AndApp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Script.Serialization;
using AndWebApi.Controllers;
using AndWebApi.Models;

namespace AndWebApi.KOTAK
{

    public class PrivateCar
    {
        DefaultController control = new DefaultController();
        public Response GetQuoteRequest(Quotation model)
        {
            Response resModel = new Response();
            CustomIDV idvvalues = new CustomIDV();
            try
            {
                if (model.IDV != 0)
                {
                    if (model.CustomIDV != null)
                    {
                        var getidvvalues = model.CustomIDV.Where(x => x.CompanyName == "KOTAK").FirstOrDefault();
                        idvvalues = getidvvalues;
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
                string strUri = "https://kgipass.kotakmahindrageneralinsurance.co.in/KGIGenericPremiumCalcService/kgiservice.svc/CalculatePremiumPrivateCarGeneric"; //PROD LINK, PLS MAKE LIMITED TESTING
                Uri uri = new Uri(strUri);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                //WebRequest request = WebRequest.Create(uri);
                request.Method = "POST";
                request.ContentType = "application/json; charset=utf-8";

                string path = AppDomain.CurrentDomain.BaseDirectory;
                string filePath = Path.Combine(path, "JSON/KOTAK/Quote.json");
                string json = File.ReadAllText(filePath);
                dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
                var a = Convert.ToDateTime(model.VehicleDetails.RegistrationDate).ToString("dd'/'MM'/'yyyy");
                jsonObj["strBusinessType"] = model.PolicyType.Equals("New") ? "New Business" : "Rollover";
                jsonObj["TotalClaimCount"] = (model.PolicyType.Equals("New") ? "0" : "No Claim");                //discuss
                jsonObj["strRTOCode"] = (string.IsNullOrEmpty(model.VehicleDetails.RegistrationNumber) ? "GJ01" : model.VehicleDetails.RegistrationNumber);
                jsonObj["strVehicleMake"] = model.VehicleDetails.MakeName;
                jsonObj["strVehicleMakeCode"] = Convert.ToInt16(model.VehicleDetails.MakeCode);
                jsonObj["strVehicleModel"] = model.VehicleDetails.ModelName;
                jsonObj["strVehicleModelCode"] = Convert.ToInt16(model.VehicleDetails.ModelCode);
                jsonObj["strVehicleVariant"] = model.VehicleDetails.VariantName;
                jsonObj["strVehicleVariantCode"] = Convert.ToInt16(model.VehicleDetails.VariantCode);
                jsonObj["strDateofRegistration"] = Convert.ToDateTime(model.VehicleDetails.RegistrationDate).ToString("dd'/'MM'/'yyyy");
                jsonObj["strIDV"] = model.IDV.ToString();
                if (model.AddonCover != null)
                {
                    jsonObj["strReturnToInvoice"] = model.AddonCover.IsReturntoInvoice.Equals(true) ? "True" : "False";
                    jsonObj["strEngineProtect"] = model.AddonCover.IsEngineProtector.Equals(true) ? "True" : "False";
                    jsonObj["strRoadsideAssistance"] = model.AddonCover.IsRoadSideAssistance.Equals(true) ? "True" : "False";
                    jsonObj["strDepreciationCover"] = model.AddonCover.IsZeroDeperation.Equals(true) ? "True" : "False";
                    jsonObj["strConsumableCover"] = model.AddonCover.IsConsumables.Equals(true) ? "True" : "False";
                }
                jsonObj["strPreviousYearNCBPercentage"] = ((model.PreviousPolicyDetails != null) ? model.PreviousPolicyDetails.PreviousNcbPercentage : "0");
                jsonObj["strNEAR"] = model.CoverageDetails.IsNonElectricalAccessories.Equals(true) ? "True" : "False";
                jsonObj["strEAR"] = model.CoverageDetails.IsElectricalAccessories.Equals(true) ? "True" : "False";
                jsonObj["strCNG"] = model.CoverageDetails.IsBiFuelKit.Equals(true) ? "True" : "False";
                jsonObj["strNEARSumInsured"] = model.CoverageDetails.SINonElectricalAccessories;
                jsonObj["strEARSumInsured"] = model.CoverageDetails.SIElectricalAccessories;
                jsonObj["strCNGSumInsured"] = model.CoverageDetails.BiFuelKitAmount;
                jsonObj["TypeOfPolicyHolder"] = (model.CustomerType == "Individual" ? "Individual Owner" : model.CustomerType);
                jsonObj["CustomerType"] = model.CustomerType;
                jsonObj["PreviousPolicyExpiryDate"] = (model.PreviousPolicyDetails != null) ? model.PreviousPolicyDetails.PreviousPolicyEndDate : "";
                jsonObj["PreviousPolicyTypeOrTypeofCover"] = (model.PolicyType.Equals("NEW") ? "" : "ComprehensivePolicy");
                jsonObj["VoluntaryDeductibleAmount"] = model.DiscountDetails.VoluntaryExcessAmount;
                jsonObj["IsPACoverForUnnamedPersons"] = model.CoverageDetails.IsPACoverUnnamedPerson.Equals(true) ? "True" : "False";
                jsonObj["NumberofPersonsUnnamed"] = model.VehicleDetails.SC;
                jsonObj["CapitalSumInsuredPerPersonUnnamed"] = model.CoverageDetails.PACoverUnnamedPersonAmount;
                jsonObj["IsPACoverForPaidDriver"] = model.CoverageDetails.IsPACoverPaidDriver.Equals(true) ? "True" : "False";
                jsonObj["SumInsuredForPaidDriver"] = model.CoverageDetails.PACoverPaidDriverAmount;
                // jsonObj["NumberofPaidDrivers"] = model.CoverageDetails.NoOfLLPaidDriver;    // bydefault 1
                jsonObj["IsPACoverForNamedPersons"] = model.CoverageDetails.IsPACoverForNamedPersons.Equals(true) ? "True" : "False";
                jsonObj["NumberofPersonsNamed"] = model.CoverageDetails.NumberofPersonsNamed;
                jsonObj["CapitalSumInsuredPerPersonNamed"] = model.CoverageDetails.CapitalSumInsuredPerPersonNamed;
                jsonObj["WiderLegalLiabilityToPaidDriverCleanerConductorIMT28"] = model.CoverageDetails.IsLegalLiablityPaidDriver.Equals(true) ? "True" : "False";
                jsonObj["NoOfPersonWiderLegalLiability"] = model.CoverageDetails.NoOfLLPaidDriver;





                //string requestjson = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
                //using (StreamWriter streamWriter = new StreamWriter(request.GetRequestStream()))
                //{
                //    streamWriter.Write(requestjson);
                //    streamWriter.Flush();
                //    streamWriter.Close();
                //}
                //var httpResponse = (HttpWebResponse)request.GetResponse();
                //StreamReader sReader = new StreamReader(httpResponse.GetResponseStream());
                //string outResult = sReader.ReadToEnd();
                //sReader.Close();
                //if (httpResponse.StatusCode.ToString() == "OK")
                //{
                //    resModel = GetQuoteResponse(outResult, model.PolicyType, model.IDV, idvvalues);
                //    resModel.Status = Status.Success;
                //}
                //else
                //{
                //    resModel.Status = Status.Fail;
                //}
                // resModel.Status =
            }
            catch (Exception ex)
            {
                resModel.Status = Status.Fail;
                resModel.ErrorMsg = ex.ToString();
                // throw;
            }
            return resModel;
        }
        public Response GetQuoteResponse(string strresponse, string policytype, int idv, CustomIDV idvvalue)
        {
            Response resModel = new Response();
            PremiumBreakUpDetails premiumdata = new PremiumBreakUpDetails();
            CompanyWiseRefference compref = new CompanyWiseRefference();

            try
            {
                var data = (JObject)JsonConvert.DeserializeObject(strresponse);
                premiumdata.BasicThirdPartyLiability = Convert.ToDouble(data["BasicTPPremium"].Value<string>());
                premiumdata.CostOfConsumablesPremium = Convert.ToDouble(data["ConsumableCover"].Value<string>());
                premiumdata.ZeroDepPremium = Convert.ToDouble(!string.IsNullOrEmpty(data["DepreciationCover"].Value<string>()) ? data["DepreciationCover"].Value<string>() : "0");
                premiumdata.EngineProtectorPremium = Convert.ToDouble(!string.IsNullOrEmpty(data["EngineProtect"].Value<string>()) ? data["EngineProtect"].Value<string>() : "0");
                premiumdata.KeyReplacementPremium = Convert.ToDouble(!string.IsNullOrEmpty(data["KeyReplacement"].Value<string>()) ? data["KeyReplacement"].Value<string>() : "0");
                premiumdata.LLToPaidDriver = Convert.ToDouble(!string.IsNullOrEmpty(data["LegalLiabilityToPaidDriverNo"].Value<string>()) ? data["LegalLiabilityToPaidDriverNo"].Value<string>() : "0");
                premiumdata.LLToPaidEmployee = Convert.ToDouble(!string.IsNullOrEmpty(data["LLEOPDCC"].Value<string>()) ? data["LLEOPDCC"].Value<string>() : "0");
                premiumdata.LossOfPersonalBelongingPremium = Convert.ToDouble(!string.IsNullOrEmpty(data["LossofPersonalBelongings"].Value<string>()) ? data["LossofPersonalBelongings"].Value<string>() : "0");
                premiumdata.NCBDiscount = Convert.ToDouble(!string.IsNullOrEmpty(data["NCB"].Value<string>()) ? data["NCB"].Value<string>() : "0");
                premiumdata.NcbProtectorPremium = Convert.ToDouble(!string.IsNullOrEmpty(data["NCBProtect"].Value<string>()) ? data["NCBProtect"].Value<string>() : "0");
                premiumdata.NonElecAccessoriesPremium = Convert.ToDouble(!string.IsNullOrEmpty(data["NonElectronicSI"].Value<string>()) ? data["NonElectronicSI"].Value<string>() : "0");
                premiumdata.BasicODPremium = Math.Round(Convert.ToDouble(data["OwnDamagePremium"].Value<string>()), 0);
                premiumdata.PACoverToOwnDriver = Convert.ToDouble(!string.IsNullOrEmpty(data["PACoverForOwnerDriver"].Value<string>()) ? data["PACoverForOwnerDriver"].Value<string>() : "0");
                premiumdata.InvoicePriceCoverPremium = Convert.ToDouble(!string.IsNullOrEmpty(data["ReturnToInvoice"].Value<string>()) ? data["ReturnToInvoice"].Value<string>() : "0");
                premiumdata.ServiceTax = Convert.ToDouble(data["ServiceTax"].Value<string>());
                premiumdata.RSAPremium = Convert.ToDouble(!string.IsNullOrEmpty(data["RSA"].Value<string>()) ? data["RSA"].Value<string>() : "0");
                premiumdata.ElecAccessoriesPremium = Convert.ToDouble(!string.IsNullOrEmpty(data["ElectronicSI"].Value<string>()) ? data["ElectronicSI"].Value<string>() : "0");
                premiumdata.CNGLPGKitPremium = Convert.ToDouble(!string.IsNullOrEmpty(data["ExternalBiFuelSI"].Value<string>()) ? data["ExternalBiFuelSI"].Value<string>() : "0");
                premiumdata.TPCNGLPGPremium = Convert.ToDouble(!string.IsNullOrEmpty(data["LiabilityForBiFuel"].Value<string>()) ? data["LiabilityForBiFuel"].Value<string>() : "0");
                premiumdata.PACoverToUnNamedPerson = Convert.ToDouble(!string.IsNullOrEmpty(data["PAForUnnamedPassengerSI"].Value<string>()) ? data["PAForUnnamedPassengerSI"].Value<string>() : "0");
                premiumdata.PACoverToNamedPerson = Convert.ToDouble(!string.IsNullOrEmpty(data["PAForNamedPassengerSI"].Value<string>()) ? data["PAForNamedPassengerSI"].Value<string>() : "0");
                premiumdata.PAToPaidDriver = Convert.ToDouble(!string.IsNullOrEmpty(data["PAToPaidDriverSI"].Value<string>()) ? data["PAToPaidDriverSI"].Value<string>() : "0");
                var addon = premiumdata.CostOfConsumablesPremium + premiumdata.EngineProtectorPremium + premiumdata.KeyReplacementPremium + premiumdata.LossOfPersonalBelongingPremium + premiumdata.InvoicePriceCoverPremium + premiumdata.RSAPremium + premiumdata.ZeroDepPremium;
                premiumdata.NetAddonPremium = Convert.ToDouble(addon);

                premiumdata.NetODPremium = premiumdata.BasicODPremium + premiumdata.ElecAccessoriesPremium + premiumdata.NonElecAccessoriesPremium + premiumdata.CNGLPGKitPremium;


                premiumdata.NetTPPremium = Convert.ToDouble(data["TotalPremiumLiability"].Value<string>());
                var netpremium = Convert.ToDouble(data["NetPremium"].Value<string>());
                premiumdata.NetPremium = Math.Round(netpremium - Convert.ToDouble(addon) - premiumdata.LLToPaidEmployee, 0);
                resModel.SC = (int)Convert.ToDouble(data["SeatingCapacity"].Value<string>());
                resModel.FinalPremium = (int)Convert.ToDouble(data["TotalPremium"].Value<string>());
                resModel.EnquiryId = data["QuoteNumber"].Value<string>();
                resModel.IDV = (int)Convert.ToDouble(data["FinalIDV"].Value<string>());
                resModel.CC = Convert.ToInt16(data["CubicCapacity"].Value<string>());
                resModel.FuelType = data["FuelType"].Value<string>();
                resModel.PolicyStartDate = data["PolicyStartDate"].Value<string>();
                if (idv == 0)
                {
                    if (policytype.Equals("New"))
                    {
                        resModel.MaxIDV = Convert.ToInt32(resModel.IDV + resModel.IDV * 5 / 100);
                        resModel.MinIDV = Convert.ToInt32(resModel.IDV - resModel.IDV * 5 / 100);
                    }
                    else
                    {
                        resModel.MaxIDV = Convert.ToInt32(resModel.IDV + resModel.IDV * 10 / 100);
                        resModel.MinIDV = Convert.ToInt32(resModel.IDV - resModel.IDV * 10 / 100);
                    }
                }
                else
                {
                    resModel.MaxIDV = idvvalue.MaxIDV;
                    resModel.MinIDV = idvvalue.MinIDV;
                }
                resModel.PremiumBreakUpDetails = premiumdata;
                compref.QuoteNo = data["QuoteNumber"].Value<string>();
                resModel.EnquiryId = control.GenerateEnquiryId();
                resModel.Product = Product.Motor;
                resModel.SubProduct = SubProduct.PrivateCar;
                resModel.CompanyName = Company.KOTAK.ToString();
                resModel.PlanName = data["ResponseProductName"].Value<string>();
            }
            catch (Exception ex)
            {
                resModel.Status = Status.Fail;
                resModel.ErrorMsg = ex.ToString();
                throw;
            }
            return resModel;
        }
    }
}