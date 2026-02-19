namespace JPC.Backup
{
    public interface ISourceDirectoryWalkerBuilder
    {
        IList<MatchExpression> DirectoryStopExpressions { get; set; }
        ISourceDirectoryWalker BuildSourceDirectoryWalker();
    }
}
