

namespace AndWebApi.ICICI
{
    using AndApp.Utilities;
    #region namespace
    using AndWebApi;
    using AndWebApi.Models;

    using Controllers;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using RestSharp;
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Security;
    using System.Text;
    using System.Web;
    using System.Web.Http;
    using System.Web.Http.Results;
    using System.Web.Mvc;
    #endregion
    public class PrivateCar
    {
        DefaultController de = new DefaultController();
        DAL.ANDAPPEntities ap = new DAL.ANDAPPEntities();
        public Response GetQuoteRequest(Quotation model)
        {

            Response resModel = new Response();
            try
            {

                string GstToState = "";
                string RtoLocCode = "";
                string icici_token = GeticiciTokenNo("");
                if (icici_token != "")
                {

                    var Getrtodata = ap.COMPANY_WISE_RTO_MASTER.Where(x => x.companyid == 9 && x.andapp_rtoid == model.VehicleDetails.RtoId).FirstOrDefault();
                    if (Getrtodata!=null)
                    {
                          GstToState = Getrtodata.rto_loc_code;
                          RtoLocCode = Getrtodata.rtolocationgrpcd;
                    }

                    if (model.IsThirdPartyOnly == true)
                    {

                        var httpWebRequest = (HttpWebRequest)WebRequest.CreateHttp("" + ConfigurationManager.AppSettings["ICICIOnlyTP"] + "");
                        httpWebRequest.ContentType = "application/json ; charset=utf-8";
                        httpWebRequest.Method = "POST";
                        httpWebRequest.Headers.Add("Authorization", "Bearer " + icici_token);


                        string path = AppDomain.CurrentDomain.BaseDirectory;
                        string filePath = Path.Combine(path, "JSON/ICICI/OnlyTP.json");
                        string json = File.ReadAllText(filePath);
                        dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
                        jsonObj["CorrelationId"] = GetGUID();
                        jsonObj["VehicleMakeCode"] = 121;
                        jsonObj["VehicleModelCode"] = 11289;
                        jsonObj["RTOLocationCode"] = 8;
                        jsonObj["PolicyStartDate"] = Convert.ToDateTime(model.PolicyStartDate).ToString("yyyy-MM-dd");
                        jsonObj["PolicyEndDate"] = Convert.ToDateTime(model.PolicyStartDate).AddYears(1).AddDays(-1).ToString("yyyy-MM-dd");


                        
                        jsonObj["GSTToState"] = GstToState;
                        jsonObj["CustomerType"] = "INDIVIDUAL";
                        jsonObj["RTOLocationCode"] = RtoLocCode;

                        jsonObj["ManufacturingYear"] = Convert.ToDateTime(model.VehicleDetails.ManufaturingDate).Year.ToString();
                        jsonObj["DeliveryOrRegistrationDate"] = Convert.ToDateTime(model.VehicleDetails.RegistrationDate).ToString("yyyy-MM-dd");

                        if (model.CoverageDetails.IsBiFuelKit == true)
                        {
                            jsonObj["IsVehicleHaveCNG"] = true;
                            jsonObj["SIVehicleHaveLPG_CNG"] = model.CoverageDetails.BiFuelKitAmount;
                        }

                        if (model.CoverageDetails.IsLegalLiablityPaidDriver == true)
                        {
                            jsonObj["IsLegalLiabilityToPaidDriver"] = true;

                        }
                        if (model.CoverageDetails.IsPACoverUnnamedPerson == true)
                        {
                            jsonObj["IsPACoverUnnamedPassenger"] = model.CoverageDetails.IsPACoverUnnamedPerson;
                            jsonObj["SIPACoverUnnamedPassenger"] = model.VehicleDetails.SC;
                        }
                        if (model.DontKnowPreviousInsurer == true)
                        {
                            jsonObj["PreviousPolicyDetails"]["previousPolicyStartDate"] = Convert.ToDateTime(DateTime.Now).AddYears(-1).AddDays(-1).ToString("yyyy-MM-dd");
                            jsonObj["PreviousPolicyDetails"]["previousPolicyEndDate"] = Convert.ToDateTime(DateTime.Now).AddDays(-2).ToString("yyyy-MM-dd");
                        }
                        else
                        {
                            jsonObj["PreviousPolicyDetails"]["previousPolicyStartDate"] = Convert.ToDateTime(DateTime.Now).AddYears(-1).AddDays(1).ToString("yyyy-MM-dd");
                            jsonObj["PreviousPolicyDetails"]["previousPolicyEndDate"] = Convert.ToDateTime(model.PreviousPolicyDetails.PreviousPolicyEndDate).ToString("yyyy-MM-dd");
                        }
                        jsonObj["PreviousPolicyDetails"]["PreviousPolicyType"] = "Comprehensive Package";

                        jsonObj["PreviousPolicyDetails"]["PeviousInsureName"] = "tata";
                        jsonObj["PreviousPolicyDetails"]["PreviousPolicyNumber"] = "223344";


                        string requestjson = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
                        using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                        {
                            streamWriter.Write(requestjson);
                            streamWriter.Flush();
                            streamWriter.Close();
                        }
                        var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                        {
                            var result = streamReader.ReadToEnd();
                            var data = (JObject)JsonConvert.DeserializeObject(result);
                            resModel = GetQuoteResponse(data, "TP");


                        }
                    }

                    else if (model.IsODOnly == true)
                    {
                        if (model.IDV != 0)
                        {
                            if (model.CustomIDV != null)
                            {
                                var getidvvalues = model.CustomIDV.Where(x => x.CompanyName == "ICICI").FirstOrDefault();
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



                        var httpWebRequest = (HttpWebRequest)WebRequest.CreateHttp("" + ConfigurationManager.AppSettings["ICICIPrivateCarCalculatePremium"] + "");
                        httpWebRequest.ContentType = "application/json ; charset=utf-8";
                        httpWebRequest.Method = "POST";
                        httpWebRequest.Headers.Add("Authorization", "Bearer " + icici_token);


                        string path = AppDomain.CurrentDomain.BaseDirectory;
                        string filePath = Path.Combine(path, "JSON/ICICI/OnlyOD.json");
                        string json = File.ReadAllText(filePath);
                        dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);


                        jsonObj["GSTToState"] = GstToState;
                        jsonObj["RTOLocationCode"] = RtoLocCode;
                        if (model.IDV != 0)
                        {
                            jsonObj["ExShowRoomPrice"] = model.IDV;
                        }
                        jsonObj["PolicyStartDate"] = Convert.ToDateTime(model.PolicyStartDate).ToString("yyyy-MM-dd");
                        jsonObj["PolicyEndDate"] = Convert.ToDateTime(model.PolicyStartDate).AddYears(1).AddDays(-1).ToString("yyyy-MM-dd");

                        jsonObj["ManufacturingYear"] = Convert.ToDateTime(model.VehicleDetails.ManufaturingDate).Year.ToString();

                        jsonObj["DeliveryOrRegistrationDate"] = Convert.ToDateTime(model.VehicleDetails.RegistrationDate).ToString("yyyy-MM-dd");


                        if (model.CoverageDetails.IsPACoverForOwnerDriver == true)
                        {

                            jsonObj["IsPACoverOwnerDriver"] = true;
                        }
                        else
                        {
                            jsonObj["IsPACoverOwnerDriver"] = false;
                        }

                        if (model.DontKnowPreviousInsurer == true)
                        {
                            jsonObj["PreviousPolicyDetails"]["previousPolicyStartDate"] = Convert.ToDateTime(DateTime.Now).AddYears(-1).AddDays(-1).ToString("yyyy-MM-dd");
                            jsonObj["PreviousPolicyDetails"]["previousPolicyEndDate"] = Convert.ToDateTime(DateTime.Now).AddDays(-2).ToString("yyyy-MM-dd");
                        }
                        else
                        {
                            jsonObj["PreviousPolicyDetails"]["PreviousPolicyStartDate"] = Convert.ToDateTime(DateTime.Now).AddYears(-1).AddDays(1).ToString("yyyy-MM-dd");
                            jsonObj["PreviousPolicyDetails"]["PreviousPolicyEndDate"] = Convert.ToDateTime(model.PreviousPolicyDetails.PreviousPolicyEndDate).ToString("yyyy-MM-dd");
                        }


                        jsonObj["PreviousPolicyDetails"]["ClaimOnPreviousPolicy"] = model.PreviousPolicyDetails.IsPreviousInsuranceClaimed;

                        jsonObj["PreviousPolicyDetails"]["PreviousInsurerName"] = "tata";
                        jsonObj["PreviousPolicyDetails"]["PreviousPolicyNumber"] = "12345abcdef";

                        jsonObj["CorrelationId"] = GetGUID();

                        jsonObj["TPStartDate"] = Convert.ToDateTime(model.PreviousTPPolicyDetails.PolicyEndDate).AddYears(-3).AddDays(1).ToString("yyyy-MM-dd");
                        jsonObj["TPEndDate"] = Convert.ToDateTime(model.PreviousTPPolicyDetails.PolicyEndDate).ToString("yyyy-MM-dd");
                        jsonObj["TPInsurerName"] = model.PreviousTPPolicyDetails.CompanyName;


                        if (model.AddonCover != null)
                        {
                            if (model.AddonCover.IsZeroDeperation == true)
                            {
                                jsonObj["ZeroDepPlanName"] = "Silver PVT";
                            }
                            if (model.AddonCover.IsLossofpersonalBelonging == true)
                            {
                                jsonObj["LossOfPersonalBelongingPlanName"] = "PLAN A";
                            }
                            if (model.AddonCover.IsRoadSideAssistance == true)
                            {
                                jsonObj["RSAPlanName"] = "RSA-Plus";
                            }
                            if (model.AddonCover.IsLossofKey == true)
                            {
                                jsonObj["KeyProtectPlan"] = "KP1";
                            }
                            jsonObj["IsConsumables"] = model.AddonCover.IsConsumables;
                            jsonObj["IsEngineProtectPlus"] = model.AddonCover.IsEngineProtector;
                            jsonObj["IsTyreProtect"] = model.AddonCover.IsTyreCover;
                            jsonObj["IsRTIApplicableflag"] = model.AddonCover.IsReturntoInvoice;

                        }

                        if (model.CoverageDetails.IsElectricalAccessories == true)
                        {
                            jsonObj["SIHaveElectricalAccessories"] = model.CoverageDetails.SIElectricalAccessories;
                        }

                        if (model.CoverageDetails.IsNonElectricalAccessories == true)
                        {
                            jsonObj["SIHaveNonElectricalAccessories"] = model.CoverageDetails.SINonElectricalAccessories;
                        }
                        if (model.CoverageDetails.IsBiFuelKit == true)
                        {
                            jsonObj["IsVehicleHaveCNG"] = true;
                            jsonObj["SIVehicleHaveLPG_CNG"] = model.CoverageDetails.BiFuelKitAmount;
                        }

                        string requestjson = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
                        using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                        {
                            streamWriter.Write(requestjson);
                            streamWriter.Flush();
                            streamWriter.Close();
                        }
                        var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                        {
                            var result = streamReader.ReadToEnd();
                            var data = (JObject)JsonConvert.DeserializeObject(result);
                            resModel = GetQuoteResponse(data, "");
                            resModel.IDV = model.IDV;
                            if (model.CustomIDV == null)
                            {

                                var generalInformation = data["generalInformation"];
                                string showRoomPrice = generalInformation["showRoomPrice"].Value<string>();

                                string IDV = generalInformation["depriciatedIDV"].Value<string>();
                                double idvdata = double.Parse(showRoomPrice);
                                resModel.IDV = idvdata;
                                resModel.MaxIDV = Convert.ToInt32(Math.Round(idvdata + (idvdata * 19.99 / 100), 0));
                                resModel.MinIDV = Convert.ToInt32(Math.Round(idvdata - (idvdata * 5.99 / 100), 0));

                            }
                            else
                            {
                                var getidvvalue = model.CustomIDV.Where(x => x.CompanyName == "ICICI").FirstOrDefault();
                                resModel.MinIDV = getidvvalue.MinIDV;
                                resModel.MaxIDV = getidvvalue.MaxIDV;
                            }
                        }
                    }

                    else
                    {

                        if (model.IDV != 0)
                        {
                            if (model.CustomIDV != null)
                            {
                                var getidvvalues = model.CustomIDV.Where(x => x.CompanyName == "ICICI").FirstOrDefault();
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


                        var httpWebRequest = (HttpWebRequest)WebRequest.CreateHttp("" + ConfigurationManager.AppSettings["ICICIPrivateCarCalculatePremium"] + "");
                        httpWebRequest.ContentType = "application/json ; charset=utf-8";
                        httpWebRequest.Method = "POST";
                        httpWebRequest.Headers.Add("Authorization", "Bearer " + icici_token);





                        string path = AppDomain.CurrentDomain.BaseDirectory;
                        string filePath = Path.Combine(path, "JSON/ICICI/Quote.json");
                        string json = File.ReadAllText(filePath);
                        dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);

                        //JObject jo = JObject.Parse(jsonObj);
                        //jo.Property("PreviousPolicyDetails").Remove();
                        //jsonObj = jo.ToString();

                        if (model.IDV != 0)
                        {
                            jsonObj["ExShowRoomPrice"] = model.IDV;
                        }
                        //string ManufacturingYear = "2020";
                        jsonObj["BusinessType"] = model.PolicyType.Equals("New") ? "New Business" : "Roll Over";
                        //jsonObj["CustomerType"] = model.CustomerType;
                        jsonObj["GSTToState"] = GstToState;
                        jsonObj["RTOLocationCode"] = RtoLocCode;
                        jsonObj["ManufacturingYear"] = Convert.ToDateTime(model.VehicleDetails.ManufaturingDate).Year.ToString();
                        jsonObj["FirstRegistrationDate"] = Convert.ToDateTime(model.VehicleDetails.RegistrationDate).ToString("yyyy-MM-dd");
                        jsonObj["DeliveryOrRegistrationDate"] = Convert.ToDateTime(model.VehicleDetails.RegistrationDate).ToString("yyyy-MM-dd");
                        jsonObj["PolicyStartDate"] = Convert.ToDateTime(model.PolicyStartDate).ToString("yyyy-MM-dd");
                        jsonObj["PolicyEndDate"] = Convert.ToDateTime(model.PolicyStartDate).AddYears(1).AddDays(-1).ToString("yyyy-MM-dd");

                        if (model.CoverageDetails.IsPACoverForOwnerDriver == true)
                        {

                            jsonObj["IsPACoverOwnerDriver"] = true;
                        }
                        else
                        {
                            jsonObj["IsPACoverOwnerDriver"] = false;
                        }
                        //jsonObj["Tenure"] = "1";
                        //jsonObj["TPTenure"] = "3";
                        //jsonObj["PACoverTenure"] = "1";
                        //jsonObj["IsVehicleHaveLPG"] = model.CoverageDetails.IsBiFuelKit;
                        //jsonObj["IsVehicleHaveCNG"] = model.CoverageDetails.IsBiFuelKit;
                        //jsonObj["IsLegaLiabilityToWorkmen"] = false;//pending;
                        //jsonObj["NoOfWorkmen"] = 0;
                        //jsonObj["IsFiberGlassFuelTank"] = false;
                        //jsonObj["IsExtensionCountry"] = false;
                        //jsonObj["LossOfPersonalBelongingPlanName"] = "PLAN A";
                        //jsonObj["OtherDiscount"] = 0;
                        //jsonObj["VehicleModelCode"] = model.VehicleDetails.ModelCode;
                        //jsonObj["RTOLocationCode"] = model.VehicleDetails.RtoId;
                        //jsonObj["ExShowRoomPrice"] = model.VehicleDetails.ExShowroomPrice;
                        //jsonObj["VehicleMakeCode"] = model.VehicleDetails.MakeCode;
                        //jsonObj["GSTToState"] = model.VehicleAddressDetails.State;
                        //jsonObj["IsPACoverOwnerDriver"] = model.CoverageDetails.IsPACoverPaidDriver;
                        //jsonObj["IsNoPrevInsurance"] = model.DontKnowPreviousInsurer;
                        //jsonObj["IsAntiTheftDisc"] = model.DiscountDetails.IsAntiTheftDevice;
                        //jsonObj["IsHandicapDisc"] = model.DiscountDetails.IsUseForHandicap;
                        //jsonObj["IsLegalLiabilityToPaidDriver"] = model.CoverageDetails.IsLegalLiablityPaidDriver;
                        //jsonObj["IsLegalLiabilityToPaidEmployee"] = model.CoverageDetails.IsEmployeeLiability;
                        //jsonObj["NoOfEmployee"] = 0;
                        //jsonObj["IsAutomobileAssocnFlag"] = true;
                        //jsonObj["IsRTIApplicableflag"] = true;




                        if (model.AddonCover != null)
                        {
                            if (model.AddonCover.IsZeroDeperation == true)
                            {
                                jsonObj["ZeroDepPlanName"] = "Silver PVT";
                            }
                            if (model.AddonCover.IsLossofpersonalBelonging == true)
                            {
                                jsonObj["LossOfPersonalBelongingPlanName"] = "PLAN A";
                            }
                            if (model.AddonCover.IsRoadSideAssistance == true)
                            {
                                jsonObj["RSAPlanName"] = "RSA-Plus";
                            }
                            if (model.AddonCover.IsLossofKey == true)
                            {
                                jsonObj["KeyProtectPlan"] = "KP1";
                            }
                            jsonObj["IsConsumables"] = model.AddonCover.IsConsumables;
                            jsonObj["IsEngineProtectPlus"] = model.AddonCover.IsEngineProtector;
                            jsonObj["IsTyreProtect"] = model.AddonCover.IsTyreCover;
                            jsonObj["IsRTIApplicableflag"] = model.AddonCover.IsReturntoInvoice;

                        }


                        if (model.CoverageDetails.IsElectricalAccessories == true)
                        {
                            jsonObj["SIHaveElectricalAccessories"] = model.CoverageDetails.SIElectricalAccessories;
                        }

                        if (model.CoverageDetails.IsNonElectricalAccessories == true)
                        {
                            jsonObj["SIHaveNonElectricalAccessories"] = model.CoverageDetails.SINonElectricalAccessories;
                        }
                        if (model.CoverageDetails.IsBiFuelKit == true)
                        {
                            jsonObj["IsVehicleHaveCNG"] = true;
                            jsonObj["SIVehicleHaveLPG_CNG"] = model.CoverageDetails.BiFuelKitAmount;
                        }

                        if (model.CoverageDetails.IsLegalLiablityPaidDriver == true)
                        {
                            jsonObj["IsLegalLiabilityToPaidDriver"] = true;

                        }
                        if (model.CoverageDetails.IsPACoverUnnamedPerson == true)
                        {
                            jsonObj["IsPACoverUnnamedPassenger"] = model.CoverageDetails.IsPACoverUnnamedPerson;
                            jsonObj["SIPACoverUnnamedPassenger"] = model.VehicleDetails.SC;
                        }


                        //jsonObj["OtherLoading"] = "0.0";
                        //if (model.DiscountDetails.IsTPPDRestrictedto6000 == true)
                        //{
                        //    jsonObj["TPPDLimit"] = "6000";
                        //}
                        //else
                        //{
                        //    jsonObj["TPPDLimit"] = "";
                        //}

                        jsonObj["CorrelationId"] = GetGUID();
                        //if (model.PreviousPolicyDetails != null)
                        //{
                        //    jsonObj["PreviousPolicyDetails"][0]["PreviousPolicyType"] = model.PreviousPolicyDetails.PreviousPolicyStartDate;
                        //    jsonObj["PreviousPolicyDetails"][0]["previousPolicyStartDate"] = model.PreviousPolicyDetails.PreviousPolicyStartDate;
                        //    jsonObj["PreviousPolicyDetails"][0]["previousPolicyEndDate"] = model.PreviousPolicyDetails.PreviousPolicyEndDate;
                        //    jsonObj["PreviousPolicyDetails"][0]["ClaimOnPreviousPolicy"] = model.PreviousPolicyDetails.IsPreviousInsuranceClaimed;
                        //}
                        //else
                        //{
                        //    jsonObj["PreviousPolicyDetails"] = null;
                        //}
                        //File.WriteAllText("D:\\Trupti\\And\\AndApp\\AndWebApi\\JSON\\ICICI\\Quote.json", output);
                        if (model.PolicyType == "New")
                        {
                            jsonObj["PreviousPolicyDetails"] = null;
                           jsonObj["RegistrationNumber"] = "New";

                        }
                        else
                        {
                            if (model.VehicleDetails.RegistrationNumber!=null)
                            {
                                jsonObj["RegistrationNumber"] = model.VehicleDetails.RegistrationNumber;
                            }
                            else
                            {
                                jsonObj["RegistrationNumber"] = "";
                            }
                            if (model.DontKnowPreviousInsurer == true)
                            {
                                jsonObj["PreviousPolicyDetails"]["previousPolicyStartDate"] = Convert.ToDateTime(DateTime.Now).AddYears(-1).AddDays(-1).ToString("yyyy-MM-dd");
                                jsonObj["PreviousPolicyDetails"]["previousPolicyEndDate"] = Convert.ToDateTime(DateTime.Now).AddDays(-2).ToString("yyyy-MM-dd");
                            }
                            else
                            {
                                jsonObj["PreviousPolicyDetails"]["previousPolicyStartDate"] = Convert.ToDateTime(DateTime.Now).AddYears(-1).AddDays(1).ToString("yyyy-MM-dd");
                                jsonObj["PreviousPolicyDetails"]["previousPolicyEndDate"] = Convert.ToDateTime(model.PreviousPolicyDetails.PreviousPolicyEndDate).ToString("yyyy-MM-dd");
                            }
                            if (model.PreviousPolicyDetails.PreviousPolicyType=="TP")
                            {
                                jsonObj["PreviousPolicyDetails"]["PreviousPolicyType"] = "TP";
                            }
                            else
                            {
                                jsonObj["PreviousPolicyDetails"]["PreviousPolicyType"] = "Comprehensive Package";
                            }
                         

                            jsonObj["PreviousPolicyDetails"]["ClaimOnPreviousPolicy"] = model.PreviousPolicyDetails.IsPreviousInsuranceClaimed;
                        }

                        string requestjson = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
                        using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                        {
                            streamWriter.Write(requestjson);
                            streamWriter.Flush();
                            streamWriter.Close();
                        }
                        var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                        {
                            var result = streamReader.ReadToEnd();
                            var data = (JObject)JsonConvert.DeserializeObject(result);
                            resModel = GetQuoteResponse(data, "");
                            resModel.IDV = model.IDV;
                            if (model.CustomIDV == null)
                            {

                                var generalInformation = data["generalInformation"];
                                string IDV = generalInformation["showRoomPrice"].Value<string>();
                                double idvdata = double.Parse(IDV);
                                resModel.IDV = idvdata;
                                resModel.MaxIDV = Convert.ToInt32(Math.Round(idvdata + (idvdata * 19.99 / 100), 0));
                                resModel.MinIDV = Convert.ToInt32(Math.Round(idvdata - (idvdata * 5.99 / 100), 0));

                            }
                            else
                            {
                                var getidvvalue = model.CustomIDV.Where(x => x.CompanyName == "ICICI").FirstOrDefault();
                                resModel.MinIDV = getidvvalue.MinIDV;
                                resModel.MaxIDV = getidvvalue.MaxIDV;
                            }
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                resModel.Status = Status.Fail;
                resModel.ErrorMsg = ex.ToString();
                //throw;
            }
            //if (model.IsODOnly == true || model.IsThirdPartyOnly == true)
            //{
            //    resModel.Status = Status.Fail;
            //}
            return resModel;
        }
        public Response GetQuoteResponse(JObject resdata, string policytype)
        {
            Response resModel = new Response();
            PremiumBreakUpDetails prm = new PremiumBreakUpDetails();
            CompanyWiseRefference compreff = new CompanyWiseRefference();
            try
            {  //JObject json = JObject.Parse(res);

                var status = resdata["status"].Value<string>();
                if (status != "Success")
                {
                    resModel.Status = Status.Fail;
                }
                else
                {
                    var riskdata = resdata["riskDetails"];
                    var generalInformation = resdata["generalInformation"];
                    var correlationId = resdata["correlationId"].Value<string>();
                    //string IDV = generalInformation["showRoomPrice"].Value<string>();
                    //double idvdata = double.Parse(IDV);


                    //  prm.ZeroDepPremium = riskdata["zeroDepreciation"].Value<double>();
                    //  prm.InvoicePriceCoverPremium = riskdata["returnToInvoice"].Value<double>();

                    // prm.KeyReplacementPremium = riskdata["keyProtect"].Value<double>();
                    //prm.LossOfPersonalBelongingPremium = riskdata["lossOfPersonalBelongings"].Value<double>();
                    resModel.PlanName = "Comprehensive Package";
                    compreff.CorrelationId = correlationId;
                    if (policytype != "TP")
                    {
                        prm.AntiTheftDiscount = Math.Round(riskdata["antiTheftDiscount"].Value<double>());
                        prm.VoluntaryDiscount = Math.Round(riskdata["voluntaryDiscount"].Value<double>());
                        prm.LoadingDiscount = Math.Round(riskdata["breakinLoadingAmount"].Value<double>());
                        prm.BasicODPremium = Math.Round(riskdata["basicOD"].Value<double>());
                        prm.BasicThirdPartyLiability = Math.Round(riskdata["basicTP"].Value<double>());

                        prm.ElecAccessoriesPremium = Math.Round(riskdata["electricalAccessories"].Value<double>());
                        prm.NonElecAccessoriesPremium = Math.Round(riskdata["nonElectricalAccessories"].Value<double>());
                        prm.FiberGlassTankPremium = Math.Round(riskdata["fibreGlassFuelTank"].Value<double>());
                        prm.VoluntaryDiscount = Math.Round(riskdata["voluntaryDiscount"].Value<double>());
                        prm.AntiTheftDiscount = Math.Round(riskdata["antiTheftDiscount"].Value<double>());
                        prm.InvoicePriceCoverPremium = Math.Round(riskdata["returnToInvoice"].Value<double>());
                        prm.KeyReplacementPremium = Math.Round(riskdata["keyProtect"].Value<double>());
                        prm.PACoverToOwnDriver = Math.Round(riskdata["paCoverForOwnerDriver"].Value<double>());
                        prm.RSAPremium = Math.Round(riskdata["roadSideAssistance"].Value<double>());
                        prm.ZeroDepPremium = Math.Round(riskdata["zeroDepreciation"].Value<double>());
                        prm.LossOfPersonalBelongingPremium = Math.Round(riskdata["lossOfPersonalBelongings"].Value<double>());
                        prm.TyreProtect = Math.Round(riskdata["tyreProtect"].Value<double>());
                        prm.EngineProtectorPremium = Math.Round(riskdata["engineProtect"].Value<double>());
                        prm.CostOfConsumablesPremium = Math.Round(riskdata["consumables"].Value<double>());
                        prm.CNGLPGKitPremium = Math.Round(riskdata["biFuelKitOD"].Value<double>());
                        resModel.IDV = generalInformation["showRoomPrice"].Value<int>();
                        double totaladdon = prm.InvoicePriceCoverPremium + prm.KeyReplacementPremium + prm.RSAPremium + prm.ZeroDepPremium + prm.LossOfPersonalBelongingPremium + prm.TyreProtect + prm.EngineProtectorPremium + prm.CostOfConsumablesPremium;
                        prm.NetAddonPremium = totaladdon;
                        //   double addontax = (riskdata["returnToInvoice"].Value<int>() * 0.18) + (riskdata["keyProtect"].Value<int>() * 0.18) + (riskdata["roadSideAssistance"].Value<int>() * 0.18) + (riskdata["zeroDepreciation"].Value<int>() * 0.18)+(riskdata["lossOfPersonalBelongings"].Value<int>() * 0.18)+(riskdata["tyreProtect"].Value<int>() * 0.18) + (riskdata["engineProtect"].Value<double>() * 0.18) + (riskdata["consumables"].Value<double>() * 0.18);
                        prm.NetPremium = (Convert.ToInt32(resdata["packagePremium"].Value<string>()) + Convert.ToInt32(prm.AntiTheftDiscount));
                        resModel.FinalPremium = Convert.ToInt32(resdata["finalPremium"].Value<string>());
                        prm.ServiceTax = Convert.ToInt32(resdata["totalTax"].Value<string>());
                        prm.NCBDiscount = riskdata["bonusDiscount"].Value<double>();
                    }

                    else
                    {



                        prm.NCBDiscount = riskdata["bonusDiscount"].Value<double>();
                        prm.LLToPaidDriver = riskdata["paidDriver"].Value<double>();
                        prm.PACoverToOwnDriver = riskdata["paCoverForOwnerDriver"].Value<double>();
                        prm.TPCNGLPGPremium = riskdata["biFuelKitTP"].Value<double>();
                        prm.BasicThirdPartyLiability = riskdata["basicTP"].Value<double>();
                        prm.NetPremium = Convert.ToInt32(riskdata["basicTP"].Value<double>()) + prm.PACoverToOwnDriver;
                        resModel.FinalPremium = Convert.ToInt32(resdata["finalPremium"].Value<string>());
                        prm.ServiceTax = Convert.ToInt32(resdata["totalTax"].Value<string>());
                        //resModel.MaxIDV = Convert.ToInt32(Math.Round(idvdata + (idvdata * 19.99 / 100), 0));
                        //resModel.MinIDV = Convert.ToInt32(Math.Round(idvdata - (idvdata * 5.99 / 100), 0));

                        resModel.IDV = 0;

                    }



                    resModel.PremiumBreakUpDetails = prm;
                    resModel.CompanyWiseRefference = compreff;
                    resModel.PolicyStartDate = generalInformation["policyInceptionDate"].Value<string>();
                    resModel.PolicyEndDate = generalInformation["policyEndDate"].Value<string>();
                    resModel.CC = generalInformation["cubicCapacity"].Value<int>();
                    resModel.SC = generalInformation["seatingCapacity"].Value<string>() == null ? 0 : generalInformation["seatingCapacity"].Value<int>();
                    resModel.Product = Product.Motor;
                    resModel.SubProduct = SubProduct.PrivateCar;
                    resModel.CompanyName = Company.ICICI.ToString();
                    //resModel.EnquiryId = de.GenerateEnquiryId();

                    resModel.Status = Status.Success;
                    resModel.ErrorMsg = "";


                }
            }
            catch (Exception ex)
            {
                resModel.Status = Status.Fail;
                resModel.ErrorMsg = Convert.ToString(ex.Message);
                Console.Write(Convert.ToString(ex.Message));
                LogU.WriteLog("ICICI >> PrivateCar >> GetQuoteResponse >> " + Convert.ToString(ex.Message));
                throw;
            }
            return resModel;
        }



        public Response GetProposalResponse(JObject resdata, string policytype)
        {
            Response resModel = new Response();
            PremiumBreakUpDetails prm = new PremiumBreakUpDetails();
            CompanyWiseRefference compreff = new CompanyWiseRefference();
            try
            {  //JObject json = JObject.Parse(res);

                var status = resdata["status"].Value<string>();
                if (status != "Success")
                {
                    resModel.Status = Status.Fail;
                }
                else
                {
                    var riskdata = resdata["riskDetails"];
                    var generalInformation = resdata["generalInformation"];
                    var correlationId = resdata["correlationId"].Value<string>();
                    //string IDV = generalInformation["showRoomPrice"].Value<string>();
                    //double idvdata = double.Parse(IDV);


                    //  prm.ZeroDepPremium = riskdata["zeroDepreciation"].Value<double>();
                    //  prm.InvoicePriceCoverPremium = riskdata["returnToInvoice"].Value<double>();

                    // prm.KeyReplacementPremium = riskdata["keyProtect"].Value<double>();
                    //prm.LossOfPersonalBelongingPremium = riskdata["lossOfPersonalBelongings"].Value<double>();
                    resModel.PlanName = "Comprehensive Package";
                    compreff.CorrelationId = correlationId;
                    compreff.OrderNo = generalInformation["proposalDate"].Value<string>();
                    compreff.QuoteId = generalInformation["customerId"].Value<string>();
                    if (policytype != "TP")
                    {
                        prm.AntiTheftDiscount =Math.Round(riskdata["antiTheftDiscount"].Value<double>());
                        prm.VoluntaryDiscount = Math.Round(riskdata["voluntaryDiscount"].Value<double>());
                        prm.LoadingDiscount = Math.Round(riskdata["breakinLoadingAmount"].Value<double>());
                        prm.BasicODPremium = Math.Round(riskdata["basicOD"].Value<double>());
                        prm.BasicThirdPartyLiability = Math.Round(riskdata["basicTP"].Value<double>());

                        prm.ElecAccessoriesPremium = Math.Round(riskdata["electricalAccessories"].Value<double>());
                        prm.NonElecAccessoriesPremium = Math.Round(riskdata["nonElectricalAccessories"].Value<double>());
                        prm.FiberGlassTankPremium = Math.Round(riskdata["fibreGlassFuelTank"].Value<double>());
                        prm.VoluntaryDiscount = Math.Round(riskdata["voluntaryDiscount"].Value<double>());
                        prm.AntiTheftDiscount = Math.Round(riskdata["antiTheftDiscount"].Value<double>());
                        prm.InvoicePriceCoverPremium = Math.Round(riskdata["returnToInvoice"].Value<double>());
                        prm.KeyReplacementPremium = Math.Round(riskdata["keyProtect"].Value<double>());

                        prm.RSAPremium = Math.Round(riskdata["roadSideAssistance"].Value<double>());
                        prm.ZeroDepPremium = Math.Round(riskdata["zeroDepreciation"].Value<double>());
                        prm.LossOfPersonalBelongingPremium = Math.Round(riskdata["lossOfPersonalBelongings"].Value<double>());
                        prm.TyreProtect = Math.Round(riskdata["tyreProtect"].Value<double>());
                        prm.EngineProtectorPremium = Math.Round(riskdata["engineProtect"].Value<double>());
                        prm.CostOfConsumablesPremium = Math.Round(riskdata["consumables"].Value<double>());
                        prm.CNGLPGKitPremium = Math.Round(riskdata["biFuelKitOD"].Value<double>());
                        resModel.IDV = generalInformation["showRoomPrice"].Value<int>();



                        //double totaladdon = prm.PAToPaidDriver + prm.InvoicePriceCoverPremium + prm.KeyReplacementPremium + prm.RSAPremium + prm.ZeroDepPremium + prm.LossOfPersonalBelongingPremium + prm.TyreProtect + prm.EngineProtectorPremium + prm.CostOfConsumablesPremium;

                        //   double addontax = (riskdata["returnToInvoice"].Value<int>() * 0.18) + (riskdata["keyProtect"].Value<int>() * 0.18) + (riskdata["roadSideAssistance"].Value<int>() * 0.18) + (riskdata["zeroDepreciation"].Value<int>() * 0.18)+(riskdata["lossOfPersonalBelongings"].Value<int>() * 0.18)+(riskdata["tyreProtect"].Value<int>() * 0.18) + (riskdata["engineProtect"].Value<double>() * 0.18) + (riskdata["consumables"].Value<double>() * 0.18);
                        prm.NetPremium = (Convert.ToInt32(resdata["packagePremium"].Value<string>()));
                        resModel.FinalPremium = Convert.ToInt32(resdata["finalPremium"].Value<string>());
                        prm.ServiceTax = Convert.ToInt32(resdata["totalTax"].Value<string>());
                    }

                    else
                    {
                        prm.ServiceTax = Convert.ToInt32(resdata["totalTax"].Value<string>());
                        prm.NCBDiscount = Math.Round(riskdata["bonusDiscount"].Value<double>());
                        prm.LLToPaidDriver = Math.Round(riskdata["paidDriver"].Value<double>());
                        prm.PACoverToOwnDriver = Math.Round(riskdata["paCoverForOwnerDriver"].Value<double>());
                        prm.TPCNGLPGPremium = Math.Round(riskdata["biFuelKitTP"].Value<double>());
                        prm.BasicThirdPartyLiability = Math.Round(riskdata["basicTP"].Value<double>());
                        prm.NetPremium = Convert.ToInt32(riskdata["basicTP"].Value<double>()) + prm.PACoverToOwnDriver;
                        resModel.FinalPremium = Convert.ToInt32(resdata["finalPremium"].Value<string>());
                        //resModel.MaxIDV = Convert.ToInt32(Math.Round(idvdata + (idvdata * 19.99 / 100), 0));
                        //resModel.MinIDV = Convert.ToInt32(Math.Round(idvdata - (idvdata * 5.99 / 100), 0));

                        resModel.IDV = 0;

                    }



                    resModel.PremiumBreakUpDetails = prm;
                    resModel.CompanyWiseRefference = compreff;
                    resModel.PolicyStartDate = generalInformation["policyInceptionDate"].Value<string>();
                    resModel.PolicyEndDate = generalInformation["policyEndDate"].Value<string>();
                    resModel.CC = generalInformation["cubicCapacity"].Value<int>();
                    resModel.SC = generalInformation["seatingCapacity"].Value<string>() == null ? 0 : generalInformation["seatingCapacity"].Value<int>();
                    resModel.Product = Product.Motor;
                    resModel.SubProduct = SubProduct.PrivateCar;
                    resModel.CompanyName = Company.ICICI.ToString();
                    //resModel.EnquiryId = de.GenerateEnquiryId();

                    resModel.Status = Status.Success;
                    resModel.ErrorMsg = "";


                }
            }
            catch (Exception ex)
            {
                resModel.Status = Status.Fail;
                resModel.ErrorMsg = Convert.ToString(ex.Message);
                Console.Write(Convert.ToString(ex.Message));
                LogU.WriteLog("ICICI >> PrivateCar >> GetQuoteResponse >> " + Convert.ToString(ex.Message));
                throw;
            }
            return resModel;
        }

        public Response GetProposalRequest(Quotation model)
        {

            Response resModel = new Response();
            try
            {

                string GstToState = "";
                string RtoLocCode = "";
                string icici_token = GeticiciTokenNo("");
                if (icici_token != "")
                {
                    var Getrtodata = ap.COMPANY_WISE_RTO_MASTER.Where(x => x.companyid == 9 && x.andapp_rtoid == model.VehicleDetails.RtoId).FirstOrDefault();
                    if (Getrtodata != null)
                    {
                        GstToState = Getrtodata.rto_loc_code;
                        RtoLocCode = Getrtodata.rtolocationgrpcd;
                    }
                    dynamic httpWebRequest;
                    if (model.IsThirdPartyOnly == true)
                    {
                        httpWebRequest = (HttpWebRequest)WebRequest.CreateHttp(ConfigurationManager.AppSettings["ICICIOnlyTPProposal"].ToString());
                    }
                    else
                    {
                        httpWebRequest = (HttpWebRequest)WebRequest.CreateHttp(ConfigurationManager.AppSettings["ICICIProposal"].ToString());
                    }

                    httpWebRequest.ContentType = "application/json ; charset=utf-8";
                    httpWebRequest.Method = "POST";
                    httpWebRequest.Headers.Add("Authorization", "Bearer " + icici_token);

                    string path = AppDomain.CurrentDomain.BaseDirectory;
                    string filePath = Path.Combine(path, "JSON/ICICI/Proposal.json");
                    string json = File.ReadAllText(filePath);
                    dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);

                    jsonObj["GSTToState"] = GstToState;
                    jsonObj["RTOLocationCode"] = RtoLocCode;
                    if (model.PolicyType.ToUpper() != "NEW")
                    {
                        jsonObj["TPStartDate"] = "";
                        jsonObj["TPEndDate"] = "";
                        jsonObj["TPPolicyNo"] = "";
                        jsonObj["TPInsurerName"] = "";
                        jsonObj["TPTenure"] = "";

                    }
                    if (model.IsODOnly == true)
                    {
                        jsonObj["DealId"] = "DL-3001/1484597";
                        jsonObj["Tenure"] = "1";
                        jsonObj["TPTenure"] = "0";
                        jsonObj["TPStartDate"] = Convert.ToDateTime(model.PreviousTPPolicyDetails.PolicyEndDate).AddYears(-3).AddDays(1).ToString("yyyy-MM-dd");
                        jsonObj["TPEndDate"] = Convert.ToDateTime(model.PreviousTPPolicyDetails.PolicyEndDate).ToString("yyyy-MM-dd");
                        jsonObj["TPInsurerName"] = model.PreviousTPPolicyDetails.CompanyName;
                        jsonObj["TPPolicyNo"] = model.PreviousTPPolicyDetails.PolicyNo;

                    }
                    
                    jsonObj["CorrelationId"] = model.CompanyWiseRefference.CorrelationId;

                    if (model.CoverageDetails.IsBiFuelKit == true)
                    {
                        jsonObj["IsVehicleHaveCNG"] = true;
                        jsonObj["SIVehicleHaveLPG_CNG"] = model.CoverageDetails.BiFuelKitAmount;
                    }

                    if (model.CoverageDetails.IsLegalLiablityPaidDriver == true)
                    {
                        jsonObj["IsLegalLiabilityToPaidDriver"] = true;

                    }
                    if (model.CoverageDetails.IsPACoverUnnamedPerson == true)
                    {
                        jsonObj["IsPACoverUnnamedPassenger"] = model.CoverageDetails.IsPACoverUnnamedPerson;
                        jsonObj["SIPACoverUnnamedPassenger"] = model.VehicleDetails.SC;
                    }

                    if (model.CoverageDetails.IsElectricalAccessories == true)
                    {
                        jsonObj["SIHaveElectricalAccessories"] = model.CoverageDetails.SIElectricalAccessories;
                    }

                    if (model.CoverageDetails.IsNonElectricalAccessories == true)
                    {
                        jsonObj["SIHaveNonElectricalAccessories"] = model.CoverageDetails.SINonElectricalAccessories;
                    }

                    // set vehicle details

                    // vechicle details

                    jsonObj["BusinessType"] = model.PolicyType.Equals("New") ? "New Business" : "Roll Over";
                    jsonObj["ManufacturingYear"] = Convert.ToDateTime(model.VehicleDetails.ManufaturingDate).Year.ToString();
                    jsonObj["FirstRegistrationDate"] = Convert.ToDateTime(model.VehicleDetails.RegistrationDate).ToString("yyyy-MM-dd");
                    jsonObj["DeliveryOrRegistrationDate"] = Convert.ToDateTime(model.VehicleDetails.RegistrationDate).ToString("yyyy-MM-dd");
                    jsonObj["PolicyStartDate"] = Convert.ToDateTime(model.PolicyStartDate).ToString("yyyy-MM-dd");
                    jsonObj["PolicyEndDate"] = Convert.ToDateTime(model.PolicyEndDate).ToString("yyyy-MM-dd");

                    jsonObj["EngineNumber"] = model.VehicleDetails.EngineNumber;
                    jsonObj["ChassisNumber"] = model.VehicleDetails.ChassisNumber;
                    jsonObj["VehicleMakeCode"] = "121";
                    jsonObj["VehicleModelCode"] = "11289";
                    jsonObj["RTOLocationCode"] = "8";
                    jsonObj["ExShowRoomPrice"] = model.IDV;

                    // end vehicle details
                    //set addons
                    if (model.IsThirdPartyOnly != true)
                    {
                        if (model.AddonCover != null)
                        {
                            if (model.AddonCover.IsZeroDeperation == true)
                            {
                                jsonObj["ZeroDepPlanName"] = "Silver PVT";
                            }
                            if (model.AddonCover.IsLossofpersonalBelonging == true)
                            {
                                jsonObj["LossOfPersonalBelongingPlanName"] = "PLAN A";
                            }
                            if (model.AddonCover.IsRoadSideAssistance == true)
                            {
                                jsonObj["RSAPlanName"] = "RSA-Plus";
                            }
                            if (model.AddonCover.IsLossofKey == true)
                            {
                                jsonObj["KeyProtectPlan"] = "KP1";
                            }
                            jsonObj["IsConsumables"] = model.AddonCover.IsConsumables;
                            jsonObj["IsEngineProtectPlus"] = model.AddonCover.IsEngineProtector;
                            jsonObj["IsTyreProtect"] = model.AddonCover.IsTyreCover;
                            jsonObj["IsRTIApplicableflag"] = model.AddonCover.IsReturntoInvoice;

                        }
                    }
                    else
                    {
                        jsonObj["DealId"] = "DL-3001/A/1484630";

                    }
                    //end set addons

                    //set customer details
                    jsonObj["CustomerDetails"]["CustomerType"] = model.CustomerType.Equals("Individual") ? "INDIVIDUAL" : model.CustomerType;
                    jsonObj["CustomerDetails"]["CustomerName"] = model.ClientDetails.FirstName + " " + model.ClientDetails.MiddleName + " " + model.ClientDetails.LastName;
                    jsonObj["CustomerDetails"]["DateOfBirth"] = Convert.ToDateTime(model.ClientDetails.DateOfBirth).ToString("yyyy-MM-dd");
                    jsonObj["CustomerDetails"]["PinCode"] = model.CustomerAddressDetails.Pincode;
                    jsonObj["CustomerDetails"]["PANCardNo"] = model.ClientDetails.PanCardNo;
                    jsonObj["CustomerDetails"]["Email"] = model.ClientDetails.EmailId;
                    jsonObj["CustomerDetails"]["MobileNumber"] = model.ClientDetails.MobileNo;
                    jsonObj["CustomerDetails"]["AddressLine1"] = model.CustomerAddressDetails.Address1;
                    jsonObj["CustomerDetails"]["CountryCode"] = "100";
                    jsonObj["CustomerDetails"]["StateCode"] = "65";
                    jsonObj["CustomerDetails"]["CityCode"] = "200";
                    jsonObj["CustomerDetails"]["AadharNumber"] = model.ClientDetails.AadharNo;
                    //end set customer details


                    //start set pp details
                    if (model.PolicyType == "New")
                    {
                        jsonObj["PreviousPolicyDetails"] = null;

                    }
                    else
                    {
                        if (model.IsODOnly == true)
                        {

                            jsonObj["PreviousPolicyDetails"]["PreviousPolicyType"] = "Bundled Package Policy";
                        }
                        else
                        {
                            jsonObj["PreviousPolicyDetails"]["PreviousPolicyType"] = "Comprehensive Package";
                        }
                        jsonObj["PreviousPolicyDetails"]["previousPolicyStartDate"] = Convert.ToDateTime(model.PreviousPolicyDetails.PreviousPolicyEndDate).AddYears(-1).AddDays(1).ToString("yyyy-MM-dd");
                        jsonObj["PreviousPolicyDetails"]["previousPolicyEndDate"] = Convert.ToDateTime(model.PreviousPolicyDetails.PreviousPolicyEndDate).ToString("yyyy-MM-dd");
                        jsonObj["PreviousPolicyDetails"]["ClaimOnPreviousPolicy"] = model.PreviousPolicyDetails.IsPreviousInsuranceClaimed;
                    }

                    jsonObj["nomineeDetails"]["NameOfNominee"] = model.NomineeName;
                    jsonObj["nomineeDetails"]["Age"] = CalculateAge(Convert.ToDateTime(model.NomineeDateOfBirth));
                    jsonObj["nomineeDetails"]["Relationship"] = model.NomineeRelationShip;
                    //end set pp details
                    string requestjson = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
                    using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                    {
                        streamWriter.Write(requestjson);
                        streamWriter.Flush();
                        streamWriter.Close();
                    }
                    var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();
                        var data = (JObject)JsonConvert.DeserializeObject(result);
                        var status = data["status"].Value<string>();
                        if (status == "Success")
                        {


                            if (model.IsThirdPartyOnly == true)
                            {
                                resModel = GetProposalResponse(data, "TP");
                            }
                            else
                            {
                                resModel = GetProposalResponse(data, "");

                            }

                            resModel.IDV = model.IDV;

                            string dealid = "";
                            if (model.IsODOnly == true)
                            {
                                dealid = "DL-3001/1484597";

                            }
                            else if (model.IsThirdPartyOnly == true)
                            {
                                dealid = "DL-3001/A/1484630";
                            }
                            else
                            {
                                dealid = "DL-3001/718881";
                            }
                            //string startdate = data["generalInformation"]["policyInceptionDate"].Value<string>();
                            //string policyEndDate = data["generalInformation"]["policyEndDate"].Value<string>();
                            ap.SP_ICICI_PAYMENT_TAGGING(model.enquiryid, data["correlationId"].Value<string>(), data["finalPremium"].Value<string>(), data["generalInformation"]["customerId"].Value<string>(), "c", data["generalInformation"]["proposalNumber"].Value<string>(), dealid);

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

                            ap.SP_POLICYDETAILSMASTER("I", model.enquiryid, 9, model.pospid, model.CustomerType, model.PolicyType, producttype,
                      null, model.ClientDetails.FirstName, model.ClientDetails.MiddleName, model.ClientDetails.LastName,
                      customeraddress, model.ClientDetails.PanCardNo, model.ClientDetails.GSTIN, model.ClientDetails.EmailId,
                      Convert.ToDateTime(model.ClientDetails.DateOfBirth), model.ClientDetails.MobileNo, model.VehicleDetails.RtoId, null,
                     Convert.ToInt64(model.VehicleDetails.MakeCode), Convert.ToInt64(model.VehicleDetails.ModelCode),
                     model.VehicleDetails.VariantId,
                      Convert.ToDateTime(model.PolicyStartDate), Convert.ToDateTime(model.VehicleDetails.RegistrationDate),
                      Convert.ToDateTime(model.VehicleDetails.ManufaturingDate),
                      model.VehicleDetails.SC, model.VehicleDetails.RegistrationNumber, model.VehicleDetails.CC,
                      model.VehicleDetails.EngineNumber, model.VehicleDetails.ChassisNumber, model.VehicleDetails.Fuel,
                      null, Convert.ToDateTime(model.PolicyStartDate), Convert.ToDateTime(model.PolicyEndDate),
                      tpstartdate, tpenddate,
                      1, ncbper, Convert.ToDecimal(model.PremiumDetails.ncbDiscAmount), null,
                      model.IDV, Convert.ToDecimal(model.PremiumDetails.AddonPremium),
                      Convert.ToDecimal(model.PremiumDetails.OdPremiumAmount),
                      Convert.ToDecimal(model.PremiumDetails.TpPremiumAmount),
                      Convert.ToDecimal(model.PremiumDetails.NetPremiumAmount),
                      Convert.ToDecimal(model.PremiumDetails.TaxAmount),
                      Convert.ToDecimal(model.PremiumDetails.TotalPremiumAmount), false);



                            ap.SP_REQUEST_RESPONSE_API_MASTER(model.enquiryid, 9, requestjson, Convert.ToString(data));
                        }
                        else
                        {
                            resModel.ErrorMsg = data["message"].Value<string>();
                        }


                    }
                }
            }
            catch (Exception ex)
            {
                resModel.Status = Status.Fail;
                resModel.ErrorMsg = ex.ToString();
                //throw;
            }
            return resModel;
        }
 
        public string PaymentMapping(PaymentRequest model)
        {
            string bytes = "";

            Response resModel = new Response();
            try
            {



                string icici_token = GeticiciTokenNo("Payment");
                if (icici_token != "")
                {
                    dynamic httpWebRequest;


                    httpWebRequest = (HttpWebRequest)WebRequest.CreateHttp(ConfigurationManager.AppSettings["ICICIPaymentMapping"].ToString());


                    httpWebRequest.ContentType = "application/json ; charset=utf-8";
                    httpWebRequest.Method = "POST";
                    httpWebRequest.Headers.Add("Authorization", "Bearer " + icici_token);

                    string path = AppDomain.CurrentDomain.BaseDirectory;
                    string filePath = Path.Combine(path, "JSON/ICICI/paymentmapping.json");
                    string json = File.ReadAllText(filePath);
                    dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);


                    jsonObj["CorrelationId"] = model.CompanyDetail.CorrelationId;


                    jsonObj["DealId"] = model.CompanyDetail.applicationId;

                    jsonObj["PaymentEntry"]["onlineDAEntry"]["TransactionId"] = model.CompanyDetail.CorrelationId;
                    jsonObj["PaymentEntry"]["onlineDAEntry"]["ReceiptDate"] = DateTime.Now.ToString("yyyy-MM-dd");
                    jsonObj["PaymentEntry"]["onlineDAEntry"]["PaymentAmount"] = model.FinalPremium.ToString();

                    jsonObj["PaymentEntry"]["onlineDAEntry"]["InstrumentDate"] = DateTime.Now.ToString("yyyy-MM-dd");
                    jsonObj["PaymentEntry"]["onlineDAEntry"]["CustomerID"] = model.CompanyDetail.QuoteId;


                    jsonObj["PaymentTagging"]["customerProposal"][0]["CustomerID"] = model.CompanyDetail.QuoteId; ;
                    jsonObj["PaymentTagging"]["customerProposal"][0]["ProposalNo"] = model.CompanyDetail.QuoteNo; ;

                    string requestjson = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
                    using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                    {
                        streamWriter.Write(requestjson);
                        streamWriter.Flush();
                        streamWriter.Close();
                    }
                    var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();
                        var data = (JObject)JsonConvert.DeserializeObject(result);

                        var paymentTagResponseList = data["paymentTagResponse"]["paymentTagResponseList"][0];
                        string policyno = paymentTagResponseList["policyNo"].Value<string>();

                        if (policyno != null)
                        {
                            //  icici_token = GeticiciTokenNo("Generic");
                            bytes = policyno;


                            //}
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                resModel.Status = Status.Fail;
                resModel.ErrorMsg = ex.ToString();
                //throw;
            }
            return bytes;
        }
 

        public string GeticiciTokenNo(string scope)
        {
            string icici_token = "";

            //var token_client = new RestClient("https://ilesbsanity.insurancearticlez.com");
            var token_client = new RestClient(ConfigurationManager.AppSettings["ICICIToken"].ToString());
          
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(delegate { return true; });
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;

            var token_request = new RestRequest("/cerberus/connect/token", Method.POST);

            token_request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            token_request.AddHeader("Accept", "application/json");
            token_request.AddParameter("grant_type", "password");
            token_request.AddParameter("username", "NodibSoftwares");
            token_request.AddParameter("password", "N@d!b$of6war35");
            if (scope == "Payment")
            {
                token_request.AddParameter("scope", "esbpayment");

            }
            else if (scope == "Generic")
            {
                token_request.AddParameter("scope", "esbgeneric");
            }
            else
            {
                token_request.AddParameter("scope", "esbmotor");
            }

            token_request.AddParameter("client_id", "ro.NodibSoftwares");
            token_request.AddParameter("client_secret", "ro.N@d!b$of6war35");

            var token_response = token_client.Post(token_request);

            if (token_response.StatusCode == HttpStatusCode.OK)
            {
                var token_data = (JObject)JsonConvert.DeserializeObject(token_response.Content.ToString());
                icici_token = token_data["access_token"].Value<string>();
            }
            return icici_token;
        }
        public string GetGUID()
        {
            Guid myuuid = Guid.NewGuid();
            string myuuidAsString = myuuid.ToString();
            return myuuidAsString;
        }


        public string GetPaymentParameter(PaymentRequest model)
        {
            string result;
            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.CreateHttp(ConfigurationManager.AppSettings["ICICICreateBaseTransaction"].ToString());
                httpWebRequest.ContentType = "application/json ; charset=utf-8";

                httpWebRequest.Method = "POST";



                string credentials = String.Format("{0}:{1}", "IANDInsuranceApp", "8LnGONqPrwtctNpQ@");

                byte[] bytes = Encoding.ASCII.GetBytes(credentials);
                string base64 = Convert.ToBase64String(bytes);
                string authorization = String.Concat("Basic ", base64);
                httpWebRequest.Headers["Authorization"] = authorization;



                string path = AppDomain.CurrentDomain.BaseDirectory;
                string filePath = Path.Combine(path, "JSON/ICICI/Payment.json");
                string json = File.ReadAllText(filePath);
                dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
                jsonObj["TransactionId"] = model.CompanyDetail.CorrelationId;
                jsonObj["Amount"] = model.FinalPremium;


                jsonObj["ReturnUrl"] = ConfigurationManager.AppSettings["ICICIthankyouurl"].ToString() + model.CompanyDetail.CorrelationId;

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
                result = result.Replace("\"", "");
                if (!string.IsNullOrEmpty(result))
                {
                    string corid = result.ToString();
                    result = ConfigurationManager.AppSettings["ICICIPaymentMethod"].ToString() + corid;
                }
            }
            catch (Exception ex)
            {

                result = "200";

            }
            return result;
        }


        public int CalculateAge(DateTime dateOfBirth)
        {
            int age;
            try
            {
                age = DateTime.Now.Year - dateOfBirth.Year;
                if (DateTime.Now.DayOfYear < dateOfBirth.DayOfYear)
                    age = age - 1;
            }
            catch (Exception ex)
            {
                LogU.WriteLog("TATA >> PrivateCar >> CalculateAge >> " + Convert.ToString(ex.Message));
                Console.Write(Convert.ToString(ex.Message));
                throw;
            }
            return age;
        }
    }

}



