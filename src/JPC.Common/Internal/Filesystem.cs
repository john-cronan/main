using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace JPC.Common.Internal
{
    internal class Filesystem : IFilesystem
    {
        private readonly char _altDirectorySeparatorChar;
        private readonly char _directorySeparatorChar;
        private readonly IEnvironment _environment;
        private readonly IFilesystem _this;
        private readonly char _volumeSeparatorChar;

        char IFilesystem.AltDirectorySeparator => _altDirectorySeparatorChar;
        char IFilesystem.DirectorySeparator => _directorySeparatorChar;
        IEnvironment IFilesystem.Environment => _environment;


        public Filesystem(IEnvironment environment)
        {
            if (environment.OperatingSystem == OperatingSystem.Windows)
            {
                _directorySeparatorChar = '\\';
                _altDirectorySeparatorChar = '/';
                _volumeSeparatorChar = ':';
            }
            else if (environment.OperatingSystem == OperatingSystem.Linux)
            {
                _directorySeparatorChar = '/';
                _altDirectorySeparatorChar = _directorySeparatorChar;
                _volumeSeparatorChar = '/';
            }
            else
            {
                throw new NotSupportedException($"{environment.OperatingSystem.ToString()} not supported");
            }
            _environment = environment;
            _this = this;
        }

        void IFilesystem.AppendAllText(string pathAndFileName, string contents)
        {
            var pathAndFileNameExpanded = Expand(pathAndFileName);
            _this.CreateDirectory(_this.GetDirectoryName(pathAndFileNameExpanded));
            File.AppendAllText(pathAndFileNameExpanded, contents);
        }

        void IFilesystem.AppendAllText(string pathAndFileName, string contents, Encoding textEncoding)
        {
            var pathAndFileNameExpanded = Expand(pathAndFileName);
            Directory.CreateDirectory(Path.GetDirectoryName(pathAndFileNameExpanded));
            File.AppendAllText(pathAndFileNameExpanded, contents, textEncoding);
        }

        void IFilesystem.AppendLine(string pathAndFileName, string content)
        {
            var pathAndFileNameExpanded = Expand(pathAndFileName);
            Directory.CreateDirectory(Path.GetDirectoryName(pathAndFileNameExpanded));
            File.AppendAllLines(pathAndFileNameExpanded, new string[] { content });
        }

        Task IFilesystem.AppendLineAsync(string pathAndFileName, string content)
        {
            var pathAndFileNameExpanded = Expand(pathAndFileName);
            Directory.CreateDirectory(Path.GetDirectoryName(pathAndFileNameExpanded));
            return File.AppendAllLinesAsync(pathAndFileNameExpanded, new string[] { content });
        }

        string IFilesystem.CombinePath(params string[] paths)
        {
            return Path.Combine(paths);
        }

        void IFilesystem.CopyFile(string sourcePathAndFileName, string destinationPathAndFileName, bool overwrite)
        {
            var sourceExpanded = Expand(sourcePathAndFileName);
            var destinationExpanded = Expand(destinationPathAndFileName);
            Directory.CreateDirectory(Path.GetDirectoryName(sourcePathAndFileName));
            File.Copy(sourceExpanded, destinationExpanded, overwrite);
        }

        void IFilesystem.CreateDirectory(string directoryPath)
        {
            var directoryPathExpanded = Expand(directoryPath);
            Directory.CreateDirectory(directoryPathExpanded);
        }

        void IFilesystem.DeleteDirectory(string directoryPath, bool recursive)
        {
            var directoryPathExpanded = Expand(directoryPath);
            Directory.Delete(directoryPathExpanded, recursive);
        }

        void IFilesystem.DeleteFile(string pathAndFileName)
        {
            var pathAndFileNameExpanded = Expand(pathAndFileName);
            File.Delete(pathAndFileNameExpanded);
        }

        bool IFilesystem.DirectoryExists(string directoryPath)
        {
            var directoryPathExpanded = Expand(directoryPath);
            return Directory.Exists(directoryPathExpanded);
        }

        IEnumerable<string> IFilesystem.EnumerateDirectories(string directoryPath, string searchPattern, 
            EnumerationOptions enumerationOptions)
        {
            var directoryPathExpanded = Expand(directoryPath);
            return Directory.EnumerateDirectories(directoryPathExpanded, searchPattern, enumerationOptions);
        }

        IEnumerable<string> IFilesystem.EnumerateFiles(string directoryPath, string searchPattern, SearchOption searchOption)
        {
            var directoryPathExpanded = Expand(directoryPath);
            return Directory.EnumerateFiles(directoryPathExpanded, searchPattern, searchOption);
        }

        bool IFilesystem.FileExists(string pathAndFileName)
        {
            var pathAndFileNameExpanded = Expand(pathAndFileName);
            return File.Exists(pathAndFileNameExpanded);
        }

        string IFilesystem.GetCurrentDirectory()
        {
            return Directory.GetCurrentDirectory();
        }

        string[] IFilesystem.GetDirectories(string directoryPath)
        {
            var directoryPathExpanded = Expand(directoryPath);
            return Directory.GetDirectories(directoryPathExpanded);
        }

        string[] IFilesystem.GetDirectories(string directoryPath, string searchPattern)
        {
            var directoryPathExpanded = Expand(directoryPath);
            var searchPatternExpanded = Expand(searchPattern);
            return Directory.GetDirectories(directoryPathExpanded, searchPatternExpanded);
        }

        string[] IFilesystem.GetDirectories(string directoryPath, string searchPattern, SearchOption searchOption)
        {
            var directoryPathExpanded = Expand(directoryPath);
            var searchPatternExpanded = Expand(searchPattern);
            return Directory.GetDirectories(directoryPathExpanded, searchPatternExpanded, searchOption);
        }

        DirectoryInformation IFilesystem.GetDirectoryInformation(string directoryPath)
        {
            var directoryPathExpanded = Expand(directoryPath);
            var isEmpty = !Directory.GetFileSystemEntries(directoryPathExpanded).Any();
            var createdTime = new DateTimeOffset(Directory.GetCreationTimeUtc(directoryPath), TimeSpan.Zero);
            var lastAccessedTime = new DateTimeOffset(Directory.GetLastAccessTimeUtc(directoryPathExpanded), TimeSpan.Zero);
            var lastWriteTime = new DateTimeOffset(Directory.GetLastWriteTimeUtc(directoryPathExpanded), TimeSpan.Zero);
            return new DirectoryInformation(directoryPath, isEmpty, created: createdTime, lastAccessed: lastAccessedTime,
                lastWrite: lastWriteTime);
        }

        string IFilesystem.GetDirectoryName(string directoryPath)
        {
            var directoryPathExpanded = Expand(directoryPath);
            return Path.GetDirectoryName(directoryPathExpanded);
        }

        string IFilesystem.GetDirectoryRoot(string directoryPath)
        {
            var directoryPathExpanded = Expand(directoryPath);
            return Directory.GetDirectoryRoot(directoryPathExpanded);
        }

        FileInformation IFilesystem.GetFileInformation(string pathAndFileName)
        {
            var pathAndFileNameExpanded = Expand(pathAndFileName);
            var path = Path.GetDirectoryName(pathAndFileNameExpanded);
            var fileName = Path.GetFileName(pathAndFileNameExpanded);
            var exists = File.Exists(pathAndFileNameExpanded);
            if (!exists)
            {
                return new FileInformation(path, fileName, exists);
            }
            var attributes = File.GetAttributes(pathAndFileNameExpanded);
            var created = File.GetCreationTime(pathAndFileNameExpanded);
            var lastAccessed = File.GetLastAccessTime(pathAndFileNameExpanded);
            var lastWrite = File.GetLastWriteTime(pathAndFileNameExpanded);
            return new FileInformation(path, fileName, exists, attributes, created, lastAccessed, lastWrite);
        }

        string IFilesystem.GetFileName(string pathAndFileName)
        {
            var pathAndFileNameExpanded = Expand(pathAndFileName);
            return Path.GetFileName(pathAndFileNameExpanded);
        }

        IEnumerable<string> IFilesystem.GetFileNames(string directoryPath)
        {
            var directoryPathExpanded = Expand(directoryPath);
            return Directory.EnumerateFiles(directoryPathExpanded);
        }

        string[] IFilesystem.GetLogicalDrives()
        {
            return Directory.GetLogicalDrives();
        }

        IEnumerable<string> IFilesystem.GetSubdirectoryNames(string directoryPath)
        {
            var directoryPathExpanded = Expand(directoryPath);
            return Directory.GetDirectories(directoryPathExpanded);
        }

        bool IFilesystem.IsPathRooted(string path)
        {
            var pathExpanded = Expand(path);
            return Path.IsPathRooted(pathExpanded);
        }

        void IFilesystem.MoveDirectory(string sourcePath, string destinationPath)
        {
            var sourcePathExpanded = Expand(sourcePath);
            var destinationPathExpanded = Expand(destinationPath);
            Directory.Move(sourcePathExpanded, destinationPathExpanded);
        }

        void IFilesystem.MoveFile(string sourcePathAndFileName, string destinationPathAndFileName)
        {
            var sourcePathAndFileNameExpanded = Expand(sourcePathAndFileName);
            var destinationPathAndFileNameExpanded = Expand(destinationPathAndFileName);
            Directory.CreateDirectory(Path.GetDirectoryName(destinationPathAndFileNameExpanded));
            File.Move(sourcePathAndFileNameExpanded, destinationPathAndFileNameExpanded);
        }

        byte[] IFilesystem.ReadAllBytes(string pathAndFileName)
        {
            var pathAndFileNameExpanded = Expand(pathAndFileName);
            return File.ReadAllBytes(pathAndFileNameExpanded);
        }

        Task<byte[]> IFilesystem.ReadAllBytesAsync(string pathAndFileName)
        {
            var pathAndFileNameExpanded = Expand(pathAndFileName);
            return File.ReadAllBytesAsync(pathAndFileNameExpanded);
        }

        string[] IFilesystem.ReadAllLines(string pathAndFileName)
        {
            var pathAndFileNameExpanded = Expand(pathAndFileName);
            return File.ReadAllLines(pathAndFileNameExpanded);
        }

        string[] IFilesystem.ReadAllLines(string pathAndFileName, Encoding textEncoding)
        {
            var pathAndFileNameExpanded = Expand(pathAndFileName);
            return File.ReadAllLines(pathAndFileNameExpanded, textEncoding);
        }

        string IFilesystem.ReadAllText(string pathAndFileName)
        {
            var pathAndFileNameExpanded = Expand(pathAndFileName);
            return File.ReadAllText(pathAndFileNameExpanded);
        }

        string IFilesystem.ReadAllText(string pathAndFileName, Encoding textEncoding)
        {
            var pathAndFileNameExpanded = Expand(pathAndFileName);
            return File.ReadAllText(pathAndFileNameExpanded, textEncoding);
        }

        Task<string> IFilesystem.ReadAllTextAsync(string pathAndFileName)
        {
            var pathAndFileNameExpanded = Expand(pathAndFileName);
            return File.ReadAllTextAsync(pathAndFileNameExpanded);
        }

        void IFilesystem.ReplaceFile(string sourcePathAndFileName, string destinationPathAndFileName, string destinationBackupPathAndFileName)
        {
            var sourcePathAndFileNameExpanded = Expand(sourcePathAndFileName);
            var destinationPathAndFileNameExpanded = Expand(destinationPathAndFileName);
            var destinationBackupPathAndFileNameExpanded = Expand(destinationBackupPathAndFileName);
            Directory.CreateDirectory(Path.GetDirectoryName(destinationPathAndFileNameExpanded));
            Directory.CreateDirectory(Path.GetDirectoryName(destinationBackupPathAndFileNameExpanded));
            File.Replace(sourcePathAndFileNameExpanded, destinationPathAndFileNameExpanded, destinationBackupPathAndFileNameExpanded);
        }

        void IFilesystem.ReplaceFile(string sourcePathAndFileName, string destinationPathAndFileName, string destinationBackupPathAndFileName,
            bool ignoreMetadataErrors)
        {
            var sourcePathAndFileNameExpanded = Expand(sourcePathAndFileName);
            var destinationPathAndFileNameExpanded = Expand(destinationPathAndFileName);
            var destinationBackupPathAndFileNameExpanded = Expand(destinationBackupPathAndFileName);
            Directory.CreateDirectory(Path.GetDirectoryName(destinationPathAndFileNameExpanded));
            Directory.CreateDirectory(Path.GetDirectoryName(destinationBackupPathAndFileNameExpanded));
            File.Replace(sourcePathAndFileNameExpanded, destinationPathAndFileNameExpanded,
                destinationBackupPathAndFileNameExpanded, ignoreMetadataErrors);
        }

        void IFilesystem.SetCurrentDirectory(string currentDirectory)
        {
            var currentDirectoryExpanded = Expand(currentDirectory);
            Directory.SetCurrentDirectory(currentDirectoryExpanded);
        }

        void IFilesystem.SetDirectoryInformation(DirectoryInformation directoryInformation)
        {
            if (directoryInformation.Created.HasValue)
            {
                Directory.SetCreationTimeUtc(directoryInformation.Path, directoryInformation.Created.Value.DateTime);
            }
            if (directoryInformation.LastAccessed.HasValue)
            {
                Directory.SetLastAccessTimeUtc(directoryInformation.Path, directoryInformation.LastAccessed.Value.DateTime);
            }
            if (directoryInformation.LastWriteTime.HasValue)
            {
                Directory.SetLastWriteTimeUtc(directoryInformation.Path, directoryInformation.LastWriteTime.Value.DateTime);
            }
        }

        void IFilesystem.SetFileInformation(FileInformation fileInformation)
        {
            var filePathAndName = Path.Combine(fileInformation.DirectoryPath, fileInformation.Name);
            if (fileInformation.Attributes.HasValue)
            {
                File.SetAttributes(filePathAndName, fileInformation.Attributes.Value);
            }
            if (fileInformation.Created.HasValue)
            {
                File.SetCreationTimeUtc(filePathAndName, fileInformation.Created.Value.UtcDateTime);
            }
            if (fileInformation.LastAccessed.HasValue)
            {
                File.SetLastAccessTimeUtc(filePathAndName, fileInformation.LastAccessed.Value.UtcDateTime);
            }
            if (fileInformation.LastWrite.HasValue)
            {
                File.SetLastWriteTimeUtc(filePathAndName, fileInformation.LastWrite.Value.UtcDateTime);
            }
        }

        string[] IFilesystem.SplitPath(string path)
        {
            //
            //TODO: This method doesn't currently support UNC paths.
            //
            var pathExpanded = Expand(path);
            var arrayBuilder = new List<string>();
            PathNode[] nodes;
            if (_environment.OperatingSystem == OperatingSystem.Windows)
            {
                nodes = ParsePathWindows(pathExpanded).ToArray();
            }
            else if (_environment.OperatingSystem == OperatingSystem.Linux)
            {
                nodes = ParsePathLinux(pathExpanded).ToArray();
            }
            else
            {
                throw new NotSupportedException($"{_environment.OperatingSystem.ToString()} not supported");
            }
            if (nodes[0].NodeType == PathNodeType.VolumeName && nodes.Count() > 1 && nodes[1].NodeType == PathNodeType.Separator)
            {
                arrayBuilder.Add(nodes[0].Text + nodes[1].Text);
            }
            else
            {
                arrayBuilder.Add(nodes[0].Text);
            }
            arrayBuilder.AddRange(nodes.Where((n, i) => n.Text != _directorySeparatorChar.ToString() && n.Text != _altDirectorySeparatorChar.ToString()
                && i > 0).Select(n => n.Text));
            return arrayBuilder.ToArray();
        }

        private IEnumerable<PathNode> ParsePathWindows(string path)
        {
            var effectivePath = path.Trim();
            var str = new StringBuilder();
            var volumeSeparatorRead = false;
            for (int i = 0; i < effectivePath.Length; i++)
            {
                var chr = effectivePath[i];
                if (chr == _volumeSeparatorChar && !volumeSeparatorRead)
                {
                    str.Append(chr);
                    yield return new PathNode(PathNodeType.VolumeName, str.ToString());
                    str.Clear();
                    volumeSeparatorRead = true;
                }
                else if ((chr == _directorySeparatorChar || chr == _altDirectorySeparatorChar) && i == 0)
                {
                    str.Append(chr);
                    yield return new PathNode(PathNodeType.Separator, str.ToString());
                    str.Clear();
                }
                else if ((chr == _directorySeparatorChar || chr == _altDirectorySeparatorChar) && i > 0)
                {
                    if (str.Length > 0)
                    {
                        yield return new PathNode(PathNodeType.Branch, str.ToString());
                    }
                    yield return new PathNode(PathNodeType.Separator, new string(chr, 1));
                    str.Clear();
                }
                else
                {
                    str.Append(chr);
                }
            }
            if (str.Length > 0)
            {
                yield return new PathNode(PathNodeType.Branch, str.ToString());
                str.Clear();
            }
        }

        private IEnumerable<PathNode> ParsePathLinux(string path)
        {
            var effectivePath = path.Trim();
            var str = new StringBuilder();
            var i = 0;
            if (path[i] == _directorySeparatorChar || path[i] == _altDirectorySeparatorChar)
            {
                yield return new PathNode(PathNodeType.VolumeName, "/");
                i++;
            }
            for (; i < effectivePath.Length; i++)
            {
                var chr = effectivePath[i];
                if (chr == _volumeSeparatorChar || chr == _altDirectorySeparatorChar)
                {
                    if (str.Length > 0)
                    {
                        yield return new PathNode(PathNodeType.Branch, str.ToString());
                    }
                    yield return new PathNode(PathNodeType.Separator, new string(chr, 1));
                    str.Clear();
                }
                else
                {
                    str.Append(chr);
                }
            }
            if (str.Length > 0)
            {
                yield return new PathNode(PathNodeType.Branch, str.ToString());
                str.Clear();
            }
        }

        void IFilesystem.WriteAllBytes(string pathAndFileName, byte[] bytes)
        {
            var pathAndFileNameExpanded = Expand(pathAndFileName);
            Directory.CreateDirectory(Path.GetDirectoryName(pathAndFileNameExpanded));
            File.WriteAllBytes(pathAndFileNameExpanded, bytes);
        }

        Task IFilesystem.WriteAllBytesAsync(string pathAndFileName, byte[] bytes)
        {
            var pathAndFileNameExpanded = Expand(pathAndFileName);
            var directoryName = Path.GetDirectoryName(pathAndFileNameExpanded);
            Directory.CreateDirectory(directoryName);
            return File.WriteAllBytesAsync(pathAndFileNameExpanded, bytes);
        }

        void IFilesystem.WriteAllLines(string pathAndFileName, string[] lines)
        {
            var pathAndFileNameExpanded = Expand(pathAndFileName);
            Directory.CreateDirectory(Path.GetDirectoryName(pathAndFileNameExpanded));
            File.WriteAllLines(pathAndFileNameExpanded, lines);
        }

        void IFilesystem.WriteAllLines(string pathAndFileName, string[] lines, Encoding encoding)
        {
            var pathAndFileNameExpanded = Expand(pathAndFileName);
            Directory.CreateDirectory(Path.GetDirectoryName(pathAndFileNameExpanded));
            File.WriteAllLines(pathAndFileNameExpanded, lines, encoding);
        }

        void IFilesystem.WriteAllText(string pathAndFileName, string text)
        {
            var pathAndFileNameExpanded = Expand(pathAndFileName);
            Directory.CreateDirectory(Path.GetDirectoryName(pathAndFileNameExpanded));
            File.WriteAllText(pathAndFileNameExpanded, text);
        }

        Task IFilesystem.WriteAllTextAsync(string pathAndFileName, string contents)
        {
            var pathAndFileNameExpanded = Expand(pathAndFileName);
            Directory.CreateDirectory(Path.GetDirectoryName(pathAndFileNameExpanded));
            return File.WriteAllTextAsync(pathAndFileNameExpanded, contents);
        }


        private string Expand(string path)
        {
            return _environment.ExpandEnvironmentVariables(path);
        }
    }
}
