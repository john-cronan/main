namespace JPC.Backup
{
    public interface ISourceDirectoryWalkerBuilder
    {
        IList<MatchExpression> DirectoryStopExpressions { get; set; }
        bool DirectoryStopOnColon { get; set; }
        ISourceDirectoryWalker BuildSourceDirectoryWalker();
    }
}
