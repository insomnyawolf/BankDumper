using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BankDumperLib
{
    public class MagicNumbers
    {
        public string Name { get; private set; }
        public byte[] Value { get; private set; }

        public MagicNumbers(string value)
        {
            Name = value;
            Value = Encoding.ASCII.GetBytes(value);
        }
    }

    // Take care for pattern at position 0

    public static class BankDumper
    {
        public static readonly List<MagicNumbers> MagicNumbers = new List<MagicNumbers>()
        {
            // new byte[] { 70, 83, 66, 53 }
            new MagicNumbers("FSB5"),
            // new byte[] { 66, 75, 72, 68 }
            new MagicNumbers("BKHD"),
            // new byte[] { 65, 75, 80, 75 }
            new MagicNumbers("AKPK"),
        };

        private static readonly int LargestPattern = 0;
        private static readonly int ShortestPattern = int.MaxValue;
        static BankDumper()
        {
            // Initialize data that will be helpful later
            for (int MagicNumberIndex = 0; MagicNumberIndex < MagicNumbers.Count; MagicNumberIndex++)
            {
                var current = MagicNumbers[MagicNumberIndex];

                if (LargestPattern < current.Value.Length)
                {
                    LargestPattern = current.Value.Length;
                }

                if (ShortestPattern > current.Value.Length)
                {
                    ShortestPattern = current.Value.Length;
                }
            }
        }

        private static bool TestAllPatterns(byte[] value)
        {
            for (int MagicNumberIndex = 0; MagicNumberIndex < MagicNumbers.Count; MagicNumberIndex++)
            {
                if (TestPattern(value, MagicNumbers[MagicNumberIndex].Value))
                {
                    Console.WriteLine($"Detected => {MagicNumbers[MagicNumberIndex].Name}");
                    return true;
                }
            }
            return false;
        }

        private static bool TestPattern(byte[] value, byte[]pattern)
        {
#warning maybe optimize this
            if (pattern.Length > value.Length)
            {
                return false;
            }

            for (int i = pattern.Length - 1; i > -1; i--)
            {
                if (value[i] != pattern[i])
                {
                    return false;
                }
            }
            return true;
        }

        public static bool ExtractFile(Stream input, Stream output)
        {
            var searchBuffer = new byte[LargestPattern];

            // Input Loop
            int bytesRead;
            while ((bytesRead = input.Read(searchBuffer, 0, searchBuffer.Length)) > 0)
            {
                if (bytesRead < ShortestPattern)
                {
                    // No possible pattern match anymore, exit early
                    break;
                }

#warning need case for different sized patterns at the beggining of the file

                if (TestAllPatterns(searchBuffer))
                {
                    // Pattern dettected

                    // That should copy from the current position to the end of the stream
                    input.CopyTo(output);
                    return true;
                }
            }

            Console.WriteLine($"File didn't match any pattern.");
            return false;
        }
    }
}
