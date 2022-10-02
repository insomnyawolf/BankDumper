using System.IO;

namespace BinaryFileTools
{
    public class PatternMatch : JsonSerializable
    {
        // Prevent instancing that class out of this library
        private PatternSearch PatternSearch;
        internal PatternMatch(PatternSearch PatternSearch) 
        {
            this.PatternSearch = PatternSearch;
        }

        public BasePattern Pattern { get; internal set; }
        public long StartOffset { get => StartOffsetWithoutPattern - Pattern.Length; }
        public long EndOffset { get; internal set; }
        public long PatternEndOffset { get => StartOffsetWithoutPattern - 1; }
        public long StartOffsetWithoutPattern { get; internal set; }

        public long Length
        {
            get
            {
                var length = EndOffset - StartOffset;

                // +1 because 0 is a valid adress
                if (StartOffset == 0)
                {
                    return length + 1;
                }
                return length;
            }
        }

        public long LengthWithoutPattern { get => Length - Pattern.Length; }

        /// <summary>
        /// Replace bytes on the pattern provided (includes the pattern on the replaced bytes)
        /// </summary>
        /// <param name="PatternMatch"></param>
        /// <param name="newBytes"></param>
        public void ReplaceAtPattern(Stream newBytes)
        {
            var writer = Pattern.GetReader(newBytes);
            PatternSearch.ReplaceAtPosition(StartOffset, writer);
        }

        /// <summary>
        /// Replace bytes after the pattern provided
        /// </summary>
        /// <param name="PatternMatch"></param>
        /// <param name="newBytes"></param>
        public void ReplaceAfterPattern(Stream newBytes)
        {
            var writer = Pattern.GetReader(newBytes);
            PatternSearch.ReplaceAtPosition(StartOffsetWithoutPattern, writer);
        }

        /// <summary>
        /// Copy the section defined by PatternMatch into the stream provided (includes the pattern)
        /// </summary>
        /// <param name="PatternMatch"></param>
        /// <param name="target"></param>
        public void ExtractWithPatternTo(Stream target)
        {
            var reader = Pattern.GetReader(target);
            PatternSearch.ExtractBytesTo(StartOffset, (int)Length, reader);
        }

        /// <summary>
        /// Copy the section defined by PatternMatch into the stream provided (does not include the pattern)
        /// </summary>
        /// <param name="PatternMatch"></param>
        /// <param name="target"></param>
        public void ExtractWithoutPatternTo(Stream target)
        {
            var reader = Pattern.GetReader(target);
            PatternSearch.ExtractBytesTo(StartOffsetWithoutPattern, (int)LengthWithoutPattern, reader);
        }
    }
}
