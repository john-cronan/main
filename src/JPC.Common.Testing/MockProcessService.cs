using System.Diagnostics;
using Moq;
using JPC.Common.Internal;

namespace JPC.Common.Testing
{
    public class MockProcessService : Mock<IProcessService>
    {
        private readonly List<IProcess> _processTable;
        private Action<ProcessStartInfo> _processStarted;

        public MockProcessService()
        {
            _processTable = new List<IProcess>();
            Setup(m => m.GetAll()).Returns(new List<IProcess>(_processTable));
            Setup(m => m.Get(It.IsAny<int>())).Returns((Delegate)(Func<int, IProcess>)(
                id => _processTable.Where(p => p.Id == id).FirstOrDefault()));
            Setup(m => m.Get(It.IsAny<string>())).Returns((Delegate)(Func<string, IEnumerable<IProcess>>)(
                name => _processTable.Where(p => p.ProcessName == name)));
            Setup(m => m.GetCurrentProcess()).Returns(new ProcessWrapper(Process.GetCurrentProcess()));
            Setup(m => m.Start(It.IsAny<ProcessStartInfo>())).Callback<ProcessStartInfo>(psi =>
            {
                if (_processStarted != null)
                {
                    _processStarted(psi);
                }
            });
        }

        private IProcess GetSingleProcess(int id)
        {
            return _processTable.Single(p => p.Id == id);
        }

        public void AddProcess(IProcess process)
        {
            _processTable.Add(process);
        }

        public void AddLocalProcesses()
            => _processTable.AddRange(Process.GetProcesses().Select(p => new ProcessWrapper(p)));

        public Action<ProcessStartInfo> ProcessStarted 
        { 
            get => _processStarted; 
            set => _processStarted = value; 
        }
    }
}
