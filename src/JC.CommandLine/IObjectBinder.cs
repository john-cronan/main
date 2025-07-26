namespace JC.CommandLine
{
    internal interface IObjectBinder
    {
        T CreateObject<T>(ActualModelResolution actualModelResolution);
    }
}
