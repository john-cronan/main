namespace JPC.Backup
{
    /// <summary>
    /// Defines members necessary to interact with file systems. 
    /// </summary>
    /// <remarks>
    /// <para>
    /// These operations are factored into an interface primarily to support
    /// WhatIf mode.
    /// </para>
    /// <para>
    /// The interface's members are designed to be asynchronous to give
    /// implementations <i>the option</i> of performing the operations
    /// asynchronously. This is mostly with possible future enhancements in
    /// mind. At the time of this writing, all operations are synchronous.
    /// </para>
    /// </remarks>
    internal interface IBackupFileOperations
    {
        /// <summary>
        /// Called after a successful file copy, to perform any actions
        /// necessary on either file at that time.
        /// </summary>
        /// <returns></returns>
        Task AfterCopyAsync(string source, string destination);

        /// <summary>
        /// Called to perform a file copy.
        /// </summary>
        Task CopyAsync(string source, string destination);

        /// <summary>
        /// Called to create a destination directory, if necessary.
        /// </summary>
        /// <returns>
        /// True if the directory was created, false if it already existed.
        /// </returns>
        Task<bool> EnsureDirectoryExistsAsync(string directoryPath);

        /// <summary>
        /// Enumerates the files in a specified directory.
        /// </summary>
        IEnumerable<string> EnumerateFiles(string inDirectory);
    }
}
