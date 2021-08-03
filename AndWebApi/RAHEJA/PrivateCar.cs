using AndApp.Utilities;
using AndWebApi.Models;
using DAL;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace AndWebApi.RAHEJA
{
    public class PrivateCar
    {
        public string path = AppDomain.CurrentDomain.BaseDirectory;
        DAL.ANDAPPEntities ap = new DAL.ANDAPPEntities();
        public Response GetQuoteRequest(Quotation model)
        {
            Response resModel = new Response();
            string PrvTypeId = string.Empty;
            string trace = string.Empty;
            try
            {
                var sdate = Convert.ToDateTime(model.PolicyStartDate).ToString("yyyy-MM-dd");
                if (model.PolicyType.Equals("New"))
                {
                    model.PolicyEndDate = Convert.ToDateTime(sdate).AddYears(3).AddDays(-1).ToString("yyyy-MM-dd");
                }
                else
                {
                    //model.PolicyEndDate = Convert.ToDateTime(model.PolicyStartDate).AddDays(364).ToString();
                    model.PolicyEndDate = Convert.ToDateTime(sdate).AddYears(1).AddDays(-1).ToString("yyyy-MM-dd");
                    if (model.PreviousPolicyDetails != null && model.PreviousPolicyDetails.PreviousPolicyType.ToUpper().Equals("COMPREHENSIVE"))
                    {
                        PrvTypeId = "1";
                        model.PreviousPolicyDetails.PreviousPolicyType = "COMPREHENSIVE";
                    }
                    else if (model.PreviousPolicyDetails.PreviousPolicyType.ToUpper().Equals("TP"))
                    {
                        PrvTypeId = "2";
                        model.PreviousPolicyDetails.PreviousPolicyType = "LIABILITY ONLY";
                    }
                    else
                    {
                        resModel.Status = Status.Fail;
                        resModel.ErrorMsg = "Raheja not support previous policy type";
                        return resModel;
                    }

                }

                dynamic idvValue = 0;
                dynamic jsonObject = string.Empty;
                if (model.IDV != 0)
                {
                    if (model.CustomIDV != null)
                    {
                        var getidvvalues = model.CustomIDV.Where(x => x.CompanyName == "RAHEJA").FirstOrDefault();
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

                else
                {

                    #region CalculateIdv
                    string IdvfilePath = Path.Combine(path, "JSON/RAHEJA/Idv.json");
                    string Idvjson = File.ReadAllText(IdvfilePath);
                    dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(Idvjson);
                    jsonObj["objVehicleDetails"]["MakeModelVarient"] = model.VehicleDetails.MakeName + "|" + model.VehicleDetails.ModelName + "|" + model.VehicleDetails.VariantName + "|" + model.VehicleDetails.CC + "CC";
                    jsonObj["objVehicleDetails"]["RtoLocation"] = "MH04";//model.VehicleDetails.RtoCode;
                    jsonObj["objVehicleDetails"]["RegistrationDate"] = Convert.ToDateTime(model.VehicleDetails.RegistrationDate).ToString("yyyy-MM-dd");
                    jsonObj["objVehicleDetails"]["ManufacturingYear"] = Convert.ToString(Convert.ToDateTime(model.VehicleDetails.ManufaturingDate).Year);
                    jsonObj["objVehicleDetails"]["ManufacturingMonth"] = Convert.ToDateTime(model.VehicleDetails.ManufaturingDate).ToString("MM");
                    jsonObj["objPolicy"]["BusinessTypeID"] = model.PolicyType.Equals("New") ? "24" : "25";
                    jsonObj["objPolicy"]["PolicyStartDate"] = Convert.ToDateTime(model.PolicyStartDate).ToString("yyyy-MM-dd"); // model.PolicyStartDate.ToString("yyyy-MM-dd");
                    jsonObj["objPolicy"]["TraceID"] = GetTraceID();
                    jsonObj["objPolicy"]["UserName"] = Convert.ToString(ConfigurationManager.AppSettings["RahUsername"]);
                    jsonObj["objPolicy"]["TPSourceName"] = Convert.ToString(ConfigurationManager.AppSettings["RahTPSrcName"]);
                    if (model.IsODOnly)
                    {
                        jsonObj["objPolicy"]["ProductCode"] = "2323";
                    }
                    else if (!model.PolicyType.ToUpper().Equals("NEW"))
                    {
                        jsonObj["objPolicy"]["ProductCode"] = "2311";
                    }
                    else
                    {
                        jsonObj["objPolicy"]["ProductCode"] = "2367";
                    }
                    string requestjson = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
                    var responseIDVCalc = Webservicecall(requestjson, Convert.ToString(ConfigurationManager.AppSettings["RahBaseUrl"]), Convert.ToString(ConfigurationManager.AppSettings["RahIdv"]));
                    //var responseIDVCalc = Webservicecall(requestjson, "http://52.172.5.3:8423/api/MotorAPI", "http://52.172.5.3:8423/api/MotorAPI/GetVehicleIDV");

                    #endregion
                    if (responseIDVCalc.StatusCode == HttpStatusCode.OK)
                    {
                        dynamic CalculateIDVresponse = (JObject)JsonConvert.DeserializeObject(responseIDVCalc.Content.ToString());
                        var errormsg = CalculateIDVresponse.objFault.ErrorMessage;
                        if (errormsg != "")
                        {
                            resModel.ErrorMsg = errormsg;
                            return resModel;
                        }
                        else
                        {
                            idvValue = CalculateIDVresponse.objVehicleDetails.ModifiedIDV;
                            if (CalculateIDVresponse.objPolicy != null)
                            {
                                trace = CalculateIDVresponse.objPolicy.TraceID.Value;
                            }

                            model.IDV = idvValue;
                        }
                    }
                    else
                    {
                        resModel.Status = Status.Fail;
                        resModel.ErrorMsg = "Getting an error from IDV service.";
                        LogU.WriteLog("RAHEJA >> PrivateCar >> GetIDVRequest >> " + resModel.ErrorMsg);
                        return resModel;
                    }
                }


                if (model.IDV > 0)
                {
                    #region PremiumCalculate
                    string PremiumCalculatefilePath = string.Empty;
                    //string rto = model.VehicleDetails.RtoId;
                    if (model.IsODOnly)
                    {
                        PremiumCalculatefilePath = Path.Combine(path, "JSON/RAHEJA/QuoteOd.json");
                    }
                    else
                    {
                        PremiumCalculatefilePath = Path.Combine(path, "JSON/RAHEJA/Quote.json");
                    }

                    string PremiumCalculatejson = File.ReadAllText(PremiumCalculatefilePath);
                    jsonObject = Newtonsoft.Json.JsonConvert.DeserializeObject(PremiumCalculatejson);
                    string regino1 = string.Empty;
                    string regino2 = string.Empty;
                    string regino3 = string.Empty;
                    string regino4 = string.Empty;
                    //string rtoandcity = AndEnt.VW_RTOMASTER.Where(x => x.rtocode == rtocode).Select(x => x.rtodesc).FirstOrDefault().ToString();
                    jsonObject["objClientDetails"]["MobileNumber"] = model.ClientDetails != null ? (!string.IsNullOrEmpty(model.ClientDetails.MobileNo) ? model.ClientDetails.MobileNo : "9898989850") : "9898989850";
                    jsonObject["objClientDetails"]["EmailId"] = model.ClientDetails != null ? (!string.IsNullOrEmpty(model.ClientDetails.EmailId) ? model.ClientDetails.EmailId : "abc@gmail.com") : "abc@gmail.com";
                    jsonObject["objClientDetails"]["ClientType"] = model.CustomerType.Equals("Individual", StringComparison.OrdinalIgnoreCase) ? "0" : "1";
                    jsonObject["objVehicleDetails"]["MakeModelVarient"] = model.VehicleDetails.MakeName + "|" + model.VehicleDetails.ModelName + "|" + model.VehicleDetails.VariantName + "|" + model.VehicleDetails.CC + "CC";
                    regino1 = model.VehicleDetails.RegistrationNumber.Substring(0, 2);
                    regino2 = model.VehicleDetails.RegistrationNumber.Substring(2, 2);
                    string RTOCode = string.Empty;
                    string RTOName = string.Empty;
                    //model.VehicleDetails.RtoId = 104;

                    if (string.IsNullOrEmpty(Convert.ToString(ConfigurationManager.AppSettings["Envi"])))
                    {
                        jsonObject["objVehicleDetails"]["RtoLocation"] = "Thane|MH04";
                        jsonObject["objVehicleDetails"]["Registration_Number1"] = "MH";
                        jsonObject["objVehicleDetails"]["Registration_Number2"] = "04";
                    }
                    else
                    {
                        if (Convert.ToString(ConfigurationManager.AppSettings["Envi"]).ToUpper().Equals("UAT"))
                        {
                            jsonObject["objVehicleDetails"]["RtoLocation"] = "Thane|MH04";
                            jsonObject["objVehicleDetails"]["Registration_Number1"] = "MH";
                            jsonObject["objVehicleDetails"]["Registration_Number2"] = "04";
                        }
                        else
                        {
                            var rtocode = ap.COMPANY_WISE_RTO_MASTER.Where(x => x.companyid == 16 && x.andapp_rtoid == model.VehicleDetails.RtoId).FirstOrDefault();
                            if (rtocode != null)
                            {
                                RTOCode = rtocode.rto_loc_code;
                                RTOName = rtocode.rto_loc_name;
                            }
                            jsonObject["objVehicleDetails"]["RtoLocation"] = RTOName.Trim() + "|" + RTOCode.Trim();
                            jsonObject["objVehicleDetails"]["Registration_Number1"] = regino1.ToUpper();
                            jsonObject["objVehicleDetails"]["Registration_Number2"] = regino2;
                        }
                    }
                    //jsonObject["objVehicleDetails"]["RtoLocation"] = "Thane|MH04";
                    //jsonObject["objVehicleDetails"]["RtoLocation"] = "AHMEDABAD|"+ regino1 + regino2"";
                    jsonObject["objVehicleDetails"]["RegistrationDate"] = Convert.ToDateTime(model.VehicleDetails.RegistrationDate).ToString("yyyy-MM-dd");
                    jsonObject["objVehicleDetails"]["ManufacturingYear"] = Convert.ToString(Convert.ToDateTime(model.VehicleDetails.ManufaturingDate).Year);
                    jsonObject["objVehicleDetails"]["ManufacturingMonth"] = Convert.ToDateTime(model.VehicleDetails.ManufaturingDate).ToString("MM");
                    jsonObject["objVehicleDetails"]["FuelType"] = model.VehicleDetails.Fuel;
                    jsonObject["objVehicleDetails"]["ModifiedIDV"] = Convert.ToString(model.IDV);
                    jsonObject["objPolicy"]["BusinessTypeID"] = model.PolicyType.Equals("New") ? "24" : "25";
                    if (model.VehicleDetails.RegistrationNumber.Length > 5)
                    {
                        //regino3 = model.VehicleDetails.RegistrationNumber.Substring(4, 2).ToUpper();
                        //regino4 = model.VehicleDetails.RegistrationNumber.Substring(6, 4);
                        jsonObject["objVehicleDetails"]["Registration_Number3"] = "AB";
                        jsonObject["objVehicleDetails"]["Registration_Number4"] = "1234";
                    }
                    else
                    {
                        jsonObject["objVehicleDetails"]["Registration_Number3"] = "AB";
                        jsonObject["objVehicleDetails"]["Registration_Number4"] = "1234";
                    }
                    if (model.IsODOnly)
                    {
                        jsonObject["objPolicy"]["ProductCode"] = "2323";
                        jsonObject["objPolicy"]["ProductName"] = "MOTOR - PRIVATE CAR STANDALONE OD(2323)";
                        jsonObject["objPolicy"]["CoverType"] = "1668";
                        if (model.PreviousTPPolicyDetails != null)
                        {
                            var PrvTPPolicyStartDate = Convert.ToDateTime(model.PreviousTPPolicyDetails.PolicyEndDate).AddYears(-3).AddDays(1).ToString("yyyy-MM-dd");
                            //DateTime PrvTPPolicyStartDate = Convert.ToDateTime(model.PreviousTPPolicyDetails.PolicyEndDate).AddYears(-3).AddDays(1);
                            jsonObject["objPolicy"]["TPPolicyStartDate"] = Convert.ToDateTime(PrvTPPolicyStartDate).ToString("yyyy-MM-dd");
                            jsonObject["objPolicy"]["TPPolicyEndDate"] = Convert.ToDateTime(model.PreviousTPPolicyDetails.PolicyEndDate).ToString("yyyy-MM-dd");
                        }
                    }
                    else if (!model.PolicyType.ToUpper().Equals("NEW"))
                    {
                        //jsonObject["objVehicleDetails"]["Registration_Number3"] = "AB";
                        //jsonObject["objVehicleDetails"]["Registration_Number4"] = "1234";
                        jsonObject["objPolicy"]["ProductCode"] = "2311";
                        jsonObject["objPolicy"]["ProductName"] = "MOTOR - PRIVATE CAR PACKAGE POLICY(2311)";
                        jsonObject["objPolicy"]["CoverType"] = "1471";
                    }
                    else
                    {
                        //jsonObject["objVehicleDetails"]["Registration_Number3"] = "BA";
                        //jsonObject["objVehicleDetails"]["Registration_Number4"] = "1222";
                        jsonObject["objPolicy"]["ProductCode"] = "2367";
                        jsonObject["objPolicy"]["ProductName"] = "MOTOR PRIVATE CAR BUNDLED POLICY(2367)";
                        jsonObject["objPolicy"]["CoverType"] = "1473";

                    }
                    jsonObject["objPolicy"]["TraceID"] = trace;//GetTraceID();
                    jsonObject["objPolicy"]["UserName"] = Convert.ToString(ConfigurationManager.AppSettings["RahUsername"]);
                    jsonObject["objPolicy"]["TPSourceName"] = Convert.ToString(ConfigurationManager.AppSettings["RahTPSrcName"]);
                    jsonObject["objPolicy"]["PolicyStartDate"] = Convert.ToDateTime(sdate).ToString("yyyy-MM-dd");
                    jsonObject["objPolicy"]["PolicyEndDate"] = Convert.ToDateTime(model.PolicyEndDate).ToString("yyyy-MM-dd");
                    jsonObject["objPolicy"]["BusinessTypeID"] = model.PolicyType.Equals("New") ? "24" : "25";
                    if (model.PolicyType.Equals("New"))
                    {
                        //model.Tennure = model.IsODOnly ? 152 : 102;
                        model.Tennure = model.IsODOnly ? 152 : 101;
                    }
                    else
                    {
                        //model.Tennure = model.IsODOnly ? 151 : 101;
                        model.Tennure = model.IsODOnly ? 151 : 102;
                    }
                    jsonObject["objPolicy"]["Tennure"] = model.Tennure;
                    JToken methods = jsonObject.SelectToken("objCovers");
                    foreach (JToken signInName in methods)
                    {
                        string CoverID = (string)signInName.SelectToken("CoverID");
                        var itemProperties = signInName.Children<JProperty>();
                        switch (CoverID)
                        {
                            case "9":
                                if (model.IsThirdPartyOnly)
                                    itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(false);
                                break;
                            case "10":
                                if (model.IsODOnly)
                                    //itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(false);
                                    signInName.Parent.Remove();
                                break;
                            case "39":
                                if (model.CoverageDetails.IsFiberGlassFuelTank)
                                    itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(true);
                                break;
                            case "49":
                                if (model.CoverageDetails.IsLegalLiablityPaidDriver)
                                    itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(true);
                                break;
                            case "21":
                                if (model.CoverageDetails.IsBiFuelKit)
                                    itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(true);
                                break;
                            case "20":
                                if (model.CoverageDetails.IsBiFuelKit)
                                {
                                    itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(true);
                                    dynamic myObject = new JObject();
                                    myObject.PHNumericFeild1 = model.CoverageDetails.BiFuelKitAmount;
                                    itemProperties.FirstOrDefault(xx => xx.Name == "objCoverDetails").Value.Replace(myObject);
                                }
                                break;
                            case "73":
                                if (model.CoverageDetails.IsPACoverForOwnerDriver)
                                {
                                }
                                else
                                {
                                    itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(false);
                                    dynamic myObjectPAOwner = new JObject();
                                    myObjectPAOwner.PHintFeild1 = string.Empty;
                                    myObjectPAOwner.PHVarcharFeild1 = string.Empty;
                                    myObjectPAOwner.PHVarcharFeild2 = string.Empty;
                                    myObjectPAOwner.PHVarcharFeild2 = string.Empty;
                                    myObjectPAOwner.PHNumericFeild2 = string.Empty;
                                    myObjectPAOwner.PHNumericFeild1 = string.Empty;
                                    myObjectPAOwner.PHVarcharFeild4 = string.Empty;
                                    myObjectPAOwner.PHVarcharFeild5 = string.Empty;
                                    myObjectPAOwner.PHBooleanField1 = false;
                                    myObjectPAOwner.PHBooleanField2 = true;
                                    itemProperties.FirstOrDefault(xx
                                        => xx.Name == "objCoverDetails").Value.Replace(myObjectPAOwner);
                                }
                                break;
                            case "87":
                                if (model.DiscountDetails.IsTPPDRestrictedto6000)
                                    itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(true);
                                break;
                            //case "76":
                            //    if (model.CoverageDetails.IsPACoverPaidDriver)
                            //    {
                            //        itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(true);
                            //        dynamic myObjectLL = new JObject();
                            //        myObjectLL.PHNumericFeild1 = model.CoverageDetails.PACoverPaidDriverAmount;
                            //        itemProperties.FirstOrDefault(xx => xx.Name == "objCoverDetails").Value.Replace(myObjectLL);
                            //    }
                            //    break;
                            //case "94":
                            //    if (model.CoverageDetails.IsPACoverUnnamedPerson)
                            //    {
                            //        itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(true);
                            //        dynamic myObjectPA = new JObject();
                            //        myObjectPA.PHVarcharFeild1 = Convert.ToString(model.CoverageDetails.NoOfLLPaidDriver);
                            //        myObjectPA.PHNumericFeild1 = Convert.ToString(model.VehicleDetails.SC);
                            //        itemProperties.FirstOrDefault(xx
                            //            => xx.Name == "objCoverDetails").Value.Replace(myObjectPA);
                            //    }
                            //    break;
                            //case "105":
                            //    if (model.CoverageDetails.IsEmployeeLiability)
                            //        itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(true);
                            //    break;
                            case "91":
                                if (model.DiscountDetails.VoluntaryExcessAmount > 0)
                                {
                                    itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(true);
                                    dynamic myObjectVoluntary = new JObject();
                                    myObjectVoluntary.PHNumericFeild1 = model.DiscountDetails.VoluntaryExcessAmount;
                                    itemProperties.FirstOrDefault(xx => xx.Name == "objCoverDetails").Value.Replace(myObjectVoluntary);
                                }
                                break;
                            case "33":
                                if (model.CoverageDetails.IsElectricalAccessories)
                                {
                                    itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(true);
                                    dynamic myObjectele = new JObject();
                                    myObjectele.PHNumericFeild1 = model.CoverageDetails.SIElectricalAccessories / 2;
                                    myObjectele.PHVarcharFeild1 = "Test";
                                    myObjectele.PHNumericFeild2 = model.CoverageDetails.SIElectricalAccessories / 2;
                                    myObjectele.PHVarcharFeild2 = "Test";
                                    itemProperties.FirstOrDefault(xx => xx.Name == "objCoverDetails").Value.Replace(myObjectele);
                                }
                                break;
                            case "70":
                                if (model.CoverageDetails.IsNonElectricalAccessories)
                                {
                                    itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(true);
                                    dynamic myObjectele = new JObject();
                                    myObjectele.PHNumericFeild1 = model.CoverageDetails.SINonElectricalAccessories / 2;
                                    myObjectele.PHVarcharFeild1 = "Test";
                                    myObjectele.PHNumericFeild2 = model.CoverageDetails.SINonElectricalAccessories / 2;
                                    myObjectele.PHVarcharFeild2 = "Test";
                                    itemProperties.FirstOrDefault(xx => xx.Name == "objCoverDetails").Value.Replace(myObjectele);
                                }
                                break;
                            case "97":
                                if (model.AddonCover.IsZeroDeperation)
                                {
                                    if (model.PolicyType.Equals("New"))
                                    {
                                        dynamic myObjectelezero = new JObject();
                                        //myObjectelezero.PHNumericFeild1 = model.CoverageDetails.SINonElectricalAccessories / 2;
                                        myObjectelezero.PHVarcharFeild1 = "2";
                                        // myObjectelezero.PHNumericFeild2 = model.CoverageDetails.SINonElectricalAccessories / 2;
                                        myObjectelezero.PHVarcharFeild2 = "";
                                        itemProperties.FirstOrDefault(xx => xx.Name == "objCoverDetails").Value.Replace(myObjectelezero);
                                        itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(true);
                                    }
                                    else
                                    {
                                        dynamic myObjectelezero = new JObject();
                                        //myObjectelezero.PHNumericFeild1 = model.CoverageDetails.SINonElectricalAccessories / 2;
                                        myObjectelezero.PHVarcharFeild1 = "2";
                                        // myObjectelezero.PHNumericFeild2 = model.CoverageDetails.SINonElectricalAccessories / 2;
                                        myObjectelezero.PHVarcharFeild2 = "Yes";
                                        itemProperties.FirstOrDefault(xx => xx.Name == "objCoverDetails").Value.Replace(myObjectelezero);
                                        itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(true);
                                        //itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(false);
                                    }
                                }
                                break;
                            case "37":
                                if (model.AddonCover.IsEngineProtector)
                                    itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(true);
                                break;
                            case "24":
                                if (model.AddonCover.IsConsumables)
                                    itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(true);
                                break;
                            case "80":
                                if (model.AddonCover.IsReturntoInvoice)
                                    itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(true);
                                break;
                            case "101":
                                if (model.AddonCover.IsLossofpersonalBelonging)
                                    itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(true);
                                break;
                            case "104":
                                if (model.AddonCover.IsTyreCover)
                                    itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(true);
                                break;
                            default:
                                break;
                        }
                    }
                    string requestjson1 = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObject, Newtonsoft.Json.Formatting.Indented);
                    //if (model.PreviousPolicyDetails != null && !model.PolicyType.ToUpper().Equals("NEW"))
                    //{
                    if (!model.PolicyType.ToUpper().Equals("NEW"))
                    {

                        if (model.PreviousPolicyDetails != null)
                        {
                            var PreviousPolicyStartDate = Convert.ToDateTime(model.PreviousPolicyDetails.PreviousPolicyEndDate).AddYears(-1).AddDays(1).ToString("yyyy-MM-dd");
                            model.PreviousPolicyDetails.CompanyId = Convert.ToInt16(GetRahejaPrevCompany(Convert.ToInt16(model.PreviousPolicyDetails.CompanyId)));
                            //model.PreviousPolicyDetails.CompanyId = model.PreviousPolicyDetails.CompanyId;
                            requestjson1 += "\"objPreviousInsurance\":{\"PrevPolicyType\":\"" + PrvTypeId +
                                     "\",\"PrevPolicyStartDate\":\"" + Convert.ToDateTime(PreviousPolicyStartDate).ToString("yyyy-MM-dd") +
                                     "\",\"PrevPolicyEndDate\":\"" + Convert.ToDateTime(model.PreviousPolicyDetails.PreviousPolicyEndDate).ToString("yyyy-MM-dd") +
                                     "\",\"ProductCode\":\"2311\",\"PrevInsuranceCompanyID\":\"" + model.PreviousPolicyDetails.CompanyId + "\",\"PrevNCB\":\"" +
                                     model.PreviousPolicyDetails.PreviousNcbPercentage + "\",\"IsClaimedLastYear\":\"" + (model.PreviousPolicyDetails.IsPreviousInsuranceClaimed ? "1" : "2") +
                                     //"\",\"NatureOfLoss\":\"" + "2" +"\",\"NatureOfLoss\":\"" + "2" +
                                     "\",\"NatureOfLoss\":\"" + (model.PreviousPolicyDetails.IsPreviousInsuranceClaimed ? PrvTypeId : string.Empty) +
                                     "\",\"prevPolicyCoverType\":\"" + model.PreviousPolicyDetails.PreviousPolicyType.ToUpper() + "\",\"CurrentNCBHidden\":\"" + model.CurrentNcb +
                                     "\"}";
                        }
                    }
                    var prmCalcResponse = Webservicecall(requestjson1, Convert.ToString(ConfigurationManager.AppSettings["RahBaseUrl"]), Convert.ToString(ConfigurationManager.AppSettings["RahQuote"]));
                    string resjson1 = Newtonsoft.Json.JsonConvert.SerializeObject(prmCalcResponse, Newtonsoft.Json.Formatting.Indented);
                    if (prmCalcResponse.StatusCode == HttpStatusCode.OK)
                    {
                        resModel = GetQuoteResponse((JObject)JsonConvert.DeserializeObject(Convert.ToString(prmCalcResponse.Content)));
                    }
                    else
                    {
                        resModel.Status = Status.Fail;
                        resModel.ErrorMsg = "Getting an error from Quote service.";
                        LogU.WriteLog("RAHEJA >> PrivateCar >> GetQuoteRequest >> " + resModel.ErrorMsg);
                        return resModel;
                    }
                    #endregion
                }
                resModel.IDV = model.IDV;
                if (model.CustomIDV == null)
                {
                    if (model.PolicyType.Equals("New"))
                    {
                        resModel.MaxIDV = Convert.ToInt32(resModel.IDV + resModel.IDV * 15 / 100);
                        resModel.MinIDV = Convert.ToInt32(resModel.IDV - resModel.IDV * 15 / 100);
                    }
                    else
                    {
                        resModel.MaxIDV = Convert.ToInt32(resModel.IDV + resModel.IDV * 10 / 100);
                        resModel.MinIDV = Convert.ToInt32(resModel.IDV - resModel.IDV * 10 / 100);
                    }
                }
                else
                {
                    var getidvvalue = model.CustomIDV.Where(x => x.CompanyName == "RAHEJA").FirstOrDefault();
                    resModel.MinIDV = getidvvalue.MinIDV;
                    resModel.MaxIDV = getidvvalue.MaxIDV;
                }
                resModel.SC = model.VehicleDetails.SC;
                resModel.CC = model.VehicleDetails.CC;
                resModel.FuelType = model.VehicleDetails.Fuel;
                resModel.PolicyStartDate = model.PolicyStartDate;
                resModel.PolicyEndDate = model.PolicyEndDate;
                //resModel.Status = Status.Success;
            }
            catch (Exception ex)
            {
                resModel.Status = Status.Fail;
                resModel.ErrorMsg = Convert.ToString(ex.Message);
                Console.Write(Convert.ToString(ex.Message));
                LogU.WriteLog("RAHEJA >> PrivateCar >> GetQuoteRequest >> " + Convert.ToString(ex.Message));
            }
            return resModel;
        }

        /// <summary>
        /// Quote response method.
        /// </summary>
        /// <param name="res">xdocument objects.</param>
        /// <returns>return response type object.</returns>
        public Response GetQuoteResponse(dynamic prmCalcResponse)
        {
            Response resModel = new Response();
            resModel.PremiumBreakUpDetails = new PremiumBreakUpDetails();
            resModel.CompanyWiseRefference = new CompanyWiseRefference();
            try
            {
                resModel.ErrorMsg = prmCalcResponse.objFault.ErrorMessage;
                if (!string.IsNullOrEmpty(resModel.ErrorMsg))
                {
                    resModel.Status = Status.Fail;
                    return resModel;
                }
                else
                {
                    resModel.Status = Status.Success;
                    //string q = prmCalcResponse.ChildrenTokens[9]["QuoteID"];
                    //resModel.PremiumBreakUpDetails.BasicThirdPartyLiability = Convert.ToDouble(prmCalcResponse.TotalLiabilityPremium.Value);
                    //resModel.FinalPremium = Convert.ToInt32(prmCalcResponse.FinalPremium.Value);
                    resModel.PremiumBreakUpDetails.NCBDiscount = Math.Round(Convert.ToDouble(prmCalcResponse.NCBPremium.Value));

                    //resModel.PremiumBreakUpDetails.NetODPremium = Convert.ToDouble(prmCalcResponse.TotalODPremium.Value);
                    //resModel.PremiumBreakUpDetails.ServiceTax = Convert.ToDouble(prmCalcResponse.TotalTax.Value);

                    if (prmCalcResponse.objPolicy != null)
                    {
                        resModel.CompanyWiseRefference.QuoteId = prmCalcResponse.objPolicy.QuoteID.Value;
                        resModel.CompanyWiseRefference.QuoteNo = prmCalcResponse.objPolicy.QuoteNo.Value;
                        resModel.CompanyWiseRefference.applicationId = prmCalcResponse.objPolicy.Policyid.Value;
                        resModel.PlanName = prmCalcResponse.objPolicy.ProductName.Value;
                        resModel.CompanyWiseRefference.OrderNo = prmCalcResponse.objPolicy.TraceID.Value;
                    }
                    var addon = prmCalcResponse.lstCoverResponce;
                    if (addon != null)
                    {
                        foreach (var item in addon)
                        {
                            string coverId = item.CoverID.Value;
                            switch (coverId)
                            {
                                case "9":
                                    resModel.PremiumBreakUpDetails.BasicODPremium = Math.Round(Convert.ToDouble(item.CoverPremium.Value));
                                    break;
                                case "10":
                                    resModel.PremiumBreakUpDetails.BasicThirdPartyLiability = Math.Round(Convert.ToDouble(item.CoverPremium.Value));
                                    break;
                                case "24":
                                    resModel.PremiumBreakUpDetails.CostOfConsumablesPremium = Math.Round(Convert.ToDouble(item.CoverPremium.Value));
                                    break;
                                case "37":
                                    resModel.PremiumBreakUpDetails.EngineProtectorPremium = Math.Round(Convert.ToDouble(item.CoverPremium.Value));
                                    break;
                                case "39":
                                    resModel.PremiumBreakUpDetails.FiberGlassTankPremium = Math.Round(Convert.ToDouble(item.CoverPremium.Value));
                                    break;
                                case "49":
                                    resModel.PremiumBreakUpDetails.LLToPaidDriver = Math.Round(Convert.ToDouble(item.CoverPremium.Value));
                                    break;
                                case "21":
                                    resModel.PremiumBreakUpDetails.TPCNGLPGPremium = Math.Round(Convert.ToDouble(item.CoverPremium.Value));
                                    break;
                                case "20":
                                    resModel.PremiumBreakUpDetails.CNGLPGKitPremium = Math.Round(Convert.ToDouble(item.CoverPremium.Value));
                                    break;
                                case "87":
                                    resModel.PremiumBreakUpDetails.RestrictLiability = Math.Round(Convert.ToDouble(item.CoverPremium.Value));
                                    break;
                                case "80":
                                    resModel.PremiumBreakUpDetails.InvoicePriceCoverPremium = Math.Round(Convert.ToDouble(item.CoverPremium.Value));
                                    break;
                                case "97":
                                    resModel.PremiumBreakUpDetails.ZeroDepPremium = Math.Round(Convert.ToDouble(item.CoverPremium.Value));
                                    break;
                                //case "100":
                                //    resModel.PremiumBreakUpDetails.KeyReplacementPremium = Convert.ToDouble(item.CoverPremium.Value);
                                //    break;
                                case "94":
                                    resModel.PremiumBreakUpDetails.PACoverToUnNamedPerson = Math.Round(Convert.ToDouble(item.CoverPremium.Value));
                                    break;
                                case "104":
                                    resModel.PremiumBreakUpDetails.TyreProtect = Math.Round(Convert.ToDouble(item.CoverPremium.Value));
                                    break;
                                case "101":
                                    resModel.PremiumBreakUpDetails.LossOfPersonalBelongingPremium = Math.Round(Convert.ToDouble(item.CoverPremium.Value));
                                    break;
                                case "106":
                                    resModel.PremiumBreakUpDetails.PAToPaidDriver = Math.Round(Convert.ToDouble(item.CoverPremium.Value));
                                    break;
                                case "91":
                                    resModel.PremiumBreakUpDetails.VoluntaryDiscount = Math.Round(Convert.ToDouble(item.CoverPremium.Value));
                                    break;
                                case "70":
                                    resModel.PremiumBreakUpDetails.NonElecAccessoriesPremium = Math.Round(Convert.ToDouble(item.CoverPremium.Value));
                                    break;
                                case "73":
                                    resModel.PremiumBreakUpDetails.PACoverToOwnDriver = Math.Round(Convert.ToDouble(item.CoverPremium.Value));
                                    break;
                                case "41":
                                    break;
                                case "42":
                                    break;
                                case "33":
                                    resModel.PremiumBreakUpDetails.ElecAccessoriesPremium = Math.Round(Convert.ToDouble(item.CoverPremium.Value));
                                    break;
                            }
                        }
                    }
                    resModel.PremiumBreakUpDetails.NetAddonPremium = Math.Round(resModel.PremiumBreakUpDetails.ZeroDepPremium
                        + resModel.PremiumBreakUpDetails.EngineProtectorPremium
                        + resModel.PremiumBreakUpDetails.CostOfConsumablesPremium
                        + resModel.PremiumBreakUpDetails.InvoicePriceCoverPremium
                        + resModel.PremiumBreakUpDetails.LossOfPersonalBelongingPremium +
                        resModel.PremiumBreakUpDetails.TyreProtect + resModel.PremiumBreakUpDetails.KeyReplacementPremium);
                    resModel.PremiumBreakUpDetails.NetTPPremium = Math.Round(Convert.ToDouble(prmCalcResponse.TotalLiabilityPremium.Value));
                    resModel.FinalPremium = Convert.ToDouble(prmCalcResponse.FinalPremium.Value);
                    resModel.PremiumBreakUpDetails.NetPremium = Math.Round(Convert.ToDouble(prmCalcResponse.NetPremium.Value));
                    resModel.PremiumBreakUpDetails.ServiceTax = Math.Round(Convert.ToDouble(prmCalcResponse.TotalTax.Value));
                    resModel.PremiumBreakUpDetails.NetODPremium = Math.Round(Convert.ToDouble(prmCalcResponse.TotalODPremium.Value));

                    //resModel.PremiumBreakUpDetails.NetPremium = Convert.ToDouble(prmCalcResponse.NetPremium.Value);
                    //resModel.PremiumBreakUpDetails.ServiceTax = Math.Round(resModel.PremiumBreakUpDetails.NetPremium * 18 / 100);
                    //resModel.FinalPremium = Math.Round(resModel.PremiumBreakUpDetails.NetPremium + resModel.PremiumBreakUpDetails.ServiceTax);
                }
            }
            catch (Exception ex)
            {
                resModel.Status = Status.Fail;
                resModel.ErrorMsg = Convert.ToString(ex.Message);
                Console.Write(Convert.ToString(ex.Message));
                LogU.WriteLog("RAHEJA >> PrivateCar >> GetQuoteResponse >> " + Convert.ToString(ex.Message));
            }
            return resModel;
        }

        public Response CreateFullQuoteRequest(Quotation model)
        {
            Response resModel = new Response();
            string PrvTypeId = string.Empty;
            try
            {
                dynamic jsonObject = string.Empty;
                string PremiumCalculatefilePath = string.Empty;
                #region PremiumCalculate
                if (model.IsODOnly)
                {
                    PremiumCalculatefilePath = Path.Combine(path, "JSON/RAHEJA/SaveMotorOd.json");
                }
                else
                {
                    PremiumCalculatefilePath = Path.Combine(path, "JSON/RAHEJA/SaveMotor.json");
                }
                //string PremiumCalculatefilePath = Path.Combine(path, "JSON/RAHEJA/SaveMotor.json");
                string PremiumCalculatejson = File.ReadAllText(PremiumCalculatefilePath);
                jsonObject = Newtonsoft.Json.JsonConvert.DeserializeObject(PremiumCalculatejson);
                var sdate = Convert.ToDateTime(model.PolicyStartDate).ToString("yyyy-MM-dd");
                if (model.PolicyType.Equals("New"))
                {
                    model.PolicyEndDate = Convert.ToDateTime(sdate).AddYears(3).AddDays(-1).ToString("yyyy-MM-dd");
                }
                else
                {
                    //model.PolicyEndDate = Convert.ToDateTime(model.PolicyStartDate).AddDays(364).ToString();
                    model.PolicyEndDate = Convert.ToDateTime(sdate).AddYears(1).AddDays(-1).ToString("yyyy-MM-dd");
                    if (model.PreviousPolicyDetails != null && model.PreviousPolicyDetails.PreviousPolicyType.ToUpper().Equals("COMPREHENSIVE"))
                    {
                        PrvTypeId = "1";
                        model.PreviousPolicyDetails.PreviousPolicyType = "COMPREHENSIVE";
                    }
                    else if (model.PreviousPolicyDetails.PreviousPolicyType.ToUpper().Equals("TP"))
                    {
                        PrvTypeId = "2";
                        model.PreviousPolicyDetails.PreviousPolicyType = "LIABILITY ONLY";
                    }
                    else
                    {
                        resModel.Status = Status.Fail;
                        resModel.ErrorMsg = "Raheja not support previous policy type";
                        return resModel;
                    }

                }
                string regino1 = string.Empty;
                string regino2 = string.Empty;
                string regino3 = string.Empty;
                string regino4 = string.Empty;
                //string rtoandcity = AndEnt.VW_RTOMASTER.Where(x => x.rtocode == rtocode).Select(x => x.rtodesc).FirstOrDefault().ToString();
                jsonObject["objClientDetails"]["MobileNumber"] = model.ClientDetails != null ? (!string.IsNullOrEmpty(model.ClientDetails.MobileNo) ? model.ClientDetails.MobileNo : "9898989850") : "9898989850";
                jsonObject["objClientDetails"]["EmailId"] = model.ClientDetails != null ? (!string.IsNullOrEmpty(model.ClientDetails.EmailId) ? model.ClientDetails.EmailId : "abc@gmail.com") : "abc@gmail.com";
                jsonObject["objClientDetails"]["ClientType"] = model.CustomerType.Equals("Individual", StringComparison.OrdinalIgnoreCase) ? "0" : "1";
                jsonObject["objVehicleDetails"]["MakeModelVarient"] = model.VehicleDetails.MakeName + "|" + model.VehicleDetails.ModelName + "|" + model.VehicleDetails.VariantName + "|" + model.VehicleDetails.CC + "CC";
                string RTOCode = string.Empty;
                string RTOName = string.Empty;
                //model.VehicleDetails.RtoId = 104;
                if (string.IsNullOrEmpty(Convert.ToString(ConfigurationManager.AppSettings["Envi"])))
                {
                    jsonObject["objVehicleDetails"]["RtoLocation"] = "Thane|MH04";
                    jsonObject["objVehicleDetails"]["Registration_Number1"] = "MH";
                    jsonObject["objVehicleDetails"]["Registration_Number2"] = "04";
                }
                else
                {
                    if (Convert.ToString(ConfigurationManager.AppSettings["Envi"]).ToUpper().Equals("UAT"))
                    {
                        jsonObject["objVehicleDetails"]["RtoLocation"] = "Thane|MH04";
                        jsonObject["objVehicleDetails"]["Registration_Number1"] = "MH";
                        jsonObject["objVehicleDetails"]["Registration_Number2"] = "04";
                    }
                    else
                    {
                        var rtocode = ap.COMPANY_WISE_RTO_MASTER.Where(x => x.companyid == 16 && x.andapp_rtoid == model.VehicleDetails.RtoId).FirstOrDefault();
                        if (rtocode != null)
                        {
                            RTOCode = rtocode.rto_loc_code;
                            RTOName = rtocode.rto_loc_name;
                        }
                        jsonObject["objVehicleDetails"]["RtoLocation"] = RTOName.Trim() + "|" + RTOCode.Trim();
                        jsonObject["objVehicleDetails"]["Registration_Number1"] = regino1.ToUpper();
                        jsonObject["objVehicleDetails"]["Registration_Number2"] = regino2;
                    }
                }

                //jsonObject["objVehicleDetails"]["RtoLocation"] = "Thane|MH04";
                jsonObject["objVehicleDetails"]["RegistrationDate"] = Convert.ToString(string.Format("{0:yyyy-MM-dd}", Convert.ToDateTime(model.VehicleDetails.RegistrationDate)));
                jsonObject["objVehicleDetails"]["ManufacturingYear"] = Convert.ToString(Convert.ToDateTime(model.VehicleDetails.ManufaturingDate).Year);
                jsonObject["objVehicleDetails"]["ManufacturingMonth"] = Convert.ToDateTime(model.VehicleDetails.ManufaturingDate).ToString("MM");
                jsonObject["objVehicleDetails"]["FuelType"] = model.VehicleDetails.Fuel;
                jsonObject["objVehicleDetails"]["ModifiedIDV"] = Convert.ToString(model.IDV);
                //jsonObject["objVehicleDetails"]["Registration_Number1"] = "MH";
                //jsonObject["objVehicleDetails"]["Registration_Number2"] = "04";

                if (model.VehicleDetails.RegistrationNumber.Length > 5)
                {
                    int rglenght = model.VehicleDetails.RegistrationNumber.Length - 4;
                    string subregno = model.VehicleDetails.RegistrationNumber.Substring(4, rglenght);
                    string part1 = model.VehicleDetails.RegistrationNumber.Substring(0, 2);
                    string part2 = model.VehicleDetails.RegistrationNumber.Substring(2, 2);
                    string part3 = string.Empty; string part4 = string.Empty;
                    if (model.VehicleDetails.RegistrationNumber.Contains("-"))
                    {
                        string[] a = subregno.Split('-');
                        part3 = Convert.ToString(a[0]);
                        part4 = Convert.ToString(a[1]);
                    }
                    else
                    {
                        part3 = subregno.Substring(0, subregno.Length - 4);
                        part4 = subregno.Substring(subregno.Length - 4);
                    }
                    jsonObject["objVehicleDetails"]["Registration_Number3"] = part3.ToUpper();
                    jsonObject["objVehicleDetails"]["Registration_Number4"] = part4;
                }
                else
                {
                    //jsonObject["objVehicleDetails"]["Registration_Number1"] = regino1.ToUpper();  //"GJ";
                    //jsonObject["objVehicleDetails"]["Registration_Number2"] = regino2.ToUpper();//"01";
                    jsonObject["objVehicleDetails"]["Registration_Number3"] = "AB";
                    jsonObject["objVehicleDetails"]["Registration_Number4"] = "1234";
                }
                if (model.IsODOnly)
                {
                    //jsonObject["objVehicleDetails"]["Registration_Number3"] = "AB";
                    //jsonObject["objVehicleDetails"]["Registration_Number4"] = "1234";
                    jsonObject["objPolicy"]["ProductCode"] = "2323";
                    jsonObject["objPolicy"]["ProductName"] = "MOTOR - PRIVATE CAR STANDALONE OD(2323)";
                    jsonObject["objPolicy"]["CoverType"] = "1668";
                    //if (model.PreviousTPPolicyDetails != null)
                    //{
                    //    DateTime PrvTPPolicyStartDate = Convert.ToDateTime(model.PreviousTPPolicyDetails.PolicyEndDate).AddYears(-3).AddDays(1);
                    //    jsonObject["objPolicy"]["TPPolicyStartDate"] = PrvTPPolicyStartDate.ToString("yyyy-MM-dd");
                    //    jsonObject["objPolicy"]["TPPolicyEndDate"] = Convert.ToDateTime(model.PreviousTPPolicyDetails.PolicyEndDate).ToString("yyyy-MM-dd");
                    //}
                    if (model.PreviousTPPolicyDetails != null)
                    {
                        var PrvTPPolicyStartDate = Convert.ToDateTime(model.PreviousTPPolicyDetails.PolicyEndDate).AddYears(-3).AddDays(1).ToString("yyyy-MM-dd");
                        //DateTime PrvTPPolicyStartDate = Convert.ToDateTime(model.PreviousTPPolicyDetails.PolicyEndDate).AddYears(-3).AddDays(1);
                        jsonObject["objPolicy"]["TPPolicyStartDate"] = Convert.ToDateTime(PrvTPPolicyStartDate).ToString("yyyy-MM-dd");
                        jsonObject["objPolicy"]["TPPolicyEndDate"] = Convert.ToDateTime(model.PreviousTPPolicyDetails.PolicyEndDate).ToString("yyyy-MM-dd");
                    }


                }
                else if (!model.PolicyType.ToUpper().Equals("NEW"))
                {
                    //jsonObject["objVehicleDetails"]["Registration_Number3"] = "AB";
                    //jsonObject["objVehicleDetails"]["Registration_Number4"] = "1234";
                    jsonObject["objPolicy"]["ProductCode"] = "2311";
                    jsonObject["objPolicy"]["ProductName"] = "MOTOR - PRIVATE CAR PACKAGE POLICY(2311)";
                    jsonObject["objPolicy"]["CoverType"] = "1471";
                    jsonObject["objPolicy"]["TPPolicyStartDate"] = string.Empty;
                    jsonObject["objPolicy"]["TPPolicyEndDate"] = string.Empty;
                }
                else
                {
                    //jsonObject["objVehicleDetails"]["Registration_Number3"] = "BA";
                    //jsonObject["objVehicleDetails"]["Registration_Number4"] = "1222";
                    jsonObject["objPolicy"]["ProductCode"] = "2367";
                    jsonObject["objPolicy"]["ProductName"] = "MOTOR PRIVATE CAR BUNDLED POLICY(2367)";
                    jsonObject["objPolicy"]["CoverType"] = "1473";

                }
                jsonObject["objPolicy"]["TraceID"] = model.CompanyWiseRefference.OrderNo;
                jsonObject["objPolicy"]["UserName"] = Convert.ToString(ConfigurationManager.AppSettings["RahUsername"]);
                jsonObject["objPolicy"]["TPSourceName"] = Convert.ToString(ConfigurationManager.AppSettings["RahTPSrcName"]);
                jsonObject["objPolicy"]["PolicyStartDate"] = Convert.ToDateTime(sdate).ToString("yyyy-MM-dd");
                jsonObject["objPolicy"]["PolicyEndDate"] = Convert.ToDateTime(model.PolicyEndDate).ToString("yyyy-MM-dd");
                jsonObject["objPolicy"]["BusinessTypeID"] = model.PolicyType.Equals("New") ? "24" : "25";

                if (model.PolicyType.Equals("New"))
                {
                    //model.Tennure = model.IsODOnly ? 152 : 102;
                    model.Tennure = model.IsODOnly ? 152 : 101;
                }
                else
                {
                    //model.Tennure = model.IsODOnly ? 151 : 101;
                    model.Tennure = model.IsODOnly ? 151 : 102;
                }
                jsonObject["objPolicy"]["Tennure"] = model.Tennure;
                JToken methods = jsonObject.SelectToken("objCovers");
                foreach (JToken signInName in methods)
                {
                    string CoverID = (string)signInName.SelectToken("CoverID");
                    var itemProperties = signInName.Children<JProperty>();
                    switch (CoverID)
                    {
                        case "9":
                            if (model.IsThirdPartyOnly)
                                itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(false);
                            break;
                        case "10":
                            if (model.IsODOnly)
                                itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(false);
                            //signInName.Parent.Remove();
                            break;
                        case "39":
                            if (model.CoverageDetails.IsFiberGlassFuelTank)
                                itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(true);
                            break;
                        case "49":
                            if (model.CoverageDetails.IsLegalLiablityPaidDriver)
                                itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(true);
                            break;
                        case "21":
                            if (model.CoverageDetails.IsBiFuelKit)
                                itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(true);
                            break;
                        case "20":
                            if (model.CoverageDetails.IsBiFuelKit)
                            {
                                itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(true);
                                dynamic myObject = new JObject();
                                myObject.PHNumericFeild1 = model.CoverageDetails.BiFuelKitAmount;
                                itemProperties.FirstOrDefault(xx => xx.Name == "objCoverDetails").Value.Replace(myObject);
                            }
                            break;
                        case "87":
                            if (model.DiscountDetails.IsTPPDRestrictedto6000)
                                itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(true);
                            break;
                        //case "76":
                        //    if (model.CoverageDetails.NoOfLLPaidDriver > 0)
                        //    {
                        //        itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(true);
                        //        dynamic myObjectLL = new JObject();
                        //        myObjectLL.PHNumericFeild1 = model.CoverageDetails.NoOfLLPaidDriver;
                        //        itemProperties.FirstOrDefault(xx => xx.Name == "objCoverDetails").Value.Replace(myObjectLL);
                        //    }
                        //    break;
                        //case "94":
                        //    if (model.CoverageDetails.IsPACoverUnnamedPerson)
                        //    {
                        //        itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(true);
                        //        dynamic myObjectPA = new JObject();
                        //        myObjectPA.PHNumericFeild1 = model.CoverageDetails.PACoverUnnamedPersonAmount;
                        //        itemProperties.FirstOrDefault(xx
                        //            => xx.Name == "objCoverDetails").Value.Replace(myObjectPA);
                        //    }
                        //    break;
                        case "73":
                            if (model.CoverageDetails.IsPACoverForOwnerDriver)
                            {
                                //itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(false);
                                dynamic myObjectPAOwner = new JObject();
                                myObjectPAOwner.PHintFeild1 = Convert.ToString(CalculateAge(Convert.ToDateTime(model.NomineeDateOfBirth)));
                                myObjectPAOwner.PHVarcharFeild1 = model.NomineeName;
                                myObjectPAOwner.PHVarcharFeild2 = model.NomineeRelationShip;
                                myObjectPAOwner.PHVarcharFeild2 = "1547";
                                myObjectPAOwner.PHNumericFeild2 = "100000";
                                myObjectPAOwner.PHNumericFeild1 = "1";
                                if (!string.IsNullOrEmpty(model.AppointeeName))
                                {
                                    myObjectPAOwner.PHVarcharFeild4 = model.AppointeeName;
                                    myObjectPAOwner.PHVarcharFeild5 = model.AppointeeRelationShip;
                                    myObjectPAOwner.PHVarcharFeild5 = "1134";
                                    myObjectPAOwner.PHBooleanField1 = "false";
                                    myObjectPAOwner.PHBooleanField2 = "false";
                                }
                                else
                                {
                                    //myObjectPAOwner.PHNumericFeild1 = string.Empty;
                                    myObjectPAOwner.PHVarcharFeild4 = string.Empty;
                                    myObjectPAOwner.PHVarcharFeild5 = string.Empty;
                                    myObjectPAOwner.PHBooleanField1 = "false";
                                    myObjectPAOwner.PHBooleanField2 = "false";
                                }
                                itemProperties.FirstOrDefault(xx => xx.Name == "objCoverDetails").Value.Replace(myObjectPAOwner);
                            }
                            else
                            {
                                itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(false);
                                dynamic myObjectPAOwner = new JObject();
                                myObjectPAOwner.PHintFeild1 = string.Empty;
                                myObjectPAOwner.PHVarcharFeild1 = string.Empty;
                                myObjectPAOwner.PHVarcharFeild2 = string.Empty;
                                myObjectPAOwner.PHVarcharFeild2 = string.Empty;
                                myObjectPAOwner.PHNumericFeild2 = string.Empty;
                                myObjectPAOwner.PHNumericFeild1 = string.Empty;
                                myObjectPAOwner.PHVarcharFeild4 = string.Empty;
                                myObjectPAOwner.PHVarcharFeild5 = string.Empty;
                                myObjectPAOwner.PHBooleanField1 = false;
                                myObjectPAOwner.PHBooleanField2 = true;
                                itemProperties.FirstOrDefault(xx
                                    => xx.Name == "objCoverDetails").Value.Replace(myObjectPAOwner);
                            }
                            break;
                        //case "105":
                        //    if (model.CoverageDetails.IsEmployeeLiability)
                        //        itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(true);
                        //    break;
                        case "91":
                            if (model.DiscountDetails.VoluntaryExcessAmount > 0)
                            {
                                itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(true);
                                dynamic myObjectVoluntary = new JObject();
                                myObjectVoluntary.PHNumericFeild1 = model.DiscountDetails.VoluntaryExcessAmount;
                                itemProperties.FirstOrDefault(xx => xx.Name == "objCoverDetails").Value.Replace(myObjectVoluntary);
                            }
                            break;
                        case "33":
                            if (model.CoverageDetails.IsElectricalAccessories)
                            {
                                itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(true);
                                dynamic myObjectele = new JObject();
                                myObjectele.PHNumericFeild1 = model.CoverageDetails.SIElectricalAccessories / 2;
                                myObjectele.PHVarcharFeild1 = "Test";
                                myObjectele.PHNumericFeild2 = model.CoverageDetails.SIElectricalAccessories / 2;
                                myObjectele.PHVarcharFeild2 = "Test";
                                itemProperties.FirstOrDefault(xx => xx.Name == "objCoverDetails").Value.Replace(myObjectele);
                            }
                            break;
                        case "70":
                            if (model.CoverageDetails.IsNonElectricalAccessories)
                            {
                                itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(true);
                                dynamic myObjectele = new JObject();
                                myObjectele.PHNumericFeild1 = model.CoverageDetails.SINonElectricalAccessories / 2;
                                myObjectele.PHVarcharFeild1 = "Test";
                                myObjectele.PHNumericFeild2 = model.CoverageDetails.SINonElectricalAccessories / 2;
                                myObjectele.PHVarcharFeild2 = "Test";
                                itemProperties.FirstOrDefault(xx => xx.Name == "objCoverDetails").Value.Replace(myObjectele);
                            }
                            break;
                        case "97":
                            if (model.AddonCover.IsZeroDeperation)
                            {
                                if (model.PolicyType.Equals("New"))
                                {
                                    dynamic myObjectelezero = new JObject();
                                    //myObjectelezero.PHNumericFeild1 = model.CoverageDetails.SINonElectricalAccessories / 2;
                                    myObjectelezero.PHVarcharFeild1 = "2";
                                    // myObjectelezero.PHNumericFeild2 = model.CoverageDetails.SINonElectricalAccessories / 2;
                                    myObjectelezero.PHVarcharFeild2 = "";
                                    itemProperties.FirstOrDefault(xx => xx.Name == "objCoverDetails").Value.Replace(myObjectelezero);
                                    itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(true);
                                }
                                else
                                {
                                    dynamic myObjectelezero = new JObject();
                                    //myObjectelezero.PHNumericFeild1 = model.CoverageDetails.SINonElectricalAccessories / 2;
                                    myObjectelezero.PHVarcharFeild1 = "2";
                                    // myObjectelezero.PHNumericFeild2 = model.CoverageDetails.SINonElectricalAccessories / 2;
                                    myObjectelezero.PHVarcharFeild2 = "Yes";
                                    itemProperties.FirstOrDefault(xx => xx.Name == "objCoverDetails").Value.Replace(myObjectelezero);
                                    itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(true);
                                    //itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(false);
                                }
                            }
                            break;
                        case "37":
                            if (model.AddonCover.IsEngineProtector)
                                itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(true);
                            break;
                        case "24":
                            if (model.AddonCover.IsConsumables)
                                itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(true);
                            break;
                        case "80":
                            if (model.AddonCover.IsReturntoInvoice)
                                itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(true);
                            break;
                        case "101":
                            if (model.AddonCover.IsLossofpersonalBelonging)
                                itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(true);
                            break;
                        case "104":
                            if (model.AddonCover.IsTyreCover)
                                itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(true);
                            break;

                        default:
                            break;
                    }
                }
                jsonObject["objPolicy"]["QuoteID"] = model.CompanyWiseRefference.QuoteId;
                jsonObject["objPolicy"]["QuoteNo"] = model.CompanyWiseRefference.QuoteNo;

                string requestjson1 = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObject, Newtonsoft.Json.Formatting.Indented);

                //string requestjson2 = Newtonsoft.Json.JsonConvert.SerializeObject(requestjson1, Newtonsoft.Json.Formatting.Indented);
                //if (model.IsODOnly)
                //{
                //    requestjson1 = "\"objPolicy\":{\"QuoteID\":\"" + (model.PreviousPolicyDetails.PreviousPolicyType) +
                //          "\",\"QuoteNo\":\"" + Convert.ToString(string.Format("{0:yyyy-MM-dd}", Convert.ToDateTime(model.PreviousPolicyDetails.PreviousPolicyStartDate))) +
                //          "\",\"TraceID\":\"" + Convert.ToString(string.Format("{0:yyyy-MM-dd}", Convert.ToDateTime(model.PreviousPolicyDetails.PreviousPolicyEndDate))) +
                //          "\",\"UserName\":\"2311\",\"TPSourceName\":\"" + model.PreviousPolicyDetails.PreviousCompanyName + "\",\"SessionID\":\"" +
                //          model.PreviousPolicyDetails.PreviousNcbPercentage + "\",\"ProductCode\":\"" + (model.PreviousPolicyDetails.IsPreviousInsuranceClaimed ? "1" : "2") +
                //          "\",\"ProductName\":\"" + "2" +
                //          "\",\"PolicyStartDate\":\"COMPREHENSIVE\",\"PolicyEndDate\":\"" + model.CurrentNcb +
                //           "\",\"BusinessTypeID\":\"COMPREHENSIVE\",\"CoverType\":\"" + model.CurrentNcb +
                //               "\",\"TPPolicyStartDate\":\"COMPREHENSIVE\",\"TPPolicyEndDate\":\"" + model.CurrentNcb +
                //               "\",\"Tennure\":\"COMPREHENSIVE\"\"}";
                //}
                if (!model.PolicyType.ToUpper().Equals("NEW"))
                {
                    if (model.PreviousPolicyDetails != null)
                    {
                        var PreviousPolicyStartDate = Convert.ToDateTime(model.PreviousPolicyDetails.PreviousPolicyEndDate).AddYears(-1).AddDays(1).ToString("yyyy-MM-dd");
                        // model.PreviousPolicyDetails.PreviousPolicyStartDate = Convert.ToDateTime(model.PreviousPolicyDetails.PreviousPolicyEndDate).AddYears(-1).AddDays(1).ToString();

                        // need dynamic from db
                        model.PreviousPolicyDetails.CompanyId = Convert.ToInt16(GetRahejaPrevCompany(Convert.ToInt16(model.PreviousPolicyDetails.CompanyId)));
                        //model.PreviousPolicyDetails.CompanyId = model.PreviousPolicyDetails.CompanyId;
                        requestjson1 += "\"objPreviousInsurance\":{\"PrevPolicyType\":\"" + PrvTypeId +
                               "\",\"PrevPolicyStartDate\":\"" + Convert.ToDateTime(PreviousPolicyStartDate).ToString("yyyy-MM-dd") +
                               "\",\"PrevPolicyEndDate\":\"" + Convert.ToDateTime(model.PreviousPolicyDetails.PreviousPolicyEndDate).ToString("yyyy-MM-dd") +
                               "\",\"ProductCode\":\"2311\",\"PrevInsuranceCompanyID\":\"" + model.PreviousPolicyDetails.CompanyId + "\",\"PrevNCB\":\"" +
                               model.PreviousPolicyDetails.PreviousNcbPercentage + "\",\"IsClaimedLastYear\":\"" + (model.PreviousPolicyDetails.IsPreviousInsuranceClaimed ? "1" : "2") +
                               //"\",\"NatureOfLoss\":\"" + "2" +"\",\"NatureOfLoss\":\"" + "2" +
                               "\",\"NatureOfLoss\":\"" + (model.PreviousPolicyDetails.IsPreviousInsuranceClaimed ? PrvTypeId : string.Empty) +
                               "\",\"prevPolicyCoverType\":\"" + model.PreviousPolicyDetails.PreviousPolicyType.ToUpper() + "\",\"CurrentNCBHidden\":\"" + model.CurrentNcb +
                               "\"}";
                    }
                }
                var prmCalcResponse = Webservicecall(requestjson1, Convert.ToString(ConfigurationManager.AppSettings["RahBaseUrl"]), Convert.ToString(ConfigurationManager.AppSettings["RahSaveMotor"]));
                //var prmCalcResponse = Webservicecall(requestjson1, "http://52.172.5.3:8423/api/MotorAPI", "http://52.172.5.3:8423/api/MotorAPI/SaveMotorQuotation");
                if (prmCalcResponse.StatusCode == HttpStatusCode.OK)
                {
                    resModel = GetProposalResponse((JObject)JsonConvert.DeserializeObject(Convert.ToString(prmCalcResponse.Content)));
                    LogU.WriteLog("RAHEJA >> PrivateCar >> GetSaveMotorQuoteRequest >> " + requestjson1);
                    LogU.WriteLog("RAHEJA >> PrivateCar >> GetSaveMotorQuoteResponse >> " + Convert.ToString(prmCalcResponse.Content));
                }
                else
                {
                    resModel.Status = Status.Fail;
                    resModel.ErrorMsg = "Getting an error from Save Motor Quote service.";
                    LogU.WriteLog("RAHEJA >> PrivateCar >> GetSaveMotorQuoteRequest >> " + resModel.ErrorMsg);
                    return resModel;
                }
                #endregion
                resModel.IDV = model.IDV;
                resModel.SC = model.VehicleDetails.SC;
                resModel.CC = model.VehicleDetails.CC;
                resModel.FuelType = model.VehicleDetails.Fuel;
            }
            catch (Exception ex)
            {
                resModel.Status = Status.Fail;
                resModel.ErrorMsg = Convert.ToString(ex.Message);
                Console.Write(Convert.ToString(ex.Message));
                LogU.WriteLog("RAHEJA >> PrivateCar >> CreateFullQuoteRequest >> " + Convert.ToString(ex.Message));
            }
            return resModel;
        }

        public Response GetProposalResponse(dynamic prmCalcResponse)
        {
            Response resModel = new Response();
            resModel.PremiumBreakUpDetails = new PremiumBreakUpDetails();
            resModel.CompanyWiseRefference = new CompanyWiseRefference();
            try
            {
                resModel.ErrorMsg = prmCalcResponse.objFault.ErrorMessage;
                if (!string.IsNullOrEmpty(resModel.ErrorMsg))
                {
                    resModel.Status = Status.Fail;
                    return resModel;
                }
                else
                {
                    resModel.Status = Status.Success;
                    //string q = prmCalcResponse.ChildrenTokens[9]["QuoteID"];

                    resModel.PremiumBreakUpDetails.NetTPPremium = Convert.ToDouble(prmCalcResponse.TotalLiabilityPremium.Value);
                    resModel.FinalPremium = Convert.ToDouble(prmCalcResponse.FinalPremium.Value);
                    resModel.PremiumBreakUpDetails.NetPremium = Convert.ToDouble(prmCalcResponse.NetPremium.Value);
                    resModel.PremiumBreakUpDetails.ServiceTax = Convert.ToDouble(prmCalcResponse.TotalTax.Value);
                    //resModel.PremiumBreakUpDetails.NCBDiscount = Convert.ToDouble(prmCalcResponse.NCBPremium.Value);
                    resModel.PremiumBreakUpDetails.NetODPremium = Convert.ToDouble(prmCalcResponse.TotalODPremium.Value);
                    ////resModel.PremiumBreakUpDetails.ServiceTax = Convert.ToDouble(prmCalcResponse.TotalTax.Value);

                    if (prmCalcResponse.objPolicy != null)
                    {
                        resModel.CompanyWiseRefference.QuoteId = prmCalcResponse.objPolicy.QuoteID.Value;
                        resModel.CompanyWiseRefference.QuoteNo = prmCalcResponse.objPolicy.QuoteNo.Value;
                        resModel.CompanyWiseRefference.applicationId = prmCalcResponse.objPolicy.Policyid.Value;
                        resModel.PlanName = prmCalcResponse.objPolicy.ProductName.Value;
                        resModel.CompanyWiseRefference.OrderNo = prmCalcResponse.objPolicy.TraceID.Value;
                    }
                    var addon = prmCalcResponse.lstCoverResponce;
                    if (addon != null)
                    {
                        foreach (var item in addon)
                        {
                            string coverId = item.CoverID.Value;
                            switch (coverId)
                            {
                                case "9":
                                    resModel.PremiumBreakUpDetails.BasicODPremium = Convert.ToDouble(item.CoverPremium.Value);
                                    break;
                                case "10":
                                    resModel.PremiumBreakUpDetails.BasicThirdPartyLiability = Convert.ToDouble(item.CoverPremium.Value);
                                    break;
                                case "24":
                                    resModel.PremiumBreakUpDetails.CostOfConsumablesPremium = Convert.ToDouble(item.CoverPremium.Value);
                                    break;
                                case "37":
                                    resModel.PremiumBreakUpDetails.EngineProtectorPremium = Convert.ToDouble(item.CoverPremium.Value);
                                    break;
                                case "39":
                                    resModel.PremiumBreakUpDetails.FiberGlassTankPremium = Convert.ToDouble(item.CoverPremium.Value);
                                    break;
                                case "49":
                                    resModel.PremiumBreakUpDetails.LLToPaidDriver = Convert.ToDouble(item.CoverPremium.Value);
                                    break;
                                case "21":
                                    resModel.PremiumBreakUpDetails.TPCNGLPGPremium = Convert.ToDouble(item.CoverPremium.Value);
                                    break;
                                case "20":
                                    resModel.PremiumBreakUpDetails.CNGLPGKitPremium = Convert.ToDouble(item.CoverPremium.Value);
                                    break;
                                case "87":
                                    resModel.PremiumBreakUpDetails.RestrictLiability = Convert.ToDouble(item.CoverPremium.Value);
                                    break;
                                case "80":
                                    resModel.PremiumBreakUpDetails.InvoicePriceCoverPremium = Convert.ToDouble(item.CoverPremium.Value);
                                    break;
                                case "97":
                                    resModel.PremiumBreakUpDetails.ZeroDepPremium = Convert.ToDouble(item.CoverPremium.Value);
                                    break;
                                //case "100":
                                //    resModel.PremiumBreakUpDetails.KeyReplacementPremium = Convert.ToDouble(item.CoverPremium.Value);
                                //    break;
                                case "94":
                                    resModel.PremiumBreakUpDetails.PACoverToUnNamedPerson = Convert.ToDouble(item.CoverPremium.Value);
                                    break;
                                case "104":
                                    resModel.PremiumBreakUpDetails.TyreProtect = Convert.ToDouble(item.CoverPremium.Value);
                                    break;
                                case "101":
                                    resModel.PremiumBreakUpDetails.LossOfPersonalBelongingPremium = Convert.ToDouble(item.CoverPremium.Value);
                                    break;
                                case "106":
                                    resModel.PremiumBreakUpDetails.PAToPaidDriver = Convert.ToDouble(item.CoverPremium.Value);
                                    break;
                                case "91":
                                    resModel.PremiumBreakUpDetails.VoluntaryDiscount = Convert.ToDouble(item.CoverPremium.Value);
                                    break;
                                case "70":
                                    resModel.PremiumBreakUpDetails.NonElecAccessoriesPremium = Convert.ToDouble(item.CoverPremium.Value);
                                    break;
                                case "73":
                                    resModel.PremiumBreakUpDetails.PACoverToOwnDriver = Convert.ToDouble(item.CoverPremium.Value);
                                    break;
                                case "41":
                                    break;
                                case "42":
                                    break;
                                case "33":
                                    resModel.PremiumBreakUpDetails.ElecAccessoriesPremium = Convert.ToDouble(item.CoverPremium.Value);
                                    break;
                            }
                        }
                    }
                    resModel.PremiumBreakUpDetails.NetAddonPremium = resModel.PremiumBreakUpDetails.ZeroDepPremium
                         + resModel.PremiumBreakUpDetails.EngineProtectorPremium
                         + resModel.PremiumBreakUpDetails.CostOfConsumablesPremium
                         + resModel.PremiumBreakUpDetails.InvoicePriceCoverPremium
                         + resModel.PremiumBreakUpDetails.LossOfPersonalBelongingPremium +
                         resModel.PremiumBreakUpDetails.TyreProtect + resModel.PremiumBreakUpDetails.KeyReplacementPremium;
                    //resModel.PremiumBreakUpDetails.NetPremium = Convert.ToDouble(prmCalcResponse.NetPremium.Value) + resModel.PremiumBreakUpDetails.NetAddonPremium;
                    //resModel.PremiumBreakUpDetails.ServiceTax = Math.Round(resModel.PremiumBreakUpDetails.NetPremium * 18 / 100);
                    //resModel.FinalPremium = Math.Round(resModel.PremiumBreakUpDetails.NetPremium + resModel.PremiumBreakUpDetails.ServiceTax);

                }
            }
            catch (Exception ex)
            {
                resModel.Status = Status.Fail;
                resModel.ErrorMsg = Convert.ToString(ex.Message);
                Console.Write(Convert.ToString(ex.Message));
                LogU.WriteLog("RAHEJA >> PrivateCar >> GetProposalResponse >> " + Convert.ToString(ex.Message));
            }
            return resModel;
        }

        //public Response CreateFullQuoteRequest(Quotation model)
        //{
        //    Response resModel = new Response();
        //    try
        //    {
        //        dynamic jsonObject = string.Empty;

        //        #region PremiumCalculate

        //        string PremiumCalculatefilePath = Path.Combine(path, "JSON/RAHEJA/SaveMotor.json");
        //        string PremiumCalculatejson = File.ReadAllText(PremiumCalculatefilePath);
        //        jsonObject = Newtonsoft.Json.JsonConvert.DeserializeObject(PremiumCalculatejson);
        //        //string rtoandcity = AndEnt.VW_RTOMASTER.Where(x => x.rtocode == rtocode).Select(x => x.rtodesc).FirstOrDefault().ToString();
        //        jsonObject["objClientDetails"]["MobileNumber"] = model.ClientDetails != null ? (!string.IsNullOrEmpty(model.ClientDetails.MobileNo) ? model.ClientDetails.MobileNo : "9898989850") : "9898989850";
        //        jsonObject["objClientDetails"]["EmailId"] = model.ClientDetails != null ? (!string.IsNullOrEmpty(model.ClientDetails.EmailId) ? model.ClientDetails.EmailId : "abc@gmail.com") : "abc@gmail.com";
        //        jsonObject["objClientDetails"]["ClientType"] = model.CustomerType.Equals("Individual", StringComparison.OrdinalIgnoreCase) ? "0" : "1";
        //        jsonObject["objVehicleDetails"]["MakeModelVarient"] = model.VehicleDetails.MakeName + "|" + model.VehicleDetails.ModelName + "|" + model.VehicleDetails.VariantName + "|" + model.VehicleDetails.CC + "CC";
        //        jsonObject["objVehicleDetails"]["RtoLocation"] = "AHMEDABAD|GJ01";
        //        jsonObject["objVehicleDetails"]["RegistrationDate"] = Convert.ToString(string.Format("{0:yyyy-MM-dd}", Convert.ToDateTime(model.VehicleDetails.RegistrationDate)));
        //        jsonObject["objVehicleDetails"]["ManufacturingYear"] = Convert.ToString(Convert.ToDateTime(model.VehicleDetails.ManufaturingDate).Year);
        //        jsonObject["objVehicleDetails"]["ManufacturingMonth"] = Convert.ToDateTime(model.VehicleDetails.ManufaturingDate).ToString("MM");
        //        jsonObject["objVehicleDetails"]["FuelType"] = model.VehicleDetails.Fuel;
        //        jsonObject["objVehicleDetails"]["ModifiedIDV"] = resModel.IDV;
        //        jsonObject["objVehicleDetails"]["Registration_Number1"] = "GJ";
        //        jsonObject["objVehicleDetails"]["Registration_Number2"] = "01";
        //        if (model.IsODOnly)
        //        {
        //            jsonObject["objVehicleDetails"]["Registration_Number3"] = "AB";
        //            jsonObject["objVehicleDetails"]["Registration_Number4"] = "1234";
        //            jsonObject["objPolicy"]["ProductCode"] = "2323";
        //            jsonObject["objPolicy"]["ProductName"] = "MOTOR - PRIVATE CAR STANDALONE OD(2323)";
        //            jsonObject["objPolicy"]["CoverType"] = "1668";
        //            if (model.PreviousTPPolicyDetails != null)
        //            {
        //                DateTime PrvTPPolicyStartDate = Convert.ToDateTime(model.PreviousTPPolicyDetails.PolicyEndDate).AddYears(-3).AddDays(1);
        //                jsonObject["objPolicy"]["TPPolicyStartDate"] = PrvTPPolicyStartDate.ToString("yyyy-MM-dd");
        //                jsonObject["objPolicy"]["TPPolicyEndDate"] = Convert.ToDateTime(model.PreviousTPPolicyDetails.PolicyEndDate).ToString("yyyy-MM-dd");
        //            }

        //        }
        //        else if (!model.PolicyType.ToUpper().Equals("NEW"))
        //        {
        //            jsonObject["objVehicleDetails"]["Registration_Number3"] = "AB";
        //            jsonObject["objVehicleDetails"]["Registration_Number4"] = "1234";
        //            jsonObject["objPolicy"]["ProductCode"] = "2311";
        //            jsonObject["objPolicy"]["ProductName"] = "MOTOR - PRIVATE CAR PACKAGE POLICY(2311)";
        //            jsonObject["objPolicy"]["CoverType"] = "1471";
        //            jsonObject["objPolicy"]["TPPolicyStartDate"].remove();
        //        }
        //        else
        //        {
        //            jsonObject["objVehicleDetails"]["Registration_Number3"] = "BA";
        //            jsonObject["objVehicleDetails"]["Registration_Number4"] = "1222";
        //            jsonObject["objPolicy"]["ProductCode"] = "2367";
        //            jsonObject["objPolicy"]["ProductName"] = "MOTOR PRIVATE CAR BUNDLED POLICY(2367)";
        //            jsonObject["objPolicy"]["CoverType"] = "1473";

        //        }
        //        jsonObject["objPolicy"]["TraceID"] = GetTraceID();
        //        jsonObject["objPolicy"]["PolicyStartDate"] = Convert.ToDateTime(model.PolicyStartDate).ToString("yyyy-MM-dd");
        //        jsonObject["objPolicy"]["PolicyEndDate"] = Convert.ToDateTime(model.PolicyEndDate).ToString("yyyy-MM-dd");
        //        jsonObject["objPolicy"]["BusinessTypeID"] = model.PolicyType.Equals("New") ? "24" : "25";

        //        if (model.PolicyType.Equals("New"))
        //        {
        //            //model.Tennure = model.IsODOnly ? 152 : 102;
        //            model.Tennure = model.IsODOnly ? 152 : 101;
        //        }
        //        else
        //        {
        //            //model.Tennure = model.IsODOnly ? 151 : 101;
        //            model.Tennure = model.IsODOnly ? 151 : 102;
        //        }
        //        //jsonObject["objPolicy"]["Tennure"] = model.Tennure;
        //        //jsonObject["objPolicy"]["TraceID"] = model.CompanyWiseRefference.OrderNo;
        //        //jsonObject["objPolicy"]["PolicyStartDate"] = Convert.ToString(string.Format("{0:yyyy-MM-dd}", Convert.ToDateTime(model.PolicyStartDate)));
        //        //jsonObject["objPolicy"]["PolicyEndDate"] = Convert.ToString(string.Format("{0:yyyy-MM-dd}", Convert.ToDateTime(model.PolicyEndDate)));
        //        //jsonObject["objPolicy"]["BusinessTypeID"] = model.PolicyType.Equals("New") ? "24" : "25";
        //        //if (model.PolicyType.Equals("New"))
        //        //{
        //        //    model.Tennure = model.IsODOnly ? 152 : 102;
        //        //}
        //        //else
        //        //{
        //        //    model.Tennure = model.IsODOnly ? 151 : 101;
        //        //}
        //        //jsonObject["objPolicy"]["Tennure"] = model.Tennure;
        //        JToken methods = jsonObject.SelectToken("objCovers");
        //        foreach (JToken signInName in methods)
        //        {
        //            string CoverID = (string)signInName.SelectToken("CoverID");
        //            var itemProperties = signInName.Children<JProperty>();
        //            switch (CoverID)
        //            {
        //                case "9":
        //                    if (model.IsThirdPartyOnly)
        //                        itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(false);
        //                    break;
        //                case "10":
        //                    if (model.IsODOnly)
        //                        itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(false);
        //                    break;
        //                case "39":
        //                    if (model.CoverageDetails.IsFiberGlassFuelTank)
        //                        itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(true);
        //                    break;
        //                case "49":
        //                    if (model.CoverageDetails.IsLegalLiablityPaidDriver)
        //                        itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(true);
        //                    break;
        //                case "21":
        //                    if (model.CoverageDetails.IsBiFuelKit)
        //                        itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(true);
        //                    break;
        //                case "20":
        //                    if (model.CoverageDetails.IsBiFuelKit)
        //                    {
        //                        itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(true);
        //                        dynamic myObject = new JObject();
        //                        myObject.PHNumericFeild1 = model.CoverageDetails.BiFuelKitAmount;
        //                        itemProperties.FirstOrDefault(xx => xx.Name == "objCoverDetails").Value.Replace(myObject);
        //                    }
        //                    break;
        //                case "87":
        //                    if (model.DiscountDetails.IsTPPDRestrictedto6000)
        //                        itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(true);
        //                    break;
        //                case "76":
        //                    if (model.CoverageDetails.NoOfLLPaidDriver > 0)
        //                    {
        //                        itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(true);
        //                        dynamic myObjectLL = new JObject();
        //                        myObjectLL.PHNumericFeild1 = model.CoverageDetails.NoOfLLPaidDriver;
        //                        itemProperties.FirstOrDefault(xx => xx.Name == "objCoverDetails").Value.Replace(myObjectLL);
        //                    }
        //                    break;
        //                case "94":
        //                    if (model.CoverageDetails.IsPACoverUnnamedPerson)
        //                    {
        //                        itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(true);
        //                        dynamic myObjectPA = new JObject();
        //                        myObjectPA.PHNumericFeild1 = model.CoverageDetails.PACoverUnnamedPersonAmount;
        //                        itemProperties.FirstOrDefault(xx => xx.Name == "objCoverDetails").Value.Replace(myObjectPA);
        //                    }
        //                    break;
        //                case "106":
        //                    if (model.CoverageDetails.IsPACoverPaidDriver)
        //                        itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(true);
        //                    break;
        //                case "91":
        //                    if (model.DiscountDetails.VoluntaryExcessAmount > 0)
        //                    {
        //                        itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(true);
        //                        dynamic myObjectVoluntary = new JObject();
        //                        myObjectVoluntary.PHNumericFeild1 = model.DiscountDetails.VoluntaryExcessAmount;
        //                        itemProperties.FirstOrDefault(xx => xx.Name == "objCoverDetails").Value.Replace(myObjectVoluntary);
        //                    }
        //                    break;
        //                case "33":
        //                    if (model.CoverageDetails.IsElectricalAccessories)
        //                    {
        //                        itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(true);
        //                        dynamic myObjectele = new JObject();
        //                        myObjectele.PHNumericFeild1 = model.CoverageDetails.SIElectricalAccessories / 2;
        //                        myObjectele.PHVarcharFeild1 = "Test";
        //                        myObjectele.PHNumericFeild2 = model.CoverageDetails.SIElectricalAccessories / 2;
        //                        myObjectele.PHVarcharFeild2 = "Test";
        //                        itemProperties.FirstOrDefault(xx => xx.Name == "objCoverDetails").Value.Replace(myObjectele);
        //                    }
        //                    break;
        //                case "70":
        //                    if (model.CoverageDetails.IsNonElectricalAccessories)
        //                    {
        //                        itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(true);
        //                        dynamic myObjectele = new JObject();
        //                        myObjectele.PHNumericFeild1 = model.CoverageDetails.SINonElectricalAccessories / 2;
        //                        myObjectele.PHVarcharFeild1 = "Test";
        //                        myObjectele.PHNumericFeild2 = model.CoverageDetails.SINonElectricalAccessories / 2;
        //                        myObjectele.PHVarcharFeild2 = "Test";
        //                        itemProperties.FirstOrDefault(xx => xx.Name == "objCoverDetails").Value.Replace(myObjectele);
        //                    }
        //                    break;
        //                case "37":
        //                    if (model.AddonCover.IsEngineProtector)
        //                        itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(true);
        //                    break;
        //                case "24":
        //                    if (model.AddonCover.IsConsumables)
        //                        itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(true);
        //                    break;
        //                case "80":
        //                    if (model.AddonCover.IsReturntoInvoice)
        //                        itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(true);
        //                    break;
        //                case "97":
        //                    if (model.AddonCover.IsZeroDeperation)
        //                        itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(true);
        //                    break;
        //                case "101":
        //                    if (model.AddonCover.IsLossofpersonalBelonging)
        //                        itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(true);
        //                    break;
        //                case "104":
        //                    if (model.AddonCover.IsTyreCover)
        //                        itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(true);
        //                    break;
        //                default:
        //                    break;
        //            }
        //        }
        //        jsonObject["objPolicy"]["QuoteID"] = resModel.CompanyWiseRefference.QuoteId;
        //        jsonObject["objPolicy"]["QuoteNo"] = resModel.CompanyWiseRefference.QuoteNo;
        //        //jsonObject["objPolicy"].Add("QuoteID", resModel.CompanyWiseRefference.QuoteId);
        //        // jsonObject["objPolicy"].Add("QuoteNo", resModel.CompanyWiseRefference.QuoteNo);
        //        string requestjson1 = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObject, Newtonsoft.Json.Formatting.Indented);
        //        if (model.PreviousPolicyDetails != null)
        //        {
        //            requestjson1 += "\"objPreviousInsurance\":{\"PrevPolicyType\":\"" + (model.PreviousPolicyDetails.PreviousPolicyType) +
        //                   "\",\"PrevPolicyStartDate\":\"" + Convert.ToString(string.Format("{0:yyyy-MM-dd}", Convert.ToDateTime(model.PreviousPolicyDetails.PreviousPolicyStartDate))) +
        //                   "\",\"PrevPolicyEndDate\":\"" + Convert.ToString(string.Format("{0:yyyy-MM-dd}", Convert.ToDateTime(model.PreviousPolicyDetails.PreviousPolicyEndDate))) +
        //                   "\",\"ProductCode\":\"2311\",\"PrevInsuranceCompanyID\":\"" + model.PreviousPolicyDetails.PreviousCompanyName + "\",\"PrevNCB\":\"" +
        //                   model.PreviousPolicyDetails.PreviousNcbPercentage + "\",\"IsClaimedLastYear\":\"" + (model.PreviousPolicyDetails.IsPreviousInsuranceClaimed ? "1" : "2") +
        //                   "\",\"NatureOfLoss\":\"" + "2" +
        //                   "\",\"prevPolicyCoverType\":\"COMPREHENSIVE\",\"CurrentNCBHidden\":\"" + model.CurrentNcb +
        //                   "\"}";
        //        }
        //        var prmCalcResponse = Webservicecall(requestjson1, "http://52.172.5.3:8423/api/MotorAPI", "http://52.172.5.3:8423/api/MotorAPI/SaveMotorQuotation");
        //        if (prmCalcResponse.StatusCode == HttpStatusCode.OK)
        //        {
        //            resModel = GetQuoteResponse((JObject)JsonConvert.DeserializeObject(Convert.ToString(prmCalcResponse.Content)));
        //        }
        //        #endregion
        //        resModel.IDV = model.IDV;
        //        resModel.SC = model.VehicleDetails.SC;
        //        resModel.CC = model.VehicleDetails.CC;
        //        resModel.FuelType = model.VehicleDetails.Fuel;

        //    }
        //    catch (Exception ex)
        //    {
        //        resModel.Status = Status.Fail;
        //        resModel.ErrorMsg = Convert.ToString(ex.Message);
        //        Console.Write(Convert.ToString(ex.Message));
        //        LogU.WriteLog("RAHEJA >> PrivateCar >> CreateFullQuoteRequest >> " + Convert.ToString(ex.Message));
        //    }
        //    return resModel;
        //}

        public Response GetProposalRequest(Quotation model)
        {
            string PrvTypeId = string.Empty;
            string ProposalPath = string.Empty;
            Response ProRes = new Response();
            ProRes = CreateFullQuoteRequest(model);
            if (string.IsNullOrEmpty(ProRes.ErrorMsg))
            {
                var sdate = Convert.ToDateTime(model.PolicyStartDate).ToString("yyyy-MM-dd");
                if (model.PolicyType.Equals("New"))
                {
                    model.PolicyEndDate = Convert.ToDateTime(sdate).AddYears(3).AddDays(-1).ToString("yyyy-MM-dd");
                }
                else
                {
                    //model.PolicyEndDate = Convert.ToDateTime(model.PolicyStartDate).AddDays(364).ToString();
                    model.PolicyEndDate = Convert.ToDateTime(sdate).AddYears(1).AddDays(-1).ToString("yyyy-MM-dd");
                    if (model.PreviousPolicyDetails != null && model.PreviousPolicyDetails.PreviousPolicyType.ToUpper().Equals("COMPREHENSIVE"))
                    {
                        PrvTypeId = "1";
                        model.PreviousPolicyDetails.PreviousPolicyType = "COMPREHENSIVE";
                    }
                    else if (model.PreviousPolicyDetails.PreviousPolicyType.ToUpper().Equals("TP"))
                    {
                        PrvTypeId = "2";
                        model.PreviousPolicyDetails.PreviousPolicyType = "LIABILITY ONLY";
                    }
                    else
                    {
                        ProRes.Status = Status.Fail;
                        ProRes.ErrorMsg = "Raheja not support previous policy type";
                        LogU.WriteLog("RAHEJA >> PrivateCar >> GetProposalRequest >> " + ProRes.ErrorMsg);
                        return ProRes;
                    }
                }
                try
                {
                    if (model.IsODOnly)
                    {
                        ProposalPath = Path.Combine(path, "JSON/RAHEJA/ProposalOd.json");
                    }
                    else
                    {
                        if (model.PolicyType.ToUpper().Equals("NEW"))
                        {
                            ProposalPath = Path.Combine(path, "JSON/RAHEJA/Proposal.json");
                        }
                        else
                        {
                            ProposalPath = Path.Combine(path, "JSON/RAHEJA/PropsalRollover.json");
                        }

                    }
                    //string ProposalPath = Path.Combine(path, "JSON/RAHEJA/Proposal.json");
                    string Proposaljson = File.ReadAllText(ProposalPath);
                    dynamic ProjsonObject = Newtonsoft.Json.JsonConvert.DeserializeObject(Proposaljson);
                    string regino1 = string.Empty;
                    string regino2 = string.Empty;
                    //string regino3 = string.Empty;
                    //string regino4 = string.Empty;
                    //string rtoandcity = AndEnt.VW_RTOMASTER.Where(x => x.rtocode == rtocode).Select(x => x.rtodesc).FirstOrDefault().ToString();
                    ProjsonObject["ContactNumber"] = model.ClientDetails.MobileNo;
                    ProjsonObject["mailid"] = model.ClientDetails.EmailId;
                    ProjsonObject["objVehicleDetails"]["MakeModelVarient"] = model.VehicleDetails.MakeName + "|" + model.VehicleDetails.ModelName + "|" + model.VehicleDetails.VariantName + "|" + model.VehicleDetails.CC + "CC";
                    //ProjsonObject["objVehicleDetails"]["RtoLocation"] = "AHMEDABAD|GJ01";
                    ProjsonObject["objVehicleDetails"]["RtoLocation"] = "Thane|MH04";
                    ProjsonObject["objVehicleDetails"]["RegistrationDate"] = Convert.ToDateTime(model.VehicleDetails.RegistrationDate).ToString("yyyy-MM-dd");
                    ProjsonObject["objVehicleDetails"]["ManufacturingYear"] = Convert.ToString(Convert.ToDateTime(model.VehicleDetails.ManufaturingDate).Year);
                    ProjsonObject["objVehicleDetails"]["ManufacturingMonth"] = Convert.ToDateTime(model.VehicleDetails.ManufaturingDate).ToString("MM");
                    ProjsonObject["objVehicleDetails"]["FuelType"] = model.VehicleDetails.Fuel;
                    ProjsonObject["objVehicleDetails"]["ModifiedIDV"] = Convert.ToString(model.IDV);
                    ProjsonObject["objVehicleDetails"]["Registration_Number1"] = "MH";
                    ProjsonObject["objVehicleDetails"]["Registration_Number2"] = "04";
                    if (model.VehicleDetails.RegistrationNumber.Length > 5)
                    {
                        //jsonObject["objVehicleDetails"]["Registration_Number1"] = regino1.ToUpper(); //"GJ";
                        //jsonObject["objVehicleDetails"]["Registration_Number2"] = regino2.ToUpper(); //"01";
                        //regino3 = model.VehicleDetails.RegistrationNumber.Substring(4, 2).ToUpper();
                        //regino4 = model.VehicleDetails.RegistrationNumber.Substring(6, 4);
                        int rglenght = model.VehicleDetails.RegistrationNumber.Length - 4;
                        string subregno = model.VehicleDetails.RegistrationNumber.Substring(4, rglenght);
                        string part1 = model.VehicleDetails.RegistrationNumber.Substring(0, 2);
                        string part2 = model.VehicleDetails.RegistrationNumber.Substring(2, 2);
                        string part3 = string.Empty; string part4 = string.Empty;
                        if (model.VehicleDetails.RegistrationNumber.Contains("-"))
                        {
                            string[] a = subregno.Split('-');
                            part3 = Convert.ToString(a[0]);
                            part4 = Convert.ToString(a[1]);
                        }
                        else
                        {
                            part3 = subregno.Substring(0, subregno.Length - 4);
                            part4 = subregno.Substring(subregno.Length - 4);
                        }
                        ProjsonObject["objVehicleDetails"]["Registration_Number3"] = part3.ToUpper();
                        ProjsonObject["objVehicleDetails"]["Registration_Number4"] = part4;
                    }
                    else
                    {
                        //jsonObject["objVehicleDetails"]["Registration_Number1"] = regino1.ToUpper();  //"GJ";
                        //jsonObject["objVehicleDetails"]["Registration_Number2"] = regino2.ToUpper();//"01";
                        ProjsonObject["objVehicleDetails"]["Registration_Number3"] = "AB";
                        ProjsonObject["objVehicleDetails"]["Registration_Number4"] = "1234";
                    }
                    //ProjsonObject["objVehicleDetails"]["Registration_Number3"] = "AA";
                    //ProjsonObject["objVehicleDetails"]["Registration_Number4"] = "5858";
                    ProjsonObject["objVehicleDetails"]["EngineNumber"] = model.VehicleDetails.EngineNumber;
                    ProjsonObject["objVehicleDetails"]["ChassisNumber"] = model.VehicleDetails.ChassisNumber;
                    //ProjsonObject["objVehicleDetails"]["VehicleColorID"]
                    //ProjsonObject["objVehicleDetails"]["AverageVehicleUsageId"]
                    //ProjsonObject["objVehicleDetails"]["PUCNumber"] = model.VehicleDetails.IsValidPUC ? model.VehicleDetails.PUCNumber : string.Empty;
                    //ProjsonObject["objVehicleDetails"]["PUCEndDate"] = model.VehicleDetails.IsValidPUC ? Convert.ToDateTime(model.VehicleDetails.PUCEndDate).ToString("yyyy-MM-dd") : string.Empty;
                    ProjsonObject["objClientAddress"]["Address1"] = model.CustomerAddressDetails.Address1;
                    ProjsonObject["objClientAddress"]["Address2"] = model.CustomerAddressDetails.Address2;
                    ProjsonObject["objClientAddress"]["Address3"] = string.IsNullOrEmpty(model.CustomerAddressDetails.Address3) ? string.Empty : model.CustomerAddressDetails.Address3;
                    // ProjsonObject["objClientAddress"]["City"] = "AHMEDABAD";//model.CustomerAddressDetails.City;
                    // ProjsonObject["objClientAddress"]["State"] = "GUJARAT";//model.CustomerAddressDetails.State;
                    // ProjsonObject["objClientAddress"]["Area"] =
                    //ProjsonObject["objClientAddress"]["Country"] = "INDIA";
                    // ProjsonObject["objClientAddress"]["Pincode"] = "380051";//model.CustomerAddressDetails.Pincode;
                    ProjsonObject["objClientAddress"]["City"] = model.CustomerAddressDetails.City; //"MUMBAI";//model.CustomerAddressDetails.City;
                    ProjsonObject["objClientAddress"]["State"] = model.CustomerAddressDetails.State; //"MAHARASHTRA";//model.CustomerAddressDetails.State;
                    ProjsonObject["objClientAddress"]["Area"] = model.CustomerAddressDetails.Area;//"Bhawani Shankar S.O";
                    ProjsonObject["objClientAddress"]["Country"] = "INDIA";
                    ProjsonObject["objClientAddress"]["Pincode"] = model.CustomerAddressDetails.Pincode;//"400028";//model.CustomerAddressDetails.Pincode;
                    if (model.VehicleAddressDetails != null)
                    {
                        ProjsonObject["objRegistrationAddress"]["Address1"] = string.IsNullOrEmpty(model.VehicleAddressDetails.Address1) ? string.Empty : model.VehicleAddressDetails.Address1;
                        ProjsonObject["objRegistrationAddress"]["Address2"] = string.IsNullOrEmpty(model.VehicleAddressDetails.Address2) ? string.Empty : model.VehicleAddressDetails.Address2;
                        ProjsonObject["objRegistrationAddress"]["Address3"] = string.IsNullOrEmpty(model.VehicleAddressDetails.Address3) ? string.Empty : model.VehicleAddressDetails.Address3;
                        //ProjsonObject["objRegistrationAddress"]["City"] = model.VehicleAddressDetails.City;
                        //ProjsonObject["objRegistrationAddress"]["State"] = model.VehicleAddressDetails.State;
                        //ProjsonObject["objRegistrationAddress"]["Country"] = "INDIA";
                        //ProjsonObject["objRegistrationAddress"]["Pincode"] = "";//model.VehicleAddressDetails.Pincode;
                        ProjsonObject["objRegistrationAddress"]["City"] = string.IsNullOrEmpty(model.VehicleAddressDetails.City) ? string.Empty : model.VehicleAddressDetails.City; //"MUMBAI";
                        ProjsonObject["objRegistrationAddress"]["State"] = string.IsNullOrEmpty(model.VehicleAddressDetails.State) ? string.Empty : model.VehicleAddressDetails.State; //"MAHARASHTRA";
                        ProjsonObject["objRegistrationAddress"]["Area"] = string.IsNullOrEmpty(model.VehicleAddressDetails.Area) ? string.Empty : model.VehicleAddressDetails.Area;// "Bhawani Shankar S.O";
                        ProjsonObject["objRegistrationAddress"]["Country"] = "INDIA";
                        ProjsonObject["objRegistrationAddress"]["Pincode"] = string.IsNullOrEmpty(model.VehicleAddressDetails.Pincode) ? string.Empty : model.VehicleAddressDetails.Pincode;//"400028";//model.VehicleAddressDetails.Pincode;
                    }
                    ProjsonObject["objClientDetails"]["Client_FirstName"] = model.ClientDetails.FirstName;
                    if (model.ClientDetails != null)
                    {
                        if (!string.IsNullOrEmpty(model.ClientDetails.MiddleName))
                        {
                            model.ClientDetails.LastName = model.ClientDetails.MiddleName + " " + model.ClientDetails.LastName;
                        }

                    }
                    ProjsonObject["objClientDetails"]["LastName"] = model.ClientDetails.LastName;
                    ProjsonObject["objClientDetails"]["Client_DOB"] = Convert.ToDateTime(model.ClientDetails.DateOfBirth).ToString("yyyy-MM-dd");
                    //ProjsonObject["objClientDetails"]["InsuredOccupation"] = model.ClientDetails.Occupation;
                    if (!model.CustomerType.Equals("Individual", StringComparison.OrdinalIgnoreCase))
                    {
                        model.ClientDetails.Gender = "91";
                    }
                    else
                    {
                        if (model.ClientDetails.Gender.ToUpper().Equals("MALE"))
                        {
                            model.ClientDetails.Gender = "20";
                        }
                        else
                        {
                            model.ClientDetails.Gender = "21";
                        }
                    }

                    ProjsonObject["objClientDetails"]["Gender"] = model.ClientDetails.Gender;
                    ProjsonObject["objClientDetails"]["Salutation"] = DecideSalutation(model.ClientDetails.Salutation);


                    ProjsonObject["objClientDetails"]["ClientType"] = model.CustomerType.Equals("Individual", StringComparison.OrdinalIgnoreCase) ? "0" : "1";
                    ProjsonObject["objClientDetails"]["CorporateName"] = string.IsNullOrEmpty(model.OrganizationName) ? string.Empty : model.OrganizationName;
                    ProjsonObject["objClientDetails"]["MobileNumber"] = model.ClientDetails.MobileNo;
                    ProjsonObject["objClientDetails"]["EmailId"] = model.ClientDetails.EmailId;
                    ProjsonObject["objClientDetails"]["PANNumber"] = string.IsNullOrEmpty(model.ClientDetails.PanCardNo) ? string.Empty : model.ClientDetails.PanCardNo;

                    ProjsonObject["objClientDetails"]["GSTINNumber"] = string.IsNullOrEmpty(model.ClientDetails.GSTIN) ? string.Empty : model.ClientDetails.GSTIN;
                    ProjsonObject["objVehicleHypoth"]["FinancierType"] = model.VehicleDetails.IsVehicleLoan ? "2" : string.Empty;
                    ProjsonObject["objVehicleHypoth"]["Financier_Address"] = string.IsNullOrEmpty(model.VehicleDetails.LoanCity) ? string.Empty : model.VehicleDetails.LoanCity;
                    ProjsonObject["objVehicleHypoth"]["Financier_Name"] = string.IsNullOrEmpty(model.VehicleDetails.LoanCompanyName) ? string.Empty : model.VehicleDetails.LoanCompanyName;


                    ProjsonObject["objPolicy"]["QuoteID"] = ProRes.CompanyWiseRefference.QuoteId;
                    ProjsonObject["objPolicy"]["QuoteNo"] = ProRes.CompanyWiseRefference.QuoteNo;
                    ProjsonObject["objPolicy"]["Policyid"] = ProRes.CompanyWiseRefference.applicationId;
                    ProjsonObject["objPolicy"]["PolicyStartDate"] = Convert.ToDateTime(sdate).ToString("yyyy-MM-dd");


                    ProjsonObject["objPolicy"]["PolicyEndDate"] = Convert.ToDateTime(model.PolicyEndDate).ToString("yyyy-MM-dd");
                    string PrvTPPolicyStartDate = string.Empty;
                    //ProjsonObject["objPolicy"]["ProductName"] = model.PlanName;
                    if (model.IsODOnly)
                    {
                        //ProjsonObject["objVehicleDetails"]["Registration_Number3"] = "AB";
                        //ProjsonObject["objVehicleDetails"]["Registration_Number4"] = "1234";
                        ProjsonObject["objPolicy"]["ProductCode"] = "2323";
                        ProjsonObject["objPolicy"]["ProductName"] = "MOTOR - PRIVATE CAR STANDALONE OD(2323)";
                        ProjsonObject["objPolicy"]["CoverType"] = "1668";

                        if (model.PreviousTPPolicyDetails != null)
                        {
                            PrvTPPolicyStartDate = Convert.ToDateTime(model.PreviousTPPolicyDetails.PolicyEndDate).AddYears(-3).AddDays(1).ToString("yyyy-MM-dd");
                            //model.PreviousTPPolicyDetails.CompanyId = Convert.ToInt16(GetRahejaPrevCompany(Convert.ToInt16(model.PreviousTPPolicyDetails.CompanyId)));
                            //DateTime PrvTPPolicyStartDate = Convert.ToDateTime(model.PreviousTPPolicyDetails.PolicyEndDate).AddYears(-3).AddDays(1);
                            ProjsonObject["objPolicy"]["TPPolicyStartDate"] = Convert.ToDateTime(PrvTPPolicyStartDate).ToString("yyyy-MM-dd");
                            ProjsonObject["objPolicy"]["TPPolicyEndDate"] = Convert.ToDateTime(model.PreviousTPPolicyDetails.PolicyEndDate).ToString("yyyy-MM-dd");
                            ProjsonObject["objPolicy"]["TPPolicyNo"] = model.PreviousTPPolicyDetails.PolicyNo;//"TP12334";
                            //ProjsonObject["objPolicy"]["TPInsurerID"] = "1";
                            ProjsonObject["objPolicy"]["TPInsurerAddress"] = model.PreviousTPPolicyDetails.CompanyAddress;//"fdftdtt";
                            ProjsonObject["objPolicy"]["TPInsurerID"] = model.PreviousTPPolicyDetails.CompanyId;
                        }


                    }
                    else if (!model.PolicyType.ToUpper().Equals("NEW"))
                    {
                        //ProjsonObject["objVehicleDetails"]["Registration_Number3"] = "AB";
                        //ProjsonObject["objVehicleDetails"]["Registration_Number4"] = "1234";
                        ProjsonObject["objPolicy"]["ProductCode"] = "2311";
                        ProjsonObject["objPolicy"]["ProductName"] = "MOTOR - PRIVATE CAR PACKAGE POLICY(2311)";
                        ProjsonObject["objPolicy"]["CoverType"] = "1471";
                        ProjsonObject["objPolicy"]["TPPolicyStartDate"] = string.Empty;
                        ProjsonObject["objPolicy"]["TPPolicyEndDate"] = string.Empty;
                        ProjsonObject["objPolicy"]["TPPolicyStartDate"] = string.Empty;


                    }
                    else
                    {
                        //ProjsonObject["objVehicleDetails"]["Registration_Number3"] = "BA";
                        //ProjsonObject["objVehicleDetails"]["Registration_Number4"] = "1222";
                        ProjsonObject["objPolicy"]["ProductCode"] = "2367";
                        ProjsonObject["objPolicy"]["ProductName"] = "MOTOR PRIVATE CAR BUNDLED POLICY(2367)";
                        ProjsonObject["objPolicy"]["CoverType"] = "1473";

                    }

                    ProjsonObject["objPolicy"]["TraceID"] = ProRes.CompanyWiseRefference.OrderNo;
                    ProjsonObject["objPolicy"]["UserName"] = Convert.ToString(ConfigurationManager.AppSettings["RahUsername"]);
                    ProjsonObject["objPolicy"]["TPSourceName"] = Convert.ToString(ConfigurationManager.AppSettings["RahTPSrcName"]);
                    ProjsonObject["objPolicy"]["PolicyStartDate"] = Convert.ToDateTime(model.PolicyStartDate).ToString("yyyy-MM-dd");
                    ProjsonObject["objPolicy"]["PolicyEndDate"] = Convert.ToDateTime(model.PolicyEndDate).ToString("yyyy-MM-dd");
                    ProjsonObject["objPolicy"]["BusinessTypeID"] = model.PolicyType.Equals("New") ? "24" : "25";
                    ProjsonObject["objPolicy"]["strODEndDate"] = Convert.ToDateTime(sdate).AddYears(1).AddDays(-1).ToString("yyyy-MM-dd");
                    if (model.PolicyType.Equals("New"))
                    {
                        //model.Tennure = model.IsODOnly ? 152 : 102;
                        model.Tennure = model.IsODOnly ? 152 : 101;
                    }
                    else
                    {
                        //model.Tennure = model.IsODOnly ? 151 : 101;
                        model.Tennure = model.IsODOnly ? 151 : 102;
                    }
                    //ProjsonObject["objPolicy"]["TraceID"] = model.CompanyWiseRefference.OrderNo;
                    //ProjsonObject["objPolicy"]["BusinessTypeID"] = model.PolicyType.Equals("New") ? "24" : "25";
                    //ProjsonObject["objPolicy"]["CoverType"] = model.PolicyType.Equals("New") ? "1473" : "1471";
                    //if (model.PolicyType.Equals("New"))
                    //{
                    //    model.Tennure = model.IsODOnly ? 152 : 102;
                    //}
                    //else
                    //{
                    //    model.Tennure = model.IsODOnly ? 151 : 101;
                    //}
                    ProjsonObject["objPolicy"]["Tennure"] = model.Tennure;
                    ProjsonObject["objPolicy"]["IsVehicleHypothicated"] = model.VehicleDetails.IsVehicleLoan ? true : false;
                    ProjsonObject["objPolicy"]["IsRegAddressSameasCorrAddress"] = model.CustomerAddressDetails.IsRegistrationAddressSame ? true : false;
                    JToken methods = ProjsonObject.SelectToken("objCovers");
                    foreach (JToken signInName in methods)
                    {
                        string CoverID = (string)signInName.SelectToken("CoverID");
                        var itemProperties = signInName.Children<JProperty>();
                        switch (CoverID)
                        {
                            case "9":
                                if (model.IsThirdPartyOnly)
                                    itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(false);
                                break;
                            case "10":
                                if (model.IsODOnly)
                                    //itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(false);
                                    signInName.Parent.Remove();
                                break;
                            case "39":
                                if (model.CoverageDetails.IsFiberGlassFuelTank)
                                    itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(true);
                                break;
                            case "49":
                                if (model.CoverageDetails.IsLegalLiablityPaidDriver)
                                    itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(true);
                                break;
                            case "21":
                                if (model.CoverageDetails.IsBiFuelKit)
                                    itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(true);
                                break;
                            case "20":
                                if (model.CoverageDetails.IsBiFuelKit)
                                {
                                    itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(true);
                                    dynamic myObject = new JObject();
                                    myObject.PHNumericFeild1 = model.CoverageDetails.BiFuelKitAmount;
                                    itemProperties.FirstOrDefault(xx => xx.Name == "objCoverDetails").Value.Replace(myObject);
                                }
                                break;
                            case "87":
                                if (model.DiscountDetails.IsTPPDRestrictedto6000)
                                    itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(true);
                                break;
                            //case "76":
                            //    if (model.CoverageDetails.NoOfLLPaidDriver > 0)
                            //    {
                            //        itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(true);
                            //        dynamic myObjectLL = new JObject();
                            //        myObjectLL.PHNumericFeild1 = model.CoverageDetails.NoOfLLPaidDriver;
                            //        itemProperties.FirstOrDefault(xx => xx.Name == "objCoverDetails").Value.Replace(myObjectLL);
                            //    }
                            //    break;
                            //case "94":
                            //    if (model.CoverageDetails.IsPACoverUnnamedPerson)
                            //    {
                            //        itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(true);
                            //        dynamic myObjectPA = new JObject();
                            //        myObjectPA.PHNumericFeild1 = model.CoverageDetails.PACoverUnnamedPersonAmount;
                            //        itemProperties.FirstOrDefault(xx
                            //            => xx.Name == "objCoverDetails").Value.Replace(myObjectPA);
                            //    }
                            //    break;
                            case "73":
                                if (model.CoverageDetails.IsPACoverForOwnerDriver)
                                {
                                    //itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(false);
                                    dynamic myObjectPAOwner = new JObject();
                                    myObjectPAOwner.PHintFeild1 = Convert.ToString(CalculateAge(Convert.ToDateTime(model.NomineeDateOfBirth)));
                                    myObjectPAOwner.PHVarcharFeild1 = model.NomineeName;
                                    myObjectPAOwner.PHVarcharFeild2 = model.NomineeRelationShip;
                                    myObjectPAOwner.PHVarcharFeild2 = "1547";
                                    myObjectPAOwner.PHNumericFeild2 = "100000";
                                    myObjectPAOwner.PHNumericFeild1 = "1";
                                    if (!string.IsNullOrEmpty(model.AppointeeName))
                                    {
                                        //myObjectPAOwner.PHNumericFeild1 = Convert.ToString(CalculateAge(Convert.ToDateTime(model.AppointeeDateOfBirth)));
                                        myObjectPAOwner.PHVarcharFeild4 = model.AppointeeName;
                                        myObjectPAOwner.PHVarcharFeild5 = model.AppointeeRelationShip;
                                        myObjectPAOwner.PHVarcharFeild5 = "1134";
                                        myObjectPAOwner.PHBooleanField1 = "false";
                                        myObjectPAOwner.PHBooleanField2 = "false";
                                    }
                                    else
                                    {
                                        //myObjectPAOwner.PHNumericFeild1 = string.Empty;
                                        myObjectPAOwner.PHVarcharFeild4 = string.Empty;
                                        myObjectPAOwner.PHVarcharFeild5 = string.Empty;
                                        myObjectPAOwner.PHBooleanField1 = "false";
                                        myObjectPAOwner.PHBooleanField2 = "false";
                                    }
                                    itemProperties.FirstOrDefault(xx => xx.Name == "objCoverDetails").Value.Replace(myObjectPAOwner);
                                }
                                else
                                {
                                    itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(false);
                                    dynamic myObjectPAOwner = new JObject();
                                    myObjectPAOwner.PHintFeild1 = string.Empty;
                                    myObjectPAOwner.PHVarcharFeild1 = string.Empty;
                                    myObjectPAOwner.PHVarcharFeild2 = string.Empty;
                                    myObjectPAOwner.PHVarcharFeild2 = string.Empty;
                                    myObjectPAOwner.PHNumericFeild2 = string.Empty;
                                    myObjectPAOwner.PHNumericFeild1 = string.Empty;
                                    myObjectPAOwner.PHVarcharFeild4 = string.Empty;
                                    myObjectPAOwner.PHVarcharFeild5 = string.Empty;
                                    myObjectPAOwner.PHBooleanField1 = false;
                                    myObjectPAOwner.PHBooleanField2 = true;
                                    itemProperties.FirstOrDefault(xx
                                        => xx.Name == "objCoverDetails").Value.Replace(myObjectPAOwner);
                                }
                                break;
                            //case "105":
                            //    if (model.CoverageDetails.IsEmployeeLiability)
                            //        itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(true);
                            //    break;
                            case "91":
                                if (model.DiscountDetails.VoluntaryExcessAmount > 0)
                                {
                                    itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(true);
                                    dynamic myObjectVoluntary = new JObject();
                                    myObjectVoluntary.PHNumericFeild1 = model.DiscountDetails.VoluntaryExcessAmount;
                                    itemProperties.FirstOrDefault(xx => xx.Name == "objCoverDetails").Value.Replace(myObjectVoluntary);
                                }
                                break;
                            case "33":
                                if (model.CoverageDetails.IsElectricalAccessories)
                                {
                                    itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(true);
                                    dynamic myObjectele = new JObject();
                                    myObjectele.PHNumericFeild1 = model.CoverageDetails.SIElectricalAccessories / 2;
                                    myObjectele.PHVarcharFeild1 = "Test";
                                    myObjectele.PHNumericFeild2 = model.CoverageDetails.SIElectricalAccessories / 2;
                                    myObjectele.PHVarcharFeild2 = "Test";
                                    itemProperties.FirstOrDefault(xx => xx.Name == "objCoverDetails").Value.Replace(myObjectele);
                                }
                                break;
                            case "70":
                                if (model.CoverageDetails.IsNonElectricalAccessories)
                                {
                                    itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(true);
                                    dynamic myObjectele = new JObject();
                                    myObjectele.PHNumericFeild1 = model.CoverageDetails.SINonElectricalAccessories / 2;
                                    myObjectele.PHVarcharFeild1 = "Test";
                                    myObjectele.PHNumericFeild2 = model.CoverageDetails.SINonElectricalAccessories / 2;
                                    myObjectele.PHVarcharFeild2 = "Test";
                                    itemProperties.FirstOrDefault(xx => xx.Name == "objCoverDetails").Value.Replace(myObjectele);
                                }
                                break;
                            case "97":
                                if (model.AddonCover.IsZeroDeperation)
                                {
                                    if (model.PolicyType.Equals("New"))
                                    {
                                        dynamic myObjectelezero = new JObject();
                                        //myObjectelezero.PHNumericFeild1 = model.CoverageDetails.SINonElectricalAccessories / 2;
                                        myObjectelezero.PHVarcharFeild1 = "2";
                                        // myObjectelezero.PHNumericFeild2 = model.CoverageDetails.SINonElectricalAccessories / 2;
                                        myObjectelezero.PHVarcharFeild2 = "";
                                        itemProperties.FirstOrDefault(xx => xx.Name == "objCoverDetails").Value.Replace(myObjectelezero);
                                        itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(true);
                                    }
                                    else
                                    {
                                        dynamic myObjectelezero = new JObject();
                                        //myObjectelezero.PHNumericFeild1 = model.CoverageDetails.SINonElectricalAccessories / 2;
                                        myObjectelezero.PHVarcharFeild1 = "2";
                                        // myObjectelezero.PHNumericFeild2 = model.CoverageDetails.SINonElectricalAccessories / 2;
                                        myObjectelezero.PHVarcharFeild2 = "Yes";
                                        itemProperties.FirstOrDefault(xx => xx.Name == "objCoverDetails").Value.Replace(myObjectelezero);
                                        itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(true);
                                        //itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(false);
                                    }
                                }
                                break;
                            case "37":
                                if (model.AddonCover.IsEngineProtector)
                                    itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(true);
                                break;
                            case "24":
                                if (model.AddonCover.IsConsumables)
                                    itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(true);
                                break;
                            case "80":
                                if (model.AddonCover.IsReturntoInvoice)
                                    itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(true);
                                break;
                            case "101":
                                if (model.AddonCover.IsLossofpersonalBelonging)
                                    itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(true);
                                break;
                            case "104":
                                if (model.AddonCover.IsTyreCover)
                                    itemProperties.FirstOrDefault(xx => xx.Name == "IsChecked").Value.Replace(true);
                                break;

                            default:
                                break;
                        }
                    }

                    //string requestjson1 = Newtonsoft.Json.JsonConvert.SerializeObject(ProjsonObject, Newtonsoft.Json.Formatting.Indented);
                    if (!model.PolicyType.ToUpper().Equals("NEW"))
                    {
                        if (model.PreviousPolicyDetails != null)
                        {
                            var PreviousPolicyStartDate = Convert.ToDateTime(model.PreviousPolicyDetails.PreviousPolicyEndDate).AddYears(-1).AddDays(1).ToString("yyyy-MM-dd");
                            //model.PreviousPolicyDetails.PreviousPolicyStartDate = Convert.ToDateTime(model.PreviousPolicyDetails.PreviousPolicyEndDate).AddYears(-1).AddDays(1).ToString();

                            // need dynamic from db
                            //model.PreviousPolicyDetails.CompanyId = Convert.ToInt16(GetRahejaPrevCompany(Convert.ToInt16(model.PreviousPolicyDetails.CompanyId)));
                            //model.PreviousPolicyDetails.CompanyId = model.PreviousPolicyDetails.CompanyId;
                            ProjsonObject["objPreviousInsurance"]["PrevPolicyType"] = PrvTypeId;
                            ProjsonObject["objPreviousInsurance"]["PrevPolicyStartDate"] = Convert.ToDateTime(PreviousPolicyStartDate).ToString("yyyy-MM-dd");
                            ProjsonObject["objPreviousInsurance"]["PrevPolicyEndDate"] = Convert.ToDateTime(model.PreviousPolicyDetails.PreviousPolicyEndDate).ToString("yyyy-MM-dd");
                            ProjsonObject["objPreviousInsurance"]["ProductCode"] = "2311";
                            ProjsonObject["objPreviousInsurance"]["PrevInsuranceCompanyID"] = model.PreviousPolicyDetails.CompanyId;
                            ProjsonObject["objPreviousInsurance"]["PrevNCB"] = model.PreviousPolicyDetails.PreviousNcbPercentage;
                            ProjsonObject["objPreviousInsurance"]["IsClaimedLastYear"] = model.PreviousPolicyDetails.IsPreviousInsuranceClaimed ? "1" : "2";
                            ProjsonObject["objPreviousInsurance"]["PrevPolicyNo"] = model.PreviousPolicyDetails.PreviousPolicyNo;
                            ProjsonObject["objPreviousInsurance"]["PrevInsurerAddress"] = model.PreviousPolicyDetails.PreviousCompanyAddress;//"Addresssff";
                            ProjsonObject["objPreviousInsurance"]["NatureOfLoss"] = (model.PreviousPolicyDetails.IsPreviousInsuranceClaimed ? PrvTypeId : string.Empty);
                            ProjsonObject["objPreviousInsurance"]["prevPolicyCoverType"] = model.PreviousPolicyDetails.PreviousPolicyType.ToUpper();
                            ProjsonObject["objPreviousInsurance"]["CurrentNCBHidden"] = model.CurrentNcb;
                            //requestjson1 += "\"objPreviousInsurance\":{\"PrevPolicyType\":\"" + PrvTypeId +
                            //       "\",\"PrevPolicyStartDate\":\"" + Convert.ToDateTime(model.PreviousPolicyDetails.PreviousPolicyStartDate).ToString("yyyy-MM-dd") +
                            //       "\",\"PrevPolicyEndDate\":\"" + Convert.ToDateTime(model.PreviousPolicyDetails.PreviousPolicyEndDate).ToString("yyyy-MM-dd") +
                            //       "\",\"ProductCode\":\"2311\",\"PrevInsuranceCompanyID\":\"" + model.PreviousPolicyDetails.CompanyId + "\",\"PrevNCB\":\"" +
                            //       model.PreviousPolicyDetails.PreviousNcbPercentage + "\",\"IsClaimedLastYear\":\"" + (model.PreviousPolicyDetails.IsPreviousInsuranceClaimed ? "1" : "2") +
                            //       "\",\"PrevPolicyNo\":\"" + model.PreviousPolicyDetails.PreviousPolicyNo+ "\",\"PrevInsurerAddress\":\"" + "Addresssff" +
                            //       "\",\"NatureOfLoss\":\"" + (model.PreviousPolicyDetails.IsPreviousInsuranceClaimed ? PrvTypeId : string.Empty) +
                            //       "\",\"prevPolicyCoverType\":\"" + model.PreviousPolicyDetails.PreviousPolicyType.ToUpper() + "\",\"CurrentNCBHidden\":\"" + model.CurrentNcb +
                            //       "\"}";
                        }
                    }
                    string requestjson1 = Newtonsoft.Json.JsonConvert.SerializeObject(ProjsonObject, Newtonsoft.Json.Formatting.Indented);

                    var prmCalcResponse = Webservicecall(requestjson1, Convert.ToString(ConfigurationManager.AppSettings["RahBaseUrl"]), Convert.ToString(ConfigurationManager.AppSettings["RahProposal"]));
                    //var prmCalcResponse = Webservicecall(requestjson1, "http://52.172.5.3:8423/api/MotorAPI", "http://52.172.5.3:8423/api/MotorAPI/MotorProposalCreation");
                    ap.SP_REQUEST_RESPONSE_API_MASTER(model.enquiryid, 16, Convert.ToString(requestjson1), Convert.ToString(prmCalcResponse.Content));
                    if (prmCalcResponse.StatusCode == HttpStatusCode.OK)
                    {
                        ap.SP_Payment_Parameter(model.enquiryid, 16, "Policyid", ProRes.CompanyWiseRefference.applicationId);
                        ap.SP_Payment_Parameter(model.enquiryid, 16, "TraceID", ProRes.CompanyWiseRefference.OrderNo);
                        ap.SP_Payment_Parameter(model.enquiryid, 16, "QuoteNo", ProRes.CompanyWiseRefference.QuoteNo);
                        ProRes = GetProposalResponse((JObject)JsonConvert.DeserializeObject(Convert.ToString(prmCalcResponse.Content)));
                        string customeraddress = model.CustomerAddressDetails.Address1 + " " + model.CustomerAddressDetails.Address2 + " " + model.CustomerAddressDetails.Address3 + " " + model.CustomerAddressDetails.Pincode;
                        DateTime? tpstartdate = null, tpenddate = null;
                        string producttype = "Comprehensive";
                        if (model.IsODOnly)
                        {
                            tpstartdate = Convert.ToDateTime(PrvTPPolicyStartDate);
                            tpenddate = Convert.ToDateTime(model.PreviousTPPolicyDetails.PolicyEndDate);
                        }

                        if (model.IsODOnly)
                        {
                            producttype = "ODOnly";
                        }

                        if (model.IsThirdPartyOnly)
                        {
                            producttype = "ThirdParty";
                        }
                        //string[] date = model.PolicyStartDate.Split('-');
                        //string datepart3 = date[2].Substring(0, 2);
                        //sdate = date[0] + "-" + date[1] + "-" + datepart3;
                        //string PolicyEndDate = string.Empty;
                        //if (model.PolicyType.Equals("New"))
                        //{
                        //    PolicyEndDate = Convert.ToDateTime(sdate).AddYears(3).AddDays(-1).ToString("yyyy-MM-dd");
                        //}
                        //else
                        //{
                        //    PolicyEndDate = Convert.ToDateTime(sdate).AddYears(1).AddDays(-1).ToString("yyyy-MM-dd");
                        //}
                        ap.SP_POLICYDETAILSMASTER("I", model.enquiryid, 16, model.pospid, model.CustomerType, model.PolicyType, producttype,
                            null, model.ClientDetails.FirstName, model.ClientDetails.MiddleName, model.ClientDetails.LastName,
                            customeraddress, model.ClientDetails.PanCardNo, model.ClientDetails.GSTIN, model.ClientDetails.EmailId,
                            Convert.ToDateTime(model.ClientDetails.DateOfBirth), model.ClientDetails.MobileNo, model.VehicleDetails.RtoId, null,
                           0, 0,
                           model.VehicleDetails.VariantId,
                            Convert.ToDateTime(sdate), Convert.ToDateTime(model.VehicleDetails.RegistrationDate),
                            Convert.ToDateTime(model.VehicleDetails.ManufaturingDate),
                            model.VehicleDetails.SC, model.VehicleDetails.RegistrationNumber, model.VehicleDetails.CC,
                            model.VehicleDetails.EngineNumber, model.VehicleDetails.ChassisNumber, model.VehicleDetails.Fuel,
                            null, Convert.ToDateTime(sdate), Convert.ToDateTime(model.PolicyEndDate),
                            tpstartdate, tpenddate,
                            1, model.CurrentNcb, Convert.ToDecimal(ProRes.PremiumBreakUpDetails.NCBDiscount), null,
                            model.IDV, Convert.ToDecimal(ProRes.PremiumBreakUpDetails.NetAddonPremium),
                            Convert.ToDecimal(model.PremiumDetails.OdPremiumAmount),
                            Convert.ToDecimal(model.PremiumDetails.TpPremiumAmount),
                            Convert.ToDecimal(model.PremiumDetails.NetPremiumAmount),
                            Convert.ToDecimal(model.PremiumDetails.TaxAmount),
                            Convert.ToDecimal(model.PremiumDetails.TotalPremiumAmount), false);
                    }
                    else
                    {
                        ProRes.Status = Status.Fail;
                        ProRes.ErrorMsg = "Getting an error from Proposal Service";
                        LogU.WriteLog("RAHEJA >> PrivateCar >> GetProposalRequest >> " + ProRes.ErrorMsg);
                        return ProRes;
                    }

                }
                catch (Exception ex)
                {
                    ProRes.Status = Status.Fail;
                    ProRes.ErrorMsg = Convert.ToString(ex.Message);
                    LogU.WriteLog("RAHEJA >> PrivateCar >> GetProposalRequest >> " + Convert.ToString(ex.Message));
                    Console.Write(Convert.ToString(ex));
                }
                ProRes.CompanyName = Company.RAHEJA.ToString();
                return ProRes;
            }
            else
            {
                ProRes.CompanyName = Company.RAHEJA.ToString();
                return ProRes;
            }

        }

        public IRestResponse Webservicecall(string jsonString, string baseurl, string method)
        {
            RestClient client = new RestClient(baseurl);
            var request = new RestRequest(method, Method.POST);
            string credentials = String.Format("{0}:{1}", "1009298", "B346DA63-A815-40A5-B17D-A96C979E6CBE");
            byte[] bytes = Encoding.ASCII.GetBytes(credentials);
            string base64 = Convert.ToBase64String(bytes);
            string authorization = String.Concat("Basic ", base64);
            request.AddHeader("Authorization", authorization.ToString());
            request.RequestFormat = DataFormat.Json;
            //var json = new JavaScriptSerializer().Serialize(obj);
            request.AddJsonBody(jsonString);
            var response = client.Execute(request);
            return response;
        }

        public string GetTraceID()
        {
            try
            {
                string credentials = String.Format("{0}:{1}", Convert.ToString(ConfigurationManager.AppSettings["RahUsername"]), Convert.ToString(ConfigurationManager.AppSettings["RahPwd"]));
                //string credentials = String.Format("{0}:{1}", "1009298", "B346DA63-A815-40A5-B17D-A96C979E6CBE");
                byte[] bytes = Encoding.ASCII.GetBytes(credentials);
                string base64 = Convert.ToBase64String(bytes);
                string authorization = String.Concat("Basic ", base64);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Convert.ToString(ConfigurationManager.AppSettings["RahTraceId"]));
                //HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://52.172.5.3:8423/api/PolicyAPI/GetTraceID?UserName=1009298");
                request.UseDefaultCredentials = true;
                request.Headers.Add("Authorization", authorization);
                request.Method = "GET";
                request.Accept = "application/json";
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                string jsonstr = new StreamReader(response.GetResponseStream()).ReadToEnd().ToString();
                return jsonstr.Replace("\"", "");
            }
            catch (Exception ex)
            {
                LogU.WriteLog("RAHEJA >> PrivateCar >> GetTraceID >> " + Convert.ToString(ex.Message));
                Console.Write(Convert.ToString(ex.Message));
            }
            return string.Empty;
        }

        public int GetRahejaPrevCompany(int compid)
        {
            ANDAPPEntities entity = new ANDAPPEntities();
            var companycode = entity.PREVIOUS_INSURER_MAPPING.Where(x => x.companyid == 16 && x.previouscompanyid == compid).FirstOrDefault();
            if (companycode != null)
                return Convert.ToInt16(companycode.inscompanycode);
            else
                return 0;
        }

        //public Response GenerateTransactionNo()
        //{
        //    Response tran = new Response();
        //    string requestjson1 = "\"objPolicy\":{\"TraceID\":\"" + PrvTypeId +
        //                      "\",\"QuoteNo\":\"" + Convert.ToDateTime(model.PreviousPolicyDetails.PreviousPolicyStartDate).ToString("yyyy-MM-dd") +
        //                      "\",\"SessionID\":\"" + Convert.ToDateTime(model.PreviousPolicyDetails.PreviousPolicyEndDate).ToString("yyyy-MM-dd") +
        //                      "\",\"TPSourceName\":\"2311\",\"UserName\":\"" + model.PreviousPolicyDetails.CompanyId +
        //                      "\"}";

        //    var prmCalcResponse = Webservicecall(requestjson1, "http://52.172.5.3:8423/api/MotorAPI", "http://52.172.5.3:8423/api/MotorAPI/SaveMotorQuotation");
        //    if (prmCalcResponse.StatusCode == HttpStatusCode.OK)
        //    {
        //        tran = GetQuoteResponse((JObject)JsonConvert.DeserializeObject(Convert.ToString(prmCalcResponse.Content)));
        //    }


        //}

        public string GetPaymentParameter(PaymentRequest model)
        {
            string result = string.Empty;
            try
            {
                string filePath = Path.Combine(path, "JSON/RAHEJA/Payment.json");
                string json = File.ReadAllText(filePath);
                dynamic jsonObject = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
                jsonObject["objPolicy"]["TraceID"] = model.CompanyDetail.OrderNo;
                jsonObject["objPolicy"]["UserName"] = Convert.ToString(ConfigurationManager.AppSettings["RahUsername"]);
                jsonObject["objPolicy"]["TPSourceName"] = Convert.ToString(ConfigurationManager.AppSettings["RahTPSrcName"]);
                jsonObject["objPolicy"]["QuoteNo"] = model.CompanyDetail.QuoteNo;
                jsonObject["objPolicy"]["SessionID"] = string.Empty;
                string resjson1 = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObject, Newtonsoft.Json.Formatting.Indented);

                var prmCalcResponse = Webservicecall(resjson1, Convert.ToString(ConfigurationManager.AppSettings["RahBaseUrl"]), Convert.ToString(ConfigurationManager.AppSettings["RahPayment"]));
                //var prmCalcResponse = Webservicecall(resjson1, "http://52.172.5.3:8423/api/MotorAPI", "http://52.172.5.3:8423/api/PaymentAPI/GenerateTransationNumber");

                if (prmCalcResponse.StatusCode == HttpStatusCode.OK)
                {
                    dynamic s = (JObject)JsonConvert.DeserializeObject(Convert.ToString(prmCalcResponse.Content));
                    LogU.WriteLog("RAHEJA >> PrivateCar >> GetPaymentParameter >> Request >>  " + resjson1);
                    LogU.WriteLog("RAHEJA >> PrivateCar >> GetPaymentParameter >> Response >>  " + s);
                    string reult = Convert.ToString(prmCalcResponse.Content);
                    result = s["OnlinePaymentPage"].Value;
                    string TxnNo = s["TxnNo"].Value;
                    ap.SP_Payment_Parameter(model.enquiryno, 16, "TxnNo", TxnNo);
                }
                else
                {
                    result = "Getting an error from Raheja";
                    LogU.WriteLog("RAHEJA >> PrivateCar >> GetPaymentParameter >> " + result);
                    result = "200";
                }
            }
            catch (Exception ex)
            {
                result = ex.ToString();
                LogU.WriteLog("RAHEJA >> PrivateCar >> GetPaymentParameter >> " + result);
                result = "200";
            }
            return result;
        }

        public Response GetPolicyno(PaymentRequest model)
        {
            string result = string.Empty;
            Response res = new Response();
            try
            {
                string filePath = Path.Combine(path, "JSON/RAHEJA/PolicyNo.json");
                string json = File.ReadAllText(filePath);
                dynamic jsonObject = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
                jsonObject["objPolicy"]["TraceID"] = model.CompanyDetail.OrderNo;
                jsonObject["objPolicy"]["UserName"] = Convert.ToString(ConfigurationManager.AppSettings["RahUsername"]);
                jsonObject["objPolicy"]["TPSourceName"] = Convert.ToString(ConfigurationManager.AppSettings["RahTPSrcName"]);
                jsonObject["objPolicy"]["QuoteNo"] = model.CompanyDetail.QuoteNo;
                jsonObject["TxnNo"] = model.CompanyDetail.applicationId;
                jsonObject["objPolicy"]["SessionID"] = string.Empty;
                string resjson1 = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObject, Newtonsoft.Json.Formatting.Indented);

                var prmCalcResponse = Webservicecall(resjson1, Convert.ToString(ConfigurationManager.AppSettings["RahBaseUrl"]), Convert.ToString(ConfigurationManager.AppSettings["RahPolicyNo"]));
                //var prmCalcResponse = Webservicecall(resjson1, "http://52.172.5.3:8423/api/MotorAPI", "http://52.172.5.3:8423/api/PaymentAPI/GeneratePolicyNumber");
                LogU.WriteLog("RAHEJA >> PrivateCar >> GetPolicyno >> Request >>  " + resjson1);
                LogU.WriteLog("RAHEJA >> PrivateCar >> GetPolicyno >> Response >>  " + prmCalcResponse);
                if (prmCalcResponse.StatusCode == HttpStatusCode.OK)
                {
                    dynamic s = (JObject)JsonConvert.DeserializeObject(Convert.ToString(prmCalcResponse.Content));
                    string reult = Convert.ToString(prmCalcResponse.Content);

                    LogU.WriteLog("RAHEJA >> PrivateCar >> GetPolicyno >> Response >>  " + s);
                    res.PolicyNo = s["PolicyNo"].Value;
                    res.PolicyPdfUrl = s["PolicyPDFDownloadLink"].Value;
                    res.PlanName = s["TxnNo"].Value;
                    res.ErrorMsg = s.objFault.ErrorMessage;
                }
                else
                {
                    res.Status = Status.Fail;
                    res.ErrorMsg = "Getting an error from Generate Policy No Service";
                    LogU.WriteLog("RAHEJA >> PrivateCar >> GetPolicyno >> " + res.ErrorMsg);
                    return res;
                }
            }
            catch (Exception ex)
            {
                res.Status = Status.Fail;
                res.ErrorMsg = "Getting an error from Generate Policy No Service";
                LogU.WriteLog("RAHEJA >> PrivateCar >> GetPolicyno >> " + ex.ToString());
                return res;

                // throw;
            }
            return res;
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
                LogU.WriteLog("RAHEJA >> PrivateCar >> CalculateAge >> " + Convert.ToString(ex.Message));
                Console.Write(Convert.ToString(ex.Message));
                throw;
            }
            return age;
        }

        public int DecideSalutation(string Salutation)
        {
            int sal;
            try
            {
                switch (Salutation.ToUpper())
                {
                    case "MR":
                        sal = 7;
                        break;
                    case "MRS":
                        sal = 8;
                        break;
                    case "MS":
                        sal = 9;
                        break;
                    case "DR":
                        sal = 10;
                        break;
                    default:
                        sal = 10;
                        break;
                }
            }
            catch (Exception ex)
            {
                LogU.WriteLog("RAHEJA >> PrivateCar >> DecideSalutation >> " + Convert.ToString(ex.Message));
                Console.Write(Convert.ToString(ex.Message));
                throw;
            }
            return sal;
        }
    }
}


