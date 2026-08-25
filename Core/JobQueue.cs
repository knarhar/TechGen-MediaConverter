using System.Diagnostics;
using System.Globalization;

namespace Core
{
    public class JobQueue
    {
        private readonly object _lock = new();
        private readonly Queue<Job> _pending = new();
        private readonly List<Job> _allJobs = new();
        private readonly List<Thread> _workerThreads = new();
        private readonly string _workerExePath;
        private readonly int _maxConcurrency;
        private int _activeCount;
        private bool _stopping;

        public event Action<Job, Process>? JobStarted;

        public JobQueue(string workerExePath, int maxConcurrency = 3)
        {
            _workerExePath = workerExePath;
            _maxConcurrency = maxConcurrency;
        }

        public void AddJob(Job job)
        {
            lock (_lock)
            {
                _allJobs.Add(job);
                _pending.Enqueue(job);
                Monitor.PulseAll(_lock);
            }
        }

        public int QueuedCount
        {
            get { lock (_lock) { return _pending.Count; } }
        }

        public IReadOnlyList<Job> Snapshot()
        {
            lock (_lock)
            {
                return _allJobs.ToList();
            }
        }

        public void Start()
        {
            lock (_lock)
            {
                if (_workerThreads.Count > 0)
                    return;

                _stopping = false;

                for (int i = 0; i < _maxConcurrency; i++)
                {
                    var thread = new Thread(WorkerLoop)
                    {
                        IsBackground = true,
                        Name = $"JobWorker-{i}"
                    };
                    _workerThreads.Add(thread);
                    thread.Start();
                }
            }
        }

        public void Stop()
        {
            lock (_lock)
            {
                _stopping = true;
                Monitor.PulseAll(_lock);
            }

            foreach (var thread in _workerThreads)
                thread.Join();

            lock (_lock)
            {
                _workerThreads.Clear();
            }
        }

        public void WaitForIdle()
        {
            lock (_lock)
            {
                while (_pending.Count > 0 || _activeCount > 0)
                    Monitor.Wait(_lock);
            }
        }

        private void WorkerLoop()
        {
            while (true)
            {
                Job job;

                lock (_lock)
                {
                    while (_pending.Count == 0 && !_stopping)
                        Monitor.Wait(_lock);

                    if (_pending.Count == 0 && _stopping)
                        return;

                    job = _pending.Dequeue();
                    _activeCount++;
                }

                RunJob(job);

                lock (_lock)
                {
                    _activeCount--;
                    Monitor.PulseAll(_lock);
                }
            }
        }

        private void RunJob(Job job)
        {
            if (job.Status == JobStatus.CANCELED)
                return;

            job.Status = JobStatus.RUNNING;

            var psi = new ProcessStartInfo
            {
                FileName = _workerExePath,
                Arguments = BuildArguments(job),
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var process = new Process { StartInfo = psi };
            job.WorkerProcess = process;

            try
            {
                process.Start();
                JobStarted?.Invoke(job, process);

                process.WaitForExit();
            }
            catch (Exception ex)
            {
                job.Status = JobStatus.FAILED;
                job.Options.Notes = $"{job.Options.Notes} [error: {ex.Message}]";
            }
            finally
            {
                job.WorkerProcess = null;
            }
        }

        private static string BuildArguments(Job job)
        {
            string estimate = job.Options.Estimate.ToString(CultureInfo.InvariantCulture);

            return $"\"{job.InputPath}\" \"{job.OutputPath}\" \"{job.Options.Notes}\" \"{estimate}\"";
        }
    }
}