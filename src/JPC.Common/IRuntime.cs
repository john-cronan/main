namespace JPC.Common
{
    public interface IRuntime
    {
        IClock Clock { get; }
        IConsole Console { get; }
        IEnvironment Environment { get; }
        IFilesystem Filesystem { get; }
        IProcessService ProcessService { get; }
        ITempFileService TempFileService { get; }
    }
}
