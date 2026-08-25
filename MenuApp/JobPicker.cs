using Core;

namespace MenuApp
{
    public static class JobPicker
    {
        public static Job? Select(IReadOnlyList<Job> jobs, string title)
        {
            if (jobs.Count == 0)
                return null;

            int index = 0;

            while (true)
            {
                Console.Clear();
                ConsoleTheme.Header(title);
                Console.WriteLine();
                ConsoleTheme.Muted("Up/Down to move, Enter to select, Esc to go back");
                Console.WriteLine();

                for (int i = 0; i < jobs.Count; i++)
                    PrintRow(jobs[i], i == index);

                switch (Console.ReadKey(intercept: true).Key)
                {
                    case ConsoleKey.UpArrow:
                        index = (index - 1 + jobs.Count) % jobs.Count;
                        break;
                    case ConsoleKey.DownArrow:
                        index = (index + 1) % jobs.Count;
                        break;
                    case ConsoleKey.Enter:
                        return jobs[index];
                    case ConsoleKey.Escape:
                        return null;
                }
            }
        }

        private static void PrintRow(Job job, bool selected)
        {
            if (!selected)
            {
                Console.Write("  ");
                ConsoleTheme.Write($"{job.Status,-10}", ConsoleTheme.ColorFor(job.Status));
                Console.WriteLine($" {job.ProgressPercent,3}%  {job.InputPath} -> {job.OutputPath}");
                return;
            }

            string label = $"{job.Status,-10} {job.ProgressPercent,3}%  {job.InputPath} -> {job.OutputPath}";
            int width = SafeWidth();
            string row = ("> " + label).PadRight(width);

            if (row.Length > width)
                row = row[..width];

            Console.BackgroundColor = ConsoleColor.DarkCyan;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.Write(row);
            Console.ResetColor();
            Console.WriteLine();
        }

        private static int SafeWidth()
        {
            try
            {
                return Math.Max(40, Console.WindowWidth - 1);
            }
            catch (IOException)
            {
                return 80;
            }
        }
    }
}
