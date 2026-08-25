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
                PrintMenu();
                string? choice = Console.ReadLine();

                switch (choice)
                {
                    case "1": HandleAddJob(); break;
                    case "2": HandleMonitor(); break;
                    case "3": HandleCancelOne(); break;
                    case "4": HandleCancelAll(); break;
                    case "5": HandleList(); break;
                    case "6": HandleWait(); break;
                    case "7":
                        _queue.Stop();
                        running = false;
                        break;
                    default:
                        Console.WriteLine("Unknown option, try again.");
                        break;
                }
            }
        }

        private void PrintMenu()
        {
            Console.WriteLine();
            Console.WriteLine("=== Conversion Manager ===");
            Console.WriteLine("1) Add job");
            Console.WriteLine("2) Monitor progress");
            Console.WriteLine("3) Cancel one job");
            Console.WriteLine("4) Cancel all jobs");
            Console.WriteLine("5) List jobs");
            Console.WriteLine("6) Wait for jobs to finish");
            Console.WriteLine("7) Exit");
            Console.Write("Choose an option: ");
        }

        private void HandleAddJob()
        {
            Console.Write("Input path: ");
            string input = Console.ReadLine() ?? "";

            Console.Write("Output path: ");
            string output = Console.ReadLine() ?? "";

            Console.Write("Options/notes (optional, press Enter to skip): ");
            string notes = Console.ReadLine() ?? "";

            var job = new Job(input, output, new JobOptions { Notes = notes });
            _queue.AddJob(job);

            Console.WriteLine($"Job {job.Id} added and queued.");
        }

        private void HandleMonitor()
        {
            new JobMonitor(_queue).Start();
        }

        private void HandleCancelOne()
        {
            Console.Write("Job id to cancel: ");
            string? idInput = Console.ReadLine();
            Console.WriteLine($"Cancel requested for job {idInput} (not implemented yet).");
        }

        private void HandleCancelAll()
        {
            Console.WriteLine("Cancel all requested (not implemented yet).");
        }

        private void HandleList()
        {
            var jobs = _queue.Snapshot();

            if (jobs.Count == 0)
            {
                Console.WriteLine("No jobs yet.");
                return;
            }

            foreach (var job in jobs)
            {
                Console.WriteLine(
                    $"[{job.Id}] {job.InputPath} -> {job.OutputPath} | {job.Status} | {job.ProgressPercent}% | Notes: {job.Options.Notes}");
            }
        }

        private void HandleWait()
        {
            Console.WriteLine("Waiting for all queued/running jobs to finish...");
            _queue.WaitForIdle();
            Console.WriteLine("All jobs finished.");
        }
    }
}