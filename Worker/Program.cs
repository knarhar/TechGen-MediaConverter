using System.Globalization;

namespace Worker
{
    internal class Program
    {
        static int Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.Error.WriteLine("Usage: Worker <input> <output> <notes> [estimateMs]");
                return 1;
            }

            double estimateMs = 5000;

            if (args.Length >= 4 &&
                double.TryParse(args[3], NumberStyles.Float, CultureInfo.InvariantCulture, out double parsed) &&
                parsed > 0)
            {
                estimateMs = parsed;
            }

            const int steps = 20;
            int delayMs = (int)Math.Max(1, estimateMs / steps);
            var random = new Random();

            for (int step = 1; step <= steps; step++)
            {
                Thread.Sleep(delayMs);
                Console.WriteLine($"PROGRESS {step * 100 / steps}");
            }

            bool success = random.Next(0, 100) < 80;
            return success ? 0 : 1;
        }
    }
}
