using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;

namespace JPC.Common
{
    public enum FileSizeUnits : long
    {
        Bytes = 1,
        KB = 1024,
        MB = KB * KB,
        GB = MB * KB,
        TB = GB * KB,
        PB = TB ^ KB,
        EB = PB * KB
    }

    public struct FileSize
    {
        public static readonly FileSize Zero = new FileSize(0);

        private static readonly Regex ParseExpr = new Regex(@"^(?<quantity>[0-9.]+)?\s?(?<unit>[a-zA-Z]+)?$", RegexOptions.Compiled
            | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture
            | RegexOptions.IgnoreCase | RegexOptions.Singleline);


        //
        //  The value is always in FileSizeUnits.Bytes.
        private readonly double _value;

        public static FileSize From(double value, FileSizeUnits unit)
            => new FileSize(value * (double)unit);

        public static FileSize Parse(string value)
        {
            GetQuantityAndUnit(value, true, out var quantity, out var unitAsString);
            ConvertStringToUnit(unitAsString, true, out var unitAsEnum);
            return new FileSize(quantity * (long)unitAsEnum);
        }

        public static bool TryParse(string valueIn, out FileSize valueOut)
        {
            valueOut = Zero;
            if (!GetQuantityAndUnit(valueIn, false, out var quantity, out var unitAsString))
            {
                return false;
            }
            if (!ConvertStringToUnit(unitAsString, false, out var unitAsEnum))
            {
                return false;
            }
            valueOut = new FileSize(quantity * (long)unitAsEnum);
            return true;
        }

        private static bool GetQuantityAndUnit(string fromValue, bool throwOnError, out double quantity,
            out string unit)
        {
            quantity = 0;
            unit = "";
            var m = ParseExpr.Match(fromValue);
            if (!m.Success)
            {
                if (throwOnError)
                {
                    throw new FormatException();
                }
                else
                {
                    return false;
                }
            }
            unit = m.Groups["unit"].Value;
            var quantityAsString = m.Groups["quantity"].Value;
            if (!double.TryParse(quantityAsString, out quantity))
            {
                if (throwOnError)
                {
                    throw new FormatException($"The value '{quantityAsString}' cannot be parsed as a number");
                }
                else
                {
                    return false;
                }
            }
            unit = m.Groups["unit"].Value;
            return true;
        }

        private static bool ConvertStringToUnit(string value, bool throwOnError, out FileSizeUnits unitOutput)
        {
            unitOutput = FileSizeUnits.Bytes;
            if (value.Equals("B", StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }
            else
            {
                try
                {
                    if (Enum.TryParse(typeof(FileSizeUnits), value, out object valueAsUnitObject))
                        unitOutput = (FileSizeUnits)valueAsUnitObject;
                    return true;
                }
                catch (Exception)
                {
                    if (throwOnError)
                    {
                        throw new FormatException($"Invalid unit '{value}'");
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }


        private FileSize(double value)
        {
            _value = value;
        }

        public override bool Equals([NotNullWhen(true)] object obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            return obj is FileSize ? ((FileSize)obj)._value == _value : false;
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public override string ToString()
        {
            var self = this;
            var asUnits =
                from member in (FileSizeUnits[])Enum.GetValues(typeof(FileSizeUnits))
                select new { Unit = member, Value = self.Value(member) };
            if (asUnits.Any(au => Math.Floor(au.Value) > 0))
            {
                var first = asUnits.OrderBy(au => au.Value).First(au => Math.Floor(au.Value) > 0);
                return $"{first.Value} {first.Unit.ToString()}";
            }
            else
            {
                return $"{self.Value(FileSizeUnits.Bytes)} bytes";
            }
        }

        public string ToString(FileSizeUnits unit)
        {
            return $"{Value(unit)} {unit.ToString()}";
        }

        public double Value(FileSizeUnits unit) => _value / (double)unit;


        public static bool operator ==(FileSize left, FileSize right)
            => left._value == right._value;

        public static bool operator !=(FileSize left, FileSize right)
            => left._value != right._value;

        public static bool operator >(FileSize left, FileSize right)
            => left._value > right._value;

        public static bool operator <(FileSize left, FileSize right)
            => left._value < right._value;

        public static bool operator >=(FileSize left, FileSize right)
            => left._value >= right._value;

        public static bool operator <=(FileSize left, FileSize right)
            => left._value <= right._value;
    }
}
