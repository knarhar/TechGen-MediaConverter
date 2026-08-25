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
                ConsoleTheme.Muted("No jobs to monitor yet.");
                Console.WriteLine();
                ConsoleTheme.Muted("Press any key to return to the menu...");
                Console.ReadKey(intercept: true);
                return;
            }

            while (true)
            {
                if (!Console.IsInputRedirected && Console.KeyAvailable)
                {
                    Console.ReadKey(intercept: true);
                    return;
                }

                var jobs = _queue.Snapshot();

                Console.Clear();
                ConsoleTheme.Header("=== Live Monitor ===");
                Console.WriteLine();

                foreach (var job in jobs)
                    PrintJob(job);

                Console.WriteLine();
                ConsoleTheme.Muted("Press any key to return to the menu...");

                bool allFinished = jobs.All(job =>
                    job.Status is JobStatus.COMPLETED or JobStatus.FAILED or JobStatus.CANCELED);

                if (allFinished)
                    break;

                Thread.Sleep(_refreshMs);
            }

            Console.WriteLine();
            ConsoleTheme.Success("All jobs finished. Press any key to return to the menu.");
            Console.ReadKey(intercept: true);
        }

        private static void PrintJob(Job job)
        {
            const int barLength = 30;

            int completed = job.ProgressPercent * barLength / 100;
            int remaining = barLength - completed;

            var color = ConsoleTheme.ColorFor(job.Status);

            ConsoleTheme.Write($"  {job.Status,-10}", color);

            Console.Write(" [");
            ConsoleTheme.Write(new string('#', completed), color);
            ConsoleTheme.Write(new string('-', remaining), ConsoleColor.DarkGray);
            Console.Write("] ");

            ConsoleTheme.Write($"{job.ProgressPercent,3}%", color);

            Console.WriteLine($"  {job.InputPath} -> {job.OutputPath}");
        }
    }
}
