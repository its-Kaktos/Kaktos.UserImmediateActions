using System;

namespace Kaktos.UserImmediateActions
{
    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTime Now() => DateTime.Now;
    }
}