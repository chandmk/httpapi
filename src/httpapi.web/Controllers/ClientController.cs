using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Mvc;
using httpapi.web.Models;

namespace httpapi.web.Controllers
{
    public class ClientController : Controller
    {
        public ActionResult Test()
        {
            ViewBag.Message = TempData["Message"];
            ViewBag.Deleted = TempData["Deleted"] ?? false;
            ViewBag.Updated = TempData["Updated"] ?? false;
            ViewBag.ExistingFormValues = GetExistingFormValues().form;
            var apiContent = TempData["APIContent"] as SampleData;
            var content = new Dictionary<string, object>();
            if(apiContent != null)
            {
                content = apiContent.form;
            }
            return View(content);
        }

        [HttpPost]
        public ActionResult Delete(FormCollection data)
        {
            var formUrlEncodedContent = ExtractFormData(data);
            var request = new HttpRequestMessage(HttpMethod.Put, "delete?key=" + data["key"]) { Content = formUrlEncodedContent };
            var response = CreateClient().SendAsync(request).Result;
            var result = response.Content.ReadAsAsync<SampleData>().Result;
            TempData["APIContent"] = result;
            TempData["Deleted"] = true;
            return RedirectToAction("test");
        }

        [HttpPost]
        public ActionResult Put(FormCollection data )
        {
            var formUrlEncodedContent = ExtractFormData(data);
            var request = new HttpRequestMessage(HttpMethod.Put, "put") {Content = formUrlEncodedContent};
            var response = CreateClient().SendAsync(request).Result;
            if(response.IsSuccessStatusCode)
            {
                var result = response.Content.ReadAsAsync<SampleData>().Result;
                TempData["APIContent"] = result;
                TempData["Updated"] = true;
            }
            else
            {
                TempData["Message"] = response.ReasonPhrase;
            }
           
            return RedirectToAction("test");
        }

        private static FormUrlEncodedContent ExtractFormData(FormCollection data)
        {
            var content = new Dictionary<string, string>();
            foreach (var key in data.AllKeys)
            {
                if(key.StartsWith("k"))
                content[key] = data[key];
            }
            var formUrlEncodedContent = new FormUrlEncodedContent(content);
            return formUrlEncodedContent;
        }

        [HttpPost]
        public ActionResult Post(string key, string value)
        {
            var formUrlEncodedContent = new FormUrlEncodedContent( new Dictionary<string, string>() {{key, value}});
            var request = new HttpRequestMessage(HttpMethod.Post, "post") { Content = formUrlEncodedContent };
            var response = CreateClient().SendAsync(request).Result;
            var result = response.Content.ReadAsAsync<SampleData>().Result;
            TempData["APIContent"] = result;
            return RedirectToAction("test");
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
