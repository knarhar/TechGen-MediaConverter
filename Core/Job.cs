using System.Diagnostics;

namespace Core
{
    public enum JobStatus
    {
        QUEUED,
        RUNNING,
        COMPLETED,
        FAILED,
        CANCELED
    }

    public class JobOptions
    {
        public double Estimate { get; set; } // milliseconds
        public string Notes { get; set; }
    }

    public class Job
    {
        public Guid Id { get; } = Guid.NewGuid();
        public string InputPath { get; set; }
        public string OutputPath { get; set; }
        public JobOptions Options { get; set; }
        public JobStatus Status { get; set; }
        public int ProgressPercent { get; set; }
        public Process? WorkerProcess { get; set; }

        public Job(string input, string output, JobOptions options)
        {
            InputPath = input;
            OutputPath = output;
            Options = options;
            Status = JobStatus.QUEUED;
            ProgressPercent = 0;
        }
    }
}
