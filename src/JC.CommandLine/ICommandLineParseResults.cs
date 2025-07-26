using System.Collections.Generic;

namespace JC.CommandLine
{
    public interface ICommandLineParseResults
    {
        T Bind<T>();
        string GetValue(string argumentName);
        T GetValueAs<T>(string argumentName);
        IEnumerable<string> GetValues(string argumentName);
        IEnumerable<T> GetValuesAs<T>(string argumentName);
        bool IsPresent(string argumentName);

        CommandLineParseException ParseWarnings { get; }
    }
}
