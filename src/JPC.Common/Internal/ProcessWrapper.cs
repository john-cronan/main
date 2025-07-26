using System;
using System.Diagnostics;
using System.IO;
using InnerProcess = System.Diagnostics.Process;

namespace JPC.Common.Internal
{
    internal class ProcessWrapper : IProcess
    {
        private readonly InnerProcess _innerProcess;

        public ProcessWrapper(InnerProcess innerProcess)
        {
            _innerProcess = innerProcess;
        }

        bool IProcess.EnabledRaisingEvents { get => _innerProcess.EnableRaisingEvents; set => _innerProcess.EnableRaisingEvents = value; }
        int IProcess.ExitCode => _innerProcess.ExitCode;
        DateTime IProcess.ExitTime => _innerProcess.ExitTime;
        bool IProcess.HasExited => _innerProcess.HasExited;
        int IProcess.Id => _innerProcess.Id;
        ProcessModule IProcess.MainModule => _innerProcess.MainModule;
        string IProcess.ProcessName => _innerProcess.ProcessName;
        StreamReader IProcess.StandardError => _innerProcess.StandardError;
        StreamWriter IProcess.StandardInput => _innerProcess.StandardInput;
        StreamReader IProcess.StandardOutput => _innerProcess.StandardOutput;
        void IProcess.BeginErrorReadLine() => _innerProcess.BeginErrorReadLine();
        void IProcess.BeginOutputReadLine() => _innerProcess.BeginOutputReadLine();
        void IProcess.CancelErrorRead() => _innerProcess.CancelErrorRead();
        void IProcess.CancelOutputRead() => _innerProcess.CancelOutputRead();
        void IProcess.Close() => _innerProcess.Close();
        void IProcess.CloseMainWindow() => _innerProcess.CloseMainWindow();
        void IDisposable.Dispose() => _innerProcess.Dispose();
        void IProcess.Kill() => _innerProcess.Kill();
        bool IProcess.Start() => _innerProcess.Start();
        void IProcess.WaitForExit() => _innerProcess.WaitForExit();
        bool IProcess.WaitForExit(int timeoutInMilliseconds) => _innerProcess.WaitForExit(timeoutInMilliseconds);
        void IProcess.WaitForInputIdle() => _innerProcess.WaitForInputIdle();
        bool IProcess.WaitForInputIdle(int timeoutInMilliseconds) => _innerProcess.WaitForInputIdle(timeoutInMilliseconds);

        event EventHandler IProcess.Exited
        {
            add { _innerProcess.Exited += value; }
            remove { _innerProcess.Exited -= value; }
        }

        event DataReceivedEventHandler IProcess.ErrorDataReceived
        {
            add { _innerProcess.ErrorDataReceived += value; }
            remove { _innerProcess.ErrorDataReceived -= value; }
        }

        event DataReceivedEventHandler IProcess.OutputDataReceived
        {
            add { _innerProcess.OutputDataReceived += value; }
            remove { _innerProcess.OutputDataReceived -= value; }
        }
    }
}
