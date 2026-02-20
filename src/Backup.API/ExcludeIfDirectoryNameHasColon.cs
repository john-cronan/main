using JPC.Common;

namespace JPC.Backup
{
    internal class ExcludeIfDirectoryNameHasColon : IExcludeRule
    {
        private readonly IRuntime _runtime;

        public ExcludeIfDirectoryNameHasColon(IRuntime runtime)
        {
            if (runtime == null)
            {
                throw new ArgumentNullException(nameof(runtime));
            }

            _runtime = runtime;
        }

        string IExcludeRule.FriendlyName => throw new NotImplementedException();

        bool IExcludeRule.ExcludeObject(string sourcePath, string destinationPath)
        {
            var directoryName = _runtime.Filesystem.GetFileName(sourcePath);
            if (directoryName.Contains(':'))
            {
                //
                //  Inexplicably, this special case does happen. Visual Studio, for
                //  one, creates directories in the TestResults directory named, 
                //  for example, "Deploy_{user name} 2020-06-18 20:34:27". So it
                //  is possible. It's just not possible to enumerate or copy
                //  those directories.
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
