/// <summary>
/// This class and all methods are reserved by AndApp.
/// </summary>
/// 
namespace AndWebApi.FGI
{
    #region namespace
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Xml;
    using System.Xml.XPath;
    using System.Xml.Linq;
    using System.IO;
    using AndApp;
    using Controllers;
    using System.Web.Script.Serialization;
    using AndApp.Utilities;
    using System.Data;
    using System.ServiceModel;
    using Models;
    using System.Globalization;
    using System.Configuration;
    #endregion

    /// <summary>
    /// This class contains methods for quotation,proposal,payment & policy pdf.
    /// </summary>
    public class PrivateCar
    {
        /// <summary>
        /// Future generali service reference object.
        /// </summary>
        public FutureService.ServiceClient Ser = new FutureService.ServiceClient();
        DAL.ANDAPPEntities ap = new DAL.ANDAPPEntities();

        /// <summary>
        /// Quotes request method.
        /// </summary>
        /// <param name="model">Object of quotation model.</param>
        /// <returns>return response type object.</returns>
        /// 
        public Response GetQuoteRequest(Quotation model)
        {
            Response resModel = new Response();
            XmlDocument doc = new XmlDocument();
            var sdate = Convert.ToDateTime(model.PolicyStartDate).ToString("yyyy-MM-dd");
            model.PolicyEndDate = Convert.ToDateTime(model.PolicyStartDate).AddYears(3).AddDays(-1).ToString("yyyy-MM-dd");
            try
            {
                if (model.PolicyType.Equals("New"))
                {
                    //model.PolicyEndDate = Convert.ToDateTime(model.PolicyStartDate).AddYears(3).AddDays(-2).ToString("yyyy-MM-dd");
                    model.PolicyEndDate = Convert.ToDateTime(sdate).AddYears(3).AddDays(-1).ToString("yyyy-MM-dd");
                }
                else
                {
                    model.PolicyEndDate = Convert.ToDateTime(sdate).AddYears(1).AddDays(-1).ToString("yyyy-MM-dd");
                }
                string path = AppDomain.CurrentDomain.BaseDirectory;
                string filePath = Path.Combine(path, "XML/FGI/Quote.xml");
                var document = XDocument.Load(filePath);
                if (model.IDV != 0)
                {
                    if (model.CustomIDV != null)
                    {
                        var getidvvalues = model.CustomIDV.Where(x => x.CompanyName == "FGI").FirstOrDefault();
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
                document.XPathSelectElement("//PolicyStartDate").Value = Convert.ToDateTime(sdate).ToString("dd/MM/yyyy");
                document.XPathSelectElement("//PolicyEndDate").Value = Convert.ToDateTime(model.PolicyEndDate).ToString("dd/MM/yyyy");
                document.XPathSelectElement("//Uid").Value = Convert.ToString(GenerateRandomNo());
                if (!model.PolicyType.Equals("New"))
                {
                    //document.XPathSelectElement("//ContractType").Value = model.IsODOnly ? "FVO" : "FPV";
                    //document.XPathSelectElement("//RiskType").Value = model.IsODOnly ? "FVO" : "FPV";
                    document.XPathSelectElement("//ContractType").Value = model.IsODOnly ? "PVO" : "PPV";
                    document.XPathSelectElement("//RiskType").Value = model.IsODOnly ? "FVO" : "FPV";
                    document.XPathSelectElement("//NCB").Value = Convert.ToString(model.CurrentNcb);
                    if (model.VehicleDetails.RegistrationNumber.Length > 4)
                    {
                        model.VehicleDetails.RegistrationNumber = model.VehicleDetails.RegistrationNumber;
                    }
                    else
                    {
                        model.VehicleDetails.RegistrationNumber = model.VehicleDetails.RegistrationNumber + "AB1125";
                    }
                    document.XPathSelectElement("//RegistrationNo").Value = !string.IsNullOrEmpty(model.VehicleDetails.RegistrationNumber) ? model.VehicleDetails.RegistrationNumber.ToUpper() : "DL01AB1221";
                }
                if (!model.CustomerType.ToUpper().Equals("INDIVIDUAL"))
                {
                    model.ClientDetails = new ClientDetails();
                    model.ClientDetails.FirstName = "M/s";
                    model.ClientDetails.LastName = model.OrganizationName;
                }
                document.XPathSelectElement("//Salutation").Value = model.ClientDetails != null ? ((string.IsNullOrEmpty(model.ClientDetails.Salutation)) ? "MR" : model.ClientDetails.Salutation) : "MR";
                document.XPathSelectElement("//ClientType").Value = string.IsNullOrEmpty(model.CustomerType) ? "I" : (model.CustomerType.Equals("Individual") ? "I" : "C");
                document.XPathSelectElement("//FirstName").Value = model.ClientDetails != null ? ((string.IsNullOrEmpty(model.ClientDetails.FirstName)) ? "TestFName" : model.ClientDetails.FirstName) : "TestFName";
                document.XPathSelectElement("//LastName").Value = model.ClientDetails != null ? ((string.IsNullOrEmpty(model.ClientDetails.LastName)) ? "TestLName" : model.ClientDetails.LastName) : "TestLName";
                document.XPathSelectElement("//DOB").Value = model.ClientDetails != null ? ((string.IsNullOrEmpty(model.ClientDetails.DateOfBirth)) ? "06/01/2002" : Convert.ToDateTime(model.ClientDetails.DateOfBirth).ToString("dd/MM/yyyy")) : "01/02/2002";
                document.XPathSelectElement("//Gender").Value = model.ClientDetails != null ? ((string.IsNullOrEmpty(model.ClientDetails.Gender)) ? "M" : model.ClientDetails.Gender) : "M";
                document.XPathSelectElement("//MaritalStatus").Value = model.ClientDetails != null ? ((string.IsNullOrEmpty(model.ClientDetails.MaritalStatus)) ? "S" : model.ClientDetails.MaritalStatus) : "S";
                document.XPathSelectElement("//Occupation").Value = model.ClientDetails != null ? ((string.IsNullOrEmpty(model.ClientDetails.Occupation)) ? "SVCM" : model.ClientDetails.Occupation) : "SVCM";
                document.XPathSelectElement("//PANNo").Value = model.ClientDetails != null ? ((string.IsNullOrEmpty(model.ClientDetails.PanCardNo)) ? string.Empty : model.ClientDetails.PanCardNo) : string.Empty;
                document.XPathSelectElement("//GSTIN").Value = model.ClientDetails != null ? ((string.IsNullOrEmpty(model.ClientDetails.GSTIN)) ? string.Empty : model.ClientDetails.GSTIN) : string.Empty;
                document.XPathSelectElement("//AadharNo").Value = model.ClientDetails != null ? ((string.IsNullOrEmpty(model.ClientDetails.AadharNo)) ? string.Empty : model.ClientDetails.AadharNo) : string.Empty;
                document.XPathSelectElement("//AddrLine1").Value = model.VehicleAddressDetails != null ? model.VehicleAddressDetails.Address1 : string.Empty;
                document.XPathSelectElement("//AddrLine2").Value = model.VehicleAddressDetails != null ? model.VehicleAddressDetails.Address2 : string.Empty;
                document.XPathSelectElement("//AddrLine3").Value = model.VehicleAddressDetails != null ? (string.IsNullOrEmpty(model.VehicleAddressDetails.Address3) ? string.Empty : model.VehicleAddressDetails.Address3) : string.Empty;
                document.XPathSelectElement("//Pincode").Value = model.VehicleAddressDetails != null ? model.VehicleAddressDetails.Pincode : string.Empty;
                document.XPathSelectElement("//City").Value = model.VehicleAddressDetails != null ? model.VehicleAddressDetails.City : string.Empty;
                document.XPathSelectElement("//State").Value = model.VehicleAddressDetails != null ? model.VehicleAddressDetails.State : string.Empty;
                document.XPathSelectElement("//MobileNo").Value = model.ClientDetails != null ? ((string.IsNullOrEmpty(model.ClientDetails.MobileNo)) ? string.Empty : model.ClientDetails.MobileNo) : string.Empty;
                document.XPathSelectElement("//EmailAddr").Value = model.ClientDetails != null ? ((string.IsNullOrEmpty(model.ClientDetails.EmailId)) ? "trupti@nodib.com" : model.ClientDetails.EmailId) : "trupti@nodib.com";
                document.XPathSelectElement("//Zone").Value = model.VehicleDetails != null ? model.VehicleDetails.RtoZone : string.Empty;
                //document.XPathSelectElement("//RTOCode").Value = model.VehicleDetails != null ? model.VehicleDetails.RtoZone : string.Empty;
                string RTOCode = string.Empty;
                if (string.IsNullOrEmpty(Convert.ToString(ConfigurationManager.AppSettings["Envi"])))
                {
                    document.XPathSelectElement("//RTOCode").Value = "DL01";
                    document.XPathSelectElement("//POS_MISP/PanNo").Value = "DHNPG6287W";
                }
                else
                {
                    if (Convert.ToString(ConfigurationManager.AppSettings["Envi"]).ToUpper().Equals("UAT"))
                    {
                        document.XPathSelectElement("//RTOCode").Value = "DL01";
                        document.XPathSelectElement("//POS_MISP/PanNo").Value = "DHNPG6287W";
                    }
                    else
                    {

                        var rtocode = ap.COMPANY_WISE_RTO_MASTER.Where(x => x.companyid == 5 && x.andapp_rtoid == model.VehicleDetails.RtoId).FirstOrDefault();
                        //var rtocode = ap.company_wise_rto_master.where(x => x.companyid == 5 && x.andapp_rtoid == model.vehicledetails.rtoid).firstordefault();
                        if (rtocode != null)
                        {
                            RTOCode = rtocode.rto_loc_code;
                        }
                        document.XPathSelectElement("//RTOCode").Value = RTOCode;
                        string pancard = string.Empty;
                        pancard = ap.POSPMASTERs.Where(x => x.pospid == model.pospid).Select(x => x.pancardno).FirstOrDefault();
                        document.XPathSelectElement("//POS_MISP/PanNo").Value = pancard;
                    }
                }

                //document.XPathSelectElement("//RTOCode").Value = "DL01";
                document.XPathSelectElement("//Make").Value = model.VehicleDetails != null ? model.VehicleDetails.MakeName : string.Empty;
                document.XPathSelectElement("//ModelCode").Value = model.VehicleDetails != null ? model.VehicleDetails.VariantCode : string.Empty;
                document.XPathSelectElement("//RegistrationDate").Value = Convert.ToDateTime(model.VehicleDetails.RegistrationDate).ToString("dd/MM/yyyy");
                //document.XPathSelectElement("//RegistrationDate").Value = Convert.ToString(dateParts[2] + "-" + dateParts[1] + "-" + dateParts[0]);//Convert.ToDateTime(model.VehicleDetails.RegistrationDate).ToString("yyyy-MM-dd");
                document.XPathSelectElement("//ManufacturingYear").Value = Convert.ToString(Convert.ToDateTime(model.VehicleDetails.ManufaturingDate).Year);
                string fuel = Convert.ToString(model.VehicleDetails.Fuel).Substring(0, 1);
                document.XPathSelectElement("//FuelType").Value = fuel;
                //document.XPathSelectElement("//InbuiltKit").Value = model.CoverageDetails != null ? (model.CoverageDetails.IsBiFuelKit ? "N" : (fuel.Equals("C")? "Y" :"N")) : "N";
                document.XPathSelectElement("//IVDOfCNGOrLPG").Value = model.CoverageDetails != null ? (model.CoverageDetails.IsBiFuelKit ? Convert.ToString(model.CoverageDetails.BiFuelKitAmount) : "0") : "0";
                document.XPathSelectElement("//IDV").Value = Convert.ToString(model.IDV);
                document.XPathSelectElement("//BodyType").Value = model.VehicleDetails != null ? ((string.IsNullOrEmpty(model.VehicleDetails.BodyType)) ? "SOLO" : model.VehicleDetails.BodyType) : "SOLO";
                document.XPathSelectElement("//EngineNo").Value = model.VehicleDetails != null ? ((string.IsNullOrEmpty(model.VehicleDetails.EngineNumber)) ? "5154545445456" : model.VehicleDetails.EngineNumber) : "Engineno123456789";
                document.XPathSelectElement("//ChassiNo").Value = model.VehicleDetails != null ? ((string.IsNullOrEmpty(model.VehicleDetails.ChassisNumber)) ? "FCFGFFGG95467655656565" : model.VehicleDetails.ChassisNumber) : "Chassisno123456789";
                document.XPathSelectElement("//CubicCapacity").Value = model.VehicleDetails != null ? ((string.IsNullOrEmpty(Convert.ToString(model.VehicleDetails.CC))) ? "0" : Convert.ToString(model.VehicleDetails.CC)) : "0";
                document.XPathSelectElement("//SeatingCapacity").Value = model.VehicleDetails != null ? ((string.IsNullOrEmpty(Convert.ToString(model.VehicleDetails.SC))) ? "0" : Convert.ToString(model.VehicleDetails.SC)) : "0";
                document.XPathSelectElement("//ElectricalAccessoriesValues").Value = model.CoverageDetails != null ? (model.CoverageDetails.IsElectricalAccessories ? Convert.ToString(model.CoverageDetails.SIElectricalAccessories) : "0") : "0";
                document.XPathSelectElement("//NonElectricalAccessoriesValues").Value = model.CoverageDetails != null ? (model.CoverageDetails.IsNonElectricalAccessories ? Convert.ToString(model.CoverageDetails.SINonElectricalAccessories) : "0") : "0";
                //document.XPathSelectElement("//PACoverForUnnamedPassengers").Value = model.CoverageDetails.IsPACoverUnnamedPerson ? Convert.ToString(model.CoverageDetails.NumberofPersonsUnnamed) : "0";
                //document.XPathSelectElement("//LegalLiabilitytoPaidDriver").Value = model.CoverageDetails.NoOfLLPaidDriver > 0 ? Convert.ToString(model.CoverageDetails.NoOfLLPaidDriver) : "1";
                document.XPathSelectElement("//LegalLiabilityForOtherEmployees").Value = model.CoverageDetails.IsEmployeeLiability ? "1" : "0";

                document.XPathSelectElement("//CPAYear").Value = model.PolicyType.Equals("New") ? "3" : string.Empty;
                document.XPathSelectElement("//CPAReq").Value = model.CoverageDetails.IsPACoverForOwnerDriver ? "Y" : "N";
                //document.XPathSelectElement("//CPANomName").Value = string.IsNullOrEmpty(model.NomineeName) ? string.Empty : model.NomineeName;
                //document.XPathSelectElement("//CPANomAge").Value = string.IsNullOrEmpty(model.NomineeDateOfBirth) ? "0" : Convert.ToString(CalculateAge(Convert.ToDateTime(model.NomineeDateOfBirth)));
                //document.XPathSelectElement("//CPARelation").Value = string.IsNullOrEmpty(model.NomineeRelationShip) ? string.Empty : model.NomineeRelationShip;
                //document.XPathSelectElement("//CPAAppointeeName").Value = string.IsNullOrEmpty(model.AppointeeName) ? string.Empty : model.AppointeeName;
                document.XPathSelectElement("//RollOver").Value = model.PolicyType.Equals("New", StringComparison.OrdinalIgnoreCase) ? "N" : "Y";
                //document.XPathSelectElement("//InsuredName").Value = model.PreviousPolicyDetails != null ? ((string.IsNullOrEmpty(model.PreviousPolicyDetails.PreviousCompanyName)) ? "0" : model.PreviousPolicyDetails.PreviousCompanyName) : "0";
                //document.XPathSelectElement("//PreviousPolExpDt").Value = model.PreviousPolicyDetails != null ? model.PreviousPolicyDetails.PreviousPolicyEndDate : "0001-01-01";
                //document.XPathSelectElement("//NCBInExpiringPolicy").Value = model.PreviousPolicyDetails != null ? model.PreviousPolicyDetails.PreviousNcbPercentage : "0";
                //document.XPathSelectElement("//ClaimInExpiringPolicy").Value = model.PreviousPolicyDetails != null ? (model.PreviousPolicyDetails.IsPreviousInsuranceClaimed ? "Y" : "N") : "N";
                document.XPathSelectElement("//NewVehicle").Value = model.PolicyType.Equals("New", StringComparison.OrdinalIgnoreCase) ? "Y" : "N";
                //document.XPathSelectElement("//PACoverForUnnamedPassengers").Value = model.CoverageDetails.IsPACoverUnnamedPerson ? Convert.ToString(model.VehicleDetails.SC) : "0";
                document.XPathSelectElement("//LegalLiabilitytoPaidDriver").Value = model.CoverageDetails.NoOfLLPaidDriver > 0 ? Convert.ToString(model.CoverageDetails.NoOfLLPaidDriver) : "0";
                //document.XPathSelectElement("//LegalLiabilityForOtherEmployees").Value = model.CoverageDetails.IsLLEmployee ? "1" : "0";
                //if (model.IsODOnly && model.PreviousTPPolicyDetails != null)
                //{
                //    DateTime PrvTPPolicyStartDate = Convert.ToDateTime(model.PreviousTPPolicyDetails.PolicyEndDate).AddYears(-3).AddDays(1);
                //    document.XPathSelectElement("//Cover").Value = "OD";
                //    document.XPathSelectElement("//CPAReq").Value = "N";
                //    document.XPathSelectElement("//ContractType").Value = "FVO";
                //    document.XPathSelectElement("//RiskType").Value = "FVO";
                //    document.XPathSelectElement("//PACoverForUnnamedPassengers").Value = "0";
                //    document.XPathSelectElement("//LegalLiabilityForOtherEmployees").Value = "0";
                //    document.XPathSelectElement("//PreviousInsurer").Value = "Bajaj Allianz General Insurance Co Ltd.";
                //    document.XPathSelectElement("//TPPolicyNumber").Value = model.PreviousTPPolicyDetails.PolicyNo;
                //    document.XPathSelectElement("//TPPolicyEffdate").Value = PrvTPPolicyStartDate.ToString("dd/MM/yyyy");
                //    document.XPathSelectElement("//TPPolicyExpiryDate").Value = Convert.ToDateTime(model.PreviousTPPolicyDetails.PolicyEndDate).ToString("dd/MM/yyyy");
                //}
                if (model.IsODOnly && model.PreviousTPPolicyDetails != null)
                {
                    var PrvTPPolicyStartDate = Convert.ToDateTime(model.PreviousTPPolicyDetails.PolicyEndDate).AddYears(-3).AddDays(1).ToString("yyyy-MM-dd");
                    //DateTime PrvTPPolicyStartDate = Convert.ToDateTime(model.PreviousTPPolicyDetails.PolicyEndDate).AddYears(-3).AddDays(1);
                    document.XPathSelectElement("//Cover").Value = "OD";
                    document.XPathSelectElement("//CPAReq").Value = "N";
                    document.XPathSelectElement("//PACoverForUnnamedPassengers").Value = "0";
                    document.XPathSelectElement("//LegalLiabilityForOtherEmployees").Value = "0";
                    string TPInsName = string.Empty;
                    if (string.IsNullOrEmpty(Convert.ToString(ConfigurationManager.AppSettings["Envi"])))
                    {
                        document.XPathSelectElement("//PreviousInsurer").Value = "Bajaj Allianz General Insurance Co Ltd.";
                    }
                    else
                    {
                        if (Convert.ToString(ConfigurationManager.AppSettings["Envi"]).ToUpper().Equals("UAT"))
                        {

                            document.XPathSelectElement("//PreviousInsurer").Value = "Bajaj Allianz General Insurance Co Ltd.";
                        }
                        else
                        {
                            var TPcompanycode = ap.PREVIOUS_INSURER_MAPPING.Where(x => x.companyid == 5 && x.previouscompanyid == model.PreviousTPPolicyDetails.CompanyId).FirstOrDefault();
                            if (TPcompanycode != null)
                            {

                                TPInsName = Convert.ToString(TPcompanycode.inscompanyname);
                            }
                            else
                            {
                                resModel.Status = Status.Fail;
                                resModel.ErrorMsg = "FGI not provide previous insurance company";
                                return resModel;
                            }
                            document.XPathSelectElement("//PreviousInsurer").Value = TPInsName;
                        }
                    }
                    //document.XPathSelectElement("//PreviousInsurer").Value = "Bajaj Allianz General Insurance Co Ltd.";
                    document.XPathSelectElement("//TPPolicyNumber").Value = model.PreviousTPPolicyDetails.PolicyNo;
                    document.XPathSelectElement("//TPPolicyEffdate").Value = Convert.ToDateTime(PrvTPPolicyStartDate).ToString("dd/MM/yyyy");
                    document.XPathSelectElement("//TPPolicyExpiryDate").Value = Convert.ToDateTime(model.PreviousTPPolicyDetails.PolicyEndDate).ToString("dd/MM/yyyy");
                }
                if (model.IsThirdPartyOnly)
                {
                    document.XPathSelectElement("//Cover").Value = "LO";
                    document.XPathSelectElement("//AddonReq").Value = "N";
                }
                if (!model.PolicyType.Equals("New") && model.PreviousPolicyDetails != null)
                {
                    string InsCode = string.Empty;
                    string InsName = string.Empty;

                    if (string.IsNullOrEmpty(Convert.ToString(ConfigurationManager.AppSettings["Envi"])))
                    {

                        document.XPathSelectElement("//InsuredName").Value = "Bajaj Allianz General Insurance Co Ltd.";
                        document.XPathSelectElement("//ClientCode").Value = "40062645";
                    }
                    else
                    {
                        if (Convert.ToString(ConfigurationManager.AppSettings["Envi"]).ToUpper().Equals("UAT"))
                        {

                            document.XPathSelectElement("//InsuredName").Value = "Bajaj Allianz General Insurance Co Ltd.";
                            document.XPathSelectElement("//ClientCode").Value = "40062645";
                        }
                        else
                        {
                            var companycode = ap.PREVIOUS_INSURER_MAPPING.Where(x => x.companyid == 5 && x.previouscompanyid == model.PreviousPolicyDetails.CompanyId).FirstOrDefault();
                            if (companycode != null)
                            {
                                InsCode = Convert.ToString(companycode.inscompanycode);
                                InsName = Convert.ToString(companycode.inscompanyname);
                            }
                            else
                            {
                                resModel.Status = Status.Fail;
                                resModel.ErrorMsg = "FGI not provide previous insurance company";
                                return resModel;
                            }
                            document.XPathSelectElement("//InsuredName").Value = InsName;
                            document.XPathSelectElement("//ClientCode").Value = InsCode;
                        }
                    }


                    var sdate1 = Convert.ToDateTime(model.PreviousPolicyDetails.PreviousPolicyEndDate).AddYears(-1).AddDays(1).ToString("yyyy-MM-dd");
                    document.XPathSelectElement("//RollOverList/PolicyNo").Value = model.PreviousPolicyDetails.PreviousPolicyNo;
                    document.XPathSelectElement("//PreviousPolExpDt").Value = Convert.ToDateTime(model.PreviousPolicyDetails.PreviousPolicyEndDate).ToString("dd/MM/yyyy");
                    document.XPathSelectElement("//NCBDeclartion").Value = model.PreviousPolicyDetails.IsPreviousInsuranceClaimed ? "N" : "Y";
                    document.XPathSelectElement("//ClaimInExpiringPolicy").Value = model.PreviousPolicyDetails.IsPreviousInsuranceClaimed ? "Y" : "N";
                    document.XPathSelectElement("//NCBInExpiringPolicy").Value = model.PreviousPolicyDetails.PreviousNcbPercentage;
                    document.XPathSelectElement("//PreviousPolStartDt").Value = Convert.ToDateTime(sdate1).ToString("dd/MM/yyyy");
                }
                bool flag = false;
                int vehiAge = CalculateAge(Convert.ToDateTime(model.VehicleDetails.RegistrationDate));
                if (model.AddonCover.IsRoadSideAssistance)
                {
                    if (model.AddonCover.IsZeroDeperation || model.AddonCover.IsConsumables || model.AddonCover.IsTyreCover || model.AddonCover.IsConsumables ||
                        model.AddonCover.IsTyreCover || model.AddonCover.IsEngineProtector || model.AddonCover.IsReturntoInvoice ||
                    model.AddonCover.IsLossofKey || model.AddonCover.IsLossofpersonalBelonging || model.AddonCover.IsPassengerAssistcover || model.AddonCover.IsHydrostaticLockCover
                    || model.AddonCover.IsHospitalCashCover || model.AddonCover.IsRimProtectionCover
                    || model.AddonCover.IsEmergencyCover
                    || model.AddonCover.IsMedicalExpensesSelected ||
                    model.AddonCover.IsAmbulanceChargesSelected)
                    {
                        flag = true;
                    }
                    if (!flag)
                    {
                        if (vehiAge <= 15)
                        {
                            document.XPathSelectElement("//AddonReq").Value = "Y";
                            document.XPathSelectElement("//CoverCode").Value = "PLAN2";
                            //document.Root.Element("Risk").Element("AddonReq").AddAfterSelf(new XElement("Addon", new XElement("CoverCode", "PLAN2")));
                        }

                    }
                    else
                    {
                        if (vehiAge <= 5)
                        {
                            if (model.AddonCover.IsZeroDeperation || model.AddonCover.IsLossofKey || model.AddonCover.IsLossofpersonalBelonging || model.AddonCover.IsRoadSideAssistance)
                            {
                                document.XPathSelectElement("//AddonReq").Value = "Y";
                                //document.Root.Element("Risk").Element("AddonReq").AddAfterSelf(new XElement("Addon", new XElement("CoverCode", "PLAN1")));
                            }
                            if (model.AddonCover.IsEngineProtector)
                            {
                                if (vehiAge <= 3)
                                {
                                    document.XPathSelectElement("//AddonReq").Value = "Y";
                                    document.Root.Element("Risk").Element("AddonReq").AddAfterSelf(new XElement("Addon", new XElement("CoverCode", "ENGPR")));
                                }

                            }
                            if (model.AddonCover.IsTyreCover)
                            {
                                if (vehiAge <= 2)
                                {
                                    document.XPathSelectElement("//AddonReq").Value = "Y";
                                    document.Root.Element("Risk").Element("AddonReq").AddAfterSelf(new XElement("Addon", new XElement("CoverCode", "00001")));
                                }
                            }
                            if (model.AddonCover.IsNCBProtection)
                            {
                                document.XPathSelectElement("//AddonReq").Value = "Y";
                                document.Root.Element("Risk").Element("AddonReq").AddAfterSelf(new XElement("Addon", new XElement("CoverCode", "00004")));
                            }
                            if (model.AddonCover.IsConsumables)
                            {
                                document.XPathSelectElement("//AddonReq").Value = "Y";
                                document.Root.Element("Risk").Element("AddonReq").AddAfterSelf(new XElement("Addon", new XElement("CoverCode", "00005")));
                            }
                            if (model.AddonCover.IsReturntoInvoice)
                            {
                                if (vehiAge <= 3)
                                {
                                    document.XPathSelectElement("//AddonReq").Value = "Y";
                                    document.Root.Element("Risk").Element("AddonReq").AddAfterSelf(new XElement("Addon", new XElement("CoverCode", "00006")));
                                }

                            }
                        }
                        //else
                        //{
                        //    resModel.Status = Status.Fail;
                        //    resModel.ErrorMsg = "Can not provided add on for vehicle age more then 5 year";
                        //    Console.Write(Convert.ToString(resModel.ErrorMsg));
                        //    LogU.WriteLog("FGI >> PrivateCar >> GetQuoteRequest >> " + Convert.ToString(resModel.ErrorMsg));
                        //    return resModel;
                        //}

                    }
                }
                else
                {
                    if (vehiAge <= 5)
                    {
                        if (model.AddonCover.IsZeroDeperation || model.AddonCover.IsLossofKey || model.AddonCover.IsLossofpersonalBelonging || model.AddonCover.IsRoadSideAssistance)
                        {
                            document.XPathSelectElement("//AddonReq").Value = "Y";
                            //document.Root.Element("Risk").Element("AddonReq").AddAfterSelf(new XElement("Addon", new XElement("CoverCode", "PLAN1")));
                        }
                        if (model.AddonCover.IsEngineProtector)
                        {
                            if (vehiAge <= 3)
                            {
                                document.XPathSelectElement("//AddonReq").Value = "Y";
                                document.Root.Element("Risk").Element("AddonReq").AddAfterSelf(new XElement("Addon", new XElement("CoverCode", "ENGPR")));
                            }

                        }
                        if (model.AddonCover.IsTyreCover)
                        {
                            if (vehiAge <= 2)
                            {
                                document.XPathSelectElement("//AddonReq").Value = "Y";
                                document.Root.Element("Risk").Element("AddonReq").AddAfterSelf(new XElement("Addon", new XElement("CoverCode", "00001")));
                            }
                        }
                        if (model.AddonCover.IsNCBProtection)
                        {
                            document.XPathSelectElement("//AddonReq").Value = "Y";
                            document.Root.Element("Risk").Element("AddonReq").AddAfterSelf(new XElement("Addon", new XElement("CoverCode", "00004")));
                        }
                        if (model.AddonCover.IsConsumables)
                        {
                            document.XPathSelectElement("//AddonReq").Value = "Y";
                            document.Root.Element("Risk").Element("AddonReq").AddAfterSelf(new XElement("Addon", new XElement("CoverCode", "00005")));
                        }
                        if (model.AddonCover.IsReturntoInvoice)
                        {
                            if (vehiAge <= 3)
                            {
                                document.XPathSelectElement("//AddonReq").Value = "Y";
                                document.Root.Element("Risk").Element("AddonReq").AddAfterSelf(new XElement("Addon", new XElement("CoverCode", "00006")));
                            }

                        }
                    }
                    //else
                    //{
                    //    resModel.Status = Status.Fail;
                    //    resModel.ErrorMsg = "Can not provided add on for vehicle age more then 5 year";
                    //    Console.Write(Convert.ToString(resModel.ErrorMsg));
                    //    LogU.WriteLog("FGI >> PrivateCar >> GetQuoteRequest >> " + Convert.ToString(resModel.ErrorMsg));
                    //    return resModel;
                    //}

                }
                document.XPathSelectElement("//POS_MISP/Type").Value = "P";
                var response = Ser.CreatePolicy("Motor", Convert.ToString(document));
                ap.SP_REQUEST_RESPONSE_API_MASTER(model.enquiryid, 5, Convert.ToString(document), Convert.ToString(response));
                //var a=     ap.SP_REQUEST_RESPONSE_MASTER("G", model.enquiryid, 5, string.Empty, string.Empty).FirstOrDefault(); 
                resModel = GetQuoteResponse(XDocument.Parse(response));
                if (model.CoverageDetails.IsPACoverUnnamedPerson)
                {
                    resModel.PremiumBreakUpDetails.BasicThirdPartyLiability = resModel.PremiumBreakUpDetails.BasicThirdPartyLiability - resModel.PremiumBreakUpDetails.PACoverToUnNamedPerson;
                }
                if (!resModel.Status.Equals("Fail"))
                {
                    if (model.IsThirdPartyOnly)
                    {
                        resModel.IDV = 0;
                    }
                    else
                    {
                        if (model.PolicyType.Equals("New"))
                        {
                            resModel.MaxIDV = Convert.ToInt32(resModel.IDV + resModel.IDV * 5 / 100);
                            resModel.MinIDV = Convert.ToInt32(resModel.IDV - resModel.IDV * 5 / 100);
                        }
                        else
                        {
                            resModel.MaxIDV = Convert.ToInt32(resModel.IDV + resModel.IDV * 10 / 100);
                            resModel.MinIDV = Convert.ToInt32(resModel.IDV - resModel.IDV * 10 / 100);
                        }
                        if (Convert.ToInt64(resModel.IDV) > resModel.MaxIDV) { resModel.MaxIDV = Convert.ToInt32(resModel.IDV); }
                        if (Convert.ToInt64(resModel.IDV) < resModel.MinIDV) { resModel.MinIDV = Convert.ToInt32(resModel.IDV); }
                    }
                }

                else
                {
                    resModel.IDV = 0;
                }
            }
            catch (Exception ex)
            {
                resModel.Status = Status.Fail;
                resModel.ErrorMsg = Convert.ToString(ex.Message);
                Console.Write(Convert.ToString(ex.Message));
                LogU.WriteLog("FGI >> PrivateCar >> GetQuoteRequest >> " + Convert.ToString(ex.Message));
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
            return resModel;
        }
        //public Response GetQuoteRequest(Quotation model)
        //{
        //    Response resModel = new Response();
        //    XmlDocument doc = new XmlDocument();

        //    try
        //    {
        //        if (model.PolicyType.Equals("New"))
        //        {
        //            model.PolicyEndDate = Convert.ToDateTime(model.PolicyStartDate).AddYears(3).AddDays(-1).ToString("yyyy-MM-dd");
        //        }
        //        else
        //        {
        //            model.PolicyEndDate = Convert.ToDateTime(model.PolicyStartDate).AddYears(1).AddDays(-1).ToString("yyyy-MM-dd");
        //        }
        //        string path = AppDomain.CurrentDomain.BaseDirectory;
        //        string filePath = Path.Combine(path, "XML/FGI/Quote.xml");
        //        var document = XDocument.Load(filePath);
        //        if (model.IDV != 0)
        //        {
        //            if (model.CustomIDV != null)
        //            {
        //                var getidvvalues = model.CustomIDV.Where(x => x.CompanyName == "FGI").FirstOrDefault();
        //                if (model.IDV == 1)  //1 is for lowest idv of company
        //                {
        //                    model.IDV = getidvvalues.MinIDV;
        //                }
        //                else if (model.IDV == 2) // 2 is for lowest idv of company
        //                {
        //                    model.IDV = getidvvalues.MaxIDV;
        //                }
        //                else
        //                {
        //                    if (model.IDV < getidvvalues.MinIDV)
        //                    {
        //                        model.IDV = getidvvalues.MinIDV;
        //                    }
        //                    else if (model.IDV > getidvvalues.MaxIDV)
        //                    {
        //                        model.IDV = getidvvalues.MaxIDV;
        //                    }
        //                    else
        //                    {

        //                    }
        //                }
        //            }
        //        }

        //        document.XPathSelectElement("//PolicyStartDate").Value = Convert.ToDateTime(model.PolicyStartDate).ToString("yyyy-MM-dd");
        //        document.XPathSelectElement("//PolicyEndDate").Value = Convert.ToDateTime(model.PolicyEndDate).ToString("yyyy-MM-dd");
        //        if (!model.PolicyType.Equals("New"))
        //        {
        //            document.XPathSelectElement("//ContractType").Value = "FPV";
        //            document.XPathSelectElement("//RiskType").Value = "FPV";
        //            document.XPathSelectElement("//NCB").Value = Convert.ToString(model.CurrentNcb);
        //            document.XPathSelectElement("//RegistrationNo").Value = !string.IsNullOrEmpty(model.VehicleDetails.RegistrationNumber) ? model.VehicleDetails.RegistrationNumber : "DL01AB1221";
        //        }
        //        if(!model.CustomerType.ToUpper().Equals("INDIVIDUAL"))
        //        {
        //            model.ClientDetails = new ClientDetails();
        //            model.ClientDetails.FirstName = "M/s";
        //            model.ClientDetails.LastName = model.OrganizationName;
        //        }
        //        document.XPathSelectElement("//Salutation").Value = model.ClientDetails != null ? ((string.IsNullOrEmpty(model.ClientDetails.Salutation)) ? "MR" : model.ClientDetails.Salutation) : "MR";
        //        document.XPathSelectElement("//ClientType").Value = string.IsNullOrEmpty(model.CustomerType) ? "I" : (model.CustomerType.Equals("Individual") ? "I" : "C");
        //        document.XPathSelectElement("//FirstName").Value = model.ClientDetails != null ? ((string.IsNullOrEmpty(model.ClientDetails.FirstName)) ? "TestFName" : model.ClientDetails.FirstName) : "TestFName";
        //        document.XPathSelectElement("//LastName").Value = model.ClientDetails != null ? ((string.IsNullOrEmpty(model.ClientDetails.LastName)) ? "TestLName" : model.ClientDetails.LastName) : "TestLName";
        //        document.XPathSelectElement("//DOB").Value = model.ClientDetails != null ? ((string.IsNullOrEmpty(model.ClientDetails.DateOfBirth)) ? "2002-06-01" : Convert.ToDateTime(model.ClientDetails.DateOfBirth).ToString("yyyy-MM-dd")) : "2002-06-01";
        //        document.XPathSelectElement("//Gender").Value = model.ClientDetails != null ? ((string.IsNullOrEmpty(model.ClientDetails.Gender)) ? "M" : model.ClientDetails.Gender) : "M";
        //        document.XPathSelectElement("//MaritalStatus").Value = model.ClientDetails != null ? ((string.IsNullOrEmpty(model.ClientDetails.MaritalStatus)) ? "S" : model.ClientDetails.MaritalStatus) : "S";
        //        document.XPathSelectElement("//Occupation").Value = model.ClientDetails != null ? ((string.IsNullOrEmpty(model.ClientDetails.Occupation)) ? "SVCM" : model.ClientDetails.Occupation) : "SVCM";
        //        document.XPathSelectElement("//PANNo").Value = model.ClientDetails != null ? ((string.IsNullOrEmpty(model.ClientDetails.PanCardNo)) ? string.Empty : model.ClientDetails.PanCardNo) : string.Empty;
        //        document.XPathSelectElement("//GSTIN").Value = model.ClientDetails != null ? ((string.IsNullOrEmpty(model.ClientDetails.GSTIN)) ? string.Empty : model.ClientDetails.GSTIN) : string.Empty;
        //        document.XPathSelectElement("//AadharNo").Value = model.ClientDetails != null ? ((string.IsNullOrEmpty(model.ClientDetails.AadharNo)) ? string.Empty : model.ClientDetails.AadharNo) : string.Empty;
        //        document.XPathSelectElement("//AddrLine1").Value = model.VehicleAddressDetails != null ? model.VehicleAddressDetails.Address1 : string.Empty;
        //        document.XPathSelectElement("//AddrLine2").Value = model.VehicleAddressDetails != null ? model.VehicleAddressDetails.Address2 : string.Empty;
        //        document.XPathSelectElement("//AddrLine3").Value = model.VehicleAddressDetails != null ? (string.IsNullOrEmpty(model.VehicleAddressDetails.Address3) ? string.Empty : model.VehicleAddressDetails.Address3) : string.Empty;
        //        document.XPathSelectElement("//Pincode").Value = model.VehicleAddressDetails != null ? model.VehicleAddressDetails.Pincode : string.Empty;
        //        document.XPathSelectElement("//City").Value = model.VehicleAddressDetails != null ? model.VehicleAddressDetails.City : string.Empty;
        //        document.XPathSelectElement("//State").Value = model.VehicleAddressDetails != null ? model.VehicleAddressDetails.State : string.Empty;
        //        document.XPathSelectElement("//MobileNo").Value = model.ClientDetails != null ? ((string.IsNullOrEmpty(model.ClientDetails.MobileNo)) ? string.Empty : model.ClientDetails.MobileNo) : string.Empty;
        //        document.XPathSelectElement("//EmailAddr").Value = model.ClientDetails != null ? ((string.IsNullOrEmpty(model.ClientDetails.EmailId)) ? "trupti@nodib.com" : model.ClientDetails.EmailId) : "trupti@nodib.com";
        //        document.XPathSelectElement("//Zone").Value = model.VehicleDetails != null ? model.VehicleDetails.RtoZone : string.Empty;
        //        //document.XPathSelectElement("//RTOCode").Value = model.VehicleDetails != null ? model.VehicleDetails.RtoZone : string.Empty;
        //        document.XPathSelectElement("//RTOCode").Value = "DL01";
        //        document.XPathSelectElement("//Make").Value = model.VehicleDetails != null ? model.VehicleDetails.MakeName : string.Empty;
        //        document.XPathSelectElement("//ModelCode").Value = model.VehicleDetails != null ? model.VehicleDetails.VariantCode : string.Empty;
        //        document.XPathSelectElement("//RegistrationDate").Value = Convert.ToDateTime(model.VehicleDetails.RegistrationDate).ToString("yyyy-MM-dd");
        //        //document.XPathSelectElement("//RegistrationDate").Value = Convert.ToString(dateParts[2] + "-" + dateParts[1] + "-" + dateParts[0]);//Convert.ToDateTime(model.VehicleDetails.RegistrationDate).ToString("yyyy-MM-dd");
        //        document.XPathSelectElement("//ManufacturingYear").Value = Convert.ToString(Convert.ToDateTime(model.VehicleDetails.ManufaturingDate).Year);
        //        string fuel = Convert.ToString(model.VehicleDetails.Fuel).Substring(0, 1);
        //        document.XPathSelectElement("//FuelType").Value = fuel;
        //        //document.XPathSelectElement("//InbuiltKit").Value = model.CoverageDetails != null ? (model.CoverageDetails.IsBiFuelKit ? "N" : (fuel.Equals("C")? "Y" :"N")) : "N";
        //        document.XPathSelectElement("//IVDOfCNGOrLPG").Value = model.CoverageDetails != null ? (model.CoverageDetails.IsBiFuelKit ? Convert.ToString(model.CoverageDetails.BiFuelKitAmount) : "0") : "0";
        //        document.XPathSelectElement("//IDV").Value = Convert.ToString(model.IDV);
        //        document.XPathSelectElement("//BodyType").Value = model.VehicleDetails != null ? ((string.IsNullOrEmpty(model.VehicleDetails.BodyType)) ? "SOLO" : model.VehicleDetails.BodyType) : "SOLO";
        //        document.XPathSelectElement("//EngineNo").Value = model.VehicleDetails != null ? ((string.IsNullOrEmpty(model.VehicleDetails.EngineNumber)) ? "5154545445456" : model.VehicleDetails.EngineNumber) : "Engineno123456789";
        //        document.XPathSelectElement("//ChassiNo").Value = model.VehicleDetails != null ? ((string.IsNullOrEmpty(model.VehicleDetails.ChassisNumber)) ? "FCFGFFGG95467655656565" : model.VehicleDetails.ChassisNumber) : "Chassisno123456789";
        //        document.XPathSelectElement("//CubicCapacity").Value = model.VehicleDetails != null ? ((string.IsNullOrEmpty(Convert.ToString(model.VehicleDetails.CC))) ? "0" : Convert.ToString(model.VehicleDetails.CC)) : "0";
        //        document.XPathSelectElement("//SeatingCapacity").Value = model.VehicleDetails != null ? ((string.IsNullOrEmpty(Convert.ToString(model.VehicleDetails.SC))) ? "0" : Convert.ToString(model.VehicleDetails.SC)) : "0";
        //        document.XPathSelectElement("//ElectricalAccessoriesValues").Value = model.CoverageDetails != null ? (model.CoverageDetails.IsElectricalAccessories ? Convert.ToString(model.CoverageDetails.SIElectricalAccessories) : "0") : "0";
        //        document.XPathSelectElement("//NonElectricalAccessoriesValues").Value = model.CoverageDetails != null ? (model.CoverageDetails.IsNonElectricalAccessories ? Convert.ToString(model.CoverageDetails.SINonElectricalAccessories) : "0") : "0";
        //        //document.XPathSelectElement("//PACoverForUnnamedPassengers").Value = model.CoverageDetails.IsPACoverUnnamedPerson ? Convert.ToString(model.CoverageDetails.NumberofPersonsUnnamed) : "0";
        //        //document.XPathSelectElement("//LegalLiabilitytoPaidDriver").Value = model.CoverageDetails.NoOfLLPaidDriver > 0 ? Convert.ToString(model.CoverageDetails.NoOfLLPaidDriver) : "1";
        //        document.XPathSelectElement("//LegalLiabilityForOtherEmployees").Value = model.CoverageDetails.IsLLEmployee ? "1" : "0";

        //        document.XPathSelectElement("//CPAYear").Value = model.PolicyType.Equals("New") ? "3" : string.Empty;
        //        //document.XPathSelectElement("//CPAReq").Value = model.CoverageDetails.IsPACoverPaidDriver ? "Y" : "N";
        //        //document.XPathSelectElement("//CPANomName").Value = string.IsNullOrEmpty(model.NomineeName) ? string.Empty : model.NomineeName;
        //        //document.XPathSelectElement("//CPANomAge").Value = string.IsNullOrEmpty(model.NomineeDateOfBirth) ? "0" : Convert.ToString(CalculateAge(Convert.ToDateTime(model.NomineeDateOfBirth)));
        //        //document.XPathSelectElement("//CPARelation").Value = string.IsNullOrEmpty(model.NomineeRelationShip) ? string.Empty : model.NomineeRelationShip;
        //        //document.XPathSelectElement("//CPAAppointeeName").Value = string.IsNullOrEmpty(model.AppointeeName) ? string.Empty : model.AppointeeName;
        //        document.XPathSelectElement("//RollOver").Value = model.PolicyType.Equals("New", StringComparison.OrdinalIgnoreCase) ? "N" : "Y";
        //        //document.XPathSelectElement("//InsuredName").Value = model.PreviousPolicyDetails != null ? ((string.IsNullOrEmpty(model.PreviousPolicyDetails.PreviousCompanyName)) ? "0" : model.PreviousPolicyDetails.PreviousCompanyName) : "0";
        //        //document.XPathSelectElement("//PreviousPolExpDt").Value = model.PreviousPolicyDetails != null ? model.PreviousPolicyDetails.PreviousPolicyEndDate : "0001-01-01";
        //        //document.XPathSelectElement("//NCBInExpiringPolicy").Value = model.PreviousPolicyDetails != null ? model.PreviousPolicyDetails.PreviousNcbPercentage : "0";
        //        //document.XPathSelectElement("//ClaimInExpiringPolicy").Value = model.PreviousPolicyDetails != null ? (model.PreviousPolicyDetails.IsPreviousInsuranceClaimed ? "Y" : "N") : "N";
        //        document.XPathSelectElement("//NewVehicle").Value = model.PolicyType.Equals("New", StringComparison.OrdinalIgnoreCase) ? "Y" : "N";
        //        //document.XPathSelectElement("//PACoverForUnnamedPassengers").Value = model.CoverageDetails.IsPACoverUnnamedPerson ? Convert.ToString(model.VehicleDetails.SC) : "0";
        //        document.XPathSelectElement("//LegalLiabilitytoPaidDriver").Value = model.CoverageDetails.NoOfLLPaidDriver > 0 ? Convert.ToString(model.CoverageDetails.NoOfLLPaidDriver) : "0";
        //        //document.XPathSelectElement("//LegalLiabilityForOtherEmployees").Value = model.CoverageDetails.IsLLEmployee ? "1" : "0";
        //        if (model.IsODOnly && model.PreviousTPPolicyDetails != null)
        //        {
        //            DateTime PrvTPPolicyStartDate = Convert.ToDateTime(model.PreviousTPPolicyDetails.PolicyEndDate).AddYears(-3).AddDays(1);
        //            document.XPathSelectElement("//Cover").Value = "OD";
        //            document.XPathSelectElement("//CPAReq").Value = "N";
        //            document.XPathSelectElement("//PACoverForUnnamedPassengers").Value = "0";
        //            document.XPathSelectElement("//LegalLiabilityForOtherEmployees").Value = "0";
        //            document.XPathSelectElement("//PreviousInsurer").Value = "Bajaj Allianz General Insurance Co Ltd.";
        //            document.XPathSelectElement("//TPPolicyNumber").Value = model.PreviousTPPolicyDetails.PolicyNo;
        //            document.XPathSelectElement("//TPPolicyEffdate").Value = PrvTPPolicyStartDate.ToString("yyyy-MM-dd");
        //            document.XPathSelectElement("//TPPolicyExpiryDate").Value = Convert.ToDateTime(model.PreviousTPPolicyDetails.PolicyEndDate).ToString("yyyy-MM-dd");
        //        }
        //        if (model.IsThirdPartyOnly)
        //        {
        //            document.XPathSelectElement("//Cover").Value = "LO";
        //            document.XPathSelectElement("//AddonReq").Value = "N";
        //        }
        //        if (!model.PolicyType.Equals("New") && model.PreviousPolicyDetails != null)
        //        {
        //            model.PreviousPolicyDetails.PreviousPolicyStartDate = Convert.ToDateTime(model.PreviousPolicyDetails.PreviousPolicyEndDate).AddYears(-1).AddDays(1).ToString();
        //            document.XPathSelectElement("//RollOverList/PolicyNo").Value = model.PreviousPolicyDetails.PreviousPolicyNo;
        //            document.XPathSelectElement("//InsuredName").Value = "Bajaj Allianz General Insurance Co Ltd.";
        //            document.XPathSelectElement("//ClientCode").Value = "40062645";
        //            document.XPathSelectElement("//PreviousPolExpDt").Value = Convert.ToDateTime(model.PreviousPolicyDetails.PreviousPolicyEndDate).ToString("yyyy-MM-dd");
        //            document.XPathSelectElement("//NCBDeclartion").Value = model.PreviousPolicyDetails.IsPreviousInsuranceClaimed ? "N" : "Y";
        //            document.XPathSelectElement("//ClaimInExpiringPolicy").Value = model.PreviousPolicyDetails.IsPreviousInsuranceClaimed ? "Y" : "N";
        //            document.XPathSelectElement("//NCBInExpiringPolicy").Value = model.PreviousPolicyDetails.PreviousNcbPercentage;
        //            document.XPathSelectElement("//PreviousPolStartDt").Value = Convert.ToDateTime(model.PreviousPolicyDetails.PreviousPolicyStartDate).ToString("yyyy-MM-dd");
        //        }
        //        var response = Ser.CreatePolicy("Motor", Convert.ToString(document));
        //        resModel = GetQuoteResponse(XDocument.Parse(response));
        //        if (!resModel.Status.Equals("Fail"))
        //        {
        //            if (model.IsThirdPartyOnly)
        //            {
        //                resModel.IDV = 0;
        //            }
        //            else
        //            {
        //                if (model.PolicyType.Equals("New"))
        //                {
        //                    resModel.MaxIDV = Convert.ToInt32(resModel.IDV + resModel.IDV * 5 / 100);
        //                    resModel.MinIDV = Convert.ToInt32(resModel.IDV - resModel.IDV * 5 / 100);
        //                }
        //                else
        //                {
        //                    resModel.MaxIDV = Convert.ToInt32(resModel.IDV + resModel.IDV * 10 / 100);
        //                    resModel.MinIDV = Convert.ToInt32(resModel.IDV - resModel.IDV * 10 / 100);
        //                }
        //                if (Convert.ToInt64(resModel.IDV) > resModel.MaxIDV) { resModel.MaxIDV = Convert.ToInt32(resModel.IDV); }
        //                if (Convert.ToInt64(resModel.IDV) < resModel.MinIDV) { resModel.MinIDV = Convert.ToInt32(resModel.IDV); }
        //            }
        //        }

        //        else
        //        {
        //            resModel.IDV = 0;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        resModel.Status = Status.Fail;
        //        resModel.ErrorMsg = Convert.ToString(ex.Message);
        //        Console.Write(Convert.ToString(ex.Message));
        //        LogU.WriteLog("FGI >> PrivateCar >> GetQuoteRequest >> " + Convert.ToString(ex.Message));
        //    }
        //    if(model.IsThirdPartyOnly)
        //    {
        //        resModel.PlanName = "Third Party Plan";
        //    }
        //    else if(model.IsODOnly)
        //    {
        //        resModel.PlanName = "Own Damage Plan";
        //    }
        //    else
        //    {
        //        resModel.PlanName = "Comprehesive Plan";
        //    }

        //    resModel.PolicyStartDate = model.PolicyStartDate;
        //    resModel.PolicyEndDate = model.PolicyEndDate;
        //    return resModel;
        //}

        /// <summary>
        /// Quote response method.
        /// </summary>
        /// <param name="res">xdocument objects.</param>
        /// <returns>return response type object.</returns>
        public Response GetQuoteResponse(XDocument res)
        {
            Response resModel = new Response();
            DataSet ds = new DataSet();
            resModel.PremiumBreakUpDetails = new PremiumBreakUpDetails();
            resModel.CompanyWiseRefference = new CompanyWiseRefference();
            resModel.FreeAddonCover = new AddonCover();
            try
            {
                string status = res.XPathSelectElement("//Status").Value;
                if ((!string.IsNullOrEmpty(status)) && status.Equals("Successful", StringComparison.OrdinalIgnoreCase))
                {
                    resModel.IDV = Convert.ToInt32(res.XPathSelectElement("//VehicleIDV").Value);
                    ds.ReadXml(new XmlTextReader(new StringReader(Convert.ToString(res))));
                    List<XElement> xElementList = res.Descendants("Table1").ToList();
                    resModel.Status = Status.Success;
                    if (ds.Tables.Count > 0)
                    {
                        if (ds.Tables.Contains("Policy"))
                        {
                            DataTable dt_Policy = ds.Tables["Policy"];
                            DataTable dt_Table = ds.Tables["Table"];
                            DataTable dt_Table1 = ds.Tables["Table1"];

                            if (Convert.ToString(dt_Policy.Rows[0]["Status"].ToString()) == "Successful")
                            {
                                if (dt_Table.Rows.Count > 0)
                                {
                                    resModel.PolicyNo = Convert.ToString(dt_Table.Rows[0]["PolNo"].ToString());
                                }
                                for (int i = 0; i < dt_Table1.Rows.Count; i++)
                                {

                                    string code = Convert.ToString(dt_Table1.Rows[i]["Code"].ToString()).Trim();
                                    string description = Convert.ToString(dt_Table1.Rows[i]["Description"].ToString()).Trim();
                                    string type = Convert.ToString(dt_Table1.Rows[i]["Type"].ToString()).Trim();
                                    string BOValue = Convert.ToString(dt_Table1.Rows[i]["BOValue"].ToString()).Trim();
                                    string DBValue = Convert.ToString(dt_Table1.Rows[i]["DBValue"].ToString()).Trim();


                                    if (code == "Gross Premium")
                                    {
                                        if (type == "OD")
                                        {
                                            resModel.PremiumBreakUpDetails.BasicODPremium = Math.Round(Convert.ToDouble(BOValue));
                                        }
                                        else if (type == "TP")
                                        {
                                            resModel.PremiumBreakUpDetails.BasicThirdPartyLiability = Math.Round(Convert.ToDouble(BOValue));
                                        }
                                    }
                                    if (code == "CNG")
                                    {
                                        if (type == "OD")
                                        {
                                            resModel.PremiumBreakUpDetails.CNGLPGKitPremium = Math.Round(Convert.ToDouble(BOValue));
                                        }
                                        else if (type == "TP")
                                        {
                                            resModel.PremiumBreakUpDetails.TPCNGLPGPremium = Math.Round(Convert.ToDouble(BOValue));
                                        }
                                    }
                                    else if (code == "NCB")
                                    {
                                        if (type == "OD")
                                        {
                                            string Ncbdis = Convert.ToString(BOValue).Substring(1);
                                            resModel.PremiumBreakUpDetails.NCBDiscount += Math.Round(Convert.ToDouble(Ncbdis));
                                        }
                                        else if (type == "TP")
                                        {
                                            string NcbdisTP = Convert.ToString(BOValue).Substring(1);
                                            resModel.PremiumBreakUpDetails.NCBDiscount += Math.Round(Convert.ToDouble(NcbdisTP));
                                        }
                                    }
                                    else if (code == "APA")
                                    {
                                        if (type == "OD")
                                        {
                                            resModel.PremiumBreakUpDetails.PACoverToUnNamedPerson += Math.Round(Convert.ToDouble(BOValue));
                                        }
                                        else if (type == "TP")
                                        {
                                            resModel.PremiumBreakUpDetails.PACoverToUnNamedPerson += Math.Round(Convert.ToDouble(BOValue));
                                        }
                                    }
                                    else if (code == "LLDE")
                                    {

                                        if (type == "TP")
                                        {
                                            resModel.PremiumBreakUpDetails.LLToPaidDriver = Math.Round(Convert.ToDouble(BOValue));
                                        }
                                    }
                                    else if (code == "LLOE")
                                    {

                                        if (type == "TP")
                                        {
                                            resModel.PremiumBreakUpDetails.LLToPaidEmployee = Math.Round(Convert.ToDouble(BOValue));
                                        }
                                    }
                                    else if (code == "LOADDISC")
                                    {

                                        if (type == "OD")
                                        {
                                            string dis = Convert.ToString(BOValue).Substring(2);
                                            resModel.PremiumBreakUpDetails.OtherDiscount += Math.Round(Convert.ToDouble(dis));
                                        }
                                        else if (type == "TP")
                                        {
                                            resModel.PremiumBreakUpDetails.OtherDiscount += Math.Round(Convert.ToDouble(BOValue));
                                        }
                                    }
                                    else if (code == "ServTax")
                                    {

                                        if (type == "OD")
                                        {
                                            resModel.PremiumBreakUpDetails.ServiceTax += Math.Round(Convert.ToDouble(BOValue));
                                        }
                                        else if (type == "TP")
                                        {
                                            resModel.PremiumBreakUpDetails.ServiceTax += Math.Round(Convert.ToDouble(BOValue));
                                        }

                                    }
                                    else if (code == "PrmDue")
                                    {
                                        if (type == "OD")
                                        {
                                            resModel.FinalPremium += Math.Round(Convert.ToDouble(BOValue));
                                        }
                                        else if (type == "TP")
                                        {
                                            resModel.FinalPremium += Math.Round(Convert.ToDouble(BOValue));
                                        }

                                    }
                                    else if (code == "MOTADON" && type == "OD")
                                    {
                                        resModel.PremiumBreakUpDetails.NetAddonPremium = Math.Round(Convert.ToDouble(BOValue));
                                    }
                                    else if (code == "00001")                      //tyre
                                    {
                                        resModel.PremiumBreakUpDetails.TyreProtect = Math.Round(Convert.ToDouble(BOValue));
                                    }
                                    else if (code == "00002")                //INconvenience
                                    {
                                        resModel.PremiumBreakUpDetails.InvoicePriceCoverPremium = Math.Round(Convert.ToDouble(BOValue));
                                    }
                                    else if (code == "00004")        // ncb
                                    {
                                        resModel.PremiumBreakUpDetails.NcbProtectorPremium = Math.Round(Convert.ToDouble(BOValue));
                                    }
                                    else if (code == "00005")      //consumables
                                    {
                                        resModel.PremiumBreakUpDetails.CostOfConsumablesPremium = Math.Round(Convert.ToDouble(BOValue));
                                    }
                                    else if (code == "00006")        //Invoice
                                    {
                                        resModel.PremiumBreakUpDetails.InvoicePriceCoverPremium = Math.Round(Convert.ToDouble(BOValue));
                                    }
                                    else if (code == "PLAN1")
                                    {
                                        resModel.PremiumBreakUpDetails.ZeroDepPremium = Math.Round(Convert.ToDouble(BOValue));
                                        resModel.FreeAddonCover.IsLossofpersonalBelonging = true;
                                        resModel.FreeAddonCover.IsLossofKey = true;
                                        resModel.FreeAddonCover.IsRoadSideAssistance = true;
                                    }
                                    else if (code == "PLAN2")
                                    {
                                        resModel.PremiumBreakUpDetails.RSAPremium = Math.Round(Convert.ToDouble(BOValue));
                                    }

                                    else if (code == "ENGPR")
                                    {
                                        resModel.PremiumBreakUpDetails.EngineProtectorPremium = Math.Round(Convert.ToDouble(BOValue));
                                    }

                                    else if (code == "CPA")
                                    {
                                        resModel.PremiumBreakUpDetails.PACoverToOwnDriver = Math.Round(Convert.ToDouble(BOValue));
                                    }
                                }
                                if (resModel.PremiumBreakUpDetails.NCBDiscount > 0)
                                {
                                    resModel.PremiumBreakUpDetails.BasicODPremium = Math.Round(resModel.PremiumBreakUpDetails.BasicODPremium - resModel.PremiumBreakUpDetails.CNGLPGKitPremium - resModel.PremiumBreakUpDetails.NetAddonPremium - resModel.PremiumBreakUpDetails.ElecAccessoriesPremium - resModel.PremiumBreakUpDetails.NonElecAccessoriesPremium + resModel.PremiumBreakUpDetails.NCBDiscount);
                                }
                                else
                                {
                                    resModel.PremiumBreakUpDetails.BasicODPremium = Math.Round(resModel.PremiumBreakUpDetails.BasicODPremium - resModel.PremiumBreakUpDetails.CNGLPGKitPremium - resModel.PremiumBreakUpDetails.NetAddonPremium - resModel.PremiumBreakUpDetails.ElecAccessoriesPremium - resModel.PremiumBreakUpDetails.NonElecAccessoriesPremium);
                                }

                                resModel.PremiumBreakUpDetails.BasicThirdPartyLiability = Math.Round(resModel.PremiumBreakUpDetails.BasicThirdPartyLiability - resModel.PremiumBreakUpDetails.PACoverToOwnDriver - resModel.PremiumBreakUpDetails.TPCNGLPGPremium - resModel.PremiumBreakUpDetails.PAToPaidDriver - resModel.PremiumBreakUpDetails.LLToPaidDriver - resModel.PremiumBreakUpDetails.LLToPaidEmployee);
                                resModel.PremiumBreakUpDetails.NetPremium = Math.Round(resModel.FinalPremium - resModel.PremiumBreakUpDetails.ServiceTax);
                            }
                        }
                    }
                }
                else
                {
                    resModel.Status = Status.Fail;
                    resModel.ErrorMsg = Convert.ToString(res.XPathSelectElement("//Policy/ErrorMessage"));
                    if (!string.IsNullOrEmpty(Convert.ToString(res.XPathSelectElement("//ValidationError"))))
                    {
                        resModel.ErrorMsg = Convert.ToString(res.XPathSelectElement("//ValidationError"));
                    }
                    else
                    {
                        resModel.ErrorMsg = Convert.ToString(res.XPathSelectElement("//Policy/ErrorMessage"));
                    }
                }
            }
            catch (Exception ex)
            {
                resModel.Status = Status.Fail;
                resModel.ErrorMsg = Convert.ToString(ex.Message);
                Console.Write(Convert.ToString(ex.Message));
                LogU.WriteLog("FGI >> PrivateCar >> GetQuoteResponse >> " + Convert.ToString(ex.Message));
            }

            return resModel;
        }
        /// <summary>
        /// Proposal request method.
        /// </summary>
        /// <param name="model">Object of quotation model.</param>
        /// <returns>return response type object.</returns>
        public Response GetProposalRequest(Quotation model)
        {
            Response resModel = new Response();
            XmlDocument doc = new XmlDocument();
            var PrvTPPolicyStartDate = string.Empty;
            try
            {

                string path = AppDomain.CurrentDomain.BaseDirectory;
                string filePath = Path.Combine(path, "XML/FGI/Proposal.xml");
                var document = XDocument.Load(filePath);
                var sdate = Convert.ToDateTime(model.PolicyStartDate).ToString("yyyy-MM-dd");
                //model.PolicyEndDate = Convert.ToDateTime(model.PolicyStartDate).AddYears(3).AddDays(-1).ToString("yyyy-MM-dd");

                if (model.PolicyType.Equals("New"))
                {
                    //model.PolicyEndDate = Convert.ToDateTime(model.PolicyStartDate).AddYears(3).AddDays(-2).ToString("yyyy-MM-dd");
                    model.PolicyEndDate = Convert.ToDateTime(sdate).AddYears(3).AddDays(-1).ToString("yyyy-MM-dd");
                }
                else
                {
                    model.PolicyEndDate = Convert.ToDateTime(sdate).AddYears(1).AddDays(-1).ToString("yyyy-MM-dd");
                }
                document.XPathSelectElement("//PolicyStartDate").Value = Convert.ToDateTime(sdate).ToString("dd/MM/yyyy");
                document.XPathSelectElement("//PolicyEndDate").Value = Convert.ToDateTime(model.PolicyEndDate).ToString("dd/MM/yyyy");
                document.XPathSelectElement("//Uid").Value = Convert.ToString(GenerateRandomNo());
                if (!model.PolicyType.Equals("New"))
                {

                    //document.XPathSelectElement("//ContractType").Value = model.IsODOnly ? "FVO" : "FPV";
                    //document.XPathSelectElement("//RiskType").Value = model.IsODOnly ? "FVO" : "FPV";
                    document.XPathSelectElement("//ContractType").Value = model.IsODOnly ? "PVO" : "PPV";
                    document.XPathSelectElement("//RiskType").Value = model.IsODOnly ? "FVO" : "FPV";
                    document.XPathSelectElement("//NCB").Value = Convert.ToString(model.CurrentNcb);
                    if (!string.IsNullOrEmpty(model.VehicleDetails.RegistrationNumber) && model.VehicleDetails.RegistrationNumber.Contains("-"))
                    {
                        document.XPathSelectElement("//RegistrationNo").Value = model.VehicleDetails.RegistrationNumber.Replace("-", "").ToUpper();
                    }
                    else
                    {
                        document.XPathSelectElement("//RegistrationNo").Value = !string.IsNullOrEmpty(model.VehicleDetails.RegistrationNumber) ? model.VehicleDetails.RegistrationNumber.ToUpper() : "DL01AB1221";
                    }

                    //document.XPathSelectElement("//RegistrationNo").Value = !string.IsNullOrEmpty(model.VehicleDetails.RegistrationNumber) ? model.VehicleDetails.RegistrationNumber.ToUpper() : "DL01AB1221";
                }
                if (!model.CustomerType.ToUpper().Equals("INDIVIDUAL"))
                {
                    model.ClientDetails = new ClientDetails();
                    model.ClientDetails.FirstName = "M/s";
                    model.ClientDetails.LastName = model.OrganizationName;
                }
                document.XPathSelectElement("//Salutation").Value = model.ClientDetails != null ? ((string.IsNullOrEmpty(model.ClientDetails.Salutation)) ? "MR" : model.ClientDetails.Salutation) : "MR";
                document.XPathSelectElement("//ClientType").Value = string.IsNullOrEmpty(model.CustomerType) ? "I" : (model.CustomerType.Equals("Individual") ? "I" : "C");
                document.XPathSelectElement("//FirstName").Value = model.ClientDetails != null ? ((string.IsNullOrEmpty(model.ClientDetails.FirstName)) ? "TestFName" : model.ClientDetails.FirstName.Trim()) : "TestFName";
                if (model.ClientDetails != null)
                {
                    if (!string.IsNullOrEmpty(model.ClientDetails.MiddleName))
                    {
                        model.ClientDetails.LastName = model.ClientDetails.MiddleName + " " + model.ClientDetails.LastName;
                    }

                }
                document.XPathSelectElement("//LastName").Value = model.ClientDetails != null ? ((string.IsNullOrEmpty(model.ClientDetails.LastName)) ? "TestLName" : model.ClientDetails.LastName.Trim()) : "TestLName";
                document.XPathSelectElement("//DOB").Value = model.ClientDetails != null ? ((string.IsNullOrEmpty(model.ClientDetails.DateOfBirth)) ? "2002-06-01" : Convert.ToDateTime(model.ClientDetails.DateOfBirth).ToString("yyyy-MM-dd")) : "2002-06-01";
                document.XPathSelectElement("//Gender").Value = model.ClientDetails != null ? ((string.IsNullOrEmpty(model.ClientDetails.Gender)) ? "M" : model.ClientDetails.Gender) : "M";
                document.XPathSelectElement("//MaritalStatus").Value = model.ClientDetails != null ? ((string.IsNullOrEmpty(model.ClientDetails.MaritalStatus)) ? "S" : model.ClientDetails.MaritalStatus) : "S";
                document.XPathSelectElement("//Occupation").Value = model.ClientDetails != null ? ((string.IsNullOrEmpty(model.ClientDetails.Occupation)) ? "SVCM" : model.ClientDetails.Occupation) : "SVCM";
                document.XPathSelectElement("//Gender").Value = model.ClientDetails.Gender.ToUpper().Equals("MALE") ? "M" : "F";
                //document.XPathSelectElement("//MaritalStatus").Value = GetMaritial(model.ClientDetails.MaritalStatus);// "S";
                //document.XPathSelectElement("//Occupation").Value = "SVCM";//model.ClientDetails.Occupation;//"SVCM";
                document.XPathSelectElement("//PANNo").Value = model.ClientDetails != null ? ((string.IsNullOrEmpty(model.ClientDetails.PanCardNo)) ? string.Empty : model.ClientDetails.PanCardNo.ToUpper()) : string.Empty;
                document.XPathSelectElement("//GSTIN").Value = model.ClientDetails != null ? ((string.IsNullOrEmpty(model.ClientDetails.GSTIN)) ? string.Empty : model.ClientDetails.GSTIN.ToUpper()) : string.Empty;
                document.XPathSelectElement("//AadharNo").Value = model.ClientDetails != null ? ((string.IsNullOrEmpty(model.ClientDetails.AadharNo)) ? string.Empty : model.ClientDetails.AadharNo) : string.Empty;

                document.XPathSelectElement("//Address1/AddrLine1").Value = model.CustomerAddressDetails != null ? model.CustomerAddressDetails.Address1 : string.Empty;
                document.XPathSelectElement("//Address1/AddrLine2").Value = model.CustomerAddressDetails != null ? model.CustomerAddressDetails.Address2 : string.Empty;
                document.XPathSelectElement("//Address1/AddrLine3").Value = model.CustomerAddressDetails != null ? (string.IsNullOrEmpty(model.CustomerAddressDetails.Address3) ? string.Empty : model.VehicleAddressDetails.Address3) : string.Empty;
                document.XPathSelectElement("//Address1/Pincode").Value = model.CustomerAddressDetails != null ? model.CustomerAddressDetails.Pincode : string.Empty;
                document.XPathSelectElement("//Address1/City").Value = model.CustomerAddressDetails != null ? model.CustomerAddressDetails.City : string.Empty;
                document.XPathSelectElement("//Address1/State").Value = model.CustomerAddressDetails != null ? model.CustomerAddressDetails.State : string.Empty;

                document.XPathSelectElement("//Address2/AddrLine1").Value = model.VehicleAddressDetails != null ? model.VehicleAddressDetails.Address1 : string.Empty;
                document.XPathSelectElement("//Address2/AddrLine2").Value = model.VehicleAddressDetails != null ? model.VehicleAddressDetails.Address2 : string.Empty;
                document.XPathSelectElement("//Address2/AddrLine3").Value = model.VehicleAddressDetails != null ? (string.IsNullOrEmpty(model.VehicleAddressDetails.Address3) ? string.Empty : model.VehicleAddressDetails.Address3) : string.Empty;
                document.XPathSelectElement("//Address2/Pincode").Value = model.VehicleAddressDetails != null ? model.VehicleAddressDetails.Pincode : string.Empty;
                document.XPathSelectElement("//Address2/City").Value = model.VehicleAddressDetails != null ? model.VehicleAddressDetails.City : string.Empty;
                document.XPathSelectElement("//Address2/State").Value = model.VehicleAddressDetails != null ? model.VehicleAddressDetails.State : string.Empty;


                document.XPathSelectElement("//MobileNo").Value = model.ClientDetails != null ? ((string.IsNullOrEmpty(model.ClientDetails.MobileNo)) ? string.Empty : model.ClientDetails.MobileNo) : string.Empty;
                document.XPathSelectElement("//EmailAddr").Value = model.ClientDetails != null ? ((string.IsNullOrEmpty(model.ClientDetails.EmailId)) ? "trupti@nodib.com" : model.ClientDetails.EmailId) : "trupti@nodib.com";
                document.XPathSelectElement("//Zone").Value = model.VehicleDetails != null ? model.VehicleDetails.RtoZone : string.Empty;
                //document.XPathSelectElement("//RTOCode").Value = model.VehicleDetails != null ? model.VehicleDetails.RtoZone : string.Empty;
                string RTOCode = string.Empty;
                if (string.IsNullOrEmpty(Convert.ToString(ConfigurationManager.AppSettings["Envi"])))
                {
                    document.XPathSelectElement("//RTOCode").Value = "DL01";
                    document.XPathSelectElement("//POS_MISP/PanNo").Value = "DHNPG6287W";
                }
                else
                {
                    if (Convert.ToString(ConfigurationManager.AppSettings["Envi"]).ToUpper().Equals("UAT"))
                    {
                        document.XPathSelectElement("//RTOCode").Value = "DL01";
                        document.XPathSelectElement("//POS_MISP/PanNo").Value = "DHNPG6287W";
                    }
                    else
                    {

                        var rtocode = ap.COMPANY_WISE_RTO_MASTER.Where(x => x.companyid == 5 && x.andapp_rtoid == model.VehicleDetails.RtoId).FirstOrDefault();
                        if (rtocode != null)
                        {
                            RTOCode = rtocode.rto_loc_code;
                        }
                        document.XPathSelectElement("//RTOCode").Value = RTOCode;
                        string pancard = string.Empty;
                        pancard = ap.POSPMASTERs.Where(x => x.pospid == model.pospid).Select(x => x.pancardno).FirstOrDefault();
                        document.XPathSelectElement("//POS_MISP/PanNo").Value = pancard;
                    }
                }
                //document.XPathSelectElement("//RTOCode").Value = "DL01";
                document.XPathSelectElement("//Make").Value = model.VehicleDetails != null ? model.VehicleDetails.MakeName : string.Empty;
                document.XPathSelectElement("//ModelCode").Value = model.VehicleDetails != null ? model.VehicleDetails.VariantCode : string.Empty;
                document.XPathSelectElement("//RegistrationDate").Value = Convert.ToDateTime(model.VehicleDetails.RegistrationDate).ToString("dd/MM/yyyy");
                //document.XPathSelectElement("//RegistrationDate").Value = Convert.ToString(dateParts[2] + "-" + dateParts[1] + "-" + dateParts[0]);//Convert.ToDateTime(model.VehicleDetails.RegistrationDate).ToString("yyyy-MM-dd");
                document.XPathSelectElement("//ManufacturingYear").Value = Convert.ToString(Convert.ToDateTime(model.VehicleDetails.ManufaturingDate).Year);
                string fuel = Convert.ToString(model.VehicleDetails.Fuel).Substring(0, 1);
                document.XPathSelectElement("//FuelType").Value = fuel;
                //document.XPathSelectElement("//InbuiltKit").Value = model.CoverageDetails != null ? (model.CoverageDetails.IsBiFuelKit ? "N" : (fuel.Equals("C")? "Y" :"N")) : "N";
                document.XPathSelectElement("//IVDOfCNGOrLPG").Value = model.CoverageDetails != null ? (model.CoverageDetails.IsBiFuelKit ? Convert.ToString(model.CoverageDetails.BiFuelKitAmount) : "0") : "0";
                document.XPathSelectElement("//IDV").Value = Convert.ToString(model.IDV);
                document.XPathSelectElement("//BodyType").Value = model.VehicleDetails != null ? ((string.IsNullOrEmpty(model.VehicleDetails.BodyType)) ? "SOLO" : model.VehicleDetails.BodyType) : "SOLO";
                document.XPathSelectElement("//EngineNo").Value = model.VehicleDetails != null ? ((string.IsNullOrEmpty(model.VehicleDetails.EngineNumber)) ? "5154545445456" : model.VehicleDetails.EngineNumber) : "Engineno123456789";
                document.XPathSelectElement("//ChassiNo").Value = model.VehicleDetails != null ? ((string.IsNullOrEmpty(model.VehicleDetails.ChassisNumber)) ? "FCFGFFGG95467655656565" : model.VehicleDetails.ChassisNumber.ToUpper()) : "Chassisno123456789";
                document.XPathSelectElement("//CubicCapacity").Value = model.VehicleDetails != null ? ((string.IsNullOrEmpty(Convert.ToString(model.VehicleDetails.CC))) ? "0" : Convert.ToString(model.VehicleDetails.CC)) : "0";
                document.XPathSelectElement("//SeatingCapacity").Value = model.VehicleDetails != null ? ((string.IsNullOrEmpty(Convert.ToString(model.VehicleDetails.SC))) ? "0" : Convert.ToString(model.VehicleDetails.SC)) : "0";
                document.XPathSelectElement("//ElectricalAccessoriesValues").Value = model.CoverageDetails != null ? (model.CoverageDetails.IsElectricalAccessories ? Convert.ToString(model.CoverageDetails.SIElectricalAccessories) : "0") : "0";
                document.XPathSelectElement("//NonElectricalAccessoriesValues").Value = model.CoverageDetails != null ? (model.CoverageDetails.IsNonElectricalAccessories ? Convert.ToString(model.CoverageDetails.SINonElectricalAccessories) : "0") : "0";
                //document.XPathSelectElement("//PACoverForUnnamedPassengers").Value = model.CoverageDetails.IsPACoverUnnamedPerson ? Convert.ToString(model.CoverageDetails.NumberofPersonsUnnamed) : "0";
                //document.XPathSelectElement("//LegalLiabilitytoPaidDriver").Value = model.CoverageDetails.NoOfLLPaidDriver > 0 ? Convert.ToString(model.CoverageDetails.NoOfLLPaidDriver) : "1";
                document.XPathSelectElement("//LegalLiabilityForOtherEmployees").Value = model.CoverageDetails.IsEmployeeLiability ? "1" : "0";
                document.XPathSelectElement("//CPAYear").Value = model.PolicyType.Equals("New") ? "3" : string.Empty;
                document.XPathSelectElement("//CPAReq").Value = model.CoverageDetails.IsPACoverForOwnerDriver ? "Y" : "N";
                document.XPathSelectElement("//CPANomName").Value = string.IsNullOrEmpty(model.NomineeName) ? string.Empty : model.NomineeName;
                document.XPathSelectElement("//CPANomAge").Value = string.IsNullOrEmpty(model.NomineeDateOfBirth) ? "0" : Convert.ToString(CalculateAge(Convert.ToDateTime(model.NomineeDateOfBirth)));
                document.XPathSelectElement("//CPARelation").Value = string.IsNullOrEmpty(model.NomineeRelationShip) ? string.Empty : model.NomineeRelationShip;
                document.XPathSelectElement("//CPAAppointeeName").Value = string.IsNullOrEmpty(model.AppointeeName) ? string.Empty : model.AppointeeName;
                document.XPathSelectElement("//CPAAppointeRel").Value = string.IsNullOrEmpty(model.AppointeeRelationShip) ? string.Empty : model.AppointeeRelationShip;
                document.XPathSelectElement("//RollOver").Value = model.PolicyType.Equals("New", StringComparison.OrdinalIgnoreCase) ? "N" : "Y";
                //document.XPathSelectElement("//InsuredName").Value = model.PreviousPolicyDetails != null ? ((string.IsNullOrEmpty(model.PreviousPolicyDetails.PreviousCompanyName)) ? "0" : model.PreviousPolicyDetails.PreviousCompanyName) : "0";
                //document.XPathSelectElement("//PreviousPolExpDt").Value = model.PreviousPolicyDetails != null ? model.PreviousPolicyDetails.PreviousPolicyEndDate : "0001-01-01";
                //document.XPathSelectElement("//NCBInExpiringPolicy").Value = model.PreviousPolicyDetails != null ? model.PreviousPolicyDetails.PreviousNcbPercentage : "0";
                //document.XPathSelectElement("//ClaimInExpiringPolicy").Value = model.PreviousPolicyDetails != null ? (model.PreviousPolicyDetails.IsPreviousInsuranceClaimed ? "Y" : "N") : "N";
                document.XPathSelectElement("//NewVehicle").Value = model.PolicyType.Equals("New", StringComparison.OrdinalIgnoreCase) ? "Y" : "N";
                //document.XPathSelectElement("//PACoverForUnnamedPassengers").Value = model.CoverageDetails.IsPACoverUnnamedPerson ? Convert.ToString(model.VehicleDetails.SC) : "0";
                document.XPathSelectElement("//LegalLiabilitytoPaidDriver").Value = model.CoverageDetails.NoOfLLPaidDriver > 0 ? Convert.ToString(model.CoverageDetails.NoOfLLPaidDriver) : "0";
                //document.XPathSelectElement("//LegalLiabilityForOtherEmployees").Value = model.CoverageDetails.IsLLEmployee ? "1" : "0";
                if (model.IsODOnly && model.PreviousTPPolicyDetails != null)
                {
                    PrvTPPolicyStartDate = Convert.ToDateTime(model.PreviousTPPolicyDetails.PolicyEndDate).AddYears(-3).AddDays(1).ToString("yyyy-MM-dd");
                    //DateTime PrvTPPolicyStartDate = Convert.ToDateTime(model.PreviousTPPolicyDetails.PolicyEndDate).AddYears(-3).AddDays(1);
                    document.XPathSelectElement("//Cover").Value = "OD";
                    document.XPathSelectElement("//CPAReq").Value = "N";
                    document.XPathSelectElement("//PACoverForUnnamedPassengers").Value = "0";
                    document.XPathSelectElement("//LegalLiabilityForOtherEmployees").Value = "0";
                    string TPInsName = string.Empty;
                    if (string.IsNullOrEmpty(Convert.ToString(ConfigurationManager.AppSettings["Envi"])))
                    {
                        document.XPathSelectElement("//PreviousInsurer").Value = "Bajaj Allianz General Insurance Co Ltd.";
                    }
                    else
                    {
                        if (Convert.ToString(ConfigurationManager.AppSettings["Envi"]).ToUpper().Equals("UAT"))
                        {
                            document.XPathSelectElement("//PreviousInsurer").Value = "Bajaj Allianz General Insurance Co Ltd.";
                        }
                        else
                        {
                            var TPcompanycode = ap.PREVIOUS_INSURER_MAPPING.Where(x => x.companyid == 5 && x.previouscompanyid == model.PreviousTPPolicyDetails.CompanyId).FirstOrDefault();
                            if (TPcompanycode != null)
                            {
                                TPInsName = Convert.ToString(TPcompanycode.inscompanyname);
                            }
                            else
                            {
                                resModel.Status = Status.Fail;
                                resModel.ErrorMsg = "FGI not provide previous insurance company";
                                return resModel;
                            }
                            document.XPathSelectElement("//PreviousInsurer").Value = TPInsName;
                        }
                    }
                    //document.XPathSelectElement("//PreviousInsurer").Value = "Bajaj Allianz General Insurance Co Ltd.";
                    document.XPathSelectElement("//TPPolicyNumber").Value = model.PreviousTPPolicyDetails.PolicyNo;
                    document.XPathSelectElement("//TPPolicyEffdate").Value = Convert.ToDateTime(PrvTPPolicyStartDate).ToString("dd/MM/yyyy");
                    document.XPathSelectElement("//TPPolicyExpiryDate").Value = Convert.ToDateTime(model.PreviousTPPolicyDetails.PolicyEndDate).ToString("dd/MM/yyyy");
                }
                if (model.IsThirdPartyOnly)
                {
                    document.XPathSelectElement("//Cover").Value = "LO";
                    document.XPathSelectElement("//AddonReq").Value = "N";
                }
                if (!model.PolicyType.Equals("New") && model.PreviousPolicyDetails != null)
                {
                    var sdate1 = Convert.ToDateTime(model.PreviousPolicyDetails.PreviousPolicyEndDate).AddYears(-1).AddDays(1).ToString("yyyy-MM-dd");
                    //model.PreviousPolicyDetails.PreviousPolicyStartDate = model.PolicyEndDate = Convert.ToDateTime(sdate1).AddYears(-1).AddDays(1).ToString("yyyy-MM-dd");
                    //model.PreviousPolicyDetails.PreviousPolicyStartDate = Convert.ToDateTime(model.PreviousPolicyDetails.PreviousPolicyEndDate).AddYears(-1).AddDays(1).ToString();
                    document.XPathSelectElement("//RollOverList/PolicyNo").Value = model.PreviousPolicyDetails.PreviousPolicyNo;
                    string InsCode = string.Empty;
                    string InsName = string.Empty;

                    if (string.IsNullOrEmpty(Convert.ToString(ConfigurationManager.AppSettings["Envi"])))
                    {

                        document.XPathSelectElement("//InsuredName").Value = "Bajaj Allianz General Insurance Co Ltd.";
                        document.XPathSelectElement("//ClientCode").Value = "40062645";

                    }
                    else
                    {
                        if (Convert.ToString(ConfigurationManager.AppSettings["Envi"]).ToUpper().Equals("UAT"))
                        {
                            document.XPathSelectElement("//InsuredName").Value = "Bajaj Allianz General Insurance Co Ltd.";
                            document.XPathSelectElement("//ClientCode").Value = "40062645";
                            //document.XPathSelectElement("//POS_MISP/PanNo").Value = "DHNPG6287W";
                        }
                        else
                        {
                            var companycode = ap.PREVIOUS_INSURER_MAPPING.Where(x => x.companyid == 5 && x.previouscompanyid == model.PreviousPolicyDetails.CompanyId).FirstOrDefault();
                            if (companycode != null)
                            {
                                InsCode = Convert.ToString(companycode.inscompanycode);
                                InsName = Convert.ToString(companycode.inscompanyname);
                            }
                            else
                            {
                                resModel.Status = Status.Fail;
                                resModel.ErrorMsg = "FGI not provide previous insurance company";
                                return resModel;
                            }
                            document.XPathSelectElement("//InsuredName").Value = InsName;
                            document.XPathSelectElement("//ClientCode").Value = InsCode;
                        }
                    }
                    //document.XPathSelectElement("//InsuredName").Value = "Bajaj Allianz General Insurance Co Ltd.";
                    //document.XPathSelectElement("//ClientCode").Value = "40062645";
                    document.XPathSelectElement("//PreviousPolExpDt").Value = Convert.ToDateTime(model.PreviousPolicyDetails.PreviousPolicyEndDate).ToString("dd/MM/yyyy");
                    document.XPathSelectElement("//NCBDeclartion").Value = model.PreviousPolicyDetails.IsPreviousInsuranceClaimed ? "N" : "Y";
                    document.XPathSelectElement("//ClaimInExpiringPolicy").Value = model.PreviousPolicyDetails.IsPreviousInsuranceClaimed ? "Y" : "N";
                    document.XPathSelectElement("//NCBInExpiringPolicy").Value = model.PreviousPolicyDetails.PreviousNcbPercentage;
                    document.XPathSelectElement("//PreviousPolStartDt").Value = Convert.ToDateTime(sdate1).ToString("dd/MM/yyyy");
                }
                bool flag = false;
                int vehiAge = CalculateAge(Convert.ToDateTime(model.VehicleDetails.RegistrationDate));
                if (model.AddonCover.IsRoadSideAssistance)
                {
                    if (model.AddonCover.IsZeroDeperation || model.AddonCover.IsConsumables || model.AddonCover.IsTyreCover || model.AddonCover.IsConsumables ||
                        model.AddonCover.IsTyreCover || model.AddonCover.IsEngineProtector || model.AddonCover.IsReturntoInvoice ||
                    model.AddonCover.IsLossofKey || model.AddonCover.IsLossofpersonalBelonging || model.AddonCover.IsPassengerAssistcover || model.AddonCover.IsHydrostaticLockCover
                    || model.AddonCover.IsHospitalCashCover || model.AddonCover.IsRimProtectionCover
                    || model.AddonCover.IsEmergencyCover
                    || model.AddonCover.IsMedicalExpensesSelected ||
                    model.AddonCover.IsAmbulanceChargesSelected)
                    {
                        flag = true;
                    }
                    if (!flag)
                    {
                        if (vehiAge <= 15)
                        {
                            document.XPathSelectElement("//AddonReq").Value = "Y";
                            document.XPathSelectElement("//CoverCode").Value = "PLAN2";
                            //document.Root.Element("Risk").Element("AddonReq").AddAfterSelf(new XElement("Addon", new XElement("CoverCode", "PLAN2")));
                        }

                    }
                    else
                    {
                        if (vehiAge <= 5)
                        {
                            if (model.AddonCover.IsZeroDeperation || model.AddonCover.IsLossofKey || model.AddonCover.IsLossofpersonalBelonging || model.AddonCover.IsRoadSideAssistance)
                            {
                                document.XPathSelectElement("//AddonReq").Value = "Y";
                                //document.Root.Element("Risk").Element("AddonReq").AddAfterSelf(new XElement("Addon", new XElement("CoverCode", "PLAN1")));
                            }
                            if (model.AddonCover.IsEngineProtector)
                            {
                                if (vehiAge <= 3)
                                {
                                    document.XPathSelectElement("//AddonReq").Value = "Y";
                                    document.Root.Element("Risk").Element("AddonReq").AddAfterSelf(new XElement("Addon", new XElement("CoverCode", "ENGPR")));
                                }

                            }
                            if (model.AddonCover.IsTyreCover)
                            {
                                if (vehiAge <= 2)
                                {
                                    document.XPathSelectElement("//AddonReq").Value = "Y";
                                    document.Root.Element("Risk").Element("AddonReq").AddAfterSelf(new XElement("Addon", new XElement("CoverCode", "00001")));
                                }
                            }
                            if (model.AddonCover.IsNCBProtection)
                            {
                                document.XPathSelectElement("//AddonReq").Value = "Y";
                                document.Root.Element("Risk").Element("AddonReq").AddAfterSelf(new XElement("Addon", new XElement("CoverCode", "00004")));
                            }
                            if (model.AddonCover.IsConsumables)
                            {
                                document.XPathSelectElement("//AddonReq").Value = "Y";
                                document.Root.Element("Risk").Element("AddonReq").AddAfterSelf(new XElement("Addon", new XElement("CoverCode", "00005")));
                            }
                            if (model.AddonCover.IsReturntoInvoice)
                            {
                                if (vehiAge <= 3)
                                {
                                    document.XPathSelectElement("//AddonReq").Value = "Y";
                                    document.Root.Element("Risk").Element("AddonReq").AddAfterSelf(new XElement("Addon", new XElement("CoverCode", "00006")));
                                }

                            }
                        }

                    }
                }
                else
                {
                    if (vehiAge <= 5)
                    {
                        if (model.AddonCover.IsZeroDeperation || model.AddonCover.IsLossofKey || model.AddonCover.IsLossofpersonalBelonging || model.AddonCover.IsRoadSideAssistance)
                        {
                            document.XPathSelectElement("//AddonReq").Value = "Y";
                            //document.Root.Element("Risk").Element("AddonReq").AddAfterSelf(new XElement("Addon", new XElement("CoverCode", "PLAN1")));
                        }
                        if (model.AddonCover.IsEngineProtector)
                        {
                            if (vehiAge <= 3)
                            {
                                document.XPathSelectElement("//AddonReq").Value = "Y";
                                document.Root.Element("Risk").Element("AddonReq").AddAfterSelf(new XElement("Addon", new XElement("CoverCode", "ENGPR")));
                            }

                        }
                        if (model.AddonCover.IsTyreCover)
                        {
                            if (vehiAge <= 2)
                            {
                                document.XPathSelectElement("//AddonReq").Value = "Y";
                                document.Root.Element("Risk").Element("AddonReq").AddAfterSelf(new XElement("Addon", new XElement("CoverCode", "00001")));
                            }
                        }
                        if (model.AddonCover.IsNCBProtection)
                        {
                            document.XPathSelectElement("//AddonReq").Value = "Y";
                            document.Root.Element("Risk").Element("AddonReq").AddAfterSelf(new XElement("Addon", new XElement("CoverCode", "00004")));
                        }
                        if (model.AddonCover.IsConsumables)
                        {
                            document.XPathSelectElement("//AddonReq").Value = "Y";
                            document.Root.Element("Risk").Element("AddonReq").AddAfterSelf(new XElement("Addon", new XElement("CoverCode", "00005")));
                        }
                        if (model.AddonCover.IsReturntoInvoice)
                        {
                            if (vehiAge <= 3)
                            {
                                document.XPathSelectElement("//AddonReq").Value = "Y";
                                document.Root.Element("Risk").Element("AddonReq").AddAfterSelf(new XElement("Addon", new XElement("CoverCode", "00006")));
                            }

                        }
                    }
                }
                document.XPathSelectElement("//Code").Value = model.VehicleDetails.IsVehicleLoan ? "HY" : string.Empty;
                document.XPathSelectElement("//BankName").Value = model.VehicleDetails.IsVehicleLoan ? (string.IsNullOrEmpty(model.VehicleDetails.LoanCompanyName) ? string.Empty : model.VehicleDetails.LoanCompanyName.Trim()) : string.Empty;
                document.XPathSelectElement("//POS_MISP/Type").Value = "P";
                string response = Ser.CreatePolicy("Motor", Convert.ToString(document));
                //ap.SP_REQUEST_RESPONSE_MASTER("I", null, model.enquiryid, 5, Convert.ToString(document), Convert.ToString(response));
                ap.SP_REQUEST_RESPONSE_API_MASTER(model.enquiryid, 5, Convert.ToString(document), Convert.ToString(response));
                resModel = GetProposalResponse(XDocument.Parse(response));
                ap.SP_Payment_Parameter(model.enquiryid, 5, "LoadingDis", resModel.CompanyWiseRefference.LoadingDiscount);
                if (!resModel.Status.Equals("Fail"))
                {
                    string customeraddress = model.CustomerAddressDetails.Address1 + " " + model.CustomerAddressDetails.Address2 + " " + model.CustomerAddressDetails.Address3 + " " + model.CustomerAddressDetails.Pincode;
                    //string customeraddress = model.CustomerAddressDetails.Address1 + " " + model.CustomerAddressDetails.Address2 + " " + model.CustomerAddressDetails.Address3 + " " + model.CustomerAddressDetails.Pincode;
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

                    ap.SP_POLICYDETAILSMASTER("I", model.enquiryid, 5, model.pospid, model.CustomerType, model.PolicyType, producttype,
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
                        1, model.CurrentNcb, Convert.ToDecimal(resModel.PremiumBreakUpDetails.NCBDiscount), null,
                        model.IDV, Convert.ToDecimal(resModel.PremiumBreakUpDetails.NetAddonPremium),
                        Convert.ToDecimal(model.PremiumDetails.OdPremiumAmount),
                        Convert.ToDecimal(model.PremiumDetails.TpPremiumAmount),
                        Convert.ToDecimal(model.PremiumDetails.NetPremiumAmount),
                        Convert.ToDecimal(model.PremiumDetails.TaxAmount),
                        Convert.ToDecimal(model.PremiumDetails.TotalPremiumAmount), false);

                    if (model.IsThirdPartyOnly)
                    {
                        resModel.IDV = 0;
                    }
                }

                else
                {
                    resModel.IDV = 0;
                }
            }
            catch (Exception ex)
            {
                resModel.Status = Status.Fail;
                resModel.ErrorMsg = Convert.ToString(ex.Message);
                Console.Write(Convert.ToString(ex.Message));
                LogU.WriteLog("FGI >> PrivateCar >> GetProposalRequest >> " + Convert.ToString(ex.Message));
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
            resModel.CompanyName = Company.FGI.ToString();
            resModel.PolicyStartDate = model.PolicyStartDate;
            resModel.PolicyEndDate = model.PolicyEndDate;
            resModel.CompanyWiseRefference.OrderNo = model.enquiryid;
            return resModel;
        }

        public Response GetProposalResponse(XDocument res)
        {
            Response resModel = new Response();
            DataSet ds = new DataSet();
            resModel.PremiumBreakUpDetails = new PremiumBreakUpDetails();
            resModel.CompanyWiseRefference = new CompanyWiseRefference();
            try
            {
                string status = res.XPathSelectElement("//Status").Value;
                if ((!string.IsNullOrEmpty(status)) && status.Equals("Successful", StringComparison.OrdinalIgnoreCase))
                {
                    resModel.IDV = Convert.ToInt32(res.XPathSelectElement("//VehicleIDV").Value);
                    ds.ReadXml(new XmlTextReader(new StringReader(Convert.ToString(res))));
                    List<XElement> xElementList = res.Descendants("Table1").ToList();
                    resModel.Status = Status.Success;
                    if (ds.Tables.Count > 0)
                    {
                        if (ds.Tables.Contains("Policy"))
                        {
                            DataTable dt_Policy = ds.Tables["Policy"];
                            DataTable dt_Table = ds.Tables["Table"];
                            DataTable dt_Table1 = ds.Tables["Table1"];

                            if (Convert.ToString(dt_Policy.Rows[0]["Status"].ToString()) == "Successful")
                            {
                                if (dt_Table.Rows.Count > 0)
                                {
                                    resModel.PolicyNo = Convert.ToString(dt_Table.Rows[0]["PolNo"].ToString());

                                }

                                for (int i = 0; i < dt_Table1.Rows.Count; i++)
                                {
                                    //AddonDetails objent = new AddonDetails();

                                    string code = Convert.ToString(dt_Table1.Rows[i]["Code"].ToString()).Trim();
                                    string description = Convert.ToString(dt_Table1.Rows[i]["Description"].ToString()).Trim();
                                    string type = Convert.ToString(dt_Table1.Rows[i]["Type"].ToString()).Trim();
                                    string BOValue = Convert.ToString(dt_Table1.Rows[i]["BOValue"].ToString()).Trim();
                                    string DBValue = Convert.ToString(dt_Table1.Rows[i]["DBValue"].ToString()).Trim();


                                    if (code == "Gross Premium")
                                    {
                                        if (type == "OD")
                                        {
                                            resModel.PremiumBreakUpDetails.BasicODPremium = Math.Round(Convert.ToDouble(BOValue), 2);
                                        }
                                        else if (type == "TP")
                                        {
                                            resModel.PremiumBreakUpDetails.BasicThirdPartyLiability = Math.Round(Convert.ToDouble(BOValue), 2);
                                        }
                                    }
                                    if (code == "CNG")
                                    {
                                        if (type == "OD")
                                        {
                                            resModel.PremiumBreakUpDetails.CNGLPGKitPremium = Math.Round(Convert.ToDouble(BOValue), 2);
                                        }
                                        else if (type == "TP")
                                        {
                                            resModel.PremiumBreakUpDetails.TPCNGLPGPremium = Math.Round(Convert.ToDouble(BOValue), 2);
                                        }
                                    }
                                    else if (code == "NCB")
                                    {
                                        if (type == "OD")
                                        {
                                            string Ncbdis = Convert.ToString(BOValue).Substring(1);
                                            resModel.PremiumBreakUpDetails.NCBDiscount += Math.Round(Convert.ToDouble(Ncbdis));
                                        }
                                        else if (type == "TP")
                                        {
                                            string NcbdisTP = Convert.ToString(BOValue).Substring(1);
                                            resModel.PremiumBreakUpDetails.NCBDiscount += Math.Round(Convert.ToDouble(NcbdisTP));
                                        }
                                    }
                                    else if (code == "APA")
                                    {
                                        if (type == "OD")
                                        {
                                            resModel.PremiumBreakUpDetails.PACoverToUnNamedPerson += Math.Round(Convert.ToDouble(BOValue), 2);
                                        }
                                        else if (type == "TP")
                                        {
                                            resModel.PremiumBreakUpDetails.PACoverToUnNamedPerson += Math.Round(Convert.ToDouble(BOValue), 2);
                                        }
                                    }
                                    else if (code == "LLDE")
                                    {
                                        if (type == "TP")
                                        {
                                            resModel.PremiumBreakUpDetails.LLToPaidDriver = Math.Round(Convert.ToDouble(BOValue), 2);
                                        }
                                    }
                                    else if (code == "LLOE")
                                    {

                                        if (type == "TP")
                                        {
                                            resModel.PremiumBreakUpDetails.LLToPaidEmployee = Math.Round(Convert.ToDouble(BOValue), 2);
                                        }
                                    }
                                    else if (code == "LOADDISC")
                                    {

                                        if (type == "OD")
                                        {
                                            string dis = Convert.ToString(BOValue).Substring(2);
                                            resModel.PremiumBreakUpDetails.OtherDiscount += Math.Round(Convert.ToDouble(dis), 2);
                                        }
                                        else if (type == "TP")
                                        {
                                            resModel.PremiumBreakUpDetails.OtherDiscount += Math.Round(Convert.ToDouble(BOValue), 2);
                                        }
                                    }
                                    else if (code == "DISCPERC")
                                    {

                                        if (type == "OD")
                                        {
                                            string dis = Convert.ToString(BOValue).Substring(0, 3);
                                            resModel.CompanyWiseRefference.LoadingDiscount = dis;
                                        }
                                        //else if (type == "TP")
                                        //{
                                        //    resModel.PremiumBreakUpDetails.OtherDiscount += Math.Round(Convert.ToDouble(BOValue), 2);
                                        //}
                                    }
                                    //else if (code == "IDV")
                                    //{
                                    //    if (description == "IDV" && type == "OD")
                                    //    {
                                    //        resModel.IDV += Convert.ToDouble(BOValue);
                                    //    }
                                    //    else if (description == "IDV" && type == "TP")
                                    //    {
                                    //        resModel.IDV += Convert.ToDouble(BOValue);
                                    //    }
                                    //}
                                    else if (code == "ServTax")
                                    {

                                        if (type == "OD")
                                        {
                                            resModel.PremiumBreakUpDetails.ServiceTax += Math.Round(Convert.ToDouble(BOValue), 2);
                                        }
                                        else if (type == "TP")
                                        {
                                            resModel.PremiumBreakUpDetails.ServiceTax += Math.Round(Convert.ToDouble(BOValue), 2);
                                        }

                                    }
                                    else if (code == "PrmDue")
                                    {

                                        if (type == "OD")
                                        {
                                            resModel.FinalPremium += Math.Round(Convert.ToDouble(BOValue), 2);
                                        }
                                        else if (type == "TP")
                                        {
                                            resModel.FinalPremium += Math.Round(Convert.ToDouble(BOValue), 2);
                                        }

                                    }
                                    else if (code == "MOTADON" && type == "OD")
                                    {
                                        resModel.PremiumBreakUpDetails.NetAddonPremium = Math.Round(Convert.ToDouble(BOValue), 2);
                                    }
                                    else if (code == "00001")                      //tyre
                                    {
                                        //objent.Code = code;
                                        //objent.Type = type;
                                        //objent.Description = description;
                                        //objent.Premium = BOValue;
                                        resModel.PremiumBreakUpDetails.TyreProtect = Math.Round(Convert.ToDouble(BOValue), 2);
                                        //resModel.PremiumBreakUpDetails.NetAddonPremium += Convert.ToDouble(BOValue);
                                        //listent.Add(objent);
                                    }
                                    else if (code == "00002")                //INconvenience
                                    {
                                        //objent.Code = code;
                                        //objent.Type = type;
                                        //objent.Description = description;
                                        //objent.Premium = BOValue;
                                        resModel.PremiumBreakUpDetails.InvoicePriceCoverPremium = Math.Round(Convert.ToDouble(BOValue), 2);
                                        //resModel.PremiumBreakUpDetails.NetAddonPremium += Convert.ToDouble(BOValue);
                                        //listent.Add(objent);
                                    }
                                    else if (code == "00004")        // ncb
                                    {
                                        //objent.Code = code;
                                        //objent.Type = type;
                                        //objent.Description = description;
                                        //objent.Premium = BOValue;
                                        resModel.PremiumBreakUpDetails.NcbProtectorPremium = Math.Round(Convert.ToDouble(BOValue), 2);
                                        //resModel.PremiumBreakUpDetails.NetAddonPremium += Convert.ToDouble(BOValue);
                                        //listent.Add(objent);
                                    }
                                    else if (code == "00005")      //consumables
                                    {
                                        //objent.Code = code;
                                        //objent.Type = type;
                                        //objent.Description = description;
                                        //objent.Premium = BOValue;
                                        resModel.PremiumBreakUpDetails.CostOfConsumablesPremium = Math.Round(Convert.ToDouble(BOValue), 2);
                                        //resModel.PremiumBreakUpDetails.NetAddonPremium += Convert.ToDouble(BOValue);
                                        //listent.Add(objent);
                                    }
                                    else if (code == "00006")        //Invoice
                                    {
                                        //objent.Code = code;
                                        //objent.Type = type;
                                        //objent.Description = description;
                                        //objent.Premium = BOValue;
                                        resModel.PremiumBreakUpDetails.InvoicePriceCoverPremium = Math.Round(Convert.ToDouble(BOValue), 2);
                                        //resModel.PremiumBreakUpDetails.NetAddonPremium += Convert.ToDouble(BOValue);
                                        //listent.Add(objent);
                                    }
                                    else if (code == "PLAN1")
                                    {
                                        //objent.Code = code;
                                        //objent.Type = type;
                                        //objent.Description = description;
                                        //objent.Premium = BOValue;
                                        resModel.PremiumBreakUpDetails.ZeroDepPremium = Math.Round(Convert.ToDouble(BOValue), 2);
                                        //listent.Add(objent);
                                    }
                                    else if (code == "PLAN2")
                                    {
                                        //objent.Code = code;
                                        //objent.Type = type;
                                        //objent.Description = description;
                                        //objent.Premium = BOValue;
                                        resModel.PremiumBreakUpDetails.RSAPremium = Math.Round(Convert.ToDouble(BOValue), 2);
                                        //listent.Add(objent);
                                    }

                                    else if (code == "ENGPR")
                                    {
                                        //objent.Code = code;
                                        //objent.Type = type;
                                        //objent.Description = description;
                                        //objent.Premium = BOValue;
                                        resModel.PremiumBreakUpDetails.EngineProtectorPremium = Math.Round(Convert.ToDouble(BOValue), 2);
                                        //resModel.PremiumBreakUpDetails.NetAddonPremium += Convert.ToDouble(BOValue);
                                        //listent.Add(objent);
                                    }

                                    else if (code == "CPA")
                                    {
                                        resModel.PremiumBreakUpDetails.PACoverToOwnDriver = Math.Round(Convert.ToDouble(BOValue), 2);
                                    }
                                }
                                resModel.PremiumBreakUpDetails.NetPremium = Math.Round(resModel.FinalPremium - resModel.PremiumBreakUpDetails.ServiceTax);


                            }
                        }
                    }
                }
                else
                {
                    resModel.Status = Status.Fail;
                    //resModel.ErrorMsg = Convert.ToString(res.XPathSelectElement("//Policy/ErrorMessage"));
                    if (!string.IsNullOrEmpty(Convert.ToString(res.XPathSelectElement("//ValidationError"))))
                    {
                        resModel.ErrorMsg = Convert.ToString(res.XPathSelectElement("//ValidationError"));
                    }
                    else if (!string.IsNullOrEmpty(Convert.ToString(res.XPathSelectElement("//Policy/ErrorMessage"))))
                    {
                        resModel.ErrorMsg = Convert.ToString(res.XPathSelectElement("//Policy/ErrorMessage"));
                    }
                    else
                    {
                        resModel.ErrorMsg = "Getting an error from Future Service";
                    }
                }
            }
            catch (Exception ex)
            {
                resModel.Status = Status.Fail;
                resModel.ErrorMsg = Convert.ToString(ex.Message);
                Console.Write(Convert.ToString(ex.Message));
                LogU.WriteLog("FGI >> PrivateCar >> GetProposalResponse >> " + Convert.ToString(ex.Message));
            }

            return resModel;
        }

        public Response GetProposalRequestAfterPayment(PaymentRequest model)
        {
            Response resModel = new Response();
            try
            {
                model.CompanyDetail.QuoteNo = "07/15/2021";
                XDocument x = XDocument.Parse(model.CompanyDetail.QuoteId);
                var TransactionDate = Convert.ToDateTime(model.CompanyDetail.QuoteNo).ToString("yyyy-MM-dd");
                x.XPathSelectElement("//METHOD").Value = "CRT";
                x.XPathSelectElement("//Amount").Value = Convert.ToString(model.FinalPremium);
                x.XPathSelectElement("//UniqueTranKey").Value = Convert.ToString(model.CompanyDetail.CorrelationId);
                x.XPathSelectElement("//TransactionDate").Value = Convert.ToDateTime(TransactionDate).ToString("dd/MM/yyyy");
                x.XPathSelectElement("//TranRefNo").Value = Convert.ToString(model.CompanyDetail.applicationId);
                x.XPathSelectElement("//TranRefNoDate").Value = Convert.ToDateTime(TransactionDate).ToString("dd/MM/yyyy");
                x.XPathSelectElement("//Uid").Value = Convert.ToString(GenerateRandomNo());
                x.XPathSelectElement("//Discount").Value = Convert.ToString(model.CompanyDetail.LoadingDiscount);
                //x.XPathSelectElement("//Pincode").Value = "380051";//Convert.ToString(model.CompanyDetail.LoadingDiscount);
                //x.XPathSelectElement("//State").Value = "GJ";//Convert.ToString(model.CompanyDetail.LoadingDiscount);
                //x.XPathSelectElement("//City").Value = "Ahmedabad";//Convert.ToString(model.CompanyDetail.LoadingDiscount);
                //x.XPathSelectElement("//Amount").Value = Convert.ToString("26878");
                var response = Ser.CreatePolicy("Motor", Convert.ToString(x));
                ap.SP_REQUEST_RESPONSE_API_MASTER(model.CompanyDetail.OrderNo, 5, Convert.ToString(x), Convert.ToString(response));
                LogU.WriteLog("FGI >> PrivateCar >> GetProposalRequestAfterPayment >> Request >> " + Convert.ToString(x));
                LogU.WriteLog("FGI >> PrivateCar >> GetProposalRequestAfterPayment >> Response >> " + Convert.ToString(response));
                resModel = GetProposalResponse(XDocument.Parse(response));
                if (resModel.Status == Status.Success)
                {
                    //resModel.PolicyNo = "V0100173";
                    //FGI_Policy_Pdf.PDFSoapClient pdf = new FGI_Policy_Pdf.PDFSoapClient();
                    //DataTable dt = pdf.GetPDF(resModel.PolicyNo, Convert.ToString(ConfigurationManager.AppSettings["FGIPolicyPdfUName"]), Convert.ToString(ConfigurationManager.AppSettings["FGIPolicyPdfPwd"]));
                    ////string dtddd = dt.ToString();
                    //byte[] tempByteArray = new byte[0];
                    //tempByteArray=(byte[])dt["GetPDFResult"];

                }
                else
                {
                    resModel.ErrorMsg = "Getting an error from Future Generali";
                    resModel.Status = Status.Fail;
                    LogU.WriteLog("FGI >> PrivateCar >> GetProposalRequestAfterPayment >> " + Convert.ToString(resModel.ErrorMsg));
                }

            }
            catch (Exception ex)
            {
                resModel.Status = Status.Fail;
                resModel.ErrorMsg = Convert.ToString(ex.Message);
                Console.Write(Convert.ToString(ex.Message));
                LogU.WriteLog("FGI >> PrivateCar >> GetProposalRequestAfterPayment >> " + Convert.ToString(ex.Message));
            }
            return resModel;
        }

        /// <summary>
        /// Proposal response method.
        /// </summary>
        /// <param name="res">xdocument objects.</param>
        /// <returns>return response type object.</returns>
        //public Response GetProposalResponse(XDocument res)
        //{
        //    Response resModel = new Response();
        //    DataSet ds = new DataSet();
        //    string status = res.XPathSelectElement("//Status").Value;
        //    if ((!string.IsNullOrEmpty(status)) && status.Equals("Successful", StringComparison.OrdinalIgnoreCase))
        //    {
        //        try
        //        {
        //            if ((!string.IsNullOrEmpty(status)) && status.Equals("Successful", StringComparison.OrdinalIgnoreCase))
        //            {
        //                resModel.IDV = Convert.ToInt32(res.XPathSelectElement("//VehicleIDV").Value);
        //                ds.ReadXml(new XmlTextReader(new StringReader(Convert.ToString(res))));
        //                List<XElement> xElementList = res.Descendants("Table1").ToList();
        //                resModel.Status = Status.Success;
        //                if (ds.Tables.Count > 0)
        //                {
        //                    if (ds.Tables.Contains("Policy"))
        //                    {
        //                        DataTable dt_Policy = ds.Tables["Policy"];
        //                        DataTable dt_Table = ds.Tables["Table"];
        //                        DataTable dt_Table1 = ds.Tables["Table1"];

        //                        if (Convert.ToString(dt_Policy.Rows[0]["Status"].ToString()) == "Successful")
        //                        {
        //                            if (dt_Table.Rows.Count > 0)
        //                            {
        //                                resModel.PolicyNo = Convert.ToString(dt_Table.Rows[0]["PolNo"].ToString());

        //                            }
        //                            for (int i = 0; i < dt_Table1.Rows.Count; i++)
        //                            {
        //                                string code = Convert.ToString(dt_Table1.Rows[i]["Code"].ToString()).Trim();
        //                                string description = Convert.ToString(dt_Table1.Rows[i]["Description"].ToString()).Trim();
        //                                string type = Convert.ToString(dt_Table1.Rows[i]["Type"].ToString()).Trim();
        //                                string BOValue = Convert.ToString(dt_Table1.Rows[i]["BOValue"].ToString()).Trim();
        //                                string DBValue = Convert.ToString(dt_Table1.Rows[i]["DBValue"].ToString()).Trim();
        //                                if (code == "Gross Premium")
        //                                {
        //                                    if (description == "Gross Premium" && type == "OD")
        //                                    {
        //                                        resModel.PremiumBreakUpDetails.NetODPremium = Convert.ToDouble(BOValue);
        //                                    }
        //                                    else if (description == "Gross Premium" && type == "TP")
        //                                    {
        //                                        resModel.PremiumBreakUpDetails.NetTPPremium = Convert.ToDouble(BOValue);
        //                                    }
        //                                }
        //                                else if (code == "ServTax")
        //                                {
        //                                    if (description == "ServTax")
        //                                    {
        //                                        if (type == "OD")
        //                                        {
        //                                            resModel.PremiumBreakUpDetails.ServiceTax += Convert.ToDouble(BOValue);
        //                                        }
        //                                        else if (type == "TP")
        //                                        {
        //                                            resModel.PremiumBreakUpDetails.ServiceTax += Convert.ToDouble(BOValue);
        //                                        }
        //                                    }
        //                                }
        //                                else if (code == "00001")                      //tyre
        //                                {
        //                                    resModel.PremiumBreakUpDetails.TyreProtect = Convert.ToDouble(BOValue);
        //                                    resModel.PremiumBreakUpDetails.NetAddonPremium += Convert.ToDouble(BOValue);
        //                                }
        //                                else if (code == "00002")                //INconvenience
        //                                {
        //                                    resModel.PremiumBreakUpDetails.InvoicePriceCoverPremium = Convert.ToDouble(BOValue);
        //                                    resModel.PremiumBreakUpDetails.NetAddonPremium += Convert.ToDouble(BOValue);
        //                                }
        //                                else if (code == "00004")        // ncb
        //                                {
        //                                    resModel.PremiumBreakUpDetails.NcbProtectorPremium = Convert.ToDouble(BOValue);
        //                                    resModel.PremiumBreakUpDetails.NetAddonPremium += Convert.ToDouble(BOValue);
        //                                }
        //                                else if (code == "00005")      //consumables
        //                                {
        //                                    resModel.PremiumBreakUpDetails.CostOfConsumablesPremium = Convert.ToDouble(BOValue);
        //                                    resModel.PremiumBreakUpDetails.NetAddonPremium += Convert.ToDouble(BOValue);
        //                                }
        //                                else if (code == "00006")        //Invoice
        //                                {
        //                                    resModel.PremiumBreakUpDetails.InvoicePriceCoverPremium = Convert.ToDouble(BOValue);
        //                                    resModel.PremiumBreakUpDetails.NetAddonPremium += Convert.ToDouble(BOValue);
        //                                }
        //                                else if (code == "PLAN1")
        //                                {
        //                                    resModel.PremiumBreakUpDetails.NetAddonPremium += Convert.ToDouble(BOValue);
        //                                }

        //                                else if (code == "ENGPR")
        //                                {
        //                                    //objent.Code = code;
        //                                    //objent.Type = type;
        //                                    //objent.Description = description;
        //                                    //objent.Premium = BOValue;
        //                                    resModel.PremiumBreakUpDetails.EngineProtectorPremium = Convert.ToDouble(BOValue);
        //                                    resModel.PremiumBreakUpDetails.NetAddonPremium += Convert.ToDouble(BOValue);
        //                                    //listent.Add(objent);
        //                                }
        //                                else if (code == "CPA")
        //                                {
        //                                    resModel.PremiumBreakUpDetails.PACoverToOwnDriver = Convert.ToDouble(BOValue);
        //                                }
        //                            }
        //                            double netpremium = resModel.PremiumBreakUpDetails.NetODPremium + resModel.PremiumBreakUpDetails.NetTPPremium - resModel.PremiumBreakUpDetails.NetAddonPremium;
        //                            double tax = Math.Round(netpremium * 0.18, 0);
        //                            resModel.FinalPremium = Convert.ToInt32(netpremium + tax);
        //                            //objdata.odprem = ODPremium.ToString();
        //                            //objdata.tpprem = TPPremium.ToString();
        //                            //objdata.totalprem = Math.Round(netpremium, 0).ToString();
        //                        }
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                resModel.Status = Status.Fail;
        //                resModel.ErrorMsg = Convert.ToString(res.XPathSelectElement("//Policy/ErrorMessage"));
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            resModel.Status = Status.Fail;
        //            resModel.ErrorMsg = Convert.ToString(ex.Message);
        //            Console.Write(Convert.ToString(ex.Message));
        //            LogU.WriteLog("FGI >> PrivateCar >> GetQuoteResponse >> " + Convert.ToString(ex.Message));
        //            throw;
        //        }
        //        resModel.CompanyName = Company.FGI.ToString();
        //        resModel.Product = Product.Motor;
        //        resModel.SubProduct = SubProduct.PrivateCar;
        //        resModel.PlanName = "Future Generali Comprehesive Plan";
        //        return resModel;
        //    }
        //    else
        //    {
        //        resModel.Status = Status.Fail;
        //        resModel.ErrorMsg = Convert.ToString(res.XPathSelectElement("//Policy/ErrorMessage"));
        //    }

        //    return resModel;
        //}

        /// <summary>
        /// Getting payment parameter details.
        /// </summary>
        /// <returns>string type return.</returns>
        public string GetPaymentParameter(PaymentRequest reqModel)
        {
            string pay = string.Empty;
            FGPaymentRequest req = new FGPaymentRequest();
            DefaultController de = new DefaultController();
            try
            {
                req.Email = reqModel.EmailId;
                req.TransactionID = de.GenerateEnquiryId();
                req.PaymentUrl = "http://fglpg001.futuregenerali.in/Ecom_NL/WEBAPPLN/UI/Common/WebAggPay.aspx";
                req.PaymentOption = 1;
                req.ResponseURL = string.Empty;
                //req.ProposalNumber = reqModel.ProposalNo;
                req.PremiumAmount = reqModel.FinalPremium;
                req.UserIdentifier = "NA";
                req.UserId = string.Empty;
                req.FirstName = reqModel.FirstName;
                req.LastName = reqModel.LastName;
                req.Mobile = reqModel.MobileNo;
                pay = new JavaScriptSerializer().Serialize(req);
            }
            catch (Exception ex)
            {
                Console.Write(Convert.ToString(ex.Message));
                LogU.WriteLog("FGI >> PrivateCar >> GetPaymentParameter >> " + Convert.ToString(ex.Message));
                throw;
            }
            return pay;
        }

        /// <summary>
        /// calculate age based on dob.
        /// </summary>
        /// <param name="dateOfBirth">pass date of birth.</param>
        /// <returns>age</returns>
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
                LogU.WriteLog("FGI >> PrivateCar >> CalculateAge >> " + Convert.ToString(ex.Message));
                Console.Write(Convert.ToString(ex.Message));
                throw;
            }
            return age;
        }

        public string GetMaritial(string mari)
        {
            string m = string.Empty;
            try
            {
                switch (mari)
                {
                    case "Divorced":
                        m = "D";
                        break;
                    case "Married":
                        m = "M";
                        break;
                    case "Single":
                        m = "S";
                        break;
                    case "Widow":
                        m = "W";
                        break;
                }
            }
            catch (Exception)
            {

            }
            return m;
        }

        public string GenerateRandomNo()
        {
            string random = string.Empty;
            random = DateTime.Now.Ticks.ToString().Substring(0, 10);
            return random;
        }

        //public Response GetQuoteResponse(XDocument res)
        //{
        //    Response resModel = new Response();
        //    DataSet ds = new DataSet();
        //    resModel.PremiumBreakUpDetails = new PremiumBreakUpDetails();
        //    resModel.CompanyWiseRefference = new CompanyWiseRefference();
        //    try
        //    {
        //        string status = res.XPathSelectElement("//Status").Value;
        //        if ((!string.IsNullOrEmpty(status)) && status.Equals("Successful", StringComparison.OrdinalIgnoreCase))
        //        {
        //            resModel.IDV = Convert.ToInt32(res.XPathSelectElement("//VehicleIDV").Value);
        //            ds.ReadXml(new XmlTextReader(new StringReader(Convert.ToString(res))));
        //            List<XElement> xElementList = res.Descendants("Table1").ToList();
        //            resModel.Status = Status.Success;
        //            if (ds.Tables.Count > 0)
        //            {
        //                if (ds.Tables.Contains("Policy"))
        //                {
        //                    DataTable dt_Policy = ds.Tables["Policy"];
        //                    DataTable dt_Table = ds.Tables["Table"];
        //                    DataTable dt_Table1 = ds.Tables["Table1"];

        //                    if (Convert.ToString(dt_Policy.Rows[0]["Status"].ToString()) == "Successful")
        //                    {
        //                        if (dt_Table.Rows.Count > 0)
        //                        {
        //                            resModel.PolicyNo = Convert.ToString(dt_Table.Rows[0]["PolNo"].ToString());

        //                        }

        //                        for (int i = 0; i < dt_Table1.Rows.Count; i++)
        //                        {
        //                            //AddonDetails objent = new AddonDetails();

        //                            string code = Convert.ToString(dt_Table1.Rows[i]["Code"].ToString()).Trim();
        //                            string description = Convert.ToString(dt_Table1.Rows[i]["Description"].ToString()).Trim();
        //                            string type = Convert.ToString(dt_Table1.Rows[i]["Type"].ToString()).Trim();
        //                            string BOValue = Convert.ToString(dt_Table1.Rows[i]["BOValue"].ToString()).Trim();
        //                            string DBValue = Convert.ToString(dt_Table1.Rows[i]["DBValue"].ToString()).Trim();


        //                            if (code == "Gross Premium")
        //                            {
        //                                if (type == "OD")
        //                                {
        //                                    resModel.PremiumBreakUpDetails.BasicODPremium = Math.Round(Convert.ToDouble(BOValue), 2);
        //                                }
        //                                else if (type == "TP")
        //                                {
        //                                    resModel.PremiumBreakUpDetails.BasicThirdPartyLiability = Math.Round(Convert.ToDouble(BOValue), 2);
        //                                }
        //                            }
        //                            if (code == "CNG")
        //                            {
        //                                if (type == "OD")
        //                                {
        //                                    resModel.PremiumBreakUpDetails.CNGLPGKitPremium = Math.Round(Convert.ToDouble(BOValue), 2);
        //                                }
        //                                else if (type == "TP")
        //                                {
        //                                    resModel.PremiumBreakUpDetails.TPCNGLPGPremium = Math.Round(Convert.ToDouble(BOValue), 2);
        //                                }
        //                            }
        //                            else if (code == "NCB")
        //                            {
        //                                if (type == "OD")
        //                                {
        //                                    resModel.PremiumBreakUpDetails.NCBDiscount += Math.Round(Convert.ToDouble(BOValue));
        //                                }
        //                                else if (type == "TP")
        //                                {
        //                                    resModel.PremiumBreakUpDetails.NCBDiscount += Math.Round(Convert.ToDouble(BOValue));
        //                                }
        //                            }
        //                            else if (code == "APA")
        //                            {
        //                                if (type == "OD")
        //                                {
        //                                    resModel.PremiumBreakUpDetails.PACoverToUnNamedPerson += Math.Round(Convert.ToDouble(BOValue), 2);
        //                                }
        //                                else if (type == "TP")
        //                                {
        //                                    resModel.PremiumBreakUpDetails.PACoverToUnNamedPerson += Math.Round(Convert.ToDouble(BOValue), 2);
        //                                }
        //                            }
        //                            else if (code == "LLDE")
        //                            {

        //                                if (type == "TP")
        //                                {
        //                                    resModel.PremiumBreakUpDetails.LLToPaidDriver = Math.Round(Convert.ToDouble(BOValue), 2);
        //                                }
        //                            }
        //                            else if (code == "LLOE")
        //                            {

        //                                if (type == "TP")
        //                                {
        //                                    resModel.PremiumBreakUpDetails.LLToPaidEmployee = Math.Round(Convert.ToDouble(BOValue), 2);
        //                                }
        //                            }
        //                            else if (code == "LOADDISC")
        //                            {

        //                                if (type == "OD")
        //                                {
        //                                    string dis = Convert.ToString(BOValue).Substring(2);
        //                                    resModel.PremiumBreakUpDetails.OtherDiscount += Math.Round(Convert.ToDouble(dis), 2);
        //                                }
        //                                else if (type == "TP")
        //                                {
        //                                    resModel.PremiumBreakUpDetails.OtherDiscount += Math.Round(Convert.ToDouble(BOValue), 2);
        //                                }
        //                            }
        //                            //else if (code == "IDV")
        //                            //{
        //                            //    if (description == "IDV" && type == "OD")
        //                            //    {
        //                            //        resModel.IDV += Convert.ToDouble(BOValue);
        //                            //    }
        //                            //    else if (description == "IDV" && type == "TP")
        //                            //    {
        //                            //        resModel.IDV += Convert.ToDouble(BOValue);
        //                            //    }
        //                            //}
        //                            else if (code == "ServTax")
        //                            {

        //                                if (type == "OD")
        //                                {
        //                                    resModel.PremiumBreakUpDetails.ServiceTax += Math.Round(Convert.ToDouble(BOValue), 2);
        //                                }
        //                                else if (type == "TP")
        //                                {
        //                                    resModel.PremiumBreakUpDetails.ServiceTax += Math.Round(Convert.ToDouble(BOValue), 2);
        //                                }

        //                            }
        //                            //else if (code == "PrmDue")
        //                            //{

        //                            //    if (type == "OD")
        //                            //    {
        //                            //        //resModel.PremiumBreakUpDetails.NetPremium += Math.Round(Convert.ToDouble(BOValue));
        //                            //    }
        //                            //    else if (type == "TP")
        //                            //    {
        //                            //          //resModel.PremiumBreakUpDetails.NetPremium += Math.Round(Convert.ToDouble(BOValue));
        //                            //    }

        //                            //}
        //                            else if (code == "MOTADON" && type == "OD")
        //                            {
        //                                resModel.PremiumBreakUpDetails.NetAddonPremium = Math.Round(Convert.ToDouble(BOValue), 2);
        //                            }
        //                            else if (code == "00001")                      //tyre
        //                            {
        //                                //objent.Code = code;
        //                                //objent.Type = type;
        //                                //objent.Description = description;
        //                                //objent.Premium = BOValue;
        //                                resModel.PremiumBreakUpDetails.TyreProtect = Math.Round(Convert.ToDouble(BOValue), 2);
        //                                //resModel.PremiumBreakUpDetails.NetAddonPremium += Convert.ToDouble(BOValue);
        //                                //listent.Add(objent);
        //                            }
        //                            else if (code == "00002")                //INconvenience
        //                            {
        //                                //objent.Code = code;
        //                                //objent.Type = type;
        //                                //objent.Description = description;
        //                                //objent.Premium = BOValue;
        //                                resModel.PremiumBreakUpDetails.InvoicePriceCoverPremium = Math.Round(Convert.ToDouble(BOValue), 2);
        //                                //resModel.PremiumBreakUpDetails.NetAddonPremium += Convert.ToDouble(BOValue);
        //                                //listent.Add(objent);
        //                            }
        //                            else if (code == "00004")        // ncb
        //                            {
        //                                //objent.Code = code;
        //                                //objent.Type = type;
        //                                //objent.Description = description;
        //                                //objent.Premium = BOValue;
        //                                resModel.PremiumBreakUpDetails.NcbProtectorPremium = Math.Round(Convert.ToDouble(BOValue), 2);
        //                                //resModel.PremiumBreakUpDetails.NetAddonPremium += Convert.ToDouble(BOValue);
        //                                //listent.Add(objent);
        //                            }
        //                            else if (code == "00005")      //consumables
        //                            {
        //                                //objent.Code = code;
        //                                //objent.Type = type;
        //                                //objent.Description = description;
        //                                //objent.Premium = BOValue;
        //                                resModel.PremiumBreakUpDetails.CostOfConsumablesPremium = Math.Round(Convert.ToDouble(BOValue), 2);
        //                                //resModel.PremiumBreakUpDetails.NetAddonPremium += Convert.ToDouble(BOValue);
        //                                //listent.Add(objent);
        //                            }
        //                            else if (code == "00006")        //Invoice
        //                            {
        //                                //objent.Code = code;
        //                                //objent.Type = type;
        //                                //objent.Description = description;
        //                                //objent.Premium = BOValue;
        //                                resModel.PremiumBreakUpDetails.InvoicePriceCoverPremium = Math.Round(Convert.ToDouble(BOValue), 2);
        //                                //resModel.PremiumBreakUpDetails.NetAddonPremium += Convert.ToDouble(BOValue);
        //                                //listent.Add(objent);
        //                            }
        //                            else if (code == "PLAN1")
        //                            {
        //                                //objent.Code = code;
        //                                //objent.Type = type;
        //                                //objent.Description = description;
        //                                //objent.Premium = BOValue;
        //                                resModel.PremiumBreakUpDetails.ZeroDepPremium = Math.Round(Convert.ToDouble(BOValue), 2);
        //                                //listent.Add(objent);
        //                            }

        //                            else if (code == "ENGPR")
        //                            {
        //                                //objent.Code = code;
        //                                //objent.Type = type;
        //                                //objent.Description = description;
        //                                //objent.Premium = BOValue;
        //                                resModel.PremiumBreakUpDetails.EngineProtectorPremium = Math.Round(Convert.ToDouble(BOValue), 2);
        //                                //resModel.PremiumBreakUpDetails.NetAddonPremium += Convert.ToDouble(BOValue);
        //                                //listent.Add(objent);
        //                            }

        //                            else if (code == "CPA")
        //                            {
        //                                resModel.PremiumBreakUpDetails.PACoverToOwnDriver = Math.Round(Convert.ToDouble(BOValue), 2);
        //                            }
        //                        }
        //                        double PAServiceTax = resModel.PremiumBreakUpDetails.PACoverToOwnDriver > 0 ? resModel.PremiumBreakUpDetails.PACoverToOwnDriver * 0.18 : 0;
        //                        resModel.PremiumBreakUpDetails.PACoverToOwnDriver = resModel.PremiumBreakUpDetails.PACoverToOwnDriver;
        //                        resModel.PremiumBreakUpDetails.NetPremium = Math.Round(resModel.PremiumBreakUpDetails.BasicODPremium + resModel.PremiumBreakUpDetails.PACoverToOwnDriver + resModel.PremiumBreakUpDetails.BasicThirdPartyLiability - resModel.PremiumBreakUpDetails.NCBDiscount);
        //                        resModel.FinalPremium = Math.Round(resModel.PremiumBreakUpDetails.NetPremium);
        //                        //objdata.odprem = ODPremium.ToString();
        //                        //objdata.tpprem = TPPremium.ToString();
        //                        //objdata.totalprem = Math.Round(netpremium, 0).ToString();
        //                    }
        //                }
        //            }
        //        }
        //        else
        //        {
        //            resModel.Status = Status.Fail;
        //            resModel.ErrorMsg = Convert.ToString(res.XPathSelectElement("//Policy/ErrorMessage"));
        //            if (!string.IsNullOrEmpty(Convert.ToString(res.XPathSelectElement("//ValidationError"))))
        //            {
        //                resModel.ErrorMsg = Convert.ToString(res.XPathSelectElement("//ValidationError"));
        //            }
        //            else
        //            {
        //                resModel.ErrorMsg = Convert.ToString(res.XPathSelectElement("//Policy/ErrorMessage"));
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        resModel.Status = Status.Fail;
        //        resModel.ErrorMsg = Convert.ToString(ex.Message);
        //        Console.Write(Convert.ToString(ex.Message));
        //        LogU.WriteLog("FGI >> PrivateCar >> GetQuoteResponse >> " + Convert.ToString(ex.Message));
        //    }

        //    return resModel;
        //}

        //public Response GetProposalResponse(XDocument res)
        //{
        //    Response resModel = new Response();
        //    DataSet ds = new DataSet();
        //    resModel.PremiumBreakUpDetails = new PremiumBreakUpDetails();
        //    resModel.CompanyWiseRefference = new CompanyWiseRefference();
        //    try
        //    {
        //        string status = res.XPathSelectElement("//Status").Value;
        //        if ((!string.IsNullOrEmpty(status)) && status.Equals("Successful", StringComparison.OrdinalIgnoreCase))
        //        {
        //            resModel.IDV = Convert.ToInt32(res.XPathSelectElement("//VehicleIDV").Value);
        //            ds.ReadXml(new XmlTextReader(new StringReader(Convert.ToString(res))));
        //            List<XElement> xElementList = res.Descendants("Table1").ToList();
        //            resModel.Status = Status.Success;
        //            if (ds.Tables.Count > 0)
        //            {
        //                if (ds.Tables.Contains("Policy"))
        //                {
        //                    DataTable dt_Policy = ds.Tables["Policy"];
        //                    DataTable dt_Table = ds.Tables["Table"];
        //                    DataTable dt_Table1 = ds.Tables["Table1"];

        //                    if (Convert.ToString(dt_Policy.Rows[0]["Status"].ToString()) == "Successful")
        //                    {
        //                        if (dt_Table.Rows.Count > 0)
        //                        {
        //                            resModel.PolicyNo = Convert.ToString(dt_Table.Rows[0]["PolNo"].ToString());

        //                        }

        //                        for (int i = 0; i < dt_Table1.Rows.Count; i++)
        //                        {
        //                            //AddonDetails objent = new AddonDetails();

        //                            string code = Convert.ToString(dt_Table1.Rows[i]["Code"].ToString()).Trim();
        //                            string description = Convert.ToString(dt_Table1.Rows[i]["Description"].ToString()).Trim();
        //                            string type = Convert.ToString(dt_Table1.Rows[i]["Type"].ToString()).Trim();
        //                            string BOValue = Convert.ToString(dt_Table1.Rows[i]["BOValue"].ToString()).Trim();
        //                            string DBValue = Convert.ToString(dt_Table1.Rows[i]["DBValue"].ToString()).Trim();


        //                            if (code == "Gross Premium")
        //                            {
        //                                if (type == "OD")
        //                                {
        //                                    resModel.PremiumBreakUpDetails.BasicODPremium = Math.Round(Convert.ToDouble(BOValue), 2);
        //                                }
        //                                else if (type == "TP")
        //                                {
        //                                    resModel.PremiumBreakUpDetails.BasicThirdPartyLiability = Math.Round(Convert.ToDouble(BOValue), 2);
        //                                }
        //                            }
        //                            else if (code == "DISCPERC")
        //                            {

        //                                if (type == "OD")
        //                                {
        //                                    string dis = Convert.ToString(BOValue).Substring(1);
        //                                    resModel.CompanyWiseRefference.LoadingDiscount += Math.Round(Convert.ToDouble(dis), 2);
        //                                }
        //                                else if (type == "TP")
        //                                {
        //                                    resModel.CompanyWiseRefference.LoadingDiscount += Math.Round(Convert.ToDouble(BOValue), 2);
        //                                }
        //                            }
        //                            if (code == "CNG")
        //                            {
        //                                if (type == "OD")
        //                                {
        //                                    resModel.PremiumBreakUpDetails.CNGLPGKitPremium = Math.Round(Convert.ToDouble(BOValue), 2);
        //                                }
        //                                else if (type == "TP")
        //                                {
        //                                    resModel.PremiumBreakUpDetails.TPCNGLPGPremium = Math.Round(Convert.ToDouble(BOValue), 2);
        //                                }
        //                            }
        //                            else if (code == "NCB")
        //                            {
        //                                if (type == "OD")
        //                                {
        //                                    resModel.PremiumBreakUpDetails.NCBDiscount += Math.Round(Convert.ToDouble(BOValue));
        //                                }
        //                                else if (type == "TP")
        //                                {
        //                                    resModel.PremiumBreakUpDetails.NCBDiscount += Math.Round(Convert.ToDouble(BOValue));
        //                                }
        //                            }
        //                            else if (code == "APA")
        //                            {
        //                                if (type == "OD")
        //                                {
        //                                    resModel.PremiumBreakUpDetails.PACoverToUnNamedPerson += Math.Round(Convert.ToDouble(BOValue), 2);
        //                                }
        //                                else if (type == "TP")
        //                                {
        //                                    resModel.PremiumBreakUpDetails.PACoverToUnNamedPerson += Math.Round(Convert.ToDouble(BOValue), 2);
        //                                }
        //                            }
        //                            else if (code == "LLDE")
        //                            {

        //                                if (type == "TP")
        //                                {
        //                                    resModel.PremiumBreakUpDetails.LLToPaidDriver = Math.Round(Convert.ToDouble(BOValue), 2);
        //                                }
        //                            }
        //                            else if (code == "LLOE")
        //                            {

        //                                if (type == "TP")
        //                                {
        //                                    resModel.PremiumBreakUpDetails.LLToPaidEmployee = Math.Round(Convert.ToDouble(BOValue), 2);
        //                                }
        //                            }
        //                            else if (code == "LOADDISC")
        //                            {

        //                                if (type == "OD")
        //                                {
        //                                    string dis = Convert.ToString(BOValue).Substring(2);
        //                                    resModel.PremiumBreakUpDetails.OtherDiscount += Math.Round(Convert.ToDouble(dis), 2);
        //                                }
        //                                else if (type == "TP")
        //                                {
        //                                    resModel.PremiumBreakUpDetails.OtherDiscount += Math.Round(Convert.ToDouble(BOValue), 2);
        //                                }
        //                            }
        //                            //else if (code == "IDV")
        //                            //{
        //                            //    if (description == "IDV" && type == "OD")
        //                            //    {
        //                            //        resModel.IDV += Convert.ToDouble(BOValue);
        //                            //    }
        //                            //    else if (description == "IDV" && type == "TP")
        //                            //    {
        //                            //        resModel.IDV += Convert.ToDouble(BOValue);
        //                            //    }
        //                            //}
        //                            else if (code == "ServTax")
        //                            {

        //                                if (type == "OD")
        //                                {
        //                                    resModel.PremiumBreakUpDetails.ServiceTax += Math.Round(Convert.ToDouble(BOValue), 2);
        //                                }
        //                                else if (type == "TP")
        //                                {
        //                                    resModel.PremiumBreakUpDetails.ServiceTax += Math.Round(Convert.ToDouble(BOValue), 2);
        //                                }

        //                            }
        //                            else if (code == "PrmDue")
        //                            {

        //                                if (type == "OD")
        //                                {
        //                                    //resModel.PremiumBreakUpDetails.BasicODPremium += Math.Round(Convert.ToDouble(BOValue));
        //                                }
        //                                else if (type == "TP")
        //                                {
        //                                    //  resModel.PremiumBreakUpDetails.BasicThirdPartyLiability += Math.Round(Convert.ToDouble(BOValue));
        //                                }

        //                            }
        //                            else if (code == "MOTADON" && type == "OD")
        //                            {
        //                                resModel.PremiumBreakUpDetails.NetAddonPremium = Math.Round(Convert.ToDouble(BOValue), 2);
        //                            }
        //                            else if (code == "00001")                      //tyre
        //                            {
        //                                //objent.Code = code;
        //                                //objent.Type = type;
        //                                //objent.Description = description;
        //                                //objent.Premium = BOValue;
        //                                resModel.PremiumBreakUpDetails.TyreProtect = Math.Round(Convert.ToDouble(BOValue), 2);
        //                                //resModel.PremiumBreakUpDetails.NetAddonPremium += Convert.ToDouble(BOValue);
        //                                //listent.Add(objent);
        //                            }
        //                            else if (code == "00002")                //INconvenience
        //                            {
        //                                //objent.Code = code;
        //                                //objent.Type = type;
        //                                //objent.Description = description;
        //                                //objent.Premium = BOValue;
        //                                resModel.PremiumBreakUpDetails.InvoicePriceCoverPremium = Math.Round(Convert.ToDouble(BOValue), 2);
        //                                //resModel.PremiumBreakUpDetails.NetAddonPremium += Convert.ToDouble(BOValue);
        //                                //listent.Add(objent);
        //                            }
        //                            else if (code == "00004")        // ncb
        //                            {
        //                                //objent.Code = code;
        //                                //objent.Type = type;
        //                                //objent.Description = description;
        //                                //objent.Premium = BOValue;
        //                                resModel.PremiumBreakUpDetails.NcbProtectorPremium = Math.Round(Convert.ToDouble(BOValue), 2);
        //                                //resModel.PremiumBreakUpDetails.NetAddonPremium += Convert.ToDouble(BOValue);
        //                                //listent.Add(objent);
        //                            }
        //                            else if (code == "00005")      //consumables
        //                            {
        //                                //objent.Code = code;
        //                                //objent.Type = type;
        //                                //objent.Description = description;
        //                                //objent.Premium = BOValue;
        //                                resModel.PremiumBreakUpDetails.CostOfConsumablesPremium = Math.Round(Convert.ToDouble(BOValue), 2);
        //                                //resModel.PremiumBreakUpDetails.NetAddonPremium += Convert.ToDouble(BOValue);
        //                                //listent.Add(objent);
        //                            }
        //                            else if (code == "00006")        //Invoice
        //                            {
        //                                //objent.Code = code;
        //                                //objent.Type = type;
        //                                //objent.Description = description;
        //                                //objent.Premium = BOValue;
        //                                resModel.PremiumBreakUpDetails.InvoicePriceCoverPremium = Math.Round(Convert.ToDouble(BOValue), 2);
        //                                //resModel.PremiumBreakUpDetails.NetAddonPremium += Convert.ToDouble(BOValue);
        //                                //listent.Add(objent);
        //                            }
        //                            else if (code == "PLAN1")
        //                            {
        //                                //objent.Code = code;
        //                                //objent.Type = type;
        //                                //objent.Description = description;
        //                                //objent.Premium = BOValue;
        //                                resModel.PremiumBreakUpDetails.ZeroDepPremium = Math.Round(Convert.ToDouble(BOValue), 2);
        //                                //listent.Add(objent);
        //                            }

        //                            else if (code == "ENGPR")
        //                            {
        //                                //objent.Code = code;
        //                                //objent.Type = type;
        //                                //objent.Description = description;
        //                                //objent.Premium = BOValue;
        //                                resModel.PremiumBreakUpDetails.EngineProtectorPremium = Math.Round(Convert.ToDouble(BOValue), 2);
        //                                //resModel.PremiumBreakUpDetails.NetAddonPremium += Convert.ToDouble(BOValue);
        //                                //listent.Add(objent);
        //                            }

        //                            else if (code == "CPA")
        //                            {
        //                                resModel.PremiumBreakUpDetails.PACoverToOwnDriver = Math.Round(Convert.ToDouble(BOValue), 2);
        //                            }
        //                        }

        //                        resModel.PremiumBreakUpDetails.NetPremium = Math.Round(resModel.PremiumBreakUpDetails.BasicODPremium + resModel.PremiumBreakUpDetails.PACoverToOwnDriver + resModel.PremiumBreakUpDetails.BasicThirdPartyLiability);
        //                        //resModel.PremiumBreakUpDetails.NetPremium = Math.Round(resModel.PremiumBreakUpDetails.NetODPremium + resModel.PremiumBreakUpDetails.NetTPPremium - resModel.PremiumBreakUpDetails.NetAddonPremium);
        //                        resModel.PremiumBreakUpDetails.ServiceTax = Math.Round(resModel.PremiumBreakUpDetails.NetPremium * 18 / 100);
        //                        resModel.FinalPremium = Math.Round(resModel.PremiumBreakUpDetails.NetPremium + resModel.PremiumBreakUpDetails.ServiceTax);
        //                        //objdata.odprem = ODPremium.ToString();
        //                        //objdata.tpprem = TPPremium.ToString();
        //                        //objdata.totalprem = Math.Round(netpremium, 0).ToString();
        //                    }
        //                }
        //            }
        //        }
        //        else
        //        {
        //            resModel.Status = Status.Fail;
        //            resModel.ErrorMsg = Convert.ToString(res.XPathSelectElement("//Policy/ErrorMessage"));
        //            if (!string.IsNullOrEmpty(Convert.ToString(res.XPathSelectElement("//ValidationError"))))
        //            {
        //                resModel.ErrorMsg = Convert.ToString(res.XPathSelectElement("//ValidationError"));
        //            }
        //            else
        //            {
        //                resModel.ErrorMsg = Convert.ToString(res.XPathSelectElement("//Policy/ErrorMessage"));
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        resModel.Status = Status.Fail;
        //        resModel.ErrorMsg = Convert.ToString(ex.Message);
        //        Console.Write(Convert.ToString(ex.Message));
        //        LogU.WriteLog("FGI >> PrivateCar >> GetProposalResponse >> " + Convert.ToString(ex.Message));
        //    }

        //    return resModel;
        //}

        public string SavePolicyPdf(string policyno)
        {
            string path = string.Empty;
            if (string.IsNullOrEmpty(policyno))
            {
            }
            else
            {
                path = AppDomain.CurrentDomain.BaseDirectory + "PolicyPdf";
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
            }
            return path;
        }



    }
}