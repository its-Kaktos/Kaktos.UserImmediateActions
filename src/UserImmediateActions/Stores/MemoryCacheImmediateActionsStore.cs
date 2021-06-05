using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using UserImmediateActions.Models;

namespace UserImmediateActions.Stores
{
    public class MemoryCacheImmediateActionsStore : IImmediateActionsStore
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IPermanentImmediateActionsStore _permanentImmediateActionsStore;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly TimeSpan _defaultExpirationRelativeToNow;

        public MemoryCacheImmediateActionsStore(IMemoryCache memoryCache,
            IPermanentImmediateActionsStore permanentImmediateActionsStore,
            IDateTimeProvider dateTimeProvider,
            IOptions<CookieAuthenticationOptions> options)
        {
            _memoryCache = memoryCache;
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

            _memoryCache.Set(key, data, absoluteExpirationRelativeToNow: _defaultExpirationRelativeToNow);
        }

        public async Task AddAsync(string key, ImmediateActionDataModel data, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentException("Value cannot be null or empty.", nameof(key));

            await _permanentImmediateActionsStore.AddAsync(key,
                _dateTimeProvider.Now().Add(_defaultExpirationRelativeToNow),
                data,
                cancellationToken);

            Add(key, data);
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