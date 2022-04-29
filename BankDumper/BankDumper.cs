using System;
using System.Collections.Generic;
using System.IO;

namespace BankDumperLib
{
    public static class BankDumper
    {
        private static readonly List<MagicNumber> MagicNumbers = new List<MagicNumber>();

        private static int LargestPattern = 0;

        private static void UpdateCache()
        {
            // Initialize data that will be helpful later
            for (int MagicNumberIndex = 0; MagicNumberIndex < MagicNumbers.Count; MagicNumberIndex++)
            {
                var current = MagicNumbers[MagicNumberIndex];

                if (LargestPattern < current.Bytes.Length)
                {
                    LargestPattern = current.Bytes.Length;
                }
            }
        }

        public static void LoadDefaultMagicNumbers()
        {
            MagicNumbers.Add(new MagicNumber("FSB5"));
            MagicNumbers.Add(new MagicNumber("BKHD"));
            MagicNumbers.Add(new MagicNumber("AKPK"));
            UpdateCache();
        }

        public static bool TryAddMagicNumber(MagicNumber number)
        {
            var result = TryFindPattern(number.Bytes);
            if (result != null)
            {
                // Prevents adding the same pattern twice
                // This will improve performance exponentially if the user doesn't validate which data inputs here
                return false;
            }

            MagicNumbers.Add(number);
            UpdateCache();
            // Added!
            return true;
        }

        public static bool TryRemoveMagicNumber(string name)
        {
            for (int i = 0; i < MagicNumbers.Count; i++)
            {
                if (MagicNumbers[i].Text == name)
                {
                    MagicNumbers.RemoveAt(i);
                    UpdateCache();
                    //Removed Sucessfully
                    return true;
                }
            }

            // Not Found
            return false;
        }

        /// <summary>
        /// Calls EndsWithPattern on eacha available pattern on MagicNumbers list
        /// </summary>
        /// <param name="value"></param>
        /// <returns>Pattern matched or null</returns>
        private static MagicNumber? TryFindPattern(byte[] value)
        {
            for (int MagicNumberIndex = 0; MagicNumberIndex < MagicNumbers.Count; MagicNumberIndex++)
            {
                var current = MagicNumbers[MagicNumberIndex];
                if (EndsWithPattern(value, current.Bytes))
                {
                    Console.WriteLine($"Detected => {current.Text}");
                    return current;
                }
            }
            return null;
        }

        private static bool EndsWithPattern(byte[] value, byte[] pattern)
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

        public static MagicNumberFound? Extract(Stream input, Stream output)
        {
            var searchBuffer = new byte[LargestPattern];
            var searchBufferLastPosition = searchBuffer.Length - 1;

#warning Can be optimized by reading as many bytes as the shortest pattern has in a single go, it won't help performance that much but it's something

            // Input Loop
            int currentByte;
#warning Reading it like that will kill the performance, needs to be updated
            // -1 means that there are no more bytes available
            // Wait why does readbyte returns int???
            while ((currentByte = input.ReadByte()) != -1)
            {
                searchBuffer[searchBufferLastPosition] = (byte)currentByte;

                var pattern = TryFindPattern(searchBuffer);
                if (pattern != null)
                {
                    // Pattern dettected

                    // Save useful Data before writing the rest of the file beacuse it will move the position
                    var data = new MagicNumberFound()
                    {
                        Pattern = pattern,
                        Position = input.Position - pattern.Bytes.Length,
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

            // No patterns were matched.
            return null;
        }

        /// <summary>
        /// Shorten the stream so it only contains the data needed and nothing after it
        /// </summary>
        /// <remarks>
        /// TAKE CARE, THIS ONE MODIFIES THE INPUT STREAM
        /// </remarks>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        public static MagicNumberFound? ExtractAndCut(Stream input, Stream output)
        {
            var found = Extract(input, output);

            if (found != null)
            {
#warning i don't know if that's supported on all kinds of streams
                input.SetLength(found.Position);
            }

            return found;
        }
    }
}
