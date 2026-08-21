using System.Diagnostics;

namespace Core
{
    public class JobQueue
    {
        private readonly Queue<Job> _jobs = new();
        private readonly string _workerExePath;
        public event Action<Job, Process>? JobStarted;

        public JobQueue(string workerExePath)
        {
            _workerExePath = workerExePath;
        }
        public void AddJob(Job job)
        {
            _jobs.Enqueue(job);
        }

        public int QueuedCount => _jobs.Count;
        public void RunAll()
        {
            while (_jobs.Count > 0)
            {
                var job = _jobs.Dequeue();
                RunJob(job);
            }
        }

        private void RunJob(Job job)
        {
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

            process.Start();
            JobStarted?.Invoke(job, process);

            process.WaitForExit();

            job.WorkerProcess = null;
        }

        private static string BuildArguments(Job job)
        {
            return $"\"{job.InputPath}\" \"{job.OutputPath}\" \"{job.Options.Notes}\"";
        }
    }
}