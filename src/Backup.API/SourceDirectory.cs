namespace JPC.Backup
{
    public class SourceDirectory
    {
        private readonly string _path;
        private readonly BackupOptions _options;

        public SourceDirectory(string path, BackupOptions options)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
            }
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _path = path;
            _options = options;
        }

        public string Path => _path;
        public BackupOptions Options => _options;
    }
}
