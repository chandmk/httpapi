using System.Net.Http.Formatting;
using System.Web.Http;
using Newtonsoft.Json;

namespace httpapi
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.Formatters.JsonFormatter.MediaTypeMappings.Add(new UriPathExtensionMapping("json", "application/json"));
            config.Formatters.XmlFormatter.MediaTypeMappings.Add(new UriPathExtensionMapping("xml", "application/xml"));
            config.Formatters.JsonFormatter.SerializerSettings.Formatting = Formatting.Indented;
           

//            config.Routes.MapHttpRoute(
//                 name: "Status",
//                routeTemplate: "status/{code}",
//                defaults: new { controller = "Home", action="status"}
//            );   
//            
//            config.Routes.MapHttpRoute(
//                 name: "Redirect",
//                routeTemplate: "redirect/{times}",
//                defaults: new { controller = "Home", action="redirect"}
//            );     
//            
//            config.Routes.MapHttpRoute(
//                 name: "delay",
//                routeTemplate: "delay/{secs}",
//                defaults: new { controller = "Home", action="delay"}
//            );   
//            
            config.Routes.MapHttpRoute(
                name: "DefaultApiExt",
                routeTemplate: "{action}.{ext}",
                 defaults: new { controller = "Home", action="Index"}
                );
            config.Routes.MapHttpRoute(
                 name: "DefaultApi",
                routeTemplate: "{action}",
                defaults: new { controller = "Home", action="Index"}
            );
        }
    }
}
