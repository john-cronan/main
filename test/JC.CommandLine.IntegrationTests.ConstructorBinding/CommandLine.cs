using System.Collections.Immutable;

namespace JC.CommandLine.IntegrationTests.ConstructorBinding
{
    internal class CommandLine
    {
        private readonly string _command;
        private readonly int? _batchSize;
        private readonly bool _verbose;
        private readonly IEnumerable<string> _files;

        public CommandLine(ImmutableArray<string> leadingUnnamedValues, int? batchSize,
            bool verbose, IEnumerable<string> trailingUnnamedValues)
        {
            //
            //  Note: We could also receive an argument named "unnamedValues", which
            //  will be populated with all unnamed values.
            //

            //  
            //  Validate that leadingUnnamedValues has only one element.
            _command = leadingUnnamedValues.Single();
            _batchSize = batchSize;
            _verbose = verbose;
            _files = trailingUnnamedValues;
        }

        public string Command => _command;
        public int? BatchSize => _batchSize;
        public bool Verbose => _verbose;
        public IEnumerable<string> Files => _files;
    }
}
