using System.Text.Json;

namespace BankDumperLib
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1 || args.Length > 1)
            {
                Console.WriteLine();
                Console.WriteLine(" " + AppDomain.CurrentDomain.FriendlyName + " <input> <output> [--force]");
                Environment.Exit(1);
            }

            if (!File.Exists(args[0]))
            {
                Console.WriteLine("Input file doesn't exist.");
                Environment.Exit(1);
            }

            //var force = args.Length == 3 && args[2] == "--force";
            //var outputAlreadyExists = File.Exists(args[1]);
            //if (!force && outputAlreadyExists)
            //{
            //    Console.WriteLine("Output file already exist, add '--force' to overwrite.");
            //    Environment.Exit(1);
            //}

            // This is a example of loading magic numbers from a file
            using var magicNumbersFile = File.Open(Path.Combine(AppContext.BaseDirectory, "patterns.json"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);

            // This fills a sample file
            if (magicNumbersFile.Length == 0)
            {
                var sample = new List<Pattern>()
                {
                    new Pattern("FSB5"),
                    new Pattern("BKHD"),
                    new Pattern("AKPK"),
                };

                JsonSerializer.Serialize(magicNumbersFile, sample);

                magicNumbersFile.Position = 0;
            }

            var magicNumbers = JsonSerializer.Deserialize<List<Pattern>>(magicNumbersFile);

            // Validations are never bad
            if (magicNumbers != null)
            {
                foreach (var number in magicNumbers)
                {
                    if (!FileTools.TryAddPattern(number))
                    {
                        Console.WriteLine($"Could not add => '{number.Name}', it already exists.");
                    }
                }
            }

            using var input = File.Open(args[0], FileMode.Open, FileAccess.Read, FileShare.Read);

            var result = FileTools.Analyze(input);

            Console.WriteLine(result.ToString());

            foreach (var pattern in result.Matches)
            {
                // Do what you want here
            }
        }
    }
}
