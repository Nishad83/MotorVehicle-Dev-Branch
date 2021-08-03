using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using static AndApp.Models.CommonModels;

namespace AndApp.Utilities
{
    public static class Common
    {
        #region "USER SESSION"
        public class MySession
        {
            public static bool IsLoggedIn
            {
                get
                {
                    object temp = HttpContext.Current.Session["IsLoggedIn"];
                    if (temp == null)
                        return false;
                    else
                        return Convert.ToBoolean(temp);
                }
                set
                {
                    if (value)
                    {
                        if (HttpContext.Current.Session["IsLoggedIn"] == null)
                            HttpContext.Current.Session.Add("IsLoggedIn", value);
                        else
                            HttpContext.Current.Session["IsLoggedIn"] = value;
                    }
                    else
                    {
                        HttpContext.Current.Session.Remove("IsLoggedIn");
                        HttpContext.Current.Session.Remove("UserDetail");
                        HttpContext.Current.Session.Remove("AuthToken");
                    }
                }
            }
            public static UserSessionDetails UserDetail
            {
                get
                {
                    UserSessionDetails objEnt = new UserSessionDetails();
                    object sessionobj = HttpContext.Current.Session["UserDetail"];
                    return sessionobj as UserSessionDetails;
                }
                set
                {
                    if (HttpContext.Current.Session["UserDetail"] != null)
                        HttpContext.Current.Session["UserDetail"] = value;
                    else
                        HttpContext.Current.Session.Add("UserDetail", value);
                }
            }
             
        }
        #endregion
    }
}
