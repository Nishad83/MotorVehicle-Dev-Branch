using AndApp;
using AndWebApi.Models;
using AndApp.Utilities;
using DAL;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web.Http;
using RestSharp;
using System.Net.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AndWebApi.Controllers
{

    public class DefaultController : ApiController
    {
        //[HttpGet]
        //public string gettoken()
        //{
        //    string crm_token = "";

        //    var token_client = new RestClient("http://192.168.2.4:88");

        //    ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(delegate { return true; });
        //    ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;

        //    var token_request = new RestRequest("token", Method.POST);

        //    token_request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
        //    token_request.AddHeader("Accept", "application/json");
        //    token_request.AddParameter("grant_type", "password");
        //   token_request.AddParameter("username", "andapp");
        //   token_request.AddParameter("password", "andapp_auth@2021");




        //    var token_response = token_client.Post(token_request);

        //    if (token_response.StatusCode == HttpStatusCode.OK)
        //    {
        //        var token_data = (JObject)JsonConvert.DeserializeObject(token_response.Content.ToString());
        //        crm_token = token_data["access_token"].Value<string>();
        //    }
        //    return crm_token;
        //}

        //[HttpGet]
        //public string insertdata(AddCRMData Model)
        //{
        //    string status = "";
        //    string crm_token = gettoken();

        //    var httpWebRequest = (HttpWebRequest)WebRequest.CreateHttp("http://192.168.2.4:88/Api/ApiMotor/Motor");
        //    httpWebRequest.ContentType = "application/json ; charset=utf-8";
        //    httpWebRequest.Method = "POST";
        //    httpWebRequest.Headers.Add("Authorization", "Bearer " + crm_token);
        //    string path = AppDomain.CurrentDomain.BaseDirectory;
        //    string filePath = Path.Combine(path, "JSON/crm/adddata.json");
        //    string json = File.ReadAllText(filePath);
        //    dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
        //    jsonObj["businesstype"] = Model.businesstype;
        //    jsonObj["pospid"] = Model.pospid;
        //    jsonObj["customergroupid"] = "";
        //    jsonObj["customermemberid"] = "";
        //    jsonObj["companyid"] = "";
        //    jsonObj["companybranchid"] = "";
        //    jsonObj["productid"] = Model.productid;
        //    jsonObj["producttype"] =Model.producttype;
        //    jsonObj["subproductid"] =Model.subproductid;
        //    jsonObj["policyno"] = Model.policyno;
        //    jsonObj["issuedate"] = Model.issuedate;
        //    jsonObj["isrenewable"] = Model.isrenewable;
        //    jsonObj["tenure"] = Model.tenure;
        //    jsonObj["startdate"] = Model.startdate;
        //    jsonObj["enddate"] = Model.enddate;
        //    jsonObj["registrationno"] = Model.registrationno;
        //    jsonObj["engineno"] = Model.engineno;
        //    jsonObj["chasisno"] = Model.chasisno;
        //    jsonObj["ccid"] = Model.ccid;
        //    jsonObj["manufacturingyear"] = Model.manufacturingyear;
        //    jsonObj["makeid"] = Model.makeid;
        //    jsonObj["modelid"] = Model.modelid;
        //    jsonObj["variantid"] = Model.variantid;

        //    jsonObj["dateofregistration"] =Model.dateofregistration;
        //    jsonObj["noofseats"] = Model.noofseats;
        //    jsonObj["idv"] = Model.idv;
        //    jsonObj["rtoid"] = Model.rtoid;
        //    jsonObj["zoneid"] = Model.zoneid;
        //    jsonObj["ncb"] = Model.ncb;



        //    jsonObj["fueltypeid"] = Model.fueltypeid;
        //    jsonObj["od"] = Model.od;
        //    jsonObj["tp"] = Model.tp;
        //    jsonObj["addonprm"] = Model.addonprm;
        //    jsonObj["netprm"] = Model.netprm;
        //    jsonObj["gst"] = Model.gst;
        //    jsonObj["gstvalue"] = Model.gstvalue;
        //    jsonObj["totalpremium"] = Model.totalpremium;
        //    jsonObj["discount"] = Model.discount;

        //    jsonObj["paymenttypeid"] = Model.paymenttypeid;

        //    jsonObj["cash"] = Model.cash;
        //    jsonObj["bankid"] = Model.bankid;
        //    jsonObj["accountno"] = Model.accountno;
        //    jsonObj["amount"] =Model.amount;
        //    jsonObj["chequeno"] =Model.chequeno;
        //    jsonObj["chequedate"] = Model.chequedate;
        //    jsonObj["bankid1"] = Model.bankid1;


        //    jsonObj["accountno1"] =Model.accountno1;
        //    jsonObj["amount1"] = Model.amount1;
        //    jsonObj["chequeno1"] = Model.chequeno1;
        //    jsonObj["chequedate1"] = Model.chequedate1;
        //    jsonObj["bankid2"] = Model.bankid2;
        //    jsonObj["accountno2"] =Model.accountno2;
        //    jsonObj["amount2"] = Model.amount2;
        //    jsonObj["chequeno2"] = Model.chequeno2;
        //    jsonObj["chequedate2"] = Model.chequedate2;
        //    jsonObj["posporreferenxe"] = Model.posporreferencecode;
        //    jsonObj["posporreferencecode"] = Model.posporreferencecode;
        //    jsonObj["brokerageodper"] = Model.brokerageodper;


        //    jsonObj["brokeragetpper"] = Model.brokeragetpper;
        //    jsonObj["brokeragegst"] = Model.brokeragegst;
        //    jsonObj["brokeagetotalamount"] = Model.brokeagetotalamount;
        //    jsonObj["feesodper"] = Model.feesodper;
        //    jsonObj["feestpper"] = Model.feestpper;
        //    jsonObj["feesgst"] = Model.feesgst;
        //    jsonObj["feestotalamount"] = Model.feestotalamount;
        //    jsonObj["bankid1"] = Model.bankid1;
        //    jsonObj["createdby"] = Model.createdby;

        //    string requestjson = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
        //    using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
        //    {
        //        streamWriter.Write(requestjson);
        //        streamWriter.Flush();
        //        streamWriter.Close();
        //    }
        //    var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
        //    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
        //    {
        //        var result = streamReader.ReadToEnd();
 


        //    }

        //    return status;
        //}

        [HttpGet]
        public string TestAPI()
        {
           
            return "HELLO GANESHA";
        }

        [HttpGet]
        public ResponseModel GetAllRto(long stateid)
        {
            List<RTOMASTER_ANDAPP> rto = new List<RTOMASTER_ANDAPP>();
            ResponseModel objresponsemodel = new ResponseModel();
            try
            {

                DAL_CommonCls objcls = new DAL_CommonCls();
                rto = objcls.GetAllRto(stateid,"");

                if (rto != null)
                {
                    objresponsemodel.success = true;
                    objresponsemodel.data = rto;
                }
            }
            catch (Exception Ex)
            {
                LogU.WriteLog("DefaultController >> GetAllRto >> Error while getting all rto for state id" + stateid+ ">> " + Convert.ToString(Ex.Message));
                Console.Write(Convert.ToString(Ex.Message));
            }
            return objresponsemodel;
        }

        [HttpGet]
        public ResponseModel GetAllModel(long makeid)
        {
            List<MODEL_ANDAPP> model = new List<MODEL_ANDAPP>();
            ResponseModel objresponsemodel = new ResponseModel();
            try
            {
                DAL_CommonCls objcls = new DAL_CommonCls();

                model = objcls.GetAllModel(makeid);
                if (model != null && model.Count() > 0)
                {
                    objresponsemodel.success = true;
                    objresponsemodel.data = model;
                }
            }
            catch (Exception Ex)
            {
                objresponsemodel.success = false;
                LogU.WriteLog("DefaultController >> GetAllModel >> Error while getting all model for make id" + makeid +">> " + Convert.ToString(Ex.Message));
                Console.Write(Convert.ToString(Ex.Message));
            }
            return objresponsemodel;
        }

        /// <summary>
        /// Qualify company list return as per input parameter. 
        /// </summary>
        /// <param name="IsBreakin">Pass true false value for break-in.</param>
        /// <param name="VariantId">Pass variant id.</param>
        /// <param name="RtoId">Pass rto id.</param>
        /// <returns>Return object type of response model.</returns>
        [HttpPost]
        public ResponseModel QualifyCompany(QualifyCompany QuaComModel)
        {
           
            ResponseModel res = new ResponseModel();
            List<SP_QUALIFYCOMPANY> model = new List<SP_QUALIFYCOMPANY>();
            try
            {
                ANDAPPEntities entity = new ANDAPPEntities();
                model = entity.SP_QUALIFYCOMPANY(QuaComModel.IsBreakin, QuaComModel.RtoId, QuaComModel.VariantId).ToList();
                res.data = model;//model.Where(x=>x.comid=="9").ToList();
                res.success = true;
                res.enquiryid = GenerateEnquiryId();
            }
            catch (Exception ex)
            {
                res.success = false;
                res.message = "Error while qualify company !!!";
                LogU.WriteLog("DefaultController >> QualifyCompany >> Error while qualify company !!!" + Convert.ToString(ex.Message));
                Console.Write(Convert.ToString(ex.Message));
            }
            return res;
        }

        /// <summary>
        /// Create a method for get nominee relation ship based on company id.  
        /// </summary>
        /// <param name="comid">Pass insurance company id.</param>
        /// <returns>Return a nominee relation ship list.</returns>
        [HttpGet]
        public ResponseModel GetNomineeRelationship(long compid)
        {
            List<NOMINEE_RELATIONSHIP> model = new List<NOMINEE_RELATIONSHIP>();
            ResponseModel objresponsemodel = new ResponseModel();
            try
            {
                DAL_CommonCls objcls = new DAL_CommonCls();
                model = objcls.GetNomineeRelation(compid);
                if (model != null && model.Count() > 0)
                {
                    objresponsemodel.success = true;
                    objresponsemodel.data = model;
                }
            }
            catch (Exception Ex)
            {
                LogU.WriteLog("DefaultController >> GetNomineeRelationship >> Error while getting noimee relation for company id" + compid + ">> " + Convert.ToString(Ex.Message));
                Console.Write(Convert.ToString(Ex.Message));
            }
            return objresponsemodel;
        }

        /// <summary>
        /// Create a method for get occupation list based on company id.
        /// </summary>
        /// <param name="comid">Pass insurance company id.</param>
        /// <returns>Return list of occupation.</returns>
        [HttpGet]
        public ResponseModel GetOccupation(long compid)
        {
            List<OCCUPATION> model = new List<OCCUPATION>();
            ResponseModel objresponsemodel = new ResponseModel();
            try
            {
                DAL_CommonCls objcls = new DAL_CommonCls();

                model = objcls.GetOccupation(compid);
                if (model != null && model.Count() > 0)
                {
                    objresponsemodel.success = true;
                    objresponsemodel.data = model;
                }
            }
            catch (Exception Ex)
            {
                LogU.WriteLog("DefaultController >> GetOccupation >> Error while getting occupation for company id" + compid + ">> " + Convert.ToString(Ex.Message));
                Console.Write(Convert.ToString(Ex.Message));
            }
            return objresponsemodel;
        }
        [HttpGet]
        public ResponseModel GetSalutation(long compid)
        {
            List<SALUTATIONMASTER> model = new List<SALUTATIONMASTER>();
            ResponseModel objresponsemodel = new ResponseModel();
            try
            {
                DAL_CommonCls objcls = new DAL_CommonCls();

                model = objcls.GetSalutation(compid);
                if (model != null && model.Count() > 0)
                {
                    objresponsemodel.success = true;
                    objresponsemodel.data = model;
                }
            }
            catch (Exception Ex)
            {
                LogU.WriteLog("DefaultController >> GetSalutation >> Error while getting Salutation for company id" + compid + ">> " + Convert.ToString(Ex.Message));
                Console.Write(Convert.ToString(Ex.Message));
            }
            return objresponsemodel;
        }
        [HttpGet]
        public ResponseModel GetStateCompanywise(long compid)
        {
            List<COMPANY_WISE_STATE_MASTER> model = new List<COMPANY_WISE_STATE_MASTER>();
            ResponseModel objresponsemodel = new ResponseModel();
            try
            {
                DAL_CommonCls objcls = new DAL_CommonCls();

                model = objcls.GetStateCompanywise(compid);
                if (model != null && model.Count() > 0)
                {
                    objresponsemodel.success = true;
                    objresponsemodel.data = model;
                }
            }
            catch (Exception Ex)
            {
                LogU.WriteLog("DefaultController >> GetStateCompanywise >> Error while getting State for company id" + compid + ">> " + Convert.ToString(Ex.Message));
                Console.Write(Convert.ToString(Ex.Message));
            }
            return objresponsemodel;
        }
        [HttpGet]
        public ResponseModel GetMarritalStatus(long compid)
        {
            List<MARITALSTATUSMASTER> model = new List<MARITALSTATUSMASTER>();
            ResponseModel objresponsemodel = new ResponseModel();
            try
            {
                DAL_CommonCls objcls = new DAL_CommonCls();

                model = objcls.GetMarritalStatus(compid);
                if (model != null && model.Count() > 0)
                {
                    objresponsemodel.success = true;
                    objresponsemodel.data = model;
                }
            }
            catch (Exception Ex)
            {
                LogU.WriteLog("DefaultController >> GetMarritalStatus >> Error while getting MarritalStatus for company id" + compid + ">> " + Convert.ToString(Ex.Message));
                Console.Write(Convert.ToString(Ex.Message));
            }
            return objresponsemodel;
        }

        public string GenerateEnquiryId()
        {
            string EnqId = string.Empty;
            EnqId = "ENQ" + DateTime.Now.Ticks.ToString().Substring(0, 10);
            return EnqId;
        }

        [HttpGet]
        public ResponseModel GetFinancerLIst(long compid)
        {
            List<FINANCERMASTER> model = new List<FINANCERMASTER>();
            ResponseModel objresponsemodel = new ResponseModel();
            try
            {
                DAL_CommonCls objcls = new DAL_CommonCls();

                model = objcls.GetFinancerLIst(compid);
                if (model != null && model.Count() > 0)
                {
                    objresponsemodel.success = true;
                    objresponsemodel.data = model;
                }
            }
            catch (Exception Ex)
            {
                LogU.WriteLog("DefaultController >> GetFinancerLIst >> Error while getting Financer for company id" + compid + ">> " + Convert.ToString(Ex.Message));
                Console.Write(Convert.ToString(Ex.Message));
            }
            return objresponsemodel;
        }


        public ResponseModel GetCompanyIdbyName(string company)
        {
            ResponseModel objresponsemodel = new ResponseModel();
            try
            {
                DAL_CommonCls objcls = new DAL_CommonCls();

                var name = objcls.GetCompanyIdbyName(company);
                if (!string.IsNullOrEmpty(name))
                {
                    objresponsemodel.success = true;
                    objresponsemodel.data = name;
                }
            }
            catch (Exception Ex)
            {
                LogU.WriteLog("DefaultController >> GetCompanyIdbyName >> Error while getting company id>> " + Convert.ToString(Ex.Message));
                Console.Write(Convert.ToString(Ex.Message));
            }
            return objresponsemodel;
        }
    }
    //public class ResponseModel
    //{
    //    public bool success;

    //    public string message;
    //    public object data { get; set; }
    //    public string enquiryid { get; set; }
    //}


}
