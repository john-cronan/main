using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace JC.CommandLine
{
    internal interface IFilesystem
    {
        bool FileExists(string path);
        bool DirectoryExists(string path);
        string ReadAllText(string path);
        string[] ReadAllLines(string path);
        byte[] ReadAllBytes(string path);
        //bool IsPathFullyQualified(string path);
        bool IsPathRooted(string path);
        string GetPathRoot(string path);
        string JoinPath(string path1, string path2);
        string CombinePath(string path1, string path2);
        IEnumerable<string> GetFileSystemEntryNames(string directory);
        IEnumerable<FileSystemInfo> GetFileSystemEntries(string directory);
        string MakePathFullyQualified(string path);
    }

    [ExcludeFromCodeCoverage]
    internal class Filesystem : IFilesystem
    {
        private readonly IFilesystem _self;

        public Filesystem()
        {
            _self = this;
        }

        bool IFilesystem.DirectoryExists(string path)
        {
            Guard.IsNotNullOrWhitespace(path, nameof(path));

            return Directory.Exists(path);
        }

        bool IFilesystem.FileExists(string path)
        {
            Guard.IsNotNullOrWhitespace(path, nameof(path));

            return File.Exists(path);
        }

        string IFilesystem.ReadAllText(string path)
        {
            Guard.IsNotNullOrWhitespace(path, nameof(path));

            return File.ReadAllText(path);
        }

        string[] IFilesystem.ReadAllLines(string path)
        {
            Guard.IsNotNullOrWhitespace(path, nameof(path));

            return File.ReadAllLines(path);
        }

        byte[] IFilesystem.ReadAllBytes(string path)
        {
            Guard.IsNotNullOrWhitespace(path, nameof(path));

            return File.ReadAllBytes(path);
        }

        //bool IFilesystem.IsPathFullyQualified(string path)
        //{
        //    Guard.IsNotNullOrWhitespace(path, nameof(path));

        //    return Path.IsPathFullyQualified(path);
        //}

        bool IFilesystem.IsPathRooted(string path)
        {
            Guard.IsNotNullOrWhitespace(path, nameof(path));

            return Path.IsPathRooted(path);
        }

        string IFilesystem.GetPathRoot(string path)
        {
            Guard.IsNotNullOrWhitespace(path, nameof(path));

            return Path.GetPathRoot(path);
        }

        string IFilesystem.JoinPath(string path1, string path2)
        {
            Guard.IsNotNullOrWhitespace(path1, nameof(path1));
            Guard.IsNotNullOrWhitespace(path2, nameof(path2));

            return Path.Combine(path1, path2);
        }

        string IFilesystem.CombinePath(string path1, string path2)
        {
            Guard.IsNotNullOrWhitespace(path1, nameof(path2));
            Guard.IsNotNullOrWhitespace(path1, nameof(path2));

            return Path.Combine(path1, path2);
        }

        IEnumerable<string> IFilesystem.GetFileSystemEntryNames(string directory)
        {
            Guard.IsNotNullOrWhitespace(directory, nameof(directory));

            return Directory.GetFileSystemEntries(directory);
        }

        IEnumerable<FileSystemInfo> IFilesystem.GetFileSystemEntries(string directory)
        {
            Guard.IsNotNullOrWhitespace(directory, nameof(directory));

            foreach (var item in _self.GetFileSystemEntryNames(directory))
            {
                if (_self.DirectoryExists(item))
                {
                    yield return new DirectoryInfo(item);
                }
                else if (_self.FileExists(item))
                {
                    yield return new FileInfo(item);
                }
            }
        }

        string IFilesystem.MakePathFullyQualified(string path)
        {
            if (_self.IsPathRooted(path))
            {
                return path;
            }
            else
            {
                return Path.Combine(Environment.CurrentDirectory, path);
            }
        }

        //string IFilesystem.MakePathFullyQualified(string path)
        //{
        //    if (_self.IsPathFullyQualified(path))
        //    {
        //        return path;
        //    }
        //    else
        //    {
        //        return Path.Combine(Environment.CurrentDirectory, path);
        //    }
        //}
    }
}
