using System.Collections.Generic;

namespace SpleeterSharp
{
    public class SpleeterResult
    {
        public int ExitCode { get; set; }
        public string Output { get; set; }

        public int FileWrittenCount { get; set; }

        public int ErrorCount { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
}
