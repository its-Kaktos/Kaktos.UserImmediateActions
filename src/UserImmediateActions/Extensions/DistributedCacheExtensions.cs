using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace UserImmediateActions.Extensions
{
    internal static class DistributedCacheExtensions
    {
        public static void SetRecord<T>(this IDistributedCache cache,
            string key,
            T record,
            TimeSpan absoluteExpirationTime,
            TimeSpan? unusedExpirationTime = default)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = absoluteExpirationTime,
                SlidingExpiration = unusedExpirationTime
            };

            var jsonData = JsonSerializer.Serialize(record);
            cache.SetString(key, jsonData, options);
        }

        public static async Task SetRecordAsync<T>(this IDistributedCache cache,
            string key,
            T record,
            TimeSpan absoluteExpirationTime,
            TimeSpan? unusedExpirationTime = default,
            CancellationToken cancellationToken = default)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = absoluteExpirationTime,
                SlidingExpiration = unusedExpirationTime
            };

            var jsonData = JsonSerializer.Serialize(record);
            await cache.SetStringAsync(key, jsonData, options, cancellationToken);
        }

        public static T GetRecord<T>(this IDistributedCache cache, string key)
        {
            var jsonData = cache.GetString(key);

            return jsonData is null ? default : JsonSerializer.Deserialize<T>(jsonData);
        }

        public static async Task<T> GetRecordAsync<T>(this IDistributedCache cache, string key, CancellationToken cancellationToken = default)
        {
            var jsonData = await cache.GetStringAsync(key, cancellationToken);

            return jsonData is null ? default : JsonSerializer.Deserialize<T>(jsonData);
        }
    }
}