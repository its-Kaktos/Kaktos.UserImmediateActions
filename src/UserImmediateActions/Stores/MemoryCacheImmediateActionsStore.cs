using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using UserImmediateActions.Models;

namespace UserImmediateActions.Stores
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

        public void Add(string key, TimeSpan expirationTime, ImmediateActionDataModel data)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentException("Value cannot be null or empty.", nameof(key));

            _permanentImmediateActionsStore.Add(key,
                _dateTimeProvider.Now().Add(expirationTime),
                data);

            _memoryCache.Set(key, data, expirationTime);
        }

        public async Task AddAsync(string key, TimeSpan expirationTime, ImmediateActionDataModel data, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentException("Value cannot be null or empty.", nameof(key));

            await _permanentImmediateActionsStore.AddAsync(key,
                _dateTimeProvider.Now().Add(expirationTime),
                data,
                cancellationToken);

            Add(key, expirationTime, data);
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