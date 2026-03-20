using System.Collections.Generic;

namespace JC.CommandLine
{
    /// <summary>
    /// Represents the result of parsing a command line.
    /// </summary>
    public interface ICommandLineParseResults
    {
        /// <summary>
        /// Creates an instance of the specified type to receive the values
        /// parsed from the command line, in strongly-typed form. What style
        /// of binding (e.g. property or constructor) is used is configured
        /// by the <see cref="CommandLineParserBuilder"/> that created
        /// the parser.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Object binding is by convention. Property names (in the case of
        /// property binding) or constructor parameter names (in the case of
        /// constructor binding) receive values of arguments with matching 
        /// names.
        /// </para>
        /// <para>
        /// If argument or switch names have hyphens, underscores, or
        /// colons, they match member names with those characters removed.
        /// </para>
        /// <para>
        /// The matching of argument names with member names is always 
        /// case-insensitive.
        /// </para>
        /// </remarks>
        T Bind<T>();

        /// <summary>
        /// Returns the value of the specified parsed argument, as a string.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the specified argument has more than one value, the first value
        /// is returned.
        /// </para>
        /// </remarks>
        string GetValue(string argumentName);

        /// <summary>
        /// Returns the value of the specified parsed argument, converted to
        /// the specified type.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the specified argument has more than one value, the first value
        /// is returned.
        /// </para>
        /// </remarks>
        T GetValueAs<T>(string argumentName);

        /// <summary>
        /// Returns all values associated with the specified parsed argument,
        /// as a sequence of strings.
        /// </summary>
        IEnumerable<string> GetValues(string argumentName);

        /// <summary>
        /// Returns all values associated with the specified parsed argument,
        /// converted to a sequence of objects of the specified type.
        /// </summary>
        IEnumerable<T> GetValuesAs<T>(string argumentName);

        /// <summary>
        /// Returns true if the specified argument or switch was present on 
        /// the parsed command line.
        /// </summary>
        bool IsPresent(string argumentName);

        /// <summary>
        /// Gets a sequence of the parsed command line's leading unnamed
        /// values, if any.
        /// </summary>
        IEnumerable<string> LeadingUnnamedValues { get; }

        /// <summary>
        /// Gets a collection of warnings that occurred during parsing and
        /// binding.
        /// </summary>
        CommandLineParseException ParseWarnings { get; }

        /// <summary>
        /// Gets a sequence of the parsed command line's trailing unnamed
        /// values, if any.
        /// </summary>
        IEnumerable<string> TrailingUnnamedValues { get; }

        /// <summary>
        /// Gets a sequence of all the parsed command line's unnamed values, 
        /// if any.
        /// </summary>
        IEnumerable<string> UnnamedValues { get; }
    }
}
