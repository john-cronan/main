using JPC.Common;
using System.Text;

namespace JPC.Backup
{
    internal class SpecificationFileFinder
    {
        private const string DefaultSpecificationFileName = "Backup.json";

        private SpecificationFile _foundFile;
        private string _foundFilePath;
        private readonly IRuntime _runtime;

        internal SpecificationFile FoundFile => _foundFile;
        public string FoundFilePath => _foundFilePath;

        public SpecificationFileFinder(IRuntime runtime)
        {
            if (runtime == null)
            {
                throw new ArgumentNullException(nameof(runtime));
            }

            _runtime = runtime;
        }

        public void Find()
        {
            _foundFile = null;
            _foundFilePath = null;
            var (specFile, specFilePath) = FindByCommandLine();
            if (specFile == null)
            {
                (specFile, specFilePath) = FindByCurrentDirectory();
            }
            if (specFile == null)
            {
                var msg = new StringBuilder()
                    .Append("Specification file not found on command line and ")
                    .Append($"no file named {DefaultSpecificationFileName} ")
                    .Append($"found in directory {_runtime.Filesystem.GetCurrentDirectory()}")
                    .ToString();
                throw new SpecificationFileNotFoundException(msg);
            }
            _foundFile = specFile;
            _foundFilePath = specFilePath;
        }

        private (SpecificationFile, string) FindByCommandLine()
        {
            var commandLineArgs = _runtime.Environment.GetCommandLineArgs();
            if (commandLineArgs.Length > 1)
            {
                var specFilePath = commandLineArgs[1];
                if (_runtime.Filesystem.FileExists(specFilePath))
                {
                    var content = _runtime.Filesystem.ReadAllText(specFilePath);
                    var specFile = SpecificationFile.ParseJson(content);
                    return (specFile, specFilePath);
                }
                else
                {
                    throw new SpecificationFileNotFoundException($"Specification file '{commandLineArgs[1]}' not found");
                }
            }
            return (null, null);
        }

        private (SpecificationFile, string) FindByCurrentDirectory()
        {
            var specFilePath = _runtime.Filesystem.CombinePath(
                _runtime.Filesystem.GetCurrentDirectory(),
                DefaultSpecificationFileName);
            if (_runtime.Filesystem.FileExists(specFilePath))
            {
                var content = _runtime.Filesystem.ReadAllText(specFilePath);
                var specFile = SpecificationFile.ParseJson(content);
                return (specFile, specFilePath);
            }
            return (null, null);
        }
    }
}
