using System.Collections.Generic;
using System.IO;
using System.Text.Json;
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

        public void ExtractWithPatternTo(PatternMatch PatternMatch, Stream target)
        {
            EventWaitHandle.WaitOne();

            Stream.Position = PatternMatch.PositionStart;

            Stream.CopyTo(target, (int)PatternMatch.Length);

            EventWaitHandle.Set();
        }

        public void ExtractWithoutPatternTo(PatternMatch PatternMatch, Stream target)
        {
            EventWaitHandle.WaitOne();

            Stream.Position = PatternMatch.PositionStartWithoutPattern;

            Stream.CopyTo(target, (int)PatternMatch.LengthWithoutPattern);

            EventWaitHandle.Set();
        }

        public override string ToString()
        {
            return ToString(Matches);
        }
    }
}
