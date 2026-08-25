namespace Core
{
    public static class ConsoleTheme
    {
        public static void Write(string text, ConsoleColor color)
        {
            var previous = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(text);
            Console.ForegroundColor = previous;
        }

        public static void WriteLine(string text, ConsoleColor color)
        {
            Write(text + Environment.NewLine, color);
        }

        public static void Header(string text)
        {
            WriteLine(text, ConsoleColor.Cyan);
        }

        public static void Success(string text)
        {
            WriteLine(text, ConsoleColor.Green);
        }

        public static void Warning(string text)
        {
            WriteLine(text, ConsoleColor.Yellow);
        }

        public static void Error(string text)
        {
            WriteLine(text, ConsoleColor.Red);
        }

        public static void Muted(string text)
        {
            WriteLine(text, ConsoleColor.DarkGray);
        }

        public static ConsoleColor ColorFor(JobStatus status) => status switch
        {
            JobStatus.QUEUED => ConsoleColor.DarkGray,
            JobStatus.RUNNING => ConsoleColor.Yellow,
            JobStatus.COMPLETED => ConsoleColor.Green,
            JobStatus.FAILED => ConsoleColor.Red,
            JobStatus.CANCELED => ConsoleColor.DarkYellow,
            _ => ConsoleColor.Gray
        };
    }
}
