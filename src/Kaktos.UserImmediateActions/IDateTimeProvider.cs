using System;

namespace Kaktos.UserImmediateActions
{
    public interface IDateTimeProvider
    {
        DateTimeOffset UtcNow();
    }
}