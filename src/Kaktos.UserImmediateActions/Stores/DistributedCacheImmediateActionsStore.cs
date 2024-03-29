using System;
using System.Threading;
using System.Threading.Tasks;
using Kaktos.UserImmediateActions.Extensions;
using Kaktos.UserImmediateActions.Models;
using Microsoft.Extensions.Caching.Distributed;

namespace Kaktos.UserImmediateActions.Stores
{
    public class DistributedCacheImmediateActionsStore : IImmediateActionsStore
    {
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IDistributedCache _cache;
        private readonly IPermanentImmediateActionsStore _permanentImmediateActionsStore;

        internal DistributedCacheImmediateActionsStore(IDistributedCache cache,
            IPermanentImmediateActionsStore permanentImmediateActionsStore,
            IDateTimeProvider dateTimeProvider)
        {
            _dateTimeProvider = dateTimeProvider;
            _cache = cache;
            _permanentImmediateActionsStore = permanentImmediateActionsStore;
        }

        public DistributedCacheImmediateActionsStore(IDistributedCache cache,
            IPermanentImmediateActionsStore permanentImmediateActionsStore)
        {
            _dateTimeProvider = new DateTimeProvider();
            _cache = cache;
            _permanentImmediateActionsStore = permanentImmediateActionsStore;
        }

        public void Add(string key, TimeSpan expirationTime, ImmediateActionDataModel data, bool storeOnPermanentStoreAsWell = true)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (string.IsNullOrEmpty(key)) throw new ArgumentException("Value cannot be null or empty.", nameof(key));

            if (storeOnPermanentStoreAsWell)
            {
                _permanentImmediateActionsStore.Add(key,
                    _dateTimeProvider.UtcNow().Add(expirationTime),
                    data);
            }

            _cache.SetRecord(key, data, expirationTime);
        }

        public async Task AddAsync(string key,
            TimeSpan expirationTime,
            ImmediateActionDataModel data,
            bool storeOnPermanentStoreAsWell = true,
            CancellationToken cancellationToken = default)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (string.IsNullOrEmpty(key)) throw new ArgumentException("Value cannot be null or empty.", nameof(key));

            if (storeOnPermanentStoreAsWell)
            {
                await _permanentImmediateActionsStore.AddAsync(key,
                    _dateTimeProvider.UtcNow().Add(expirationTime),
                    data,
                    cancellationToken);
            }

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