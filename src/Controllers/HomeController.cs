using System;
using System.Collections.Generic;
using System.IO;
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
using System.Web.Http.Description;
using Newtonsoft.Json;
using httpapi.Helpers;
using httpapi.Models;

namespace httpapi.Controllers
{
    /// <summary>
    /// Class Request Response API
    /// </summary>
    public class HomeController : ApiController
    {
        /// <summary>
        /// Returns this page
        /// </summary>
        /// <returns>HttpResponseMessage.</returns>
        [HttpGet]
        public HttpResponseMessage Index()
        {
            return Html();
        }

        /// <summary>
        /// Returns user-agent
        /// </summary>
        /// <returns>HttpResponseMessage.</returns>
        [HttpGet]
        public HttpResponseMessage UserAgent()
        {
            return Request.CreateResponse(HttpStatusCode.OK, Request.Headers.UserAgent.ToString());
        }

        /// <summary>
        /// Returns origin ip
        /// </summary>
        /// <returns>HttpResponseMessage.</returns>
        [HttpGet]
        public HttpResponseMessage Ip()
        {
            return Request.CreateResponse(HttpStatusCode.OK, UserIpAddress);
        }

        /// <summary>
        /// Returns request header collection
        /// </summary>
        /// <returns>HttpResponseMessage.</returns>
        [HttpGet]
        public HttpResponseMessage Headers()
        {
            return Request.CreateResponse(HttpStatusCode.OK, GetRequestHeaders());
        }

        /// <summary>
        /// Returns cookie collection
        /// </summary>
        /// <returns>HttpResponseMessage.</returns>
        [HttpGet]
        public HttpResponseMessage Cookies()
        {
            var cookieCollection = GetHttpContextWrapper().Request.Cookies;
            var cookies = cookieCollection.AllKeys.ToDictionary(key => key, key => cookieCollection[key]);
            return Request.CreateResponse(HttpStatusCode.OK, cookies);
        }

        /// <summary>
        /// Allows to set cookies and returns the set cookie collection
        /// </summary>
        /// <returns>HttpResponseMessage.</returns>
        [HttpGet]
        public HttpResponseMessage SetCookies()
        {

            var queryString = Request.RequestUri.ParseQueryString();
            var cookieHeaderValues = queryString.AllKeys.Select(qs => new CookieHeaderValue(qs, queryString[qs])).ToList();
            var response = new HttpResponseMessage(HttpStatusCode.Redirect);
            response.Headers.AddCookies(cookieHeaderValues);
            response.Headers.Location = ExtensionAwareReturnPath("cookies");

            return response;
        }
        
        /// <summary>
        /// Returns GET data.  Allows HEAD method calls.
        /// </summary>
        /// <returns>HttpResponseMessage.</returns>
        [HttpHead, HttpGet]
        public HttpResponseMessage Get()
        {
            return Request.CreateResponse(HttpStatusCode.OK,
                                   new GetModel
                                       {
                                       url = Request.RequestUri.ToString(),
                                       headers = GetRequestHeaders(),
                                       origin = UserIpAddress
                                   });
        }

        /// <summary>
        /// Adds key value pairs to the existing key value pairs. Server maintains a key value pair of k0/v0 for testing.
        /// </summary>
        /// <returns>HttpResponseMessage.</returns>
        [HttpPost]
        public HttpResponseMessage Post()
        {
            var existingHeaders = new GetModel().form;
            var newHeaders = Request.Content.ReadAsFormDataAsync().Result;
            newHeaders.Add(existingHeaders);
            return Request.CreateResponse(HttpStatusCode.OK,
                                   new PostModel()
                                   {
                                       url = Request.RequestUri.ToString(),
                                       headers = GetRequestHeaders(),
                                       origin = UserIpAddress,
                                       form = newHeaders
                                   });
        }

        /// <summary>
        /// Replaces the key value pair data.  Server maintains a key value pair of k0/v0 for testing.
        /// </summary>
        /// <returns>HttpResponseMessage.</returns>
        [HttpPut]
        public HttpResponseMessage Put()
        {
            var existingHeaders = new GetModel().form;
            var newHeaders = Request.Content.ReadAsFormDataAsync().Result;
            existingHeaders.Set(existingHeaders.AllKeys[0], newHeaders[existingHeaders.AllKeys[0]]);
            return Request.CreateResponse(HttpStatusCode.OK,
                                   new PostModel()
                                   {
                                       url = Request.RequestUri.ToString(),
                                       headers = GetRequestHeaders(),
                                       origin = UserIpAddress,
                                       form = existingHeaders
                                   });
        }

        /// <summary>
        /// Deletes the data for the given key. Server maintains a key value pair of k0/v0 for testing.
        /// </summary>
        /// <returns>HttpResponseMessage.</returns>
        [HttpDelete]
        public HttpResponseMessage Delete(string key)
        {
            var existingHeaders = new GetModel().form;
            existingHeaders.Remove(key);
            return Request.CreateResponse(HttpStatusCode.OK,
                                   new PostModel()
                                   {
                                       url = Request.RequestUri.ToString(),
                                       headers = GetRequestHeaders(),
                                       origin = UserIpAddress,
                                       form = existingHeaders
                                   });
        }

        /// <summary>
        /// Returns PATCH data.
        /// </summary>
        /// <returns>HttpResponseMessage.</returns>
        [HttpPost]
        public HttpResponseMessage Patch()
        {
            return Request.CreateResponse(HttpStatusCode.OK,
                                   new
                                   {
                                       url = Request.RequestUri.ToString(),
                                       headers = GetRequestHeaders(),
                                       origin = UserIpAddress
                                   });
        }




        /// <summary>
        /// Delays response for n-10 secs
        /// </summary>
        /// <param name="secs">The secs.</param>
        /// <returns>HttpResponseMessage.</returns>
        [HttpGet]
        public HttpResponseMessage Delay(int secs)
        {
            secs = secs > 10 ? 10 : secs;
            Thread.Sleep(TimeSpan.FromSeconds(secs));
            return Get();
        }

        /// <summary>
        /// Streams n-100 lines
        /// </summary>
        /// <param name="lines">The lines.</param>
        /// <returns>HttpResponseMessage.</returns>
        [HttpGet]
        public HttpResponseMessage Stream(int lines = 1)
        {
           //  FileStream fs = new FileStream("httpapi.xml", FileMode.Open);
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            lines = lines > 100 ? 100
               : lines < 1 ? 1 : lines;
            for (var i = 0; i < lines; i++)
            {
                writer.WriteLine(JsonConvert.SerializeObject(GetRequestHeaders()));
                writer.WriteLine("<br/><br/>");
            }
            writer.Flush();
            stream.Position = 0;

            var response = new HttpResponseMessage {Content =  new StreamContent(stream)};
            return response;
        }

        /// <summary>
        ///  returns robots.txt rules
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage Robotstxt()
        {
            var response = new HttpResponseMessage
                               {
                           Content = new StringContent(@"User-agent: *
Disallow: /")
                       };

            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
            return response;
        }

        /// <summary>
        /// Returns html content
        /// </summary>
        /// <returns>HttpResponseMessage.</returns>
        [HttpGet]
        public HttpResponseMessage Html()
        {
            // we don't want to support xml and json extensions
            var response = new HttpResponseMessage { Content = new StringContent(HelpContent()) };

            response.Content.Headers.ContentType.MediaType = "text/html";
            response.Content.Headers.ContentType.CharSet = "utf-8";
            return response;
        }

        private string HelpContent()
        {
            var sb = new StringBuilder();
            var apiExExplorer = new ApiExplorer(ControllerContext.Configuration);
            foreach (var api in apiExExplorer.ApiDescriptions)
            {
                sb.AppendFormat("<li><a href='{0}'><strong>/{1}</strong></a> - {2} - {3}", ToLink(api.RelativePath), api.RelativePath.ToLower(), api.HttpMethod, api.Documentation);
                //                if (api.ParameterDescriptions.Count > 0)
                //                {
                //                    sb.AppendFormat("<ul>");
                //                    foreach (var parameter in api.ParameterDescriptions)
                //                    {
                //                        sb.AppendFormat("<li>{0}: {1} ({2})</li>", parameter.Name, parameter.Documentation, parameter.Source);
                //                    }
                //                    sb.AppendFormat("</ul></li>");
                //                }
            }

            var content = string.Format(@"
                            <!DOCTYPE html>
                            <html>
                                <head><title>httpapi - Request Response Service</title></head>
                                <body>
                                    <h1>httpapi - Request Response Service</h1>
                                    <section>
                                    <h3>ENDPOINTS</h3>
                                    <ul style='list-style:none; line-height:1.5em;'>
                                   {0}
                                    </ul>
                                </section>
                                </body>
                            </html>", sb);
            return content;
        }

        /// <summary>
        /// Returns gzip-encoded content.
        /// </summary>
        /// <returns>HttpResponseMessage.</returns>
        [HttpGet]
        public HttpResponseMessage Gzip()
        {
            // we want the webapi to choose the MediaTypeFormatter to support the xml and json extension in the paths
            var response =
                Request.CreateResponse(HttpStatusCode.OK, new CompressedContentModel
                                           {
                                               origin = UserIpAddress,
                                               headers = GetRequestHeaders(),
                                               gzipped = true,
                                               method = Request.Method
                                           });
            
            // There is no compressed content implementation out of the box
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content =
                new CompressedContent(response.Content, 
                        CompressedContent.EncodingType.gzip)
            };
        }

        /// <summary>
        /// Returns dflate-encoded content.
        /// </summary>
        /// <returns>HttpResponseMessage.</returns>
        [HttpGet]
        public HttpResponseMessage Deflate()
        {
            // we want the webapi to choose the MediaTypeFormatter to support the xml and json extension in the paths
            var response =
                Request.CreateResponse(HttpStatusCode.OK, new CompressedContentModel
                {
                    origin = UserIpAddress,
                    headers = GetRequestHeaders(),
                    deflated = true,
                    method = Request.Method
                });
            // There is no compressed content implementation out of the box
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content =
                new CompressedContent(response.Content,
                        CompressedContent.EncodingType.deflate)
            };
        }

        /// <summary>
        /// Responds with the requested http status code
        /// </summary>
        /// <param name="code">HttpStatusCode</param>
        /// <returns>HttpResponseMessage.</returns>
        [HttpGet]
        public HttpResponseMessage Status(int code)
        {
            switch (code)
            {
                case 418:
                    return new HttpResponseMessage { StatusCode = (HttpStatusCode)code, ReasonPhrase = "418 I'M A TEAPOT", Content = new StringContent(IAM_A_TEAPOT) };
                default:
                    return Request.CreateResponse((HttpStatusCode)code);
            }
        }

        /// <summary>
        /// Returns given response headers.
        /// </summary>
        /// <returns>HttpResponseMessage.</returns>
        [HttpGet]
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
        /// 302 Redirects n times.
        /// </summary>
        /// <param name="times">The times.</param>
        /// <returns>HttpResponseMessage.</returns>
        [HttpGet]
        public HttpResponseMessage Redirect(int times)
        {
            var redirectResponse = Request.CreateResponse(HttpStatusCode.Redirect);
            redirectResponse.Headers.Location = times > 0
                                                    ? ExtensionAwareReturnPath("redirect", "times=" + (times - 1))
                                                     : ExtensionAwareReturnPath("get");
            return redirectResponse;
        }

        private string UserIpAddress
        {
            get
            {
                if (Request.Properties.ContainsKey("MS_HttpContext"))
                {
                    return ((HttpContextBase)Request.Properties["MS_HttpContext"]).Request.UserHostAddress;
                }
                if (Request.Properties.ContainsKey(RemoteEndpointMessageProperty.Name))
                {
                    var prop = (RemoteEndpointMessageProperty)Request.Properties[RemoteEndpointMessageProperty.Name];
                    return prop.Address;
                }
                return null;
            }
        }

        private Uri BaseUri
        {
            get
            {
                return new UriBuilder(Request.RequestUri.Scheme, Request.RequestUri.Host, Request.RequestUri.Port).Uri;
            }
        }

        private Uri ExtensionAwareReturnPath(string extensionlessPath, string query = "")
        {
            var ext = Path.GetExtension(Request.RequestUri.AbsolutePath);
            var relativePath = extensionlessPath + ext;
           if(!string.IsNullOrEmpty(query))
            {
                relativePath += "?" + query;
            }
            var returnPath = new Uri(BaseUri, relativePath);
            return returnPath;
        }
      
        /// <summary>
        /// Gets the HTTP context wrapper.
        /// </summary>
        /// <returns>HttpContextBase.</returns>
        private HttpContextBase GetHttpContextWrapper()
        {
            var httpContext = Request.Properties["MS_HttpContext"] as HttpContextBase;
            return httpContext;
        }

        private Dictionary<string, string> GetRequestHeaders()
        {
            var headers = Request.Headers.ToDictionary(header => header.Key, header => string.Join(" ", header.Value));
            return headers;
        }

        private HtmlString ToLink(string relativePath)
        {

            var src = relativePath.ToLower()
                .Replace("{code}", "418")
                .Replace("{times}", "6")
                .Replace("{lines}", "10")
                .Replace("{secs}", "3")
                .Replace("setcookies", "setcookies?k1=v1&k2=v2")
                .Replace("responseheaders", "responseheaders?Content-Type=text/plain;%20charset=UTF-8&Server=httpapi")
                ;
            return new HtmlString(src);
        }

        const string IAM_A_TEAPOT = @"
                    

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
