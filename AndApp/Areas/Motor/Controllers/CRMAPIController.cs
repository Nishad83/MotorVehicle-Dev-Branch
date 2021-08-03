using AndApp.Models;
using AndApp.Utilities;
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
using System.Web.Mvc;
using static AndApp.Utilities.Common;

namespace AndApp.Areas.Motor.Controllers
{
    public class CRMAPIController : Controller
    {
        DAL.ANDAPPEntities ap = new DAL.ANDAPPEntities();
        [HttpGet]
        public string gettoken()
        {
            string crm_token = "";
           
            var token_client = new RestClient(ConfigurationManager.AppSettings["crmtoken"].ToString());

            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(delegate { return true; });
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;

            var token_request = new RestRequest("token", Method.POST);

            token_request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            token_request.AddHeader("Accept", "application/json");
            token_request.AddParameter("grant_type", "password");
            token_request.AddParameter("username", "andapp");
            token_request.AddParameter("password", "andapp_auth@2021");




            var token_response = token_client.Post(token_request);

            if (token_response.StatusCode == HttpStatusCode.OK)
            {
                var token_data = (JObject)JsonConvert.DeserializeObject(token_response.Content.ToString());
                crm_token = token_data["access_token"].Value<string>();
            }
            return crm_token;
        }

        [HttpGet]
        public string insertdata(AddCRMData Model)
        {
            string status = "";
            try
            {
 
            string crm_token = gettoken();
            LogU.WriteLog("hello ganesha ");
            var httpWebRequest = (HttpWebRequest)WebRequest.CreateHttp(ConfigurationManager.AppSettings["crmaddapi"].ToString());
            httpWebRequest.ContentType = "application/json ; charset=utf-8";
            httpWebRequest.Method = "POST";
            httpWebRequest.Headers.Add("Authorization", "Bearer " + crm_token);
            string path = AppDomain.CurrentDomain.BaseDirectory;
            string filePath = Path.Combine(path, "JSON/crm/adddata.json");
            string json = System.IO.File.ReadAllText(filePath);
            dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);

           
              jsonObj["companyid"] = Model.companyid;
            jsonObj["customermemberid"] = Model.customermemberid;
            jsonObj["businesstype"] = Model.businesstype;
            jsonObj["pospid"] = Model.pospid;
            jsonObj["producttype"] = Model.producttype;
            jsonObj["policyno"] = Model.policyno;
            jsonObj["issuedate"] = Model.issuedate;
            jsonObj["startdate"] = Model.startdate;
            jsonObj["enddate"] = Model.enddate;
            jsonObj["registrationno"] = Model.registrationno;
            jsonObj["engineno"] = Model.engineno;
            jsonObj["chasisno"] = Model.chasisno;
            jsonObj["tenure"] = Model.tenure;
            jsonObj["ccid"] = Model.ccid;
            jsonObj["manufacturingyear"] = Model.manufacturingyear;
            jsonObj["makeid"] = Model.makeid;
            jsonObj["modelid"] = Model.modelid;
            jsonObj["variantid"] = Model.variantid;
            jsonObj["dateofregistration"] = Model.dateofregistration;
            jsonObj["noofseats"] = Model.noofseats;
            jsonObj["idv"] = Model.idv;
            jsonObj["rtoid"] = Model.rtoid;
            jsonObj["ncb"] = Model.ncb;
            jsonObj["od"] = Model.od;
            jsonObj["tp"] = Model.tp;
            jsonObj["addonprm"] = Model.addonprm;
            jsonObj["netprm"] = Model.netprm;
            jsonObj["gstvalue"] = Model.gstvalue;
            jsonObj["totalpremium"] = Model.totalpremium;
            jsonObj["createdby"] = Model.createdby;
                jsonObj["chequeno"] = Model.chequeno;

                string requestjson = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);

                LogU.WriteLog(requestjson);
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



            }

        

            }
            catch (Exception Ex)
            {
                LogU.WriteLog(Ex.Message);

            }
            return status;
        }

        public bool FetchData(int comid, string enqno,string tranid)
        {

            try
            {
                LogU.WriteLog("hello");
                var mypolicydata = ap.VW_POLICYDETAILSMASTER.Where(x => x.companyid == comid && x.enquiryid == enqno).FirstOrDefault();

             

                if (mypolicydata != null)
                {


                    CRMAPIController data = new CRMAPIController();
                    AddCRMData adddatamodel = new AddCRMData();


                    if (!string.IsNullOrEmpty(tranid))
                    {
                        adddatamodel.chasisno = tranid;

                    }
                    else
                    {
                        adddatamodel.chasisno = "";
                    }
                    adddatamodel.customermemberid = mypolicydata.customerfirstname != null ? mypolicydata.customerfirstname : string.Empty + " " + mypolicydata.customermiddlename != null ? mypolicydata.customermiddlename : string.Empty + " " + mypolicydata.customerlastname != null ? mypolicydata.customerlastname : string.Empty;
                    adddatamodel.companyid = comid.ToString();
                    adddatamodel.pospid = mypolicydata.pospid.ToString();
                    adddatamodel.businesstype = mypolicydata.policytype.ToString();
                    adddatamodel.policyno = mypolicydata.policyno.ToString();
                    adddatamodel.tenure = mypolicydata.tenure.ToString();

                    adddatamodel.startdate = Convert.ToDateTime(mypolicydata.policystartdate).ToString("MM/dd/yyyy");
                    adddatamodel.enddate =   Convert.ToDateTime(mypolicydata.policyenddate).ToString("MM/dd/yyyy");

                    if (mypolicydata.registrationno != null)
                    {
                        adddatamodel.registrationno = mypolicydata.registrationno.ToString();
                    }
                    else
                    {
                        adddatamodel.registrationno = "";
                    }
                    adddatamodel.engineno = mypolicydata.engingno.ToString();
                    adddatamodel.chasisno = mypolicydata.chassisno.ToString();
                    adddatamodel.makeid = mypolicydata.makeid.ToString();
                    adddatamodel.modelid = mypolicydata.modelid.ToString();
                    adddatamodel.variantid = mypolicydata.variantid.ToString();
                    adddatamodel.idv = mypolicydata.idv.ToString();
                    adddatamodel.od = mypolicydata.odpremium.ToString();
                    adddatamodel.tp = mypolicydata.tppremium.ToString();
                    adddatamodel.netprm = mypolicydata.netpremium.ToString();
                    adddatamodel.gst = "18";
                    if (mypolicydata.gstvalue != null)
                    {
                        adddatamodel.gstvalue = mypolicydata.gstvalue.ToString();
                    }
                    else
                    {
                        adddatamodel.gstvalue = "0";
                    }
                  
                    adddatamodel.totalpremium = mypolicydata.finalpremium.ToString();
                    adddatamodel.createdby = mypolicydata.pospid.ToString();
                    adddatamodel.totalpremium = mypolicydata.finalpremium.ToString();
                  
                    adddatamodel.totalpremium = mypolicydata.finalpremium.ToString();

                    if (mypolicydata.producttype != null)
                    {
                        adddatamodel.producttype = mypolicydata.producttype.ToString();
                    }
                    else
                    {
                        adddatamodel.producttype ="";
                    }
                  
                    adddatamodel.issuedate = Convert.ToDateTime(mypolicydata.policystartdate).ToString("MM/dd/yyyy");

                    if (mypolicydata.cubiccapacity != null)
                    {
                        adddatamodel.ccid = mypolicydata.cubiccapacity.ToString();
                    }
                    else
                    {
                        adddatamodel.ccid = "";
                    }

                    if (mypolicydata.manufacturdate != null)
                    {
                        adddatamodel.manufacturingyear = mypolicydata.manufacturdate.Value.Year.ToString();
                    }
                    else
                    {
                        adddatamodel.manufacturingyear = "";
                    }

                    if (mypolicydata.registrationdate != null)
                    {
                        adddatamodel.dateofregistration = Convert.ToDateTime(mypolicydata.registrationdate).ToString("MM/dd/yyyy");
                    }
                    else
                    {
                        adddatamodel.dateofregistration = "";
                    }


                    if (mypolicydata.seatingcapacity != null)
                    {
                        adddatamodel.noofseats = mypolicydata.seatingcapacity.ToString();
                    }
                    else
                    {
                        adddatamodel.noofseats ="";
                    }

                    if (mypolicydata.rtoid != null)
                    {
                        adddatamodel.rtoid = mypolicydata.rtoid.ToString();
                    }
                    else
                    {
                        adddatamodel.rtoid = "0";
                    }
                  
                    adddatamodel.zoneid = "0";

                    if (mypolicydata.ncdpercentage != null)
                    {
                        adddatamodel.ncb = mypolicydata.ncdpercentage.ToString();
                    }
                    else
                    {
                        adddatamodel.ncb ="0";
                    }
                  
                    if (mypolicydata.fueltype != null)
                    {
                        adddatamodel.fueltypeid = mypolicydata.fueltype.ToString();
                    }
                    else
                    {
                        adddatamodel.fueltypeid = "";
                    }
                    if (mypolicydata.addonpremium != null)
                    {
                        adddatamodel.addonprm = mypolicydata.addonpremium.ToString();
                    }
                    else
                    {
                        adddatamodel.addonprm = "0";
                    }
                    LogU.WriteLog("ggggggggggggggggg");
                    data.insertdata(adddatamodel);
                    LogU.WriteLog("insert done");
                    return true;
                }
                else
                {
                    LogU.WriteLog("Data Not Found In CRM");
                    return false;
                }
            }
            catch (Exception Ex)
            {
                LogU.WriteLog(Ex.InnerException.ToString());
                return false;

            }

        }


    }
}