using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Mvc;
using System.Web.Routing;
using Newtonsoft.Json;
using httpapi.Helpers.System.Web.Http;


namespace httpapi
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            RouteTable.Routes.LowercaseUrls = true;
            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.Formatting = Formatting.Indented;
            GlobalConfiguration.Configuration.Services.Replace(typeof(IDocumentationProvider), 
                new XmlCommentDocumentationProvider(Server.MapPath("~/App_Data/httpapi.xml")));
        }
    }
}