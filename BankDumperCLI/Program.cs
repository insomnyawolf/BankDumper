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
                return;
            }

            if (!File.Exists(args[0]))
            {
                Console.WriteLine("Input file doesn't exist.");
                return;
            }

            var force = args.Length == 3 && args[2] == "--force";
            if (!force && File.Exists(args[1]))
            {
                Console.WriteLine("Output file already exist, add '--force' to overwrite.");
                return;
            }

            var input = File.Open(args[0], FileMode.Open, FileAccess.Read, FileShare.Read);
            var output = File.Open(args[1], FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);

            if (BankDumper.ExtractFile(input, output))
            {
                output.SetLength(output.Position + 1);
                Console.WriteLine("Success");
            }
        }
    }
}
