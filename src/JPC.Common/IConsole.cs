using System;
using System.IO;

namespace JPC.Common
{
    public interface IConsole
    {
        void Clear();
        Stream OpenStandardError();
        Stream OpenStandardInput();
        Stream OpenStandardOutput();
        int Read();
        ConsoleKeyInfo ReadKey(bool intercept);
        string ReadLine();
        void SetError(TextWriter writer);
        void SetIn(TextReader reader);
        void SetOut(TextWriter writer);
        void Write(char chr);
        void Write(string str);
        void WriteLine(string str);
        TextWriter Error { get; }
        TextReader In { get; }
        TextWriter Out { get; }
    }
}
