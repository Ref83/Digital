using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Digital.Parallel.Example
{
    class Program
    {
        private static readonly Random _random = new Random();

        static void Main(string[] args)
        {
            var threadPool = new FixedThreadPool(3);

            var producerLow = StartTasks(threadPool, Priority.LOW, 20);
            var producerHigh = StartTasks(threadPool, Priority.HIGH, 20);
            var producerNormal = StartTasks(threadPool, Priority.NORMAL, 20);

            Task.WaitAll(producerLow, producerNormal, producerHigh);

            var pool = threadPool.Stop();

            Task.WaitAll(pool);

            Console.ReadKey();
        }

        private static async Task StartTasks(FixedThreadPool threadPool, Priority priority, int count)
        {
            await Task.Run(() =>
            {
                foreach (var ind in Enumerable.Range(0, count))
                {
                    threadPool.Execute(new ActionTask(
                        () =>
                        {
                            var delay = _random.Next(1000) / 1000.0;
                            Thread.Sleep(TimeSpan.FromSeconds(delay));
                            Console.WriteLine($"Task with priority {priority} number {ind} fromy thread Id = {Thread.CurrentThread.ManagedThreadId}, complexity = {delay}");
                        }),
                        priority);
                }
            }
            );
        }



    }
}