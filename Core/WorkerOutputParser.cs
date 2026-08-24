using System.Diagnostics;

namespace Core
{
    public static class WorkerOutputParser
    {
        public static void Parse(Process process, Job job)
        {
            job.Status = JobStatus.RUNNING;

            while (!process.StandardOutput.EndOfStream)
            {
                string? line = process.StandardOutput.ReadLine();

                if (line == null)
                    continue;

                Console.WriteLine($"[{job.Id}] {line}");

                ParseLine(line, job);
            }

            process.WaitForExit();

            if (job.Status == JobStatus.CANCELED)
                return;

            if (process.ExitCode == 0)
            {
                job.ProgressPercent = 100;
                job.Status = JobStatus.COMPLETED;
            }
            else
            {
                job.Status = JobStatus.FAILED;
            }
        }

        private static void ParseLine(string line, Job job)
        {
            const string prefix = "PROGRESS ";

            if (!line.StartsWith(prefix))
                return;

            string value = line[prefix.Length..];

            if (int.TryParse(value, out int progress))
            {
                job.ProgressPercent = Math.Clamp(progress, 0, 100);
            }
        }
    }
}