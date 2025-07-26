using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace JPC.Common
{
    [ImmutableObject(true)]
    public class StringSegment : IEquatable<StringSegment>, IEnumerable<char>, IReadOnlyList<char>
    {
        private enum RemoveStates
        {
            ReadingFirstCharacter,
            ReadingCharactersToPass,
            ReadingCharactersToRemove
        }


        public static readonly StringSegment Null = new StringSegment();

        private readonly string _string;
        private readonly int _start;
        private readonly int _length;

        private StringSegment()
        {
        }

        public StringSegment(string str)
        {
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str));
            }

            _string = str;
            _start = 0;
            _length = str.Length;
        }

        public StringSegment(string str, int start, int length)
        {
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str));
            }
            if (start < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(start), "The starting index must be non-negative");
            }
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "{nameof(length)} must be a positive integer");
            }
            if (start + length > str.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "{nameof(length)} must be less than the end of the string");
            }

            _string = str;
            _start = start;
            _length = length;
        }

        public int Count => Length;
        public string String => _string;
        public int Start => _start;
        public int Length => _length;
        public char this[int index]
        {
            get
            {
                if (_length == 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }
                if (index >= _length)
                {
                    throw new ArgumentOutOfRangeException(nameof(Index));
                }
                return _string[_start + index];
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            return obj is StringSegment ? Equals(obj as StringSegment) : false;
        }

        public bool Equals(StringSegment other)
        {
            if (_length != other._length)
            {
                return false;
            }
            for (int i = 0; i < _length; i++)
            {
                var thisChar = _string[_start + i];
                var otherChar = other._string[other._start + i];
                if (thisChar != otherChar)
                {
                    return false;
                }
            }
            return true;
        }

        public override int GetHashCode()
        {
            if (_length == 0)
            {
                return 0;
            }
            var hashCode = _string[_start].GetHashCode();
            for (int i = 1; i < _length; i++)
            {
                hashCode = hashCode ^ _string[_start + i];
            }
            return hashCode;
        }

        public IEnumerator<char> GetEnumerator()
        {
            for (int i = 0; i < _length; i++)
            {
                yield return _string[_start + i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            for (int i = 0; i < _length; i++)
            {
                yield return _string[_start + i];
            }
        }

        public IEnumerable<StringSegment> Remove(Func<Char, bool> removeWhereTrue)
        {
            var state = RemoveStates.ReadingFirstCharacter;
            var filteredOutAnyChrs = false;
            int index = _start;
            int startOfSubsegmentIndex = index;
            while (index < _start + _length)
            {
                var removeThisChr = removeWhereTrue(_string[index]);
                filteredOutAnyChrs = filteredOutAnyChrs || removeThisChr;
                if (state == RemoveStates.ReadingFirstCharacter)
                {
                    state = removeThisChr ? RemoveStates.ReadingCharactersToRemove : RemoveStates.ReadingCharactersToPass;
                }
                else if (state == RemoveStates.ReadingCharactersToPass && removeThisChr)
                {
                    yield return new StringSegment(_string, startOfSubsegmentIndex, index - startOfSubsegmentIndex);
                    state = RemoveStates.ReadingCharactersToRemove;
                }
                else if (state == RemoveStates.ReadingCharactersToRemove && !removeThisChr)
                {
                    startOfSubsegmentIndex = index;
                    state = RemoveStates.ReadingCharactersToPass;
                }
                index++;
            }
            if (!filteredOutAnyChrs)
            {
                yield return this;
            }
            else if (state == RemoveStates.ReadingCharactersToPass)
            {
                yield return new StringSegment(_string, startOfSubsegmentIndex, index - startOfSubsegmentIndex);
            }
        }

        public StringSegment Truncate(int length)
        {
            if (length > _length)
            {
                return this;
            }

            return new StringSegment(_string, _start, length);
        }

        public override string ToString()
        {
            if (_start == 0 && _length == _string.Length)
            {
                return _string;
            }
            else
            {
                return _string.Substring(_start, _length);
            }
        }

        public StringSegment TrimEnd()
        {
            var lastNonWhitespaceChr = LastNonWhitespaceCharacter();
            if (lastNonWhitespaceChr < _start)
            {
                return Null;
            }
            else
            {
                var newLength = lastNonWhitespaceChr - _start + 1;
                return new StringSegment(_string, _start, newLength);
            }
        }

        public StringSegment TrimStart()
        {
            var firstNonWhitespaceChr = FirstNonWhitespaceCharacter();
            if (firstNonWhitespaceChr == -1)
            {
                return Null;
            }
            else
            {
                var newLength = _length - (firstNonWhitespaceChr - _start);
                return new StringSegment(_string, firstNonWhitespaceChr, newLength);
            }
        }

        public void WriteTo(StringBuilder str)
        {
            for (int i = _start; i < _start + _length; i++)
            {
                str.Append(_string[i]);
            }
        }

        public void WriteTo(TextWriter writer)
        {
            for (int i = _start; i < _start + _length; i++)
            {
                writer.Write(_string[i]);
            }
        }

        public async Task WriteToAsync(TextWriter writer)
        {
            for (int i = _start; i < _start + _length; i++)
            {
                await writer.WriteAsync(_string[i]);
            }
        }


        public static bool operator ==(StringSegment lhs, StringSegment rhs)
        {
            if (Object.ReferenceEquals(lhs, null) || Object.ReferenceEquals(rhs, null))
            {
                return Object.ReferenceEquals(lhs, null) && Object.ReferenceEquals(rhs, null);
            }
            return lhs.Equals(rhs);
        }

        public static bool operator !=(StringSegment lhs, StringSegment rhs)
        {
            return !(lhs == rhs);
        }



        private int FirstNonWhitespaceCharacter()
        {
            var currentIndex = _start;
            while (currentIndex < _string.Length && char.IsWhiteSpace(_string[currentIndex]))
            {
                currentIndex++;
            }
            currentIndex = currentIndex >= _string.Length ? -1 : currentIndex;
            return currentIndex;
        }

        private int LastNonWhitespaceCharacter()
        {
            var currentIndex = _start + _length - 1;
            while (currentIndex >= _start && char.IsWhiteSpace(_string[currentIndex]))
            {
                currentIndex--;
            }
            return currentIndex;
        }

    }
}
