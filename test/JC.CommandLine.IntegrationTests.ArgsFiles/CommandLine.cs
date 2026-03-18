namespace JC.CommandLine.IntegrationTests.ArgsFiles
{
    internal class CommandLine
    {
        private readonly string _command;
        private readonly IEnumerable<string> _files;
        private readonly bool _strict;
        private readonly int? _batchSize;
        private readonly int? _maxParallelism;

        public CommandLine(IEnumerable<string> leadingUnnamedValues,
            IEnumerable<string> files, bool strict, int? batchSize,
            int? maxParallelism)
        {
            _command = leadingUnnamedValues.Single();
            _files = files;
            _strict = strict;
            _batchSize = batchSize;
            _maxParallelism = maxParallelism;
        }

        public string Command => _command;
        public IEnumerable<string> Files => _files;
        public bool Strict => _strict;
        public int? BatchSize => _batchSize;
        public int? MaxParallelism => _maxParallelism;
    }
}
