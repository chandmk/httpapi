using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace httpapi.Controllers
{
    public class HelpController : Controller
    {
        public ActionResult Index()
        {
            return View(GlobalConfiguration.Configuration.Services.GetApiExplorer());
        }
    }

    public class HomeController : ApiController
    {
        [System.Web.Http.HttpGet]
        public HttpResponseMessage UserAgent()
        {
            var request = this.Request;
            return new HttpResponseMessage(HttpStatusCode.OK)
                           {
                               Content =
                                   new ObjectContent<dynamic>(new { UserAgent = this.Request.Headers.UserAgent.ToString() }, new JsonMediaTypeFormatter())
                           };
        }  
        
        [System.Web.Http.HttpGet]
        public HttpResponseMessage Ip()
        {
            var httpContext = this.Request.Properties["MS_HttpContext"] as HttpContextWrapper;

            return new HttpResponseMessage(HttpStatusCode.OK)
                           {
                               Content =
                                   new ObjectContent<dynamic>(new { ip = httpContext.Request.UserHostAddress }, new JsonMediaTypeFormatter())
                           };
        }  
        
       
    }


}
