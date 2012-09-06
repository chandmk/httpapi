﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.ServiceModel.Channels;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        /// <summary>
        /// Returns user's ip address
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

        [System.Web.Http.HttpGet]
        public HttpResponseMessage Headers()
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
                           {
                               Content =
                                   new ObjectContent<dynamic>(new { headers = GetRequestHeaders() }, jsonMediaTypeFormatter)
                           };
        }

        [System.Web.Http.HttpGet]
        public HttpResponseMessage Get()
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content =
                    new ObjectContent<dynamic>(new { url = Request.RequestUri.ToString(), headers = GetRequestHeaders(), origin = UserIPAddress }, jsonMediaTypeFormatter)
            };
        }

        [System.Web.Http.HttpGet]
        public HttpResponseMessage gzip()
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content =
                new CompressedContent(new ObjectContent<dynamic>(new { origin = UserIPAddress, headers = GetRequestHeaders(), gzipped = true, method = Request.Method }, jsonMediaTypeFormatter), "gzip")
            };
        }

        [System.Web.Http.HttpGet]
        public HttpResponseMessage deflate()
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content =
                new CompressedContent(new ObjectContent<dynamic>(new { origin = UserIPAddress, headers = GetRequestHeaders(), deflated = true, method = Request.Method }, jsonMediaTypeFormatter), "deflate")
            };
        }

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
        [System.Web.Http.HttpGet]
        public HttpResponseMessage ResponseHeaders()
        {
            var querystring = Request.RequestUri.ParseQueryString();
            var results = querystring.AllKeys.Where(key => Enum.IsDefined(typeof(HttpResponseHeader), key.Replace("-", ""))).ToDictionary(key => key, key => string.Join(",", querystring[key]));

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content =
                    new ObjectContent<dynamic>(results, jsonMediaTypeFormatter)
            };
        }

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
