using System;

namespace JC.CommandLine
{
    /// <summary>
    /// An exception thrown when an error occurs during command line
    /// object binding.
    /// </summary>
    public class CommandLineBindingException : Exception
    {
        /// <summary>
        /// Constructs a new instance with the specified error message.
        /// </summary>
        public CommandLineBindingException(string message)
            : base(message)
        {
        }
    }
}
