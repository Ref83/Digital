using Digital.Parallel.ТаskScheduling;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Digital.Parallel.Tests
{
    public sealed class PriorityQueueTests
    {
        private readonly PriorityTaskQueueTester _queueTester = new PriorityTaskQueueTester();

        [Fact]
        public void Should_Dequeue_Queued_Tasks()
        {
            _queueTester.Enqueue("test", Priority.NORMAL);
            var result = _queueTester.Apply();

            result.Should().Equal("test");
        }

        [Fact]
        public void Same_Priority_Shold_Set_At_Last_Position()
        {
            _queueTester.Enqueue("low_1", Priority.LOW);
            _queueTester.Enqueue("low_2", Priority.LOW);

            var result = _queueTester.Apply();

            result.Should().Equal("low_1", "low_2");
        }

        [Fact]
        public void Same_Priority_Shold_Set_At_Last_Position_Of_Same_Priorities()
        {
            _queueTester.Enqueue("normal_1", Priority.NORMAL);
            _queueTester.Enqueue("low_1", Priority.LOW);
            _queueTester.Enqueue("normal_2", Priority.NORMAL);

            var result = _queueTester.Apply();

            result.Should().Equal("normal_1", "normal_2", "low_1");
        }      

        [Fact]
        public void Low_Priority_Shold_Be_Shifted_By_Greater_Priority()
        {
            _queueTester.Enqueue("low", Priority.LOW);
            _queueTester.Enqueue("normal", Priority.NORMAL);
            _queueTester.Enqueue("high", Priority.HIGH);

            var result = _queueTester.Apply();

            result.Should().Equal("high", "normal", "low");
        }

        [Fact]
        public void One_Normal_Shold_Set_After_Five_High()
        {
            Times(ind => _queueTester.Enqueue($"high_{ind}", Priority.HIGH), 6);
            _queueTester.Enqueue("normal", Priority.NORMAL);

            var result = _queueTester.Apply();

            result.Skip(4).Should().Equal(
                "high_4",
                "normal",
                "high_5");
        }

        [Fact]
        public void One_Normal_Shold_Set_After_Five_High_Ever_Partly_Dequeued()
        {
            Times(ind => _queueTester.Enqueue("none", Priority.HIGH), 4);
            _queueTester.Apply();

            Times(ind => _queueTester.Enqueue($"high_{ind}", Priority.HIGH), 2);
            _queueTester.Enqueue("normal", Priority.NORMAL);

            var result = _queueTester.Apply();

            result.Should().Equal(
                "high_0",
                "normal",
                "high_1");
        }

        [Fact]
        public void One_Normal_Shold_Set_After_Five_High_Ever_All_Dequeued()
        {
            Times(ind => _queueTester.Enqueue("none", Priority.HIGH), 6);
            _queueTester.Apply();

            Times(ind => _queueTester.Enqueue($"high_{ind}", Priority.HIGH), 2);
            _queueTester.Enqueue("normal", Priority.NORMAL);

            var result = _queueTester.Apply();

            result.Should().Equal(
                "normal",
                "high_0",
                "high_1");
        }

        [Fact]
        public void Two_Normal_Shold_Set_After_Ten_High()
        {
            Times(ind => _queueTester.Enqueue($"high_{ind}", Priority.HIGH), 11);
            Times(ind => _queueTester.Enqueue($"normal_{ind}", Priority.NORMAL), 4);

            var result = _queueTester.Apply();

            result.Skip(4).Take(3).Should().Equal(
                "high_4",
                "normal_0",
                "high_5");

            result.Skip(10).Take(4).Should().Equal(
                "high_9",
                "normal_1",
                "normal_2",
                "high_10");
        }

        [Fact]
        public void Two_Normal_Shold_Set_After_Ten_High_Ever_Partly_Dequeued()
        {
            Times(ind => _queueTester.Enqueue($"high_{ind}", Priority.HIGH), 9);
            Times(ind => _queueTester.Enqueue($"normal_{ind}", Priority.NORMAL), 2);
            _queueTester.Apply();

            Times(ind => _queueTester.Enqueue($"high_{ind}", Priority.HIGH), 2);
            Times(ind => _queueTester.Enqueue($"normal_{ind}", Priority.NORMAL), 2);

            var result = _queueTester.Apply();

            result.Should().Equal(
                "high_0",
                "normal_0",
                "normal_1",
                "high_1");
        }

        [Fact]
        public void Normal_Priority_Tasks_Debt_Can_Collected()
        {
            Times(ind => _queueTester.Enqueue($"high_{ind}", Priority.HIGH), 12);
            _queueTester.Apply();

            Times(ind => _queueTester.Enqueue($"high_{ind}", Priority.HIGH), 1);
            Times(ind => _queueTester.Enqueue($"normal_{ind}", Priority.NORMAL), 4);

            var result = _queueTester.Apply();

            result.Should().Equal(
                "normal_0",
                "normal_1",
                "normal_2",
                "high_0",
                "normal_3");
        }

        [Fact]
        public void ToArray_Returns_All_Queued_Items()
        {
            var queue = new PriorityQueue<string>();

            queue.Enqueue("low", Priority.LOW);
            queue.Enqueue("high", Priority.HIGH);
            queue.Enqueue("normal", Priority.NORMAL);

            var result = queue.ToArray();

            result.Should().Equal(
                "high",
                "normal",
                "low");
        }

        [Fact]
        public void ToArray_Returns_All_Queued_Items_Ever_Partly_Dequeued()
        {
            var queue = new PriorityQueue<string>();

            queue.Enqueue("low", Priority.LOW);
            queue.Enqueue("high", Priority.HIGH);
            queue.Enqueue("normal", Priority.NORMAL);

            queue.TryDequeue(out string item);

            var result = queue.ToArray();

            result.Should().Equal(
                "normal",
                "low");
        }

        [Fact]
        public void ToArray_Saves_Priority_Order_For_Normal_Items_Dequeued()
        {
            var queue = new PriorityQueue<string>();

            Times(ind => queue.Enqueue($"high_{ind}", Priority.HIGH), 6);
            Times(_ => queue.TryDequeue(out string item), 4);
            queue.Enqueue("normal", Priority.NORMAL);

            var result = queue.ToArray();

            result.Should().Equal(
                "high_4",
                "normal",
                "high_5"
                );
        }

        private void Times(Action<int> action, int times)
        {
            times.Should().BeGreaterOrEqualTo(1);

            foreach (var ind in Enumerable.Range(0, times))
                action(ind);
        }

        private sealed class PriorityTaskQueueTester
        {
            private readonly PriorityQueue<string> _queue;

            public PriorityTaskQueueTester()
            {
                _queue = new PriorityQueue<string>();
            }

            public void Enqueue(string text, Priority priority)
            {
                _queue.Enqueue(text, priority);
            }

            public IEnumerable<string> Apply()
            {
                var results = new List<string>();
                string item;
                while (_queue.TryDequeue(out item))
                    results.Add(item);

                return results;
            }
        }
    }
}
