using System;
using System.IO;

namespace JPC.Common
{
    public class FileInformation
    {
        private FileAttributes? _attributes;
        private DateTimeOffset? _created;
        private readonly string _directoryPath;
        private readonly string _name;
        private readonly bool _exists;
        private DateTimeOffset? _lastAccessed;
        private DateTimeOffset? _lastWrite;

        public FileInformation(string directoryPath, string name, bool exists, FileAttributes? attributes = null,
            DateTimeOffset? created = null, DateTimeOffset? lastAccessed = null, DateTimeOffset? lastWrite = null)
        {
            _directoryPath = directoryPath;
            _name = name;
            _exists = exists;
            Attributes = attributes;
            Created = created;
            LastAccessed = lastAccessed;
            LastWrite = lastWrite;
        }

        public bool IsReadOnly => 
            Attributes.HasValue
                ? ((Attributes.Value & FileAttributes.ReadOnly) == FileAttributes.ReadOnly) 
            : throw new FileNotFoundException("File not found");

        public bool IsHidden =>
            Attributes.HasValue
                ? ((Attributes.Value & FileAttributes.ReadOnly) == FileAttributes.Hidden)
                : throw new FileNotFoundException("File not found");

        public bool IsSystem =>
            Attributes.HasValue
                ? ((Attributes.Value & FileAttributes.System) == FileAttributes.System)
                : throw new FileNotFoundException("File not found");

        public FileAttributes? Attributes { get => _attributes; set => _attributes = value; }
        public DateTimeOffset? Created { get => _created; set => _created = value; }
        public string DirectoryPath => _directoryPath;
        public string Name => _name;
        public bool Exists => _exists;
        public DateTimeOffset? LastAccessed { get => _lastAccessed; set => _lastAccessed = value; }
        public DateTimeOffset? LastWrite { get => _lastWrite; set => _lastWrite = value; }
    }

    //public class FileInformation
    //{
    //    private readonly DateTimeOffset _created;
    //    private readonly string _directoryName;
    //    private readonly string _filePathAndName;
    //    private readonly bool _exists;
    //    private readonly bool _isReadOnly;

    //    public FileInformation(FileInfo fileInfo)
    //    {
    //        _created = fileInfo.CreationTime;
    //        _directoryName = fileInfo.DirectoryName;
    //        _filePathAndName = fileInfo.FullName;
    //        _exists = fileInfo.Exists;
    //        _isReadOnly = fileInfo.IsReadOnly;
    //    }

    //    public DateTimeOffset Created => _created;
    //    public string DirectoryName => _directoryName;
    //    public string FilePathAndName => _filePathAndName;
    //    public bool Exists => _exists;
    //    public bool IsReadOnly => _isReadOnly;
    //}
}
