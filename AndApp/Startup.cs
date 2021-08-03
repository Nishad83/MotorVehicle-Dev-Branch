using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(AndApp.Startup))]
namespace AndApp
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
