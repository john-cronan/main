using System;

namespace JC.CommandLine
{
    /// <summary>
    /// Enumerates the possible constraints on an argument's number
    /// of associated values.
    /// </summary>
    public enum ArgumentMultiplicity
    {
        /// <summary>
        /// Specifies that an argument cannot have associated values. Such
        /// arguments are known as Switches.
        /// </summary>
        Zero,

        /// <summary>
        /// Specifies that an argument can optionally have any number of values.
        /// </summary>
        ZeroOrMore,

        /// <summary>
        /// Specifies that an argument can have exactly one value.
        /// </summary>
        One,

        /// <summary>
        /// Specifies that an argument must have at least one value, but can have
        /// any number of values.
        /// </summary>
        OneOrMore        
    }

    /// <summary>
    /// Enumerates the possible options for matching defined argument names to
    /// actual command line arguments.
    /// </summary>
    public enum NameMatchingOptions
    {
        /// <summary>
        /// Specifies exact name matching, in which case the enitre argument
        /// name must be specified on the the command line.
        /// </summary>
        Exact,

        /// <summary>
        /// Specifies stem matching, in which case only the beginning portion
        /// of an argument name must be specified on the command line. A
        /// parse exception occurs if the specified portion is ambiguous.
        /// </summary>
        Stem
    }

    /// <summary>
    /// Specifies one or more flags that may be applied to an argument.
    /// </summary>
    [Flags]
    public enum ArgumentFlags
    {
        None = 0,

        /// <summary>
        /// Specifies that the argument's value(s) are validated as file paths,
        /// which must identify existing files.
        /// </summary>
        ExistingFile = 1,

        /// <summary>
        /// Specifies that the argument's value(s) are validated as directory
        /// paths, which must identify existing directories.
        /// </summary>
        ExistingDirectory = 2,

        /// <summary>
        /// Specifies that the value of the argument is the content of an
        /// existing file.
        /// </summary>
        ReadFileContent = 4,

        /// <summary>
        /// Specifies that the argument is a binary value, expressed as a
        /// hexadecimal string.
        /// </summary>
        AssumeHexadecimal = 8,

        /// <summary>
        /// Specifies that the argument is a binary value, expressed as a
        /// base-64 encoded string.
        /// </summary>
        AssumeBase64 = 16
    }
}
