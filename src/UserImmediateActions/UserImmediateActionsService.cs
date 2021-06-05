using System;
using System.Threading;
using System.Threading.Tasks;
using UserImmediateActions.Models;
using UserImmediateActions.Stores;

namespace UserImmediateActions
{
    public class UserImmediateActionsService : IUserImmediateActionsService
    {
        private readonly IImmediateActionsStore _immediateActionsStore;
        private readonly IUserActionStoreKeyGenerator _userActionStoreKeyGenerator;
        private readonly IDateTimeProvider _dateTimeProvider;

        public UserImmediateActionsService(IImmediateActionsStore immediateActionsStore, IUserActionStoreKeyGenerator userActionStoreKeyGenerator, IDateTimeProvider dateTimeProvider)
        {
            _immediateActionsStore = immediateActionsStore;
            _userActionStoreKeyGenerator = userActionStoreKeyGenerator;
            _dateTimeProvider = dateTimeProvider;
        }

        public void RefreshCookie(string userId)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentException("Value cannot be null or empty.", nameof(userId));

            var key = _userActionStoreKeyGenerator.GenerateKey(userId);
            
            _immediateActionsStore.Add(key, new ImmediateActionDataModel(_dateTimeProvider.Now(), AddPurpose.RefreshCookie));
        }

        public async Task RefreshCookieAsync(string userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentException("Value cannot be null or empty.", nameof(userId));

            var key = _userActionStoreKeyGenerator.GenerateKey(userId);
            await _immediateActionsStore.AddAsync(key, new ImmediateActionDataModel(_dateTimeProvider.Now(), AddPurpose.RefreshCookie), cancellationToken);
        }

        public void SignOut(string userId)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentException("Value cannot be null or empty.", nameof(userId));

            var key = _userActionStoreKeyGenerator.GenerateKey(userId);
            _immediateActionsStore.Add(key, new ImmediateActionDataModel(_dateTimeProvider.Now(), AddPurpose.SignOut));
        }

        public async Task SignOutAsync(string userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentException("Value cannot be null or empty.", nameof(userId));

            var key = _userActionStoreKeyGenerator.GenerateKey(userId);
            await _immediateActionsStore.AddAsync(key, new ImmediateActionDataModel(_dateTimeProvider.Now(), AddPurpose.SignOut), cancellationToken);
        }
    }
}