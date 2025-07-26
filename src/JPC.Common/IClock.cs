using System;

namespace JPC.Common
{
    public interface IClock
    {
        DateTime Now { get; }
        DateTime UtcNow { get; }
        DateTime MinValue { get; }
        DateTime MaxValue { get; }

        DateTimeOffset DateTimeOffsetNow { get; }
        DateTimeOffset DateTimeOffsetUtcNow { get; }
        DateTimeOffset DateTimeOffsetMinValue { get; }
        DateTimeOffset DateTimeOffsetMaxValue { get; }

        void StartTimer(string name);
        TimeSpan StopTimer(string name);
        void ResetTimer(string name);

    }
}
