using System.Diagnostics;
using Moq;

namespace JPC.Common.Testing
{
    public class MockTempFileService : Mock<ITempFileService>
    {
        private Func<string, string> _onReserveFileName;
        private readonly string _processName;
        private readonly int _processId;
        private readonly string _tempDir;


        public MockTempFileService()
            : this(Process.GetCurrentProcess().ProcessName, Process.GetCurrentProcess().Id)
        {
        }

        public MockTempFileService(string processName)
            : this(processName, (int)new Random().NextInt64() % (10 ^ 5))
        {
        }

        public MockTempFileService(string processName, int processId)
        {
            _processName = processName;
            _processId = processId;
            _tempDir = Environment.GetEnvironmentVariable("TEMP");

            Setup(m => m.CleanAsync()).Returns(Task.CompletedTask);
            Setup(m => m.CleanAsync(It.IsAny<Action<CleanObjectResult>>())).Returns(Task.CompletedTask);
            SetupReserveFileNameToReturnGeneratedFilePath();
        }

        public Func<string, string> OnReserveFileName 
        {
            get { return _onReserveFileName; }
            set
            {
                _onReserveFileName = value;
                if (_onReserveFileName == null)
                {
                    SetupReserveFileNameToReturnGeneratedFilePath();
                }
                else
                {
                    Setup(m => m.ReserveFileName(It.IsAny<string>())).Returns((Delegate)(Func<string, string>)_onReserveFileName);
                }
            }
        }

        private void SetupReserveFileNameToReturnGeneratedFilePath()
        {
            Setup(m => m.ReserveFileName(It.IsAny<string>())).Returns((Delegate)(Func<string, string>)(
                (ext) =>
                {
                    var fileName = Guid.NewGuid().ToString("n").Substring(0, 8) + "." + ext;
                    return Path.Join(_tempDir, _processName, _processId.ToString(), fileName);
                }));
        }
    }
}
