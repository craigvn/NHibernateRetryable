using System;
using System.Threading;
using Microsoft.WindowsAzure.StorageClient;
using log4net;
using System.Reflection;

namespace YoureOnTime.Common
{
    public static class Retry
    {
        private static ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static T RequestWithRetry<T>(RetryPolicy retryPolicy, Func<T> action)
        {
            ShouldRetry shouldRetry = retryPolicy();

            int retryCount = 0;
            while (true)
            {
                TimeSpan delay = TimeSpan.FromSeconds(0);

                try
                {
                    return action();
                }
                catch (Exception ex)
                {
                    _log.Error("RequestWithRetry", ex);

                    if (!shouldRetry(retryCount, ex, out delay))
                        throw;
                }
                retryCount++;
                Thread.Sleep(delay);
            }
        }

        public static void RequestWithRetry(RetryPolicy retryPolicy, Action action)
        {
            ShouldRetry shouldRetry = retryPolicy();

            int retryCount = 0;
            while (true)
            {
                TimeSpan delay = TimeSpan.FromSeconds(0);

                try
                {
                    action();
                    return;
                }
                catch (Exception ex)
                {
                    _log.Error("RequestWithRetry", ex);

                    if (!shouldRetry(retryCount, ex, out delay))
                        throw;
                }

                retryCount++;
                Thread.Sleep(delay);
            }
        }
    }
}
