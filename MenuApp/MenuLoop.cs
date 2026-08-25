using System.Globalization;
using Core;
using System.Linq;

namespace MenuApp
{
    public class MenuLoop
    {
        private static readonly MenuCommand[] Commands =
        {
            new("1", "Add job", "Queue a new job: input, output, notes and estimate."),
            new("2", "Monitor progress", "Live progress bars; press any key to return."),
            new("3", "Cancel one job", "Pick a job with the arrow keys and cancel it."),
            new("4", "Cancel all jobs", "Cancel every queued and running job, with a confirm."),
            new("5", "List jobs", "Show every job with status, progress and notes."),
            new("6", "Wait for jobs to finish", "Block until nothing is queued or running."),
            new("7", "Exit", "Finish queued jobs, stop the workers and quit."),
            new("8", "Help", "Show this screen.")
        };

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
                    case "8":
                        HandleHelp();
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

            foreach (var command in Commands)
                PrintOption(command.Key, command.Label);

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

        private static void HandleHelp()
        {
            ConsoleTheme.Header("=== Help ===");
            Console.WriteLine();

            foreach (var command in Commands)
            {
                ConsoleTheme.Write($"  {command.Key}) ", ConsoleColor.Cyan);
                Console.WriteLine(command.Label);
                ConsoleTheme.Muted($"     {command.Description}");
                Console.WriteLine();
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
            var jobs = _queue.Snapshot();

            if (jobs.Count == 0)
            {
                ConsoleTheme.Muted("No jobs yet.");
                return;
            }

            Console.Write($"Cancel all {jobs.Count} job(s)? (y/n): ");
            string? confirm = Console.ReadLine();

            if (!string.Equals(confirm, "y", StringComparison.OrdinalIgnoreCase))
            {
                ConsoleTheme.Muted("Canceled nothing.");
                return;
            }

            var results = _queue.CancelAll();

            int queued = results.Count(r => r.Result == CancelResult.CanceledQueued);
            int running = results.Count(r => r.Result == CancelResult.CanceledRunning);
            int finished = results.Count(r => r.Result == CancelResult.AlreadyFinished);

            ConsoleTheme.Success($"Canceled {queued} queued and {running} running job(s).");

            if (finished > 0)
                ConsoleTheme.Muted($"{finished} job(s) were already finished, left untouched.");
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
