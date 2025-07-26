using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

namespace JC.CommandLine
{
    [DebuggerDisplay("{System.String.Join(\", \", Names)}, {Multiplicity}, {Required ? \"Required\" : \"Optional\"}")]
    internal class Argument
    {
        private readonly ArgumentFlags _flags;
        private readonly ImmutableArray<string> _names;
        private readonly ArgumentMultiplicity _multiplicity;
        private readonly bool _required;

        public Argument(string name, ArgumentMultiplicity multiplicity,
            bool required)
            : this(ImmutableArray<string>.Empty.Add(name), multiplicity,
                 required, ArgumentFlags.None)
        {
        }

        public Argument(string name, ArgumentMultiplicity multiplicity,
            bool required, ArgumentFlags flags)
            : this(ImmutableArray<string>.Empty.Add(name), multiplicity,
                  required, flags)
        {
        }

        public Argument(ImmutableArray<string> names, ArgumentMultiplicity multiplicity,
            bool required)
            : this(names, multiplicity, required, ArgumentFlags.None)
        {
        }

        public Argument(ImmutableArray<string> names, ArgumentMultiplicity multiplicity,
            bool required, ArgumentFlags flags)
        {
            Guard.IsNotEmpty(names, nameof(names));
            if (!names.Any(n => !string.IsNullOrWhiteSpace(n)))
            {
                throw new ArgumentNullException(nameof(names));
            }
            if (flags.HasFlag(ArgumentFlags.AssumeBase64) &&
                flags.HasFlag(ArgumentFlags.AssumeHexadecimal))
            {
                throw new ArgumentException($"{nameof(ArgumentFlags.AssumeBase64)} and " +
                    $"{nameof(ArgumentFlags.AssumeHexadecimal)} cannot be used together", 
                    nameof(flags));
            }
            if (flags.HasFlag(ArgumentFlags.ExistingDirectory) &&
                flags.HasFlag(ArgumentFlags.ExistingFile))
            {
                throw new ArgumentException($"{nameof(ArgumentFlags.ExistingDirectory)} and " +
                   $"{nameof(ArgumentFlags.ExistingFile)} cannot be used together",
                   nameof(flags));

            }

            _names = names;
            _multiplicity = multiplicity;
            _required = required;
            _flags = flags;
        }

        public ArgumentFlags Flags => _flags;

        public ImmutableArray<string> Names => _names;

        public ArgumentMultiplicity Multiplicity => _multiplicity;

        public bool Required => _required;
    }
}
