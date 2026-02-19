namespace JPC.Backup
{
    [Serializable]
    public abstract class BackupException : Exception
    {
        protected BackupException(string message)
            : base(message)
        {
        }
    }
}
