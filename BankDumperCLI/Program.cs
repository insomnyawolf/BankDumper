using System.Runtime.InteropServices;
using System.Text.Json;

namespace BinaryFileTools
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // This is a example of loading magic numbers from a file
            using var patternsFile = File.Open(Path.Combine(AppContext.BaseDirectory, "patterns.json"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);

            char osMarker;
            string executableName = AppDomain.CurrentDomain.FriendlyName;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                osMarker = '>';
                executableName += ".exe";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                osMarker = '$';
            }
            else
            {
                throw new PlatformNotSupportedException();
            }

            // This fills the sample file if it's empty
            if (patternsFile.Length == 0)
            {
                var sample = new List<Pattern>()
                {
                    new Pattern("FSB5"),
                    new Pattern("BKHD"),
                    new Pattern("AKPK"),
                    new Pattern("FileSample", null, new PatternFile(executableName, 0, 6))
                };

                JsonSerializer.Serialize(patternsFile, sample);

                // Seek to the beggining of the file so it can be read
                patternsFile.Position = 0;
            }

            if (args.Length != 1)
            {
                Console.WriteLine();

                

                Console.WriteLine(osMarker + executableName + " <input>");
                Environment.Exit(1);
            }

            if (!File.Exists(args[0]))
            {
                Console.WriteLine("Input file doesn't exist.");
                Environment.Exit(1);
            }

            var patterns = JsonSerializer.Deserialize<List<Pattern>>(patternsFile);

            // Validations are never bad
            if (patterns != null)
            {
                foreach (var pattern in patterns)
                {
                    if (!FileTools.TryAddPattern(pattern))
                    {
                        Console.WriteLine($"Could not add => '{pattern.Name}', it already exists.");
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
