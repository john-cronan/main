using System;
using System.Diagnostics;
using System.IO;

namespace JPC.Common
{
    public interface IProcess : IDisposable
    {
        void BeginErrorReadLine();
        void BeginOutputReadLine();
        void CancelErrorRead();
        void CloseMainWindow();
        void CancelOutputRead();
        void Close();
        void Kill();
        bool Start();
        void WaitForExit();
        bool WaitForExit(int timeoutInMilliseconds);
        void WaitForInputIdle();
        bool WaitForInputIdle(int timeoutInMilliseconds);

        bool EnabledRaisingEvents { get; set; }
        int ExitCode { get; }
        DateTime ExitTime { get; }
        bool HasExited { get; }
        int Id { get; }
        ProcessModule MainModule { get; }
        string ProcessName { get; }
        StreamReader StandardError {  get; }
        StreamWriter StandardInput { get; } 
        StreamReader StandardOutput { get; }
        event EventHandler Exited;
        event DataReceivedEventHandler ErrorDataReceived;
        event DataReceivedEventHandler OutputDataReceived;
    }
}
