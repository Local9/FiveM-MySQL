using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GHMatti.Core
{
    public sealed class GHMattiTaskScheduler : TaskScheduler, IDisposable
    {
        private List<Thread> threads = new List<Thread>();
        private List<BlockingCollection<Task>> tasks = new List<BlockingCollection<Task>>();

        private static int GetNumberOfThreads()
        {
            if (Environment.ProcessorCount > 2)
                return Environment.ProcessorCount - 1;
            else
                return (Environment.ProcessorCount > 1) ? Environment.ProcessorCount : 1;
        }

        public GHMattiTaskScheduler()
        {
            for (int i = 0; i < GetNumberOfThreads(); i++)
                tasks.Add(new BlockingCollection<Task>());
            for (int i = 0; i < GetNumberOfThreads(); i++)
            {
                if (threads.Count <= i)
                {
                    ParameterizedThreadStart threadStart = new ParameterizedThreadStart(Execute);
                    Thread thread = new Thread(threadStart);
                    if (!thread.IsAlive)
                    {
                        thread.Start(i);
                    }
                    threads.Add(thread);
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Execute(object i)
        {
            int index = (int)i;
            foreach (Task task in tasks[index].GetConsumingEnumerable())
            {
                TryExecuteTask(task);
            }
        }

        protected override void QueueTask(Task task)
        {
            if (task != null)
            {
                int internalThreadId = 0;
                for (int i = 1; i < GetNumberOfThreads(); i++)
                {
                    if (tasks[i].Count < tasks[internalThreadId].Count)
                        internalThreadId = i;
                }
                tasks[internalThreadId].Add(task);
            }
        }

        private void Dispose(bool dispose)
        {
            if (dispose)
            {
                for (int i = 0; i < GetNumberOfThreads(); i++)
                {
                    tasks[i].CompleteAdding();
                    tasks[i].Dispose();
                }
            }
        }

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            IEnumerable<Task> taskList = tasks[0].ToArray();
            for (int i = 1; i < GetNumberOfThreads(); i++)
            {
                taskList = taskList.Concat(tasks[i].ToArray());
            }
            return taskList;
        }

        protected override bool TryExecuteTaskInline(Task task, bool wasQueued)
        {
            return false;
        }
    }
}
