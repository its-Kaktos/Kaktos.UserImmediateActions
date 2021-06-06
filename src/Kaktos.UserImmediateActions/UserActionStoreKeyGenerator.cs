using System;

namespace Kaktos.UserImmediateActions
{
    public class UserActionStoreKeyGenerator : IUserActionStoreKeyGenerator
    {
        private const string Prefix = "_IUASK";

        public string GenerateKey(string userId)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentException("Value cannot be null or empty.", nameof(userId));
            
            return $"{Prefix}_{userId}";
        }

        public string GenerateKey(string userId, string userAgent, string userIp)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentException("Value cannot be null or empty.", nameof(userId));
            if (string.IsNullOrEmpty(userAgent)) throw new ArgumentException("Value cannot be null or empty.", nameof(userAgent));
            if (string.IsNullOrEmpty(userIp)) throw new ArgumentException("Value cannot be null or empty.", nameof(userIp));

            return $"{Prefix}_{userId}_{userAgent}_{userIp}";
        }
    }
}