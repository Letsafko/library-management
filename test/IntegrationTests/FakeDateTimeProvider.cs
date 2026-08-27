using System;
using SharedKernel.Primitives;

namespace IntegrationTests;

internal sealed class FakeDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow
    {
        get
        {
            var utcNow = DateTime.UtcNow;
            return new DateTime(utcNow.Year, utcNow.Month, utcNow.Day, 12, 0, 0, DateTimeKind.Utc);
        }
    }
}