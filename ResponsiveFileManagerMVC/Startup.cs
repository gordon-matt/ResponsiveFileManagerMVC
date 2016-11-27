using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ResponsiveFileManagerMVC.Startup))]
namespace ResponsiveFileManagerMVC
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
