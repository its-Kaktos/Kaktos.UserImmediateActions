using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using UserImmediateActions.Extensions;
using UserImmediateActions.Models;

namespace UserImmediateActions.Stores
{
    public class DistributedCacheImmediateActionsStore : IImmediateActionsStore
    {
        private readonly IDistributedCache _cache;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IPermanentImmediateActionsStore _permanentImmediateActionsStore;

        public DistributedCacheImmediateActionsStore(IDistributedCache cache,
            IPermanentImmediateActionsStore permanentImmediateActionsStore,
            IDateTimeProvider dateTimeProvider)
        {
            _cache = cache;
            _permanentImmediateActionsStore = permanentImmediateActionsStore;
            _dateTimeProvider = dateTimeProvider;
        }

        public void Add(string key, TimeSpan expirationTime, ImmediateActionDataModel data)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentException("Value cannot be null or empty.", nameof(key));

            _permanentImmediateActionsStore.Add(key,
                _dateTimeProvider.Now().Add(expirationTime),
                data);

            _cache.SetRecord(key, data, expirationTime);
        }

        public async Task AddAsync(string key, TimeSpan expirationTime, ImmediateActionDataModel data, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentException("Value cannot be null or empty.", nameof(key));

            await _permanentImmediateActionsStore.AddAsync(key,
                _dateTimeProvider.Now().Add(expirationTime),
                data,
                cancellationToken);

            await _cache.SetRecordAsync(key,
                data,
                expirationTime,
                cancellationToken: cancellationToken);
        }

        public ImmediateActionDataModel Get(string key)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentException("Value cannot be null or empty.", nameof(key));

            return _cache.GetRecord<ImmediateActionDataModel>(key);
        }

        public async Task<ImmediateActionDataModel> GetAsync(string key, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentException("Value cannot be null or empty.", nameof(key));

            return await _cache.GetRecordAsync<ImmediateActionDataModel>(key, cancellationToken);
        }

        public bool Exists(string key)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentException("Value cannot be null or empty.", nameof(key));

            var record = _cache.GetString(key);
            return record != null;
        }

        public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentException("Value cannot be null or empty.", nameof(key));

            var record = await _cache.GetStringAsync(key, cancellationToken);
            return record != null;
        }
    }
}