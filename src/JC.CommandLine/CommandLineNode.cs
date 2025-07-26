using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace JC.CommandLine
{
    internal enum CommandLineNodeTypes
    {
        Exe,
        ArgumentName,
        Value
    }

    [DebuggerDisplay("{NodeType}: {Text}")]
    internal class CommandLineNode
    {
        public static readonly IEnumerable<CommandLineNode> EmptyArray =
            new CommandLineNode[0];

        public static IEnumerable<CommandLineNode> Parse(
            IEnumerable<string> arguments, IEnumerable<char> argumentDelimitters)
        {
            Guard.IsNotNull(arguments, nameof(arguments));
            Guard.IsNotNullOrEmpty(argumentDelimitters, nameof(argumentDelimitters));

            var exe = Environment.GetCommandLineArgs().First();
            foreach (var arg in arguments)
            {
                if (arg.Equals(exe, StringComparison.InvariantCulture))
                {
                    yield return new CommandLineNode(CommandLineNodeTypes.Exe, arg);
                }
                else if (IsArgumentName(arg, argumentDelimitters))
                {
                    yield return new CommandLineNode(CommandLineNodeTypes.ArgumentName, arg.Substring(1));
                }
                else
                {
                    yield return new CommandLineNode(CommandLineNodeTypes.Value, arg);
                }
                
                // if (IsArgumentName(arg, argumentDelimitters))
                // {
                //     yield return new CommandLineNode(CommandLineNodeTypes.ArgumentName, arg.Substring(1));
                // }
                // else if (arg.Equals(exe, StringComparison.InvariantCulture))
                // {
                //     yield return new CommandLineNode(CommandLineNodeTypes.Exe, arg);
                // }
                // else
                // {
                //     yield return new CommandLineNode(CommandLineNodeTypes.Value, arg);
                // }
            }
        }

        private static bool IsArgumentName(string commandLineNode,
            IEnumerable<char> argumentDelimitters)
        {
            if (argumentDelimitters.Contains(commandLineNode[0]))
            {
                if (commandLineNode.Length > 1 && argumentDelimitters.Contains(commandLineNode[1]))
                {
                    return false;
                }
                return true;
            }
            return false;
        }


        private readonly CommandLineNodeTypes _nodeType;
        private readonly string _text;

        public CommandLineNode(CommandLineNodeTypes nodeType, string text)
        {
            _nodeType = nodeType;
            _text = text;
        }

        public string Text => _text;

        internal CommandLineNodeTypes NodeType => _nodeType;
    }
}
