using Moq;

namespace JPC.Common.Testing
{
    public class MockRuntime : IRuntime
    {
        private readonly Mock<IConsole> _console;
        private readonly MockEnvironment _environment;
        private readonly MockFilesystem _filesystem;
        private readonly MockProcessService _processService;
        private readonly MockTempFileService _tempFileService;
        private readonly MockClock _clock;

        public MockRuntime()
        {
            _console = new Mock<IConsole>();
            _environment = new MockEnvironment();
            _filesystem = new MockFilesystem();
            _processService = new MockProcessService();
            _tempFileService = new MockTempFileService();
            _clock = new MockClock();
        }

        public MockClock Clock => _clock;
        public Mock<IConsole> Console => _console;
        public MockEnvironment Environment => _environment;
        public MockFilesystem Filesystem => _filesystem;
        public MockProcessService ProcessService => _processService;
        public MockTempFileService TempFileService => _tempFileService;


        IClock IRuntime.Clock => _clock.Object;
        IConsole IRuntime.Console => _console.Object;
        IEnvironment IRuntime.Environment => _environment.Object;
        IFilesystem IRuntime.Filesystem => _filesystem.Object;
        IProcessService IRuntime.ProcessService => _processService.Object;
        ITempFileService IRuntime.TempFileService => _tempFileService.Object;

    }
}
