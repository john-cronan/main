using System;
using System.Collections.Generic;

namespace JC.CommandLine
{
    /// <summary>
    /// An abstract type representing the behavior of parsing a command line.
    /// </summary>
    public interface ICommandLineParser
    {
        /// <summary>
        /// Parses the command line returned by <see cref="Environment.GetCommandLineArgs()"/>
        /// and returns true if a help switch is both defined and present on the actual
        /// command line, without applying full validation (i.e. conditions that usually 
        /// throw exceptions will not).
        /// </summary>
        bool IsHelpSwitchPresent();

        /// <summary>
        /// Parses the specified command line and returns true if a help switch is both 
        /// defined and present on the actual command line, without applying full validation
        /// (i.e. conditions that usually throw exceptions will not).
        /// </summary>
        bool IsHelpSwitchPresent(IEnumerable<string> arguments);

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
        /// Parses the command line returned by 
        /// <see cref="Environment.GetCommandLineArgs()"/> and returns the 
        /// leading unnamed values portion (which, depending on your command
        /// line, may identify a command), without applying full validation 
        /// (i.e. conditions that usually throw exceptions will not).
        /// </summary>
        IEnumerable<string> ParseLeadingUnnamedValues();

        /// <summary>
        /// Parses the specified command line and returns the leading unnamed 
        /// values portion (which, depending on your command line, may identify 
        /// a command), without applying full validation (i.e. conditions that 
        /// usually throw exceptions will not).
        /// </summary>
        IEnumerable<string> ParseLeadingUnnamedValues(IEnumerable<string> arguments);

        /// <summary>
        /// Gets a collection of characters that will be used to delimit 
        /// argument names.
        /// </summary>
        IEnumerable<char> ArgumentDelimitters { get; }

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
