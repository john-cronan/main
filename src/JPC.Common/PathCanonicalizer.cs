using System;
using System.Collections.Generic;
using System.Linq;
using JPC.Common.Internal;

namespace JPC.Common
{
    public class PathCanonicalizer
    {
        private readonly IFilesystem _filesystem;

        public PathCanonicalizer()
            : this(new Filesystem(new EnvironmentWrapper()))
        {
            StringComparison = StringComparison.InvariantCultureIgnoreCase;
        }

        public PathCanonicalizer(IFilesystem filesystem)
        {
            if (filesystem == null)
            {
                throw new ArgumentNullException(nameof(filesystem));
            }

            _filesystem = filesystem;
        }

        public StringComparison StringComparison { get; set; }

        public string MakeCanonical(string path)
        {
            var effectivePath = path.Trim();
            effectivePath = effectivePath.Replace(_filesystem.AltDirectorySeparator, _filesystem.DirectorySeparator);
            var nodes = SplitPathIntoNodes(effectivePath);
            return _filesystem.CombinePath(nodes.ToArray());
        }

        public string MakeCanonical(string currentDirectory, string relativeOrAbsolutePath)
        {
            var effectivePath = 
                _filesystem.IsPathRooted(relativeOrAbsolutePath)
                    ? relativeOrAbsolutePath
                    : _filesystem.CombinePath(currentDirectory,relativeOrAbsolutePath);
            return MakeCanonical(effectivePath);
        }

        private IEnumerable<string> SplitPathIntoNodes(string effectivePath)
        {
            var isPathRooted = _filesystem.IsPathRooted(effectivePath);
            var nodes = _filesystem.SplitPath(effectivePath).ToList();
            RemoveDotNotes(nodes);
            RemoveDotDotNodes(nodes, isPathRooted);
            return nodes;
        }

        private void RemoveDotNotes(List<string> nodes)
        {
            var i = 1;
            while (i < nodes.Count)
            {
                if (nodes[i] == ".")
                {
                    nodes.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }
        }

        private void RemoveDotDotNodes(List<string> nodes, bool pathIsAbsolute)
        {
            var startingIndex = 0;
            if (!pathIsAbsolute)
            {
                while (nodes[startingIndex] == "..")
                {
                    startingIndex++;
                }
            }
            var index = IndexOfFirstDotDot(nodes, startingIndex);
            while (index != -1)
            {
                if (pathIsAbsolute && index - 1 == 0)
                {
                    throw new InvalidOperationException("Attempt to traverse above volume root");
                }
                nodes.RemoveAt(index);
                nodes.RemoveAt(index - 1);
                index = IndexOfFirstDotDot(nodes, startingIndex);
            }
        }

        private int IndexOfFirstDotDot(List<string> nodes, int startingAt)
        {
            for (int i = startingAt; i < nodes.Count; i++)
            {
                if ("..".Equals(nodes[i], StringComparison))
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
