using System;
using System.Collections.Immutable;

namespace JC.CommandLine.UnitTests
{
    internal static class CommandLineNodeCompare
    {
        public static bool Equals(CommandLineNode x, CommandLineNode y, 
            StringComparison stringComparison)
        {
            if (x == null || y == null)
            {
                return x == null && y == null;
            }
            if (x.NodeType != y.NodeType)
            {
                return false;
            }
            if (string.IsNullOrEmpty(x.Text))
            {
                return string.IsNullOrEmpty(y.Text);
            }
            return x.Text.Equals(y.Text, stringComparison);
        }

        public static bool Equals(CommandLineNodeGroup x, 
            CommandLineNodeGroup y, StringComparison stringComparison)
        {
            if (x == null || y == null)
            {
                return x == null && y == null;
            }
            if (!Equals(x.KeyNode, y.KeyNode, stringComparison))
            {
                return false;
            }
            if (x.ValueNodes == null || y.ValueNodes == null)
            {
                return x.ValueNodes == null && y.ValueNodes == null;
            }
            if (x.ValueNodes.Length != y.ValueNodes.Length)
            {
                return false;
            }
            for (int i = 0; i < x.ValueNodes.Length; i++)
            {
                if (!Equals(x.ValueNodes[i], y.ValueNodes[i], stringComparison))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool Equals(ImmutableArray<CommandLineNodeGroup> x,
            ImmutableArray<CommandLineNodeGroup> y, 
            StringComparison stringComparison)
        {
            if (x.IsDefault || y.IsDefault)
            {
                return x.IsDefault && y.IsDefault;
            }
            if (x.IsEmpty || y.IsEmpty)
            {
                return x.IsEmpty && y.IsEmpty;
            }
            if (x.Length != y.Length)
            {
                return false;
            }
            for (int i = 0; i < x.Length; i++)
            {
                if (!Equals(x[i], y[i], stringComparison))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
