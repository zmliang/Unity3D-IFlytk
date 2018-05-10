using System;
using UnityEngine;


namespace JinkeGroup.Threading
{
    public class MainExecutor : MonoBehaviour
    {
        private Looper looper;
        private int mainThreadID;

        protected virtual void Awake()
        {
            looper = Looper.ThreadInstance;
            mainThreadID = JKThread.CurrentThreadID;
        }

        protected virtual void Update()
        {
            looper.LoopOnce();
        }

        public bool IsMainThread
        {
            get
            {
                return JKThread.CurrentThreadID == mainThreadID;
            }
        }

        public void RunOnMainThread(Action action)
        {
            if (IsMainThread)
                action();
            else
                Post(action);
        }

        public void Post(Action action)
        {
            PostAtTime(action,DateTime.UtcNow);
        }

        public void PostDelayed(Action action,double delaySecs)
        {
            PostAtTime(action,DateTime.UtcNow.AddSeconds(delaySecs));
        }

        public void PostAtTime(Action action, DateTime time)
        {
            looper.Schedule(action,time,false);
        }

        public int RemoveAllSchedules(Action action)
        {
            return looper.RemoveAllSchedules(action);
        }

    }

}

