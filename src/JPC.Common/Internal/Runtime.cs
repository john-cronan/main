namespace JPC.Common.Internal
{
    internal class Runtime : IRuntime
    {
        private readonly IConsole _console;
        private readonly IEnvironment _environment;
        private readonly IFilesystem _filesystem;
        private readonly IProcessService _processService;
        private readonly ITempFileService _tempFileService;
        private readonly IClock _clock;

        public Runtime(IConsole console, IEnvironment environment, IFilesystem filesystem,
            IProcessService processService, IClock clock, ITempFileService tempFileService)
        {
            _console = console;
            _environment = environment;
            _filesystem = filesystem;
            _processService = processService;
            _tempFileService = tempFileService;
            _clock = clock;
        }

        IConsole IRuntime.Console => _console;
        IEnvironment IRuntime.Environment => _environment;
        IFilesystem IRuntime.Filesystem => _filesystem;
        IProcessService IRuntime.ProcessService => _processService;
        ITempFileService IRuntime.TempFileService => _tempFileService;
        IClock IRuntime.Clock => _clock;

    }
}
