using System.Collections.Generic;
using System.IO;

namespace BinaryFileTools
{
    public static class BinaryTools
    {
        private static readonly List<Pattern> PatternsLoaded = new List<Pattern>();

        private static int LargestPattern = 0;
        private static int ShortestPattern = int.MaxValue;

        private static void UpdateCache()
        {
            // Initialize data that will be helpful later
            for (int PatternIndex = 0; PatternIndex < PatternsLoaded.Count; PatternIndex++)
            {
                var current = PatternsLoaded[PatternIndex];

                var currentLenght = current.Length;

                if (LargestPattern < currentLenght)
                {
                    LargestPattern = currentLenght;
                }

                if (ShortestPattern > currentLenght)
                {
                    ShortestPattern = currentLenght;
                }
            }

            var tempList = new List<Pattern>();

            Pattern tempPatternStorage = null;
            var currentOrderIndex = PatternsLoaded.Count;

            // Longer patterns should take priority over shorter patterns
            SortHelper.SelectionSort(PatternsLoaded, (a, b) => a.Length > b.Length);
        }

        // is this needed?
        public static void LoadDefaultPatterns()
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
                    return current;
                }
            }
            return null;
        }

        private static bool EndsWithPattern(byte[] value, byte[] pattern)
        {
            // value has to have the same length or more than the pattern always 

            var index = value.Length - 1;
            var patterChecksRemeaning = pattern.Length;

            // we check once per pattern byte
            while (patterChecksRemeaning > 0)
            {
                if (value[index] != pattern[index])
                {
                    return false;
                }

                index--;
                patterChecksRemeaning--;
            }

            return true;
        }

        public static PatternMatches Analyze(Stream input)
        {
            var searchBuffer = new byte[LargestPattern];
            var searchBufferLastPosition = searchBuffer.Length - 1;

            var patternMatches = new PatternMatches(input);

            PatternMatch? currentPatternMatch = null;

            // Input Loop
            int currentByte;
#warning Reading it like that will kill the performance, maybe needs to be updated
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
                    currentPatternMatch.PositionEnd = input.Position - pattern.Bytes.LongLength - 1;
                    patternMatches.Matches.Add(currentPatternMatch);
                }

                // create a item with the data for the currently detected pattern
                currentPatternMatch = new PatternMatch()
                {
                    Pattern = pattern,
                    PositionStartWithoutPattern = input.Position,
                };
            }

            // end of file fix for the last pattern match
            if (currentPatternMatch != null)
            {
                currentPatternMatch.PositionEnd = input.Position - 1;
                patternMatches.Matches.Add(currentPatternMatch);
            }

            return patternMatches;
        }
    }
}
