using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using httpapi.web.Models;

namespace httpapi.web.Controllers
{
    public class ClientController : Controller
    {
        public ActionResult Index()
        {
            var content = GetExistingFormValues();
            return View(content.form);
        }

        [HttpPost]
        public ActionResult Post(KeyValuePair<string, string> item)
        {
            return View();
        }

        public ActionResult Delete(string key)
        {
            var client = CreateClient();
            var response = client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, "delete?key=" + key)).Result;
            var result = response.Content.ReadAsAsync<SampleData>().Result;
            var form = result.form;
            return View("Index", form);
        }

        public ActionResult Put()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Put(KeyValuePair<string, string> item)
        {
            return View();
        }

        private SampleData GetExistingFormValues()
        {
            var client = CreateClient();
            var response = client.GetAsync("get").Result;
            var content = response.Content.ReadAsAsync<SampleData>().Result;
            return content;
        }

        private HttpClient CreateClient()
        {
            var currentUrl = ControllerContext.HttpContext.Request.Url;
            Uri baseUri = new UriBuilder(currentUrl.Scheme, currentUrl.Host, currentUrl.Port).Uri;
            var client = new HttpClient {BaseAddress = baseUri};
            return client;
        }
    }
}
