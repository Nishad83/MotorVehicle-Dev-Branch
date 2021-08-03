using AndApp.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;


namespace AndApp.AuthData
{
    public class SessionTimeoutAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)

        {
            if (Common.MySession.IsLoggedIn == false)
            {
                filterContext.Result = new RedirectResult("~/Home/POSPLOGIN");
            }
        }
    }
}