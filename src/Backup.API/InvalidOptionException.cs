namespace JPC.Backup
{
    public class InvalidOptionException : BackupException
    {
        public InvalidOptionException(string msg)
            : base(msg)
        {
        }
    }
}
