using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GHMatti.Core
{
    // Task Scheduler Replacement Class, so we do not have to Block the Server Thread
    public sealed class GHMattiTaskScheduler : TaskScheduler, IDisposable
    {
        // List to store all threads
        private List<Thread> threads = new List<Thread>();
        // List to Store all Task Lists / Stacks
        private List<BlockingCollection<Task>> tasks = new List<BlockingCollection<Task>>();
        // Number of Threads we will be using
        private int numberOfThreads = 1;

        // Constructor
        public GHMattiTaskScheduler()
        {
            numberOfThreads = GetNumberOfThreads();
            for (int i = 0; i < numberOfThreads; i++)
            {
                tasks.Add(new BlockingCollection<Task>());
                ParameterizedThreadStart threadStart = new ParameterizedThreadStart(Execute);
                Thread thread = new Thread(threadStart);
                if (!thread.IsAlive)
                {
                    thread.Start(i);
                }
                threads.Add(thread);
            }
        }

        // Will be called because of IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Return the amount of Threads we will use. Make this configurable in the future
        // If we got 2 Logical CPUs, we want at least 2 Threads, but we leave one open
        // therafter for the server thread
        private static int GetNumberOfThreads(int threadLimit = 0)
        {
            if (threadLimit <= Environment.ProcessorCount && threadLimit > 0)
                return threadLimit;
            if (Environment.ProcessorCount > 2)
                return Environment.ProcessorCount - 1;
            else
                return (Environment.ProcessorCount > 1) ? Environment.ProcessorCount : 1;
        }

        public void ApplyThreadLimit(int threadLimit = 0)
        {
            if (threadLimit < Environment.ProcessorCount && threadLimit > 0)
                numberOfThreads = threadLimit;
            if (Environment.ProcessorCount > 2)
                numberOfThreads = Environment.ProcessorCount - 1;
            else
                numberOfThreads = (Environment.ProcessorCount > 1) ? Environment.ProcessorCount : 1;
        }

        // Keep looping the Execution of Tasks forever
        private void Execute(object internalThreadId)
        {
            foreach (Task task in tasks[(int) internalThreadId].GetConsumingEnumerable())
            {
                TryExecuteTask(task);
            }
        }

        // Find the thread with the lowest amount of tasks and add the new task there
        protected override void QueueTask(Task task)
        {
            if (task != null)
            {
                int internalThreadId = 0;
                for (int i = 1; i < numberOfThreads ; i++)
                {
                    if (tasks[i].Count < tasks[internalThreadId].Count)
                        internalThreadId = i;
                }
                tasks[internalThreadId].Add(task);
            }
        }

        // Call to Dispose
        private void Dispose(bool dispose)
        {
            if (dispose)
            {
                for (int i = 0; i < numberOfThreads; i++)
                {
                    tasks[i].CompleteAdding();
                    tasks[i].Dispose();
                }
            }
        }

        // Return a List of all Tasks currently still being handled
        protected override IEnumerable<Task> GetScheduledTasks()
        {
            IEnumerable<Task> taskList = tasks[0].ToArray();
            for (int i = 1; i < numberOfThreads; i++)
            {
                taskList = taskList.Concat(tasks[i].ToArray());
            }
            return taskList;
        }

        // We don't allow inline execution
        protected override bool TryExecuteTaskInline(Task task, bool wasQueued)
        {
            return false;
        }
    }
}
