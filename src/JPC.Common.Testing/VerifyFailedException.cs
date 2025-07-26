namespace JPC.Common.Testing
{
    public class VerifyFailedException : Exception
    {
        public VerifyFailedException(string message) 
            : base(message)
        {
        }
    }
}
