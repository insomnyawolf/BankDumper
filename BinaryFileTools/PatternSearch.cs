using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace BinaryFileTools
{
    public partial class PatternSearch : JsonSerializable
    {
        private readonly List<BasePattern> PatternsLoaded = new List<BasePattern>();
        private int LargestPattern;
        private int ShortestPattern;

        public Stream Stream { get; private set; }

        public PatternSearch(Stream? Stream = null, List<BasePattern>? PatternsLoaded = null)
        {
            this.Stream = Stream;

            if (PatternsLoaded != null)
            {
                foreach (var pattern in PatternsLoaded)
                {
                    TryAddPattern(pattern);
                }
            }
        }

        public void SetStream(Stream Stream)
        {
            this.Stream = Stream;
        }

        private void UpdateCache()
        {
            LargestPattern = 0;
            ShortestPattern = int.MaxValue;

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

            // Longer patterns should take priority over shorter patterns
            SortHelper.SelectionSort(PatternsLoaded, (a, b) => a.Length > b.Length);
        }

        public bool TryAddPattern(BasePattern pattern)
        {
            if (pattern == null)
            {
                return false;
            }

            if (pattern.Bytes.Length < 1)
            {
                return false;
            }

            foreach (var patternload in PatternsLoaded)
            {
                if (patternload.Length != pattern.Length)
                {
                    continue;
                }

                if (EndsWithPattern(pattern.Bytes, patternload.Bytes))
                {
                    return false;
                }                
            }

            PatternsLoaded.Add(pattern);

            UpdateCache();
            // Added!
            return true;
        }

        public bool TryRemovePattern(string name)
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

        public List<PatternMatch> Analyze()
        {
            var searchBuffer = new byte[LargestPattern];
            var searchBufferLastPosition = searchBuffer.Length - 1;

            var patternMatches = new List<PatternMatch>();

            PatternMatch? currentPatternMatch = null;

            // Input Loop
            int currentByte;
#warning Reading it like that will kill the performance, maybe needs to be updated
            // -1 means that there are no more bytes available
            // Wait why does readbyte returns int???
            while ((currentByte = Stream.ReadByte()) != -1)
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
                    currentPatternMatch.EndOffset = Stream.Position - pattern.Bytes.LongLength - 1;
                    patternMatches.Add(currentPatternMatch);
                }

                // create a item with the data for the currently detected pattern
                currentPatternMatch = new PatternMatch(this)
                {
                    Pattern = pattern,
                    StartOffsetWithoutPattern = Stream.Position,
                };
            }

            // end of file fix for the last pattern match
            if (currentPatternMatch != null)
            {
                currentPatternMatch.EndOffset = Stream.Position - 1;
                patternMatches.Add(currentPatternMatch);
            }

            return patternMatches;
        }

        /// <summary>
        /// Calls EndsWithPattern on eacha available pattern on MagicNumbers list
        /// </summary>
        /// <param name="value"></param>
        /// <returns>Pattern matched or null</returns>
        private BasePattern? TryFindPattern(byte[] value)
        {
            for (int patternIndex = 0; patternIndex < PatternsLoaded.Count; patternIndex++)
            {
                var current = PatternsLoaded[patternIndex];
                if (EndsWithPattern(value, current.Bytes))
                {
                    return current;
                }
            }
            return null;
        }

        private bool EndsWithPattern(byte[] value, byte[] pattern)
        {
            // value has to have the same length or more than the pattern always 

            var sliceIndex = value.Length;
            var patternIndex = pattern.Length;

            // we check once per pattern byte
            while (patternIndex > 0)
            {
                sliceIndex--;
                patternIndex--;

                var sliceValue = value[sliceIndex];
                var patternValue = pattern[patternIndex];
                var temp = sliceValue != patternValue;
                if (temp)
                {
                    return false;
                }
            }

            return true;
        }

        // I'm using this to prevent multithreading issues (just in case)
        private EventWaitHandle EventWaitHandle = new EventWaitHandle(true, EventResetMode.ManualReset);

        /// <summary>
        /// Replace bytes at the offset provided
        /// </summary>
        /// <param name="PatternMatch"></param>
        /// <param name="newBytes"></param>
        public void ReplaceAtPosition(long offset, Stream newBytes)
        {
            EventWaitHandle.WaitOne();

            Stream.Position = offset;

            newBytes.CopyTo(Stream);

            EventWaitHandle.Set();
        }

        /// <summary>
        /// Copy the defined section of the stream into the stream provided
        /// </summary>
        /// <param name="PatternMatch"></param>
        /// <param name="target"></param>
        public void ExtractBytesTo(long offset, int length, Stream target)
        {
            EventWaitHandle.WaitOne();

            Stream.Position = offset;

            Stream.CopyTo(target, length);

            EventWaitHandle.Set();
        }
    }
}
