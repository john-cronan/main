using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace JC.CommandLine
{
    internal static class ParsingFixups
    {
        public static ImmutableArray<CommandLineNodeGroup> SplitExeNode(
            ImmutableArray<CommandLineNodeGroup> nodeGroups)
        {
            if (!nodeGroups.Any())
            {
                return nodeGroups;
            }
            var firstNodeGroup = nodeGroups[0];
            if (firstNodeGroup.IsExeNodeGroup && firstNodeGroup.HasValues)
            {
                var newCollection = new List<CommandLineNodeGroup>();
                (var newExeNodeGroup, var newUnnamedValuesNodeGroup)
                    = firstNodeGroup.Split(CommandLineNodeTypes.ArgumentName, 
                    Constants.UnnamedValuesNode, v => CommandLineNode.EmptyArray, 
                    v => v);
                newCollection.Add(newExeNodeGroup);
                newCollection.Add(newUnnamedValuesNodeGroup);
                newCollection.AddRange(nodeGroups.Skip(1));
                return newCollection.ToImmutableArray();
            }
            else
            {
                return nodeGroups;
            }
        }

        public static ImmutableArray<CommandLineNodeGroup> SplitEndingUnnamedValues(
            ImmutableArray<CommandLineNodeGroup> nodeGroups,
            ImmutableArray<Argument> arguments, StringComparison stringComparisons)
        {
            if (!nodeGroups.Any())
            {
                return nodeGroups;
            }
            if (!arguments.Any())
            {
                return nodeGroups;
            }

            var lastNodeGroup = nodeGroups[nodeGroups.Length - 1];
            if (!lastNodeGroup.HasValues)
            {
                return nodeGroups;
            }
            var matchingArguments =
                from argument in arguments
                where argument.Names.Any(n => n.Equals(lastNodeGroup.KeyNode.Text, stringComparisons))
                select argument;

            var matchingArgument = matchingArguments.SingleOrDefault();
            if (matchingArgument == null)
            {
                return nodeGroups;
            }

            if (matchingArgument.Multiplicity == ArgumentMultiplicity.One)
            {
                return LastNodeGroupTakesSingleValue(nodeGroups);
            }
            if (matchingArgument.Multiplicity == ArgumentMultiplicity.Zero)
            {
                return LastNodeGroupTakesNoValues(nodeGroups);
            }
            return nodeGroups;
        }

        public static ImmutableArray<CommandLineNodeGroup> ConsolidateDuplicateArguments(
                        ImmutableArray<CommandLineNodeGroup> nodeGroups,
                        ImmutableArray<Argument> arguments, StringComparison stringComparisons,
                        NameMatchingOptions nameMatching)
        {
            var output = new List<CommandLineNodeGroup>();
            var canonicalNames = nodeGroups
                .Where(ng => !ng.IsExeNodeGroup)
                .Where(ng => !ng.IsUnnamedValuesNodeGroup)
                .ToDictionary(
                    ng => ng,
                    ng => GetCanonicalName(ng.KeyNode.Text, arguments, stringComparisons, nameMatching));
            //var nodeGroupsMutable = nodeGroups.Where(ng => !ng.IsUnnamedValuesNodeGroup).ToList();
            var nodeGroupsMutable = nodeGroups.ToList();
            var i = 0;
            while (i < nodeGroupsMutable.Count)
            {
                var currentNodeGroup = nodeGroupsMutable[i];
                if (currentNodeGroup.KeyNode.NodeType == CommandLineNodeTypes.ArgumentName
                    && !currentNodeGroup.IsExeNodeGroup
                    && !currentNodeGroup.IsUnnamedValuesNodeGroup)
                {
                    //
                    //  Node group is "eligible" for consolidation. Read forward
                    //  and find all nodeGroups with the same canonical name. Join
                    //  them all and output.
                    var identical = nodeGroupsMutable
                        .Where((ng, n) => 
                            n > i 
                            && canonicalNames.ContainsKey(ng)
                            && canonicalNames[ng].Equals(canonicalNames[currentNodeGroup], stringComparisons))
                        .ToArray();
                    var joined = currentNodeGroup.Join(identical);
                    output.Add(joined);

                    //
                    //  Remove everything that's been joined from nodeGroupsMutable.
                    nodeGroupsMutable.Remove(currentNodeGroup);
                    foreach (var item in identical)
                    {
                        nodeGroupsMutable.Remove(item);
                    }

                    //
                    //  Don't advance.
                }
                else
                {
                    //
                    //  Node group is not eligible; simply output it.
                    output.Add(nodeGroupsMutable[i]);

                    //
                    //  Advance.
                    i++;
                }
            }
            return output.ToImmutableArray();
        }

        private static string GetCanonicalName(string actualName, 
            ImmutableArray<Argument> arguments, StringComparison stringComparisons,
            NameMatchingOptions nameMatching)
        {
            var firstMatch = arguments.FirstOrDefault(mo =>
                    NameMatching.IsMatch(actualName, mo.Names, nameMatching, stringComparisons));
            return firstMatch == null ? actualName : firstMatch.Names.First();
        }

        private static ImmutableArray<CommandLineNodeGroup> LastNodeGroupTakesSingleValue(
            ImmutableArray<CommandLineNodeGroup> nodeGroups)
        {
            if (!nodeGroups.Any())
            {
                throw new ArgumentException("Expected: At least one node group", nameof(nodeGroups));
            }

            var lastNodeGroup = nodeGroups.Last();
            if (!lastNodeGroup.HasValues)
            {
                return nodeGroups;
            }
            var newNodeGroups = new List<CommandLineNodeGroup>();
            newNodeGroups.AddRange(nodeGroups.Take(nodeGroups.Length - 1));
            (var singleValueGroup, var newUnnamedValuesGroup) =
                lastNodeGroup.Split(CommandLineNodeTypes.ArgumentName,
                    Constants.UnnamedValuesNode, v => v.Take(1), v => v.Skip(1));
            newNodeGroups.Add(singleValueGroup);
            if (newUnnamedValuesGroup.ValueNodes.Any())
            {
                newNodeGroups.Add(newUnnamedValuesGroup);
            }
            return newNodeGroups.ToImmutableArray();
        }

        private static ImmutableArray<CommandLineNodeGroup> LastNodeGroupTakesNoValues(
            ImmutableArray<CommandLineNodeGroup> nodeGroups)
        {
            if (!nodeGroups.Any())
            {
                throw new ArgumentException("Expected: At least one node group", nameof(nodeGroups));
            }

            var newNodeGroups = nodeGroups.Take(nodeGroups.Length - 1).ToList();            
            var lastNodeGroup = nodeGroups.Last();
            if (!lastNodeGroup.HasValues)
            {
                return nodeGroups;
            }
            (var noValuesGroup, var unnamedValuesGroup) =
                lastNodeGroup.Split(CommandLineNodeTypes.ArgumentName,
                    Constants.UnnamedValuesNode, v => CommandLineNode.EmptyArray, 
                    v => v);
            newNodeGroups.Add(noValuesGroup);
            newNodeGroups.Add(unnamedValuesGroup);
            return newNodeGroups.ToImmutableArray();
        }
    }
}
