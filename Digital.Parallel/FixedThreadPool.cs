using Digital.Parallel.ТаskScheduling;
using System.Threading;
using System.Threading.Tasks;

namespace Digital.Parallel
{
    public class FixedThreadPool
    {
        private readonly TaskQueueScheduler _scheduler;
        
        public FixedThreadPool(int concurrencyLevel)
        {
            _scheduler = new TaskQueueScheduler(concurrencyLevel);
        }

        public bool Execute(ITask task, Priority priority)
        {
            if (_scheduler.IsStopping())
                return false;

            Task.Factory.StartNew(() => task.Execute(), CancellationToken.None, TaskCreationOptions.None, _scheduler.GetSchedulerFor(priority));

            return true;
        }

        public async Task Stop()
        {
            await _scheduler.Stop();
        }
    }
}
