using System.Text.Json;

namespace BankDumperLib
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2 || args.Length > 3)
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

            var force = args.Length == 3 && args[2] == "--force";
            var outputAlreadyExists = File.Exists(args[1]);
            if (!force && outputAlreadyExists)
            {
                Console.WriteLine("Output file already exist, add '--force' to overwrite.");
                Environment.Exit(1);
            }

#if true
            // This is a example of loading magic numbers from a file
            using var magicNumbersFile = File.Open(Path.Combine(AppContext.BaseDirectory, "magicNumbers.json"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);

            // This fills a sample file
            if (magicNumbersFile.Length == 0)
            {
                var sample = new List<MagicNumber>()
                {
                    new MagicNumber("FSB5"),
                    new MagicNumber("BKHD"),
                    new MagicNumber("AKPK"),
                };

                JsonSerializer.Serialize(magicNumbersFile, sample);

                magicNumbersFile.Position = 0;
            }

            var magicNumbers = JsonSerializer.Deserialize<List<MagicNumber>>(magicNumbersFile);

            // Validations are never bad
            if (magicNumbers != null)
            {
                foreach (var number in magicNumbers)
                {
                    if (!BankDumper.TryAddMagicNumber(number))
                    {
                        Console.WriteLine($"Could not add => '{number.Text}', it already exists.");
                    }
                }
            }

#else
            BankDumper.LoadDefaultMagicNumbers();
#endif

            var fileCount = 0;

            MagicNumberFound? marker;
            var outputName = string.Format(args[1], fileCount);
            using (var input = File.Open(args[0], FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                using (var output = File.Open(outputName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
                {
                    marker = BankDumper.Extract(input, output);
                    if (marker == null)
                    {
                        if (!outputAlreadyExists)
                        {
                            // Cleanup here
                            File.Delete(outputName);
                        }
                        Console.WriteLine("No markers found");
                        Environment.Exit(1);
                    }

                    Console.WriteLine($"Success, marker => '{marker.Pattern.Text}' found at => '{marker.Position}'");
                }
            }



            while (marker != null)
            {
                // Make the old output the new input
                using (var newInput = File.Open(outputName, FileMode.Open, FileAccess.ReadWrite, FileShare.Read))
                {
                    fileCount++;
                    outputName = string.Format(args[1], fileCount);

                    var newOutputAlreadyExists = File.Exists(outputName);

                    using (var newOutput = File.Open(outputName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
                    {
                        marker = BankDumper.ExtractAndCut(newInput, newOutput);

                        if (marker == null)
                        {
                            // Prevents Error
                            newOutput.Close();

                            if (!newOutputAlreadyExists)
                            {
                                File.Delete(outputName);
                            }
                            Environment.Exit(0);
                        }

                        Console.WriteLine($"Success, marker => '{marker.Pattern.Text}' found at => '{marker.Position}' away from the previous.");
                    }
                }
            }
        }
    }
}
