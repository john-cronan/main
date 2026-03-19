using System;
using System.Collections.Generic;
using System.Linq;

namespace JC.CommandLine
{
    /// <summary>
    /// An exception thrown when an error occurs parsing a command line.
    /// </summary>
    public class CommandLineParseException : Exception
    {
        private readonly IEnumerable<Exception> _errors;

        /// <summary>
        /// Constructs a new instance with the specified error message.
        /// </summary>
        public CommandLineParseException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Constructs a new instance with the specified error message and 
        /// a collection of parse errors.
        /// </summary>
        public CommandLineParseException(string message, 
            IEnumerable<Exception> errors)
            :base(message)
        {
            Guard.IsNotNullOrEmpty(errors, nameof(errors));

            _errors = errors.ToArray();
        }

        /// <summary>
        /// Returns the collection of parse errors passed into the object's
        /// constructor.
        /// </summary>
        public IEnumerable<Exception> ParseErrors => _errors;
    }
}
