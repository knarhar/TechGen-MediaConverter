using Core;

namespace MenuApp
{
    internal class Program
    {
        // add your worker.exe path here
        const string WORKERPATH = "\"C:\\Users\\harut\\OneDrive\\Desktop\\folder\\projects\\ameria\\console\\TechGen-MediaConverter\\Worker\\bin\\Debug\\net8.0\\Worker.exe\"";
        static void Main(string[] args)
        {
            var queue = new JobQueue(WORKERPATH);
            queue.JobStarted += (job, process) => WorkerOutputParser.Parse(process, job);

            var job = new Job("input.mp4", "output.mp4", new JobOptions { Estimate = 5000, Notes = "test run" });
            queue.AddJob(job);
            queue.RunAll();
        }
    }
}
