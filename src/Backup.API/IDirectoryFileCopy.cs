namespace JPC.Backup
{
    /// <summary>
    /// Abstracts the members of a class that backs up files from a single
    /// source directory to a destination directory. Instances of an 
    /// implementation are bound to a particular instance of 
    /// <see cref="BackupOptions"/>, which, it's assumed, can be different
    /// directory-by-directory. <see cref="IDirectoryFileCopyFactory"/> is
    /// responsible for building and caching instances of implementations.
    /// </summary>
    public interface IDirectoryFileCopy
    {
        /// <summary>
        /// Returns the <see cref="BackupOptions"/> used to create the
        /// instance. Instances are cached based on this value.
        /// </summary>
        BackupOptions Options { get; }

        /// <summary>
        /// Copies files from source to destination.
        /// </summary>
        Task CopyFilesAsync(string sourceDirectoryPath, string destinationDirectoryPath);
    }
}
