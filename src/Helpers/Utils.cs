using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace httpapi.Helpers
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
    }
}