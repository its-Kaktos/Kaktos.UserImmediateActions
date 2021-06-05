using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using UserImmediateActions.Extensions;
using UserImmediateActions.Models;

namespace UserImmediateActions.Stores
{
    public class DistributedCacheImmediateActionsStore : IImmediateActionsStore
    {
        private readonly IDistributedCache _cache;
        private readonly IPermanentImmediateActionsStore _permanentImmediateActionsStore;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly TimeSpan _defaultExpirationRelativeToNow;

        public DistributedCacheImmediateActionsStore(IDistributedCache cache,
            IPermanentImmediateActionsStore permanentImmediateActionsStore,
            IDateTimeProvider dateTimeProvider,
            IOptions<CookieAuthenticationOptions> options)
        {
            _cache = cache;
            _permanentImmediateActionsStore = permanentImmediateActionsStore;
            _dateTimeProvider = dateTimeProvider;
            var cookieAuthenticationOptions = options?.Value ?? new CookieAuthenticationOptions();
            _defaultExpirationRelativeToNow = cookieAuthenticationOptions.ExpireTimeSpan;
        }

        public void Add(string key, ImmediateActionDataModel data)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentException("Value cannot be null or empty.", nameof(key));

            _permanentImmediateActionsStore.Add(key,
                _dateTimeProvider.Now().Add(_defaultExpirationRelativeToNow),
                data);
            
            _cache.SetRecord(key, data, _defaultExpirationRelativeToNow);
        }

        public async Task AddAsync(string key, ImmediateActionDataModel data, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentException("Value cannot be null or empty.", nameof(key));

            await _permanentImmediateActionsStore.AddAsync(key,
                _dateTimeProvider.Now().Add(_defaultExpirationRelativeToNow),
                data,
                cancellationToken);
            
            await _cache.SetRecordAsync(key,
                data,
                _defaultExpirationRelativeToNow,
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

            return await _cache.GetRecordAsync<ImmediateActionDataModel>(key, cancellationToken: cancellationToken);
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