using Moq;

namespace JPC.Common.Testing
{
    public class MockClock : Mock<IClock>
    {
        public MockClock()
        {
            Setup(p => p.MinValue).Returns(DateTime.MinValue);
            Setup(p => p.MaxValue).Returns(DateTime.MaxValue);
            Setup(p => p.DateTimeOffsetMinValue).Returns(DateTimeOffset.MinValue);
            Setup(p => p.DateTimeOffsetMaxValue).Returns(DateTimeOffset.MaxValue);

            Setup(p => p.Now).Returns((Delegate)(Func<DateTime>)(
                () => DateTime.Now));
            Setup(p => p.DateTimeOffsetNow).Returns((Delegate)(Func<DateTimeOffset>)(
                () => DateTimeOffset.Now));
        }

        public DateTime Now
        {
            get { return Object.Now; }
            set
            {
                Setup(p => p.Now).Returns(value);
                Setup(p => p.UtcNow).Returns(value.ToUniversalTime());
            }
        }

        public DateTimeOffset DateTimeOffsetNow
        {
            get { return Object.DateTimeOffsetNow; }
            set
            {
                Setup(p => p.DateTimeOffsetNow).Returns(value);
                Setup(p => p.DateTimeOffsetUtcNow).Returns(value.ToUniversalTime());
            }
        }

        public void StopTimeReturns(string timerName, TimeSpan value)
            => Setup(m => m.StopTimer(timerName)).Returns(value);
    }
}
