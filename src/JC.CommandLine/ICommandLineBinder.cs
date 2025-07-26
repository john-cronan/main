namespace JC.CommandLine
{
    public interface ICommandLineBinder
    {
        string GetValue(string name);
        T GetValue<T>(string name);
        bool IsPresent(string name);
        string GetFirstValue(params string[] names);
        T GetFirstValue<T>(params string[] names);
        bool IsAnyPresent(params string[] names);
        T Bind<T>();
    }
}
