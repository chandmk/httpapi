using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;
using Moq;
using NUnit.Framework;
using httpapi;

namespace tests
{
    [TestFixture]
    public class HttpapiTests
    {
        private const string USER_AGENT = "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:17.0) Gecko/17.0 Firefox/17.0";
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
            mockContext = SetUpMockHttpContext();
        }

        private Mock<HttpContextBase> SetUpMockHttpContext()
        {
            mockContext = new Mock<HttpContextBase>();
            var mockRequest = new Mock<HttpRequestBase>();
            var mockResponse = new Mock<HttpResponseBase>();
            mockRequest.SetupGet(r => r.UserHostAddress).Returns("127.0.0.1");
            mockContext.SetupGet(c => c.Request).Returns(mockRequest.Object);
            mockContext.SetupGet(c => c.Response).Returns(mockResponse.Object);
            return mockContext;
        }

        [Test]
        public void Ip()
        {
            var request = CreateRequest("ip", HttpMethod.Get);
            var result = client.SendAsync(request).Result;
            var content = result.Content.ReadAsAsync<dynamic>().Result.ToString();
            Assert.AreEqual(content, new { ip = "127.0.0.1" }.ToString());
        } 
        
        [Test]
        public void UserAgent()
        {
            var request = CreateRequest("useragent", HttpMethod.Get);
            var result = client.SendAsync(request).Result;
            var content = result.Content.ReadAsStringAsync().Result;
            Assert.AreEqual( "{\"user-agent\":\"" + USER_AGENT + "\"}", content);
        } 
        
        [Test]
        public void Headers()
        {
            var request = CreateRequest("headers", HttpMethod.Get);
            var result = client.SendAsync(request).Result;
            dynamic content = result.Content.ReadAsAsync<dynamic>().Result;
            Dictionary<string, string> headers = content.GetType().GetProperty("headers").GetValue(content);
            Assert.AreEqual(USER_AGENT, headers.First().Value);
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            server.Dispose();
            client.Dispose();
        }

        private HttpRequestMessage CreateRequest(string url, HttpMethod method, string mthv = null)
        {
            var request = new HttpRequestMessage {RequestUri = new Uri("http://testhost/" + url), Method = method};
//            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(mthv));
            request.Headers.UserAgent.ParseAdd(USER_AGENT);
            request.Properties["MS_HttpContext"] = mockContext.Object;
            return request;
        }
        private HttpRequestMessage CreateRequest<T>(string url, string mthv, HttpMethod method, T content, MediaTypeFormatter formatter) where T : class
        {
            HttpRequestMessage request = CreateRequest(url, method, mthv);
            request.Content = new ObjectContent<T>(content, formatter);

            return request;
        }
    }

  
}
