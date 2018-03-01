using System;

namespace Digital.Parallel
{
    public class ActionTask : ITask
    {
        private readonly Action _action;

        public ActionTask(Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            _action = action;
        }

        public void Execute()
        {
            _action();
        }
    }
}
