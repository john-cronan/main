using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace JC.CommandLine
{
    /// <summary>
    /// An abstract type representing the behavior of parsing a command line.
    /// </summary>
    public interface ICommandLineParser
    {
        /// <summary>
        /// Parses the command line returned by invoking
        /// <see cref="Environment.GetCommandLineArgs()"/>
        /// </summary>
        /// <returns>
        /// An implementation of <see cref="ICommandLineParseResults"/>
        /// that can be queried for parse results, or bind parse results 
        /// to objects.
        /// </returns>
        ICommandLineParseResults Parse();

        /// <summary>
        /// Parses the specified command line arguments.
        /// </summary>
        /// <returns>
        /// An implementation of <see cref="ICommandLineParseResults"/>
        /// that can be queried for parse results, or bind parse results 
        /// to objects.
        /// </returns>
        ICommandLineParseResults Parse(IEnumerable<string> arguments);

        /// <summary>
        /// Gets a collection of characters that will be used to delimit 
        /// argument names.
        /// </summary>
        ImmutableArray<char> ArgumentDelimitters { get; }

        /// <summary>
        /// Gets a value indicating whether argument names are treated as
        /// case-sensitive.
        /// </summary>
        bool CaseSensitive { get; }

        /// <summary>
        /// Gets a value indicating what kind of name matching is perforrmed
        /// between defined argument names and actual command line arguments.
        /// </summary>
        NameMatchingOptions NameMatching { get; }
    }
}
