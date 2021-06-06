using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using UserImmediateActions.Models;
using UserImmediateActions.Stores;

namespace UserImmediateActions
{
    public class UserImmediateActionsService : IUserImmediateActionsService
    {
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly TimeSpan _expirationTimeForRefreshCookie;
        private readonly TimeSpan _expirationTimeForSignOut;
        private readonly IImmediateActionsStore _immediateActionsStore;
        private readonly IUserActionStoreKeyGenerator _userActionStoreKeyGenerator;

        public UserImmediateActionsService(IImmediateActionsStore immediateActionsStore,
            IUserActionStoreKeyGenerator userActionStoreKeyGenerator,
            IDateTimeProvider dateTimeProvider,
            IOptions<CookieAuthenticationOptions> cookieAuthenticationOptions,
            IOptions<SecurityStampValidatorOptions> securityStampValidatorOptions)
        {
            _immediateActionsStore = immediateActionsStore;
            _userActionStoreKeyGenerator = userActionStoreKeyGenerator;
            _dateTimeProvider = dateTimeProvider;
            var cookieOptions = cookieAuthenticationOptions?.Value ?? new CookieAuthenticationOptions();
            _expirationTimeForRefreshCookie = cookieOptions.ExpireTimeSpan;
            var securityStampOptions = securityStampValidatorOptions?.Value ?? new SecurityStampValidatorOptions();
            _expirationTimeForSignOut = securityStampOptions.ValidationInterval;
        }

        public void RefreshCookie(string userId)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentException("Value cannot be null or empty.", nameof(userId));

            var key = _userActionStoreKeyGenerator.GenerateKey(userId);

            _immediateActionsStore.Add(key, _expirationTimeForRefreshCookie, new ImmediateActionDataModel(_dateTimeProvider.Now(), AddPurpose.RefreshCookie));
        }

        public async Task RefreshCookieAsync(string userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentException("Value cannot be null or empty.", nameof(userId));

            var key = _userActionStoreKeyGenerator.GenerateKey(userId);
            await _immediateActionsStore.AddAsync(key, _expirationTimeForSignOut, new ImmediateActionDataModel(_dateTimeProvider.Now(), AddPurpose.RefreshCookie),
                cancellationToken);
        }

        public void SignOut(string userId)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentException("Value cannot be null or empty.", nameof(userId));

            var key = _userActionStoreKeyGenerator.GenerateKey(userId);

            _immediateActionsStore.Add(key, _expirationTimeForRefreshCookie, new ImmediateActionDataModel(_dateTimeProvider.Now(), AddPurpose.SignOut));
        }

        public async Task SignOutAsync(string userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentException("Value cannot be null or empty.", nameof(userId));

            var key = _userActionStoreKeyGenerator.GenerateKey(userId);
            await _immediateActionsStore.AddAsync(key, _expirationTimeForSignOut, new ImmediateActionDataModel(_dateTimeProvider.Now(), AddPurpose.SignOut), cancellationToken);
        }
    }
}