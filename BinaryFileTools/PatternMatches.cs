using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace BinaryFileTools
{
    public class PatternMatches : JsonSerializable
    {
        // I'm using this to prevent multithreading issues
        private EventWaitHandle EventWaitHandle = new EventWaitHandle(true, EventResetMode.ManualReset);
        internal Stream Stream { get; }
        public List<PatternMatch> Matches { get; } = new List<PatternMatch>();

        internal PatternMatches(Stream stream)
        {
            Stream = stream;
        }

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
        /// Replace bytes on the pattern provided (includes the pattern on the replaced bytes)
        /// </summary>
        /// <param name="PatternMatch"></param>
        /// <param name="newBytes"></param>
        public void ReplaceAtPattern(PatternMatch PatternMatch, Stream newBytes)
        {
            ReplaceAtPosition(PatternMatch.PositionStart, newBytes);
        }

        /// <summary>
        /// Replace bytes after the pattern provided
        /// </summary>
        /// <param name="PatternMatch"></param>
        /// <param name="newBytes"></param>
        public void ReplaceAfterPattern(PatternMatch PatternMatch, Stream newBytes)
        {
            ReplaceAtPosition(PatternMatch.PositionStartWithoutPattern, newBytes);
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

        /// <summary>
        /// Copy the section defined by PatternMatch into the stream provided (includes the pattern)
        /// </summary>
        /// <param name="PatternMatch"></param>
        /// <param name="target"></param>
        public void ExtractWithPatternTo(PatternMatch PatternMatch, Stream target)
        {
            ExtractBytesTo(PatternMatch.PositionStart, (int)PatternMatch.Length, target);
        }

        /// <summary>
        /// Copy the section defined by PatternMatch into the stream provided (does not include the pattern)
        /// </summary>
        /// <param name="PatternMatch"></param>
        /// <param name="target"></param>
        public void ExtractWithoutPatternTo(PatternMatch PatternMatch, Stream target)
        {
            ExtractBytesTo(PatternMatch.PositionStartWithoutPattern, (int)PatternMatch.LengthWithoutPattern, target);
        }

        public override string ToString()
        {
            return ToString(Matches);
        }
    }
}
