using System;
using System.Threading;
using System.Threading.Tasks;
using Kaktos.UserImmediateActions.Models;
using Microsoft.Extensions.Caching.Memory;

namespace Kaktos.UserImmediateActions.Stores
{
    public class MemoryCacheImmediateActionsStore : IImmediateActionsStore
    {
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IMemoryCache _memoryCache;
        private readonly IPermanentImmediateActionsStore _permanentImmediateActionsStore;

        public MemoryCacheImmediateActionsStore(IMemoryCache memoryCache,
            IPermanentImmediateActionsStore permanentImmediateActionsStore,
            IDateTimeProvider dateTimeProvider)
        {
            _memoryCache = memoryCache;
            _permanentImmediateActionsStore = permanentImmediateActionsStore;
            _dateTimeProvider = dateTimeProvider;
        }

        public void Add(string key, TimeSpan expirationTime, ImmediateActionDataModel data, bool storeOnPermanentStoreAsWell = true)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentException("Value cannot be null or empty.", nameof(key));

            if (storeOnPermanentStoreAsWell)
            {
                _permanentImmediateActionsStore.Add(key,
                    _dateTimeProvider.Now().Add(expirationTime),
                    data);
            }

            _memoryCache.Set(key, data, expirationTime);
        }

        public async Task AddAsync(string key,
            TimeSpan expirationTime,
            ImmediateActionDataModel data,
            bool storeOnPermanentStoreAsWell = true,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentException("Value cannot be null or empty.", nameof(key));

            if (storeOnPermanentStoreAsWell)
            {
                await _permanentImmediateActionsStore.AddAsync(key,
                    _dateTimeProvider.Now().Add(expirationTime),
                    data,
                    cancellationToken);
            }

            _memoryCache.Set(key, data, expirationTime);
        }

        public ImmediateActionDataModel Get(string key)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentException("Value cannot be null or empty.", nameof(key));

            return _memoryCache.TryGetValue<ImmediateActionDataModel>(key, out var outPut)
                ? outPut
                : default;
        }

        public Task<ImmediateActionDataModel> GetAsync(string key, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentException("Value cannot be null or empty.", nameof(key));

            return Task.FromResult(Get(key));
        }

        public bool Exists(string key)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentException("Value cannot be null or empty.", nameof(key));

            return _memoryCache.TryGetValue(key, out _);
        }

        public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentException("Value cannot be null or empty.", nameof(key));

            return Task.FromResult(Exists(key));
        }
    }
}