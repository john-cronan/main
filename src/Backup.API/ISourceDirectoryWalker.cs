namespace JPC.Backup
{
    public interface ISourceDirectoryWalker
    {
        IEnumerable<SourceDirectory> Enumerate(string startingPath, BackupOptions options);
    }
}
