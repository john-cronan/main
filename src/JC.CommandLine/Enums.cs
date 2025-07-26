using System;

namespace JC.CommandLine
{
    public enum ArgumentMultiplicity
    {
        Zero,
        ZeroOrMore,
        One,
        OneOrMore        
    }

    internal enum NameMatchingOptions
    {
        Exact,
        Stem
    }

    [Flags]
    public enum ArgumentFlags
    {
        None = 0,
        ExistingFile = 1,
        ExistingDirectory = 2,
        ReadFileContent = 4,
        AssumeHexadecimal = 8,
        AssumeBase64 = 16
    }
}
