using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace JC.CommandLine.UnitTests
{
    [TestClass]
    public class ParsingFixupsUnitTests
    {
        [TestMethod]
        public void Exe_group_is_transformed_into_unnamed_values_node()
        {
            var exeNodeIn = new CommandLineNodeGroup
            (
                new CommandLineNode(CommandLineNodeTypes.Exe, "program.exe"),
                new CommandLineNode[]
                {
                    new CommandLineNode(CommandLineNodeTypes.Value, "delete")
                }
             );
            var argsNodeIn = new CommandLineNodeGroup
            (
                new CommandLineNode(CommandLineNodeTypes.ArgumentName, "Files"),
                new CommandLineNode[]
                {
                    new CommandLineNode(CommandLineNodeTypes.Value, "FileA.txt"),
                    new CommandLineNode(CommandLineNodeTypes.Value, "FileB.txt")
                }
            );
            var input = new CommandLineNodeGroup[] { exeNodeIn, argsNodeIn }.ToImmutableArray();
            var output = ParsingFixups.SplitExeNode(input);
            var exeNodeGroupOut = output.Single(kn => kn.KeyNode.NodeType == CommandLineNodeTypes.Exe);
            Assert.IsFalse(exeNodeGroupOut.ValueNodes.Any());
            var unnamedGroupOut = output.Single(ng => ng.IsUnnamedValuesNodeGroup);
            Assert.AreEqual(1, unnamedGroupOut.ValueNodes.Count());
            var argsGroupOut = output.Single(ng =>
                ng.KeyNode.NodeType == CommandLineNodeTypes.ArgumentName
                && ng.KeyNode.Text == "Files");
            Assert.AreEqual(2, argsGroupOut.ValueNodes.Count());
        }

        [TestMethod]
        public void Exe_group_without_values_returns_equal_graph()
        {
            var exeNodeIn = new CommandLineNodeGroup
            (
                new CommandLineNode(CommandLineNodeTypes.Exe, "FixLottery.exe"),
                CommandLineNode.EmptyArray
             );
            var argsNodeIn = new CommandLineNodeGroup
            (
                new CommandLineNode(CommandLineNodeTypes.ArgumentName, "Files"),
                new CommandLineNode[]
                {
                    new CommandLineNode(CommandLineNodeTypes.Value, "FileA.txt"),
                    new CommandLineNode(CommandLineNodeTypes.Value, "FileB.txt")
                }
            );
            var input = new CommandLineNodeGroup[] { exeNodeIn, argsNodeIn }.ToImmutableArray();
            var output = ParsingFixups.SplitExeNode(input);
            Assert.AreEqual(input, output);
        }

        [TestMethod]
        public void Ending_values_are_not_unnamed()
        {
            Ending_values_are_not_unnamed_internal(ArgumentMultiplicity.OneOrMore);
            Ending_values_are_not_unnamed_internal(ArgumentMultiplicity.ZeroOrMore);
        }

        [TestMethod]
        public void Ending_values_are_changed_to_unnamed_node_Zero_multiplicity()
        {
            var inputNodes = new CommandLineNodeGroup[]
            {
                new CommandLineNodeGroup
                (
                    new CommandLineNode(CommandLineNodeTypes.Exe, "program.exe"),
                    CommandLineNode.EmptyArray
                ),
                new CommandLineNodeGroup
                (
                    new CommandLineNode(CommandLineNodeTypes.ArgumentName, "recurse"),
                    new CommandLineNode[]
                    {
                        new CommandLineNode(CommandLineNodeTypes.Value, "DirectoryA"),
                        new CommandLineNode(CommandLineNodeTypes.Value, "DirectoryB")
                    }
                )
            }.ToImmutableArray();
            var model = new Argument[]
            {
                new Argument(new string[] {"recurse" }.ToImmutableArray(),
                    ArgumentMultiplicity.Zero, false)
            }.ToImmutableArray();
            var outputNodes = ParsingFixups.SplitEndingUnnamedValues(inputNodes,
                model, StringComparison.InvariantCultureIgnoreCase);
            Assert.AreNotEqual(inputNodes.Length, outputNodes.Length);
            Assert.AreEqual(inputNodes[0].KeyNode.NodeType, outputNodes[0].KeyNode.NodeType);
            Assert.AreEqual(inputNodes[0].ValueNodes.Length, inputNodes[0].ValueNodes.Length);
            Assert.AreEqual(0, outputNodes[0].ValueNodes.Length);
            Assert.AreEqual(CommandLineNodeTypes.ArgumentName, outputNodes[1].KeyNode.NodeType);
            Assert.AreEqual("recurse", outputNodes[1].KeyNode.Text);
            Assert.AreEqual(0, outputNodes[1].ValueNodes.Length);
            Assert.AreEqual(CommandLineNodeTypes.ArgumentName, outputNodes[2].KeyNode.NodeType);
            Assert.AreEqual(2, outputNodes[2].ValueNodes.Length);
            Assert.IsTrue(inputNodes[1].ValueNodes.SequenceEqual(outputNodes[2].ValueNodes));
        }

        [TestMethod]
        public void Ending_values_are_changed_to_unnamed_node_One_multiplicity()
        {
            var inputNodes = new CommandLineNodeGroup[]
            {
                new CommandLineNodeGroup
                (
                    new CommandLineNode(CommandLineNodeTypes.Exe, "program.exe"),
                    CommandLineNode.EmptyArray
                ),
                new CommandLineNodeGroup
                (
                    new CommandLineNode(CommandLineNodeTypes.ArgumentName, "ControlFile"),
                    new CommandLineNode[]
                    {
                        new CommandLineNode(CommandLineNodeTypes.Value, "DirectoryA"),
                        new CommandLineNode(CommandLineNodeTypes.Value, "DirectoryB")
                    }
                )
            }.ToImmutableArray();
            var arguments = new Argument[]
            {
                new Argument(new string[] {"ControlFile" }.ToImmutableArray(),
                        ArgumentMultiplicity.One, false)
            }.ToImmutableArray();
            var outputNodes = ParsingFixups.SplitEndingUnnamedValues(inputNodes,
                arguments, StringComparison.InvariantCultureIgnoreCase);
            Assert.AreNotEqual(inputNodes.Length, outputNodes.Length);
            Assert.AreEqual(inputNodes[0].KeyNode.NodeType, outputNodes[0].KeyNode.NodeType);
            Assert.AreEqual(inputNodes[0].ValueNodes.Length, inputNodes[0].ValueNodes.Length);
            Assert.AreEqual(0, outputNodes[0].ValueNodes.Length);
            Assert.AreEqual(CommandLineNodeTypes.ArgumentName, outputNodes[1].KeyNode.NodeType);
            Assert.AreEqual("ControlFile", outputNodes[1].KeyNode.Text);
            Assert.AreEqual(1, outputNodes[1].ValueNodes.Length);
            Assert.AreEqual(CommandLineNodeTypes.ArgumentName, outputNodes[2].KeyNode.NodeType);
            Assert.AreEqual(1, outputNodes[2].ValueNodes.Length);
            Assert.AreEqual(inputNodes[1].ValueNodes[1], outputNodes[2].ValueNodes[0]);
        }

        [TestMethod]
        public void NodeGroup_collection_with_no_duplicates_is_unmodified()
        {
            var input =
                new CommandLineBuilder()
                    .AddExeNode("DestroyTheWorld.exe")
                    .AddArgument("File", "FileA.txt")
                    .AddArgument("Recurse")
                    .GetCommandLine();
            var arguments = new Argument[]
            {
                new Argument(new string[] {"File" }.ToImmutableArray(),
                        ArgumentMultiplicity.OneOrMore, true),
                new Argument(new string[] {"Recurse" }.ToImmutableArray(),
                        ArgumentMultiplicity.Zero, false)
            }.ToImmutableArray();
            var output = ParsingFixups.ConsolidateDuplicateArguments(input, arguments,
                StringComparison.InvariantCultureIgnoreCase, 
                NameMatchingOptions.Exact);
            Assert.IsTrue(CommandLineNodeCompare.Equals(input, output, StringComparison.Ordinal));
        }

        [TestMethod]
        public void NodeGroup_collection_with_duplicates_is_consolidated()
        {
            var input =
                new CommandLineBuilder()
                    .AddExeNode("DeleteSomeStuff.exe")
                    .AddArgument("Directory", "DirectoryA")
                    .AddArgument("Directory", "DirectoryB")
                    .AddArgument("Recurse")
                    .GetCommandLine();
            var arguments = new Argument[]
            {
                new Argument(new string[] { "Directory" }.ToImmutableArray(),
                        ArgumentMultiplicity.OneOrMore, true),
                new Argument(new string[] {"Recurse" }.ToImmutableArray(),
                        ArgumentMultiplicity.Zero, false)
            }.ToImmutableArray();
            var output = ParsingFixups.ConsolidateDuplicateArguments(input, arguments,
                StringComparison.InvariantCultureIgnoreCase, 
                NameMatchingOptions.Stem);
            Assert.AreEqual(3, output.Count());
            Assert.AreEqual(CommandLineNodeTypes.ArgumentName, output[1].KeyNode.NodeType);
            Assert.AreEqual(2, output[1].ValueNodes.Count());
        }

        [TestMethod]
        public void NodeGroup_collection_with_multiple_duplicates_is_consolidated()
        {
            var input =
                new CommandLineBuilder()
                    .AddExeNode("DeleteSomeStuff.exe")
                    .AddArgument("Directory", "DirectoryA")
                    .AddArgument("Directory", "DirectoryB")
                    .AddArgument("Recurse")
                    .AddArgument("File", "FileA.docx")
                    .AddArgument("File", "FileB.docx")
                    .AddArgument("File", "Index.xslx")
                    .GetCommandLine();
            var arguments = new Argument[]
            {
                new Argument(new string[] { "Directory" }.ToImmutableArray(),
                        ArgumentMultiplicity.OneOrMore, true),
                new Argument(new string[] { "File" }.ToImmutableArray(),
                        ArgumentMultiplicity.OneOrMore, false),
                new Argument(new string[] {"Recurse" }.ToImmutableArray(),
                        ArgumentMultiplicity.Zero, false)
            }.ToImmutableArray();
            var output = ParsingFixups.ConsolidateDuplicateArguments(input, arguments,
                StringComparison.InvariantCultureIgnoreCase, 
                NameMatchingOptions.Stem);
            Assert.AreEqual(4, output.Count());
            Assert.AreEqual(CommandLineNodeTypes.ArgumentName, output[1].KeyNode.NodeType);
            Assert.AreEqual(2, output[1].ValueNodes.Count());
            Assert.AreEqual(CommandLineNodeTypes.ArgumentName, output[3].KeyNode.NodeType);
            Assert.AreEqual(3, output[3].ValueNodes.Count());
        }

        [TestMethod]
        public void NodeGroup_collection_with_duplicates_and_multiple_names_is_consolidated()
        {
            var input =
                new CommandLineBuilder()
                    .AddExeNode("DeleteSomeStuff.exe")
                    .AddArgument("Dir", "DirectoryA")
                    .AddArgument("Fold", "DirectoryB")
                    .AddArgument("Recurse")
                    .GetCommandLine();
            var arguments = new Argument[]
            {
                new Argument(new string[] { "Directory", "Folder" }.ToImmutableArray(),
                        ArgumentMultiplicity.OneOrMore, true),
                new Argument(new string[] {"Recurse" }.ToImmutableArray(),
                        ArgumentMultiplicity.Zero, false)
            }.ToImmutableArray();
            var output = ParsingFixups.ConsolidateDuplicateArguments(input, arguments,
                StringComparison.InvariantCultureIgnoreCase, 
                NameMatchingOptions.Stem);
            Assert.AreEqual(3, output.Count());
            Assert.AreEqual(CommandLineNodeTypes.ArgumentName, output[1].KeyNode.NodeType);
            Assert.AreEqual(2, output[1].ValueNodes.Count());
        }

        [TestMethod]
        public void Consolidate_ignores_unnamed_values_groups()
        {
            var input =
                new CommandLineBuilder()
                    .AddExeNode("Fly.exe")
                    .AddUnnamedArgument("Air", "Spain")
                    .AddArgument("File", "FileA.txt")
                    .AddArgument("file", "FileB.txt")
                    .AddArgument("Recurse")
                    .GetCommandLine();
            var arguments = new Argument[]
            {
                new Argument(new string[] { "File" }.ToImmutableArray(),
                        ArgumentMultiplicity.OneOrMore, true),
                new Argument(new string[] {"Recurse" }.ToImmutableArray(),
                        ArgumentMultiplicity.Zero, false)
            }.ToImmutableArray();
            var output = ParsingFixups.ConsolidateDuplicateArguments(input, arguments,
                StringComparison.InvariantCultureIgnoreCase, 
                NameMatchingOptions.Exact);
            Assert.AreEqual(4, output.Count());
            Assert.IsTrue(output[1].IsUnnamedValuesNodeGroup);
            Assert.AreEqual(2, output[1].ValueNodes.Count());
        }
        
        [TestMethod]
        public void Consolidate_obeys_case_sensitivity()
        {
            var input =
                new CommandLineBuilder()
                    .AddExeNode("DeleteSomeStuff.exe")
                    .AddArgument("Dir", "DirectoryA")
                    .AddArgument("dir", "DirectoryB")
                    .AddArgument("Recurse")
                    .GetCommandLine();
            var arguments = new Argument[]
            {
                new Argument(new string[] { "Directory", "Folder" }.ToImmutableArray(),
                        ArgumentMultiplicity.OneOrMore, true),
                new Argument(new string[] {"Recurse" }.ToImmutableArray(),
                        ArgumentMultiplicity.Zero, false)
            }.ToImmutableArray();
            var output = ParsingFixups.ConsolidateDuplicateArguments(input, arguments,
                StringComparison.InvariantCulture, 
                NameMatchingOptions.Exact);
            Assert.AreEqual(4, output.Count());
        }

        [TestMethod]
        public void Consolidates_with_stem_matching()
        {
            var input =
                new CommandLineBuilder()
                    .AddExeNode("DeleteSomeStuff.exe")
                    .AddArgument("Dir", "DirectoryA")
                    .AddArgument("Dir", "DirectoryB")
                    .AddArgument("Recurse")
                    .GetCommandLine();
            var arguments = new Argument[]
            {
                new Argument(new string[] { "Directory", "Folder" }.ToImmutableArray(),
                        ArgumentMultiplicity.OneOrMore, true),
                new Argument(new string[] {"Recurse" }.ToImmutableArray(),
                        ArgumentMultiplicity.Zero, false)
            }.ToImmutableArray();
            var output = ParsingFixups.ConsolidateDuplicateArguments(input, arguments,
                StringComparison.InvariantCultureIgnoreCase, 
                NameMatchingOptions.Stem);
            Assert.AreEqual(3, output.Count());
            Assert.IsTrue(output[0].IsExeNodeGroup);
            Assert.AreEqual(CommandLineNodeTypes.ArgumentName, output[1].KeyNode.NodeType);
            Assert.AreEqual(2, output[1].ValueNodes.Count());
            Assert.AreEqual(0, output[2].ValueNodes.Count());
        }

        [TestMethod]
        public void Consolidate_consolidates_undefined_arguments()
        {
            var input =
                new CommandLineBuilder()
                    .AddExeNode("DestroyTheWorld.exe")
                    .AddArgument("File", "FileA.txt")
                    .AddArgument("file", "FileB.txt")
                    .AddArgument("Recurse")
                    .GetCommandLine();
            var arguments = new Argument[]
            {
                new Argument(new string[] {"Recurse" }.ToImmutableArray(),
                        ArgumentMultiplicity.Zero, false)
            }.ToImmutableArray();
            var output = ParsingFixups.ConsolidateDuplicateArguments(input, arguments,
                StringComparison.InvariantCultureIgnoreCase, NameMatchingOptions.Exact);
            Assert.AreEqual(3, output.Count());
            Assert.AreEqual(1, output.Count(ng => ng.KeyNode.Text.Equals("File", StringComparison.InvariantCultureIgnoreCase)));
            Assert.AreEqual(2, output.Single(ng => ng.KeyNode.Text.Equals("File", StringComparison.InvariantCultureIgnoreCase)).ValueNodes.Count());
        }

        [TestMethod]
        public void Consolidate_tolerates_missing_arguments()
        {
            var input =
                new CommandLineBuilder()
                    .AddExeNode("DestroyTheWorld.exe")
                    .AddArgument("File", "FileA.txt")
                    .AddArgument("Recurse")
                    .GetCommandLine();
            var arguments = new Argument[]
            {
                new Argument(new string[] { "File" }.ToImmutableArray(),
                        ArgumentMultiplicity.OneOrMore, true),
                new Argument(new string[] { "Recurse" }.ToImmutableArray(),
                        ArgumentMultiplicity.Zero, false),
                new Argument(new string[] { "Delete" }.ToImmutableArray(),
                        ArgumentMultiplicity.Zero, true)
            }.ToImmutableArray();
            var output = ParsingFixups.ConsolidateDuplicateArguments(input, arguments,
                StringComparison.InvariantCultureIgnoreCase, NameMatchingOptions.Exact);
            Assert.IsTrue(CommandLineNodeCompare.Equals(input, output, StringComparison.Ordinal));
        }

        [TestMethod]
        public void Last_argument_has_one_value()
        {
            var input =
                new CommandLineBuilder()
                    .AddArgument("File", "SomeFile.txt")
                    .GetCommandLine();
            var arguments = new Argument[]
            {
                new Argument("File", ArgumentMultiplicity.One, true)
            }.ToImmutableArray();
            var output = ParsingFixups.SplitEndingUnnamedValues(input, arguments, StringComparison.InvariantCultureIgnoreCase);
            var areEqual = CommandLineNodeCompare.Equals(input, output, StringComparison.InvariantCultureIgnoreCase);
            Assert.IsTrue(areEqual);

        }

        private void Ending_values_are_not_unnamed_internal(ArgumentMultiplicity multiplicity)
        {
            var inputNodes = new CommandLineNodeGroup[]
                        {
                new CommandLineNodeGroup
                (
                    new CommandLineNode(CommandLineNodeTypes.Exe, "program.exe"),
                    CommandLineNode.EmptyArray
                ),
                new CommandLineNodeGroup
                (
                    new CommandLineNode(CommandLineNodeTypes.ArgumentName, "files"),
                    new CommandLineNode[]
                    {
                        new CommandLineNode(CommandLineNodeTypes.Value, "FileA.txt"),
                        new CommandLineNode(CommandLineNodeTypes.Value, "FileB.txt")
                    }
                )
                        }.ToImmutableArray();
            var arguments = new Argument[]
            {
                new Argument(new string[] {"files" }.ToImmutableArray(),
                        multiplicity, false)
            }.ToImmutableArray();
            var outputNodes = ParsingFixups.SplitEndingUnnamedValues(inputNodes,
                arguments, StringComparison.InvariantCultureIgnoreCase);
            Assert.AreEqual(inputNodes, outputNodes);
        }

    }
}
