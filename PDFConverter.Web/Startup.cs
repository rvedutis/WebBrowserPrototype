using Microsoft.Owin;
using Owin;
using PDFConverter.Web;

[assembly: OwinStartup(typeof(Startup))]
namespace PDFConverter.Web
{
    public partial class Startup {
        public void Configuration(IAppBuilder app) {
        }
    }
}
