using System;
using System.Collections.Generic;
using JinkeGroup.Util;


namespace JinkeGroup.Threading
{
    public sealed class Looper
    {
        private const string Tag = "Looper";
        private static readonly object Guard = new object();

        [ThreadStatic]
        private static Looper looper;
        
        public static bool DoesLooperExistForCurrentThread
        {
            get
            {
                return looper != null;
            }
        }

        public static Looper ThreadInstance
        {
            get
            {
                lock (Guard)
                {
                    if(looper == null)
                    {
                        looper = new Looper();
                    }
                    return looper;
                }
            }
        }

        private struct LooperTask
        {
            public Action Action;
            public DateTime When;
            public LooperTask(Action action, DateTime when)
            {
                this.Action = action;
                this.When = when;
            }
        }

        private readonly object Locker = new object();
        private readonly LinkedList<LooperTask> TaskQueue = new LinkedList<LooperTask>();
        private readonly List<LooperTask> ReadyTasks = new List<LooperTask>();

        public int LoopOnce()
        {
            if (TaskQueue.Count == 0)
                return 0;
            if (ReadyTasks.Count > 0)
                throw new InvalidOperationException("Concurrent call is not allowed");

            lock (Locker)
            {
                DateTime now = DateTime.UtcNow;
                LinkedListNode<LooperTask> node = TaskQueue.First;
                while (node != null)
                {
                    LooperTask task = node.Value;
                    if (task.When <= now)
                    {
                        ReadyTasks.Add(task);
                        LinkedListNode<LooperTask> nodeToRemove = node;
                        node = node.Next;
                        TaskQueue.Remove(nodeToRemove);
                    }
                    else
                    {
                        node = node.Next;
                    }
                }
            }
            int count = ReadyTasks.Count;
            if (count == 0)
                return 0;
            for(int i = 0; i < count; i++)
            {
                ReadyTasks[i].Action();
            }
            ReadyTasks.Clear();
            return count;
        }

        public void Schedule(Action action,DateTime when,bool atFrontQueue)
        {
            Assert.NotNull(action,"action");
            lock (Locker)
            {
                LooperTask task = new LooperTask(action, when);
                if (atFrontQueue)
                {
                    TaskQueue.AddFirst(task);
                }
                else
                {
                    TaskQueue.AddLast(task);
                }
            }
        }

        public int RemoveAllSchedules(Action action)
        {
            Assert.NotNull(action,"action");
            lock (Locker)
            {
                int removedCount = 0;
                LinkedListNode<LooperTask> node = TaskQueue.First;
                while (node != null)
                {
                    LooperTask task = node.Value;
                    if(task.Action == action)
                    {
                        LinkedListNode<LooperTask> nodeToRemove = node;
                        node = node.Next;
                        TaskQueue.Remove(nodeToRemove);
                        removedCount++;
                    }
                    else
                    {
                        node = node.Next;
                    }
                }
                return removedCount;
            }
        }

    }
}


