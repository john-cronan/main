# John Cronan's Command Line Parser

This project is a flexible, feature-rich, well-tested command 
line parser implemented in 100% managed C#.

**Philosophy**

The parser is not attribute-based. Instead, command-line arguments 
and switches are defined and parsing options set via a builder 
(CommandLineParserBuilder), which is oriented toward simple, judicious 
use of method-chaining. Fully "fluent" syntax is specifically avoided.

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






