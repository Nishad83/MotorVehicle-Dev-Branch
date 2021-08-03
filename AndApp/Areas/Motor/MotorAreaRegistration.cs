using System.Web.Mvc;

namespace AndApp.Areas.Motor
{
    public class MotorAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Motor";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Motor_default",
                "Motor/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}