using JPC.Common;

namespace JPC.Backup
{
    internal enum OutputLevel
    {
        None,
        Error,
        Warning,
        Information,
        Verbose
    }

    internal class MutableMatchExpression
    {
        public string Expression { get; set; }
        public MatchType MatchType { get; set; }
    }

    internal class SpecificationFile
    {
        /// <summary>
        /// Gets / Sets the path of the files being backed up. If neither
        /// this property nor <see cref="SourceVolume"/> is specified,
        /// the directory containing the specification file is used. Both
        /// cannot be specified. The directory identified by this property 
        /// or <see cref="SourceVolume"/> must exist.
        /// </summary>
        public string SourcePath { get; set; }

        /// <summary>
        /// Gets / Sets the label of a volume whose root directory will be
        /// backed up.If neither this property nor <see cref="SourcePath"/> is
        /// specified, the directory containing the specification file is used. 
        /// Both cannot be specified.  The  directory identified by this property 
        /// or <see cref="SourcePath"/> must exist.
        /// </summary>
        public string SourceVolume { get; set; }

        /// <summary>
        /// Gets / Sets the path that backed up files will be copied to. This
        /// directory needn't exist (it will be created if it doesn't), but
        /// it must, of course, reside on a root that exists. Either this
        /// value or <see cref="DestinationVolume"/> is required.
        /// </summary>
        public string DestinationPath { get; set; }

        /// <summary>
        /// Gets / Sets the label of a volume whose root directory backed up files
        /// will be copied to. Either this value or <see cref="DestinationPath"/>
        /// is required.
        /// </summary>
        public string DestinationVolume { get; set; }

        /// <summary>
        /// Gets / Sets a maximum size for files backed up. If set, files larger
        /// than this value will not be backed up. If not set, all files will be
        /// backed up.
        /// </summary>
        public FileSize? MaxFileSize { get; set; }

        /// <summary>
        /// Gets / Sets a flag indicating if files bearing the System attribute
        /// will be backed up. If not specified, defaults to false.
        /// </summary>
        public bool? CopySystemFiles { get; set; }

        /// <summary>
        /// Gets / Sets a value indicating what method is used to determine a
        /// source file should be copied.
        /// </summary>
        public FileComparisonMethod ComparisonMethod { get; set; }

        /// <summary>
        /// Gets / Sets a flag indicating whether destination files bearing 
        /// the ReadOnly attribute will be overwritten. If not specified, 
        /// defaults to true.
        /// </summary>
        public bool? OverwriteReadOnlyFiles { get; set; }

        /// <summary>
        /// Gets / Sets an object representing a log file. If absent, no log 
        /// file will be written.
        /// </summary>
        public LogFileSettings LogFile { get; set; }
        
        /// <summary>
        /// Gets / Sets a maximum depth for recursion of source directories. 
        /// If not specified, all directories are processed recursively. If
        /// zero, only files in the directory identified by 
        /// <see cref="SourcePath" /> will be processed, with no recursion.
        /// </summary>
        public int? MaxDepth { get; set; }
        
        /// <summary>
        /// Gets / Sets the maximum number of times a transiently-failed file
        /// will be retried before it is failed. If not specified, defaults to
        /// one.
        /// </summary>
        public int? MaxRetriesOnFailure { get; set; }

        /// <summary>
        /// Gets / Sets a value governing how much output is written to the
        /// console. Defaults to <see cref="LogLevel.Information"/>.
        /// </summary>
        public OutputLevel? OutputLevel { get; set; }

        /// <summary>
        /// Gets / Sets a flag determining whether the Archive attribute is
        /// cleared after a file is backed up. If not specified, defafults 
        /// to true.
        /// </summary>
        public bool? ResetArchiveBit { get; set; }
        
        /// <summary>
        /// Gets / Sets a value determining the minimum amount of time before
        /// a transiently-failed file is retried. Retries may occur any time
        /// after this delay. If not specified, defaults to Zero, indicating
        /// that the file may be retried immediately.
        /// </summary>
        public TimeSpan? RetryDelay { get; set; }
        
        /// <summary>
        /// Gets / Sets a collection specifying expressions that, if matched,
        /// will cause a file to be skipped.
        /// </summary>
        public MutableMatchExpression[] ExcludeFilesMatching { get; set; }

        /// <summary>
        /// Gets / Sets a flag indicating whether the application should ignore
        /// directories that have colons in their names. If true, files in such
        /// directories are not copied and subdirectories are not recursed.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Inexplicably, this special case does happen. Visual Studio, for
        /// one, creates directories in the TestResults directory named, 
        /// for example, "Deploy_{user name} 2020-06-18 20:34:27". These directories
        /// lead to failures, and the application cannot copy them anyway (most
        /// other utilities are unable to as well), so at least on some platforms 
        /// they must be ignored.
        /// </para>
        /// </remarks>
        public bool? StopWhenDirectoryNameHasColon { get; set; }

        /// <summary>
        /// Gets / Sets a collection specifying expressions that, if matched,
        /// will cause the application to not process the current directory
        /// and not recurse any of its subdirectories. The application will
        /// continue with the next sibling directory.
        /// </summary>
        public MutableMatchExpression[] StopWhenDirectoryMatches { get; set; }
        
        /// <summary>
        /// Gets / Sets a flag indicating if "What-If" mode is activated. In
        /// What-If mode, no writing file system operations are performed. If
        /// not specified, defaults to false.
        /// </summary>
        public bool? WhatIf { get; set; }
    }

    internal class LogFileSettings
    {
        /// <summary>
        /// Gets / Sets the name of the log file output. The value should be
        /// a file name only, with no path. The file is always placed in 
        /// the directory indicated by 
        /// <see cref="SpecificationFile.DestinationPath"/>. If not specified,
        /// a new file is written with a name generated from the date and time
        /// the backup started.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Gets / Sets a value indicating how much output is written to the
        /// generated log file.
        /// </summary>
        public OutputLevel OutputLevel { get; set; }

        /// <summary>
        /// Gets / Sets a value indicating how the application behaves if the
        /// specified log file already exists. If not specified, defaults to
        /// <see cref="LogFileWriteMode.Append"/>.
        /// </summary>
        public LogFileWriteMode? OverwriteMode { get; set; }
    }
}
