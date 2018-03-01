using Digital.Parallel.ТаsкScheduling;
using System.Threading;
using System.Threading.Tasks;

namespace Digital.Parallel
{
    public class FixedThreadPool
    {
        private static readonly Task<bool> TrueResult = Task.FromResult<bool>(true);
        private static readonly Task<bool> FalseResult = Task.FromResult<bool>(false);

        private readonly TaskQueueScheduler _scheduler;
        
        public FixedThreadPool(int concurrencyLevel)
        {
            _scheduler = new TaskQueueScheduler(concurrencyLevel);
        }

        public async Task<bool> Execute(ITask task, Priority priority)
        {
            if (_scheduler.IsStopping())
                return await FalseResult;

            Task.Factory.StartNew(() => task.Execute(), CancellationToken.None, TaskCreationOptions.None, _scheduler.GetSchedulerFor(priority));

            return await TrueResult;
        }

        public async Task Stop()
        {
            await Task.Run(() => _scheduler.Stop());            
        }
    }
}
