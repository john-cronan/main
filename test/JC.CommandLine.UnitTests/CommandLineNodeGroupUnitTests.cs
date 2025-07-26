using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JC.CommandLine.UnitTests
{
    [TestClass]
    public class CommandLineNodeGroupUnitTests
    {
        [TestMethod]
        public void Nothing_but_values_results_in_an_unnamed_group()
        {
            var nodeStream = new CommandLineNode[]
            {
                new CommandLineNode(CommandLineNodeTypes.Value, "File1.txt"),
                new CommandLineNode(CommandLineNodeTypes.Value, "File2.txt"),
                new CommandLineNode(CommandLineNodeTypes.Value, "File3.txt"),
                new CommandLineNode(CommandLineNodeTypes.Value, "File4.txt"),
            };
            var nodeGroups = CommandLineNodeGroup.Parse(nodeStream).ToArray();
            Assert.AreEqual(1, nodeGroups.Count());
            var single = nodeGroups.Single();
            Assert.AreEqual(CommandLineNodeTypes.ArgumentName, single.KeyNode.NodeType);
            Assert.IsTrue(single.IsUnnamedValuesNodeGroup);
            Assert.AreEqual(single.ValueNodes.Count(), 4);
        }

        [TestMethod]
        public void No_params_results_in_an_exe_group()
        {
            var nodeStream = new CommandLineNode[]
            {
                new CommandLineNode(CommandLineNodeTypes.Exe, "something.exe")
            };
            var nodeGroups = CommandLineNodeGroup.Parse(nodeStream).ToArray();
            Assert.AreEqual(1, nodeGroups.Count());
            var single = nodeGroups.Single();
            Assert.AreEqual(CommandLineNodeTypes.Exe, single.KeyNode.NodeType);
            Assert.IsFalse(single.ValueNodes.Any());
        }

        [TestMethod]
        public void Single_argument_with_value_results_in_one_group()
        {
            var nodeStream = new CommandLineNode[]
            {
                new CommandLineNode(CommandLineNodeTypes.Exe, "something.exe"),
                new CommandLineNode(CommandLineNodeTypes.ArgumentName, "file"),
                new CommandLineNode(CommandLineNodeTypes.Value, "SomeFile.txt")
            };
            var nodeGroups = CommandLineNodeGroup.Parse(nodeStream).ToArray();
            Assert.AreEqual(1, nodeGroups.Count(g => g.KeyNode.NodeType == CommandLineNodeTypes.Exe));
            Assert.AreEqual(1, nodeGroups.Count(g => g.KeyNode.NodeType == CommandLineNodeTypes.ArgumentName));
            var argumentName = nodeGroups.Single(g => g.KeyNode.NodeType == CommandLineNodeTypes.ArgumentName);
            Assert.AreEqual(CommandLineNodeTypes.ArgumentName, argumentName.KeyNode.NodeType);
            Assert.AreEqual(1, argumentName.ValueNodes.Count());
            var valueNode = argumentName.ValueNodes.Single();
            Assert.IsTrue(valueNode.Text.Equals("SomeFile.txt", StringComparison.InvariantCulture));
            Assert.AreEqual(CommandLineNodeTypes.Value, valueNode.NodeType);
        }

        [TestMethod]
        public void Multi_valued_argument_has_multiple_value_nodes()
        {
            var nodeStream = new CommandLineNode[]
            {
                new CommandLineNode(CommandLineNodeTypes.Exe, "something.exe"),
                new CommandLineNode(CommandLineNodeTypes.ArgumentName, "files"),
                new CommandLineNode(CommandLineNodeTypes.Value, "SomeFile.txt"),
                new CommandLineNode(CommandLineNodeTypes.Value, "SomeOtherFile.txt")
            };
            var nodeGroups = CommandLineNodeGroup.Parse(nodeStream).ToArray();
            Assert.AreEqual(1, nodeGroups.Count(g => g.KeyNode.NodeType == CommandLineNodeTypes.Exe));
            Assert.AreEqual(1, nodeGroups.Count(g => g.KeyNode.NodeType == CommandLineNodeTypes.ArgumentName));
            var argumentName = nodeGroups.Single(g => g.KeyNode.NodeType == CommandLineNodeTypes.ArgumentName);
            Assert.AreEqual(CommandLineNodeTypes.ArgumentName, argumentName.KeyNode.NodeType);
            Assert.AreEqual(2, argumentName.ValueNodes.Count());
            Assert.AreEqual(1, argumentName.ValueNodes.Count(vn => vn.Text.Equals("SomeFile.txt", StringComparison.InvariantCulture)));
            Assert.AreEqual(1, argumentName.ValueNodes.Count(vn => vn.Text.Equals("SomeOtherFile.txt", StringComparison.InvariantCulture)));
        }

        [TestMethod]
        public void Multiple_arguments_mean_multiple_groups()
        {
            var nodeStream = new CommandLineNode[]
            {
                new CommandLineNode(CommandLineNodeTypes.Exe, "something.exe"),
                new CommandLineNode(CommandLineNodeTypes.ArgumentName, "files"),
                new CommandLineNode(CommandLineNodeTypes.Value, "SomeFile.txt"),
                new CommandLineNode(CommandLineNodeTypes.Value, "SomeOtherFile.txt"),
                new CommandLineNode(CommandLineNodeTypes.ArgumentName, "recurse")
            };
            var nodeGroups = CommandLineNodeGroup.Parse(nodeStream).ToArray();
            Assert.AreEqual(3, nodeGroups.Count());
            var filesGroup = nodeGroups.Single(ng => ng.KeyNode.NodeType == CommandLineNodeTypes.ArgumentName && ng.KeyNode.Text.Equals("files", StringComparison.InvariantCulture));
            Assert.AreEqual(2, filesGroup.ValueNodes.Count());
            var recurseGroup = nodeGroups.Single(ng => ng.KeyNode.NodeType == CommandLineNodeTypes.ArgumentName && ng.KeyNode.Text.Equals("recurse", StringComparison.InvariantCulture));
            Assert.AreEqual(0, recurseGroup.ValueNodes.Count());
        }

        [TestMethod]
        public void Multiple_arguments_mean_multiple_groups_reversed()
        {
            var nodeStream = new CommandLineNode[]
            {
                new CommandLineNode(CommandLineNodeTypes.Exe, "something.exe"),
                new CommandLineNode(CommandLineNodeTypes.ArgumentName, "recurse"),
                new CommandLineNode(CommandLineNodeTypes.ArgumentName, "files"),
                new CommandLineNode(CommandLineNodeTypes.Value, "SomeFile.txt"),
                new CommandLineNode(CommandLineNodeTypes.Value, "SomeOtherFile.txt")
            };
            var nodeGroups = CommandLineNodeGroup.Parse(nodeStream).ToArray();
            Assert.AreEqual(3, nodeGroups.Count());
            var filesGroup = nodeGroups.Single(ng => ng.KeyNode.NodeType == CommandLineNodeTypes.ArgumentName && ng.KeyNode.Text.Equals("files", StringComparison.InvariantCulture));
            Assert.AreEqual(2, filesGroup.ValueNodes.Count());
            var recurseGroup = nodeGroups.Single(ng => ng.KeyNode.NodeType == CommandLineNodeTypes.ArgumentName && ng.KeyNode.Text.Equals("recurse", StringComparison.InvariantCulture));
            Assert.AreEqual(0, recurseGroup.ValueNodes.Count());
        }

        [TestMethod]
        public void Multiple_instances_are_supported()
        {
            var nodeStream = new CommandLineNode[]
            {
                new CommandLineNode(CommandLineNodeTypes.Exe, "something.exe"),
                new CommandLineNode(CommandLineNodeTypes.ArgumentName, "file"),
                new CommandLineNode(CommandLineNodeTypes.Value, "SomeFile.txt"),
                new CommandLineNode(CommandLineNodeTypes.ArgumentName, "file"),
                new CommandLineNode(CommandLineNodeTypes.Value, "SomeOtherFile.txt"),
                new CommandLineNode(CommandLineNodeTypes.ArgumentName, "recurse")
            };
            var nodeGroups = CommandLineNodeGroup.Parse(nodeStream).ToArray();
            Assert.AreEqual(4, nodeGroups.Count());
            Assert.AreEqual(1, nodeGroups.Count(ng => ng.KeyNode.NodeType == CommandLineNodeTypes.Exe));
            Assert.AreEqual(2, nodeGroups.Count(ng => ng.KeyNode.NodeType == CommandLineNodeTypes.ArgumentName && ng.KeyNode.Text.Equals("file")));
            Assert.AreEqual(1, nodeGroups.Count(ng => ng.KeyNode.NodeType == CommandLineNodeTypes.ArgumentName && ng.KeyNode.Text.Equals("recurse", StringComparison.InvariantCulture)));
            var totalCount = 
                nodeGroups.Where(ng => ng.KeyNode.Text.Equals("file", StringComparison.InvariantCulture))
                        .Select(ng => ng.ValueNodes.Count())
                        .Sum();
            Assert.AreEqual(2, totalCount);
        }

        [TestMethod]
        public void Exe_node_contains_leading_unnamed_args()
        {
            var nodes = new CommandLineNode[]
            {
                new CommandLineNode(CommandLineNodeTypes.Exe, "Program.exe"),
                new CommandLineNode(CommandLineNodeTypes.Value, "delete"),
                new CommandLineNode(CommandLineNodeTypes.ArgumentName, "recurse"),
                new CommandLineNode(CommandLineNodeTypes.Value, "FileA.txt"),
                new CommandLineNode(CommandLineNodeTypes.Value, "FileB.txt")
            };
            var groups = CommandLineNodeGroup.Parse(nodes).ToArray();
            Assert.AreEqual(2, groups.Count());
            var exeGroup = groups.Single(g => g.KeyNode.NodeType == CommandLineNodeTypes.Exe);
            Assert.AreEqual(1, exeGroup.ValueNodes.Count());
            Assert.AreEqual("delete", exeGroup.ValueNodes.Single().Text);
            var argGroup = groups.Single(g => g.KeyNode.NodeType == CommandLineNodeTypes.ArgumentName);
            Assert.AreEqual(2, argGroup.ValueNodes.Count());
            Assert.AreEqual("FileA.txt", argGroup.ValueNodes.ElementAt(0).Text);
            Assert.AreEqual("FileB.txt", argGroup.ValueNodes.ElementAt(1).Text);
        }

        [TestMethod]
        public void LINQ_Take_accepts_zero()
        {
            var x = new List<string>
            {
                "a", "b"
            };
            var y = x.Take(0);
            Assert.AreEqual(0, y.Count());
        }
    }
}
