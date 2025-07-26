using System.Collections.Generic;
using System.Collections.Immutable;

namespace JC.CommandLine
{
    public interface ICommandLineParser
    {
        ICommandLineParseResults Parse();
        ICommandLineParseResults Parse(IEnumerable<string> arguments);

        ImmutableArray<char> ArgumentDelimitters { get; }
        bool CaseSensitive { get; }
    }
}
