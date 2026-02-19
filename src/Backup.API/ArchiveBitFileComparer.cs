using JPC.Common;

namespace JPC.Backup
{
    /// <summary>
    /// An implementation of <see cref="IFileComparer"/> that determines if
    /// a file should be copied based on the state of the source file's
    /// archive bit. This "comparison" does not take the destination file
    /// into account.
    /// </summary>
    internal class ArchiveBitFileComparer : IFileComparer
    {
        private readonly IFilesystem _filesystem;

        public ArchiveBitFileComparer(IFilesystem filesystem)
        {
            if (filesystem == null)
            {
                throw new ArgumentNullException(nameof(filesystem));    
            }

            _filesystem = filesystem;
        }

        bool IFileComparer.ShouldCopy(string sourcePath, string destinationPath)
        {
            if (string.IsNullOrWhiteSpace(sourcePath))
            {
                throw new ArgumentNullException(nameof(sourcePath));
            }
            if (string.IsNullOrWhiteSpace(destinationPath))
            {
                throw new ArgumentNullException(nameof(destinationPath));
            }

            var sourceFileInfo = _filesystem.GetFileInformation(sourcePath);
            if (!sourceFileInfo.Exists)
            {
                throw new ArgumentException($"The source file {sourcePath} does not exist");
            }
            return sourceFileInfo.Attributes != null
                && (sourceFileInfo.Attributes.Value & FileAttributes.Archive) == FileAttributes.Archive;
        }
    }
}
