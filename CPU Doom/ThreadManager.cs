using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPU_Doom
{

    public class ThreadManagerFor : ThreadManager
    {
        public ThreadManagerFor(Action<int>[] tasks)
            : base(tasks.Select((task, i) => (Action) (() => task(i))).ToArray()) {}

        public ThreadManagerFor(int start, int count, Action<int> task)
            : base((from i in Enumerable.Range(start, count) select (Action) (() => task(i))).ToArray()) { }
    }


    public class ThreadManager
    {
        public bool Finished => _workingWorkers <= 0;

        private readonly Action[] _tasks;
        private readonly Worker[] _workers;
        private int _workingWorkers;
        private readonly int _basicRange;

        public ThreadManager(Action[] tasks)
        {
            _tasks = tasks;
            _workers = new Worker[Environment.ProcessorCount];
            _basicRange = tasks.Length / _workers.Length;
        }

        public void Execute()
        {
            int maxWorkers = _workers.Length;
            _workingWorkers = maxWorkers;

            for (int i = 0; i < maxWorkers; i++)
            {
                Worker worker = _workers[i] = new Worker(i, this);
                Thread thread = new Thread(worker.Execute);
                thread.Start();
            }
        }

        private void SignOff()
        {
            Interlocked.Decrement(ref _workingWorkers);
        }

        class Worker
        {
            private readonly int _workerID;
            private readonly ThreadManager _manager;
            private readonly Random _random = new Random();
            private int _from, _to;

            public Worker(int workerID, ThreadManager manager)
            {
                _workerID = workerID;
                _manager = manager;
                _from = manager._basicRange * workerID;
                _to = workerID == manager._workers.Length - 1 ? manager._tasks.Length : _from + manager._basicRange;
            }

            public void Execute()
            {
                for (int i = _from; i < _to; ++i)
                {
                    TryTakeTask(_manager._tasks[i]);
                }
                _manager.SignOff();

                while (!_manager.Finished)
                {
                    int stealFrom = _random.Next(_manager._workers.Length);
                    if (stealFrom == _workerID) continue;
                    Worker victim = _manager._workers[stealFrom];
                    if (victim == null) continue;
                    int taskIndex = victim._from + _random.Next(victim._to - victim._from);
                    TryTakeTask(_manager._tasks[taskIndex]);
                }
            }

            private void TryTakeTask(Action task)
            {
                if (Monitor.TryEnter(task))
                {
                    try
                    {
                        task();
                    }
                    finally
                    {
                        Monitor.Exit(task);
                    }
                }
            }
        }
    }
}
