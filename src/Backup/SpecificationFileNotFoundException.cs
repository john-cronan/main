namespace JPC.Backup
{
    [Serializable]
    internal class SpecificationFileNotFoundException : BackupException
    {
        public SpecificationFileNotFoundException(string message)
            : base(message)
        {
        }
    }
}
