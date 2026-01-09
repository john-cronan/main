using System;
using System.Threading.Tasks;

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

        object StartTimer();
        TimeSpan StopTimer(object token);
        void ResetTimer(object token);

        void Sleep(TimeSpan howLong);
        Task SleepAsync(TimeSpan howLong);
    }
}
