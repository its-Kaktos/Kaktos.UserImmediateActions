using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using UserImmediateActions.Models;
using UserImmediateActions.Stores;

namespace UserImmediateActions
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class UserImmediateActionsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<UserImmediateActionsMiddleware> _logger;

        public UserImmediateActionsMiddleware(RequestDelegate next, ILogger<UserImmediateActionsMiddleware> logger)
        {
            _next = next;
            _logger = logger;
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
                            new ImmediateActionDataModel(DateTime.Now, AddPurpose.UserCookieWasRefreshed));

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
                            new ImmediateActionDataModel(DateTime.Now, AddPurpose.UserWasSignedOut));

                        _logger.LogInformation("User with userId '{UserId}' has been signed out", userId);
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}