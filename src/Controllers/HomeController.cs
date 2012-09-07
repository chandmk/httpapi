using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using Newtonsoft.Json.Linq;
using httpapi.Helpers;

namespace httpapi.Controllers
{
    public class HelpController : Controller
    {
        public ActionResult Index()
        {
            return View(GlobalConfiguration.Configuration.Services.GetApiExplorer());
        }
    }
    /// <summary>
    /// Http request & response service
    /// </summary>
    public class HomeController : ApiController
    {
        private readonly JsonMediaTypeFormatter jsonMediaTypeFormatter = new JsonMediaTypeFormatter();

        public HomeController()
        {
            jsonMediaTypeFormatter.Indent = true;
        }

        /// <summary>
        /// Returns user-agent
        /// </summary>
        /// <returns>Origin IP</returns>
        [System.Web.Http.HttpGet]
        public HttpResponseMessage UserAgent()
        {
            var result = new JObject(new JProperty("user-agent", string.Join(" ", Request.Headers.UserAgent)));
            return this.Request.CreateResponse(HttpStatusCode.OK, result);
        }

        /// <summary>
        /// Returns origin ip
        /// </summary>
        /// <returns>ip address</returns>
        [System.Web.Http.HttpGet]
        public HttpResponseMessage Ip()
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
                           {
                               Content =
                                   new ObjectContent<dynamic>(new { ip = UserIPAddress }, jsonMediaTypeFormatter)
                           };
        }

        /// <summary>
        /// Returns request header collection
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.HttpGet]
        public HttpResponseMessage Headers()
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
                           {
                               Content =
                                   new ObjectContent<dynamic>(new { headers = GetRequestHeaders() }, jsonMediaTypeFormatter)
                           };
        }

        /// <summary>a
        /// Returns cookie collection
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.HttpGet]
        public HttpResponseMessage Cookies()
        {
            var cookieCollection = GetHttpContextWrapper().Request.Cookies;
            var cookies = cookieCollection.AllKeys.ToDictionary(key => key, key => cookieCollection[key]);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content =
                    new ObjectContent<dynamic>(new { cookies }, jsonMediaTypeFormatter)
            };
        }

        /// <summary>a
        /// Allows to set cookies and returns the set cookie collection
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.HttpGet]
        public HttpResponseMessage SetCookies()
        {
            var queryString = Request.RequestUri.ParseQueryString();

            var response = new HttpResponseMessage(HttpStatusCode.Redirect);
            response.Headers.Location = new Uri(BaseUri, "cookies");
            foreach (var qs in queryString.AllKeys)
            {
                response.Headers.Add("Set-Cookie", string.Format("{0}={1}", qs, queryString[qs]));
            }

            return response;
        }

        /// <summary>
        /// Returns GET data. 
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.HttpGet]
        public HttpResponseMessage Get()
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content =
                    new ObjectContent<dynamic>(new { url = Request.RequestUri.ToString(), headers = GetRequestHeaders(), origin = UserIPAddress }, jsonMediaTypeFormatter)
            };
        }

        /// <summary>
        /// Delays response for n-10 secs 
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.HttpGet]
        public HttpResponseMessage Delay(int secs)
        {
            secs = secs > 10 ? 10 : secs;
            Thread.Sleep(TimeSpan.FromSeconds(secs));
            return Get();
        }

        /// <summary>
        /// Delays response for n-10 secs 
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.HttpGet]
        public HttpResponseMessage html()
        {
            StringBuilder sb = new StringBuilder();
            var apiExExplorer = GlobalConfiguration.Configuration.Services.GetApiExplorer();
            foreach (var api in apiExExplorer.ApiDescriptions)
            {
                sb.AppendFormat("<li>{0} - <strong> {1}</strong> - {2}", api.HttpMethod, api.RelativePath,
                                api.Documentation);
                if (api.ParameterDescriptions.Count > 0)
                {
                    sb.AppendFormat("<blockquote><ul>");
                    foreach (var parameter in api.ParameterDescriptions)
                    {
                        sb.AppendFormat("<li>{0}: {1} ({2})</li>", parameter.Name, parameter.Documentation, parameter.Source);
                    }
                    sb.AppendFormat("</ul></blockquote></li>");
                }

            }
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content =
                    new StringContent(string.Format(@"
                            <!DOCTYPE html>
                            <html>
                                <head><title>httpapi - Request Response Service</title></head>
                                <body>
                                    <h1>httpapi - Request Response Service</h1>
                                    <section>
                                    <h3>ENDPOINTS</h3>
                                    <ul>
                                   {0}
                                    </ul>
                                </section>
                                </body>
                            </html>", sb.ToString()))
            };
            response.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("text/html");
            return response;
        }

        /// <summary>
        /// Returns gzip-encoded content.
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.HttpGet]
        public HttpResponseMessage gzip()
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content =
                new CompressedContent(new ObjectContent<dynamic>(new { origin = UserIPAddress, headers = GetRequestHeaders(), gzipped = true, method = Request.Method }, jsonMediaTypeFormatter), CompressedContent.EncodingType.gzip)
            };
        }

        /// <summary>
        /// Returns dflate-encoded content.
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.HttpGet]
        public HttpResponseMessage deflate()
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content =
                new CompressedContent(new ObjectContent<dynamic>(new { origin = UserIPAddress, headers = GetRequestHeaders(), deflated = true, method = Request.Method }, jsonMediaTypeFormatter), CompressedContent.EncodingType.deflate)
            };
        }

        /// <summary>
        /// Responds with the requested http status code
        /// </summary>
        /// <param name="code">HttpStatusCode</param>
        /// <returns></returns>
        [System.Web.Http.HttpGet]
        public HttpResponseMessage Status(int code)
        {
            switch (code)
            {
                case 418:
                    return new HttpResponseMessage() { StatusCode = (HttpStatusCode)code, ReasonPhrase = "418 I'M A TEAPOT", Content = new StringContent(I_AM_A_TEAPOT) };
                default:
                    return Request.CreateResponse((HttpStatusCode)code);
            }
        }

        /// <summary>
        /// Returns given response headers.
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.HttpGet]
        public HttpResponseMessage ResponseHeaders()
        {
            var querystring = Request.RequestUri.ParseQueryString();
            var headers = querystring.AllKeys.Where(key => Enum.IsDefined(typeof(HttpResponseHeader), key.Replace("-", ""))).ToDictionary(key => key, key => string.Join(",", querystring[key]));

            var response = Request.CreateResponse(HttpStatusCode.OK, headers, jsonMediaTypeFormatter);

            foreach (var header in headers)
            {
                if (header.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                {
                    string[] contentTypeParts = header.Value.Split(';');
                    response.Content.Headers.ContentType.MediaType = contentTypeParts.First();
                }
                response.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            return response;
        }

        /// <summary>
        /// Redirects the request for n times before returning GET content.
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.HttpGet]
        public HttpResponseMessage Redirect(int times)
        {
            var redirectResponse = Request.CreateResponse(HttpStatusCode.Redirect);
            redirectResponse.Headers.Location = times > 0
                                                    ? new Uri(Request.RequestUri.AbsoluteUri.Replace("/" + times, "/" + (times - 1)))
                                                     : new Uri(BaseUri, "get");
            return redirectResponse;
        }

        //        /// <summary>
        //        /// Redirects to a relative url for n times before returning GET content.
        //        /// </summary>
        //        /// <returns></returns>
        //        [System.Web.Http.HttpGet]
        //        public HttpResponseMessage RelativeRedirect(int times)
        //        {
        //            var redirectResponse = Request.CreateResponse(HttpStatusCode.Redirect);
        //            var redirectUri = new Uri(Request.RequestUri.AbsoluteUri.Replace("/" + times, "/" + (times - 1)));
        //            var baseUri = new UriBuilder(Request.RequestUri.Scheme, Request.RequestUri.Host, Request.RequestUri.Port, "Redirect").Uri;
        //            var relativeUri = baseUri.MakeRelativeUri(redirectUri);
        //            redirectResponse.Headers.Location = times > 0 ? relativeUri : new Uri(baseUri + "/Get");
        //            return redirectResponse;
        //        }





        private string UserIPAddress
        {
            get
            {
                {
                    if (Request.Properties.ContainsKey("MS_HttpContext"))
                    {
                        return ((HttpContextBase)Request.Properties["MS_HttpContext"]).Request.UserHostAddress;
                    }
                    else if (Request.Properties.ContainsKey(RemoteEndpointMessageProperty.Name))
                    {
                        RemoteEndpointMessageProperty prop;
                        prop = (RemoteEndpointMessageProperty)this.Request.Properties[RemoteEndpointMessageProperty.Name];
                        return prop.Address;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }

        private Uri BaseUri
        {
            get
            {
                return new UriBuilder(Request.RequestUri.Scheme, Request.RequestUri.Host, Request.RequestUri.Port).Uri;
            }
        }
        private HttpContextBase GetHttpContextWrapper()
        {
            var httpContext = this.Request.Properties["MS_HttpContext"] as HttpContextBase;
            return httpContext;
        }

        private Dictionary<string, string> GetRequestHeaders()
        {
            var headers = Request.Headers.ToDictionary(header => header.Key, header => string.Join(" ", header.Value));
            return headers;
        }

        const string I_AM_A_TEAPOT = @"
                    

    -=[ teapot ]=-

       _...._
     .'  _ _ `.
    | .""` ^ `"". _,
    \_;`""---""`|//
      |       ;/
      \_     _/
        `""""""`
";
    }
}
