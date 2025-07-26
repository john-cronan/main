using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace JC.CommandLine.UnitTests
{
    internal class CommandLineBuilder
    {
        private readonly List<CommandLineNodeGroup> _NodeGroups;

        public CommandLineBuilder()
        {
            _NodeGroups = new List<CommandLineNodeGroup>();
        }

        public CommandLineBuilder AddExeNode(string path)
        {
            var newNode = new CommandLineNodeGroup
            (
                new CommandLineNode(CommandLineNodeTypes.Exe, path),
                CommandLineNode.EmptyArray
            );
            _NodeGroups.Add(newNode);
            return this;
        }

        public CommandLineBuilder AddExeNode(string path, params string[] arguments)
        {
            var newNode = new CommandLineNodeGroup
            (
                new CommandLineNode(CommandLineNodeTypes.Exe, path),
                arguments.Select(a => new CommandLineNode(CommandLineNodeTypes.ArgumentName, a))
            );
            _NodeGroups.Add(newNode);
            return this;
        }

        public CommandLineBuilder AddArgument(string name, 
            params string[] values)
        {
            var newNode = new CommandLineNodeGroup
            (
                new CommandLineNode(CommandLineNodeTypes.ArgumentName, name),
                values.Select(v => new CommandLineNode(CommandLineNodeTypes.ArgumentName, v))
            );
            _NodeGroups.Add(newNode);
            return this;
        }   


        public CommandLineBuilder AddUnnamedArgument(params string[] values)
        {
            return AddArgument(Constants.UnnamedValuesNode, values);
        }

        public ImmutableArray<CommandLineNodeGroup> GetCommandLine()
        {
            return _NodeGroups.ToImmutableArray();
        }
    }
}
