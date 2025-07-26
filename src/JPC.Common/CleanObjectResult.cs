using System;

namespace JPC.Common
{
    public class CleanObjectResult
    {
        private readonly string _path;
        private readonly bool _directory;
        private readonly bool _success;
        private readonly Exception _exception;
        private readonly TimeSpan? _elapsed;

        public CleanObjectResult(string path, bool directory, bool success, Exception exception, TimeSpan? elapsed)
        {
            _path = path;
            _directory = directory;
            _success = success;
            _exception = exception;
            _elapsed = elapsed;
        }

        public string Path => _path;
        public bool Success => _success;
        public Exception Exception => _exception;
        public TimeSpan? Elapsed => _elapsed;
        public bool Directory => _directory;

        public void ThrowIfFailed()
        {
            if (_exception != null)
            {
                throw new AggregateException(_exception);
            }
        }
    }
}
