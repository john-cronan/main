using JPC.Common;

namespace JPC.Backup
{
    internal class SizeIsDifferentFileComparer : FileComparerBase
    {
        public SizeIsDifferentFileComparer(IFilesystem filesystem)
            : base(filesystem)
        {
        }

        protected override bool ShouldCopy(FileInformation sourceFile, 
            FileInformation destinationFile)
        {
            if (sourceFile.Length == null || destinationFile.Length == null)
            {
                return true;
            }

            return sourceFile.Length != destinationFile.Length;
        }
    }
}
