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
                 name: "Redirect",
                routeTemplate: "redirect/{times}",
                defaults: new { controller = "Home", action="redirect"}
            );   
            
            config.Routes.MapHttpRoute(
                 name: "RelativeRedirect",
                routeTemplate: "relativeredirect/{times}",
                defaults: new { controller = "Home", action="relativeredirect"}
            ); 
            
            config.Routes.MapHttpRoute(
                 name: "DefaultApi",
                routeTemplate: "{action}",
                defaults: new { controller = "Home"}
            );
        }
    }
}
