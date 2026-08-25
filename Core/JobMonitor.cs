namespace Core
{
    public class JobMonitor
    {
        private readonly JobQueue _queue;
        private readonly int _refreshMs;

        public JobMonitor(JobQueue queue, int refreshMs = 500)
        {
            _queue = queue;
            _refreshMs = refreshMs;
        }

        public void Start()
        {
            if (_queue.Snapshot().Count == 0)
            {
                Console.WriteLine("No jobs to monitor yet.");
                return;
            }

            Console.WriteLine("(press any key to return to the menu)");

            while (true)
            {
                if (!Console.IsInputRedirected && Console.KeyAvailable)
                {
                    Console.ReadKey(intercept: true);
                    return;
                }

                var jobs = _queue.Snapshot();

                Console.Clear();
                foreach (var job in jobs)
                    PrintJob(job);

                bool allFinished = jobs.All(job =>
                    job.Status is JobStatus.COMPLETED or JobStatus.FAILED or JobStatus.CANCELED);

                if (allFinished)
                    break;

                Thread.Sleep(_refreshMs);
            }

            Console.WriteLine("All jobs finished. Press any key to return to the menu.");
            Console.ReadKey(intercept: true);
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