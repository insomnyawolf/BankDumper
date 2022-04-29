using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BankDumperLib
{
    public class MagicNumbers
    {
        public string Name { get; }
        public byte[] Value { get; }

        public MagicNumbers(string value)
        {
            Name = value;
            Value = Encoding.ASCII.GetBytes(value);
        }
    }

    public class PatternFind
    {
        public MagicNumbers Pattern { get; internal set; }
        public long Position { get; internal set; }
    }

    // Take care for pattern at position 0

    public static class BankDumper
    {
        public static readonly List<MagicNumbers> MagicNumbers = new List<MagicNumbers>()
        {
            new MagicNumbers("FSB5"),
            new MagicNumbers("BKHD"),
            new MagicNumbers("AKPK"),
        };

        private static readonly int LargestPattern = 0;
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
            }
        }

        private static MagicNumbers? TestAllPatterns(byte[] value)
        {
            for (int MagicNumberIndex = 0; MagicNumberIndex < MagicNumbers.Count; MagicNumberIndex++)
            {
                var current = MagicNumbers[MagicNumberIndex];
                if (TestPattern(value, current.Value))
                {
                    Console.WriteLine($"Detected => {current.Name}");
                    return current;
                }
            }
            return null;
        }

        private static bool TestPattern(byte[] value, byte[] pattern)
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

        public static PatternFind? Extract(Stream input, Stream output)
        {
            var searchBuffer = new byte[LargestPattern];
            var searchBufferLastPosition = searchBuffer.Length - 1;

            // Input Loop
            int currentByte;
#warning Reading it like that will kill the performance, needs to be updated
            // -1 means that there are no more bytes available
            // Wait why does readbyte returns int???
            while ((currentByte = input.ReadByte()) != -1)
            {
                searchBuffer[searchBufferLastPosition] = (byte)currentByte;

                var pattern = TestAllPatterns(searchBuffer);
                if (pattern != null)
                {
                    // Pattern dettected

                    // Save useful Data
                    var data = new PatternFind()
                    {
                        Pattern = pattern,
                        Position = input.Position - pattern.Value.Length,
                    };

                    // That should copy from the current position to the end of the stream
                    input.CopyTo(output);
                    return data;
                }

                // Shift Bytes
                for (int i = 0; i < searchBufferLastPosition; i++)
                {
                    searchBuffer[i] = searchBuffer[i + 1];
                }
            }

            Console.WriteLine($"File didn't match any pattern.");
            return null;
        }

        public static PatternFind? ExtractMultiple(Stream input, Stream output)
        {
            var found = Extract(input, output);

            if (found != null)
            {
#warning i don't know if that's supported on all kinds of streams
                input.SetLength(found.Position);
                // Shorten the stream so it only contains certain file
            }

            return found;
        }
    }
}
