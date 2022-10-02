using BinaryFileTools;
using BinaryFileToolsPatterns;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace BankDumperCLI
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
                // allright, i'm just lazy today
                throw new PlatformNotSupportedException();
            }

            // This fills the sample file if it's empty
            if (patternsFile.Length == 0)
            {
                var sample = new List<PatternSettings>()
                {
                    new PatternSettings("FSB5"),
                    new PatternSettings("BKHD"),
                    new PatternSettings("AKPK"),
                    new PatternSettings("FileSample", null, new PatternFile(executableName, 0, 6))
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

            var patternSearch = new PatternSearch();

            patternSearch.AddPatternAKPK();
            patternSearch.AddPatternBKHD();
            patternSearch.AddPatternFSB5();

            var patterns = JsonSerializer.Deserialize<List<PatternSettings>>(patternsFile);

            // Validations are never bad
            // ???????????????????
            // Slice value evaluates as 0 for no apparent reason
            // Probably related to the custom converter (?)

            if (patterns != null)
            {
                foreach (var pattern in patterns)
                {
                    if (!patternSearch.TryAddPattern(pattern))
                    {
                        Console.WriteLine($"Could not add => '{pattern.Name}', it already exists.");
                    }
                }
            }

            using var input = File.Open(args[0], FileMode.Open, FileAccess.Read, FileShare.Read);

            patternSearch.SetStream(input);

            var result = patternSearch.Analyze();

            Console.WriteLine(result.ToString());

            foreach (var pattern in result)
            {
                // Do what you want here
            }
        }

        public void SimplifiedMain()
        {
            using var input = File.Open("SamplePath", FileMode.Open, FileAccess.Read, FileShare.Read);

            var patternSearch = new PatternSearch(input);

            patternSearch.AddPatternAKPK();
            patternSearch.AddPatternBKHD();
            patternSearch.AddPatternFSB5();

            var result = patternSearch.Analyze();

            Console.WriteLine(result.ToString());

            foreach (var pattern in result)
            {
                //pattern.
                // Do what you want here
            }
        }
    }
}
