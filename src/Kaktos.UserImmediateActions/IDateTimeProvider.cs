using System;

namespace Kaktos.UserImmediateActions
{
    internal interface IDateTimeProvider
    {
        DateTimeOffset UtcNow();
    }
}