namespace BankDumper
{
    internal class Program
    {

        private static readonly byte[] contentStartPattern = new byte[] { 70, 83, 66, 53 };

        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine();
                Console.WriteLine(" " + AppDomain.CurrentDomain.FriendlyName + " <input bank> <output fsb>");
                return;
            }

            byte[] infile = File.ReadAllBytes(args[0]);

            // File format guess
            // Padding bytes => Filestart Marker => RealData => EoF
            for (int i = 0; i < infile.Length; i++)
            {
                // Theorically does the same without the type conversion
                var buffer = infile[i..(i + 3)];
                if (contentStartPattern.SequenceEqual(buffer))
                {
                    int newlen = infile.Length - i;
                    byte[] outfile = new byte[newlen];
                    Array.Copy(infile, i, outfile, 0, newlen);
                    File.WriteAllBytes(args[1], outfile);

                    // Exit early condition
                    return;
                }
            }
        }
    }
}
