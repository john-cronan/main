using System.Collections.Generic;
using System.Diagnostics;

namespace JPC.Common
{
    public interface IProcessService
    {
        IEnumerable<IProcess> GetAll();
        IProcess Get(int id);
        IEnumerable<IProcess> Get(string name);
        IProcess GetCurrentProcess();
        IProcess Start(ProcessStartInfo startInfo);
    }
}
