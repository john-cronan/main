namespace JPC.Backup
{
    public interface IExcludeRule
    {
        string FriendlyName { get; }
        bool ExcludeObject(string sourcePath, string destinationPath);
    }
}
