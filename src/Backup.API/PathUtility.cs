using JPC.Common;

namespace JPC.Backup
{
    internal static class PathUtility
    {
        public static string ComputeDestinationPath(IFilesystem filesystem, string sourceRoot, 
            string destinationRoot, string sourceDirectoryPath)
        {
            if (filesystem == null)
            {
                throw new ArgumentNullException(nameof(filesystem));
            }
            if (string.IsNullOrWhiteSpace(sourceRoot))
            {
                throw new ArgumentNullException(nameof(sourceRoot));
            }
            if (string.IsNullOrWhiteSpace(destinationRoot))
            {
                throw new ArgumentNullException(nameof(destinationRoot));
            }
            if (string.IsNullOrWhiteSpace(sourceDirectoryPath))
            {
                throw new ArgumentNullException(nameof(sourceDirectoryPath));
            }

            var sourceRootSplit = filesystem.SplitPath(sourceRoot);
            var destinationRootSplit = filesystem.SplitPath(destinationRoot);
            var sourceDirectoryPathSplit = filesystem.SplitPath(sourceDirectoryPath);
            if (sourceRootSplit.Length > sourceDirectoryPathSplit.Length)
            {
                throw new ArgumentException($"{nameof(sourceDirectoryPath)} is not a subdirectory of {nameof(sourceRoot)}");
            }
            var destinationSubDirectorySplit = destinationRootSplit.Concat(
                sourceDirectoryPathSplit.Skip(sourceRootSplit.Length)).ToArray();
            return filesystem.CombinePath(destinationSubDirectorySplit);
        }
    }
}
