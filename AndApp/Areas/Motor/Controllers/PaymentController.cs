using AndApp.Models;
using AndApp.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using static AndApp.Models.CommonModels;
using static AndApp.Utilities.Common;

namespace AndApp.Areas.Motor.Controllers
{
    public class PaymentController : Controller
    {
        DAL.ANDAPPEntities ap = new DAL.ANDAPPEntities();

        #region HDFC PAYMENT START
        public ActionResult HDFCPayment(string transactionno, string enquiryid)
        {
            string pay = string.Empty, ChecksumRequest = string.Empty, ChecksumResponse = string.Empty, strForm = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(transactionno))
                {
                    var gettrno = ap.SP_REQUEST_RESPONSE_MASTER("G", enquiryid, 8, null, null).ToList();
                    if (gettrno.Count > 0)
                    {
                        dynamic Response = (JObject)JsonConvert.DeserializeObject(gettrno[0].response.ToString());
                        string responseMessage = Convert.ToString(Response.Message);
                        if (string.IsNullOrEmpty(responseMessage))
                        {
                            dynamic responseData = Response.Data;
                            transactionno = Convert.ToString(responseData.TransactionNo);
                        }
                    }
                }
                var GetEnqNo = ap.GET_Payment_Parameter(transactionno, 8).ToList();
                if (GetEnqNo.Count == 0)
                {
                    ap.SP_Payment_Parameter(enquiryid, 8, "TxnNo", transactionno);
                }


                string Trnsno = transactionno;//resModel.CompanyWiseRefference.OrderNo;

                ChecksumRequest = "IAND INSURANCE BROKER" + "|" + Trnsno + "|" + "pce7d5CDaVuvXasix7H6jw==" + "|" + "S001";
                ChecksumResponse = GenerateCheckSumResponse(ChecksumRequest).ToUpper();

                string action1 = "https://uatpg.hdfcergo.com/PaymentGateway/Payments.aspx";

                try
                {
                    System.Collections.Hashtable data = new System.Collections.Hashtable(); // adding values in gash table for data post
                    data.Add("Trnsno", Trnsno);
                    data.Add("FeatureID", "S001");
                    data.Add("Checksum", ChecksumResponse);

                    strForm = PreparePOSTForm(action1, data);
                }
                catch (Exception ex)
                {
                    Console.Write(Convert.ToString(ex.Message));
                }
            }
            catch (Exception ex)
            {
                Console.Write(Convert.ToString(ex.Message));
            }

            return Content(strForm, System.Net.Mime.MediaTypeNames.Text.Html);

        }
        private string PreparePOSTForm(string url, System.Collections.Hashtable data)      // post form
        {
            //Set a name for the form
            string formID = "PostForm";
            //Build the form using the specified data to be posted.
            StringBuilder strForm = new StringBuilder();
            strForm.Append("<form id=\"" + formID + "\" name=\"" + formID + "\" action=\"" + url + "\" method=\"POST\">");

            foreach (System.Collections.DictionaryEntry key in data)
            {
                strForm.Append("<input type=\"hidden\" name=\"" + key.Key + "\" value=\"" + key.Value + "\">");
            }

            strForm.Append("</form>");
            //Build the JavaScript which will do the Posting operation.
            StringBuilder strScript = new StringBuilder();
            strScript.Append("<script language='javascript'>");
            strScript.Append("var v" + formID + " = document." + formID + ";");
            strScript.Append("v" + formID + ".submit();");
            strScript.Append("</script>");
            //Return the form and the script concatenated.
            //(The order is important, Form then JavaScript)
            return strForm.ToString() + strScript.ToString();
        }
        public static string GenerateCheckSumResponse(string ChecksumRequest)
        {
            using (SHA512Managed sha1 = new SHA512Managed())
            {
                var hashSh1 = sha1.ComputeHash(Encoding.UTF8.GetBytes(ChecksumRequest));

                // declare stringbuilder
                var sb = new StringBuilder(hashSh1.Length * 2);

                // computing hashSh1
                foreach (byte b in hashSh1)
                {
                    // "x2"
                    sb.Append(b.ToString("X2").ToLower());
                }
                return sb.ToString();
            }
        }
        #endregion

        #region FUTURE PAYMENT 
        public JsonResult setdata(PaymentRequest reqModel)
        {

            TempData["reqmodel"] = reqModel;
            return Json("", JsonRequestBehavior.AllowGet);
        }

        public ActionResult FuturePayment(PaymentRequest reqModel)

        {

            var a = TempData["reqmodel"];
            reqModel = a as PaymentRequest;
            string tranid = reqModel.CompanyDetail.OrderNo;
            string payopt = "3"; //1[PayTm] OR 2[HDFC] OR 3[Pay U]
            string resurl = ConfigurationManager.AppSettings["FGIPAYMENTRETURN"].ToString();
            string proposaNo = reqModel.CompanyDetail.OrderNo;

            string premamount = Convert.ToString(Math.Round(reqModel.FinalPremium));
            string useridentifier = "NA";
            string userid = string.Empty;
            if (!string.IsNullOrEmpty(reqModel.MiddleName))
            {
                reqModel.LastName = reqModel.MiddleName + " " + reqModel.LastName;
            }
            string firstname = reqModel.FirstName.Trim();
            string lastname = reqModel.LastName.Trim();
            string mobile = reqModel.MobileNo;
            string email = reqModel.EmailId;
            string vendortype = "0";
            DAL.ANDAPPEntities apent = new DAL.ANDAPPEntities();
            apent.SP_Payment_Parameter(reqModel.CompanyDetail.OrderNo, 5, "TransactionID", tranid);
            apent.SP_Payment_Parameter(reqModel.CompanyDetail.OrderNo, 5, "ProposalNumber", proposaNo);
            apent.SP_Payment_Parameter(reqModel.CompanyDetail.OrderNo, 5, "PremiumAmount", premamount);
            //apent.SP_Payment_Parameter(reqModel.CompanyDetail.OrderNo, 5, "TransactionID", tranid);
            string texttoencrypt = tranid + "|" + payopt + "|" + resurl + "|" + proposaNo + "|" + premamount + "|" + useridentifier + "|" + userid + "|" + firstname + "|" + lastname + "|" + mobile + "|" + email + "|";
            LogU.WritePaymentLog("FGI >> PaymentController >> FuturePayment >> TextToEncrpt >> " + texttoencrypt);
            string checksum = Generatehash256ForFuture(texttoencrypt);
            LogU.WritePaymentLog("FGI >> PaymentController >> FuturePayment>> CheckSum >> " + checksum);
            //apent.SP_Payment_Parameter(reqModel.CompanyDetail.OrderNo, 5, "CheckSum", checksum);
            string action1 = "http://fglpg001.futuregenerali.in/Ecom_NL/WEBAPPLN/UI/Common/WebAggPayNew.aspx";
            string strForm = "";

            try
            {
                System.Collections.Hashtable data = new System.Collections.Hashtable(); // adding values in gash table for data post
                                                                                        //   CheckSum = Generatehash256(texttoencrypt);
                                                                                        //data.Add("CheckSum", checksum);
                data.Add("TransactionID", tranid);
                data.Add("PaymentOption", payopt);
                data.Add("ResponseURL", resurl);
                data.Add("ProposalNumber", proposaNo);
                data.Add("PremiumAmount", premamount);
                data.Add("UserIdentifier", useridentifier);
                data.Add("UserId", userid);
                data.Add("FirstName", firstname);
                data.Add("LastName", lastname);
                data.Add("Mobile", mobile);
                data.Add("Email", email);
                data.Add("Vendor", vendortype);
                data.Add("CheckSum", checksum);

                strForm = FuturePOSTForm(action1, data);

            }
            catch (Exception ex)
            {
                Response.Write("<span style='color:red'>" + ex.Message + "</span>");
            }

            return Content(strForm, System.Net.Mime.MediaTypeNames.Text.Html);
        }
        public ActionResult FutureResponse()
        {
            DAL.ANDAPPEntities ap = new DAL.ANDAPPEntities();
            ViewBag.pospname = MySession.UserDetail.username;
            PaymentRequest model = new PaymentRequest();
            CRMAPIController data = new CRMAPIController();
            model.CompanyDetail = new CompanyWiseRefference();
            string Response = "", WS_P_ID = "", TID = "", PGID = "", Premium = "";
            string json = "";
            try
            {


                if (Request.QueryString.Count > 0)
                {

                    string ResponseData = Request.QueryString["ResponseData"].ToString();
                    //BUYidAjRUV7INIiBelXXi5a7ZOJBIVudbkQWxONlnZGNjRPKGAAwlDfALnGny2iP / yXzIRmaTUnuhkB8tpg7BZlTphe8e / 7lw1jBQtK7lQSmEq0W2Rxlqx5SIcG5Dqpf7uo03ACY1 / w =
                    LogU.WritePaymentLog("FGI >> PaymentController >> FutureResponse >> " + ResponseData);
                    ResponseData = ResponseData.Replace("$", "+");
                    string dd = DecryptText(ResponseData);
                    string[] Spilt = dd.Split('&');
                    Response = Spilt[4];
                    WS_P_ID = Spilt[0];
                    TID = Spilt[1];
                    PGID = Spilt[2];
                    Premium = Spilt[3];
                    if (Response.Contains("Fail"))
                    {
                        ViewBag.payment = "Payment Fail";
                    }
                    else
                    {
                        model.CompanyName = Company.FGI;
                        string[] TIDspi = TID.Split('=');
                        string[] WS_P_IDspi = WS_P_ID.Split('=');
                        string[] PGIDspi = PGID.Split('=');
                        string[] Premiumspi = Premium.Split('=');
                        string[] Responsespi = Response.Split('=');
                        model.CompanyDetail.OrderNo = TIDspi[1];
                        model.CompanyDetail.applicationId = WS_P_IDspi[1];
                        model.CompanyDetail.CorrelationId = PGIDspi[1];
                        var data1 = ap.GET_Payment_Parameter(model.CompanyDetail.OrderNo, 5).ToList();
                        model.CompanyDetail.LoadingDiscount = data1[3].parameter_value;
                        var pospid = ap.VW_POLICYDETAILSMASTER.Where(x => x.enquiryid == model.CompanyDetail.OrderNo && x.companyid == 5).FirstOrDefault();
                        //model.CompanyDetail.QuoteId = TIDspi[1];
                        //var XmlReq=    ap.SP_REQUEST_RESPONSE_MASTER("G", model.CompanyDetail.OrderNo, 5,null, null).FirstOrDefault();
                        var XmlReq = ap.SP_REQUEST_RESPONSE_MASTER("G", model.CompanyDetail.OrderNo, 5, string.Empty, string.Empty).FirstOrDefault();
                        model.CompanyDetail.QuoteId = XmlReq.request;
                        LogU.WritePaymentLog("Akash >> PaymentController >> FutureResponse >> " + XmlReq.requestedon);
                        // model.CompanyDetail.QuoteNo = Convert.ToDateTime(XmlReq.requestedon).ToString("dd-MM-yyyy");
                        model.CompanyDetail.QuoteNo = DateTime.Now.ToString("dd/MM/yyyy").Replace('-', '/');
                        //model.CompanyDetail.QuoteNo = Convert.ToDateTime(XmlReq.requestedon.Value).ToString("dd-MM-yyyy");
                        LogU.WritePaymentLog("Trupti >> PaymentController >> FutureResponse >> " + Convert.ToDateTime(XmlReq.requestedon.Value).ToString("dd-MM-yyyy"));
                        LogU.WritePaymentLog("Trupti >> PaymentController >> FutureResponse >> " + XmlReq.requestedon.Value);

                        model.FinalPremium = Convert.ToDouble(Premiumspi[1]);
                        string apiUrl = ConfigurationManager.AppSettings["ProposalAfterPayment"];
                        string inputJson = (new JavaScriptSerializer()).Serialize(model);
                        WebClient client = new WebClient();
                        client.Headers["Content-type"] = "application/json";
                        client.Encoding = Encoding.UTF8;
                        json = client.UploadString(apiUrl, inputJson);
                        var detailsFGI = JObject.Parse(json);
                        string err = Convert.ToString(detailsFGI["ErrorMsg"]);
                        if (string.IsNullOrEmpty(err))
                        {
                            //ViewBag.pdfurl = Convert.ToString(detailsFGI["PolicyPdfUrl"]);
                            ViewBag.payment = "Payment Successful";
                            ViewBag.IsPolicyNo = "True";
                            ViewBag.PolicyNo = Convert.ToString(detailsFGI["PolicyNo"]);
                            ap.SP_POLICYDETAILSMASTER("U", model.CompanyDetail.OrderNo, 5, pospid.pospid,
                                    null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
                                    null, null, null, null, null, null, null, null, null, null, null, null, Convert.ToString(detailsFGI["PolicyNo"]), null, null,
                                    null, null, 1, null, null, null, null, null, null, null, null, null, null, true);

                            if (!string.IsNullOrEmpty(Convert.ToString(detailsFGI["PolicyNo"])))
                            {
                                data.FetchData(5, model.CompanyDetail.OrderNo, model.CompanyDetail.CorrelationId);
                            }


                            var pospdata = ap.POSPMASTERs.Where(x => x.pospid == pospid.pospid).FirstOrDefault();
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
                        }
                        else
                        {
                            ViewBag.payment = "Payment Fail";
                            ViewBag.EnqNo = model.CompanyDetail.OrderNo;
                            string datep = DateTime.UtcNow.ToString("D");
                            ViewBag.paymentDate = datep;
                        }
                    }
                }

                ViewBag.EnqNo = model.CompanyDetail.OrderNo;
                string date = DateTime.UtcNow.ToString("D");
                ViewBag.paymentDate = date;
                return View();
            }
            catch (Exception ex)
            {
                LogU.WritePaymentLog("FGI >> PaymentController >> FutureResponse >> " + ex.ToString());
                ViewBag.EnqNo = model.CompanyDetail.OrderNo;
                string date = DateTime.UtcNow.ToString("D");
                ViewBag.paymentDate = date;
                ViewBag.payment = "Payment Fail";
                return View();
            }
        }
        public string Generatehash256ForFuture(string text)
        {
            byte[] message = Encoding.UTF8.GetBytes(text);
            byte[] hashValue;
            SHA256Managed hashString = new SHA256Managed();
            string hex = "";
            hashValue = hashString.ComputeHash(message);
            foreach (byte x in hashValue)
            {
                hex += String.Format("{0:x2}", x);
            }

            return hex;
        }
        private string FuturePOSTForm(string url, System.Collections.Hashtable data)      // post form
        {
            //Set a name for the form
            string formID = "PostForm";
            //Build the form using the specified data to be posted.
            StringBuilder strForm = new StringBuilder();
            strForm.Append("<form id=\"" + formID + "\" name=\"" + formID + "\" action=\"" + url + "\" method=\"POST\">");

            foreach (System.Collections.DictionaryEntry key in data)
            {
                strForm.Append("<input type=\"hidden\" name=\"" + key.Key + "\" value=\"" + key.Value + "\">");
            }

            strForm.Append("</form>");
            //Build the JavaScript which will do the Posting operation.
            StringBuilder strScript = new StringBuilder();
            strScript.Append("<script language='javascript'>");
            strScript.Append("var v" + formID + " = document." + formID + ";");
            strScript.Append("v" + formID + ".submit();");
            strScript.Append("</script>");
            //Return the form and the script concatenated.
            //(The order is important, Form then JavaScript)
            return strForm.ToString() + strScript.ToString();
        }
        public string EncryptText(string strText)
        {
            return doEncrypt(strText, "&%#@?,:*");
        }
        public string DecryptText(string strText)
        {
            return doDecrypt(strText, "&%#@?,:*");
        }
        //The function used to encrypt the text
        private string doEncrypt(string strText, string strEncrKey)
        {
            byte[] byKey;
            byte[] IV = { 18, 50, 80, 125, 140, 170, 205, 230 };
            try
            {
                byKey = System.Text.Encoding.UTF8.GetBytes(strEncrKey.Substring(0, 8));
                DESCryptoServiceProvider des = new DESCryptoServiceProvider();
                byte[] inputByteArray = System.Text.Encoding.UTF8.GetBytes(strText);
                MemoryStream ms = new MemoryStream();
                CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(byKey, IV),
               CryptoStreamMode.Write);
                cs.Write(inputByteArray, 0, inputByteArray.Length);
                cs.FlushFinalBlock();
                return Convert.ToBase64String(ms.ToArray());
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        //The function used to decrypt the text
        private string doDecrypt(string strText, string sDecrKey)
        {
            byte[] byKey;
            byte[] IV = new byte[8] { 18, 50, 80, 125, 140, 170, 205, 230 };
            byte[] inputByteArray = new byte[strText.Length];
            try
            {
                byKey = System.Text.Encoding.UTF8.GetBytes(sDecrKey.Substring(0, 8));
                DESCryptoServiceProvider des = new DESCryptoServiceProvider();
                inputByteArray = Convert.FromBase64String(strText);
                MemoryStream ms = new MemoryStream();
                CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(byKey, IV),
               CryptoStreamMode.Write);
                cs.Write(inputByteArray, 0, inputByteArray.Length);
                cs.FlushFinalBlock();
                System.Text.Encoding encoding = System.Text.Encoding.UTF8;
                return encoding.GetString(ms.ToArray());
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }


        #endregion

        #region RAHEJA
        public ActionResult RahejaPaymentReturn()
        {
            CRMAPIController data = new CRMAPIController();
            string json = "";
            try
            {

                //string s = "mihpayid=403993715523407661&mode=CC&status=success&unmappedstatus=captured&key=uaWLkx&txnid=C020721028551&amount=18552.00&cardCategory=domestic&discount=0.00&net_amount_debit=18552&addedon=2021-07-02+11%3A28%3A03&productinfo=MOTOR&firstname=trtrt+dfdg&lastname=&address1=&address2=&city=&state=&country=&zipcode=&email=fdgdf%40fgfg.com&phone=9093858943&udf1=&udf2=&udf3=&udf4=&udf5=&udf6=&udf7=&udf8=&udf9=&udf10=&hash=1c431be4f911af5344d7593f8788d8c6c8dfa9f5c49873f7bbab511e69ed3b446230311d8db2bcb1d6f44ec9cad11f7501f3d729f1cca9d6ec57942c3bea5242&field1=139988&field2=912939&field3=20210702&field4=0&field5=499242052384&field6=00&field7=AUTHPOSITIVE&field8=Approved+or+completed+successfully&field9=No+Error&payment_source=payu&PG_TYPE=AXISPG&bank_ref_num=139988&bankcode=CC&error=E000&error_Message=No+Error&name_on_card=dfdsg&cardnum=401200XXXXXX1112&cardhash=This+field+is+no+longer+supported+in+postback+params.";
                string s = Request.Form.ToString();
                LogU.WritePaymentLog("RAHEJA >> PaymentController >> RahejaPaymentReturn >> RequestForm >> " + Request.Form.ToString());
                string enqid = string.Empty;
                DAL.ANDAPPEntities andEnt = new DAL.ANDAPPEntities();
                LogU.WritePaymentLog("RAHEJA >> PaymentController >> RahejaPaymentReturn >> FormData >> " + s);
                string[] Spilt = s.Split('&');
                string paymentStatus = Spilt[2];
                string[] TIDspi = paymentStatus.Split('=');
                string sfdf = Spilt[5].ToString();
                string[] TID = sfdf.Split('=');
                var data1 = andEnt.GET_Payment_Parameter(TID[1], 16).ToList();
                enqid = data1[0].enq_id;
                var pospid = ap.VW_POLICYDETAILSMASTER.Where(x => x.enquiryid == enqid && x.companyid == 16).FirstOrDefault();
                if (TIDspi[1].Contains("suc"))
                {
                    PaymentRequest model = new PaymentRequest();
                    model.CompanyDetail = new CompanyWiseRefference();
                    model.CompanyName = Company.RAHEJA;
                    model.CompanyDetail.applicationId = data1[0].parameter_value;// "TAPI150621221129";
                    model.CompanyDetail.OrderNo = data1[2].parameter_value;// "TAPI150621221129";
                    model.CompanyDetail.QuoteNo = data1[1].parameter_value;//"QUOT_MOT_000232939_15062021";
                    string apiUrl = ConfigurationManager.AppSettings["ProposalAfterPayment"];
                    string inputJson = (new JavaScriptSerializer()).Serialize(model);
                    WebClient client = new WebClient();
                    client.Headers["Content-type"] = "application/json";
                    client.Encoding = Encoding.UTF8;
                    json = client.UploadString(apiUrl, inputJson);
                    var details = JObject.Parse(json);
                    string dd = Convert.ToString(details["ErrorMsg"]);
                    if (string.IsNullOrEmpty(dd))
                    {
                        ViewBag.pdfurl = Convert.ToString(details["PolicyPdfUrl"]);
                        ViewBag.payment = "Payment Successful";
                        ViewBag.IsPolicyNo = "True";
                        ViewBag.PolicyNo = Convert.ToString(details["PolicyNo"]);
                        ap.SP_POLICYDETAILSMASTER("U", enqid, 16, pospid.pospid,
                                  null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
                                  null, null, null, null, null, null, null, null, null, null, null, null, Convert.ToString(details["PolicyNo"]), null, null,
                                  null, null, 1, null, null, null, null, null, null, null, null, null, null, true);

                        if (!string.IsNullOrEmpty(Convert.ToString(details["PolicyNo"])))
                        {
                            data.FetchData(16, enqid,"");
                        }


                        string pa = SavePolicyPdf(Convert.ToString(details["PolicyNo"]));
                        using (WebClient ccc = new WebClient())
                        {
                            ccc.DownloadFile(ViewBag.pdfurl, pa + "\\" + Convert.ToString(details["PolicyNo"]) + ".pdf");
                        }
                    }

                    //ViewBag.pdfurl = Convert.ToString(json["PolicyPdfUrl"]);
                }
                else
                {
                    ViewBag.payment = "Payment Fail";
                }
                string date = DateTime.UtcNow.ToString("D");
                ViewBag.paymentDate = date;
                ViewBag.EnqNo = enqid;
                var pospdata = ap.POSPMASTERs.Where(x => x.pospid == pospid.pospid).FirstOrDefault();
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
                //LogU.WritePaymentLog(Common.MySession.UserDetail.username);
                //ViewBag.pdfurl = Convert.ToString(json["PolicyPdfUrl"]);
                //http://103.254.244.86:85/Motor/Payment/RahejaPaymentReturn
                //http://localhost:14264/Motor/Payment/RahejaPaymentReturn
                //http://103.254.244.86:85/Motor/Payment/RahejaPaymentReturn
                //http://103.254.244.86:85/Motor/Payment/FutureResponse
            }
            catch (Exception ex)
            {
                ViewBag.payment = "Payment Fail";
                string date = DateTime.UtcNow.ToString("D");
                ViewBag.paymentDate = date;
                ViewBag.EnqNo = "";
                ViewBag.pospname = MySession.UserDetail.username;
                LogU.WritePaymentLog("RAHEJA >> PaymentController >> RahejaPaymentReturn >> " + ex.ToString());
            }
            return View();
        }

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
                //path = PolicyPdfPath + "//" + policyno+".pdf";

            }


            return path;
        }

        //public ActionResult RahejaPaymentReturn()
        //{
        //    string json = "";
        //  //  string s = "mihpayid=403993715523304758&mode=CC&status=success&unmappedstatus=captured&key=uaWLkx&txnid=C160621027972&amount=6702.00&cardCategory=domestic&discount=0.00&net_amount_debit=6702&addedon=2021-06-16+19%3a01%3a14&productinfo=MOTOR&firstname=trtrtyuu+dfdgvhgfh&lastname=&address1=&address2=&city=&state=&country=&zipcode=&email=fdgdf%40fgfg.com&phone=9067568787&udf1=&udf2=&udf3=&udf4=&udf5=&udf6=&udf7=&udf8=&udf9=&udf10=&hash=3c8e2b5c077c8b0b9e5c27b6120c3b86e69d5005d89e9de899795a4b37d01791989fbd073d6f7c1aa77f911da3ba1f673688d1cbc6539d150cacffc1dbd550d8&field1=193439&field2=123052&field3=20210616&field4=0&field5=311961168800&field6=00&field7=AUTHPOSITIVE&field8=Approved+or+completed+successfully&field9=No+Error&payment_source=payu&PG_TYPE=AXISPG&bank_ref_num=193439&bankcode=CC&error=E000&error_Message=No+Error&name_on_card=dsfdf&cardnum=401200XXXXXX1112&cardhash=This+field+is+no+longer+supported+in+postback+params.";
        //    string s =Convert.ToString(Request.Form);
        //    string enqid = string.Empty;
        //    DAL.ANDAPPEntities andEnt = new DAL.ANDAPPEntities();
        //    LogU.WritePaymentLog("RAHEJA >> PaymentController >> RahejaPaymentReturn >> " + s);
        //    string[] Spilt = s.Split('&');
        //    string paymentStatus = Spilt[2];
        //    string[] TIDspi = paymentStatus.Split('=');
        //    string sfdf = Spilt[5].ToString();
        //    string[] TID = sfdf.Split('=');
        //    var data1 = andEnt.GET_Payment_Parameter(TID[1], 16).ToList();
        //    enqid = data1[0].enq_id;
        //    if (TIDspi[1].Contains("suc"))
        //    {
        //        PaymentRequest model = new PaymentRequest();
        //        model.CompanyDetail = new CompanyWiseRefference();
        //        model.CompanyName = Company.RAHEJA;
        //        model.CompanyDetail.applicationId = data1[0].parameter_value;// "TAPI150621221129";
        //        model.CompanyDetail.OrderNo = data1[2].parameter_value;// "TAPI150621221129";
        //        model.CompanyDetail.QuoteNo = data1[1].parameter_value;//"QUOT_MOT_000232939_15062021";
        //        string apiUrl = ConfigurationManager.AppSettings["ProposalAfterPayment"];
        //        string inputJson = (new JavaScriptSerializer()).Serialize(model);
        //        WebClient client = new WebClient();
        //        client.Headers["Content-type"] = "application/json";
        //        client.Encoding = Encoding.UTF8;
        //        json = client.UploadString(apiUrl, inputJson);
        //        var details = JObject.Parse(json);
        //        string dd = Convert.ToString(details["ErrorMsg"]);
        //        if (string.IsNullOrEmpty(dd))
        //        {
        //            ViewBag.pdfurl = Convert.ToString(details["PolicyPdfUrl"]);
        //            ViewBag.payment = "Payment Successful";
        //            ViewBag.IsPolicyNo = "True";
        //            ViewBag.PolicyNo = Convert.ToString(details["PolicyNo"]);
        //            ap.SP_POLICYDETAILSMASTER("U", enqid, null, "", null, null, null, null, null,
        //                           null, null, null, null, null, null, null, null, null, Convert.ToString(details["PolicyNo"]), null, null, null, null, null, null, true);
        //        }
        //        //ViewBag.pdfurl = Convert.ToString(json["PolicyPdfUrl"]);
        //    }
        //    else
        //    {
        //        ViewBag.payment = "Payment Fail";
        //    }
        //    string date = DateTime.UtcNow.ToString("D");
        //    ViewBag.paymentDate = date;
        //    ViewBag.EnqNo = enqid;
        //    //ViewBag.pdfurl = Convert.ToString(json["PolicyPdfUrl"]);
        //    //http://103.254.244.86:85/Motor/Payment/RahejaPaymentReturn
        //    //http://localhost:14264/Motor/Payment/RahejaPaymentReturn

        //    return View();
        //}

        #endregion
    }
}