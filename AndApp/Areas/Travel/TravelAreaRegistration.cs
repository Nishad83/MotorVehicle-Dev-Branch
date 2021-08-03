using System.Web.Mvc;

namespace AndApp.Areas.Travel
{
    public class TravelAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Travel";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Travel_default",
                "Travel/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}