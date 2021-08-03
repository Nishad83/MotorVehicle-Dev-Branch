using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AndApp.Areas.Motor.Controllers
{
    public class TwoWheelerController : Controller
    {
        // GET: Motor/TwoWheeler
        public ActionResult Index()
        {
            return View();
        }

        // GET: Motor/TwoWheeler
        public ActionResult Result()
        {
            return View();
        }
    }
}