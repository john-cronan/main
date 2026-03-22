# John Cronan's Command Line Parser

This project is a flexible, feature-rich, well-tested command 
line parser implemented in 100% managed C#.

**Philosophy**

The parser is not attribute-based. Instead, command-line arguments 
and switches are defined and parsing options set via a builder 
(CommandLineParserBuilder), which is oriented toward simple, judicious 
use of method-chaining. Fully "fluent" syntax is specifically avoided.

Over-validation is specifically avoided. Consumers, it is assumed, will
necessarily perform validation of their own anyway, so it doesn't make
sense to write a lot of code to implement validation features that are
very easy to do in consuming code.

Parsing and validation errors result in exceptions. It is the consumer's
responsibility to handle them appropriately, e.g. displaying command
usage text and potentially exiting prematurely. Parse warnings are 
exposed through the result returned by the Parse method, or by object
binding (see below).

## Usage

The package can be consumed one of three ways: By calling properties and
methods of the ICommandLineParseResults instance returned from 
ICommandLineParser's Parse method; by property binding; or by constructor
binding. Although property binding, being the expected most-common
usage, is the default object binding, constructor binding is actually
the recommended usage, as a command line is naturally immutable
anyway.

Usage is simple: Create an instance of CommandLineParserBuilder, call
its methods to set parse options and define arguments and switches, 
invoke the CreateParser method and, in turn, invoke Parse. Parse returns
and instance of ICommandLineParseResults, which can then be used to Bind
parse results to a newly-created object.

The object binders can bind (case insensitively) to arguments and 
properrties of most commonly used value types. Command line arguments 
with multiple values can be bound to a wide variety of collection types, 
including array, IEnumerable&lt;T&gt;, List&lt;T&gt;, IList&lt;T&gt;, 
ImmutableArray&lt;T&gt;, as well as their corresponding non-generic 
types.

Unnamed values are bound to arguments with the names "unnamedValues",
"leadingUnnamedValues", and "trailingUnnamedValues". Parse warning are 
bound to a property named "parseWarnings".

The following command line:

	Program.exe Import /Files authors.csv titles.csv publishers.csv /Batch-Size 1000 /S (local) /D Books /Verbose

May be defined and parsed by the following code:

	var args =
		new CommandLineParserBuilder()
			.UseConstructorBinding()
			.AddArgument("Files", ArgumentMultiplicity.OneOrMore, true)
			.AddArgument("Batch-Size", ArgumentMultiplicity.One, false)
			.AddArgument("Server", ArgumentMultiplicity.One, false)
			.AddArgument("Database", ArgumentMultiplicity.One, true)
			.AddSwitch("Verbose")
			.CreateParser()
			.Parse()
			.Bind<CommandLine>();

And bound to a class with the following constructor:

    internal class CommandLine
    {
        public CommandLine(ImmutableArray<string> leadingUnnamedValues,
            ImmutableArray<string> files, int? batchSize, string server,
            string database, bool verbose)
        {                
        }
    }

Or, if using property binding (with the `UsePropertyBinding` method):

    internal class CommandLine
    {
        public ImmutableArray<string> LeadingUnnamedValues { get; set; }
        public ImmutableArray<string> Files { get; set; }
        public int? BatchSize { get; set; }
        public string Server { get; set; }
        public string Database { get; set; }
        public bool Verbose { get; set; }
    }

See the integration tests in the test folder at 
https://github.com/john-cronan/main for additional examples of 
usage.

## Future Directions

Testing. The project could use more organized, thorough testing.

Binding errors and warnings. Currently, exceptions occurring during object
binding are not handled ideally, especially type conversion errors. Revisions
to allow the binders to contribute errors and warnings to the parsing/binding
process would be beneficial.








