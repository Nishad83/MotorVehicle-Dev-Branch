
namespace AndWebApi.RELIANCE
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
    using AndApp;
    using Controllers;
    using System.Web.Script.Serialization;
    using AndApp.Utilities;
    using System.Net;
    using System.Text;
    using Newtonsoft.Json.Linq;
    #endregion


    public class PrivateCar
    {
        /// <summary>
        /// Quotes request method.
        /// </summary>
        /// <param name="model">Object of quotation model.</param>
        /// <returns>return response type object.</returns>

        #region GetQuoteRequest - Response
        public Response GetQuoteRequest(Quotation model)
        {
            Response resModel = new Response();
            XmlDocument doc = new XmlDocument();
            try
            {
                string path = AppDomain.CurrentDomain.BaseDirectory;
                string filePath = Path.Combine(path, "XML/RELIANCE/Quote.xml");
                var document = XDocument.Load(filePath);

                #region  ClientDetails
                document.XPathSelectElement("//ClientType").Value = string.IsNullOrEmpty(model.CustomerType) ? "0" : (model.CustomerType);
                #endregion


                #region Policy
                document.XPathSelectElement("//BusinessType").Value = model.PolicyType;
                document.XPathSelectElement("//Cover_From").Value = model.PolicyStartDate;
                document.XPathSelectElement("//Cover_To").Value = model.PolicyEndDate;
                #endregion


                #region Risk
                document.XPathSelectElement("//VehicleMakeID").Value = model.VehicleDetails != null ? model.VehicleDetails.MakeCode : string.Empty;
                document.XPathSelectElement("//VehicleModelID").Value = model.VehicleDetails != null ? model.VehicleDetails.ModelCode : string.Empty;
                document.XPathSelectElement("//CubicCapacity").Value = model.VehicleDetails != null ? ((string.IsNullOrEmpty(Convert.ToString(model.VehicleDetails.CC))) ? "0" : Convert.ToString(model.VehicleDetails.CC)) : "0";
                document.XPathSelectElement("//RTOLocationID").Value = model.VehicleDetails != null ? ((string.IsNullOrEmpty(Convert.ToString(model.VehicleDetails.RtoId))) ? "0" : Convert.ToString(model.VehicleDetails.RtoId)) : "0";

              
                string purchaseDate = Convert.ToDateTime(model.VehicleDetails.PurchaseDate).ToString("dd/MM/yyyy");
                document.XPathSelectElement("//IDV").Value = model.VehicleDetails != null ? ((string.IsNullOrEmpty(Convert.ToString(model.IDV))) ? "0" : Convert.ToString(model.IDV)) : "0"; ;
                document.XPathSelectElement("//DateOfPurchase").Value = purchaseDate;

                int ManufactureMonth = Convert.ToDateTime(model.VehicleDetails.ManufaturingDate).Month;
                int ManufactureYear = Convert.ToDateTime(model.VehicleDetails.ManufaturingDate).Year;
                document.XPathSelectElement("//ManufactureMonth").Value = ManufactureMonth.ToString();
                document.XPathSelectElement("//ManufactureYear").Value = ManufactureYear.ToString();

                document.XPathSelectElement("//VehicleVariant").Value = model.VehicleDetails != null ? model.VehicleDetails.VariantName : string.Empty;
                document.XPathSelectElement("//StateOfRegistrationID").Value = model.VehicleAddressDetails != null ? model.VehicleAddressDetails.State.ToString() : string.Empty; ;
                document.XPathSelectElement("//Rto_RegionCode").Value = model.VehicleDetails != null ? model.VehicleDetails.RtoZone : string.Empty;
                #endregion


                #region Vehicle
                string regisrationDate = Convert.ToDateTime(model.VehicleDetails.RegistrationDate).ToString("dd/MM/yyyy");
                document.XPathSelectElement("//Registration_Number").Value = model.VehicleDetails != null ? model.VehicleDetails.RegistrationNumber : string.Empty;
                document.XPathSelectElement("//Registration_date").Value = regisrationDate;
                document.XPathSelectElement("//SeatingCapacity").Value = model.VehicleDetails != null ? ((string.IsNullOrEmpty(Convert.ToString(model.VehicleDetails.SC))) ? "0" : Convert.ToString(model.VehicleDetails.SC)) : "0";
                document.XPathSelectElement("//TypeOfFuel").Value = model.VehicleDetails != null ? ((string.IsNullOrEmpty(Convert.ToString(model.VehicleDetails.Fuel))) ? "1" : Convert.ToString(model.VehicleDetails.Fuel)) : "1";
                #endregion


                #region Cover
                document.XPathSelectElement("//IsElectricalItemFitted").Value = model.CoverageDetails.IsElectricalAccessories.ToString();
                document.XPathSelectElement("//ElectricalItemsTotalSI").Value = model.CoverageDetails.ElectricalAccessoriesDetails[0].Amount.ToString();

                document.XPathSelectElement("//IsNonElectricalItemFitted").Value = model.CoverageDetails.IsNonElectricalAccessories.ToString();
                document.XPathSelectElement("//ElectricalItemsTotalSI").Value = model.CoverageDetails.NonElectricalAccessoriesDetails[0].Amount.ToString();

                document.XPathSelectElement("//IsBiFuelKit").Value = model.CoverageDetails.IsBiFuelKit.ToString();
                document.XPathSelectElement("//Fueltype").Value = "";//Take into Model
                document.XPathSelectElement("//BifuelKit/BifuelKit/ISLpgCng").Value = model.CoverageDetails.IsBiFuelKit.ToString();
                document.XPathSelectElement("//BifuelKit/BifuelKit/SumInsured").Value = model.CoverageDetails.BiFuelKitAmount.ToString();

                document.XPathSelectElement("//IsAutomobileAssociationMember").Value = model.DiscountDetails.IsMemberOfAutomobileAssociation.ToString();
                document.XPathSelectElement("//AutomobileAssociationExpiryDate").Value = model.DiscountDetails != null ? ((string.IsNullOrEmpty(Convert.ToString(model.DiscountDetails.AutomobileAssociationMemberExpiryDate))) ? "" : Convert.ToString(model.DiscountDetails.AutomobileAssociationMemberExpiryDate)) : "";

                document.XPathSelectElement("//IsVoluntaryDeductableOpted").Value = model.DiscountDetails.IsVoluntaryExcess.ToString();
                document.XPathSelectElement("//VoluntaryDeductible/VoluntaryDeductible/SumInsured").Value = model.DiscountDetails.VoluntaryExcessAmount.ToString();
                document.XPathSelectElement("//IsAntiTheftDeviceFitted").Value = model.DiscountDetails.IsAntiTheftDevice.ToString();

                #region Applicable Rate
                int cyear = DateTime.Now.Year;
                int cday = DateTime.Now.Day;
                int vage = cyear - ManufactureYear;
                string ApplicableRate = "";
                if (vage == 1 || vage == 2)
                {
                    ApplicableRate = "0.45";
                }
                else if (vage == 3)
                {
                    ApplicableRate = "0.55";
                }
                else if (vage == 3)
                {
                    ApplicableRate = "0.70";
                }
                else if (vage >= 4 || vage <= 15)
                {
                    ApplicableRate = "0.85";
                }

                document.XPathSelectElement("//NilDepreciationCoverage/NilDepreciationCoverage/ApplicableRate").Value = ApplicableRate;

                #endregion

                document.XPathSelectElement("//IsPAToNamedPassenger").Value = model.CoverageDetails.IsPACoverForNamedPersons.ToString();
                document.XPathSelectElement("//PAToNamedPassenger/PAToNamedPassenger/NoOfItems").Value = model.VehicleDetails != null ? ((string.IsNullOrEmpty(Convert.ToString(model.VehicleDetails.SC))) ? "0" : Convert.ToString(model.VehicleDetails.SC)) : "0";
                document.XPathSelectElement("//PAToNamedPassenger/PAToNamedPassenger/SumInsured").Value = model.CoverageDetails.CapitalSumInsuredPerPersonNamed.ToString();


                document.XPathSelectElement("//IsPAToUnnamedPassengerCovered").Value = model.CoverageDetails.IsPACoverUnnamedPerson.ToString();
                document.XPathSelectElement("//PAToUnNamedPassenger/PAToUnNamedPassenger/NoOfItems").Value = model.VehicleDetails != null ? ((string.IsNullOrEmpty(Convert.ToString(model.VehicleDetails.SC))) ? "0" : Convert.ToString(model.VehicleDetails.SC)) : "0";
                document.XPathSelectElement("//PAToUnNamedPassenger/PAToUnNamedPassenger/SumInsured").Value = model.CoverageDetails.PACoverUnnamedPersonAmount.ToString();


                document.XPathSelectElement("//IsPAToDriverCovered").Value = model.CoverageDetails.IsPACoverPaidDriver.ToString();
                document.XPathSelectElement("//PAToPaidDriver/PAToPaidDriver/NoOfItems").Value = "1";
                document.XPathSelectElement("//PAToPaidDriver/PAToPaidDriver/SumInsured").Value = model.CoverageDetails.PACoverPaidDriverAmount.ToString();


                #endregion


                document.XPathSelectElement("//PrevYearInsurer").Value = model.PreviousPolicyDetails.CompanyId.ToString();
                document.XPathSelectElement("//PrevYearPolicyNo").Value = model.PreviousPolicyDetails.PreviousPolicyNo.ToString();
                document.XPathSelectElement("//PrevYearPolicyStartDate").Value = model.PreviousPolicyDetails.PreviousPolicyStartDate;
                document.XPathSelectElement("//PrevYearPolicyEndDate").Value = model.PreviousPolicyDetails.PreviousPolicyEndDate;

                document.XPathSelectElement("//NCBEligibility/PreviousNCB").Value = model.PreviousPolicyDetails.PreviousNcbPercentage.ToString();
                document.XPathSelectElement("//NCBEligibility/CurrentNCB").Value = model.CurrentNcb.ToString();

                var response = RelianceResponse(document.ToString(), "https://rgipartners.reliancegeneral.co.in/API/Service/PremiumCalulationForMotor");
                resModel = GetQuoteResponse(response);
            }
            catch (Exception ex)
            {
                resModel.Status = Status.Fail;
                resModel.ErrorMsg = Convert.ToString(ex.Message);
                Console.Write(Convert.ToString(ex.Message));
                LogU.WriteLog("RELIANCE >> PrivateCar >> GetQuoteRequest >> " + Convert.ToString(ex.Message));
                throw;
            }
            return resModel;
        }

        /// Quote response method.
        /// </summary>
        /// <param name="res">xdocument objects.</param>
        /// <returns>return response type object.</returns>
        public Response GetQuoteResponse(string res)
        {
            Response resModel = new Response();
            try
            {
                dynamic result = JObject.Parse(res);
                dynamic MotorPolicy = result.MotorPolicy;

                string status = result.MotorPolicy.ErrorMessages;
                PremiumBreakUpDetails pemBreakupDetails = new PremiumBreakUpDetails();
                if (string.IsNullOrEmpty(status))
                {
                    for (int i = 0; i < MotorPolicy.LstTaxComponentDetails.TaxComponent.Count; i++)
                    {
                        pemBreakupDetails.ServiceTax += MotorPolicy.LstTaxComponentDetails.TaxComponent[i].Amount;
                    }
                    for (int i = 0; i < MotorPolicy.lstPricingResponse.Count; i++)
                    {
                        if (MotorPolicy.lstPricingResponse[i].CoverID == 1)//Electrical Accessories
                        {
                            pemBreakupDetails.ElecAccessoriesPremium = MotorPolicy.lstPricingResponse[i].Premium;
                            continue;
                        }

                        if (MotorPolicy.lstPricingResponse[i].CoverID == 2)//Non Electrical Accessories
                        {
                            pemBreakupDetails.NonElecAccessoriesPremium = MotorPolicy.lstPricingResponse[i].Premium;
                            continue;
                        }

                        if (MotorPolicy.lstPricingResponse[i].CoverID == 3)//Voluntary Deductible
                        {
                            pemBreakupDetails.VoluntaryDiscount = MotorPolicy.lstPricingResponse[i].Premium;
                            continue;
                        }

                        if (MotorPolicy.lstPricingResponse[i].CoverID == 4)//Bifuel Kit
                        {
                            pemBreakupDetails.CNGLPGKitPremium = MotorPolicy.lstPricingResponse[i].Premium;
                            continue;
                        }

                        if (MotorPolicy.lstPricingResponse[i].CoverID == 10)//Nil Depreciation / Zero Depreciation
                        {
                            pemBreakupDetails.ZeroDepPremium = MotorPolicy.lstPricingResponse[i].Premium;
                            continue;
                        }

                        if (MotorPolicy.lstPricingResponse[i].CoverID == 11)//Anti-Theft Device
                        {
                            pemBreakupDetails.AntiTheftDiscount = MotorPolicy.lstPricingResponse[i].Premium;
                            continue;
                        }

                        if (MotorPolicy.lstPricingResponse[i].CoverID == 13)//Liability to Paid Driver
                        {
                            pemBreakupDetails.LLToPaidDriver = MotorPolicy.lstPricingResponse[i].Premium;
                            continue;
                        }

                        if (MotorPolicy.lstPricingResponse[i].CoverID == 15)//PA to Named Passenger
                        {
                            pemBreakupDetails.PACoverToNamedPerson = MotorPolicy.lstPricingResponse[i].Premium;
                            continue;
                        }

                        if (MotorPolicy.lstPricingResponse[i].CoverID == 16)//PA to Unnamed Passenger
                        {
                            pemBreakupDetails.PACoverToUnNamedPerson = MotorPolicy.lstPricingResponse[i].Premium;
                            continue;
                        }

                        if (MotorPolicy.lstPricingResponse[i].CoverID == 18)//Bifuel Kit TP
                        {
                            pemBreakupDetails.TPCNGLPGPremium = MotorPolicy.lstPricingResponse[i].Premium;
                            continue;
                        }

                        if (MotorPolicy.lstPricingResponse[i].CoverID == 19)//Automobile Association Membership
                        {
                            pemBreakupDetails.AAIDiscount = MotorPolicy.lstPricingResponse[i].Premium;
                            continue;
                        }

                        if (MotorPolicy.lstPricingResponse[i].CoverID == 21)//Basic OD
                        {
                            pemBreakupDetails.BasicODPremium = MotorPolicy.lstPricingResponse[i].Premium;
                            continue;
                        }

                        if (MotorPolicy.lstPricingResponse[i].CoverID == 22)//Basic Liability
                        {
                            pemBreakupDetails.BasicThirdPartyLiability = MotorPolicy.lstPricingResponse[i].Premium;
                            continue;
                        }

                        if (MotorPolicy.lstPricingResponse[i].CoverID == 23)//NCB
                        {
                            pemBreakupDetails.NCBDiscount = MotorPolicy.lstPricingResponse[i].Premium;
                            continue;
                        }

                        if (MotorPolicy.lstPricingResponse[i].CoverID == 24)//PA to Owner Driver
                        {
                            pemBreakupDetails.PACoverToOwnDriver = MotorPolicy.lstPricingResponse[i].Premium;
                            continue;
                        }

                        if (MotorPolicy.lstPricingResponse[i].CoverID == 25) //PA to Paid Driver
                        {
                            pemBreakupDetails.PAToPaidDriver = MotorPolicy.lstPricingResponse[i].Premium;
                            continue;
                        }
                    }

                    pemBreakupDetails.CurrentNCB = MotorPolicy.CurrentYearNCB;
                    resModel.PremiumBreakUpDetails = pemBreakupDetails;
                    resModel.Product = Product.Motor;
                    resModel.SubProduct = SubProduct.PrivateCar;
                    resModel.CompanyName = Company.RELIANCE.ToString();
                    resModel.IDV = MotorPolicy.IDV;
                    resModel.MinIDV = MotorPolicy.MinIDV;
                    resModel.MaxIDV = MotorPolicy.MaxIDV;
                    resModel.FinalPremium = MotorPolicy.FinalPremium;
                    resModel.EnquiryId = MotorPolicy.TraceID;
                    resModel.Status = Status.Success;
                }
            }
            catch (Exception ex)
            {
                resModel.Status = Status.Fail;
                resModel.ErrorMsg = Convert.ToString(ex.Message);
                Console.Write(Convert.ToString(ex.Message));
                LogU.WriteLog("RELIANCE >> PrivateCar >> GetQuoteResponse >> " + Convert.ToString(ex.Message));
                throw;
            }
            resModel.CompanyName = Company.RELIANCE.ToString();
            return resModel;
        }
        #endregion


        #region GetProposalRequest - Response
        public Response GetProposalRequest(Quotation model)
        {
            Response resModel = new Response();
            XmlDocument doc = new XmlDocument();

            try
            {
                string path = AppDomain.CurrentDomain.BaseDirectory;
                string filePath = Path.Combine(path, "XML/RELIANCE/Proposal.xml");
                var document = XDocument.Load(filePath);

                #region  ClientDetails
                document.XPathSelectElement("//ClientType").Value = string.IsNullOrEmpty(model.CustomerType) ? "0" : (model.CustomerType);
                document.XPathSelectElement("//LastName").Value = model.ClientDetails != null ? ((string.IsNullOrEmpty(model.ClientDetails.LastName)) ? "TestLName" : model.ClientDetails.LastName) : "TestLName";
                document.XPathSelectElement("//MidName").Value = model.ClientDetails != null ? ((string.IsNullOrEmpty(model.ClientDetails.MiddleName)) ? "TestMName" : model.ClientDetails.MiddleName) : "TestMName";
                document.XPathSelectElement("//ForeName").Value = model.ClientDetails != null ? ((string.IsNullOrEmpty(model.ClientDetails.FirstName)) ? "TestForeName" : model.ClientDetails.FirstName) : "TestForeName";

                document.XPathSelectElement("//OccupationID").Value = model.ClientDetails != null ? ((string.IsNullOrEmpty(model.ClientDetails.Occupation)) ? "0" : model.ClientDetails.Occupation) : "0";
                document.XPathSelectElement("//DOB").Value = model.ClientDetails != null ? ((string.IsNullOrEmpty(model.ClientDetails.DateOfBirth)) ? "2002-06-01" : model.ClientDetails.MiddleName) : "2002-06-01";
                document.XPathSelectElement("//Gender").Value = model.ClientDetails != null ? ((string.IsNullOrEmpty(model.ClientDetails.Gender)) ? "M" : model.ClientDetails.FirstName) : "M";
                document.XPathSelectElement("//MobileNo").Value = model.ClientDetails != null ? model.ClientDetails.MobileNo : string.Empty;

                #region  Communication Address
                document.XPathSelectElement("//ClientAddress/CommunicationAddress/Address1").Value = model.ClientDetails != null ? model.CustomerAddressDetails.Address1 : string.Empty;
                document.XPathSelectElement("//ClientAddress/CommunicationAddress/Address2").Value = model.ClientDetails != null ? model.CustomerAddressDetails.Address1 : string.Empty;
                document.XPathSelectElement("//ClientAddress/CommunicationAddress/CityID").Value = string.IsNullOrEmpty(model.CustomerAddressDetails.City) ? "0" : (model.CustomerAddressDetails.City);
                document.XPathSelectElement("//ClientAddress/CommunicationAddress/DistrictID").Value = string.IsNullOrEmpty(model.CustomerAddressDetails.district) ? "0" : (model.CustomerAddressDetails.district);
                document.XPathSelectElement("//ClientAddress/CommunicationAddress/StateID").Value = string.IsNullOrEmpty(model.CustomerAddressDetails.State) ? "0" : (model.CustomerAddressDetails.State);
                #endregion

                #region  Permanent Address  
                document.XPathSelectElement("//ClientAddress/PermanentAddress/Address1").Value = model.ClientDetails != null ? model.CustomerAddressDetails.Address1 : string.Empty;
                document.XPathSelectElement("//ClientAddress/PermanentAddress/Address2").Value = model.ClientDetails != null ? model.CustomerAddressDetails.Address1 : string.Empty;
                document.XPathSelectElement("//ClientAddress/PermanentAddress/CityID").Value = string.IsNullOrEmpty(model.CustomerAddressDetails.City) ? "0" : (model.CustomerAddressDetails.City);
                document.XPathSelectElement("//ClientAddress/PermanentAddress/DistrictID").Value = string.IsNullOrEmpty(model.CustomerAddressDetails.district) ? "0" : (model.CustomerAddressDetails.district);
                document.XPathSelectElement("//ClientAddress/PermanentAddress/StateID").Value = string.IsNullOrEmpty(model.CustomerAddressDetails.State) ? "0" : (model.CustomerAddressDetails.State);
                #endregion

                #region  Registration Address  
                document.XPathSelectElement("//ClientAddress/RegistrationAddress/Address1").Value = model.ClientDetails != null ? model.VehicleAddressDetails.Address1 : string.Empty;
                document.XPathSelectElement("//ClientAddress/RegistrationAddress/Address2").Value = model.ClientDetails != null ? model.VehicleAddressDetails.Address1 : string.Empty;
                document.XPathSelectElement("//ClientAddress/RegistrationAddress/CityID").Value = string.IsNullOrEmpty(model.VehicleAddressDetails.City) ? "0" : (model.VehicleAddressDetails.City);
                document.XPathSelectElement("//ClientAddress/RegistrationAddress/DistrictID").Value = string.IsNullOrEmpty(model.VehicleAddressDetails.district) ? "0" : (model.VehicleAddressDetails.district);
                document.XPathSelectElement("//ClientAddress/RegistrationAddress/StateID").Value = string.IsNullOrEmpty(model.VehicleAddressDetails.State.ToString()) ? "0" : (model.VehicleAddressDetails.State.ToString());
                #endregion


                document.XPathSelectElement("//EmailID").Value = model.ClientDetails != null ? model.ClientDetails.EmailId : string.Empty;
                document.XPathSelectElement("//Salutation").Value = model.ClientDetails != null ? model.ClientDetails.Salutation : string.Empty;
                document.XPathSelectElement("//MaritalStatus").Value = model.ClientDetails != null ? model.ClientDetails.MaritalStatus : string.Empty;
                #endregion

                #region Policy
                document.XPathSelectElement("//BusinessType").Value = model.PolicyType;
                document.XPathSelectElement("//Cover_From").Value = model.PolicyStartDate;
                document.XPathSelectElement("//Cover_To").Value = model.PolicyEndDate;
                #endregion

                #region Risk
                string regisrationDate = Convert.ToDateTime(model.VehicleDetails.RegistrationDate).ToString("dd/MM/yyyy");
                string purchaseDate = Convert.ToDateTime(model.VehicleDetails.PurchaseDate).ToString("dd/MM/yyyy");
                document.XPathSelectElement("//VehicleMakeID").Value = model.VehicleDetails != null ? model.VehicleDetails.MakeCode : string.Empty;
                document.XPathSelectElement("//VehicleModelID").Value = model.VehicleDetails != null ? model.VehicleDetails.ModelCode : string.Empty;
                document.XPathSelectElement("//CubicCapacity").Value = model.VehicleDetails != null ? ((string.IsNullOrEmpty(Convert.ToString(model.VehicleDetails.CC))) ? "0" : Convert.ToString(model.VehicleDetails.CC)) : "0";
                document.XPathSelectElement("//RTOLocationID").Value = model.VehicleDetails != null ? ((string.IsNullOrEmpty(Convert.ToString(model.VehicleDetails.RtoId))) ? "0" : Convert.ToString(model.VehicleDetails.RtoId)) : "0";
                document.XPathSelectElement("//Zone").Value = model.VehicleDetails != null ? model.VehicleDetails.RtoZone : string.Empty;
                document.XPathSelectElement("//IDV").Value = Convert.ToString(model.IDV);
                document.XPathSelectElement("//DateOfPurchase").Value = purchaseDate;

                int ManufactureMonth = Convert.ToDateTime(model.VehicleDetails.ManufaturingDate).Month;
                int ManufactureYear = Convert.ToDateTime(model.VehicleDetails.ManufaturingDate).Year;
                document.XPathSelectElement("//ManufactureMonth").Value = ManufactureMonth.ToString();
                document.XPathSelectElement("//ManufactureYear").Value = ManufactureYear.ToString();

                document.XPathSelectElement("//VehicleVariant").Value = model.VehicleDetails != null ? model.VehicleDetails.VariantName : string.Empty;
                document.XPathSelectElement("//StateOfRegistrationID").Value = model.VehicleAddressDetails != null ? model.VehicleAddressDetails.State.ToString() : string.Empty; ;
                document.XPathSelectElement("//Rto_RegionCode").Value = model.VehicleDetails != null ? model.VehicleDetails.RtoZone : string.Empty;

                string financertype = "0";
                if (model.VehicleDetails.IsVehicleLoan==true)
                {
                    financertype = "1";
                }

                document.XPathSelectElement("//IsVehicleHypothicated").Value = model.VehicleDetails.IsVehicleLoan.ToString();
                document.XPathSelectElement("//FinanceType").Value = financertype;
                document.XPathSelectElement("//FinancierName").Value = string.IsNullOrEmpty(model.VehicleDetails.LoanCompanyName) ? "" : string.Empty;
                document.XPathSelectElement("//FinancierAddress").Value = string.IsNullOrEmpty(model.VehicleDetails.LoanCompanyName) ? "" : string.Empty;

                document.XPathSelectElement("//EngineNo").Value = model.VehicleDetails != null ? ((string.IsNullOrEmpty(model.VehicleDetails.EngineNumber)) ? "Engineno123456789" : model.VehicleDetails.EngineNumber) : "Engineno123456789";
                document.XPathSelectElement("//Chassis").Value = model.VehicleDetails != null ? ((string.IsNullOrEmpty(model.VehicleDetails.ChassisNumber)) ? "Chassisno123456789" : model.VehicleDetails.ChassisNumber) : "Chassisno123456789";

                #endregion

                #region Vehicle
                document.XPathSelectElement("//Registration_Number").Value = model.VehicleDetails != null ? model.VehicleDetails.RegistrationNumber : string.Empty;
                document.XPathSelectElement("//Registration_date").Value = regisrationDate;
                document.XPathSelectElement("//SeatingCapacity").Value = model.VehicleDetails != null ? ((string.IsNullOrEmpty(Convert.ToString(model.VehicleDetails.SC))) ? "0" : Convert.ToString(model.VehicleDetails.SC)) : "0";
                document.XPathSelectElement("//TypeOfFuel").Value = model.VehicleDetails != null ? ((string.IsNullOrEmpty(Convert.ToString(model.VehicleDetails.Fuel))) ? "1" : Convert.ToString(model.VehicleDetails.Fuel)) : "1";
                #endregion

                #region Cover
                document.XPathSelectElement("//IsElectricalItemFitted").Value = model.CoverageDetails.IsElectricalAccessories.ToString();
                document.XPathSelectElement("//ElectricalItemsTotalSI").Value = model.CoverageDetails.ElectricalAccessoriesDetails[0].Amount.ToString();

                document.XPathSelectElement("//IsNonElectricalItemFitted").Value = model.CoverageDetails.IsNonElectricalAccessories.ToString();
                document.XPathSelectElement("//ElectricalItemsTotalSI").Value = model.CoverageDetails.NonElectricalAccessoriesDetails[0].Amount.ToString();

                document.XPathSelectElement("//IsBiFuelKit").Value = model.CoverageDetails.IsBiFuelKit.ToString();
                document.XPathSelectElement("//BifuelKit/BifuelKit/ISLpgCng").Value = model.CoverageDetails.IsBiFuelKit.ToString();
                document.XPathSelectElement("//BifuelKit/BifuelKit/SumInsured").Value = model.CoverageDetails.BiFuelKitAmount.ToString();

                document.XPathSelectElement("//IsAutomobileAssociationMember").Value = model.DiscountDetails.IsMemberOfAutomobileAssociation.ToString();
                document.XPathSelectElement("//AutomobileAssociationExpiryDate").Value = model.DiscountDetails.AutomobileAssociationMemberExpiryDate.ToString();

                document.XPathSelectElement("//IsVoluntaryDeductableOpted").Value = model.DiscountDetails.IsVoluntaryExcess.ToString();
                document.XPathSelectElement("//VoluntaryDeductible/VoluntaryDeductible/SumInsured").Value = model.DiscountDetails.VoluntaryExcessAmount.ToString();
                document.XPathSelectElement("//IsAntiTheftDeviceFitted").Value = model.DiscountDetails.IsAntiTheftDevice.ToString();

                #region Applicable Rate
                int cyear = DateTime.Now.Year;
                int cday = DateTime.Now.Day;
                int vage = cyear - ManufactureYear;
                string ApplicableRate = "";
                if (vage == 1 || vage == 2)
                {
                    ApplicableRate = "0.45";
                }
                else if (vage == 3)
                {
                    ApplicableRate = "0.55";
                }
                else if (vage == 3)
                {
                    ApplicableRate = "0.70";
                }
                else if (vage >= 4 || vage <= 15)
                {
                    ApplicableRate = "0.85";
                }

                document.XPathSelectElement("//NilDepreciationCoverage/NilDepreciationCoverage/ApplicableRate").Value = ApplicableRate;

                #endregion

                document.XPathSelectElement("//IsPAToNamedPassenger").Value = model.CoverageDetails.IsPACoverForNamedPersons.ToString();
                document.XPathSelectElement("//PAToNamedPassenger/PAToNamedPassenger/NoOfItems").Value = model.VehicleDetails != null ? ((string.IsNullOrEmpty(Convert.ToString(model.VehicleDetails.SC))) ? "0" : Convert.ToString(model.VehicleDetails.SC)) : "0";
                document.XPathSelectElement("//PAToNamedPassenger/PAToNamedPassenger/SumInsured").Value = model.CoverageDetails.CapitalSumInsuredPerPersonNamed.ToString();


                document.XPathSelectElement("//IsPAToUnnamedPassengerCovered").Value = model.CoverageDetails.IsPACoverUnnamedPerson.ToString();
                document.XPathSelectElement("//PAToUnNamedPassenger/PAToUnNamedPassenger/NoOfItems").Value = model.VehicleDetails != null ? ((string.IsNullOrEmpty(Convert.ToString(model.VehicleDetails.SC))) ? "0" : Convert.ToString(model.VehicleDetails.SC)) : "0";
                document.XPathSelectElement("//PAToUnNamedPassenger/PAToUnNamedPassenger/SumInsured").Value = model.CoverageDetails.PACoverUnnamedPersonAmount.ToString();


                document.XPathSelectElement("//IsPAToDriverCovered").Value = model.CoverageDetails.IsPACoverPaidDriver.ToString();
                document.XPathSelectElement("//PAToPaidDriver/PAToPaidDriver/NoOfItems").Value = "1";
                document.XPathSelectElement("//PAToPaidDriver/PAToPaidDriver/SumInsured").Value = model.CoverageDetails.PACoverPaidDriverAmount.ToString();


                #endregion

                document.XPathSelectElement("//PrevYearInsurer").Value = model.PreviousPolicyDetails.CompanyId.ToString();
                document.XPathSelectElement("//PrevYearPolicyNo").Value = model.PreviousPolicyDetails.PreviousPolicyNo.ToString();
                document.XPathSelectElement("//PrevYearPolicyStartDate").Value = model.PreviousPolicyDetails.PreviousPolicyStartDate;
                document.XPathSelectElement("//PrevYearPolicyEndDate").Value = model.PreviousPolicyDetails.PreviousPolicyEndDate;

                document.XPathSelectElement("//NCBEligibility/PreviousNCB").Value = model.PreviousPolicyDetails.PreviousNcbPercentage.ToString();
                document.XPathSelectElement("//NCBEligibility/CurrentNCB").Value = model.CurrentNcb.ToString();

                var response = RelianceResponse(document.ToString(), "https://rgipartners.reliancegeneral.co.in/API/Service/ProposalCreationForMotor");
                resModel = GetProposalResponse(response);
            }
            catch (Exception ex)
            {
                resModel.Status = Status.Fail;
                resModel.ErrorMsg = Convert.ToString(ex.Message);
                Console.Write(Convert.ToString(ex.Message));
                LogU.WriteLog("RELIANCE >> PrivateCar >> GetProposalRequest >> " + Convert.ToString(ex.Message));
                throw;
            }
            return resModel;
        }

        /// <summary>
        /// Proposal response method.
        /// </summary>
        /// <param name="res">xdocument objects.</param>
        /// <returns>return response type object.</returns>
        public Response GetProposalResponse(string res)
        {
            Response resModel = new Response();
            try
            {
                dynamic result = JObject.Parse(res);
                dynamic MotorPolicy = result.MotorPolicy;

                string status = result.MotorPolicy.ErrorMessages;
                PremiumBreakUpDetails pemBreakupDetails = new PremiumBreakUpDetails();

                if (string.IsNullOrEmpty(status))
                {
                    for (int i = 0; i < MotorPolicy.LstTaxComponentDetails.TaxComponent.Count; i++)
                    {
                        pemBreakupDetails.ServiceTax += MotorPolicy.LstTaxComponentDetails.TaxComponent[i].Amount;
                    }
                    for (int i = 0; i < MotorPolicy.lstPricingResponse.Count; i++)
                    {
                        if (MotorPolicy.lstPricingResponse[i].CoverID == 1)//Electrical Accessories
                        {
                            pemBreakupDetails.ElecAccessoriesPremium = MotorPolicy.lstPricingResponse[i].Premium;
                            continue;
                        }

                        if (MotorPolicy.lstPricingResponse[i].CoverID == 2)//Non Electrical Accessories
                        {
                            pemBreakupDetails.NonElecAccessoriesPremium = MotorPolicy.lstPricingResponse[i].Premium;
                            continue;
                        }

                        if (MotorPolicy.lstPricingResponse[i].CoverID == 3)//Voluntary Deductible
                        {
                            pemBreakupDetails.VoluntaryDiscount = MotorPolicy.lstPricingResponse[i].Premium;
                            continue;
                        }

                        if (MotorPolicy.lstPricingResponse[i].CoverID == 4)//Bifuel Kit
                        {
                            pemBreakupDetails.CNGLPGKitPremium = MotorPolicy.lstPricingResponse[i].Premium;
                            continue;
                        }

                        if (MotorPolicy.lstPricingResponse[i].CoverID == 10)//Nil Depreciation / Zero Depreciation
                        {
                            pemBreakupDetails.ZeroDepPremium = MotorPolicy.lstPricingResponse[i].Premium;
                            continue;
                        }

                        if (MotorPolicy.lstPricingResponse[i].CoverID == 11)//Anti-Theft Device
                        {
                            pemBreakupDetails.AntiTheftDiscount = MotorPolicy.lstPricingResponse[i].Premium;
                            continue;
                        }

                        if (MotorPolicy.lstPricingResponse[i].CoverID == 13)//Liability to Paid Driver
                        {
                            pemBreakupDetails.LLToPaidDriver = MotorPolicy.lstPricingResponse[i].Premium;
                            continue;
                        }

                        if (MotorPolicy.lstPricingResponse[i].CoverID == 15)//PA to Named Passenger
                        {
                            pemBreakupDetails.PACoverToNamedPerson = MotorPolicy.lstPricingResponse[i].Premium;
                            continue;
                        }

                        if (MotorPolicy.lstPricingResponse[i].CoverID == 16)//PA to Unnamed Passenger
                        {
                            pemBreakupDetails.PACoverToUnNamedPerson = MotorPolicy.lstPricingResponse[i].Premium;
                            continue;
                        }

                        if (MotorPolicy.lstPricingResponse[i].CoverID == 18)//Bifuel Kit TP
                        {
                            pemBreakupDetails.TPCNGLPGPremium = MotorPolicy.lstPricingResponse[i].Premium;
                            continue;
                        }

                        if (MotorPolicy.lstPricingResponse[i].CoverID == 19)//Automobile Association Membership
                        {
                            pemBreakupDetails.AAIDiscount = MotorPolicy.lstPricingResponse[i].Premium;
                            continue;
                        }

                        if (MotorPolicy.lstPricingResponse[i].CoverID == 21)//Basic OD
                        {
                            pemBreakupDetails.BasicODPremium = MotorPolicy.lstPricingResponse[i].Premium;
                            continue;
                        }

                        if (MotorPolicy.lstPricingResponse[i].CoverID == 22)//Basic Liability
                        {
                            pemBreakupDetails.BasicThirdPartyLiability = MotorPolicy.lstPricingResponse[i].Premium;
                            continue;
                        }

                        if (MotorPolicy.lstPricingResponse[i].CoverID == 23)//NCB
                        {
                            pemBreakupDetails.NCBDiscount = MotorPolicy.lstPricingResponse[i].Premium;
                            continue;
                        }

                        if (MotorPolicy.lstPricingResponse[i].CoverID == 24)//PA to Owner Driver
                        {
                            pemBreakupDetails.PACoverToOwnDriver = MotorPolicy.lstPricingResponse[i].Premium;
                            continue;
                        }

                        if (MotorPolicy.lstPricingResponse[i].CoverID == 25) //PA to Paid Driver
                        {
                            pemBreakupDetails.PAToPaidDriver = MotorPolicy.lstPricingResponse[i].Premium;
                            continue;
                        }
                    }

                    pemBreakupDetails.CurrentNCB = MotorPolicy.CurrentYearNCB;
                    resModel.PremiumBreakUpDetails = pemBreakupDetails;
                    resModel.Product = Product.Motor;
                    resModel.SubProduct = SubProduct.PrivateCar;
                    resModel.CompanyName = Company.RELIANCE.ToString();
                    resModel.IDV = MotorPolicy.IDV;
                    resModel.MinIDV = MotorPolicy.MinIDV;
                    resModel.MaxIDV = MotorPolicy.MaxIDV;
                    resModel.FinalPremium = MotorPolicy.FinalPremium;
                    resModel.EnquiryId = MotorPolicy.TraceID;
                    resModel.Status = Status.Success;
                }
                else
                {
                    resModel.Status = Status.Fail;
                    resModel.ErrorMsg = Convert.ToString(result.MotorPolicy.ErrorMessages);
                }
            }
            catch (Exception ex)
            {
                resModel.Status = Status.Fail;
                resModel.ErrorMsg = Convert.ToString(ex.Message);
                Console.Write(Convert.ToString(ex.Message));
                LogU.WriteLog("RELIANCE >> PrivateCar >> GetProposalResponse >> " + Convert.ToString(ex.Message));
                throw;
            }
            return resModel;
        }

        #endregion


        public string RelianceResponse(string xml, string url)
        {

            Uri myUri = new Uri(url.ToString(), UriKind.Absolute);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(myUri);
            System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            string requestData = xml.ToString();
            byte[] data = Encoding.UTF8.GetBytes(requestData);
            request.Method = "POST";
            Stream dataStream = request.GetRequestStream();
            dataStream.Write(data, 0, data.Length);
            dataStream.Close();
            request.ContentType = "application/xml";
            request.Accept = "application/json";
            WebResponse response = request.GetResponse();
            response = request.GetResponse();

            string xmlresponse = new StreamReader(response.GetResponseStream()).ReadToEnd();


            return xmlresponse;
        }
    }
}