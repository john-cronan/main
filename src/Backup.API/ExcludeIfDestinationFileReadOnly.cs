using JPC.Common;

namespace JPC.Backup
{
    internal class ExcludeIfDestinationFileReadOnly : IExcludeRule
    {
        private readonly IFilesystem _filesystem;

        public ExcludeIfDestinationFileReadOnly(IFilesystem filesystem)
        {
            if (filesystem == null)
            {
                throw new ArgumentNullException(nameof(filesystem));
            }

            _filesystem = filesystem;
        }

        string IExcludeRule.FriendlyName => "Destination read-only rule";

        bool IExcludeRule.ExcludeObject(string sourcePath, string destinationPath)
        {
            var destinationFileInfo = _filesystem.GetFileInformation(destinationPath);
            return destinationFileInfo.Exists 
                && (destinationFileInfo.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly;
        }
    }
}
