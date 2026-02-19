namespace JPC.Backup
{
    /// <summary>
    /// Repesents a strategy for determining if a particular file has changed
    /// and needs to be backed up.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Not all implementations actually compare the two files. Some decide based 
    /// on the state/existence of one or the other.
    /// </para>
    /// </remarks>
    public interface IFileComparer
    {
        bool ShouldCopy(string sourcePath, string destinationPath);
    }
}
