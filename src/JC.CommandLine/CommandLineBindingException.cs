using System;

namespace JC.CommandLine
{
    public class CommandLineBindingException : Exception
    {
        public CommandLineBindingException(string message)
            : base(message)
        {
        }
    }
}
