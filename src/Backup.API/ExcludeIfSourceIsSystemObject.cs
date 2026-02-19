using JPC.Common;

namespace JPC.Backup
{
    internal class ExcludeIfSourceIsSystemObject : IExcludeRule
    {
        private readonly IFilesystem _filesystem;

        public ExcludeIfSourceIsSystemObject(IFilesystem filesystem)
        {
            if (filesystem == null)
            {
                throw new ArgumentNullException(nameof(filesystem));
            }

            _filesystem = filesystem;
        }

        string IExcludeRule.FriendlyName => "Exclude system objects rule";

        bool IExcludeRule.ExcludeObject(string sourcePath, string destinationPath)
        {
            var fileInfo = _filesystem.GetFileInformation(sourcePath);
            if (fileInfo != null && fileInfo.Exists)
            {
                return fileInfo.Exists &&  fileInfo.Attributes != null
                    && (fileInfo.Attributes & FileAttributes.System) == FileAttributes.System;
            }
            else
            {
                var directoryInfo = _filesystem.GetDirectoryInformation(sourcePath);
                if (directoryInfo != null &&  directoryInfo.Exists)
                {
                    return directoryInfo.Exists &&  directoryInfo.Attributes != null
                        && (directoryInfo.Attributes & FileAttributes.System) == FileAttributes.System;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}
