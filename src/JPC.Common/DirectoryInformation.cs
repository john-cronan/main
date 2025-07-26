using System;
using System.IO;

namespace JPC.Common
{
    public class DirectoryInformation
    {
        private DateTimeOffset? _created;
        private DateTimeOffset? _lastAccessed;
        private DateTimeOffset? _lastWriteTime;
        private bool _isEmpty;
        private readonly string _path;

        public DirectoryInformation(string path, bool isEmpty, DateTimeOffset? created = null, DateTimeOffset? lastAccessed = null,
            DateTimeOffset? lastWrite = null)
        {
            _path = path;
            _isEmpty = isEmpty;
            _created = created;
            _lastAccessed = lastAccessed;
            _lastWriteTime = lastWrite;
        }

        public DateTimeOffset? Created { get => _created; set => _created = value; }
        public DateTimeOffset? LastAccessed { get => _lastAccessed; set => _lastAccessed = value; }
        public DateTimeOffset? LastWriteTime { get => _lastWriteTime; set => _lastWriteTime = value; }
        public bool IsEmpty { get => _isEmpty; set => _isEmpty = value; }
        public string Path => _path;
    }
}
