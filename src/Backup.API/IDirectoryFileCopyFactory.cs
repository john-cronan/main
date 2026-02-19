namespace JPC.Backup
{
    public interface IDirectoryFileCopyFactory
    {
        IDirectoryFileCopy Create(BackupOptions options, IDirectoryFileCopy existingInstance);
    }
}
