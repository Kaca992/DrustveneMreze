using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(DrustveneMrezev3.Startup))]
namespace DrustveneMrezev3
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
