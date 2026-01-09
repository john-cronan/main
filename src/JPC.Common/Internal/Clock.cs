using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace JPC.Common.Internal
{
    internal class Clock : IClock
    {
        private readonly ConcurrentDictionary<object, Stopwatch> _stopWatches;

        public Clock()
        {
            _stopWatches = new ConcurrentDictionary<object, Stopwatch>();
        }

        DateTime IClock.Now => DateTime.Now;
        DateTime IClock.UtcNow => DateTime.UtcNow;
        DateTime IClock.MinValue => DateTime.MinValue;
        DateTime IClock.MaxValue => DateTime.MaxValue;
        DateTimeOffset IClock.DateTimeOffsetNow => DateTimeOffset.Now;
        DateTimeOffset IClock.DateTimeOffsetUtcNow => DateTimeOffset.UtcNow;
        DateTimeOffset IClock.DateTimeOffsetMinValue => DateTimeOffset.MinValue;
        DateTimeOffset IClock.DateTimeOffsetMaxValue => DateTimeOffset.MaxValue;

        void IClock.Sleep(TimeSpan howLong) => Thread.Sleep(howLong);

        Task IClock.SleepAsync(TimeSpan howLong) => Task.Delay(howLong);


        void IClock.ResetTimer(object token)
        {
            _stopWatches.AddOrUpdate(token, 
                t => throw new ArgumentException($"Timer not found by he specified token"), 
                (t, existing) =>
                {
                    existing.Reset();
                    return existing;
                });
        }

        object IClock.StartTimer()
        {
            var token = new object();
            _stopWatches.AddOrUpdate(token, t => Stopwatch.StartNew(), (t, existing) =>
            {
                existing.Stop();
                return Stopwatch.StartNew();
            });
            return token;
        }

        TimeSpan IClock.StopTimer(object token)
        {
            if (!_stopWatches.TryRemove(token, out var value))
            {
                throw new ArgumentException($"Timer not found by the specified token");
            }
            return value.Elapsed;
        }
    }
}
