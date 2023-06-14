using System;

namespace Kaktos.UserImmediateActions
{
    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTimeOffset UtcNow() => DateTimeOffset.UtcNow;
    }
}