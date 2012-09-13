using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using httpapi.web.Models;

namespace httpapi.web.Controllers
{
    public class ClientController : Controller
    {
        [HttpGet]
        public ActionResult Post()
        {
            var content = GetExistingFormValues();
            return View(content.form);
        }

        private GetModel GetExistingFormValues()
        {
            var currentUrl = ControllerContext.HttpContext.Request.Url;
            Uri baseUri = new UriBuilder(currentUrl.Scheme, currentUrl.Host, currentUrl.Port).Uri;
            var client = new HttpClient();
            client.BaseAddress = baseUri;
            var response = client.GetAsync("get").Result;
            var content = response.Content.ReadAsAsync<GetModel>().Result;
            return content;
        }

        [HttpPost]
        public ActionResult Post(KeyValuePair<string, string> item)
        {
            return View();
        }

        public ActionResult Delete()
        {
            return View();
        }

        [HttpDelete]
        public ActionResult Delete(string key)
        {
            return View();
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
    }
}
