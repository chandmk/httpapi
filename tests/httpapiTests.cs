﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;
using Moq;
using NUnit.Framework;
using httpapi.web.Models;
using httpapi.web;

namespace tests
{
    [TestFixture]
    public class HttpapiTests
    {
        private const string USER_AGENT = "Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.1 (KHTML, like Gecko) Chrome/21.0.1180.89 Safari/537.1";
        private HttpClient client;
        private HttpServer server;
        private Mock<HttpContextBase> mockContext;
        
        [TestFixtureSetUp]
        public void Init()
        {
            var config = new HttpConfiguration();
            WebApiConfig.Register(config);
            server = new HttpServer(config);
            client = new HttpClient(server) {BaseAddress = new Uri("http://testhost/")};
            client.DefaultRequestHeaders.UserAgent.ParseAdd(USER_AGENT);
            mockContext = SetUpMockHttpContext();
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            server.Dispose();
            client.Dispose();
        }

        private Mock<HttpContextBase> SetUpMockHttpContext()
        {
            mockContext = new Mock<HttpContextBase>();
            var mockRequest = new Mock<HttpRequestBase>();
            var mockResponse = new Mock<HttpResponseBase>();
            mockRequest.SetupGet(r => r.UserHostAddress).Returns("127.0.0.1");
            mockRequest.SetupGet(r => r.Cookies).Returns(new HttpCookieCollection());
            mockContext.SetupGet(c => c.Request).Returns(mockRequest.Object);
            mockContext.SetupGet(c => c.Response).Returns(mockResponse.Object);
            return mockContext;
        }

        [Test]
        public void Home()
        {
            var request = CreateRequest("",HttpMethod.Get);
            var response = client.SendAsync(request).Result;
            Assert.AreEqual("text/html", response.Content.Headers.ContentType.MediaType);
        }

        [Test]
        public void Ip()
        {
            var request = CreateRequest("ip", HttpMethod.Get);
            var response = client.SendAsync(request).Result;

            Assert.AreEqual("\"127.0.0.1\"", response.Content.ReadAsStringAsync().Result);
        } 
        
        [Test]
        public void UserAgent()
        {
            var request = CreateRequest("useragent", HttpMethod.Get);
            var response = client.SendAsync(request).Result;
            var content = response.Content.ReadAsStringAsync().Result;
            Assert.AreEqual(USER_AGENT, content.TrimStart('"').TrimEnd('"'));
        } 
        
        [Test]
        public void Head()
        {
            var request = CreateRequest("get", HttpMethod.Head);
            var response = client.SendAsync(request).Result;
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            //TODO: Why httpclient is able to fetch the content, in theory HEAD method response shouldn't have body
            //Assert.IsNull(response.Content);
        } 
        
        [Test]
        public void Get()
        {
            var request = CreateRequest("get", HttpMethod.Get);
            var response = client.SendAsync(request).Result;
            GetModel content = response.Content.ReadAsAsync<GetModel>().Result;
            Assert.AreEqual(request.RequestUri.ToString(), content.url);
        } 
        
        [Test]
        public void Headers()
        {
            var request = CreateRequest("headers", HttpMethod.Get);
            var response = client.SendAsync(request).Result;
            var headers = response.Content.ReadAsAsync<Dictionary<String, string>>().Result;
            Assert.AreEqual(USER_AGENT, headers.First().Value);
        }  
        
        [Test]
        public void ResponseHeaders()
        {
            var request = CreateRequest("responseheaders?Content-Type=text/plain;%20charset=UTF-8&Server=httpapi", HttpMethod.Get);
            var response = client.SendAsync(request).Result;

            Assert.AreEqual("httpapi", response.Headers.GetValues("Server").First());
            Assert.AreEqual("text/plain; charset=utf-8", response.Content.Headers.GetValues("Content-Type").First());

        }

        [Test]
        public void GZip()
        {
            var request = CreateRequest("gZip", HttpMethod.Get);
            var response = client.SendAsync(request).Result;

            Assert.AreEqual("gzip", response.Content.Headers.GetValues("Content-Encoding").First());
        }
        
        [Test]
        public void Deflate()
        {
            var request = CreateRequest("deflate", HttpMethod.Get);
            var response = client.SendAsync(request).Result;

            Assert.AreEqual("deflate", response.Content.Headers.GetValues("Content-Encoding").First());
        }

        [Test]
        public void Status418()
        {
            var request = CreateRequest("status?code=418", HttpMethod.Get);
            var response = client.SendAsync(request).Result;

            Assert.AreEqual("418", response.StatusCode.ToString());
            Assert.AreEqual("418 I'M A TEAPOT", response.ReasonPhrase);
        }  
        
        [Test]
        public void Status500()
        {
            var request = CreateRequest("status?code=500", HttpMethod.Get);
            var response = client.SendAsync(request).Result;

            Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
        } 
        
        [Test]
        public void Redirect()
        {
            var request = CreateRequest("redirect?times=6", HttpMethod.Get);
            var response = client.SendAsync(request).Result;
           
            Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);
            Assert.IsTrue(response.Headers.Location.ToString().EndsWith("redirect?times=5"));
        }

        [Test]
        public void Cookies()
        {
            var request = CreateRequest("cookies", HttpMethod.Get);
            var response = client.SendAsync(request).Result;
            var cookies = response.Content.ReadAsAsync<Dictionary<string, HttpCookie>>().Result;
            Assert.IsTrue(cookies.Count == 0);
        }    
        
        [Test]
        public void SetCookies()
        {
            var request = CreateRequest("setcookies?k1=v1&k2=v2", HttpMethod.Get);
            var response = client.SendAsync(request).Result;

            Assert.AreEqual("k1=v1", response.Headers.GetValues("Set-Cookie").First());
            Assert.AreEqual("k2=v2", response.Headers.GetValues("Set-Cookie").Last());
            Assert.IsTrue(response.Headers.Location.ToString().EndsWith("cookies"));
        }    
        
        [Test]
        public void Html()
        {

            var request = CreateRequest("html", HttpMethod.Get);
            var response = client.SendAsync(request).Result;

            Assert.AreEqual("text/html", response.Content.Headers.ContentType.MediaType);
        }  
        
        [Test]
        public void Delay()
        {
            var start = DateTime.Now.Ticks;
            var request = CreateRequest("delay?secs=1", HttpMethod.Get);
            var response = client.SendAsync(request).Result;
            var end = DateTime.Now.Ticks;

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(1, TimeSpan.FromTicks(end - start).Seconds);
        }  

        [Test]
        public void Stream()
        {
            var request = CreateRequest("stream?lines=1", HttpMethod.Get);
            var response = client.SendAsync(request).Result;
           Assert.IsTrue(response.Content is StreamContent);
        }

        [Test]
        public void Post()
        {
            var request = CreateRequest("post", HttpMethod.Post);
            var kv = new List<KeyValuePair<string, string>> {new KeyValuePair<string, string>("k1", "v1")};
            request.Content = new FormUrlEncodedContent(kv);
            var response = client.SendAsync(request).Result;
            var result = response.Content.ReadAsAsync<PostModel>().Result;
            Assert.AreEqual(2, result.form.Count);
            Assert.AreEqual("v1", result.form["k1"]);
        }

        [Test]
        public void Put()
        {
            var request = CreateRequest("put", HttpMethod.Put);
            var kv = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("k0", "v1") };
            request.Content = new FormUrlEncodedContent(kv);
            var response = client.SendAsync(request).Result;
            var result = response.Content.ReadAsAsync<PostModel>().Result;
            Assert.AreEqual(1, result.form.Count);
            Assert.AreEqual("v1", result.form["k0"]);
        } 
        
        [Test]
        public void Delete()
        {
            var request = CreateRequest("delete?key=k0", HttpMethod.Delete);
            var response = client.SendAsync(request).Result;
            var result = response.Content.ReadAsAsync<PostModel>().Result;
            Assert.AreEqual(0, result.form.Count);
        }
        
        private HttpRequestMessage CreateRequest(string url, HttpMethod method, string mthv = null)
        {
            var request = new HttpRequestMessage {RequestUri = new Uri("http://testhost/" + url), Method = method};
//            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(mthv));
            request.Properties["MS_HttpContext"] = mockContext.Object;
            return request;
        }
        private HttpRequestMessage CreateRequest<T>(string url, string mthv, HttpMethod method, T content, MediaTypeFormatter formatter) where T : class
        {
            HttpRequestMessage request = CreateRequest(url, method, mthv);
            request.Content = new ObjectContent<T>(content, formatter);

            return request;
        }

        string Serialize<T>(MediaTypeFormatter formatter, T value)
        {
            // Create a dummy HTTP Content.
            Stream stream = new MemoryStream();
            var content = new StreamContent(stream);
            /// Serialize the object.
            formatter.WriteToStreamAsync(typeof(T), value, stream, content, null).Wait();
            // Read the serialized string.
            stream.Position = 0;
            return content.ReadAsStringAsync().Result;
        }

        T Deserialize<T>(MediaTypeFormatter formatter, string str) where T : class
        {
            // Write the serialized string to a memory stream.
            Stream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(str);
            writer.Flush();
            stream.Position = 0;
            // Deserialize to an object of type T
            return formatter.ReadFromStreamAsync(typeof(T), stream, null, null).Result as T;
        }

        // Example of use
//        void TestSerialization()
//        {
//            var value = new Person() { Name = "Alice", Age = 23 };
//
//            var xml = new XmlMediaTypeFormatter();
//            string str = Serialize(xml, value);
//
//            var json = new JsonMediaTypeFormatter();
//            str = Serialize(json, value);
//
//            // Round trip
//            Person person2 = Deserialize<Person>(json, str);
//        }
    }

  
}
