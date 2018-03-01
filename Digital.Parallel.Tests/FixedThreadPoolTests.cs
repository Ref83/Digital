using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Digital.Parallel.Tests
{
    public sealed class FixedThreadPoolTests
    {
        private readonly FixedThreadPool _threadPool;

        public FixedThreadPoolTests()
        {
            _threadPool = new FixedThreadPool(5);
        }

        [Fact]
        public async Task Execute_Planed_Tasks()
        {
            var count = 0;
            _threadPool.Execute(new ActionTask(() => Interlocked.Increment(ref count)), Priority.NORMAL);
            await _threadPool.Stop();

            Assert.Equal(1, count);
        }
    }
}
