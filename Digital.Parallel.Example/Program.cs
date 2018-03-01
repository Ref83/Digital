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
        private static ConcurrentQueue<string> _collector = new ConcurrentQueue<string>();

        static void Main(string[] args)
        {
            var threadPool = new FixedThreadPool(2);

            StartTasks(threadPool, Priority.LOW, 20);
            StartTasks(threadPool, Priority.HIGH, 20);
            StartTasks(threadPool, Priority.NORMAL, 20);

            var pool = threadPool.Stop();

            Task.WaitAll(pool);

            Console.ReadKey();
        }

        private static void StartTasks(FixedThreadPool threadPool, Priority priority, int count)
        {
            var tasks = Enumerable.Range(0, count).Select(ind => 
                threadPool.Execute(new ActionTask(
                    () =>
                    {
                        //Console.WriteLine($"Task with priority {priority} number {ind} is started...");
                        var delay = 0.5;// _random.Next(3000) / 1000.0;
                        Thread.Sleep(TimeSpan.FromSeconds(delay));
                        Console.WriteLine($"Task with priority {priority} number {ind} fromy thread Id = {Thread.CurrentThread.ManagedThreadId}, complexity = {delay}");
                    }),
                    priority)
            )
            ;

            Task.WaitAll(tasks.ToArray());
        }



    }
}