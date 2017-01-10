using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(WebbrowserPrototype.Startup))]
namespace WebbrowserPrototype
{
    public partial class Startup {
        public void Configuration(IAppBuilder app) {
        }
    }
}
