using JPC.Common;

namespace JPC.Backup
{
    internal abstract class FileComparerBase : IFileComparer
    {
        private readonly IFilesystem _filesystem;

        public FileComparerBase(IFilesystem filesystem)
        {
            if (filesystem == null)
            {
                throw new ArgumentNullException(nameof(filesystem));
            }

            _filesystem = filesystem;
        }

        protected IFilesystem Filesystem => _filesystem;

        protected abstract bool ShouldCopy(FileInformation sourceFile,
            FileInformation destinationFile);

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
            var destinationFileInfo = _filesystem.GetFileInformation(destinationPath);
            if (!destinationFileInfo.Exists)
            {
                return true;
            }

            return ShouldCopy(sourceFileInfo, destinationFileInfo);
        }
    }
}
