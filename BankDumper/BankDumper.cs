using System;
using System.Collections.Generic;
using System.IO;

namespace BankDumperLib
{
    public static class FileTools
    {
        private static readonly List<Pattern> PatternsLoaded = new List<Pattern>();

        private static int LargestPattern = 0;
        private static int ShortestPattern = int.MaxValue;

        private static void UpdateCache()
        {
            // Initialize data that will be helpful later
            for (int MagicNumberIndex = 0; MagicNumberIndex < PatternsLoaded.Count; MagicNumberIndex++)
            {
                var current = PatternsLoaded[MagicNumberIndex];

                var currentLenght = current.Bytes.Length;

                if (LargestPattern < currentLenght)
                {
                    LargestPattern = currentLenght;
                }

                if (ShortestPattern > currentLenght)
                {
                    ShortestPattern = currentLenght;
                }
            }
        }

        public static void LoadDefaultMagicNumbers()
        {
            PatternsLoaded.Add(new Pattern("FSB5"));
            PatternsLoaded.Add(new Pattern("BKHD"));
            PatternsLoaded.Add(new Pattern("AKPK"));
            UpdateCache();
        }

        public static bool TryAddPattern(Pattern number)
        {
            var result = TryFindPattern(number.Bytes);
            if (result != null)
            {
                // Prevents adding the same pattern twice
                // This will improve performance exponentially if the user doesn't validate which data inputs here
                return false;
            }

            if (number.Bytes.Length < 1)
            {
                return false;
            }

            PatternsLoaded.Add(number);

            UpdateCache();
            // Added!
            return true;
        }

        public static bool TryRemoveMagicNumber(string name)
        {
            for (int i = 0; i < PatternsLoaded.Count; i++)
            {
                if (PatternsLoaded[i].Name == name)
                {
                    PatternsLoaded.RemoveAt(i);
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
        private static Pattern? TryFindPattern(byte[] value)
        {
            for (int MagicNumberIndex = 0; MagicNumberIndex < PatternsLoaded.Count; MagicNumberIndex++)
            {
                var current = PatternsLoaded[MagicNumberIndex];
                if (EndsWithPattern(value, current.Bytes))
                {
#if DEBUG
                    Console.WriteLine($"Detected => {current.Name}");
#endif
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

        public static PatternMatches Analyze(Stream input)
        {
            var searchBuffer = new byte[LargestPattern];
            var searchBufferLastPosition = searchBuffer.Length - 1;

#warning Can be optimized by reading as many bytes as the shortest pattern has in a single go, it won't help performance that much but it's something

            var patternMatches = new PatternMatches(input);

            PatternMatch? currentPatternMatch = null;

            // Input Loop
            int currentByte;
#warning Reading it like that will kill the performance, needs to be updated
            // -1 means that there are no more bytes available
            // Wait why does readbyte returns int???
            while ((currentByte = input.ReadByte()) != -1)
            {
                // Shift Bytes 
                for (int i = 0; i < searchBufferLastPosition; i++)
                {
                    searchBuffer[i] = searchBuffer[i + 1];
                }

                searchBuffer[searchBufferLastPosition] = (byte)currentByte;

                var pattern = TryFindPattern(searchBuffer);

                // no pattern found, skip to next loop iteration
                if (pattern == null)
                {
                    continue;
                }

                // Previously dettected pattern need to store it's ending position
                // then save it
                if (currentPatternMatch != null)
                {
                    currentPatternMatch.PositionEnd = input.Position - pattern.Bytes.LongLength;
                    patternMatches.Patterns.Add(currentPatternMatch);
                }

                // create a item with the data for the currently detected pattern
                currentPatternMatch = new PatternMatch()
                {
                    Pattern = pattern,
                    PositionStartWithoutNumber = input.Position,
                };
            }

            // end of file fix for the last pattern match
            if (currentPatternMatch != null)
            {
                currentPatternMatch.PositionEnd = input.Position - 1;
                patternMatches.Patterns.Add(currentPatternMatch);
            }

            return patternMatches;
        }
    }
}
