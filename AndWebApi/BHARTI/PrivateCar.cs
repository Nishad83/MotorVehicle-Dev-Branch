

namespace AndWebApi.BHARTI
{
    #region namespace
    using AndApp;
    using AndWebApi.Models;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Security;
    using System.Text;
    using System.Web;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using AndWebApi.Controllers;
    #endregion
    public class PrivateCar
    {
        DefaultController control = new DefaultController();
        public Response GetQuoteRequest(Quotation model)
        {
            Response resModel = new Response();
            XmlDocument doc = new XmlDocument();
            try
            {
                var IDVData = GetIDV(model);
                string path = AppDomain.CurrentDomain.BaseDirectory;
                string filePath = Path.Combine(path, "XML/BHARTI/Quote.xml");
                var document = XDocument.Load(filePath);

                XNamespace vehiclens = "http://schemas.cordys.com/bagi/b2c/emotor/2.0";

                var date = Convert.ToDateTime(model.VehicleDetails.ManufaturingDate.ToString()).ToString("s");

                document.Descendants(vehiclens + "TypeOfBusiness").FirstOrDefault().Value = (model.PolicyType.Equals("New") ? "NB" : "TR");       //TR for rollover
                document.Descendants(vehiclens + "PolicyStartDate").FirstOrDefault().Value = Convert.ToDateTime(model.PolicyStartDate).ToString("s");


                document.Descendants(vehiclens + "AccessoryInsured").FirstOrDefault().Value = (model.CoverageDetails.IsElectricalAccessories) ? "Y" : "N";
                document.Descendants(vehiclens + "AccessoryValue").FirstOrDefault().Value = model.CoverageDetails.SIElectricalAccessories.ToString();
                document.Descendants(vehiclens + "NonElecAccessoryInsured").FirstOrDefault().Value = (model.CoverageDetails.IsNonElectricalAccessories) ? "Y" : "N";
                document.Descendants(vehiclens + "NonElecAccessoryValue").FirstOrDefault().Value = model.CoverageDetails.SINonElectricalAccessories.ToString();
                document.Descendants(vehiclens + "IsBiFuelKit").FirstOrDefault().Value = (model.CoverageDetails.IsBiFuelKit) ? "Y" : "N";
                document.Descendants(vehiclens + "BiFuelKitValue").FirstOrDefault().Value = model.CoverageDetails.BiFuelKitAmount.ToString();
                document.Descendants(vehiclens + "DateOfManufacture").FirstOrDefault().Value = Convert.ToDateTime(model.VehicleDetails.ManufaturingDate.ToString()).ToString("s") + "Z";
                document.Descendants(vehiclens + "DateOfRegistration").FirstOrDefault().Value = Convert.ToDateTime(model.VehicleDetails.RegistrationDate.ToString()).ToString("s") + "Z";
                document.Descendants(vehiclens + "RiskType").FirstOrDefault().Value = (model.PolicyType.Equals("New") ? "L13" : "FPV");
                document.Descendants(vehiclens + "Make").FirstOrDefault().Value = model.VehicleDetails.MakeName;
                document.Descendants(vehiclens + "Model").FirstOrDefault().Value = model.VehicleDetails.ModelName;
                document.Descendants(vehiclens + "FuelType").FirstOrDefault().Value = model.VehicleDetails.Fuel;
                document.Descendants(vehiclens + "Variant").FirstOrDefault().Value = model.VehicleDetails.VariantName;
                document.Descendants(vehiclens + "IDV").FirstOrDefault().Value = IDVData.IDV.ToString();
                document.Descendants(vehiclens + "PAOwnerDriverTenure").FirstOrDefault().Value = (model.PolicyType.Equals("New") ? "1" : "");    // 1 for L13 3 for L33
                // need to discuss         document.XPathSelectElement("//VehicleAge").Value =
                document.Descendants(vehiclens + "CC").FirstOrDefault().Value = model.VehicleDetails.CC.ToString();
                document.Descendants(vehiclens + "PlaceOfRegistration").FirstOrDefault().Value = model.VehicleAddressDetails != null ? model.VehicleAddressDetails.City : string.Empty;                 //discuss
                document.Descendants(vehiclens + "RegistrationNo").FirstOrDefault().Value = (model.PolicyType.Equals("New") ? "NEW" : model.VehicleDetails.RegistrationNumber);
                document.Descendants(vehiclens + "ExShowroomPrice").FirstOrDefault().Value = IDVData.ErrorMsg;
                // document.Descendants(vehiclens + "PolicyType").FirstOrDefault().Value = (model.PolicyType.Equals("New")) ? "Comprehensive" :
                //  document.Descendants(vehiclens + "NCB").FirstOrDefault().Value =
                document.Descendants(vehiclens + "PolicyStartDate").FirstOrDefault().Value = Convert.ToDateTime(model.PolicyStartDate).ToString("s");
                document.Descendants(vehiclens + "PolicyEndDate").FirstOrDefault().Value = Convert.ToDateTime(model.PolicyEndDate).ToString("s");
                //selected covers

                var selectedcovers = document.Descendants(vehiclens + "SelectedCovers").FirstOrDefault();
                XNamespace ns = selectedcovers.GetDefaultNamespace();
                if (model.AddonCover != null)
                {
                    selectedcovers.Elements(ns + "ZeroDepriciationSelected").FirstOrDefault().Value = (model.AddonCover.IsZeroDeperation) ? "True" : "False";
                    selectedcovers.Elements(ns + "RoadsideAssistanceSelected").FirstOrDefault().Value = (model.AddonCover.IsRoadSideAssistance) ? "True" : "False";
                    selectedcovers.Elements(ns + "InvoicePriceSelected").FirstOrDefault().Value = (model.AddonCover.IsReturntoInvoice) ? "True" : "False";
                    selectedcovers.Elements(ns + "HospitalCashSelected").FirstOrDefault().Value = (model.AddonCover.IsHospitalCashCover) ? "True" : "False";
                    selectedcovers.Elements(ns + "MedicalExpensesSelected").FirstOrDefault().Value = (model.AddonCover.IsMedicalExpensesSelected) ? "True" : "False";
                    selectedcovers.Elements(ns + "AmbulanceChargesSelected").FirstOrDefault().Value = (model.AddonCover.IsAmbulanceChargesSelected) ? "True" : "False";
                    selectedcovers.Elements(ns + "EngineGearBoxProtectionSelected").FirstOrDefault().Value = (model.AddonCover.IsEngineProtector) ? "True" : "False";
                    selectedcovers.Elements(ns + "HydrostaticLockSelected").FirstOrDefault().Value = (model.AddonCover.IsHydrostaticLockCover) ? "True" : "False";
                    selectedcovers.Elements(ns + "KeyReplacementSelected").FirstOrDefault().Value = (model.AddonCover.IsLossofKey) ? "True" : "False";
                    selectedcovers.Elements(ns + "NoClaimBonusSameSlabSelected").FirstOrDefault().Value = (model.AddonCover.IsNCBProtection) ? "True" : "False";
                    selectedcovers.Elements(ns + "CosumableCoverSelected").FirstOrDefault().Value = (model.AddonCover.IsConsumables) ? "True" : "False";
                    //
                }
                document.Descendants(vehiclens + "ClientType").FirstOrDefault().Value = model.CustomerType;
                string url = "https://awpuat.bharti-axagi.co.in/home/B2C/com.eibus.web.soap.Gateway.wcp?organization=o=B2C,cn=cordys,cn=defaultInst106,o=mydomain.com";

                string requestData = document.ToString();
                var response = CallWebAPI(requestData, url);
                resModel = GetQuoteResponse(response);
                resModel.IDV = IDVData.IDV;
                resModel.MinIDV = IDVData.MinIDV;
                resModel.MaxIDV = IDVData.MaxIDV;
                resModel.CC = model.VehicleDetails.CC;
                resModel.SC = model.VehicleDetails.SC;
                resModel.FuelType = model.VehicleDetails.Fuel;
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
        public Response GetQuoteResponse(List<XElement> response)
        {
            Response resModel = new Response();
            PremiumBreakUpDetails premdata = new PremiumBreakUpDetails();
            CompanyWiseRefference compdata = new CompanyWiseRefference();
            try
            {
                if (response.Count > 0)
                {
                    string responsedata = response[0].ToString();
                    XmlReader xmlReader = XmlReader.Create(new StringReader(responsedata));
                    DataSet ds = new DataSet();
                    ds.ReadXml(xmlReader);

                    if (ds.Tables.Contains("response"))
                    {
                        DataTable dt_response = ds.Tables["response"];
                        //  List<AddonDetails> List_objent = new List<AddonDetails>();

                        string StatusMsg = dt_response.Rows[0]["StatusMsg"].ToString();
                        string StatusCode = dt_response.Rows[0]["StatusCode"].ToString();
                        if (StatusMsg == "Success" && StatusCode == "200")
                        {
                            resModel.Status = Status.Success;

                            compdata.OrderNo = dt_response.Rows[0]["OrderNo"].ToString();
                            compdata.QuoteNo = dt_response.Rows[0]["QuoteNo"].ToString();
                            DataTable dt_premium = ds.Tables["PremiumDetails"];
                            DataTable dt_cover = ds.Tables["Cover"];
                            DataTable dt_breakup = ds.Tables["Breakup"];

                            double addon = 0;
                            var basicod = 0.0;
                            for (int i = 0; i < dt_cover.Rows.Count; i++)
                            {
                                string covertype = dt_cover.Rows[i]["Type"].ToString();
                                string covername = dt_cover.Rows[i]["Name"].ToString();
                                string Premium = dt_cover.Rows[i]["Premium"].ToString();
                                if (covertype == "Basic" && covername == "CarDamage")
                                {

                                    premdata.NetODPremium = Convert.ToDouble(Premium);
                                    premdata.ElecAccessoriesPremium = Convert.ToDouble(dt_breakup.Rows[0]["Accessory"].ToString());
                                    premdata.NonElecAccessoriesPremium = Convert.ToDouble(dt_breakup.Rows[0]["NonElecAccessory"].ToString());
                                    premdata.CNGLPGKitPremium = Convert.ToDouble(dt_breakup.Rows[0]["BiFuel"].ToString());
                                }
                                if (covertype == "Basic" && covername == "ThirdPartyLiability")
                                {
                                    premdata.NetTPPremium = Convert.ToDouble(Premium);
                                }
                                if (covertype == "Basic" && covername == "PAOwnerDriver")
                                {
                                    premdata.PACoverToOwnDriver = Convert.ToDouble(Premium);
                                }
                                if (covertype == "Addon" && covername == "CONC")
                                {
                                    premdata.CostOfConsumablesPremium = Convert.ToDouble(Premium);
                                    addon = addon + premdata.CostOfConsumablesPremium;
                                }
                                if (covertype == "Addon" && covername == "KEYC")
                                {
                                    premdata.KeyReplacementPremium = Convert.ToDouble(Premium);
                                    addon = addon + premdata.KeyReplacementPremium;
                                }
                                if (covertype == "Addon" && covername == "RSAP")
                                {
                                    premdata.RSAPremium = Convert.ToDouble(Premium);
                                    addon = addon + premdata.RSAPremium;
                                }
                                if (covertype == "Addon" && covername == "AMBC")
                                {
                                    premdata.AmbulanceChargesPremium = Convert.ToDouble(Premium);
                                    addon = addon + premdata.AmbulanceChargesPremium;
                                }
                                if (covertype == "Addon" && covername == "HOSP")
                                {
                                    premdata.HospitalCashCoverPremium = Convert.ToDouble(Premium);
                                    addon = addon + premdata.HospitalCashCoverPremium;
                                }
                                if (covertype == "Addon" && covername == "MEDI")
                                {
                                    premdata.MedicalExpensesPremium = Convert.ToDouble(Premium);
                                    addon = addon + premdata.MedicalExpensesPremium;
                                }
                            }
                            premdata.NetAddonPremium = addon;

                            basicod = Convert.ToDouble(dt_breakup.Rows[0]["BasicOD"].ToString());
                            premdata.BasicODPremium = basicod - addon;
                            int discount = Convert.ToInt16(ds.Tables["PremiumSet"].Rows[0]["Discount"].ToString().Substring(1));

                            premdata.ElecAccessoriesPremium = Convert.ToDouble(dt_breakup.Rows[0]["Accessory"].ToString());
                            premdata.NonElecAccessoriesPremium = Convert.ToDouble(dt_breakup.Rows[0]["NonElecAccessory"].ToString());
                            premdata.CNGLPGKitPremium = Convert.ToDouble(dt_breakup.Rows[0]["BiFuel"].ToString());
                            premdata.AntiTheftDiscount = Convert.ToDouble(dt_breakup.Rows[0]["AntiTheft"].ToString());
                            premdata.OtherDiscount = Math.Round(((basicod * discount) / 100), 0);
                            premdata.NCBDiscount = Convert.ToDouble(dt_breakup.Rows[0]["NCB"].ToString());
                            premdata.BasicThirdPartyLiability = Convert.ToDouble(dt_breakup.Rows[1]["TP"].ToString());
                            premdata.LLToPaidDriver = Convert.ToDouble(dt_breakup.Rows[1]["LLDriver"].ToString());
                            premdata.TPCNGLPGPremium = Convert.ToDouble(dt_breakup.Rows[1]["TPBiFuel"].ToString());

                            premdata.NetDiscount = premdata.NCBDiscount + premdata.OtherDiscount + premdata.AntiTheftDiscount + premdata.VoluntaryDiscount;
                            premdata.NetPremium = Convert.ToDouble(dt_premium.Rows[0]["Premium"].ToString());
                            resModel.FinalPremium = Convert.ToInt32(dt_premium.Rows[0]["TotalPremium"].ToString());
                            resModel.CompanyName = Company.BHARTI.ToString();
                            resModel.Product = Product.Motor;
                            resModel.SubProduct = SubProduct.PrivateCar;
                            premdata.ServiceTax = (int)Convert.ToDouble(ds.Tables["PremiumSet"].Rows[0]["ServiceTax"].ToString());
                            premdata.NetTPPremium = premdata.NetTPPremium + premdata.PACoverToOwnDriver;

                            resModel.CompanyWiseRefference = compdata;
                            resModel.PremiumBreakUpDetails = premdata;
                            resModel.EnquiryId = control.GenerateEnquiryId();
                            //   premdata.BasicODPremium =

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                resModel.Status = Status.Fail;
                resModel.ErrorMsg = ex.ToString();
                throw;
            }
            return resModel;
        }

        //Get IDV
        public Response GetIDV(Quotation model)
        {
            Response resmodel = new Response();
            XmlDocument doc = new XmlDocument();
            try
            {
                string path = AppDomain.CurrentDomain.BaseDirectory;
                string filePath = Path.Combine(path, "XML/BHARTI/IDV.xml");
                var document = XDocument.Load(filePath);
                var manufacturingyear = Convert.ToDateTime(model.VehicleDetails.ManufaturingDate).Year.ToString();
                XNamespace nsAccVal = "http://schemas.cordys.com/default";

                document.Descendants(nsAccVal + "Make").FirstOrDefault().Value = model.VehicleDetails.MakeName;
                document.Descendants(nsAccVal + "Model").FirstOrDefault().Value = model.VehicleDetails.ModelName;
                document.Descendants(nsAccVal + "Variant").FirstOrDefault().Value = model.VehicleDetails.VariantName;
                document.Descendants(nsAccVal + "StateCode").FirstOrDefault().Value = (string.IsNullOrEmpty(model.VehicleDetails.RegistrationNumber) ? "GJ" : model.VehicleDetails.RegistrationNumber.Substring(0, 2));
                document.Descendants(nsAccVal + "Manfaturingyear").FirstOrDefault().Value = manufacturingyear;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://awpuat.bharti-axagi.co.in/home/B2C/com.eibus.web.soap.Gateway.wcp?organization=o=B2C,cn=cordys,cn=defaultInst106,o=mydomain.com");

                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(delegate { return true; });
                // ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                string requestData = document.ToString();
                byte[] data = Encoding.UTF8.GetBytes(requestData);
                request.Method = "POST";
                request.ContentType = "application/xml;charset=utf-8";
                Stream dataStream = request.GetRequestStream();
                dataStream.Write(data, 0, data.Length);
                dataStream.Close();
                WebResponse response = request.GetResponse();
                response = request.GetResponse();
                string result = new StreamReader(response.GetResponseStream()).ReadToEnd();

                XmlReader xmlReader_result = XmlReader.Create(new StringReader(result));
                XDocument xdoc = XDocument.Load(xmlReader_result);

                XNamespace ns = "http://schemas.cordys.com/default";
                var responses = xdoc.Descendants(ns + "getIDVResponse").ToList();
                dataStream.Close();


                if (responses.Count > 0)
                {
                    string responsedata = responses[0].ToString();

                    XmlReader xmlReader = XmlReader.Create(new StringReader(responsedata));
                    DataSet ds = new DataSet();
                    ds.ReadXml(xmlReader);
                    if (ds.Tables.Contains("getIDVResponse"))
                    {
                        DataTable dt_response = ds.Tables["getIDVResponse"];

                        string StatusMsg = dt_response.Rows[0]["Status"].ToString();
                        string StatusCode = dt_response.Rows[0]["Status_Code"].ToString();


                        if (StatusMsg == "Success" && StatusCode == "200")
                        {
                            resmodel.IDV = Convert.ToInt32(dt_response.Rows[0]["IDV"].ToString());
                            string exshowroomprice = dt_response.Rows[0]["Ex_Showromm_Price"].ToString();

                            resmodel.MinIDV = Convert.ToInt32(dt_response.Rows[0]["IDVMinRange"].ToString());
                            resmodel.MaxIDV = Convert.ToInt32(dt_response.Rows[0]["IDVMaxRange"].ToString());
                            resmodel.ErrorMsg = exshowroomprice;
                            //IDV = idv.ToString();
                            //exshowroom = Ex_Showroom_Price.ToString();
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                resmodel.Status = Status.Fail;
                resmodel.ErrorMsg = ex.ToString();

            }
            return resmodel;
        }
        public Response GetProposalRequest(Quotation model)
        {
            Response ResModel = new Response();
            XmlDocument doc = new XmlDocument();
            try
            {
                Random _random = new Random(DateTime.Now.Ticks.GetHashCode());
                var givname = _random.Next(1, 50);
                string path = AppDomain.CurrentDomain.BaseDirectory;
                string filePath = Path.Combine(path, "XML/BHARTI/Quote.xml");
                var document = XDocument.Load(filePath);
                XNamespace nsd = "http://schemas.cordys.com/bagi/b2c/emotor/bpm/1.0";
                XNamespace vehiclens = "http://schemas.cordys.com/bagi/b2c/emotor/2.0";

                var manufacturingyear = Convert.ToDateTime(model.VehicleDetails.ManufaturingDate).Year;
                var vechicleage = DateTime.Now.Year - manufacturingyear + 1;
                var risktype = (model.PolicyType.Equals("New") ? "L13" : "FPV");
                var date = Convert.ToDateTime(model.VehicleDetails.ManufaturingDate.ToString()).ToString("s");
                var currentdate = DateTime.Now.ToString("{ddd,dd MMM yyyy hh:mm:ss}");
                document.Descendants(nsd + "InitTime").FirstOrDefault().Value = currentdate + "GMT";
                document.Descendants(nsd + "OrderNo").FirstOrDefault().Value = (string.IsNullOrEmpty(model.CompanyWiseRefference.OrderNo) ? "NA" : model.CompanyWiseRefference.OrderNo);
                document.Descendants(nsd + "QuoteNo").FirstOrDefault().Value = (string.IsNullOrEmpty(model.CompanyWiseRefference.QuoteNo) ? "NA" : model.CompanyWiseRefference.QuoteNo);
                document.Descendants(vehiclens + "TypeOfBusiness").FirstOrDefault().Value = (model.PolicyType.Equals("New") ? "NB" : "TR");       //TR for rollover
                document.Descendants(vehiclens + "PolicyStartDate").FirstOrDefault().Value = Convert.ToDateTime(model.PolicyStartDate).ToString("o");
                document.Descendants(vehiclens + "AccessoryInsured").FirstOrDefault().Value = (model.CoverageDetails.IsElectricalAccessories) ? "Y" : "N";
                document.Descendants(vehiclens + "AccessoryValue").FirstOrDefault().Value = model.CoverageDetails.SIElectricalAccessories.ToString();
                document.Descendants(vehiclens + "NonElecAccessoryInsured").FirstOrDefault().Value = (model.CoverageDetails.IsNonElectricalAccessories) ? "Y" : "N";
                document.Descendants(vehiclens + "NonElecAccessoryValue").FirstOrDefault().Value = model.CoverageDetails.SINonElectricalAccessories.ToString();
                document.Descendants(vehiclens + "IsBiFuelKit").FirstOrDefault().Value = (model.CoverageDetails.IsBiFuelKit) ? "Y" : "N";
                document.Descendants(vehiclens + "BiFuelKitValue").FirstOrDefault().Value = model.CoverageDetails.BiFuelKitAmount.ToString();
                document.Descendants(vehiclens + "ExternallyFitted").FirstOrDefault().Value = (model.CoverageDetails.IsBiFuelKit) ? "Y" : "N";
                document.Descendants(vehiclens + "DateOfManufacture").FirstOrDefault().Value = Convert.ToDateTime(model.VehicleDetails.ManufaturingDate.ToString()).ToString("s") + "Z";
                document.Descendants(vehiclens + "DateOfRegistration").FirstOrDefault().Value = Convert.ToDateTime(model.VehicleDetails.RegistrationDate.ToString()).ToString("s") + "Z";
                document.Descendants(vehiclens + "RiskType").FirstOrDefault().Value = risktype;
                document.Descendants(vehiclens + "Make").FirstOrDefault().Value = model.VehicleDetails.MakeName;
                document.Descendants(vehiclens + "Model").FirstOrDefault().Value = model.VehicleDetails.ModelName;
                document.Descendants(vehiclens + "FuelType").FirstOrDefault().Value = model.VehicleDetails.Fuel;
                document.Descendants(vehiclens + "Variant").FirstOrDefault().Value = model.VehicleDetails.VariantName;
                document.Descendants(vehiclens + "IDV").FirstOrDefault().Value = model.IDV.ToString();
                document.Descendants(vehiclens + "VehicleAge").FirstOrDefault().Value = vechicleage.ToString();
                document.Descendants(vehiclens + "CC").FirstOrDefault().Value = model.VehicleDetails.CC.ToString();
                document.Descendants(vehiclens + "PlaceOfRegistration").FirstOrDefault().Value = model.VehicleAddressDetails != null ? model.VehicleAddressDetails.City : string.Empty;                 //discuss
                document.Descendants(vehiclens + "SeatingCapacity").FirstOrDefault().Value = model.VehicleDetails.SC.ToString();
                document.Descendants(vehiclens + "RegistrationNo").FirstOrDefault().Value = (model.PolicyType.Equals("New") ? "NEW" : model.VehicleDetails.RegistrationNumber);
                //   document.Descendants(vehiclens + "ExShowroomPrice").FirstOrDefault().Value = IDVData.ErrorMsg;
                document.Descendants(vehiclens + "PAOwnerDriverTenure").FirstOrDefault().Value = (risktype == "L13") ? "1" : risktype == "L33" ? "3" : string.Empty;    // 1 for L13 3 for L33
                document.Descendants(vehiclens + "PolicyStartDate").FirstOrDefault().Value = Convert.ToDateTime(model.PolicyStartDate).ToString("o");
                document.Descendants(vehiclens + "PolicyEndDate").FirstOrDefault().Value = Convert.ToDateTime(model.PolicyEndDate).ToString("o");
                document.Descendants(vehiclens + "PolicyTenure").FirstOrDefault().Value = (model.PolicyType.Equals("New") ? "3" : string.Empty);
                //selected covers
                var selectedcovers = document.Descendants(vehiclens + "SelectedCovers").FirstOrDefault();
                if (model.AddonCover != null)
                {
                    XNamespace ns = selectedcovers.GetDefaultNamespace();
                    selectedcovers.Elements(ns + "ZeroDepriciationSelected").FirstOrDefault().Value = (model.AddonCover.IsZeroDeperation) ? "True" : "False";
                    selectedcovers.Elements(ns + "RoadsideAssistanceSelected").FirstOrDefault().Value = (model.AddonCover.IsRoadSideAssistance) ? "True" : "False";
                    selectedcovers.Elements(ns + "InvoicePriceSelected").FirstOrDefault().Value = (model.AddonCover.IsReturntoInvoice) ? "True" : "False";
                    selectedcovers.Elements(ns + "HospitalCashSelected").FirstOrDefault().Value = (model.AddonCover.IsHospitalCashCover) ? "True" : "False";
                    selectedcovers.Elements(ns + "MedicalExpensesSelected").FirstOrDefault().Value = (model.AddonCover.IsMedicalExpensesSelected) ? "True" : "False";
                    selectedcovers.Elements(ns + "AmbulanceChargesSelected").FirstOrDefault().Value = (model.AddonCover.IsAmbulanceChargesSelected) ? "True" : "False";
                    selectedcovers.Elements(ns + "EngineGearBoxProtectionSelected").FirstOrDefault().Value = (model.AddonCover.IsEngineProtector) ? "True" : "False";
                    selectedcovers.Elements(ns + "HydrostaticLockSelected").FirstOrDefault().Value = (model.AddonCover.IsHydrostaticLockCover) ? "True" : "False";
                    selectedcovers.Elements(ns + "KeyReplacementSelected").FirstOrDefault().Value = (model.AddonCover.IsLossofKey) ? "True" : "False";
                    selectedcovers.Elements(ns + "NoClaimBonusSameSlabSelected").FirstOrDefault().Value = (model.AddonCover.IsNCBProtection) ? "True" : "False";
                    selectedcovers.Elements(ns + "CosumableCoverSelected").FirstOrDefault().Value = (model.AddonCover.IsConsumables) ? "True" : "False";
                    //
                }
                document.Descendants(vehiclens + "ClientType").FirstOrDefault().Value = model.CustomerType;
                document.Descendants(vehiclens + "CltDOB").FirstOrDefault().Value = !string.IsNullOrEmpty(model.ClientDetails.DateOfBirth) ? model.ClientDetails.DateOfBirth : string.Empty;
                document.Descendants(vehiclens + "GivName").FirstOrDefault().Value = !string.IsNullOrEmpty(model.ClientDetails.FirstName) ? givname + "_" + model.ClientDetails.FirstName : string.Empty;                  //need to be unique
                document.Descendants(vehiclens + "SurName").FirstOrDefault().Value = !string.IsNullOrEmpty(model.ClientDetails.LastName) ? model.ClientDetails.LastName : string.Empty;
                document.Descendants(vehiclens + "ClientExtraTag01").FirstOrDefault().Value = !string.IsNullOrEmpty(model.VehicleAddressDetails.State) ? model.VehicleAddressDetails.State : string.Empty;
                document.Descendants(vehiclens + "CityOfResidence").FirstOrDefault().Value = !string.IsNullOrEmpty(model.VehicleAddressDetails.City) ? model.VehicleAddressDetails.City : string.Empty;
                document.Descendants(vehiclens + "EmailID").FirstOrDefault().Value = !string.IsNullOrEmpty(model.ClientDetails.EmailId) ? model.ClientDetails.EmailId : string.Empty;
                document.Descendants(vehiclens + "MobileNo").FirstOrDefault().Value = !string.IsNullOrEmpty(model.ClientDetails.MobileNo) ? model.ClientDetails.MobileNo : string.Empty;
                document.Descendants(vehiclens + "CltSex").FirstOrDefault().Value = !string.IsNullOrEmpty(model.ClientDetails.Gender) ? model.ClientDetails.Gender : string.Empty;
                document.Descendants(vehiclens + "Marryd").FirstOrDefault().Value = !string.IsNullOrEmpty(model.ClientDetails.MaritalStatus) ? model.ClientDetails.MaritalStatus : string.Empty;
                document.Descendants(vehiclens + "Occupation").FirstOrDefault().Value = !string.IsNullOrEmpty(model.ClientDetails.Occupation) ? model.ClientDetails.Occupation : string.Empty;
                document.Descendants(vehiclens + "CltAddr01").FirstOrDefault().Value = !string.IsNullOrEmpty(model.CustomerAddressDetails.Address1) ? model.CustomerAddressDetails.Address1 : string.Empty;
                document.Descendants(vehiclens + "CltAddr02").FirstOrDefault().Value = !string.IsNullOrEmpty(model.CustomerAddressDetails.Address2) ? model.CustomerAddressDetails.Address2 : string.Empty;
                document.Descendants(vehiclens + "CltAddr03").FirstOrDefault().Value = !string.IsNullOrEmpty(model.CustomerAddressDetails.Address3) ? model.CustomerAddressDetails.Address3 : string.Empty;
                document.Descendants(vehiclens + "City").FirstOrDefault().Value = !string.IsNullOrEmpty(model.CustomerAddressDetails.City) ? model.CustomerAddressDetails.City : string.Empty;
                document.Descendants(vehiclens + "State").FirstOrDefault().Value = !string.IsNullOrEmpty(model.CustomerAddressDetails.State) ? model.CustomerAddressDetails.State : string.Empty;
                document.Descendants(vehiclens + "PinCode").FirstOrDefault().Value = !string.IsNullOrEmpty(model.CustomerAddressDetails.Pincode) ? model.CustomerAddressDetails.Pincode : string.Empty;
                document.Descendants(vehiclens + "Name").FirstOrDefault().Value = !string.IsNullOrEmpty(model.NomineeName) ? model.NomineeName : string.Empty;
                document.Descendants(vehiclens + "Age").FirstOrDefault().Value = !string.IsNullOrEmpty(model.NomineeDateOfBirth) ? (DateTime.Now.Year - Convert.ToDateTime(model.NomineeDateOfBirth).Year).ToString() : "0";
                document.Descendants(vehiclens + "Relationship").FirstOrDefault().Value = !string.IsNullOrEmpty(model.NomineeRelationShip) ? model.NomineeRelationShip : string.Empty;
                document.Descendants(vehiclens + "RegistrationZone").FirstOrDefault().Value = !string.IsNullOrEmpty(model.VehicleDetails.RtoZone) ? model.VehicleDetails.RtoZone : string.Empty;

                string url = "https://awpuat.bharti-axagi.co.in/home/B2C/com.eibus.web.soap.Gateway.wcp?organization=o=B2C,cn=cordys,cn=defaultInst106,o=mydomain.com";

                string requestData = document.ToString();
                var response = CallWebAPI(requestData, url);
                ResModel = GetProposalResponse(response);
                ResModel.IDV = model.IDV;
                ResModel.CC = model.VehicleDetails.CC;
                ResModel.SC = model.VehicleDetails.SC;
                ResModel.FuelType = model.VehicleDetails.Fuel;
                ResModel.PolicyStartDate = model.PolicyStartDate;
                ResModel.PolicyEndDate = model.PolicyEndDate;
            }
            catch (Exception ex)
            {
                ResModel.Status = Status.Fail;
                ResModel.ErrorMsg = ex.ToString();

            }
            return ResModel;
        }
        public Response GetProposalResponse(List<XElement> response)
        {
            Response resModel = new Response();
            PremiumBreakUpDetails premdata = new PremiumBreakUpDetails();
            CompanyWiseRefference compdata = new CompanyWiseRefference();
            try
            {
                if (response.Count > 0)
                {
                    string responsedata = response[0].ToString();
                    XmlReader xmlReader = XmlReader.Create(new StringReader(responsedata));
                    DataSet ds = new DataSet();
                    ds.ReadXml(xmlReader);

                    if (ds.Tables.Contains("response"))
                    {
                        DataTable dt_response = ds.Tables["response"];
                        //  List<AddonDetails> List_objent = new List<AddonDetails>();

                        string StatusMsg = dt_response.Rows[0]["StatusMsg"].ToString();
                        string StatusCode = dt_response.Rows[0]["StatusCode"].ToString();
                        if (StatusMsg == "Success" && StatusCode == "200")
                        {
                            resModel.Status = Status.Success;

                            compdata.OrderNo = dt_response.Rows[0]["OrderNo"].ToString();
                            compdata.QuoteNo = dt_response.Rows[0]["QuoteNo"].ToString();
                            DataTable dt_premium = ds.Tables["PremiumDetails"];
                            DataTable dt_cover = ds.Tables["Cover"];
                            DataTable dt_breakup = ds.Tables["Breakup"];

                            double addon = 0;
                            var basicod = 0.0;
                            for (int i = 0; i < dt_cover.Rows.Count; i++)
                            {
                                string covertype = dt_cover.Rows[i]["Type"].ToString();
                                string covername = dt_cover.Rows[i]["Name"].ToString();
                                string Premium = dt_cover.Rows[i]["Premium"].ToString();
                                if (covertype == "Basic" && covername == "CarDamage")
                                {

                                    premdata.NetODPremium = Convert.ToDouble(Premium);
                                    premdata.ElecAccessoriesPremium = Convert.ToDouble(dt_breakup.Rows[0]["Accessory"].ToString());
                                    premdata.NonElecAccessoriesPremium = Convert.ToDouble(dt_breakup.Rows[0]["NonElecAccessory"].ToString());
                                    premdata.CNGLPGKitPremium = Convert.ToDouble(dt_breakup.Rows[0]["BiFuel"].ToString());
                                }
                                if (covertype == "Basic" && covername == "ThirdPartyLiability")
                                {
                                    premdata.NetTPPremium = Convert.ToDouble(Premium);
                                }
                                if (covertype == "Basic" && covername == "PAOwnerDriver")
                                {
                                    premdata.PACoverToOwnDriver = Convert.ToDouble(Premium);
                                }
                                if (covertype == "Addon" && covername == "CONC")
                                {
                                    premdata.CostOfConsumablesPremium = Convert.ToDouble(Premium);
                                    addon = addon + premdata.CostOfConsumablesPremium;
                                }
                                if (covertype == "Addon" && covername == "KEYC")
                                {
                                    premdata.KeyReplacementPremium = Convert.ToDouble(Premium);
                                    addon = addon + premdata.KeyReplacementPremium;
                                }
                                if (covertype == "Addon" && covername == "RSAP")
                                {
                                    premdata.RSAPremium = Convert.ToDouble(Premium);
                                    addon = addon + premdata.RSAPremium;
                                }
                                if (covertype == "Addon" && covername == "AMBC")
                                {
                                    premdata.AmbulanceChargesPremium = Convert.ToDouble(Premium);
                                    addon = addon + premdata.AmbulanceChargesPremium;
                                }
                                if (covertype == "Addon" && covername == "HOSP")
                                {
                                    premdata.HospitalCashCoverPremium = Convert.ToDouble(Premium);
                                    addon = addon + premdata.HospitalCashCoverPremium;
                                }
                                if (covertype == "Addon" && covername == "MEDI")
                                {
                                    premdata.MedicalExpensesPremium = Convert.ToDouble(Premium);
                                    addon = addon + premdata.MedicalExpensesPremium;
                                }
                            }
                            premdata.NetAddonPremium = addon;

                            basicod = Convert.ToDouble(dt_breakup.Rows[0]["BasicOD"].ToString());
                            premdata.BasicODPremium = basicod - addon;
                            int discount = Convert.ToInt16(ds.Tables["PremiumSet"].Rows[0]["Discount"].ToString().Substring(1));

                            premdata.ElecAccessoriesPremium = Convert.ToDouble(dt_breakup.Rows[0]["Accessory"].ToString());
                            premdata.NonElecAccessoriesPremium = Convert.ToDouble(dt_breakup.Rows[0]["NonElecAccessory"].ToString());
                            premdata.CNGLPGKitPremium = Convert.ToDouble(dt_breakup.Rows[0]["BiFuel"].ToString());
                            premdata.AntiTheftDiscount = Convert.ToDouble(dt_breakup.Rows[0]["AntiTheft"].ToString());
                            premdata.OtherDiscount = Math.Round(((premdata.BasicODPremium * discount) / 100), 0);
                            premdata.NCBDiscount = Convert.ToDouble(dt_breakup.Rows[0]["NCB"].ToString());
                            premdata.BasicThirdPartyLiability = Convert.ToDouble(dt_breakup.Rows[1]["TP"].ToString());
                            premdata.LLToPaidDriver = Convert.ToDouble(dt_breakup.Rows[1]["LLDriver"].ToString());
                            premdata.TPCNGLPGPremium = Convert.ToDouble(dt_breakup.Rows[1]["TPBiFuel"].ToString());

                            premdata.NetDiscount = premdata.NCBDiscount + premdata.OtherDiscount + premdata.AntiTheftDiscount + premdata.VoluntaryDiscount;
                            premdata.NetPremium = Convert.ToDouble(dt_premium.Rows[0]["Premium"].ToString());
                            resModel.FinalPremium = Convert.ToInt32(dt_premium.Rows[0]["TotalPremium"].ToString());
                            resModel.CompanyName = Company.BHARTI.ToString();
                            resModel.Product = Product.Motor;
                            resModel.SubProduct = SubProduct.PrivateCar;
                            premdata.ServiceTax = (int)Convert.ToDouble(ds.Tables["PremiumSet"].Rows[0]["ServiceTax"].ToString());
                            premdata.NetTPPremium = premdata.NetTPPremium + premdata.PACoverToOwnDriver;

                            resModel.CompanyWiseRefference = compdata;
                            resModel.PremiumBreakUpDetails = premdata;
                            resModel.EnquiryId = control.GenerateEnquiryId();
                            //   premdata.BasicODPremium =

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                resModel.Status = Status.Fail;
                resModel.ErrorMsg = ex.ToString();
                throw;
            }
            return resModel;

        }
        public List<XElement> CallWebAPI(object obj, string url)
        {
            string requeststring = obj.ToString();
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/json";
            byte[] data = Encoding.UTF8.GetBytes(requeststring);
            Stream datastream = request.GetRequestStream();
            datastream.Write(data, 0, data.Length);
            datastream.Close();
            WebResponse response = request.GetResponse();
            response = request.GetResponse();
            string result = new StreamReader(response.GetResponseStream()).ReadToEnd();

            XmlReader xmlReader_result = XmlReader.Create(new StringReader(result));
            XDocument xdoc = XDocument.Load(xmlReader_result);

            XNamespace ns = "http://schemas.cordys.com/default";
            var responses = xdoc.Descendants(ns + "processTPRequestResponse").ToList();
            return responses;

        }
    }
}