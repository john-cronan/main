using System;
using System.IO;

namespace JPC.Common.Internal
{
    internal class ConsoleWrapper : IConsole
    {
        TextWriter IConsole.Error => Console.Error;
        TextReader IConsole.In => Console.In;
        TextWriter IConsole.Out => Console.Out;
        void IConsole.Clear() => Console.Clear();
        Stream IConsole.OpenStandardError() => Console.OpenStandardError();
        Stream IConsole.OpenStandardInput() => Console.OpenStandardInput();
        Stream IConsole.OpenStandardOutput() => Console.OpenStandardInput();
        int IConsole.Read() => Console.Read();
        ConsoleKeyInfo IConsole.ReadKey(bool intercept) => Console.ReadKey(intercept);
        string IConsole.ReadLine() => Console.ReadLine();
        void IConsole.SetError(TextWriter writer) => Console.SetError(writer);
        void IConsole.SetIn(TextReader reader) => Console.SetIn(reader);
        void IConsole.SetOut(TextWriter writer) => Console.SetOut(writer);
        void IConsole.Write(char chr) => Console.Write(chr);
        void IConsole.Write(string str) => Console.Write(str);
        void IConsole.WriteLine(string str) => Console.WriteLine(str);
    }
}
