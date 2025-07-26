using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace JPC.Common
{
    public interface IFilesystem
    {
        public const int DefaultFileBufferSize = 4 * 1024;

        char AltDirectorySeparator { get; }
        char DirectorySeparator { get; }
        IEnvironment Environment { get; }


        void AppendAllText(string path, string contents);
        void AppendAllText(string path, string contents, Encoding textEncoding);
        void AppendLine(string pathAndFileName, string content);
        Task AppendLineAsync(string pathAndFileName, string content);
        string CombinePath(params string[] paths);
        void CopyFile(string sourcePathAndFileName, string destinationPathAndFileName, bool overwrite);
        void CreateDirectory(string directoryPath);
        void DeleteDirectory(string directoryPath, bool recursive);
        void DeleteFile(string pathAndFileName);
        bool DirectoryExists(string directoryPath);
        IEnumerable<string> EnumerateDirectories(string directoryPath, string searchPattern = "*",
            EnumerationOptions enumerationOptions = null);
        IEnumerable<string> EnumerateFiles(string directoryPath, string searchPattern = "*", 
            SearchOption searchOption = SearchOption.TopDirectoryOnly);
        bool FileExists(string pathAndFileName);
        string GetCurrentDirectory();
        string[] GetDirectories(string directoryPath);
        string[] GetDirectories(string directoryPath, string searchPattern);
        string[] GetDirectories(string directoryPath, string searchPattern, SearchOption searchOption);
        DirectoryInformation GetDirectoryInformation(string directoryPath);
        string GetDirectoryName(string directoryPath);
        string GetDirectoryRoot(string directoryPath);
        FileInformation GetFileInformation(string pathAndFileName);
        string GetFileName(string pathAndFileName);
        IEnumerable<string> GetFileNames(string directoryPath);
        string[] GetLogicalDrives();
        IEnumerable<string> GetSubdirectoryNames(string directoryPath);
        bool IsPathRooted(string path);
        void MoveDirectory(string sourcePath, string destinationPath);
        void MoveFile(string sourcePathAndFileName, string destinationPathAndFileName);
        byte[] ReadAllBytes(string pathAndFileName);
        Task<byte[]> ReadAllBytesAsync(string pathAndFileName);
        string[] ReadAllLines(string pathAndFileName);
        string[] ReadAllLines(string pathAndFileName, Encoding textEncoding);
        string ReadAllText(string pathAndFileName);
        string ReadAllText(string pathAndFileName, Encoding textEncoding);
        Task<string> ReadAllTextAsync(string pathAndFileName);
        void ReplaceFile(string sourceFileName, string destinationFileName, string destinationBackupFileName);
        void ReplaceFile(string sourceFileName, string destinationFileName, string destinationBackupFileName, bool ignoreMetadataErrors);
        void SetCurrentDirectory(string currentDirectory);
        void SetDirectoryInformation(DirectoryInformation directoryInformation);
        void SetFileInformation(FileInformation fileInformation);
        string[] SplitPath(string path);
        Task WriteAllBytesAsync(string pathAndFileName, byte[] bytes);
        void WriteAllBytes(string pathAndFileName, byte[] bytes);
        void WriteAllLines(string pathAndFileName, string[] lines);
        void WriteAllLines(string pathAndFileName, string[] lines, Encoding encoding);
        void WriteAllText(string pathAndFileName, string text);
        Task WriteAllTextAsync(string pathAndFileName, string contents);
    }
}
