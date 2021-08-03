using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web;
using System.Net;
using System.Net;
using System.IO;

namespace AndApp.Controllers
{
    public class testController : Controller
    {
        // GET: test
        public ActionResult Index()
        {
            return View();
        }
        public static string AccessAPI(string url, string postcontent)
        {
            byte[] bodyBytes = System.Text.Encoding.UTF8.GetBytes(postcontent);
            HttpWebRequest request = default(HttpWebRequest);
            request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Method = "Post"; //methodtype;//-post or get
            request.ContentType = "application/x-www-form-urlencoded";
            using (Stream ostream = request.GetRequestStream())
            {
                ostream.Write(bodyBytes, 0, bodyBytes.Length);
                ostream.Flush();
                ostream.Close();
            }
            HttpWebResponse response = default(HttpWebResponse);
            response = (HttpWebResponse)request.GetResponse();
            StreamReader strrespo = default(StreamReader);
            strrespo = new StreamReader(response.GetResponseStream());
            string s = null;
            s = strrespo.ReadToEnd();
            return s;
        }

    }
}