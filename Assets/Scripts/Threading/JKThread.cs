using System;
using System.Threading;

namespace JinkeGroup.Threading
{
    public class JKThread
    {
        public static int CurrentThreadID
        {
            get
            {
                return Thread.CurrentThread.ManagedThreadId;
            }
        }

        public static void Sleep(int millisecondsTimeout)
        {
            Thread.Sleep(millisecondsTimeout);
        }

    }
}
