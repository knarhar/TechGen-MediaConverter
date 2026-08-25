using MenuApp;
using Core;

namespace MenuApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string workerPath = ResolveWorkerPath();

            var queue = new JobQueue(workerPath, maxConcurrency: 3);
            queue.JobStarted += (job, process) => WorkerOutputParser.Parse(process, job);
            queue.Start();

            var menu = new MenuLoop(queue);
            menu.Run();

            static string ResolveWorkerPath()
            {
                var baseDir = AppContext.BaseDirectory;
                var solutionDir = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", ".."));
                var exeName = OperatingSystem.IsWindows() ? "Worker.exe" : "Worker";
                return Path.Combine(solutionDir, "Worker", "bin", "Debug", "net8.0", exeName);
            }
        }
    }
}