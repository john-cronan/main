using System;
using System.Collections.Generic;
using System.Linq;

namespace JC.CommandLine
{
    internal class CommandLineArgumentEnumerator
    {
        private readonly char _argsFileDelimitter;
        private readonly IFilesystem _filesystem;

        public CommandLineArgumentEnumerator(char argsFileDelimitter)
            : this(argsFileDelimitter, new Filesystem())
        {
        }

        public CommandLineArgumentEnumerator(char argsFileDelimitter, IFilesystem filesystem)
        {
            Guard.IsNotNull(filesystem, nameof(filesystem));

            _argsFileDelimitter = argsFileDelimitter;
            _filesystem = filesystem;
        }

        public IEnumerable<string> Enumerate(IEnumerable<string> arguments)
        {
            Guard.IsNotNull(arguments, nameof(arguments));

            var visitedFiles = new Lazy<List<string>>(() => new List<string>());
            return Enumerate(arguments, visitedFiles);
        }

        private IEnumerable<string> Enumerate(IEnumerable<string> arguments,
            Lazy<List<string>> visitedFiles)
        {
            foreach (var argument in arguments)
            {
                if (string.IsNullOrWhiteSpace(argument))
                {
                    yield return argument;
                }
                else
                {
                    if (argument[0] == _argsFileDelimitter)
                    {
                        var fileName = argument.Substring(1);
                        var filePath = _filesystem.MakePathFullyQualified(fileName);
                        if (visitedFiles.Value.Contains(filePath))
                        {
                            throw new CommandLineParseException("Cyclic dependency detected in args files");
                        }
                        visitedFiles.Value.Add(filePath);
                        var contents = _filesystem.ReadAllLines(fileName)
                                            .Select(_ => _.Trim()).ToArray();
                        foreach (var item in Enumerate(contents, visitedFiles))
                        {
                            yield return item;
                        }
                        visitedFiles.Value.Remove(filePath);
                    }
                    else
                    {
                        yield return argument;
                    }
                }
            }
        }
    }
}
