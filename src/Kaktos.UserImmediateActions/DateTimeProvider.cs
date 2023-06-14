using System;

namespace Kaktos.UserImmediateActions
{
    internal class DateTimeProvider : IDateTimeProvider
    {
        public DateTimeOffset UtcNow() => DateTimeOffset.UtcNow;
    }
}