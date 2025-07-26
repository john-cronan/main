using System.Collections.Generic;

namespace JC.CommandLine.IntegrationTests.SimpleParse
{
    internal class CommandLine
    {
        public IEnumerable<string> LeadingUnnamedValues { get; set; }
        public IEnumerable<string> Files { get; set; }
        public IList<int> Ids { get; set; }
        public bool Strict { get; set; }
    }
}
