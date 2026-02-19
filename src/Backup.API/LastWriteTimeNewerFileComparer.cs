using JPC.Common;

namespace JPC.Backup
{
    internal class LastWriteTimeNewerFileComparer : FileComparerBase
    {
        public LastWriteTimeNewerFileComparer(IFilesystem filesystem) 
            : base(filesystem)
        {
        }

        protected override bool ShouldCopy(FileInformation sourceFile,
            FileInformation destinationFile)
        {
            if (sourceFile.LastWrite == null)
            {
                return true;
            }
            if (destinationFile.LastWrite == null)
            {
                return true;
            }

            return sourceFile.LastWrite.Value > destinationFile.LastWrite.Value;
        }
    }
}
