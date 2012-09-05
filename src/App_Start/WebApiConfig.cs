using System.Web.Http;
using System.Web.Mvc;

namespace httpapi
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.Routes.MapHttpRoute(
                 name: "Status",
                routeTemplate: "status/{code}",
                defaults: new { controller = "Home", action="status"}
            ); 
            
            config.Routes.MapHttpRoute(
                 name: "DefaultApi",
                routeTemplate: "{action}",
                defaults: new { controller = "Home"}
            );
        }
    }
}
