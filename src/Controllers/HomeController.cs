using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
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
    /// Class Request Response API
    /// </summary>
    public class HomeController : ApiController
    {
        /// <summary>
        /// Returns user-agent
        /// </summary>
        /// <returns>HttpResponseMessage.</returns>
        /// <example>/useragent</example>
        [System.Web.Http.HttpGet]
        public HttpResponseMessage UserAgent()
        {
            // using newtonsoft's JObject directly
            var result = new JObject(new JProperty("user-agent", string.Join(" ", Request.Headers.UserAgent)));
            return this.Request.CreateResponse(HttpStatusCode.OK, result);
        }

        /// <summary>
        /// Returns origin ip
        /// </summary>
        /// <returns>HttpResponseMessage.</returns>
        [System.Web.Http.HttpGet]
        public HttpResponseMessage Ip()
        {
            return Request.CreateResponse(HttpStatusCode.OK, new {ip = UserIPAddress});
        }

        /// <summary>
        /// Returns request header collection
        /// </summary>
        /// <returns>HttpResponseMessage.</returns>
        [System.Web.Http.HttpGet]
        public HttpResponseMessage Headers()
        {
            return Request.CreateResponse(HttpStatusCode.OK, new {headers = GetRequestHeaders()});
        }

        /// <summary>
        /// Returns cookie collection
        /// </summary>
        /// <returns>HttpResponseMessage.</returns>
        [System.Web.Http.HttpGet]
        public HttpResponseMessage Cookies()
        {
            var cookieCollection = GetHttpContextWrapper().Request.Cookies;
            var cookies = cookieCollection.AllKeys.ToDictionary(key => key, key => cookieCollection[key]);
            return Request.CreateResponse(HttpStatusCode.OK, new {cookies});
        }

        /// <summary>
        /// Allows to set cookies and returns the set cookie collection
        /// </summary>
        /// <returns>HttpResponseMessage.</returns>
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
        /// <returns>HttpResponseMessage.</returns>
        [System.Web.Http.HttpGet]
        public HttpResponseMessage Get()
        {
            return Request.CreateResponse(HttpStatusCode.OK,
                                   new
                                       {
                                           url = Request.RequestUri.ToString(),
                                           headers = GetRequestHeaders(),
                                           origin = UserIPAddress
                                       });
        }

        /// <summary>
        /// Delays response for n-10 secs
        /// </summary>
        /// <param name="secs">The secs.</param>
        /// <returns>HttpResponseMessage.</returns>
        [System.Web.Http.HttpGet]
        public HttpResponseMessage Delay(int secs)
        {
            secs = secs > 10 ? 10 : secs;
            Thread.Sleep(TimeSpan.FromSeconds(secs));
            return Get();
        }

        /// <summary>
        /// Returns html content
        /// </summary>
        /// <returns>HttpResponseMessage.</returns>
        [System.Web.Http.HttpGet]
        public HttpResponseMessage html()
        {
            var sb = new StringBuilder();
            var apiExExplorer = new ApiExplorer(ControllerContext.Configuration);
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

            var content = string.Format(@"
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
                            </html>", sb);

            var response = new HttpResponseMessage {Content = new StringContent(content)};

            response.Content.Headers.ContentType.MediaType ="text/html";
            response.Content.Headers.ContentType.CharSet ="utf-8";
            return response;
        }

        /// <summary>
        /// Returns gzip-encoded content.
        /// </summary>
        /// <returns>HttpResponseMessage.</returns>
        [System.Web.Http.HttpGet]
        public HttpResponseMessage gzip()
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content =
                new CompressedContent(new ObjectContent<dynamic>(new { origin = UserIPAddress, headers = GetRequestHeaders(), gzipped = true, method = Request.Method }, JsonMediaFormatter), CompressedContent.EncodingType.gzip)
            };
        }

        /// <summary>
        /// Returns dflate-encoded content.
        /// </summary>
        /// <returns>HttpResponseMessage.</returns>
        [System.Web.Http.HttpGet]
        public HttpResponseMessage deflate()
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content =
                new CompressedContent(new ObjectContent<dynamic>(new { origin = UserIPAddress, headers = GetRequestHeaders(), deflated = true, method = Request.Method }, JsonMediaFormatter), CompressedContent.EncodingType.deflate)
            };
        }

        /// <summary>
        /// Responds with the requested http status code
        /// </summary>
        /// <param name="code">HttpStatusCode</param>
        /// <returns>HttpResponseMessage.</returns>
        [System.Web.Http.HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
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
        /// <returns>HttpResponseMessage.</returns>
        [System.Web.Http.HttpGet]
        public HttpResponseMessage ResponseHeaders()
        {
            var querystring = Request.RequestUri.ParseQueryString();
            var headers = querystring.AllKeys.Where(key => Enum.IsDefined(typeof(HttpResponseHeader), key.Replace("-", ""))).ToDictionary(key => key, key => string.Join(",", querystring[key]));

            var response = Request.CreateResponse(HttpStatusCode.OK, headers);

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
        /// Redirects the request for specified times.
        /// </summary>
        /// <param name="times">The times.</param>
        /// <returns>HttpResponseMessage.</returns>
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





        /// <summary>
        /// Gets the user IP address.
        /// </summary>
        /// <value>The user IP address.</value>
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

        /// <summary>
        /// Gets the base URI.
        /// </summary>
        /// <value>The base URI.</value>
        private Uri BaseUri
        {
            get
            {
                return new UriBuilder(Request.RequestUri.Scheme, Request.RequestUri.Host, Request.RequestUri.Port).Uri;
            }
        }

        private JsonMediaTypeFormatter JsonMediaFormatter
        {
            get
            {
                return ControllerContext.Configuration.Formatters.JsonFormatter;
            }
        }
        /// <summary>
        /// Gets the HTTP context wrapper.
        /// </summary>
        /// <returns>HttpContextBase.</returns>
        private HttpContextBase GetHttpContextWrapper()
        {
            var httpContext = this.Request.Properties["MS_HttpContext"] as HttpContextBase;
            return httpContext;
        }

        /// <summary>
        /// Gets the request headers.
        /// </summary>
        /// <returns>Dictionary{System.StringSystem.String}.</returns>
        private Dictionary<string, string> GetRequestHeaders()
        {
            var headers = Request.Headers.ToDictionary(header => header.Key, header => string.Join(" ", header.Value));
            return headers;
        }

        /// <summary>
        /// 
        /// </summary>
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
