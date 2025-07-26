using System;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace JPC.Common.Internal
{
    internal class Clock : IClock
    {
        private readonly ConcurrentDictionary<string, Stopwatch> _stopWatches;

        public Clock()
        {
            _stopWatches = new ConcurrentDictionary<string, Stopwatch>();
        }

        DateTime IClock.Now => DateTime.Now;
        DateTime IClock.UtcNow => DateTime.UtcNow;
        DateTime IClock.MinValue => DateTime.MinValue;
        DateTime IClock.MaxValue => DateTime.MaxValue;
        DateTimeOffset IClock.DateTimeOffsetNow => DateTimeOffset.Now;
        DateTimeOffset IClock.DateTimeOffsetUtcNow => DateTimeOffset.UtcNow;
        DateTimeOffset IClock.DateTimeOffsetMinValue => DateTimeOffset.MinValue;
        DateTimeOffset IClock.DateTimeOffsetMaxValue => DateTimeOffset.MaxValue;

        void IClock.ResetTimer(string name)
        {
            _stopWatches.AddOrUpdate(name, 
                nm => throw new ArgumentException($"Timer '{name}' not found"), 
                (nm, existing) =>
                {
                    existing.Reset();
                    return existing;
                });
        }

        void IClock.StartTimer(string name)
        {
            _stopWatches.AddOrUpdate(name, nm => Stopwatch.StartNew(), (nm, existing) =>
            {
                existing.Stop();
                return Stopwatch.StartNew();
            });
        }

        TimeSpan IClock.StopTimer(string name)
        {
            if (!_stopWatches.TryRemove(name, out var value))
            {
                throw new ArgumentException($"Timer named '{name}' not found");
            }
            return value.Elapsed;
        }
    }
}
