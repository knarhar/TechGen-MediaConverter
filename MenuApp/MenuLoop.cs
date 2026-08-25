using System.Globalization;
using Core;

namespace MenuApp
{
    public class MenuLoop
    {
        private readonly JobQueue _queue;

        public MenuLoop(JobQueue queue)
        {
            _queue = queue;
        }

        public void Run()
        {
            bool running = true;
            while (running)
            {
                Console.Clear();
                PrintMenu();

                string? choice = Console.ReadLine();
                Console.WriteLine();

                bool pause = true;

                switch (choice)
                {
                    case "1":
                        HandleAddJob();
                        break;
                    case "2":
                        HandleMonitor();
                        pause = false;
                        break;
                    case "3":
                        HandleCancelOne();
                        break;
                    case "4":
                        HandleCancelAll();
                        break;
                    case "5":
                        HandleList();
                        break;
                    case "6":
                        HandleWait();
                        break;
                    case "7":
                        ConsoleTheme.Muted("Finishing queued jobs, please wait...");
                        _queue.Stop();
                        ConsoleTheme.Success("Goodbye.");
                        running = false;
                        pause = false;
                        break;
                    default:
                        ConsoleTheme.Error("Unknown option, try again.");
                        break;
                }

                if (pause)
                    Pause();
            }
        }

        private static void Pause()
        {
            Console.WriteLine();
            ConsoleTheme.Muted("Press any key to return to the menu...");
            Console.ReadKey(intercept: true);
        }

        private void PrintMenu()
        {
            ConsoleTheme.Header("=== Conversion Manager ===");
            Console.WriteLine();

            PrintOption("1", "Add job");
            PrintOption("2", "Monitor progress");
            PrintOption("3", "Cancel one job");
            PrintOption("4", "Cancel all jobs");
            PrintOption("5", "List jobs");
            PrintOption("6", "Wait for jobs to finish");
            PrintOption("7", "Exit");

            Console.WriteLine();
            ConsoleTheme.Muted($"queued: {_queue.QueuedCount}   total: {_queue.Snapshot().Count}");
            Console.WriteLine();
            Console.Write("Choose an option: ");
        }

        private static void PrintOption(string key, string label)
        {
            ConsoleTheme.Write($"  {key}) ", ConsoleColor.Cyan);
            Console.WriteLine(label);
        }

        private void HandleAddJob()
        {
            Console.Write("Input path: ");
            string input = Console.ReadLine() ?? "";

            Console.Write("Output path: ");
            string output = Console.ReadLine() ?? "";

            Console.Write("Options/notes (optional, press Enter to skip): ");
            string notes = Console.ReadLine() ?? "";

            double estimateMs = ReadEstimateMs();

            var job = new Job(input, output, new JobOptions
            {
                Notes = notes,
                Estimate = estimateMs
            });

            _queue.AddJob(job);

            ConsoleTheme.Success($"Job {job.Id} queued (~{estimateMs / 1000:0.#}s).");
        }

        private static double ReadEstimateMs()
        {
            const double defaultSeconds = 5;

            while (true)
            {
                Console.Write($"Estimated duration in seconds (Enter for {defaultSeconds:0.#}): ");
                string raw = (Console.ReadLine() ?? "").Trim();

                if (raw.Length == 0)
                    return defaultSeconds * 1000;

                if (double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out double seconds) &&
                    seconds > 0)
                {
                    return seconds * 1000;
                }

                ConsoleTheme.Error("Enter a positive number of seconds.");
            }
        }

        private void HandleMonitor()
        {
            new JobMonitor(_queue).Start();
        }

        private void HandleCancelOne()
        {
            var jobs = _queue.Snapshot();

            if (jobs.Count == 0)
            {
                ConsoleTheme.Muted("No jobs yet.");
                return;
            }

            var job = JobPicker.Select(jobs, "Select a job to cancel");

            if (job == null)
                return;

            switch (_queue.CancelJob(job.Id))
            {
                case CancelResult.CanceledQueued:
                    ConsoleTheme.Success($"Job {job.Id} canceled before it started.");
                    break;
                case CancelResult.CanceledRunning:
                    ConsoleTheme.Success($"Job {job.Id} was running - worker process killed.");
                    break;
                case CancelResult.AlreadyFinished:
                    ConsoleTheme.Warning($"Job {job.Id} already finished ({job.Status}).");
                    break;
                case CancelResult.NotFound:
                    ConsoleTheme.Error($"Job {job.Id} not found.");
                    break;
            }
        }

        private void HandleCancelAll()
        {
            // TODO: wire up real CancelAll logic once cancel branches merge
            ConsoleTheme.Warning("Cancel all requested (not implemented yet).");
        }

        private void HandleList()
        {
            var jobs = _queue.Snapshot();

            if (jobs.Count == 0)
            {
                ConsoleTheme.Muted("No jobs yet.");
                return;
            }

            ConsoleTheme.Header("Jobs");
            Console.WriteLine();

            foreach (var job in jobs)
            {
                ConsoleTheme.Write($"  {job.Status,-10}", ConsoleTheme.ColorFor(job.Status));
                Console.Write($" {job.ProgressPercent,3}%  ");
                Console.Write($"{job.InputPath} -> {job.OutputPath}");

                if (!string.IsNullOrWhiteSpace(job.Options.Notes))
                    ConsoleTheme.Write($"  ({job.Options.Notes})", ConsoleColor.DarkGray);

                Console.WriteLine();
                ConsoleTheme.Muted($"             {job.Id}");
            }
        }

        private void HandleWait()
        {
            ConsoleTheme.Muted("Waiting for all queued/running jobs to finish...");
            _queue.WaitForIdle();
            ConsoleTheme.Success("All jobs finished.");
        }
    }
}
