using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Digital.Parallel.ТаsкScheduling
{
    public class PriorityQueue<T>
    {
        private readonly (int HighTaskCount, int NormalTaskCount) FirstThreshold = (5, 1);
        private readonly (int HighTaskCount, int NormalTaskCount) SecondThreshold = (10, 2);

        private readonly object _lock = new object();

        private readonly IDictionary<Priority, ConcurrentQueue<T>> _queues =
            Enum.GetValues(typeof(Priority)).Cast<Priority>().ToDictionary(p => p, p => new ConcurrentQueue<T>());

        private int _highPriorityTasksCount = 0;
        private int _normalPriorityTasksDebt = 0;

        public void Enqueue(T task, Priority priority)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));
            _queues[priority].Enqueue(task);
        }

        public bool TryDequeue(out T task)
        {
            lock (_lock)
            {
                Priority priority;

                if (_normalPriorityTasksDebt > 0)
                {
                    if (!TryDequeue(out task, out priority, Priority.NORMAL, Priority.HIGH, Priority.LOW))
                        return false;

                    if (priority == Priority.NORMAL)
                        _normalPriorityTasksDebt--;
                }
                else
                {
                    if (!TryDequeue(out task, out priority, Priority.HIGH, Priority.NORMAL, Priority.LOW))
                        return false;
                }

                if (priority == Priority.HIGH)
                {
                    _highPriorityTasksCount++;

                    if (_highPriorityTasksCount == FirstThreshold.HighTaskCount)
                        _normalPriorityTasksDebt += FirstThreshold.NormalTaskCount;

                    if (_highPriorityTasksCount == SecondThreshold.HighTaskCount)
                    {
                        _normalPriorityTasksDebt += SecondThreshold.NormalTaskCount;
                        _highPriorityTasksCount = 0;
                    }
                }

                return true;
            }
        }

        public T[] ToArray()
        {
            lock(_lock)
            {
                return _queues
                    .Values
                    .SelectMany(q => q.ToArray())
                    .ToArray();
            }
        }

        private bool TryDequeue(out T task, out Priority priority, params Priority[] priorityOrder)
        {
            foreach (var priorityInd in priorityOrder)
            {
                if (_queues[priorityInd].TryDequeue(out task))
                {
                    priority = priorityInd;
                    return true;
                }
            }

            task = default(T);
            priority = default(Priority);
            return false;
        }
    }
}
