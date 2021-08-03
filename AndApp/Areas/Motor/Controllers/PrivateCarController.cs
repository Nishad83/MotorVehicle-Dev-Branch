using AndApp.Models;
using DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AndWebApi.FGI;
using Newtonsoft.Json;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Web.Script.Serialization;
using System.Net;
using System.Text;
using System.Net.Http.Headers;
using AndApp.AuthData;
using System.Security.Cryptography;
using SelectPdf;
using System.IO;
using System.Configuration;
using RestSharp;
using System.Net.Security;
using System.Data.Entity.Core.Objects;
using AndApp.Utilities;
using static AndApp.Models.SearchCriteria;
using System.Data;

using System.Net.Mime;
using static AndApp.Models.CommonModels;
using static AndApp.Utilities.Common;

namespace AndApp.Areas.Motor.Controllers
{


    public class PrivateCarController : Controller
    {
        DAL_CommonCls objcls = new DAL_CommonCls();

        DAL.ANDAPPEntities ap = new DAL.ANDAPPEntities();

        CRMAPIController datacrm = new CRMAPIController();



        public ActionResult Downloadd(string fileName)

        {

            //var g = Path.GetFileName(fil);
            //g.SaveAs("https://pipuat.tataaiginsurance.in/tagichubws/motor_policy.jsp?polno=064001/0177533393/000000/00&src=app&key=C0MJOrWjkbNYtMKbOKiV2LiDs", "gg");
            return View();
        }

        public ActionResult s()
        {

            string icici_token = GeticiciTokenNo("Generic");


            var client = new RestClient("https://ilesbsanity.insurancearticlez.com/ILServices/Misc/v1/Generic/PolicyCertificate?policyNo=3001/51992405/00/000");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            request.AddHeader("Authorization", "Bearer " + icici_token);
            IRestResponse response = client.Execute(request);
            Console.WriteLine(response.Content);

            var result = response.Content;
            byte[] results = response.RawBytes;
            Stream stream = new MemoryStream(results);
            stream.Flush();
            stream.Position = 0;

            return File(stream, "application/pdf", "Labels.pdf");
        }
        public string GeticiciTokenNo(string scope)
        {
            string icici_token = "";

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
        // GET: PrivateCar
        [HttpGet]
        public ActionResult DIGITPaymentFail(string errorMsg)
        {

            return RedirectToAction("thankyouurl", new { Response = errorMsg });
        }
        [HttpGet]
        public ActionResult DIGITPaymentSuccess(string applicationid, string policyno, string transactionNumber)
        {
            LogU.WriteLog("DIGITPaymentSuccess start " + policyno + "");
            ANDAPPEntities andent = new ANDAPPEntities();
            long userid = 0;
            TempData["applicationid"] = applicationid;
            TempData["policyno"] = policyno;
            var GetEnqNo = ap.GET_Payment_Parameter(policyno, 6).ToList();//18-06-2021 Todays Changes
            if (GetEnqNo.Count > 0)
            {
                string enqno = GetEnqNo[0].enq_id;
                var pospid = ap.VW_POLICYDETAILSMASTER.Where(x => x.enquiryid == enqno && x.companyid == 6).FirstOrDefault();
                userid = (long)pospid.pospid;
                ap.SP_POLICYDETAILSMASTER("U", enqno, 6, pospid.pospid,
                                   null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
                                   null, null, null, null, null, null, null, null, null, null, null, null, policyno, null, null,
                                   null, null, 1, null, null, null, null, null, null, null, null, null, null, true);

                if (!string.IsNullOrEmpty(policyno))
                {
                    datacrm.FetchData(6, enqno, transactionNumber);
                }
              
                
                //ap.SP_POLICYDETAILSMASTER("U", enqno, 6, Common.MySession.UserDetail.userid, null, null, null, null, null,
                //                            null, null, null, null, null, null, null, null, null, null, null, null, null, null,
                //                            null, null, null, null, null, null, null, null, null, null, true);
                ap.SP_Payment_Parameter(enqno, 6, "TransactionNumber", transactionNumber);
            }

            return RedirectToAction("thankyouurl", new { company = "DIGIT", response = "Pass", pospid = userid });
        }

        public ActionResult tatathankyouurl(string enqno)
        {
            string response = Request.QueryString.ToString();

            long userid = 0;
            if (response != null)
            {
                var responsedata = response.Split('=');
                if (responsedata != null)
                {

                    string enqid = responsedata[0].Substring(0, 13);
                    response = responsedata[1];
                    response = string.Concat(response.Reverse().Skip(6).Reverse());
                    response = response + "==";
                    byte[] data = Convert.FromBase64String(response);
                    string decodedString = Encoding.UTF8.GetString(data);

                    var proposal_resdata = (JObject)JsonConvert.DeserializeObject(decodedString.ToString());
                    var Data = proposal_resdata["data"];
                    var status = proposal_resdata["status"].ToString();
                    if (status == "1")

                    {
                        string policyno = Data["policyno"].ToString();
                        string mypno = "";
                        string rnd_str = Data["rnd_str"].ToString();
                        using (WebClient ccc = new WebClient())
                        {
                            mypno = policyno.Replace("/", "");
                            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                            ccc.DownloadFile("https://pipuat.tataaiginsurance.in/tagichubws/motor_policy.jsp?polno=064001/0177533393/000000/00&src=app&key=C0MJOrWjkbNYtMKbOKiV2LiDs", ConfigurationManager.AppSettings["savepdf"].ToString() + mypno + ".pdf");
                        }
                        TempData["tatapdf"] = "https://pipuat.tataaiginsurance.in/tagichubws/motor_policy.jsp?polno=" + policyno + "&src=app&key=" + rnd_str + "";
                        //ViewBag.pdfurl = ";
                        //ap.SP_POLICYDETAILSMASTER("U", enqid, 22, MySession.UserDetail.userid, null, null, null, null, null,
                        //                     null, null, null, null, null, null, null, null, null, null, null, null, mypno, null,
                        //                     null, null, null, null, null, null, null, null, null, null, true);
                        var pospid = ap.VW_POLICYDETAILSMASTER.Where(x => x.enquiryid == enqid && x.companyid == 22).FirstOrDefault();
                        userid = (long)pospid.pospid;
                        ap.SP_POLICYDETAILSMASTER("U", enqid, 22, pospid.pospid,
                                   null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
                                   null, null, null, null, null, null, null, null, null, null, null, null, mypno, null, null,
                                   null, null, null, null, null, null, null, null, null, null, null, null, null, true);


                        if (!string.IsNullOrEmpty(mypno))
                        {
                            datacrm.FetchData(22, enqid,"");
                        }
                       
                        return RedirectToAction("thankyouurl", new { company = "tata", response = "Payment Successful" , pospid = userid });
                    }
                    else
                    {
                        return RedirectToAction("thankyouurl", new { company = "tata", response = "Payment Fail" });

                    }
                }
            }
            return RedirectToAction("thankyouurl", new { company = "tata", response = "Payment Fail" });
        }
        public ActionResult icicithankyouurl(string response)
        {
            LogU.WriteLog("ICICI OK");
            try
            {
                long userid = 0;
                if (response != null)
                {


                    var mappingdata = ap.icici_paymenttagging.Where(x => x.TransactionId == response).FirstOrDefault();

                    PaymentRequest model = new PaymentRequest();

                    CompanyWiseRefference comp = new CompanyWiseRefference();
                    comp.CorrelationId = mappingdata.TransactionId;
                    comp.QuoteId = mappingdata.CustomerID;
                    comp.QuoteNo = mappingdata.ProposalNo;
                    comp.applicationId = mappingdata.dealid;


                    model.CompanyDetail = comp;
                    model.FinalPremium = Convert.ToDouble(mappingdata.PaymentAmount);

                    string apiUrl = ConfigurationManager.AppSettings["PaymentMapping"].ToString();
                    string inputJson = (new JavaScriptSerializer()).Serialize(model);
                    WebClient client = new WebClient();
                    client.Headers["Content-type"] = "application/json";
                    client.Encoding = Encoding.UTF8;
                    string policyno = client.UploadString(apiUrl, inputJson);
                    //byte[] bytes = Encoding.UTF8.GetBytes(json);
                    if (policyno != null)
                    {
                        string enqno = mappingdata.EnqNo;
                        policyno = policyno.Replace("\"", "");
                        string PolicyNo = policyno.Replace("/", "");

                        var pospid = ap.VW_POLICYDETAILSMASTER.Where(x => x.enquiryid == enqno && x.companyid == 9).FirstOrDefault();

                        userid = (long)pospid.pospid;
                        ap.SP_POLICYDETAILSMASTER("U", enqno, 9, pospid.pospid,
                                      null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
                                      null, null, null, null, null, null, null, null, null, null, null, null, PolicyNo, null, null,
                                      null, null, null, null, null, null, null, null, null, null, null, null, null, true);


                        //var mypolicydata = ap.VW_POLICYDETAILSMASTER.Where(x => x.companyid == 9 && x.enquiryid == enqno).FirstOrDefault();


                        if (!string.IsNullOrEmpty(policyno))
                        {
                            datacrm.FetchData(9, enqno, mappingdata.TransactionId);
                        }
                       

                        ViewBag.company = "ICICI";
                        TempData["policyno"] = policyno;
                        TempData["enqno"] = enqno;
                        return RedirectToAction("thankyouurl", new { company = "icici", response = "Payment Successful", pospid = userid });

                    }
                }
            }
            catch (Exception Ex)
            {

                LogU.WriteLog("ICICIThankURL >> " + response + " >>" + Ex.Message);
            }

            return RedirectToAction("thankyouurl", new { company = "icici", response = "Payment Fail" });
        }

        public ActionResult thankyouurl(string company, string response, long pospid)
        {

            LogU.WriteLog("thankyouurl start "+ company + "");

            try
            {
                var pospdata = ap.POSPMASTERs.Where(x => x.pospid == pospid).FirstOrDefault();

                ViewBag.pospname = (!string.IsNullOrEmpty(pospdata.firstname) ? pospdata.firstname : string.Empty) + " " + (!string.IsNullOrEmpty(pospdata.middelname) ? pospdata.middelname.Substring(0, 1) : string.Empty) + " " + (!string.IsNullOrEmpty(pospdata.lastname) ? pospdata.lastname : string.Empty);

                UserSessionDetails pdata = new UserSessionDetails();
                pdata.mobileno = pospdata.mobileno;
                pdata.pospcode = pospdata.pospcode;
                pdata.username = (!string.IsNullOrEmpty(pospdata.firstname) ? pospdata.firstname : string.Empty) + " " + (!string.IsNullOrEmpty(pospdata.middelname) ? pospdata.middelname.Substring(0, 1) : string.Empty) + " " + (!string.IsNullOrEmpty(pospdata.lastname) ? pospdata.lastname : string.Empty);
                pdata.userid = pospdata.pospid;
                pdata.stateid = pospdata.stateid;
                pdata.cityid = pospdata.cityid;
                pdata.pincode = pospdata.pincode;
                pdata.emailid = pospdata.emailid;
                pdata.mobileno = pospdata.mobileno;
                pdata.area = pospdata.area;
                pdata.beneficiaryname = pospdata.beneficiaryname;
                pdata.accountno = pospdata.accountno;
                pdata.ifsc = pospdata.ifsc;
                pdata.salutation = pospdata.salutation;
                pdata.accountno = pospdata.accountno;
                pdata.addressline1 = pospdata.addressline1;
                pdata.addressline2 = pospdata.addressline2;
                Common.MySession.UserDetail = pdata;
                Common.MySession.UserDetail = pdata;
                Common.MySession.IsLoggedIn = true;
                if (company == "tata")

                {
                    if (TempData["tatapdf"] != null)
                    {
                        ViewBag.pdfurl = TempData["tatapdf"].ToString();
                        ViewBag.payment = "Payment Successful";

                    }
                    else
                    {
                        ViewBag.payment = "Payment Fail";
                    }

                }

                if (company == "icici")
                {
                    if (TempData["policyno"] != null)
                    {
                        TempData.Keep("policyno");
                        ViewBag.payment = "Payment Successful";
                        ViewBag.company = "ICICI";
                    }
                    else
                    {
                        ViewBag.payment = "Payment Fail";
                    }
                }
                if (company == "DIGIT")
                {
                    //if(2==2)
                    //ViewBag.payment = "Payment Successful";

                    if (TempData["applicationid"] != null)
                    {
                        var applicationid = TempData["applicationid"].ToString();
                        var policyno = TempData["policyno"].ToString();
                        string status = GetPolicystatus(policyno);
                        string pdf = GenerateDIGITPDF(applicationid, status, policyno);
                        ViewBag.payment = "Payment Successful";
                        ViewBag.pdfurl = pdf;
                    }
                    else
                    {
                        ViewBag.payment = "Payment Fail";
                    }

                }
            }
            catch (Exception Ex)
            {

                LogU.WriteLog("Payment >> " + company + " >>" + Ex.Message);
            }



            return View();
        }
        //public ActionResult thankyouurl(string company, string response)
        //{



        //    if (company == "tata")
        //    {


        //    }

        //    else if (company == "icici")
        //    {
        //        if (response != null)
        //        {


        //        var mappingdata = ap.icici_paymenttagging.Where(x => x.TransactionId == response).FirstOrDefault();

        //        PaymentRequest model = new PaymentRequest();

        //        CompanyWiseRefference comp = new CompanyWiseRefference();
        //        comp.CorrelationId = mappingdata.TransactionId;
        //        comp.QuoteId = mappingdata.CustomerID;
        //        comp.QuoteNo = mappingdata.ProposalNo;

        //        model.CompanyDetail = comp;
        //        model.FinalPremium = Convert.ToDouble(mappingdata.PaymentAmount);

        //        string apiUrl = "http://localhost:17676/api/PrivateCar/PaymentMapping";
        //        string inputJson = (new JavaScriptSerializer()).Serialize(model);
        //        WebClient client = new WebClient();
        //        client.Headers["Content-type"] = "application/json";
        //        client.Encoding = Encoding.UTF8;
        //        string policyno = client.UploadString(apiUrl, inputJson);
        //        //byte[] bytes = Encoding.UTF8.GetBytes(json);
        //        if (policyno != null)
        //        {
        //                ViewBag.company = "ICICI";
        //            TempData["policyno"] = policyno;
        //            ViewBag.payment = "Payment Successful";

        //        }
        //        }
        //        return View();
        //    }


        //    return View();
        //}



        public ActionResult ICICPDF()
        {

            string icici_token = GeticiciTokenNo("Generic");

            string policyno = TempData["policyno"].ToString();
            string mypolicyno = policyno.Replace("/", "");

            var client = new RestClient("" + ConfigurationManager.AppSettings["ICICIPolicyCertificate"].ToString() + "" + policyno + "");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            request.AddHeader("Authorization", "Bearer " + icici_token);
            IRestResponse response = client.Execute(request);



            //var result = response.Content;
            byte[] result = response.RawBytes;



            Stream stream = new MemoryStream(result);
            stream.Flush();
            stream.Position = 0;



            System.IO.File.WriteAllBytes(ConfigurationManager.AppSettings["savepdf"] + mypolicyno + ".pdf", result);



            return File(stream, "application/pdf", "" + policyno + ".pdf");


        }



        #region HDFC PAYMENT SUCCESS AND FAIL VIEW AND DOWNLOAD POLICY
        public ActionResult PaymentSuccess()
        {
            ViewBag.ErrMsg = "";
            try
            {
                string formrequest = Request.Form.ToString();

                #region Generate Policy No
                if (!string.IsNullOrEmpty(formrequest))
                {
                    string trno = formrequest.Split('=')[1];
                    return RedirectToAction("HDFCThankyouUrl", "PrivateCar", new { transactionno = trno });
                }
                //18-06-2021 Todays Changes start
                else
                {
                    ViewBag.ErrMsg = "Transaction Number not Found!!!";
                }
                //18-06-2021 Todays Changes end
                #endregion

            }
            catch (Exception ex)
            {
                Console.Write(ex.Message.ToString());
                Console.Write(ex.InnerException.ToString());
            }
            return View();
        }
        public ActionResult HDFCThankyouUrl(string transactionno)
        {
            ViewBag.pospname = MySession.UserDetail.username;
            string policyGenerateURL = ConfigurationManager.AppSettings["HDFCPOLICYGENERATE"];
            string DownloadPolicyURL = ConfigurationManager.AppSettings["HDFCPOLICYDOWNLOAD"];
            try
            {
                if (!string.IsNullOrEmpty(transactionno))
                {
                    #region Generate Policy Number
                    string JsonGeneratePolicyNumber = "{\"UniqueRequestID\":\"" + "100a0cd5-3928-44c2-a38f-7785e6c85300" +
                      "\",\"TransactionNo\":" + transactionno
                      + ",\"AgentCode\":\"FWC00164\"}";

                    dynamic responseGeneratePolicyNumber = Webservice(JsonGeneratePolicyNumber, policyGenerateURL);

                    if (responseGeneratePolicyNumber.StatusCode == HttpStatusCode.OK)
                    {
                        dynamic ResponsePolicyNumber = (JObject)JsonConvert.DeserializeObject(responseGeneratePolicyNumber.Content.ToString());
                        string responseGeneratePolicyNumberMessage = Convert.ToString(ResponsePolicyNumber.Message);
                        if (string.IsNullOrEmpty(responseGeneratePolicyNumberMessage))
                        {
                            dynamic ResponsePolicyNumberData = ResponsePolicyNumber.Data;
                            string PolicyNo = ResponsePolicyNumberData.PolicyNumber;

                            var GetEnqNo = ap.GET_Payment_Parameter(transactionno, 8).ToList();//18-06-2021 Todays Changes
                            if (GetEnqNo.Count > 0)
                            {
                                string enno = GetEnqNo[0].enq_id;
                                ap.SP_POLICYDETAILSMASTER("U", enno, 8, Common.MySession.UserDetail.userid,
                                      null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
                                      null, null, null, null, null, null, null, null, null, null, null, null, PolicyNo, null, null,
                                      null, null, 1, null, null, null, null, null, null, null, null, null, null, true);

                                if (!string.IsNullOrEmpty(PolicyNo))
                                {
                                    datacrm.FetchData(8, enno, transactionno);
                                }

                               
                            }

                            #region DOWNLOAD PDF 
                            string policyDownload = "{\"AgentCd\":\"FWC00164\",\"PolicyNo\":\"" + PolicyNo + "\"}";

                            dynamic responseDownloadPolicy = Webservice(policyDownload, DownloadPolicyURL);
                            if (responseDownloadPolicy.StatusCode == HttpStatusCode.OK)
                            {
                                dynamic DownloadPolicyResponse = (JObject)JsonConvert.DeserializeObject(responseDownloadPolicy.Content.ToString());
                                string responseDownloadMessage = Convert.ToString(DownloadPolicyResponse.ErrMsg);
                                string policybytes = Convert.ToString(DownloadPolicyResponse.pdfbytes);

                                //18-06-2021 Todays Changes start
                                if (responseDownloadMessage != "NA")
                                {
                                    ViewBag.ErrMsg = responseDownloadMessage;
                                    return View();
                                }
                                //18-06-2021 Todays Changes end

                                if (!string.IsNullOrEmpty(policybytes))
                                {
                                    ViewBag.PolicyNo = PolicyNo;


                                    byte[] bytes = System.Convert.FromBase64String(policybytes);
                                    System.IO.File.WriteAllBytes(ConfigurationManager.AppSettings["savepdf"] + PolicyNo + ".pdf", bytes);
                                    return File(bytes, "aplication/pdf", PolicyNo + ".pdf");
                                }
                            }
                            #endregion
                        }
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message.ToString());
                Console.Write(ex.InnerException.ToString());
            }
            return View();
        }

        public IRestResponse Webservice(string jsonString, string url)
        {
            RestClient clientPremCalc = new RestClient("https://uatcp.hdfcergo.com/PCOnline/ChannelPartner/");
            var requestPremCalc = new RestRequest(url, Method.POST);
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            requestPremCalc.AddHeader("MerchantKey", "IAND INSURANCE BROKER");
            requestPremCalc.AddHeader("SecretToken", "pce7d5CDaVuvXasix7H6jw==");

            requestPremCalc.RequestFormat = DataFormat.Json;
            requestPremCalc.AddJsonBody(jsonString);
            var responsePremCalc = clientPremCalc.Execute(requestPremCalc);

            return responsePremCalc;
        }
        public ActionResult PaymentFail()
        {
            return View();
        }

        #endregion

        #region My Policy

        [SessionTimeout]
        public ActionResult DownloadMyPolicy(string policyno, string companyid)
        {

            if (!string.IsNullOrEmpty(policyno))
            {
                string Pno = policyno + ".pdf";
                //var path = "D:\\andappQA\\PolicyPDF\\" + Pno;
                string path = ConfigurationManager.AppSettings["savepdf"] + Pno;
                if (System.IO.File.Exists(path))
                {
                    return File(path, "application/pdf", Pno);
                }
                else
                {
                    if ((companyid.Equals("5")) || (companyid.ToUpper().Equals("FGI")))
                    {
                        AndWebApi.FGI_Policy_Pdf.PDFSoapClient pdf = new AndWebApi.FGI_Policy_Pdf.PDFSoapClient();
                        DataTable dt = pdf.GetPDF(policyno, Convert.ToString(ConfigurationManager.AppSettings["FGIPolicyPdfUName"]), Convert.ToString(ConfigurationManager.AppSettings["FGIPolicyPdfPwd"]));
                        //string dtddd = dt.ToString();
                        byte[] tempByteArray = new byte[0];
                        if (dt != null && dt.Rows.Count > 0)
                        {
                            if (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["PDFBytes"])))
                            {
                                tempByteArray = (byte[])dt.Rows[0]["PDFBytes"];
                                System.IO.File.WriteAllBytes(path, tempByteArray);
                                return File(path, "application/pdf", Pno);
                            }
                        }

                    }
                    else if ((companyid.Equals("6")) || (companyid.ToUpper().Equals("DIGIT")))
                    {
                        var enqid = ap.Payment_Parameter.Where(x => x.parameter_value == policyno).Select(x => x.enq_id).FirstOrDefault();
                        var applicationid = ap.Payment_Parameter.Where(x => x.enq_id == enqid && x.parameter_name == "applicationId").FirstOrDefault().parameter_value.ToString();
                        var pdfpath = GenerateDIGITPDF(applicationid, "success", policyno);
                        if (pdfpath != "serviceerror" && pdfpath != "error" && !string.IsNullOrEmpty(pdfpath))
                        {
                            return File(path, "application/pdf", policyno);
                        }

                    }
                    else if ((companyid.Equals("8")) || (companyid.ToUpper().Equals("HDFC")))
                    {




                        #region DOWNLOAD PDF 
                        string DownloadPolicyURL = ConfigurationManager.AppSettings["HDFCPOLICYDOWNLOAD"];
                        string policyDownload = "{\"AgentCd\":\"FWC00164\",\"PolicyNo\":\"" + policyno + "\"}";

                        dynamic responseDownloadPolicy = Webservice(policyDownload, DownloadPolicyURL);
                        if (responseDownloadPolicy.StatusCode == HttpStatusCode.OK)
                        {
                            dynamic DownloadPolicyResponse = (JObject)JsonConvert.DeserializeObject(responseDownloadPolicy.Content.ToString());
                            string responseDownloadMessage = Convert.ToString(DownloadPolicyResponse.ErrMsg);
                            string policybytes = Convert.ToString(DownloadPolicyResponse.pdfbytes);

                            //18-06-2021 Todays Changes start
                            if (responseDownloadMessage != "NA")
                            {
                                ViewBag.ErrMsg = responseDownloadMessage;
                                return View();
                            }
                            //18-06-2021 Todays Changes end

                            if (!string.IsNullOrEmpty(policybytes))
                            {
                                byte[] bytes = System.Convert.FromBase64String(policybytes);
                                System.IO.File.WriteAllBytes(ConfigurationManager.AppSettings["savepdf"] + policyno + ".pdf", bytes);
                                return File(bytes, "aplication/pdf", policyno + ".pdf");
                            }
                        }




                        #endregion
                    }
                }
            }
            else
            {

            }
            return RedirectToAction("MyPolicy", "PrivateCar");
        }

        [SessionTimeout]
        public ActionResult MyPolicy()
        {
            QuotaionSearchCriteria model = new QuotaionSearchCriteria();

            FillDropDown_List();
            BindMyPolicy(model);
            return View();
        }

        [HttpPost]
        public ActionResult MyPolicy(QuotaionSearchCriteria model)
        {
            FillDropDown_List();
            BindMyPolicy(model);
            return View();
        }
        public void BindMyPolicy(QuotaionSearchCriteria model)
        {
            var predicate = PredicateBuilder.True<VW_POLICYDETAILSMASTER>();

            //sessiondetails.userid
            //Make ID
            if (model.makeid != 0 && model.makeid != null)
            {
                //model.variantid = 12;
                predicate = predicate.And(i => i.makeid == model.makeid);
            }

            //Model ID
            if (model.modelid != 0 && model.modelid != null)
            {
                //model.variantid = 12;
                predicate = predicate.And(i => i.modelid == model.modelid);
            }

            //variant ID
            if (model.variantid != 0 && model.variantid != null)
            {
                //model.variantid = 12;
                predicate = predicate.And(i => i.variantid == model.variantid);
            }

            //insurancename
            if (model.insurancename != null)
            {
                Int64 comid = Convert.ToInt64(model.insurancename);
                predicate = predicate.And(i => i.companyid == comid);
            }

            // registrationno
            if (model.registrationno != null)
            {
                predicate = predicate.And(i => i.registrationno.ToString() == model.registrationno);
            }
            if (model.quotationno != null)
            {
                predicate = predicate.And(i => i.enquiryid.ToString() == model.quotationno);
            }

            // quotationno
            if (model.policyno != null)
            {
                predicate = predicate.And(i => i.policyno.ToString() == model.policyno.ToString());
            }

            //fromdate todate
            if (model.fromdate != null && model.todate != null)
            {
                predicate = predicate.And(i => EntityFunctions.TruncateTime(i.createdon) >= EntityFunctions.TruncateTime(model.fromdate) && EntityFunctions.TruncateTime(i.createdon) <= EntityFunctions.TruncateTime(model.todate));
            }

            //status
            if (!string.IsNullOrEmpty(model.paymentstatus))
            {
                bool paystatus = true;
                if (model.paymentstatus == "1")
                {
                    paystatus = true;
                }
                else
                {
                    paystatus = false;
                }
                predicate = predicate.And(i => i.paymentstatus == paystatus);
            }

            var data = ap.VW_POLICYDETAILSMASTER.Where(predicate).Select(i => i).Where(x => x.pospid == Common.MySession.UserDetail.userid).ToList();
            ViewBag.PolicyData = data;
        }
        public void FillDropDown_List()
        {
            try
            {
                ViewBag.FillMakeName = objcls.GetAllMake();
                ViewBag.FillInsuranceCompany = objcls.Get_InsCompany();
            }
            catch (Exception Ex)
            {
                Console.Write(Ex.Message);
                LogU.WriteLog("QuotationController >> FillDropDown_List >>" + Ex.Message);
            }
        }

        #endregion




        public ActionResult sort()
        {
            byte[] data = Convert.FromBase64String("eyJkYXRhIjp7InByb3Bvc2Fsbm8iOiJQL0gvMzEyMS8wMDAwMTA0NjY3IiwicHJvZHVjdGNvZGUiOiIzMTIxIiwiZXJyb2NkZSI6IiIsInJuZF9zdHIiOiJ3RkU2Y0hQalhBQmJKcHV4RExWMUNscE9yIiwiaXNfU0FPRCI6ZmFsc2UsInV3X3JlZiI6IiIsInByb2R1Y3RuYW1lIjoiUHJpdmF0ZSBDYXIiLCJwb2xpY3lubyI6IjA2NDAwMS8wMTc3NTMyNTg3LzAwMDAwMC8wMCIsIm1lc3NhZ2UiOiIiLCJzdGF0dXMiOiIxIn0sInN0YXR1cyI6IjEifQ==");
            string decodedString = Encoding.UTF8.GetString(data);

            var proposal_resdata = (JObject)JsonConvert.DeserializeObject(decodedString.ToString());
            var Data = proposal_resdata["data"];
            var status = proposal_resdata["status"].ToString();
            if (status == "1")
            {
                string policyno = Data["policyno"].ToString();
            }
            else
            {
                //payment fail; 
            }
            return View();
        }
        #region Policy Details

        [SessionTimeout]
        public ActionResult PolicyDetails()
        {

            ViewBag.Addolist = objcls.GetAddonList();
            return View();
        }

        #endregion

        [SessionTimeout]
        public ActionResult CarDetails()
        {
            //ViewBag.prvinsurer = GetPrvInsurerSelect();
            ViewBag.makelist = GetMakeSelect();
            return View();
        }

        [SessionTimeout]
        [HttpPost]
        public ActionResult CarDetails(CarDetails model)
        {

            ViewBag.makelist = GetMakeSelect();
            return View();
        }
        [HttpPost]
        public JsonResult GetRtoCity(int stateid)
        {
            long pospcityid = (long)MySession.UserDetail.cityid;
            var result = ap.PospCity_Mapping.Where(x => x.stateid == stateid && x.cityid == pospcityid).FirstOrDefault();
            var obj = objcls.GetAllRto(stateid, "");
            dynamic citylist;
            //result.rtoid = 239;
            if (result != null)
            {
                // AndEnt.STATEMASTERs.OrderByDescending(x => x.stateid == pospstateid).ThenBy(x => x.statename).ToList();
                var data = obj.Select(x => new { stateid = x.stateid, rtodesc = x.rtodesc.ToUpper(), rtoid = x.rtoid }).OrderByDescending(x => x.rtoid == result.rtoid).ThenBy(x => x.rtoid).ToList();
                citylist = data.Select(x => new { stateid = x.stateid, rtodesc = x.rtodesc }).Distinct().ToList();
            }
            else
            {
                var data = obj;
                citylist = obj.Select(x => new { stateid = x.stateid, rtodesc = x.rtodesc.ToUpper() }).Distinct().ToList().OrderBy(x => x.rtodesc).ToList();
            }





            return Json(citylist, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetRto(int stateid, string rtodesc)
        {
            var obj = objcls.GetAllRto(stateid, "");
            var rtolist = obj.Where(x => x.rtodesc == rtodesc).Distinct().ToList();
            return Json(rtolist, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]

        public JsonResult Getstate()
        {
            long pospstateid = Convert.ToInt64(MySession.UserDetail.stateid);
            var obj = objcls.GetAllState(pospstateid).ToArray();
            return Json(obj, JsonRequestBehavior.AllowGet);


        }
        public JsonResult GetMake()
        {
            var obj = objcls.GetMakeWithLogo().ToArray();
            return Json(obj, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult GetModel(int makeid)
        {
            var obj = objcls.GetAllModel(makeid);
            var modelselect = (from db in obj
                               select new
                               {
                                   id = db.modelid,
                                   text = db.modelname

                               }).ToList();
            return Json(new { model = obj.ToArray(), selectmodel = modelselect }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult GetFuel(int modelid)
        {
            var obj = objcls.GetFuel(modelid);
            var fuelselect = (from db in obj
                              select new
                              {

                                  text = db.fueltype

                              }).ToList();
            return Json(new { fuel = obj.ToArray(), fuelselect = fuelselect }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetVariant(int modelid, string fueltype)
        {
            var obj = objcls.GetAllVariant(modelid, fueltype);
            var variantselect = (from db in obj
                                 select new
                                 {
                                     id = db.variantid,
                                     text = db.variantname.ToUpper()
                                 }).ToList().OrderBy(x => x.text);
            return Json(new { variant = obj.ToArray(), selectvariant = variantselect }, JsonRequestBehavior.AllowGet);
        }
        public List<SelectListItem> GetMakeSelect()
        {
            List<SelectListItem> selectlist = new List<SelectListItem>();
            try
            {
                var data = objcls.GetAllMake();
                for (int i = 0; i < data.Count; i++)
                {
                    SelectListItem objselect = new SelectListItem();
                    objselect.Value = data[i].makeid.ToString();
                    objselect.Text = data[i].makename.ToString().ToUpper();
                    selectlist.Add(objselect);
                }
            }
            catch (Exception Ex)
            {
                Console.Write(Ex.Message.ToString());

            }
            return selectlist;
        }
        public JsonResult GetPrvInsurerSelect()
        {
            var data = objcls.Get_InsCompany();
            //for (int i = 0; i < data.Count; i++)
            //{
            //    SelectListItem objselect = new SelectListItem();
            //    objselect.Value = data[i].id.ToString();
            //    objselect.Text = data[i].companyname.ToString().ToUpper();
            //    selectlist.Add(objselect);
            //}
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetFuelType(long modelid)
        {
            var data = objcls.Get_FuelTYpe(modelid);

            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }
        //public List<SelectListItem> GetPrvInsurerSelect()
        //{
        //    List<SelectListItem> selectlist = new List<SelectListItem>();
        //    try
        //    {
        //        var data = objcls.Get_InsCompany();
        //        for (int i = 0; i < data.Count; i++)
        //        {
        //            SelectListItem objselect = new SelectListItem();
        //            objselect.Value = data[i].id.ToString();
        //            objselect.Text = data[i].companyname.ToString().ToUpper();
        //            selectlist.Add(objselect);
        //        }
        //    }
        //    catch (Exception Ex)
        //    {
        //        Console.Write(Ex.Message.ToString());

        //    }
        //    return selectlist;
        //}
        public JsonResult GetManufacturingYear(string policytype)
        {
            var myDate = DateTime.Now;
            List<SelectListItem> YearList = new List<SelectListItem>();
            if (policytype != "New")
            {
                for (int i = 0; i < 15; i++)
                {
                    myDate = myDate.AddYears(-1);
                    SelectListItem objselect = new SelectListItem();
                    objselect.Text = myDate.ToString("yyyy");
                    objselect.Value = myDate.Year.ToString();
                    YearList.Add(objselect);

                }
            }
            else
            {
                SelectListItem objselect = new SelectListItem();
                objselect.Text = myDate.ToString("yyyy");
                objselect.Value = myDate.Year.ToString();
                YearList.Add(objselect);
            }
            return Json(YearList.ToArray(), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public string QualifyCom(QualifyCompany model)
        {
            string apiUrl = ConfigurationManager.AppSettings["default"];
            string inputJson = (new JavaScriptSerializer()).Serialize(model);
            WebClient client = new WebClient();
            client.Headers["Content-type"] = "application/json";
            client.Encoding = Encoding.UTF8;
            string json = client.UploadString(apiUrl + "/QualifyCompany", inputJson);
            return json;
        }
        [HttpPost]
        public string GetQuoteCompanyWise(Quotation model)
        {
            string json = "";

            try
            {
                model.pospid = Common.MySession.UserDetail.userid;

                string apiUrl = ConfigurationManager.AppSettings["PrivateCar"] + model.CompanyName;

                string inputJson = (new JavaScriptSerializer()).Serialize(model);
                WebClient client = new WebClient();
                client.Headers["Content-type"] = "application/json";
                client.Encoding = Encoding.UTF8;
                json = client.UploadString(apiUrl, inputJson);
            }
            catch (Exception)
            {


            }


            return json;
        }


        [HttpPost]
        public string paymentmapping(QualifyCompany model)
        {
            string apiUrl = "http://localhost:17676/api/PrivateCar/PaymentMapping";
            string inputJson = (new JavaScriptSerializer()).Serialize(model);
            WebClient client = new WebClient();
            client.Headers["Content-type"] = "application/json";
            client.Encoding = Encoding.UTF8;
            string json = client.UploadString(apiUrl + "/QualifyCompany", inputJson);
            return json;
        }


        public void InsertEnq(string EnqId, string req, string res, string firstquote, int type)
        {
            Int64 pospid = MySession.UserDetail.userid;
            objcls.InsertEnqDetails(req, res, EnqId, type, firstquote, pospid);
        }

        public JsonResult GetQuoteByEnquiryNo(string enqno)
        {
            var data = objcls.GetQuoteByEnqNo(enqno);
            return Json(new { enquiryrequest = data.enquiryrequest, enquiryresponse = data.enquiryresponse, firstquote = data.firstquote }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetModelid(int variantid)
        {
            var data = objcls.GetModelid(variantid);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #region Proposal
        [HttpGet]
        [Route("Proposal")]
        public ActionResult Proposal(string CompanyName)
        {
            return View();
        }
        [HttpPost]
        public string GetStateCompanywise(int comid)
        {
            string json = "";
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(ConfigurationManager.AppSettings["default"]);

                var responseTask = client.GetAsync("GetStateCompanywise/?compid=" + comid);
                responseTask.Wait();
                //To store result of web api response.
                var result = responseTask.Result;
                //If success received
                if (result.IsSuccessStatusCode)
                {
                    json = result.Content.ReadAsStringAsync().Result;
                }
            }
            return json;
        }
        [HttpPost]
        public JsonResult GetCityCompanywise(int stateid)
        {
            ANDAPPEntities AndEnt = new ANDAPPEntities();
            var city = AndEnt.SP_COMPANY_WISE_CITY("And", stateid).ToArray();
            return Json(city, JsonRequestBehavior.AllowGet);
        }
        public string GetNomineeCompanywise(int comid)
        {

            string json = "";
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(ConfigurationManager.AppSettings["default"]);

                var responseTask = client.GetAsync("GetNomineeRelationship/?compid=" + comid);
                responseTask.Wait();
                //To store result of web api response.
                var result = responseTask.Result;
                //If success received
                if (result.IsSuccessStatusCode)
                {
                    json = result.Content.ReadAsStringAsync().Result;
                }
            }
            return json;
        }
        public string GetOccupationCompanywise(int comid)
        {

            string json = "";
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(ConfigurationManager.AppSettings["default"]);

                var responseTask = client.GetAsync("GetOccupation/?compid=" + comid);
                responseTask.Wait();
                //To store result of web api response.
                var result = responseTask.Result;
                //If success received
                if (result.IsSuccessStatusCode)
                {
                    json = result.Content.ReadAsStringAsync().Result;
                }
            }
            return json;
        }
        public string GetMarritalStatusCompanywise(int comid)
        {

            string json = "";
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(ConfigurationManager.AppSettings["default"].ToString());
                //client.BaseAddress = new Uri("http://localhost:17676/api/default/");
                //client.BaseAddress = new Uri("http://192.168.2.4:86/api/default/");

                var responseTask = client.GetAsync("GetMarritalStatus/?compid=" + comid);
                responseTask.Wait();
                //To store result of web api response.
                var result = responseTask.Result;
                //If success received
                if (result.IsSuccessStatusCode)
                {
                    json = result.Content.ReadAsStringAsync().Result;
                }
            }
            return json;
        }
        public string GetSalutationCompanywise(int comid)
        {

            string json = "";
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("" + ConfigurationManager.AppSettings["default"] + "");

                var responseTask = client.GetAsync("GetSalutation/?compid=" + comid);
                responseTask.Wait();
                //To store result of web api response.
                var result = responseTask.Result;
                //If success received
                if (result.IsSuccessStatusCode)
                {
                    json = result.Content.ReadAsStringAsync().Result;
                }
            }
            return json;
        }

        public string GetFinancerLIst(int comid)
        {

            string json = "";
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(ConfigurationManager.AppSettings["default"]);
                var responseTask = client.GetAsync("GetFinancerLIst/?compid=" + comid);
                responseTask.Wait();
                //To store result of web api response.
                var result = responseTask.Result;
                //If success received
                if (result.IsSuccessStatusCode)
                {
                    json = result.Content.ReadAsStringAsync().Result;
                }
            }
            return json;
        }
        public string GetComapnyIdbyName(string company)
        {

            string json = "";
            using (var client = new HttpClient())
            {

                client.BaseAddress = new Uri(ConfigurationManager.AppSettings["default"]);
                var responseTask = client.GetAsync("GetCompanyIdbyName/?company=" + company);
                responseTask.Wait();
                //To store result of web api response.
                var result = responseTask.Result;
                //If success received
                if (result.IsSuccessStatusCode)
                {
                    json = result.Content.ReadAsStringAsync().Result;
                }
            }
            return json;
        }
        public string Getproposal(Quotation model)
        {
            string json = "";

            try
            {
                model.pospid = Common.MySession.UserDetail.userid;
                string apiUrl = ConfigurationManager.AppSettings["Proposal"];

                string inputJson = (new JavaScriptSerializer()).Serialize(model);
                WebClient client = new WebClient();
                client.Headers["Content-type"] = "application/json";
                client.Encoding = Encoding.UTF8;
                json = client.UploadString(apiUrl, inputJson);
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }


            return json;
        }


        #endregion
        #region Payment
        public ActionResult ReviewAndPay()
        {
            return View();
        }
        [HttpPost]
        public string MakePayment(PaymentRequest model)
        {
            string json = "";

            try
            {

                string apiUrl = ConfigurationManager.AppSettings["PaymentRequestDetails"];
                string inputJson = (new JavaScriptSerializer()).Serialize(model);
                WebClient client = new WebClient();
                client.Headers["Content-type"] = "application/json";
                client.Encoding = Encoding.UTF8;
                json = client.UploadString(apiUrl, inputJson);
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
            return json.Replace("\"", "");

        }




        public string RgeneratePayment(string enqid, string companyid)
        {

            string json = "";
            CompanyWiseRefference comref = new CompanyWiseRefference();
            PaymentRequest model = new PaymentRequest();

            try
            {


                if (companyid.Equals("9"))
                {
                    var data = ap.icici_paymenttagging.Where(x => x.EnqNo == enqid).FirstOrDefault();
                    if (data != null)
                    {
                        comref.CorrelationId = data.TransactionId;
                        model.CompanyDetail = comref;
                        model.FinalPremium = Convert.ToDouble(data.PaymentAmount);
                        model.CompanyName = Company.ICICI;
                    }
                }

                else if (companyid.Equals("22"))
                {
                    var propdata = ap.GET_Payment_Parameter(enqid, 22).FirstOrDefault();
                    comref.OrderNo = propdata.parameter_value;
                    model.CompanyDetail = comref;
                    model.CompanyName = Company.TATA;

                }
                else if (companyid.Equals("6"))
                {
                    var policyno = ap.Payment_Parameter.Where(x => x.parameter_name == "policyno" && x.enq_id == enqid).FirstOrDefault().parameter_value.ToString();
                    var applicationid = ap.Payment_Parameter.Where(x => x.parameter_name == "applicationid" && x.enq_id == enqid).FirstOrDefault().parameter_value.ToString();
                    if (policyno != null && applicationid != null)
                    {
                        comref.QuoteNo = policyno;
                        comref.applicationId = applicationid;
                        model.CompanyDetail = comref;
                        model.CompanyName = Company.DIGIT;
                    }
                }
                else if (companyid.Equals("16"))
                {
                    //var j = ap.COMPANY_WISE_STATE_CITY_AREA_MASTER.Where(x => x.companyid == comid).Select(x => new { compid = x.companyid, statename = x.statename, value = x.statevalue, id = (long)x.stateid, status = x.status }).Distinct();
                    var j = ap.Payment_Parameter.Where(x => x.com_id == 16 && x.enq_id == enqid).OrderByDescending(x => x.id).ToList();
                    if (j != null)
                    {
                        comref.applicationId = j[0].parameter_value;// "TAPI150621221129";
                        comref.OrderNo = j[2].parameter_value;// "TAPI150621221129";
                        comref.QuoteNo = j[1].parameter_value;//"QUOT_MOT_000232939_15062021";
                        model.CompanyDetail = comref;
                        model.CompanyName = Company.RAHEJA;
                        //QuoteNo", model.CompanyDetail.QuoteNo

                    }
                }



                string apiUrl = ConfigurationManager.AppSettings["PaymentRequestDetails"];
                string inputJson = (new JavaScriptSerializer()).Serialize(model);
                WebClient client = new WebClient();
                client.Headers["Content-type"] = "application/json";
                client.Encoding = Encoding.UTF8;
                json = client.UploadString(apiUrl, inputJson);
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
            return json.Replace("\"", "");

        }

        public ActionResult FutureRegenerate(string enqid, string companyid)
        {

            CompanyWiseRefference comref = new CompanyWiseRefference();
            PaymentRequest model = new PaymentRequest();
            var jo = ap.VW_POLICYDETAILSMASTER.Where(x => x.companyid == 5 && x.enquiryid == enqid).OrderByDescending(x => x.pdid).Select(x => new { custfname = x.customerfirstname, custmname = x.customermiddlename, custlname = x.customerlastname, email = x.customeremailid, amt = x.finalpremium, mobile = x.customermobileno }).ToList();
            //var jo = ap.POLICYDETAILSMASTER.Where(x => x.companyid == 5 && x.enquiryid == enqid).OrderByDescending(x => x.pdid).Select(x => new { custfname = x.customerfirstname, custmname = x.customermiddlename, custlname = x.customerlastname, email = x.customeremailid, amt = x.finalpremium }).ToList();
            if (jo != null)
            {
                model.CompanyName = Company.FGI;
                model.FirstName = Convert.ToString(jo[0].custfname).Trim();
                model.FinalPremium = Convert.ToDouble(Math.Round(Convert.ToDouble(jo[0].amt)));
                if (!string.IsNullOrEmpty(jo[0].custmname))
                {
                    model.LastName = (jo[0].custmname + " " + jo[0].custlname).Trim();
                }
                else
                {
                    model.LastName = jo[0].custlname.Trim();
                }
                model.EmailId = jo[0].email;
                model.MobileNo = jo[0].mobile;
                comref.OrderNo = enqid;
                model.CompanyDetail = comref;
                TempData["reqmodel"] = model;
                return RedirectToAction("FuturePayment", "Payment");

            }
            return null;
        }
        #endregion

        #region added by pratik for SharePDF



        [HttpPost]
        public ActionResult sharepdf(List<MotorQuoteSharePDF> sharepdf)
        {


            //v.PospName = MySession.UserDetail.username;
            //v.PospCode = MySession.UserDetail.pospcode;
            //v.PospMobile = MySession.UserDetail.mobileno;
            //v.PospEmail = MySession.UserDetail.emailid;

            UriBuilder tempUri = new UriBuilder(Request.Url);
            tempUri.Path = Url.Action("GetQuotation", "PrivateCar", new { area = "Motor" });
            string baseUrl = tempUri.ToString();

            string pdf_page_size = "A4";
            PdfPageSize pageSize = (PdfPageSize)Enum.Parse(typeof(PdfPageSize),
                pdf_page_size, true);

            string pdf_orientation = "Portrait";
            PdfPageOrientation pdfOrientation =
                (PdfPageOrientation)Enum.Parse(typeof(PdfPageOrientation),
                pdf_orientation, true);

            int webPageWidth = 1024;
            int webPageHeight = 0;

            int MarginLeft = 10;
            int MarginRight = 10;
            int MarginTop = 10;
            int MarginBottom = 5;

            try
            {
                webPageWidth = Convert.ToInt32(1024);
            }
            catch { }


            try
            {
                webPageHeight = Convert.ToInt32(0);
            }
            catch { }

            // instantiate a html to pdf converter object
            HtmlToPdf converter = new HtmlToPdf();



            // set converter options
            converter.Options.PdfPageSize = pageSize;
            converter.Options.PdfPageOrientation = pdfOrientation;
            converter.Options.WebPageWidth = webPageWidth;
            converter.Options.WebPageHeight = webPageHeight;
            converter.Options.MarginLeft = MarginLeft;
            converter.Options.MarginRight = MarginRight;
            converter.Options.MarginTop = MarginTop;
            converter.Options.MarginBottom = MarginBottom;

            // footer settings
            converter.Options.DisplayFooter = true;
            converter.Footer.DisplayOnFirstPage = true;
            converter.Footer.DisplayOnOddPages = true;
            converter.Footer.DisplayOnEvenPages = true;
            converter.Footer.Height = 50;

            string QuoteNo = sharepdf[0].QuoteNo == null ? "" : sharepdf[0].QuoteNo;
            // page numbers can be added using a PdfTextSection object
            string page_number = "Q.No : " + QuoteNo + "\n" + "Page: {page_number} of {total_pages}";

            //PdfTextSection text = new PdfTextSection(0, 10, 550,page_number, new System.Drawing.Font("Lato-Regular", 8), System.Drawing.Color.FromArgb(85,85,85));
            PdfTextSection text = new PdfTextSection(0, 10, page_number, new System.Drawing.Font("Lato-Regular", 8));
            text.HorizontalAlign = PdfTextHorizontalAlign.Right;
            converter.Footer.Add(text);

            converter.Options.HttpPostParameters.Add("sharepdf", JsonConvert.SerializeObject(sharepdf));

            string FileName = QuoteNo + DateTime.Now.ToString("dd_MMM_yyyyhhmmss") + ".pdf";
            string fullPath = Path.Combine(Server.MapPath("~/SharePDF"), FileName);

            // create a new pdf document converting an url
            PdfDocument doc = converter.ConvertUrl(baseUrl);
            // save pdf document
            //byte[] pdf = doc.Save();
            doc.Save(fullPath);

            // close pdf document
            doc.Close();

            // return resulted pdf document

            //FileResult fileResult = new FileContentResult(pdf, "application/pdf");
            //fileResult.FileDownloadName = FileName;
            //return fileResult;
            //return Json(new { FileName =FileName}, JsonRequestBehavior.AllowGet);
            return new JsonResult { Data = new { FileName = FileName } };



        }

        [HttpPost]
        public ViewResult GetQuotation(string sharepdf)
        {

            //var a = Common.MySession.UserDetail.pospcode;
            List<MotorQuoteSharePDF> lstSharepdf = JsonConvert.DeserializeObject<List<MotorQuoteSharePDF>>(sharepdf);
            return View(lstSharepdf);
        }
        [HttpGet]
        public virtual ActionResult Download(string file)
        {
            string fullPath = Path.Combine(Server.MapPath("~/SharePDF"), file);
            return File(fullPath, "application/pdf", file);
        }
        #endregion added by pratik for SharePDF




        public JsonResult CheckValidRTO(string rto)
        {
            bool IsValidRto = true;
            var rtocode = rto.Substring(0, 4);
            var data = ap.RTOMASTER_ANDAPP.Where(x => x.rtocode == rtocode).FirstOrDefault();
            if (data == null)
                IsValidRto = false;

            return Json(IsValidRto, JsonRequestBehavior.AllowGet);
        }

        public string GenerateDIGITPDF(string applicationid, string status, string policyno)
        {
            string json = "";
            try
            {
                if (status == "success")
                {
                    using (var client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(ConfigurationManager.AppSettings["PrivateCar"]);

                        var responseTask = client.GetAsync("DIGITPolicyPdf/?applicationid=" + applicationid);
                        responseTask.Wait();
                        //To store result of web api response.
                        var result = responseTask.Result;
                        //If success received
                        if (result.IsSuccessStatusCode)
                        {
                            json = result.Content.ReadAsStringAsync().Result;
                            json = json.Replace("\"", "");
                            using (WebClient ccc = new WebClient())
                            {
                                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                                ccc.DownloadFile(json, ConfigurationManager.AppSettings["savepdf"].ToString() + policyno + ".pdf");
                            }
                        }
                        else
                        {
                            json = "serviceerror";
                        }
                    }
                }
                else
                {
                    json = "error";
                }
            }

            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
            return json.Replace("\"", "");
        }
        public string GetPolicystatus(string policyno)
        {
            string json = "";
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(ConfigurationManager.AppSettings["PrivateCar"]);

                    var responseTask = client.GetAsync("DIGITPolicySearch/?policyno=" + policyno);
                    responseTask.Wait();
                    //To store result of web api response.
                    var result = responseTask.Result;
                    //If success received
                    if (result.IsSuccessStatusCode)
                    {
                        json = result.Content.ReadAsStringAsync().Result;
                        json = json.Replace("\"", "");
                        var policystatus = json.Split(',')[0].ToString().Split('=')[1];
                        var policystate = json.Split(',')[1].ToString().Split('=')[1];
                        if (policystatus == "EFFECTIVE" && policystate == "CONTRACT")
                            json = "success";
                    }

                }
            }

            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
            return json;
        }

        [HttpPost]
        public JsonResult GetstateRaheja(int comid)
        {
            //model = AndEnt.SP_COMPANY_WISE_STATE_MASTER(comid).Select(x => new COMPANY_WISE_STATE_MASTER { compid = x.compid, statename = x.statename, value = x.value, id = (long)x.mstateid, status = x.status }).ToList();
            var j = ap.COMPANY_WISE_STATE_CITY_AREA_MASTER.Where(x => x.companyid == comid).Select(x => new { compid = x.companyid, statename = x.statename, value = x.statevalue, id = (long)x.stateid, status = x.status }).Distinct();
            //var j=ap.COMPANY_WISE_STATE_CITY_AREA_MASTER.Where(x => x.companyid == comid).Select(m => new { m.stateid,m.statename,m.statevalue }).Distinct();
            //var j = app.COMPANY_WISE_STATE_CITY_AREA_MASTER.Where(x => x.companyid == 16 && x.stateid==18).Select(m => new { m.cityid, m.cityname,m.cityvalue}).Distinct();
            //var j = ap.COMPANY_WISE_STATE_CITY_AREA_MASTER.Where(x => x.companyid == comid && x.cityid == 2043).Select(m => new {  m.areaname, m.areavalue }).Distinct();
            return Json(j, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public JsonResult GetcityRaheja(int stateid)
        {
            //var j=app.COMPANY_WISE_STATE_CITY_AREA_MASTER.Where(x => x.companyid == 16).Select(m => new { m.stateid,m.statename }).Distinct();
            //var j = ap.COMPANY_WISE_STATE_CITY_AREA_MASTER.Where(x => x.companyid == comid).Select(x => new { compid = x.companyid, statename = x.statename, value = x.statevalue, id = (long)x.stateid, status = x.status }).Distinct();
            var j = ap.COMPANY_WISE_STATE_CITY_AREA_MASTER.Where(x => x.companyid == 16 && x.stateid == stateid).Select(m => new { id = (long)m.cityid, cityname = m.cityname, value = m.cityvalue }).Distinct();
            //var j = app.COMPANY_WISE_STATE_CITY_AREA_MASTER.Where(x => x.companyid == 16 && x.cityid == 2043).Select(m => new { m.areaname, m.areavalue }).Distinct();
            return Json(j, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public JsonResult GetareaRaheja(int cityid)
        {

            //var j=app.COMPANY_WISE_STATE_CITY_AREA_MASTER.Where(x => x.companyid == 16).Select(m => new { m.stateid,m.statename }).Distinct();
            //var j = app.COMPANY_WISE_STATE_CITY_AREA_MASTER.Where(x => x.companyid == 16 && x.stateid == stateid).Select(m => new { m.cityid, m.cityname, m.cityvalue }).Distinct();
            var j = ap.COMPANY_WISE_STATE_CITY_AREA_MASTER.Where(x => x.companyid == 16 && x.cityid == cityid).Select(m => new { areaname = m.areaname, value = m.areavalue }).Distinct();
            return Json(j, JsonRequestBehavior.AllowGet);

        }


        [HttpPost]
        public JsonResult top_make_model_variant(int make, int model, int variant)

        {
            ap.SP_TOP_MMV(make, model, variant);

            return Json("", JsonRequestBehavior.AllowGet);

        }


        [HttpPost]
        public JsonResult chkod(string manufYear)
        {
        
            dynamic citylist;

            var Month = DateTime.Now.Month;
             var mydate=Convert.ToDateTime(Month+"/"+"01"+"/"+ manufYear).ToShortDateString();

           var  oddate = Convert.ToDateTime("9/1/18").ToShortDateString();

            if (Convert.ToDateTime(mydate)>=Convert.ToDateTime(oddate))
            {
                citylist = "ok";
            }
            else
            {
                citylist = "not ok";
            }
         



            return Json(citylist, JsonRequestBehavior.AllowGet);
        }
    }

    public class ResponseModel
    {
        public bool success;

        public string message;
        public object data { get; set; }
        public string enquiryid { get; set; }
    }
}