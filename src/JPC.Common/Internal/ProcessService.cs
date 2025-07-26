using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace JPC.Common.Internal
{
    internal class ProcessService : IProcessService
    {
        IProcess IProcessService.Get(int id)
        {
            return new ProcessWrapper(Process.GetProcessById(id));
        }

        IEnumerable<IProcess> IProcessService.Get(string name)
        {
            return Process.GetProcessesByName(name).Select(p => new ProcessWrapper(p));
        }

        IEnumerable<IProcess> IProcessService.GetAll()
        {
            return Process.GetProcesses().Select(p => new ProcessWrapper(p));
        }

        IProcess IProcessService.GetCurrentProcess()
        {
            return new ProcessWrapper(Process.GetCurrentProcess());
        }

        IProcess IProcessService.Start(ProcessStartInfo startInfo)
        {
            return new ProcessWrapper(Process.Start(startInfo));
        }
    }
}
