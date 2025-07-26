using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

namespace JC.CommandLine
{
    [DebuggerDisplay("{KeyNode.Text}, {ValueNodes.Length} value(s)")]
    internal class CommandLineNodeGroup
    {
        public static IEnumerable<CommandLineNodeGroup> Parse(
            IEnumerable<CommandLineNode> nodes)
        {
            Guard.IsNotNull(nodes, nameof(nodes));

            if (!nodes.Any())
            {
                yield break;
            }
            CommandLineNode currentArgumentNameNode = null;
            var currentValueNodes = new List<CommandLineNode>();
            int nodeCount = 0;
            foreach (var node in nodes)
            {
                switch (node.NodeType)
                {
                    case CommandLineNodeTypes.Exe:
                        if (nodeCount != 0)
                        {
                            throw new CommandLineParseException("The exe name, if included, must be first");
                        }
                        currentArgumentNameNode = node;
                        nodeCount++;
                        break;
                    case CommandLineNodeTypes.ArgumentName:
                        if (currentArgumentNameNode != null)
                        {
                            yield return new CommandLineNodeGroup(currentArgumentNameNode,
                                currentValueNodes.ToImmutableArray());
                        }
                        else if (currentValueNodes.Any())
                        {
                            yield return new CommandLineNodeGroup(
                                new CommandLineNode(CommandLineNodeTypes.ArgumentName,
                                    Constants.UnnamedValuesNode), 
                                    currentValueNodes.ToImmutableArray());
                        }
                        currentArgumentNameNode = node;
                        nodeCount++;
                        currentValueNodes.Clear();
                        break;
                    case CommandLineNodeTypes.Value:
                    default:
                        currentValueNodes.Add(node);
                        nodeCount++;
                        break;
                }
            }
            if (currentArgumentNameNode != null)
            {
                yield return new CommandLineNodeGroup(currentArgumentNameNode,
                    currentValueNodes.ToImmutableArray());
            }
            else if (currentValueNodes.Any())
            {
                var unnamedNode = new CommandLineNode(CommandLineNodeTypes.ArgumentName,
                    Constants.UnnamedValuesNode);
                yield return new CommandLineNodeGroup(unnamedNode,
                    currentValueNodes.ToImmutableArray());
            }
        }

        public static IEnumerable<CommandLineNode> GetLeadingUnnamedValues(
            IEnumerable<CommandLineNodeGroup> nodeGroups)
        {
            //
            //  "Leading Unnamed Values" are defined as ones *after* the application path
            //  and *before* any named arguments.
            var leading =
                nodeGroups
                    .SkipWhile(a => a.IsExeNodeGroup)
                    .TakeWhile(a => a.IsUnnamedValuesNodeGroup)
                    .SelectMany(n => n.ValueNodes);
            return leading;
        }

        public static IEnumerable<CommandLineNode> GetTrailingUnnamedValues(
            IEnumerable<CommandLineNodeGroup> nodeGroups)
        {
            var trailing =
                nodeGroups
                    .Reverse()
                    .TakeWhile(n => n.IsUnnamedValuesNodeGroup)
                    .Reverse()
                    .SelectMany(n => n.ValueNodes);
            return trailing;
        }

        private readonly CommandLineNode _keyNode;
        private readonly ImmutableArray<CommandLineNode> _valueNodes;

        public CommandLineNodeGroup(CommandLineNode keyNode,
            IEnumerable<CommandLineNode> valueNodes)
        {
            Guard.IsNotNull(keyNode, nameof(keyNode));
            Guard.IsNotNull(valueNodes, nameof(valueNodes));

            _keyNode = keyNode;
            _valueNodes = valueNodes.ToImmutableArray();
        }


        public (CommandLineNodeGroup, CommandLineNodeGroup) Split(
            CommandLineNodeTypes rightKeyNodeType, string rightKeyNodeText,
            Func<IEnumerable<CommandLineNode>, IEnumerable<CommandLineNode>> leftValueNodeSelector,
            Func<IEnumerable<CommandLineNode>, IEnumerable<CommandLineNode>> rightValueNodeSelector
            )
        {
            Guard.IsNotNull(leftValueNodeSelector, nameof(leftValueNodeSelector));
            Guard.IsNotNull(rightValueNodeSelector, nameof(rightValueNodeSelector));

            var leftKeyNode = _keyNode;
            var leftValueNodes = leftValueNodeSelector(_valueNodes);
            var rightKeyNode = new CommandLineNode(rightKeyNodeType, rightKeyNodeText);
            var rightValueNodes = rightValueNodeSelector(_valueNodes);
            var left = new CommandLineNodeGroup(leftKeyNode, leftValueNodes);
            var right = new CommandLineNodeGroup(rightKeyNode, rightValueNodes);
            return (left, right);
        }

        public CommandLineNodeGroup Join(
            params CommandLineNodeGroup[] others)
        {
            Guard.IsNotNull(others, nameof(others));

            if (others.Length == 0)
            {
                return this;
            }
            var newValueNodes = _valueNodes.Concat(others.SelectMany(o => o.ValueNodes)).ToImmutableArray();
            return new CommandLineNodeGroup(_keyNode, newValueNodes);
        }

        public bool HasValues => _valueNodes.Any();

        public bool IsExeNodeGroup => _keyNode.NodeType == CommandLineNodeTypes.Exe;

        public bool IsUnnamedValuesNodeGroup => _keyNode.NodeType == CommandLineNodeTypes.ArgumentName && _keyNode.Text.Equals(Constants.UnnamedValuesNode);

        public CommandLineNode KeyNode => _keyNode;

        public ImmutableArray<CommandLineNode> ValueNodes => _valueNodes;
    }
}
