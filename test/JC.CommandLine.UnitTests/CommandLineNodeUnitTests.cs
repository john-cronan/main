using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace JC.CommandLine.UnitTests
{
    [TestClass]
    public class CommandLineNodeUnitTests
    {
        [TestMethod]
        public void Values_then_argument_name_then_values()
        {
            var commandLine = new string[]
            {
                "FileA.txt", "FileB.txt", "-recurse", "FileC.txt"
            };
            var nodes = CommandLineNode.Parse(commandLine, new char[] { '-' }).ToArray();
            Assert.AreEqual(4, nodes.Count());
            var expected = new CommandLineNodeTypes[]
            {
                CommandLineNodeTypes.Value, CommandLineNodeTypes.Value,
                CommandLineNodeTypes.ArgumentName, CommandLineNodeTypes.Value
            };
            Assert.IsTrue(nodes.Select(n => n.NodeType).SequenceEqual(expected));
        }

        [TestMethod]
        public void Starts_with_switch()
        {
            var commandLine = new string[]
            {
                "-files", "FileA.txt", "FileB.txt", "FileC.txt",
                "-recycle"
            };
            var nodes = CommandLineNode.Parse(commandLine, new char[] { '-' }).ToArray();
            var counts =
                (from node in nodes
                group node by node.NodeType into g
                select new
                {
                    NodeType = g.Key,
                    Count = g.Count()
                })
                .ToDictionary(x => x.NodeType, x => x.Count);
            Assert.IsFalse(counts.TryGetValue(CommandLineNodeTypes.Exe, out var value));
            Assert.AreEqual(2, counts[CommandLineNodeTypes.ArgumentName]);
            Assert.AreEqual(3, counts[CommandLineNodeTypes.Value]);
        }

        [TestMethod]
        public void Detects_exe()
        {
            var commandLine = new string[]
            {
                Environment.GetCommandLineArgs()[0],
                "-recurse"
            };
            var nodes = CommandLineNode.Parse(commandLine, new char[] { '-' });
            var exeNode = nodes.Single(n => n.NodeType == CommandLineNodeTypes.Exe);
            Assert.IsTrue(exeNode.Text == Environment.GetCommandLineArgs()[0]);
        }

        [TestMethod]
        public void Allows_multiple_argument_delimitters()
        {
            var commandLine = new string[]
            {
                "-files", "FileA.txt", "FileB.txt", "FileC.txt",
                "/recycle"
            };
            var nodes = CommandLineNode.Parse(commandLine, new char[] { '-', '/' }).ToArray();
            var counts =
                (from node in nodes
                 group node by node.NodeType into g
                 select new
                 {
                     NodeType = g.Key,
                     Count = g.Count()
                 })
                .ToDictionary(x => x.NodeType, x => x.Count);
            Assert.AreEqual(2, counts[CommandLineNodeTypes.ArgumentName]);
            Assert.AreEqual(3, counts[CommandLineNodeTypes.Value]);
        }

        [TestMethod]
        public void Dash_is_not_argument_delimitter()
        {
            var commandLine = new string[]
            {
                "-files", "FileA.txt", "FileB.txt", "FileC.txt"
            };
            var nodes = CommandLineNode.Parse(commandLine, new char[] { '/' });
            Assert.IsTrue(nodes.All(n => n.NodeType == CommandLineNodeTypes.Value));
        }

        [TestMethod]
        public void Returns_node_text()
        {
            var commandLine = new string[]
            {
                "-files", "FileA.txt", "FileB.txt", "FileC.txt"
            };
            var nodes = CommandLineNode.Parse(commandLine, new char[] { '-' }).ToArray();
            Assert.IsTrue(commandLine[0].Substring(1).Equals(nodes[0].Text, StringComparison.InvariantCulture));
            Assert.IsTrue(commandLine[1].Equals(nodes[1].Text, StringComparison.InvariantCulture));
            Assert.IsTrue(commandLine[2].Equals(nodes[2].Text, StringComparison.InvariantCulture));

        }
    }
}
