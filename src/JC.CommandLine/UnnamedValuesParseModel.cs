namespace JC.CommandLine
{
    internal class UnnamedValuesParseModel
    {
        public static readonly UnnamedValuesParseModel AllowAll =
            new UnnamedValuesParseModel(ArgumentMultiplicity.ZeroOrMore,
                ArgumentMultiplicity.ZeroOrMore);
        public static readonly UnnamedValuesParseModel DisallowAll =
            new UnnamedValuesParseModel(ArgumentMultiplicity.Zero,
                ArgumentMultiplicity.Zero);


        private readonly ArgumentMultiplicity _leadingMultiplicity;
        private readonly ArgumentMultiplicity _trailingMultiplicity;

        public UnnamedValuesParseModel(ArgumentMultiplicity leadingMultiplicity,
            ArgumentMultiplicity trailingMultiplicity)
        {
            _leadingMultiplicity = leadingMultiplicity;
            _trailingMultiplicity = trailingMultiplicity;
        }

        public ArgumentMultiplicity LeadingMultiplicity => _leadingMultiplicity;
        public ArgumentMultiplicity TrailingMultiplicity => _trailingMultiplicity;

        public override bool Equals(object obj)
            => obj is UnnamedValuesParseModel 
                ? Equals(obj as UnnamedValuesParseModel) : false;

        public bool Equals(UnnamedValuesParseModel other)
        {
            if (other == null) return false;
            if (_leadingMultiplicity != other._leadingMultiplicity) return false;
            if (_trailingMultiplicity != other._trailingMultiplicity) return false;
            return true;
        }

        public override int GetHashCode()
        {
            var hashCode = _leadingMultiplicity.GetHashCode();
            hashCode ^= _trailingMultiplicity.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(UnnamedValuesParseModel left, UnnamedValuesParseModel right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (ReferenceEquals(left, null) || ReferenceEquals(right, null)) return false;
            return left.Equals(right);
        }

        public static bool operator !=(UnnamedValuesParseModel left, UnnamedValuesParseModel right)
            => !(left == right);
    }
}
