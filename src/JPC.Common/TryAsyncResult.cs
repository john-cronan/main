namespace JPC.Common
{
    public struct TryAsyncResult<T>
    {
        private readonly bool _isSuccessful;
        private readonly T _result;

        public TryAsyncResult(bool isSuccessful, T result)
        {
            _isSuccessful = isSuccessful;
            _result = result;
        }

        public bool IsSuccessful => _isSuccessful;
        public T Result => _result;
    }
}
