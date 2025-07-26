using System.Collections;
using System.Text;
using Moq;

namespace JPC.Common.Testing
{
    public class MockEnvironment : Mock<IEnvironment>
    {
        private readonly Dictionary<string, string> _environmentVariables;
        private Action<int> _exitInvoked;

        public MockEnvironment()
        {
            _environmentVariables = new Dictionary<string, string>();
            Setup(p => p.CommandLine).Returns(System.Environment.CommandLine);
            Setup(p => p.MachineName).Returns(System.Environment.MachineName);
            Setup(p => p.NewLine).Returns(System.Environment.NewLine);
            Setup(p => p.UserName).Returns(System.Environment.UserName);
            Setup(m => m.Exit(It.IsAny<int>())).Callback<int>(exitCode =>
            {
                if (_exitInvoked != null)
                {
                    _exitInvoked(exitCode);
                }
            });
            Setup(m => m.ExpandEnvironmentVariables(It.IsAny<string>())).Returns((Delegate)(Func<string, string>)(
                str => RecursivelyReplaceVariables(str)));
            Setup(m => m.ExpandEnvironmentVariables(It.IsAny<string>())).Returns((Delegate)(Func<string, string>)RecursivelyReplaceVariables);
        }

        public string CommandLine
        {
            get { return Object.CommandLine; }
            set { Setup(p => p.CommandLine).Returns(value); }
        }

        public string[] CommandLineArgs
        {
            get { return Object.GetCommandLineArgs(); }
            set { Setup(m => m.GetCommandLineArgs()).Returns(value); }
        }

        public OperatingSystem? OperatingSystem
        {
            get { return Object.OperatingSystem; }
            set { Setup(p => p.OperatingSystem).Returns(value); }
        }
        public string MachineName
        {
            get { return Object.MachineName; }
            set { Setup(p => p.MachineName).Returns(value); }
        }

        public string NewLine
        {
            get { return Object.NewLine; }
            set { Setup(p => p.NewLine).Returns(value); }
        }

        public string UserName
        {
            get { return Object.UserName; }
            set { Setup(p => p.UserName).Returns(value); }
        }

        public Action<int> ExitInvoked
        {
            get { return _exitInvoked; }
            set { _exitInvoked = value; }
        }

        public void AddLocalEnvironmentVariables()
        {
            foreach (DictionaryEntry entry in System.Environment.GetEnvironmentVariables())
            {
                _environmentVariables.Add(entry.Key.ToString(), (entry.Value ?? string.Empty).ToString());
            }
        }

        public void AddMachineEnvironmentVariables()
            => AddEnvironmentVariables(EnvironmentVariableTarget.Machine);

        public void AddUserEnvironmentVariables()
            => AddEnvironmentVariables(EnvironmentVariableTarget.User);

        public void AddProcessEnvironmentVariables()
            => AddEnvironmentVariables(EnvironmentVariableTarget.Process);

        public void ClearEnvironmentVariables()
            => _environmentVariables.Clear();

        public void SetEnvironmentVariable(string name, string value)
            => _environmentVariables[name] = value;


        private void AddEnvironmentVariables(EnvironmentVariableTarget target)
        {
            foreach (DictionaryEntry entry in System.Environment.GetEnvironmentVariables(target))
            {
                _environmentVariables.Add(entry.Key.ToString(), (entry.Value ?? string.Empty).ToString());
            }
        }

        private string RecursivelyReplaceVariables(string stringIn)
        {
            if (string.IsNullOrWhiteSpace(stringIn))
            {
                return stringIn;
            }

            var str = new StringBuilder(stringIn);
            var before = stringIn;
            var after = string.Empty;
            while (after != before)
            {
                before = str.ToString();
                foreach (var variable in _environmentVariables)
                {
                    str.Replace("%" + variable.Key + "%", variable.Value);
                }
                after = str.ToString();
            }
            return str.ToString();
        }
    }
}
