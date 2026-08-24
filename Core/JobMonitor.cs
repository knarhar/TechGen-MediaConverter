namespace Core
{
    public class JobMonitor
    {
        private readonly List<Job> _jobs;

        public JobMonitor(List<Job> jobs)
        {
            _jobs = jobs;
        }

        public void Start()
        {
            while (true)
            {
                Console.Clear();

                foreach (var job in _jobs)
                {
                    PrintJob(job);
                }

                bool allFinished = _jobs.All(job =>
                    job.Status == JobStatus.COMPLETED ||
                    job.Status == JobStatus.FAILED ||
                    job.Status == JobStatus.CANCELED);

                if (allFinished)
                    break;

                Thread.Sleep(500);
            }
        }

        private static void PrintJob(Job job)
        {
            const int barLength = 30;

            int completed = job.ProgressPercent * barLength / 100;
            int remaining = barLength - completed;

            string progressBar =
                new string('#', completed) +
                new string('-', remaining);

            Console.WriteLine(
                $"{job.Id} | {job.Status,-10} | [{progressBar}] {job.ProgressPercent}%");
        }
    }
}