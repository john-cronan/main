using System;
using System.Threading;

namespace JPC.Common
{
    public static class SyncronizationUtility
    {
        public static void With(SemaphoreSlim semaphore, Action action)
        {
            semaphore.Wait();
            try
            {
                action();
            }
            finally
            {
                semaphore.Release();
            }
        }
    }
}
