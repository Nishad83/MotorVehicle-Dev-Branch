

namespace AndWebApi.TATA
{
    #region namespace
    using AndWebApi;
    using AndWebApi.Models;
    using AndApp.Utilities;
    using Controllers;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using RestSharp;
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Security;
    using System.Web;
    using System.Web.Http.Results;
    using System.Web.Mvc;
    using System.Web.Script.Serialization;
    using System.Globalization;
    #endregion
    public class PrivateCar
    {
        DAL.ANDAPPEntities ap = new DAL.ANDAPPEntities();
        DefaultController de = new DefaultController();
        public Response GetQuoteRequest(Quotation model)
        {
            Response resModel = new Response();
            try
            {
                long rsawithoutgst = 0;
                string IDV = "";
                object obj = "";
                string btnflag = "cal";


                if (model.IDV == 0)
                {

                    obj = GetQuoteTata("", "quick", model, "");

                    var quickquote_response = Webservicecall(obj, "" + ConfigurationManager.AppSettings["TATAquotation"] + "", "quotation", model.enquiryid);
                    if (quickquote_response.StatusCode == HttpStatusCode.OK)
                    {
                        var resdata = (JObject)JsonConvert.DeserializeObject(quickquote_response.Content.ToString());
                        var q_data = resdata["data"];
                        var status = q_data["status"].Value<string>(); ;
                        var q_quotationdata = q_data["quotationdata"];
                        var idvdatamin = q_quotationdata["idvlowerlimit"].Value<string>();
                        var idvdatamax = q_quotationdata["idvupperlimit"].Value<string>();
                        var idv = q_quotationdata["idv"].Value<string>();
                        double idvdata = double.Parse(idv);
                        IDV = idv;
                      
                        var fullquote_obj = GetQuoteTata(IDV, "full", model, "");
                        var fullquote_res = Webservicecall(fullquote_obj, "" + ConfigurationManager.AppSettings["TATAquotation"] + "", "quotation", model.enquiryid);
                        if (fullquote_res.StatusCode == HttpStatusCode.OK)
                        {
                            resModel.FreeAddonCover = new AddonCover();
                            var fullquote_resdata = (JObject)JsonConvert.DeserializeObject(fullquote_res.Content.ToString());

                            var fullquote_data = fullquote_resdata["data"];
                            var fullquote_tax = fullquote_data["TAX"];
                            var quotation_data = fullquote_data["quotationdata"];
                            var proposal_status = fullquote_data["status"].Value<string>();

                            string quotation_no = quotation_data["quotation_no"].Value<string>();
                            PremiumBreakUpDetails prm = new PremiumBreakUpDetails();
                          

                            if (model.IsThirdPartyOnly!=true)
                            {
                                prm.BasicODPremium = Math.Round(fullquote_data["C1"]["premium"].Value<double>());
                                prm.NetODPremium = Math.Round(fullquote_data["NETOD"].Value<double>());
                            }
                          
                            if (fullquote_data["NETADDON"].Value<double>() != 0)
                            {
                                //var rsaG= fullquote_data["C47"]["premium"].Value<double>() * 0.18;
                                rsawithoutgst = (long)((fullquote_data["C47"]["premium"].Value<double>() *100)/118);
                                prm.NetAddonPremium = fullquote_data["NETADDON"].Value<double>() + rsawithoutgst;
                                prm.NetPremium = fullquote_data["NETPREM"].Value<double>() + rsawithoutgst ;

                                prm.ServiceTax = fullquote_tax["total_prem"].Value<double>() + (fullquote_data["C47"]["premium"].Value<double>() - rsawithoutgst);
                                resModel.FreeAddonCover.RepairFiberGlass = true;
                            
                            }
                            else
                            {
                                if (model.AddonCover.IsRoadSideAssistance==true)
                                {
                                    rsawithoutgst = (long)((fullquote_data["C47"]["premium"].Value<double>() * 100) / 118);
                                    prm.NetAddonPremium = fullquote_data["NETADDON"].Value<double>() + rsawithoutgst;
                                    prm.NetPremium = fullquote_data["NETPREM"].Value<double>() + rsawithoutgst;

                                    prm.ServiceTax = fullquote_tax["total_prem"].Value<double>() + (fullquote_data["C47"]["premium"].Value<double>() - rsawithoutgst);
                                    resModel.FreeAddonCover.RepairFiberGlass = true;
                                }
                                else
                                {
                                    prm.NetAddonPremium = fullquote_data["NETADDON"].Value<double>();
                                    prm.NetPremium = fullquote_data["NETPREM"].Value<double>();
                                    prm.ServiceTax = fullquote_tax["total_prem"].Value<double>();

                                }
                              

                            }

                            prm.NetTPPremium = Math.Round(fullquote_data["NETTP"].Value<double>());

                         
                            prm.NCBDiscount = Math.Round(fullquote_data["C15"]["premium"].Value<double>());
                            if (model.IsODOnly != true)
                            {
                                prm.BasicThirdPartyLiability = Math.Round(fullquote_data["C2"]["premium"].Value<double>());
                                prm.PACoverToOwnDriver = Math.Round(fullquote_data["C3"]["premium"].Value<double>());
                            }
                            //prm.BasicThirdPartyLiability
                            resModel.EnquiryId = de.GenerateEnquiryId();
                            resModel.FinalPremium = fullquote_data["TOTALPAYABLE"].Value<int>();


                            if (model.IsThirdPartyOnly == true)
                            {
                                resModel.IDV = 0;
                                resModel.MaxIDV = 0;
                                resModel.MinIDV = 0;
                            }
                            else
                            {
                                resModel.IDV = Convert.ToInt32(IDV);
                                resModel.MaxIDV = Convert.ToInt32(idvdatamax);
                                resModel.MinIDV = Convert.ToInt32(idvdatamin);
                            }



                            //resModel.PolicyStartDate = quotation_data["risk_startdate"].Value<string>();
                            //resModel.PolicyEndDate = quotation_data["risk_enddate"].Value<string>();
                            var sd = quotation_data["risk_startdate"].Value<string>();
                            var ed = quotation_data["risk_enddate"].Value<string>();
                            DateTime startdate = DateTime.ParseExact(sd, "yyyyMMdd",
                                         CultureInfo.InvariantCulture);
                            resModel.PolicyStartDate = Convert.ToDateTime(startdate).ToString("dd-MM-yyyy");

                            DateTime enddate = DateTime.ParseExact(ed, "yyyyMMdd",
                                       CultureInfo.InvariantCulture);
                            resModel.PolicyEndDate = Convert.ToDateTime(enddate).ToString("dd-MM-yyyy");




                            resModel.CC = quotation_data["cc"].Value<int>();
                            resModel.SC = quotation_data["sc"].Value<int>();
                            resModel.FuelType = quotation_data["fuel_name"].Value<string>();

                            var coveropted = quotation_data["cover_opted"].Value<string>();
                            var od_covers = quotation_data["od_covers"].Value<string>();
                            var addon_covers = quotation_data["addon_covers"].Value<string>();
                            var tp_covers = quotation_data["tp_covers"].Value<string>();
                            if (model.CoverageDetails.IsElectricalAccessories == true)
                            {
                                prm.ElecAccessoriesPremium = Math.Round(fullquote_data["C4"]["premium"].Value<double>());
                            }

                            if (model.CoverageDetails.IsNonElectricalAccessories == true)
                            {
                                prm.NonElecAccessoriesPremium = Math.Round(fullquote_data["C5"]["premium"].Value<double>());
                            }

                            if (model.CoverageDetails.IsBiFuelKit == true)
                            {

                                if (model.IsThirdPartyOnly==true)
                                {
                                    prm.TPCNGLPGPremium = Math.Round(fullquote_data["C29"]["premium"].Value<double>());
                                }
                                else
                                {
                                    prm.CNGLPGKitPremium = Math.Round(fullquote_data["C7"]["premium"].Value<double>());
                                    if (model.IsODOnly == true)
                                    {

                                    }
                                    else
                                    {
                                        prm.TPCNGLPGPremium = Math.Round(fullquote_data["C29"]["premium"].Value<double>());

                                    }
                                }
                               
                            }
                            var addoncovers = coveropted.ToString().Split(',');

                            for (int i = 0; i < addoncovers.Length; i++)
                            {
                                string covercode = addoncovers[i];
                                if (covercode != "C1" && covercode != "C2" && covercode != "C3" && covercode != "C4" && covercode != "C5")
                                {

                                    //if (covercode == "C4")
                                    //{
                                    //    prm.ElecAccessoriesPremium = fullquote_data[covercode]["premium"].Value<double>();
                                    //}
                                    //if (covercode == "C5")
                                    //{
                                    //    prm.NonElecAccessoriesPremium = fullquote_data[covercode]["premium"].Value<double>();
                                    //}
                                    if (covercode == "C17")
                                    {
                                        prm.PACoverToUnNamedPerson = Math.Round(fullquote_data[covercode]["premium"].Value<double>());
                                    }
                                    if (covercode == "C35")
                                    {
                                        prm.ZeroDepPremium = Math.Round(fullquote_data[covercode]["premium"].Value<double>());
                                    }
                                    if (covercode == "C37")
                                    {
                                        prm.CostOfConsumablesPremium = Math.Round(fullquote_data[covercode]["premium"].Value<double>());
                                    }
                                    if (covercode == "C38")
                                    {
                                        prm.InvoicePriceCoverPremium = Math.Round(fullquote_data[covercode]["premium"].Value<double>());
                                    }
                                    if (covercode == "C39")
                                    {
                                        prm.NcbProtectorPremium = Math.Round(fullquote_data[covercode]["premium"].Value<double>());
                                    }
                                    if (covercode == "C45")
                                    {
                                        prm.TyreProtect = Math.Round(fullquote_data[covercode]["premium"].Value<double>());
                                    }
                                    if (covercode == "C47")
                                    {
                                        prm.RSAPremium = rsawithoutgst;
                                       
                                        //fullquote_data[covercode]["premium"].Value<double>();
                                    }
                                    if (covercode == "C48")
                                    {
                                        prm.EmergencyAssistancePremium = Math.Round(fullquote_data[covercode]["premium"].Value<double>());
                                    }
                                    if (covercode == "C40")
                                    {
                                        prm.FiberGlassTankPremium = Math.Round(fullquote_data[covercode]["premium"].Value<double>());
                                    }
                                    if (covercode == "C41")
                                    {
                                        prm.LossOfPersonalBelongingPremium = Math.Round(fullquote_data[covercode]["premium"].Value<double>());
                                    }
                                    if (covercode == "C42")
                                    {
                                        prm.EmergencyAssistancePremium = Math.Round(fullquote_data[covercode]["premium"].Value<double>());
                                    }
                                    if (covercode == "C43")
                                    {
                                        prm.KeyReplacementPremium = Math.Round(fullquote_data[covercode]["premium"].Value<double>());
                                    }
                                    if (covercode == "C44")
                                    {
                                        prm.EngineProtectorPremium = Math.Round(fullquote_data[covercode]["premium"].Value<double>());
                                    }
                                    if (covercode == "C18")
                                    {
                                        prm.LLToPaidDriver = Math.Round(fullquote_data[covercode]["premium"].Value<double>());
                                    }
                                    // netaddon = netaddon + fullquote_data[covercode]["premium"].Value<double>();
                                }
                            }


                            resModel.PremiumBreakUpDetails = prm;
                            CompanyWiseRefference reff = new CompanyWiseRefference();
                            reff.QuoteNo = quotation_no;
                            resModel.CompanyWiseRefference = reff;
                            resModel.CompanyName = Company.TATA.ToString();
                            resModel.Status = Status.Success;
                        }
                        else
                        {
                            string errmsg = fullquote_res.Content.ToString();
                            resModel.Status = Status.Fail;
                            resModel.ErrorMsg = errmsg;
                        }
                    }

                    else
                    {
                        string errmsg = quickquote_response.Content.ToString();
                        resModel.Status = Status.Fail;
                        resModel.ErrorMsg = errmsg;
                    }
                }
                else
                {
                    dynamic getidvvalues = null;
                    if (model.IDV != 0)
                    {
                        if (model.CustomIDV != null)
                        {
                            getidvvalues = model.CustomIDV.Where(x => x.CompanyName == "TATA").FirstOrDefault();
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

                        var fullquote_obj = GetQuoteTata(model.IDV.ToString(), "full", model, "");
                        var fullquote_res = Webservicecall(fullquote_obj, "" + ConfigurationManager.AppSettings["TATAquotation"] + "", "quotation", model.enquiryid);
                        if (fullquote_res.StatusCode == HttpStatusCode.OK)
                        {
                            resModel.FreeAddonCover = new AddonCover();
                            var fullquote_resdata = (JObject)JsonConvert.DeserializeObject(fullquote_res.Content.ToString());

                            var fullquote_data = fullquote_resdata["data"];
                            var fullquote_tax = fullquote_data["TAX"];
                            var quotation_data = fullquote_data["quotationdata"];
                            var proposal_status = fullquote_data["status"].Value<string>();

                            string quotation_no = quotation_data["quotation_no"].Value<string>();
                            PremiumBreakUpDetails prm = new PremiumBreakUpDetails();

                            if (model.IsODOnly != true)
                            {
                                prm.BasicThirdPartyLiability = Math.Round(fullquote_data["C2"]["premium"].Value<double>());
                                prm.PACoverToOwnDriver = Math.Round(fullquote_data["C3"]["premium"].Value<double>());

                            }

                            if (fullquote_data["NETADDON"].Value<double>() != 0)
                            {
                                //var rsaG= fullquote_data["C47"]["premium"].Value<double>() * 0.18;
                                rsawithoutgst = (long)((fullquote_data["C47"]["premium"].Value<double>() * 100) / 118);
                                prm.NetAddonPremium = fullquote_data["NETADDON"].Value<double>() + rsawithoutgst;
                                prm.NetPremium = fullquote_data["NETPREM"].Value<double>() + rsawithoutgst;

                                prm.ServiceTax = fullquote_tax["total_prem"].Value<double>() + (fullquote_data["C47"]["premium"].Value<double>() - rsawithoutgst);
                                resModel.FreeAddonCover.RepairFiberGlass = true;
                            }
                            else
                            {
                                if (model.AddonCover.IsRoadSideAssistance == true)
                                {
                                    rsawithoutgst = (long)((fullquote_data["C47"]["premium"].Value<double>() * 100) / 118);
                                    prm.NetAddonPremium = fullquote_data["NETADDON"].Value<double>() + rsawithoutgst;
                                    prm.NetPremium = fullquote_data["NETPREM"].Value<double>() + rsawithoutgst;

                                    prm.ServiceTax = fullquote_tax["total_prem"].Value<double>() + (fullquote_data["C47"]["premium"].Value<double>() - rsawithoutgst);
                                    resModel.FreeAddonCover.RepairFiberGlass = true;
                                }
                                else
                                {
                                    prm.NetAddonPremium = fullquote_data["NETADDON"].Value<double>();
                                    prm.NetPremium = fullquote_data["NETPREM"].Value<double>();
                                    prm.ServiceTax = fullquote_tax["total_prem"].Value<double>();

                                }


                            }
                            prm.BasicODPremium = Math.Round(fullquote_data["C1"]["premium"].Value<double>());
                            prm.NetODPremium = Math.Round(fullquote_data["NETOD"].Value<double>());
                            //prm.NetAddonPremium = fullquote_data["NETADDON"].Value<double>();
                            prm.NetTPPremium = Math.Round(fullquote_data["NETTP"].Value<double>());
                            //prm.NetPremium = fullquote_data["NETPREM"].Value<double>();
                        //    prm.ServiceTax = fullquote_tax["total_prem"].Value<double>();
                            prm.NCBDiscount = Math.Round(fullquote_data["C15"]["premium"].Value<double>());
                            resModel.EnquiryId = de.GenerateEnquiryId();
                            resModel.FinalPremium = fullquote_data["TOTALPAYABLE"].Value<int>();

                            resModel.IDV = Convert.ToInt32(model.IDV);
                            resModel.MaxIDV = Convert.ToInt32(getidvvalues.MaxIDV);
                            resModel.MinIDV = Convert.ToInt32(getidvvalues.MinIDV);
                            //resModel.PolicyStartDate = quotation_data["risk_startdate"].Value<string>();
                            //resModel.PolicyEndDate = quotation_data["risk_enddate"].Value<string>();


                            var sd = quotation_data["risk_startdate"].Value<string>();
                            var ed = quotation_data["risk_enddate"].Value<string>();
                            DateTime startdate = DateTime.ParseExact(sd, "yyyyMMdd",
                                         CultureInfo.InvariantCulture);
                            resModel.PolicyStartDate = Convert.ToDateTime(startdate).ToString("dd-MM-yyyy");

                            DateTime enddate = DateTime.ParseExact(ed, "yyyyMMdd",
                                       CultureInfo.InvariantCulture);
                            resModel.PolicyEndDate = Convert.ToDateTime(enddate).ToString("dd-MM-yyyy");
                            resModel.CC = quotation_data["cc"].Value<int>();
                            resModel.SC = quotation_data["sc"].Value<int>();
                            resModel.FuelType = quotation_data["fuel_name"].Value<string>();



                            var coveropted = quotation_data["cover_opted"].Value<string>();
                            var od_covers = quotation_data["od_covers"].Value<string>();
                            var addon_covers = quotation_data["addon_covers"].Value<string>();
                            var tp_covers = quotation_data["tp_covers"].Value<string>();

                            if (model.CoverageDetails.IsElectricalAccessories == true)
                            {
                                prm.ElecAccessoriesPremium = Math.Round(fullquote_data["C4"]["premium"].Value<double>());
                            }

                            if (model.CoverageDetails.IsNonElectricalAccessories == true)
                            {
                                prm.NonElecAccessoriesPremium = Math.Round(fullquote_data["C5"]["premium"].Value<double>());
                            }
                            if (model.CoverageDetails.IsBiFuelKit == true)
                            {
                                if (model.IsThirdPartyOnly == true)
                                {
                                    prm.TPCNGLPGPremium = Math.Round(fullquote_data["C29"]["premium"].Value<double>());
                                }
                                else
                                {
                                    prm.CNGLPGKitPremium = Math.Round(fullquote_data["C7"]["premium"].Value<double>());
                                    if (model.IsODOnly == true)
                                    {

                                    }
                                    else
                                    {
                                        prm.TPCNGLPGPremium = Math.Round(fullquote_data["C29"]["premium"].Value<double>());

                                    }
                                }
                            }
                            // ADD ON  COVERS FOR LOOP
                            var addoncovers = coveropted.ToString().Split(',');
                            for (int i = 0; i < addoncovers.Length; i++)
                            {
                                string covercode = addoncovers[i];
                                if (covercode != "C1" && covercode != "C2" && covercode != "C3" && covercode != "C4" && covercode != "C5")
                                {

                                    if (covercode == "C17")
                                    {
                                        prm.PACoverToUnNamedPerson = Math.Round(fullquote_data[covercode]["premium"].Value<double>());
                                    }
                                    if (covercode == "C35")
                                    {
                                        prm.ZeroDepPremium = Math.Round(fullquote_data[covercode]["premium"].Value<double>());
                                    }
                                    if (covercode == "C37")
                                    {
                                        prm.CostOfConsumablesPremium = Math.Round(fullquote_data[covercode]["premium"].Value<double>());
                                    }
                                    if (covercode == "C38")
                                    {
                                        prm.InvoicePriceCoverPremium = Math.Round(fullquote_data[covercode]["premium"].Value<double>());
                                    }
                                    if (covercode == "C39")
                                    {
                                        prm.NcbProtectorPremium = Math.Round(fullquote_data[covercode]["premium"].Value<double>());
                                    }
                                    if (covercode == "C45")
                                    {
                                        prm.TyreProtect = Math.Round(fullquote_data[covercode]["premium"].Value<double>());
                                    }
                                    if (covercode == "C47")
                                    {
                                        prm.RSAPremium = Math.Round(fullquote_data[covercode]["premium"].Value<double>());
                                    }
                                    if (covercode == "C48")
                                    {
                                        prm.EmergencyAssistancePremium = Math.Round(fullquote_data[covercode]["premium"].Value<double>());
                                    }
                                    if (covercode == "C40")
                                    {
                                        prm.FiberGlassTankPremium = Math.Round(fullquote_data[covercode]["premium"].Value<double>());
                                    }

                                    if (covercode == "C41")
                                    {
                                        prm.LossOfPersonalBelongingPremium = Math.Round(fullquote_data[covercode]["premium"].Value<double>());
                                    }
                                    if (covercode == "C42")
                                    {
                                        prm.EmergencyAssistancePremium = Math.Round(fullquote_data[covercode]["premium"].Value<double>());
                                    }
                                    if (covercode == "C43")
                                    {
                                        prm.KeyReplacementPremium = Math.Round(fullquote_data[covercode]["premium"].Value<double>());
                                    }
                                    if (covercode == "C44")
                                    {
                                        prm.EngineProtectorPremium = Math.Round(fullquote_data[covercode]["premium"].Value<double>());
                                    }
                                    if (covercode == "C18")
                                    {
                                        prm.LLToPaidDriver = Math.Round(fullquote_data[covercode]["premium"].Value<double>());
                                    }
                                    // netaddon = netaddon + fullquote_data[covercode]["premium"].Value<double>();
                                }
                            }

                            resModel.PremiumBreakUpDetails = prm;
                            CompanyWiseRefference reff = new CompanyWiseRefference();
                            reff.QuoteNo = quotation_no;
                            resModel.CompanyWiseRefference = reff;
                            resModel.CompanyName = Company.TATA.ToString();
                            resModel.Status = Status.Success;
                        }

                        else
                        {
                            string errmsg = fullquote_res.Content.ToString();
                            resModel.Status = Status.Fail;
                            resModel.ErrorMsg = errmsg;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                resModel.Status = Status.Fail;
                resModel.ErrorMsg = Convert.ToString(ex.Message);
                Console.Write(Convert.ToString(ex.Message));
                LogU.WriteLog("TATA >> PrivateCar >> GetQuoteRequest >> " + Convert.ToString(ex.Message));

            }
            return resModel;
        }
        public Response GetQuoteResponse(JObject resdata)
        {
            Response resModel = new Response();
            try
            {  //JObject json = JObject.Parse(res);


            }
            catch (Exception ex)
            {
                resModel.Status = Status.Fail;
                resModel.ErrorMsg = ex.ToString();
                throw;
            }
            return resModel;
        }
        public object GetQuoteTata(string idv, string quote_type, Quotation data, string mehod)
        {

            string rto_loc_code = "";
            string rto_loc_name = "";
            string rtolocationgrpcd = "";
            string rto_zone = "";
            string tenure = "", segment_code = "", btype_code = "", btype_name = "", fuel_code = "", covertype_code = "", covertype_name = "", PolicyEndDate = "", PolicyStartDate = ""; ;
            string fuel_name = data.VehicleDetails.Fuel;
            #region get fuel_code

            if (fuel_name == "Petrol")
            {
                fuel_code = "1";
            }
            else if (fuel_name == "Diesel")
            {
                fuel_code = "2";
            }
            else if (fuel_name == "CNG")
            {
                fuel_code = "3";
            }
            else if (fuel_name == "Battery")
            {
                fuel_code = "4";
            }
            else if (fuel_name == "External CNG")
            {
                fuel_code = "5";
            }
            else if (fuel_name == "External LPG")
            {
                fuel_code = "6";
            }
            else if (fuel_name == "Electricity")
            {
                fuel_code = "7";
            }
            else if (fuel_name == "Hydrogen")
            {
                fuel_code = "8";
            }

            #endregion
            #region get segment_code
            string segment_name = data.VehicleDetails.Segment;

            //   VEHICLEC SEGMENT CODE AND NAME
            if (segment_name == "Mini")
            {
                segment_code = "1";
            }
            else if (segment_name == "Compact")
            {
                segment_code = "2";
            }
            else if (segment_name == "Mid Size")
            {
                segment_code = "3";
            }
            else if (segment_name == "High End")
            {
                segment_code = "4";
            }
            else if (segment_name == "MPV SUV")
            {
                segment_code = "5";
            }
            #endregion
            if (data.PolicyType.Equals("New"))
            {
                tenure = "1";

                PolicyStartDate = Convert.ToDateTime(data.PolicyStartDate).ToString("yyyyMMdd");
                PolicyEndDate = Convert.ToDateTime(data.PolicyStartDate).AddYears(3).AddDays(-1).ToString("yyyyMMdd");

                // BUSINESS TYPE
                btype_code = "1";
                btype_name = "New Business";

                covertype_code = "1";
                covertype_name = "Package";
            }

            else
            {
                // BUSINESS TYPE
                btype_code = "2";
                btype_name = "Roll Over";
                tenure = "1";

                covertype_code = "1";
                covertype_name = "Package";


                if (mehod == "Proposal")
                {

                    PolicyStartDate = Convert.ToDateTime(data.PolicyStartDate).ToString("yyyyMMdd");
                    PolicyEndDate = Convert.ToDateTime(data.PolicyEndDate).ToString("yyyyMMdd");

                }
                else
                {
                    PolicyStartDate = Convert.ToDateTime(data.PolicyStartDate).ToString("yyyyMMdd");
                    PolicyEndDate = Convert.ToDateTime(data.PolicyStartDate).AddYears(1).AddDays(-1).ToString("yyyyMMdd");
                }
                if (data.IsThirdPartyOnly == true)
                {
                    tenure = "1";

                    covertype_code = "2";
                    covertype_name = "Liability";



                    if (mehod == "Proposal")
                    {
                        PolicyStartDate = Convert.ToDateTime(data.PolicyStartDate).ToString("yyyyMMdd");
                        PolicyEndDate = Convert.ToDateTime(data.PolicyEndDate).ToString("yyyyMMdd");


                    }
                    else
                    {
                        PolicyStartDate = Convert.ToDateTime(data.PolicyStartDate).ToString("yyyyMMdd");
                    PolicyEndDate = Convert.ToDateTime(data.PolicyStartDate).AddYears(1).AddDays(-1).ToString("yyyyMMdd");
                    }
                }
                else if (data.IsODOnly == true)
                {
                    
                    tenure = "1";
                    covertype_code = "3";
                    covertype_name = "Standalone own damage";
                    if (mehod == "Proposal")
                    {
                        PolicyStartDate = Convert.ToDateTime(data.PolicyStartDate).ToString("yyyyMMdd");
                        PolicyEndDate = Convert.ToDateTime(data.PolicyEndDate).ToString("yyyyMMdd");


                    }
                    else
                    {
                        PolicyStartDate = Convert.ToDateTime(DateTime.Now).AddDays(1).ToString("yyyyMMdd");
                        PolicyEndDate = Convert.ToDateTime(DateTime.Now).AddYears(1).ToString("yyyyMMdd");
                    }

                }


            }
            string path = AppDomain.CurrentDomain.BaseDirectory;
            string filePath = Path.Combine(path, "JSON/TATA/Quote.json");
            string json = File.ReadAllText(filePath);
            dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);


            var Getrtodata = ap.COMPANY_WISE_RTO_MASTER.Where(x => x.companyid == 9 && x.andapp_rtoid == data.VehicleDetails.RtoId).FirstOrDefault();
            if (Getrtodata != null)
            {
                rto_loc_code = Getrtodata.rto_loc_code;
                rto_loc_name = Getrtodata.rto_loc_name;
                rtolocationgrpcd = Getrtodata.rtolocationgrpcd;
                rto_zone = Getrtodata.rto_zone;
            }

            jsonObj["functionality"] = "validatequote";
            jsonObj["quote_type"] = quote_type;
            jsonObj["vehicle"]["quotation_no"] = "";
            jsonObj["vehicle"]["segment_code"] = segment_code;
            jsonObj["vehicle"]["segment_name"] = segment_name;
            jsonObj["vehicle"]["cc"] = data.VehicleDetails.CC.ToString();
            jsonObj["vehicle"]["sc"] = data.VehicleDetails.SC.ToString();
            jsonObj["vehicle"]["sol_id"] = "1001";
            jsonObj["vehicle"]["lead_id"] = "";
            jsonObj["vehicle"]["mobile_no"] = "";
            jsonObj["vehicle"]["email_id"] = "";
            jsonObj["vehicle"]["emp_email_id"] = "";
            jsonObj["vehicle"]["customer_type"] = data.CustomerType;
            jsonObj["vehicle"]["product_code"] = "3121";
            jsonObj["vehicle"]["product_name"] = "Private Car";
            jsonObj["vehicle"]["subproduct_code"] = "45";
            jsonObj["vehicle"]["subproduct_name"] = "Private Car";
            jsonObj["vehicle"]["subclass_code"] = "";
            jsonObj["vehicle"]["subclass_name"] = "";
            jsonObj["vehicle"]["covertype_code"] = covertype_code;
            jsonObj["vehicle"]["covertype_name"] = covertype_name;
            jsonObj["vehicle"]["btype_code"] = btype_code;
            jsonObj["vehicle"]["btype_name"] = btype_name;
            jsonObj["vehicle"]["risk_startdate"] = PolicyStartDate;
            jsonObj["vehicle"]["risk_enddate"] = PolicyEndDate;
            jsonObj["vehicle"]["purchase_date"] = Convert.ToDateTime(data.VehicleDetails.RegistrationDate).ToString("yyyyMMdd");
            jsonObj["vehicle"]["veh_age"] = "";
            jsonObj["vehicle"]["manf_year"] = Convert.ToDateTime(data.VehicleDetails.ManufaturingDate).ToString("yyyy");
            jsonObj["vehicle"]["make_code"] = data.VehicleDetails.MakeCode;
            jsonObj["vehicle"]["make_name"] = data.VehicleDetails.MakeName;
            jsonObj["vehicle"]["model_code"] = data.VehicleDetails.ModelCode;
            jsonObj["vehicle"]["model_name"] = data.VehicleDetails.ModelName;
            jsonObj["vehicle"]["variant_code"] = data.VehicleDetails.VariantCode;
            jsonObj["vehicle"]["variant_name"] = data.VehicleDetails.VariantName;
            jsonObj["vehicle"]["model_parent_code"] = data.VehicleDetails.ModelCode;

            jsonObj["vehicle"]["fuel_code"] = fuel_code;
            jsonObj["vehicle"]["fuel_name"] = fuel_name;
            jsonObj["vehicle"]["gvw"] = "";
            jsonObj["vehicle"]["age"] = "0";
            jsonObj["vehicle"]["miscdtype_code"] = "";
            jsonObj["vehicle"]["bodytype_id"] = data.VehicleDetails.BodyType;
            jsonObj["vehicle"]["idv"] = idv;
            jsonObj["vehicle"]["revised_idv"] = idv;




            string registrationno = "NEW";            string part1 = "";            string part2 = "";            string part3 = "";            string part4 = "";            if (data.PolicyType != "NEW" && data.PolicyType != "New")            {                string rgno = data.VehicleDetails != null ? !string.IsNullOrEmpty(data.VehicleDetails.RegistrationNumber) ? data.VehicleDetails.RegistrationNumber : "MH02AB9685" : "MH02AB9685";                int rglenght = rgno.Length - 4;                string subregno = rgno.Substring(4, rglenght);
                part1 = rgno.Substring(0, 2);
                part2 = rgno.Substring(2, 2);
                part3 = subregno.Substring(0, subregno.Length - 4);
                part4 = subregno.Substring(subregno.Length - 4);                registrationno = part1 + "-" + part2 + "-" + part3 + "-" + part4;                data.VehicleDetails.RegistrationNumber = registrationno;            }            else            {                data.VehicleDetails.RegistrationNumber = registrationno;            }            string rtocombo = "";            rtocombo = part1 + "-" + part2;            jsonObj["vehicle"]["regno_1"] = part1;            jsonObj["vehicle"]["regno_2"] = part2;            jsonObj["vehicle"]["regno_3"] = part3;            jsonObj["vehicle"]["regno_4"] = part4;


            jsonObj["vehicle"]["rto_loc_code"] = rto_loc_code;
            jsonObj["vehicle"]["rto_loc_name"] = rto_loc_name;
            jsonObj["vehicle"]["rtolocationgrpcd"] = rtolocationgrpcd;
            jsonObj["vehicle"]["rto_zone"] = rto_zone;



            jsonObj["vehicle"]["regno_1"] = "GJ";
            jsonObj["vehicle"]["regno_2"] = "01";
            jsonObj["vehicle"]["regno_3"] = "ab";
            jsonObj["vehicle"]["regno_4"] = "0123";
             
            jsonObj["vehicle"]["rto_loc_code"] = "GJ-01";
            jsonObj["vehicle"]["rto_loc_name"] = "AHMEDABAD GJ-01";
            jsonObj["vehicle"]["rtolocationgrpcd"] = "12";
            jsonObj["vehicle"]["rto_zone"] = data.VehicleDetails.RtoZone;



            jsonObj["vehicle"]["rating_logic"] = "";
            jsonObj["vehicle"]["campaign_id"] = "";
            jsonObj["vehicle"]["fleet_id"] = "";
            jsonObj["vehicle"]["discount_perc"] = "";

            if (data.PolicyType != "New")
            {

                jsonObj["vehicle"]["pp_covertype_code"] = data.PreviousPolicyDetails.PreviousPolicyType.Equals("Comprehensive") ? "1" : "2";
                jsonObj["vehicle"]["pp_covertype_name"] = data.PreviousPolicyDetails.PreviousPolicyType.Equals("Comprehensive") ? "Package" : "Liability";

                if (data.DontKnowPreviousInsurer == true)
                {
                    jsonObj["vehicle"]["pp_enddate"] = Convert.ToDateTime(DateTime.Now).AddDays(-2).ToString("yyyyMMdd");

                }
                else
                {
                    jsonObj["vehicle"]["pp_enddate"] = Convert.ToDateTime(data.PreviousPolicyDetails.PreviousPolicyEndDate).ToString("yyyyMMdd");
                }

                jsonObj["vehicle"]["pp_claim_yn"] = data.PreviousPolicyDetails.IsPreviousInsuranceClaimed.Equals(true) ? "1" : "0"; ;
                jsonObj["vehicle"]["pp_prev_ncb"] = data.PreviousPolicyDetails.PreviousNcbPercentage;
                if (data.IsThirdPartyOnly == true)
                {
                    jsonObj["vehicle"]["pp_curr_ncb"] = "";


                }
                else
                {
                    jsonObj["vehicle"]["pp_curr_ncb"] = data.CurrentNcb;
                }

            }

            jsonObj["vehicle"]["addon_plan_code"] = "";
            jsonObj["vehicle"]["addon_choice_code"] = "CHOICE1";
            jsonObj["vehicle"]["cust_name"] = "";
            jsonObj["vehicle"]["ab_cust_id"] = "";
            jsonObj["vehicle"]["ab_emp_id"] = "";
            jsonObj["vehicle"]["usr_name"] = "";
            jsonObj["vehicle"]["producer_code"] = "";
            jsonObj["vehicle"]["pup_check"] = "N";
            jsonObj["vehicle"]["pos_panNo"] = "";
            jsonObj["vehicle"]["pos_aadharNo"] = "";
            jsonObj["vehicle"]["is_cust_JandK"] = "NO";

            jsonObj["vehicle"]["cust_pincode"] = "380001";
            jsonObj["vehicle"]["cust_gstin"] = "";
            jsonObj["vehicle"]["tenure"] = tenure;
            jsonObj["vehicle"]["uw_discount"] = "";
            jsonObj["vehicle"]["Uw_DisDb"] = "";
            jsonObj["vehicle"]["uw_load"] = "";
            jsonObj["vehicle"]["uw_loading_discount"] = "0";
            jsonObj["vehicle"]["uw_loading_discount_flag"] = "D";
            jsonObj["vehicle"]["engine_no"] = data.VehicleDetails.EngineNumber;
            jsonObj["vehicle"]["chasis_no"] = data.VehicleDetails.ChassisNumber;

            if (data.IsThirdPartyOnly != true)
            {     //Basic OD
                jsonObj["cover"]["C1"]["opted"] = "Y";

                //Roadside Assistance 
                jsonObj["cover"]["C47"]["opted"] = "Y";
            }



            if (data.IsODOnly != true)
            {
                //Basic TP
                jsonObj["cover"]["C2"]["opted"] = "Y";
                //PA Cover for Owner Driver    



               
                    jsonObj["cover"]["C3"]["opted"] = "Y";
              


            }
            else
            {  //Basic OD
                jsonObj["cover"]["C1"]["opted"] = "Y";
                //Basic TP
                jsonObj["cover"]["C2"]["opted"] = "N";
                //PA Cover for Owner Driver                                    
               
                    jsonObj["cover"]["C3"]["opted"] = "N";
              
                   
                

                jsonObj["vehicle"]["tppolicytype"] = "Comprehensive Package";

                jsonObj["vehicle"]["tppolicytenure"] = "3";
            }





            if (quote_type == "full")
            {
                //Electrical Accessories  Package  Only
                if (data.CoverageDetails.IsElectricalAccessories == true)
                {
                    jsonObj["cover"]["C4"]["opted"] = "Y";
                    jsonObj["cover"]["C4"]["SI"] = data.CoverageDetails.SIElectricalAccessories;
                }



                //Non Electrical Accessories Package  Only
                if (data.CoverageDetails.IsNonElectricalAccessories == true)
                {
                    jsonObj["cover"]["C5"]["opted"] = "Y";
                    jsonObj["cover"]["C5"]["SI"] = data.CoverageDetails.SINonElectricalAccessories;
                }


                //Side Car
                jsonObj["cover"]["C6"]["opted"] = "N";
                jsonObj["cover"]["C6"]["SI"] = "";


                //Bi-Fuel Kit  Package  / Liability 
                if (data.CoverageDetails.IsBiFuelKit == true)
                {

                    if (data.IsThirdPartyOnly==true)
                    {
                        jsonObj["cover"]["C29"]["opted"] = "Y";
                        jsonObj["cover"]["C29"]["SI"] = data.CoverageDetails.BiFuelKitAmount;
                    }
                    else
                    {
                        jsonObj["cover"]["C7"]["opted"] = "Y";
                        jsonObj["cover"]["C7"]["SI"] = data.CoverageDetails.BiFuelKitAmount;
                    }
                   


                    jsonObj["vehicle"]["fuel_code"] = "5";
                    jsonObj["vehicle"]["fuel_name"] = "External CNG";
                }


                //Automobile Association Membership

                jsonObj["cover"]["C8"]["opted"] = "N";

                //Voluntary Deductibles
                if (data.DiscountDetails.IsVoluntaryExcess == true)
                {
                    jsonObj["cover"]["C10"]["opted"] = "Y";
                    jsonObj["cover"]["C10"]["SI"] = data.DiscountDetails.VoluntaryExcessAmount;
                }

                //Anti-Theft Device   
                if (data.DiscountDetails.IsAntiTheftDevice == true)
                {
                    jsonObj["cover"]["C11"]["opted"] = "Y";
                }

                //TPPD Restricted    
                if (data.DiscountDetails.IsTPPDRestrictedto6000 == true)
                {
                    jsonObj["cover"]["C12"]["opted"] = "Y";
                }

                //Geographical Extension
                jsonObj["cover"]["C13"]["opted"] = "N";

                //Geographical Extension - TP
                jsonObj["cover"]["C14"]["opted"] = "N";

                jsonObj["cover"]["C15"]["opted"] = "N";
                jsonObj["cover"]["C15"]["perc"] = "";

                //PA Cover for Unnamed Person                             
                if (data.CoverageDetails.IsPACoverUnnamedPerson == true)
                {
                    jsonObj["cover"]["C17"]["opted"] = "Y";
                    jsonObj["cover"]["C17"]["SI"] = data.CoverageDetails.PACoverUnnamedPersonAmount;
                    jsonObj["cover"]["C17"]["persons"] = data.VehicleDetails.SC;

                }

                //Legal Liability to Paid Driver
                if (data.CoverageDetails.IsLegalLiablityPaidDriver == true)
                {
                    jsonObj["cover"]["C18"]["opted"] = "Y";
                    jsonObj["cover"]["C18"]["persons"] = data.CoverageDetails.NoOfLLPaidDriver;
                }



                //CNG/LPG Kit - Tp
                jsonObj["cover"]["C29"]["opted"] = "N";
                //jsonObj["cover"]["C48"]["opted"] = "N";
                //jsonObj["cover"]["C48"]["SI"] = null;
                //Additional PA cover to Owner - Driver
                if (data.CoverageDetails.IsPACoverPaidDriver == true)
                {
                    jsonObj["cover"]["C50"]["opted"] = "Y";
                    jsonObj["cover"]["C50"]["SI"] = null;
                }
                //Additional PA cover to Unnamed Passenger
                //if (data.CoverageDetails.IsPACoverUnnamedPerson == true)
                //{
                //    jsonObj["cover"]["C51"]["opted"] = "Y";
                //    jsonObj["cover"]["C51"]["SI"] = data.CoverageDetails.PACoverUnnamedPersonAmount;
                //}
                //jsonObj["cover"]["C35"]["no_of_claims"] = "2";
                //jsonObj["cover"]["C35"]["Deductibles"] = "0";

                if (data.AddonCover != null)
                {   //Depreciation Re-imbursement
                    if (data.AddonCover.IsZeroDeperation == true)
                    {
                        jsonObj["cover"]["C35"]["opted"] = "Y";
                    }
                    else
                    {
                        jsonObj["cover"]["C35"]["opted"] = "N";
                    }
                    //Consumables expenses
                    if (data.AddonCover.IsConsumables == true)
                    {
                        jsonObj["cover"]["C37"]["opted"] = "Y";
                    }
                    else
                    {
                        jsonObj["cover"]["C37"]["opted"] = "N";
                    }
                    //Emergency Medical Expenses


                    if (data.AddonCover.IsEmergencyCover == true)
                    {
                        jsonObj["cover"]["C49"]["opted"] = "Y";
                        jsonObj["cover"]["C49"]["SI"] = null;
                    }
                    else
                    {
                        jsonObj["cover"]["C49"]["opted"] = "N";
                        jsonObj["cover"]["C49"]["SI"] = null;
                    }
                    //Roadside Assistance
                    if (data.AddonCover.IsRoadSideAssistance == true)
                    {
                        jsonObj["cover"]["C47"]["opted"] = "Y";
                    }
                    else
                    {
                        jsonObj["cover"]["C47"]["opted"] = "N";

                    }

                    // NCB Protection Cover
                    if (data.AddonCover.IsNCBProtection == true)
                    {
                        jsonObj["cover"]["C39"]["opted"] = "Y";
                    }
                    else
                    {
                        jsonObj["cover"]["C39"]["opted"] = "N";
                    }

                    //Return to Invoice
                    if (data.AddonCover.IsReturntoInvoice == true)
                    {
                        jsonObj["cover"]["C38"]["opted"] = "Y";
                    }
                    else
                    {
                        jsonObj["cover"]["C38"]["opted"] = "N";
                    }
                    ////Tyre Secure
                    if (data.AddonCover.IsTyreCover == true)
                    {

                        jsonObj["cover"]["C45"]["opted"] = "Y";
                    }
                    else
                    {
                        jsonObj["cover"]["C45"]["opted"] = "N";
                    }




                    ////IsLossofpersonalBelonging

                    if (data.AddonCover.IsLossofpersonalBelonging == true)
                    {

                        jsonObj["cover"]["C41"]["opted"] = "Y";
                    }
                    else
                    {
                        jsonObj["cover"]["C41"]["opted"] = "N";
                    }

                    ////Key Replacement

                    if (data.AddonCover.IsLossofKey == true)
                    {

                        jsonObj["cover"]["C43"]["opted"] = "Y";
                    }
                    else
                    {
                        jsonObj["cover"]["C43"]["opted"] = "N";
                    }

                    //Engine Secure
                    if (data.AddonCover.IsEngineProtector == true)
                    {

                        jsonObj["cover"]["C44"]["opted"] = "Y";
                    }
                    else
                    {
                        jsonObj["cover"]["C44"]["opted"] = "N";
                    }


                }

            }


            string requestjson = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);





            return requestjson;
        }


        /// <summary>
        /// Proposal request method.
        /// </summary>
        /// <param name="model">Object of quotation model.</param>
        /// <returns>return response type object.</returns>
        public Response GetProposalRequest(Quotation model)
        {
            string quotation_no = "";
            long rsawithoutgst = 0;
            CompanyWiseRefference compreff = new CompanyWiseRefference();
            Response resModel = new Response();



            var fullquote_obj = GetQuoteTata(model.IDV.ToString(), "full", model, "Proposal");
            var fullquote_res = Webservicecall(fullquote_obj, "" + ConfigurationManager.AppSettings["TATAquotation"] + "", "quotation", model.enquiryid);
            if (fullquote_res.StatusCode == HttpStatusCode.OK)
            {
                resModel.FreeAddonCover = new AddonCover();
                var fullquote_resdata = (JObject)JsonConvert.DeserializeObject(fullquote_res.Content.ToString());

                var fullquote_data = fullquote_resdata["data"];
                var fullquote_tax = fullquote_data["TAX"];
                var quotation_data = fullquote_data["quotationdata"];
                var proposal_status = fullquote_data["status"].Value<string>();

                quotation_no = quotation_data["quotation_no"].Value<string>();

                PremiumBreakUpDetails prm = new PremiumBreakUpDetails();

                if (model.IsODOnly != true)
                {
                    prm.BasicThirdPartyLiability = Math.Round(fullquote_data["C2"]["premium"].Value<double>());
                    prm.PACoverToOwnDriver = Math.Round(fullquote_data["C3"]["premium"].Value<double>());

                }
                if (model.IsODOnly==true)
                {
                    prm.BasicODPremium = Math.Round(fullquote_data["C1"]["premium"].Value<double>());

                }
             
                prm.NetODPremium = Math.Round(fullquote_data["NETOD"].Value<double>());

                if (fullquote_data["NETADDON"].Value<double>() != 0)
                {
                    //var rsaG= fullquote_data["C47"]["premium"].Value<double>() * 0.18;
                    rsawithoutgst = (long)((fullquote_data["C47"]["premium"].Value<double>() * 100) / 118);
                    prm.NetAddonPremium = fullquote_data["NETADDON"].Value<double>() + rsawithoutgst;
                    prm.NetPremium = fullquote_data["NETPREM"].Value<double>() + rsawithoutgst;

                    prm.ServiceTax = fullquote_tax["total_prem"].Value<double>() + (fullquote_data["C47"]["premium"].Value<double>() - rsawithoutgst);
                    resModel.FreeAddonCover.RepairFiberGlass = true;
                }
                else
                {
                    if (model.AddonCover.IsRoadSideAssistance == true)
                    {
                        rsawithoutgst = (long)((fullquote_data["C47"]["premium"].Value<double>() * 100) / 118);
                        prm.NetAddonPremium = fullquote_data["NETADDON"].Value<double>() + rsawithoutgst;
                        prm.NetPremium = fullquote_data["NETPREM"].Value<double>() + rsawithoutgst;

                        prm.ServiceTax = fullquote_tax["total_prem"].Value<double>() + (fullquote_data["C47"]["premium"].Value<double>() - rsawithoutgst);
                        resModel.FreeAddonCover.RepairFiberGlass = true;
                    }
                    else
                    {
                        prm.NetAddonPremium = fullquote_data["NETADDON"].Value<double>();
                        prm.NetPremium = fullquote_data["NETPREM"].Value<double>();
                        prm.ServiceTax = fullquote_tax["total_prem"].Value<double>();

                    }


                }
                //prm.NetAddonPremium = fullquote_data["NETADDON"].Value<double>();
                prm.NetTPPremium = Math.Round(fullquote_data["NETTP"].Value<double>());
                //prm.NetPremium = fullquote_data["NETPREM"].Value<double>();
               // prm.ServiceTax = fullquote_tax["total_prem"].Value<double>();
                prm.NCBDiscount = Math.Round(fullquote_data["C15"]["premium"].Value<double>());
                resModel.EnquiryId = de.GenerateEnquiryId();
                resModel.FinalPremium = fullquote_data["TOTALPAYABLE"].Value<int>();

                resModel.IDV = Convert.ToInt32(model.IDV);

                //resModel.PolicyStartDate = quotation_data["risk_startdate"].Value<string>();
                //resModel.PolicyEndDate = quotation_data["risk_enddate"].Value<string>();

                resModel.CC = quotation_data["cc"].Value<int>();
                resModel.SC = quotation_data["sc"].Value<int>();
                resModel.FuelType = quotation_data["fuel_name"].Value<string>();

                var coveropted = quotation_data["cover_opted"].Value<string>();
                var od_covers = quotation_data["od_covers"].Value<string>();
                var addon_covers = quotation_data["addon_covers"].Value<string>();
                var tp_covers = quotation_data["tp_covers"].Value<string>();

                //double netod = 0.0;
                //// OD COVERS FOR LOOP
                //var odcovers = od_covers.ToString().Split(',');
                //for (int i = 0; i < odcovers.Length; i++)
                //{
                //    string covercode = odcovers[i];
                //    netod = netod+ fullquote_data[covercode]["premium"].Value<double>();
                //}
                //double netaddon = 0.0;
                // ADD ON  COVERS FOR LOOP
                if (model.CoverageDetails.IsElectricalAccessories == true)
                {
                    prm.ElecAccessoriesPremium = Math.Round(fullquote_data["C4"]["premium"].Value<double>());
                }

                if (model.CoverageDetails.IsNonElectricalAccessories == true)
                {
                    prm.NonElecAccessoriesPremium = Math.Round(fullquote_data["C5"]["premium"].Value<double>());
                }
                if (model.CoverageDetails.IsBiFuelKit == true)
                {
                    if (model.IsThirdPartyOnly == true)
                    {
                        prm.TPCNGLPGPremium = Math.Round(fullquote_data["C29"]["premium"].Value<double>());
                    }
                    else
                    {
                        prm.CNGLPGKitPremium = Math.Round(fullquote_data["C7"]["premium"].Value<double>());
                        if (model.IsODOnly == true)
                        {

                        }
                        else
                        {
                            prm.TPCNGLPGPremium = Math.Round(fullquote_data["C29"]["premium"].Value<double>());

                        }
                    }

                }
                var addoncovers = coveropted.ToString().Split(',');
                for (int i = 0; i < addoncovers.Length; i++)
                {
                    string covercode = addoncovers[i];
                    if (covercode != "C1" && covercode != "C2" && covercode != "C3" && covercode != "C4" && covercode != "C5")
                    {
                        if (covercode == "C17")
                        {
                            prm.PACoverToUnNamedPerson = Math.Round(fullquote_data[covercode]["premium"].Value<double>());
                        }
                        if (covercode == "C35")
                        {
                            prm.ZeroDepPremium =Math.Round(fullquote_data[covercode]["premium"].Value<double>());
                        }
                        if (covercode == "C37")
                        {
                            prm.CostOfConsumablesPremium = Math.Round(fullquote_data[covercode]["premium"].Value<double>());
                        }
                        if (covercode == "C38")
                        {
                            prm.InvoicePriceCoverPremium = Math.Round(fullquote_data[covercode]["premium"].Value<double>());
                        }
                        if (covercode == "C39")
                        {
                            prm.NcbProtectorPremium = Math.Round(fullquote_data[covercode]["premium"].Value<double>());
                        }
                        if (covercode == "C45")
                        {
                            prm.TyreProtect = Math.Round(fullquote_data[covercode]["premium"].Value<double>());
                        }
                        if (covercode == "C47")
                        {
                            prm.RSAPremium = Math.Round(fullquote_data[covercode]["premium"].Value<double>());
                        }
                        if (covercode == "C48")
                        {
                            prm.EmergencyAssistancePremium = Math.Round(fullquote_data[covercode]["premium"].Value<double>());
                        }
                        if (covercode == "C40")
                        {
                            prm.FiberGlassTankPremium = Math.Round(fullquote_data[covercode]["premium"].Value<double>());
                        }

                        if (covercode == "C41")
                        {
                            prm.LossOfPersonalBelongingPremium = Math.Round(fullquote_data[covercode]["premium"].Value<double>());
                        }
                        if (covercode == "C42")
                        {
                            prm.EmergencyAssistancePremium = Math.Round(fullquote_data[covercode]["premium"].Value<double>());
                        }
                        if (covercode == "C43")
                        {
                            prm.KeyReplacementPremium = Math.Round(fullquote_data[covercode]["premium"].Value<double>());
                        }
                        if (covercode == "C44")
                        {
                            prm.EngineProtectorPremium = Math.Round(fullquote_data[covercode]["premium"].Value<double>());
                        }
                        if (covercode == "C18")
                        {
                            prm.LLToPaidDriver = Math.Round(fullquote_data[covercode]["premium"].Value<double>());
                        }
                        // netaddon = netaddon + fullquote_data[covercode]["premium"].Value<double>();
                    }
                }
                // TP  COVERS FOR LOOP
                //double nettp = 0.0;
                //var tpcovers = tp_covers.ToString().Split(',');
                //for (int i = 0; i < tpcovers.Length; i++)
                //{
                //    string covercode = tpcovers[i];
                //    nettp = nettp + fullquote_data[covercode]["premium"].Value<double>();
                //}
                resModel.PremiumBreakUpDetails = prm;

                compreff.QuoteNo = quotation_no;
                resModel.CompanyWiseRefference = compreff;
                resModel.CompanyName = Company.TATA.ToString();
                resModel.Status = Status.Success;


            }

            string path = AppDomain.CurrentDomain.BaseDirectory;
            string filePath = Path.Combine(path, "JSON/TATA/Proposal.json");
            string json = File.ReadAllText(filePath);
            dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
            try
            {
                if (model.IsODOnly == true)
                {
                    jsonObj["bundpolicy"]["flag"] = "Y";
                    jsonObj["bundpolicy"]["code"] = "ICICI";
                    jsonObj["bundpolicy"]["name"] = "ICICI";
                    jsonObj["bundpolicy"]["bp_no"] = "ABCD123";

                    jsonObj["bundpolicy"]["bp_edate"] = Convert.ToDateTime(model.PreviousPolicyDetails.PreviousPolicyEndDate).ToString("yyyyMMdd");


                    jsonObj["bundpolicy"]["op_sdate"] = Convert.ToDateTime(model.PreviousPolicyDetails.PreviousPolicyEndDate).AddYears(-1).AddDays(-1).ToString("yyyyMMdd");
                    jsonObj["bundpolicy"]["op_edate"] = Convert.ToDateTime(model.PreviousPolicyDetails.PreviousPolicyEndDate).ToString("yyyyMMdd");

                    jsonObj["bundpolicy"]["tp_polnum"] = model.PreviousTPPolicyDetails.PolicyNo;
                    jsonObj["bundpolicy"]["tp_pol_sdate"] = Convert.ToDateTime(model.PreviousTPPolicyDetails.PolicyEndDate).AddYears(-3).AddDays(+1).ToString("yyyyMMdd");
                    jsonObj["bundpolicy"]["tp_pol_edate"] = Convert.ToDateTime(model.PreviousTPPolicyDetails.PolicyEndDate).ToString("yyyyMMdd");
                   


                }

                jsonObj["functionality"] = "validateproposal";
                jsonObj["quotation_no"] = quotation_no;
                jsonObj["pol_sdate"] = Convert.ToDateTime(model.PolicyStartDate).ToString("yyyyMMdd");

                jsonObj["customer"]["salutation"] = model.ClientDetails.Salutation;
                jsonObj["customer"]["client_type"] = model.CustomerType;
                jsonObj["customer"]["first_name"] = model.ClientDetails.FirstName;
                jsonObj["customer"]["middle_name"] = model.ClientDetails.MiddleName;
                jsonObj["customer"]["last_name"] = model.ClientDetails.LastName;
                jsonObj["customer"]["gender"] = model.ClientDetails.Gender;
                jsonObj["customer"]["dob"] = Convert.ToDateTime(model.ClientDetails.DateOfBirth).ToString("yyyyMMdd");
                jsonObj["customer"]["marital_status"] = model.ClientDetails.MaritalStatus;
                jsonObj["customer"]["address_1"] = model.CustomerAddressDetails.Address1;
                jsonObj["customer"]["address_2"] = model.CustomerAddressDetails.Address2;
                jsonObj["customer"]["address_3"] = model.CustomerAddressDetails.Address3;
                jsonObj["customer"]["address_4"] = "";
                jsonObj["customer"]["pincode"] = model.CustomerAddressDetails.Pincode;
                jsonObj["customer"]["account_no"] = "";
                jsonObj["customer"]["cust_aadhaar"] = model.ClientDetails.AadharNo;
                jsonObj["customer"]["mobile_no"] = model.ClientDetails.MobileNo;
                jsonObj["customer"]["email_id"] = model.ClientDetails.EmailId;

                jsonObj["vehicle"]["engine_no"] = model.VehicleDetails.EngineNumber;
                jsonObj["vehicle"]["chassis_no"] = model.VehicleDetails.ChassisNumber;


                if (model.VehicleDetails.IsVehicleLoan == true)
                {
                    jsonObj["financier"]["name"] = model.VehicleDetails.LoanCompanyName;
                    jsonObj["financier"]["type"] = "Bank";
                }

               

                jsonObj["nominee"]["name"] = model.NomineeName;
                jsonObj["nominee"]["age"] = string.IsNullOrEmpty(model.NomineeDateOfBirth) ? "0" : Convert.ToString(CalculateAge(Convert.ToDateTime(model.NomineeDateOfBirth)));
                jsonObj["nominee"]["relation"] = model.NomineeRelationShip;

                if (model.PolicyType != "New")
                {


                    jsonObj["prevpolicy"]["flag"] = "Y";
                    jsonObj["prevpolicy"]["code"] = "GO DIGIT";
                    jsonObj["prevpolicy"]["name"] = "GO DIGIT GENERAL INSURANCE CO LTD ";
                    jsonObj["prevpolicy"]["address1"] = "Test";
                    jsonObj["prevpolicy"]["address2"] = "Test";
                    jsonObj["prevpolicy"]["address3"] = "Test";
                    jsonObj["prevpolicy"]["polno"] = model.PreviousPolicyDetails.PreviousPolicyNo;
                }


                string requestjsonprop = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
                var proposal_res = Webservicecall(requestjsonprop, "" + ConfigurationManager.AppSettings["TATAproposal"] + "", "proposal", model.enquiryid);
                if (proposal_res.StatusCode == HttpStatusCode.OK)
                {
                    var proposal_resdata = (JObject)JsonConvert.DeserializeObject(proposal_res.Content.ToString());



                    var data = proposal_resdata["data"];

                    var propstatus = data["status"].ToString();
                    if (propstatus != "0")
                    {
                        compreff.QuoteNo = data["quotationno"].Value<string>();
                        compreff.OrderNo = data["proposalno"].Value<string>();
                        resModel.CompanyWiseRefference = compreff;
                        resModel.Status = Status.Success;
                        ap.SP_Payment_Parameter(model.enquiryid, 22, "quotationno", data["quotationno"].Value<string>());
                        ap.SP_Payment_Parameter(model.enquiryid, 22, "proposalno", data["proposalno"].Value<string>());


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
                        ap.SP_POLICYDETAILSMASTER("I", model.enquiryid, 22, model.pospid, model.CustomerType, model.PolicyType, producttype,
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

                        //   string customeraddress = model.CustomerAddressDetails.Address1 + " " + model.CustomerAddressDetails.Address2 + " " + model.CustomerAddressDetails.Address3 + " " + model.CustomerAddressDetails.Pincode;
                        //      ap.SP_POLICYDETAILSMASTER("I", model.enquiryid, 22, model.pospid, model.CustomerType, model.PolicyType,
                        //model.ClientDetails.FirstName, model.ClientDetails.MiddleName, model.ClientDetails.LastName,
                        //model.ClientDetails.EmailId, Convert.ToDateTime(model.ClientDetails.DateOfBirth), customeraddress,
                        //model.ClientDetails.PanCardNo, model.ClientDetails.GSTIN, model.ClientDetails.MobileNo,
                        //Convert.ToInt64(model.VehicleDetails.MakeCode), Convert.ToInt64(model.VehicleDetails.ModelCode),
                        //model.VehicleDetails.VariantId, model.VehicleDetails.RegistrationNumber, model.VehicleDetails.EngineNumber,
                        //model.VehicleDetails.ChassisNumber, null, Convert.ToDateTime(model.PolicyStartDate),
                        //Convert.ToDateTime(model.PolicyEndDate), null, null, 1,
                        //Convert.ToDecimal(model.IDV), Convert.ToDecimal(model.PremiumDetails.OdPremiumAmount),
                        //Convert.ToDecimal(model.PremiumDetails.TpPremiumAmount),
                        //Convert.ToDecimal(model.PremiumDetails.NetPremiumAmount),
                        //Convert.ToDecimal(model.PremiumDetails.TaxAmount),
                        //Convert.ToDecimal(model.PremiumDetails.TotalPremiumAmount), false);

                    }
                    else
                    {
                        resModel.Status = Status.Fail;
                        resModel.ErrorMsg = data["message"].ToString();

                    }

                }


            }
            catch (Exception ex)
            {
                resModel.Status = Status.Fail;
                resModel.ErrorMsg = Convert.ToString(ex.Message);
                Console.Write(Convert.ToString(ex.Message));
                LogU.WriteLog("TATA >> PrivateCar >> GetProposalRequest >> " + Convert.ToString(ex.Message));
                throw;
            }
            return resModel;
        }


        public string GetPaymentParameter(PaymentRequest model)
        {
            string result;
            try
            {
                string proposal_no = "";
                proposal_no = model.CompanyDetail.OrderNo;
                result = "https://pipuat.tataaiginsurance.in/tagichubws/cpirequest.jsp?proposal_no=" + proposal_no;
                result = result + "&src=TP";
            }
            catch (Exception ex)
            {

                result = ex.ToString();
                throw;
            }
            return result;
        }



        public IRestResponse Webservicecall(object obj, string baseurl, string method, string enqno)
        {
            RestClient client = new RestClient(baseurl);


            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(delegate { return true; });
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
            var request = new RestRequest("/" + method + "/", Method.POST);
            request.RequestFormat = DataFormat.Json;

            //var json = new JavaScriptSerializer().Serialize(obj);

            request.AddParameter("T", "2D9D1BC5A837E7A2741C6121317E9EE6CE1D32145CBCF7084FA4493ECDA2C2804969A5473610BC2AB4FC034359C11D55F99F8AEC736D84F0EFD531DFE24FFC74F0923F1288A83121B8045A8AAA4D9F920B4D737E3A1134B824E23B1F0561D97AEA647554A31570720BDB6E4CE3D8813A1138ABF16F2A23A8E6BAB012DD07B768019A5B583351F6D36C1F6F26B5C8D474D2F701E664A96F73806EE3A5235DEFFD76CF4106F7F074A55258D75B1DDEFD38");
            request.AddParameter("SRC", "TP");

            if (method == "quotation")
            {
                request.AddParameter("QDATA", obj);
                request.AddParameter("productid", "3121");
            }
            else
            {
                request.AddParameter("product_code", "3121");
                request.AddParameter("PDATA", obj);
                request.AddParameter("THANKYOU_URL", ConfigurationManager.AppSettings["TATAthankyouurl"].ToString() + enqno);
            }

            var response = client.Execute(request);

            return response;
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

public class OdDetails
{
    public string persons { get; set; }
    public string premium { get; set; }
    public string rate { get; set; }
    public string SI { get; set; }
    public string name { get; set; }
    public string type { get; set; }
}