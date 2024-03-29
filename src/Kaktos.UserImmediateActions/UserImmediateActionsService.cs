using System;
using System.Threading;
using System.Threading.Tasks;
using Kaktos.UserImmediateActions.Models;
using Kaktos.UserImmediateActions.Stores;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Kaktos.UserImmediateActions
{
    public class UserImmediateActionsService : IUserImmediateActionsService
    {
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly TimeSpan _expirationTimeForRefreshCookie;
        private readonly TimeSpan _expirationTimeForSignOut;
        private readonly IImmediateActionsStore _immediateActionsStore;
        private readonly IUserActionStoreKeyGenerator _userActionStoreKeyGenerator;

        internal UserImmediateActionsService(IImmediateActionsStore immediateActionsStore,
            IUserActionStoreKeyGenerator userActionStoreKeyGenerator,
            IDateTimeProvider dateTimeProvider,
            IOptions<CookieAuthenticationOptions> cookieAuthenticationOptions,
            IOptions<SecurityStampValidatorOptions> securityStampValidatorOptions)
        {
            _dateTimeProvider = dateTimeProvider;
            _immediateActionsStore = immediateActionsStore;
            _userActionStoreKeyGenerator = userActionStoreKeyGenerator;
            var cookieOptions = cookieAuthenticationOptions?.Value ?? new CookieAuthenticationOptions();
            _expirationTimeForRefreshCookie = cookieOptions.ExpireTimeSpan;
            var securityStampOptions = securityStampValidatorOptions?.Value ?? new SecurityStampValidatorOptions();
            _expirationTimeForSignOut = securityStampOptions.ValidationInterval;
        }

        public UserImmediateActionsService(IImmediateActionsStore immediateActionsStore,
            IUserActionStoreKeyGenerator userActionStoreKeyGenerator,
            IOptions<CookieAuthenticationOptions> cookieAuthenticationOptions,
            IOptions<SecurityStampValidatorOptions> securityStampValidatorOptions)
        {
            _dateTimeProvider = new DateTimeProvider();
            _immediateActionsStore = immediateActionsStore;
            _userActionStoreKeyGenerator = userActionStoreKeyGenerator;
            var cookieOptions = cookieAuthenticationOptions?.Value ?? new CookieAuthenticationOptions();
            _expirationTimeForRefreshCookie = cookieOptions.ExpireTimeSpan;
            var securityStampOptions = securityStampValidatorOptions?.Value ?? new SecurityStampValidatorOptions();
            _expirationTimeForSignOut = securityStampOptions.ValidationInterval;
        }

        public void RefreshCookie(string userId)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentException("Value cannot be null or empty.", nameof(userId));

            var key = _userActionStoreKeyGenerator.GenerateKey(userId);

            _immediateActionsStore.Add(key, _expirationTimeForRefreshCookie, new ImmediateActionDataModel(_dateTimeProvider.UtcNow(), AddPurpose.RefreshCookie));
        }

        public async Task RefreshCookieAsync(string userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentException("Value cannot be null or empty.", nameof(userId));

            var key = _userActionStoreKeyGenerator.GenerateKey(userId);
            await _immediateActionsStore.AddAsync(key,
                _expirationTimeForRefreshCookie,
                new ImmediateActionDataModel(_dateTimeProvider.UtcNow(), AddPurpose.RefreshCookie),
                true,
                cancellationToken);
        }

        public void SignOut(string userId)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentException("Value cannot be null or empty.", nameof(userId));

            var key = _userActionStoreKeyGenerator.GenerateKey(userId);

            _immediateActionsStore.Add(key, _expirationTimeForSignOut, new ImmediateActionDataModel(_dateTimeProvider.UtcNow(), AddPurpose.SignOut));
        }

        public async Task SignOutAsync(string userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentException("Value cannot be null or empty.", nameof(userId));

            var key = _userActionStoreKeyGenerator.GenerateKey(userId);
            await _immediateActionsStore.AddAsync(key,
                _expirationTimeForSignOut,
                new ImmediateActionDataModel(_dateTimeProvider.UtcNow(), AddPurpose.SignOut),
                true,
                cancellationToken);
        }
    }
}