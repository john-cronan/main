using JPC.Common;

namespace JPC.Backup
{
    internal class ExcludeIfFileOverSize : IExcludeRule
    {
        private readonly FileSize _criticalSize;
        private readonly IFilesystem _filesystem;

        public ExcludeIfFileOverSize(IFilesystem filesystem, FileSize criticalSize)
        {
            if (filesystem == null)
            {
                throw new ArgumentNullException(nameof(filesystem));
            }

            _filesystem = filesystem;
            _criticalSize = criticalSize;    
        }

        public FileSize CriticalSize => _criticalSize;

        string IExcludeRule.FriendlyName => "File size exclude rule";

        bool IExcludeRule.ExcludeObject(string sourcePath, string destinationPath)
        {
            var fileInfo = _filesystem.GetFileInformation(sourcePath);
            var actualSize = FileSize.From(fileInfo.Length.Value, FileSizeUnits.Bytes);
            return actualSize > _criticalSize;
        }
    }
}
