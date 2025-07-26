namespace JC.CommandLine.TargetTypeConverters
{
    internal interface ITargetTypeConverterInstances
    {
        T Get<T>() where T : TargetTypeConverter;
    }
}
