using System;
using System.Threading.Tasks;

namespace JPC.Common
{
    public interface ITempFileService
    {
        Task CleanAsync();
        Task CleanAsync(Action<CleanObjectResult> cleanObjectCallback);
        string ReserveFileName(string extension);
    }
}
