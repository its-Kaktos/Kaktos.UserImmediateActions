using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Kaktos.UserImmediateActions
{
    public class IdentityCurrentUserWrapperService<TUser> : ICurrentUserWrapperService where TUser : IdentityUser
    {
        private readonly SignInManager<TUser> _signInManager;
        private readonly UserManager<TUser> _userManager;
        private readonly string _userIdClaimType;

        public IdentityCurrentUserWrapperService(SignInManager<TUser> signInManager, UserManager<TUser> userManager, IOptions<IdentityOptions> options)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            var identityOptions = options?.Value ?? new IdentityOptions();
            _userIdClaimType = identityOptions.ClaimsIdentity.UserIdClaimType;
        }

        public string GetUserId(ClaimsPrincipal userPrincipal)
        {
            if (userPrincipal == null) throw new ArgumentNullException(nameof(userPrincipal));

            return userPrincipal.FindFirstValue(_userIdClaimType);
        }

        public bool IsSignedIn(ClaimsPrincipal userPrincipal)
        {
            if (userPrincipal == null) throw new ArgumentNullException(nameof(userPrincipal));

            return _signInManager.IsSignedIn(userPrincipal);
        }

        public async Task RefreshSignInAsync(ClaimsPrincipal userPrincipal, CancellationToken cancellationToken = default)
        {
            if (userPrincipal == null) throw new ArgumentNullException(nameof(userPrincipal));

            var userId = GetUserId(userPrincipal);
            var user = await _userManager.FindByIdAsync(userId);
            await _signInManager.RefreshSignInAsync(user);
        }

        public async Task SignOutAsync(CancellationToken cancellationToken = default)
        {
            await _signInManager.SignOutAsync();
        }
    }
}