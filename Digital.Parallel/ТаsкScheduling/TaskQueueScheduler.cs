using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Digital.Parallel.ТаsкScheduling
{
    public class TaskQueueScheduler : TaskScheduler
    {
        private readonly PriorityQueue<ScheduledTask> _taskQueue = new PriorityQueue<ScheduledTask>();
        private readonly IDictionary<Priority, PriorityTaskScheduler> _schedulers;

        private readonly int _concurrencyLevel;
        private int _taskCount = 0;

        private readonly CancellationTokenSource _cancallation = new CancellationTokenSource();

        public TaskQueueScheduler(int concurrencyLevel)
        {
            if (concurrencyLevel <= 0) throw new ArgumentOutOfRangeException($"{nameof(concurrencyLevel)} should be greater than 0", nameof(concurrencyLevel));

            _concurrencyLevel = concurrencyLevel;

            _schedulers = Enum.GetValues(typeof(Priority))
                .Cast<Priority>()
                .ToDictionary(p => p, p => new PriorityTaskScheduler(_taskQueue, p, this)); ;
        }

        public TaskScheduler GetSchedulerFor(Priority priority)
        {
            return _schedulers[priority];
        }

        public void Stop()
        {
            _cancallation.Cancel();

            Task.WaitAll(_taskQueue.ToArray().Select(t => t.Task).ToArray());
        }

        public bool IsStopping()
        {
            return _cancallation.IsCancellationRequested;
        }

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            throw new NotImplementedException();
        }

        protected override void QueueTask(Task task)
        {
            if (_cancallation.IsCancellationRequested)
                return;

            Interlocked.Increment(ref _taskCount);
            if (_taskCount <= _concurrencyLevel)
                Task.Factory.StartNew(() => ExecuteQueuedTasks(), CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            throw new NotImplementedException();
        }

        private void ExecuteQueuedTasks()
        {

            ScheduledTask scheduledTask;
            while (_taskQueue.TryDequeue(out scheduledTask))
            {
                scheduledTask.Scheduler.Execute(scheduledTask.Task);
            }

            Interlocked.Decrement(ref _taskCount);
        }

        private void NotifyTaskEnqueued()
        {
            QueueTask(null);
        }

        private class PriorityTaskScheduler : TaskScheduler
        {
            private readonly PriorityQueue<ScheduledTask> _taskQueue;
            private readonly Priority _priority;
            private readonly TaskQueueScheduler _parentScheduler;

            public PriorityTaskScheduler(PriorityQueue<ScheduledTask> taskQueue, Priority priority, TaskQueueScheduler parentScheduler)
            {
                _taskQueue = taskQueue ?? throw new ArgumentNullException(nameof(taskQueue));
                _priority = priority;
                _parentScheduler = parentScheduler ?? throw new ArgumentNullException(nameof(parentScheduler));
            }

            protected override IEnumerable<Task> GetScheduledTasks()
            {
                return new Task[0];
            }

            protected override void QueueTask(Task task)
            {
                if (_parentScheduler.IsStopping())
                    return;

                _taskQueue.Enqueue(new ScheduledTask(task, this), _priority);
                _parentScheduler.NotifyTaskEnqueued();
            }

            protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
            {
                return false;
            }

            internal void Execute(Task task)
            {
                TryExecuteTask(task);
            }
        }

        private class ScheduledTask
        {
            public ScheduledTask(Task task, PriorityTaskScheduler scheduler)
            {
                Task = task ?? throw new ArgumentNullException(nameof(task));
                Scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
            }

            public Task Task { get; }
            public PriorityTaskScheduler Scheduler { get; }
        }
    }
}
