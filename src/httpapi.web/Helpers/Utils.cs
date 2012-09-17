using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Web.Mvc;

namespace httpapi.web.Helpers
{
    public static class Utils
    {
        public static bool IsFatal(this Exception exception)
        {
            while (exception != null)
            {
                if (exception as OutOfMemoryException != null &&
                    exception as InsufficientMemoryException == null || 
                    exception as ThreadAbortException != null ||
                    exception as AccessViolationException != null ||
                    exception as SEHException != null ||
                    exception as StackOverflowException != null)
                {
                    return true;
                }
                if (exception as TypeInitializationException == null && 
                    exception as TargetInvocationException == null)
                {
                    break;
                } 
                exception = exception.InnerException;
            } 
            return false;
        }

        public static string ToPublicUrl(this UrlHelper urlHelper, Uri relativeUri)
        {
            var httpContext = urlHelper.RequestContext.HttpContext;

            var uriBuilder = new UriBuilder
            {
                Host = httpContext.Request.Url.Host,
                Path = "/",
                Port = 80,
                Scheme = "http",
            };

            if (httpContext.Request.IsLocal)
            {
                uriBuilder.Port = httpContext.Request.Url.Port;
            }

            return new Uri(uriBuilder.Uri, relativeUri).AbsoluteUri;
        }
    }
}