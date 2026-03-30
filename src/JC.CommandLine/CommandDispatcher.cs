using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace JC.CommandLine
{
    /// <summary>
    /// A class that routes commands to functions that parse the related
    /// command line and execute an appropriate action.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class assumes that a command line takes the following form:
    /// </para>
    /// <para>
    /// MyUtility.exe Import -Batch-Size 1000 -File Authors.csv
    /// </para>
    /// <para>
    /// Where "Import" is the command. Commands are routed to handling
    /// functions by this value. Sub-commands are also supported, such as:
    /// </para>
    /// <para>
    /// MyUtility.exe Import Authors -File Authors.csv
    /// </para>
    /// </remarks>
    public class CommandDispatcher
    {
        private readonly List<CommandDefinition> _commands;
        private CommandDefinition _defaultCommand;

        /// <summary>
        /// Constructs a new instance of the object.
        /// </summary>
        public CommandDispatcher()
        {
            _commands = new List<CommandDefinition>();
        }

        /// <summary>
        /// Gets / Sets a value indicating whether command text comparisons are
        /// performed in a case-sensitive manner.
        /// </summary>
        public bool CaseSenitive { get; set; }

        /// <summary>
        /// Executes the registered command, or the registered default command, 
        /// that matches the one specified on the command line.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If more than on registered command matches the command line, the most
        /// specific command is executed. This can happen if fallback commands
        /// are defined. For example, if both "Import" and "Import Authors" are
        /// registered, and the command line specifies "Import Authors", both
        /// registered commands will match, and "Import Authors" will be selected
        /// because more commands/sub-commands are matched.
        /// </para>
        /// </remarks>
        public Task<int> ExecuteAsync()
            => ExecuteAsync(Environment.GetCommandLineArgs());

        /// <summary>
        /// Executes the registered command, or the registered default command, 
        /// that matches the one specified on the provided command line.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If more than on registered command matches the command line, the most
        /// specific command is executed. This can happen if fallback commands
        /// are defined. For example, if both "Import" and "Import Authors" are
        /// registered, and the command line specifies "Import Authors", both
        /// registered commands will match, and "Import Authors" will be selected
        /// because more commands/sub-commands are matched.
        /// </para>
        /// </remarks>
        public async Task<int> ExecuteAsync(IEnumerable<string> arguments)
        {
            Guard.IsNotNullOrEmpty(arguments, nameof(arguments));

            var stringComparer = CaseSenitive
                ? StringComparer.InvariantCulture
                : StringComparer.InvariantCultureIgnoreCase;
            var matches =
                _commands.Where(c => arguments.Skip(1).StartsWith(c.CommandText, stringComparer.Equals))
                    .OrderByDescending(c => c.CommandText.Length);
            var match = matches.FirstOrDefault() ?? _defaultCommand;
            if (match == null)
            {
                throw new InvalidOperationException("No matching command was foundand no default command defined");
            }
            var returnCode = await match.Handler(arguments);
            Environment.ExitCode = returnCode;
            return returnCode;
        }

        /// <summary>
        /// Associates a specified command with a specified function which
        /// will parse the command line according to command-specific rules
        /// and execute appropriate action.
        /// </summary>
        /// <param name="commandText">
        /// The command text. If this value matches the
        /// value on the command line, function will be executed.
        /// </param>
        /// <param name="parseAndExecute">
        /// A function accepting command line parameters, to be passed to
        /// <see cref="ICommandLineParser.Parse(IEnumerable{string})"/>,
        /// parsed, and application action taken.
        /// </param>
        public void Register(string commandText,
            Func<IEnumerable<string>, Task<int>> parseAndExecute)
        {
            Guard.IsNotNullOrWhitespace(commandText, nameof(commandText));
            Guard.IsNotNull(parseAndExecute, nameof(parseAndExecute));

            var cmd = new CommandDefinition(ImmutableArray.Create(commandText), parseAndExecute);
            _commands.Add(cmd);
        }

        /// <summary>
        /// Associates a specified command and sub-command with a specified 
        /// function which will parse the command line according to 
        /// command-specific rules and execute appropriate action.
        /// </summary>
        /// <param name="commandAndSubCommandText">
        /// The command and sub-command text. If this value matches the
        /// value on the command line, function will be executed.
        /// </param>
        /// <param name="parseAndExecute">
        /// A function accepting command line parameters, to be passed to
        /// <see cref="ICommandLineParser.Parse(IEnumerable{string})"/>,
        /// parsed, and application action taken.
        /// </param>
        public void Register(string[] commandAndSubCommandText,
            Func<IEnumerable<string>, Task<int>> parseAndExecute)
        {
            Guard.IsNotNullOrEmpty(commandAndSubCommandText, nameof(commandAndSubCommandText));
            Guard.IsNotNull(parseAndExecute, nameof(parseAndExecute));

            var cmd = new CommandDefinition(commandAndSubCommandText.ToImmutableArray(),
                parseAndExecute);
            _commands.Add(cmd);

        }

        /// <summary>
        /// Provides a function that will be executed if the command line does
        /// not match any registered command.
        /// </summary>
        public void RegisterDefault(Func<IEnumerable<string>, Task<int>> parseAndExecute)
        {
            Guard.IsNotNull(parseAndExecute, nameof(parseAndExecute));

            _defaultCommand = new CommandDefinition(ImmutableArray<string>.Empty, parseAndExecute);
        }



        private class CommandDefinition
        {
            private readonly ImmutableArray<string> _commandText;
            private readonly Func<IEnumerable<string>, Task<int>> _handler;

            public CommandDefinition(ImmutableArray<string> commandText,
                Func<IEnumerable<string>, Task<int>> handler)
            {
                _commandText = commandText;
                _handler = handler;
            }

            public ImmutableArray<string> CommandText => _commandText;
            public Func<IEnumerable<string>, Task<int>> Handler => _handler;
        }
    }
}
