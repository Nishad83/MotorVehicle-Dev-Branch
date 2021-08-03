using AndWebApi.Models;
using AndApp.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace AndWebApi.Controllers
{
    public class HealthController : Controller
    {
        #region CompanyWiseQuotation
        // Summary for Getting DIGIT QUOTE
        [HttpPost]
        public Response DIGIT(Quotation QuoteModel)
        {
            Response res = new Response();
            if (ModelState.IsValid)
            {
                AndWebApi.DIGIT.Health hldigitservice = new AndWebApi.DIGIT.Health();
                Thread thread = new Thread(() => { res = hldigitservice.GetQuoteRequest(QuoteModel); });
                thread.Start();
                thread.Join();
            }
            else
            {
                var message = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                res.ErrorMsg = message;
                res.FinalPremium = 0;
                LogU.WriteLog("PrivateCarController >> DIGIT >>" + message);
            }

          //  res.CompanyName = Company.DIGIT;
            return res;
        }

        #endregion

    }
}