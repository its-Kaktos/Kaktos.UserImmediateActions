using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Kaktos.UserImmediateActions.Models;
using Kaktos.UserImmediateActions.Stores;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kaktos.UserImmediateActions
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class UserImmediateActionsMiddleware
    {
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly TimeSpan _expirationTimeForRefreshCookie;
        private readonly TimeSpan _expirationTimeForSignOut;
        private readonly ILogger<UserImmediateActionsMiddleware> _logger;
        private readonly RequestDelegate _next;

        // Only Used for unit testing
        internal UserImmediateActionsMiddleware(RequestDelegate next,
            ILogger<UserImmediateActionsMiddleware> logger,
            IOptions<CookieAuthenticationOptions> cookieAuthenticationOptions,
            IOptions<SecurityStampValidatorOptions> securityStampValidatorOptions,
            IDateTimeProvider dateTimeProvider)
        {
            _dateTimeProvider = dateTimeProvider;
            _next = next;
            _logger = logger;
            var cookieOptions = cookieAuthenticationOptions?.Value ?? new CookieAuthenticationOptions();
            _expirationTimeForRefreshCookie = cookieOptions.ExpireTimeSpan;
            var securityStampOptions = securityStampValidatorOptions?.Value ?? new SecurityStampValidatorOptions();
            _expirationTimeForSignOut = securityStampOptions.ValidationInterval;
        }

        public UserImmediateActionsMiddleware(RequestDelegate next,
            ILogger<UserImmediateActionsMiddleware> logger,
            IOptions<CookieAuthenticationOptions> cookieAuthenticationOptions,
            IOptions<SecurityStampValidatorOptions> securityStampValidatorOptions)
        {
            _dateTimeProvider = new DateTimeProvider();
            _next = next;
            _logger = logger;
            var cookieOptions = cookieAuthenticationOptions?.Value ?? new CookieAuthenticationOptions();
            _expirationTimeForRefreshCookie = cookieOptions.ExpireTimeSpan;
            var securityStampOptions = securityStampValidatorOptions?.Value ?? new SecurityStampValidatorOptions();
            _expirationTimeForSignOut = securityStampOptions.ValidationInterval;
        }

        public async Task InvokeAsync(HttpContext context,
            IImmediateActionsStore actionsStore,
            IUserActionStoreKeyGenerator userActionStoreKeyGenerator,
            ICurrentUserWrapperService currentUserWrapperService)
        {
            var user = context.User;

            if (currentUserWrapperService.IsSignedIn(user))
            {
                var userId = currentUserWrapperService.GetUserId(user);
                var userActionsStoreKey = userActionStoreKeyGenerator.GenerateKey(userId);

                var actionToPreformOnUser = await actionsStore.GetAsync(userActionsStoreKey);
                if (actionToPreformOnUser != null)
                {
                    var userActionStoreUniqueKey = userActionStoreKeyGenerator.GenerateKey(userId,
                        context.Request.Headers["User-Agent"].ToString(),
                        context.Connection.RemoteIpAddress?.ToString());

                    var previousActionPreformedOnUser = await actionsStore.GetAsync(userActionStoreUniqueKey);

                    await PreformActionOnUserIfNecessaryAsync(actionsStore,
                        currentUserWrapperService,
                        actionToPreformOnUser,
                        previousActionPreformedOnUser,
                        user,
                        userId,
                        userActionStoreUniqueKey);
                }
            }

            await _next(context);
        }

        private async Task PreformActionOnUserIfNecessaryAsync(IImmediateActionsStore actionsStore,
            ICurrentUserWrapperService currentUserWrapperService,
            ImmediateActionDataModel actionToPreformOnUser,
            ImmediateActionDataModel previousActionPreformedOnUser,
            ClaimsPrincipal user,
            string userId,
            string userActionStoreUniqueKey)
        {
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (actionToPreformOnUser.Purpose)
            {
                case AddPurpose.RefreshCookie:
                    var doesUseCookieNeedToBeRefreshed = previousActionPreformedOnUser == null ||
                                                         previousActionPreformedOnUser.AddedDate < actionToPreformOnUser.AddedDate;
                    if (doesUseCookieNeedToBeRefreshed)
                    {
                        await currentUserWrapperService.RefreshSignInAsync(user);
                        await actionsStore.AddAsync(userActionStoreUniqueKey,
                            _expirationTimeForRefreshCookie,
                            new ImmediateActionDataModel(_dateTimeProvider.UtcNow(), AddPurpose.UserCookieWasRefreshed));

                        _logger.LogInformation("User cookie with userId '{UserId}' has been refreshed", userId);
                    }

                    break;
                case AddPurpose.SignOut:
                    var doesUserNeedToBeSingedOut = previousActionPreformedOnUser == null ||
                                                    previousActionPreformedOnUser.AddedDate < actionToPreformOnUser.AddedDate;
                    if (doesUserNeedToBeSingedOut)
                    {
                        await currentUserWrapperService.SignOutAsync();
                        await actionsStore.AddAsync(userActionStoreUniqueKey,
                            _expirationTimeForSignOut,
                            new ImmediateActionDataModel(_dateTimeProvider.UtcNow(), AddPurpose.UserWasSignedOut));

                        _logger.LogInformation("User with userId '{UserId}' has been signed out", userId);
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(actionToPreformOnUser),
                        $"The action to preform on use either is something that is not possible E.g: AddPurpose.UserWasSignedOut" +
                        $" or the action is out of the range of the enum");
            }
        }
    }
}