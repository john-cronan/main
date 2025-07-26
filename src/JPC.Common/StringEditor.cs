using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JPC.Common
{
    public class StringEditor
    {
        private readonly IList<StringSegment> _segments;

        public StringEditor()
        {
            _segments = new List<StringSegment>();
        }

        public StringEditor(string str)
        {
            _segments = new List<StringSegment>()
            {
                new StringSegment(str, 0, str.Length)
            };
        }

        public int Length => _segments.Sum(seg => seg.Length);

        public void Append(string str)
        {
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str));
            }

            _segments.Add(new StringSegment(str));
        }

        public void Append(string str, int start, int length)
        {
            _segments.Add(new StringSegment(str, start, length));
        }

        public void AppendLine()
        {
            Append(Environment.NewLine);
        }

        public void Remove(Func<Char, bool> removeWhereTrue)
        {
            int index = 0;
            while (index < _segments.Count)
            {
                var newSegments = _segments[index].Remove(removeWhereTrue);
                if (newSegments.Count() == 1 && newSegments.Single() == _segments[index])
                {
                    index++;
                }
                else
                {
                    foreach (var newSegment in newSegments)
                    {
                        _segments.Insert(index, newSegment);
                        index++;
                    }
                    _segments.RemoveAt(index);
                }
            }
        }

        public override string ToString()
        {
            return string.Create(Length, this, (chars, editor) =>
            {
                var runningIndex = 0;
                for (int i = 0; i < editor._segments.Count; i++)
                {
                    editor._segments[i].String.AsSpan(editor._segments[i].Start, editor._segments[i].Length).CopyTo(chars.Slice(runningIndex));
                    runningIndex += editor._segments[i].Length;
                }
            });
        }

        public void Trim()
        {
            TrimStart();
            TrimEnd();
        }

        public void TrimEnd()
        {
            if (_segments.Count == 0)
            {
                return;
            }

            var segmentIndex = _segments.Count - 1;
            while (segmentIndex >= 0)
            {
                var newSegment = _segments[segmentIndex].TrimEnd();
                if (newSegment == StringSegment.Null)
                {
                    _segments.RemoveAt(segmentIndex);
                    segmentIndex = _segments.Count - 1;
                }
                else
                {
                    _segments.Insert(segmentIndex, newSegment);
                    _segments.RemoveAt(segmentIndex + 1);
                    return;
                }
            }
        }

        public void TrimStart()
        {
            if (_segments.Count == 0)
            {
                return;
            }

            var segmentIndex = 0;
            while (segmentIndex < _segments.Count)
            {
                var newSegment = _segments[segmentIndex].TrimStart();
                if (newSegment == StringSegment.Null)
                {
                    _segments.RemoveAt(segmentIndex);
                }
                else
                {
                    _segments.Insert(segmentIndex, newSegment);
                    _segments.RemoveAt(segmentIndex + 1);
                    return;
                }
            }
        }

        public void Truncate(int length)
        {
            if (length > Length)
            {
                return;
            }

            var remainingToRemove = Length - length;
            while (remainingToRemove >= _segments.Last().Length)
            {
                var removedSegment = _segments.Last();
                _segments.RemoveAt(_segments.Count - 1);
                remainingToRemove -= removedSegment.Length;
            }
            if (remainingToRemove > 0)
            {
                var newLastSegment = _segments.Last().Truncate(_segments.Last().Length - remainingToRemove);
                _segments.RemoveAt(_segments.Count() - 1);
                _segments.Add(newLastSegment);
            }
        }

        public void WriteTo(StringBuilder builder)
        {
            for (int i = 0; i < _segments.Count; i++)
            {
                builder.Append(_segments[i].String, _segments[i].Start, _segments[i].Length);
            }
        }
    }
}
