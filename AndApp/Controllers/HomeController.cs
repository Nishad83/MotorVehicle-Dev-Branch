using AndApp.AuthData;
using AndApp.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Web;
using System.Web.Mvc;
using static AndApp.Models.CommonModels;

namespace AndApp.Controllers
{
    public class HomeController : Controller
    {
        DAL.ANDAPPEntities andapp = new DAL.ANDAPPEntities();



        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Getguid(long id)
        {

            Guid myuuid = Guid.NewGuid();
            string myuuidAsString = myuuid.ToString();
            long pospid = Common.MySession.UserDetail.userid;
            andapp.UpdatePOSPSecretKey(Convert.ToInt32(pospid), myuuidAsString);

            return Json(myuuidAsString, JsonRequestBehavior.AllowGet);
        }
        public ActionResult About()
        {
            string url = "https://pipuat.tataaiginsurance.in/tagichubws/motor_policy.jsp?polno=064001/0177533393/000000/00&src=app&key=C0MJOrWjkbNYtMKbOKiV2LiDs";
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            string filename = "";
            string destinationpath = "D:\\source";
            //if (!Directory.Exists(destinationpath))
            //{
            //    Directory.CreateDirectory(destinationpath);
            //}
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponseAsync().Result)
            {
                string path = response.Headers["Content-Disposition"];
                if (string.IsNullOrWhiteSpace(path))
                {
                    var uri = new Uri(url);
                    filename = Path.GetFileName(uri.LocalPath);
                }
                else
                {
                    ContentDisposition contentDisposition = new ContentDisposition(path);
                    filename = contentDisposition.FileName;

                }

                var responseStream = response.GetResponseStream();
                using (var fileStream = System.IO.File.Create(Path.Combine(destinationpath, filename)))
                {
                    responseStream.CopyTo(fileStream);
                }
            }

            //return Path.Combine(destinationpath, filename);
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }

        string NUM;

        public ActionResult POSPLOGIN()
        {
            string myguid = Request.QueryString.ToString();
            if (myguid != null && myguid != "")
            {
                var pospdata = andapp.POSPMASTERs.Where(x => x.status == true && x.secretkey == myguid).FirstOrDefault();
                if (pospdata != null)
                {


                    UserSessionDetails pdata = new UserSessionDetails();
                    pdata.mobileno = pospdata.mobileno;
                    pdata.pospcode = pospdata.pospcode;
                    pdata.username = (!string.IsNullOrEmpty(pospdata.firstname) ? pospdata.firstname : string.Empty) +" "+ (!string.IsNullOrEmpty(pospdata.middelname) ? pospdata.middelname.Substring(0,1) : string.Empty) +" "+ (!string.IsNullOrEmpty(pospdata.lastname) ? pospdata.lastname : string.Empty);
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
                    Common.MySession.IsLoggedIn = true;

                    andapp.UpdatePOSPSecretKey(Convert.ToInt32(pospdata.pospid), null);

                    return RedirectToAction("CarDetails", "PrivateCar");
                }
            }
            if (TempData["errmsg"] != null)
            {
                ViewBag.errmsg = TempData["errmsg"].ToString();
            }
            return View();
        }
        [HttpPost]
        public ActionResult GenerateOTP(DAL.QA_LOGIN_MASTER qamaster)
        {
            DAL.POSPMASTER posp = new DAL.POSPMASTER();
            DAL.QA_LOGIN_MASTER qa = new DAL.QA_LOGIN_MASTER();
            int OTP;

            string Randomnumber;
            Random rnd = new Random();
            Randomnumber = (rnd.Next(10000, 99999)).ToString();
            NUM = qamaster.mobileno;
            OTP = Convert.ToInt32(Randomnumber);
            this.Session["MobileNo"] = qamaster.mobileno;
            TempData["MobileNo"] = qamaster.mobileno;
            TempData["OTP"] = OTP;
            try
            {
                var Exist = (from db in andapp.POSPMASTERs where db.mobileno == qamaster.mobileno && db.status == true select db).FirstOrDefault();
                if (Exist != null)
                {
                    //chek GI Compleate
                    bool GIExamDone = andapp.VW_ExamPassDate_ForAndReport.Any(x => x.userid == Exist.pospid);
                    bool LiExamDone = andapp.VW_ExamPassDate_ForAndReport_LI.Any(x => x.userid == Exist.pospid);
                    if (GIExamDone)
                    {

                        Exist.otp = OTP.ToString();
                        Exist.otpupdatedon = DateTime.Now;
                        andapp.SaveChanges();
                        string smstext = "Dear User,Your OTP for registration is " + OTP + ". Use this otp to validate your login.";
                        SMS(NUM, smstext);
                    }
                    //else if (LiExamDone)
                    //{
                    //    Exist.otp = OTP.ToString();
                    //    Exist.otpupdatedon = DateTime.Now;
                    //    andapp.SaveChanges();
                    //    string smstext = "Dear User,Your OTP for registration is " + OTP + ". Use this otp to validate your login.";
                    //    SMS(NUM, smstext);
                    //}
                    else
                    {
                        TempData["errmsg"] = "Please Complete Training Module And Give The Exam First";
                        return RedirectToAction("POSPLOGIN");
                    }


                }
                else
                {
                    TempData["errmsg"] = "Invalid User.";
                    return RedirectToAction("POSPLOGIN");
                }
                //else
                //{
                //    posp.mobileno = qamaster.mobileno;
                //    posp.otp = OTP.ToString();
                //    qa.status = true;
                //    andapp.POSPMASTERs.Add(posp);
                //    andapp.SaveChanges();

                //    string smstext = "Dear User,Your OTP for registration is " + OTP + ". Use this otp to validate your login.";
                //    SMS(NUM, smstext);

                //}
            }
            catch (Exception Ex)
            {
                Console.Write(Ex.Message);
            }
            return RedirectToAction("OTP");
        }

        public ActionResult OTP()
        {
            return View();
        }

        [HttpPost]
        public JsonResult SendOTP(string OTP)
        {
            var count = 0;
            UserSessionDetails pdata = new UserSessionDetails();

            if (OTP != null)
            {
                string MobileNo = this.Session["MobileNo"].ToString();
                TempData["MobileNo"] = MobileNo;
                var pospdata = (from db in andapp.POSPMASTERs where db.mobileno == MobileNo && db.otp == OTP && db.status == true select db).FirstOrDefault();
                if (pospdata != null)
                {
                    count = 1;

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
                    Common.MySession.IsLoggedIn = true;

                    ViewBag.errormsg = "100";
                    //return RedirectToAction("CarDetails", "Motor/PrivateCar");
                }
                else
                {
                    ViewBag.errormsg = "200";
                }
            }
            return Json(count, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ResendOTP(DAL.QA_LOGIN_MASTER qamaster)
        {
            DAL.QA_LOGIN_MASTER posp = new DAL.QA_LOGIN_MASTER();
            int OTP;
            string Randomnumber;
            Random rnd = new Random();
            Randomnumber = (rnd.Next(10000, 99999)).ToString();
            OTP = Convert.ToInt32(Randomnumber);

            string Mobileno = this.Session["MobileNo"].ToString();
            TempData["MobileNo"] = Mobileno;
            TempData["OTP"] = OTP;
            try
            {
                var Exist = (from db in andapp.POSPMASTERs where db.mobileno == Mobileno && db.status == true select db).FirstOrDefault();
                if (Exist != null)
                {
                    Exist.otp = OTP.ToString();
                    andapp.SaveChanges();

                    string smstext = "Dear User,Your OTP for registration is " + OTP + ". Use this otp to validate your login.";
                    SMS(Mobileno, smstext);
                }
            }
            catch (Exception Ex)
            {
                Console.Write(Ex.Message);
            }
            return Json("", JsonRequestBehavior.AllowGet);
        }

        public void SMS(string mobileno, string SMSmsg)
        {
            try
            {
                string strurl = "http://mobi1.blogdns.com/websmss/smssenders.aspx?UserID=iandins&UserPass=Iand909@&Message=" + SMSmsg + "&MobileNo=" + mobileno + "&GSMID=ANDAPP";
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(strurl);
                request.Method = "POST";
                Byte[] byteArray = Encoding.UTF8.GetBytes(SMSmsg);
                request.ContentType = "application/x-www-form-urlencoded";
                Stream dataStream = request.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                dataStream = response.GetResponseStream();
                StreamReader readersms = new StreamReader(dataStream);
                String responseFromServer = readersms.ReadToEnd();
                string IsErrorMsg = string.Empty;
                if (responseFromServer.Contains("100"))
                {
                    IsErrorMsg = "1";
                }
                else
                {
                    IsErrorMsg = "0";
                }
                dataStream.Close();
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
        }

        #region LOG OUT
        [SessionTimeout]
        public ActionResult LogOut()
        { 
            Common.MySession.IsLoggedIn = false;
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("POSPLOGIN", "Home");
        }
        #endregion
    }
}