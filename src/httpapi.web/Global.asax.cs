using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using httpapi.web.Helpers.System.Web.Http;

namespace httpapi.web
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            RouteTable.Routes.LowercaseUrls = true;
            GlobalConfiguration.Configuration.Services.Replace(typeof(IDocumentationProvider),
               new XmlCommentDocumentationProvider(Server.MapPath("~/App_Data/httpapi.web.xml")));
        }
    }
}