using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace UserImmediateActions
{
    public interface ICurrentUserWrapperService
    {
        /// <summary>
        /// Returns user id.
        /// </summary>
        /// <param name="userPrincipal">The user instance.</param>
        /// <returns>User id.</returns>
        string GetUserId(ClaimsPrincipal userPrincipal);

        /// <summary>
        /// Returns true if user is logged in.
        /// </summary>
        /// <param name="userPrincipal">The user instance.</param>
        /// <returns>True if the user is logged in, else false.</returns>
        bool IsSignedIn(ClaimsPrincipal userPrincipal);

        /// <summary>
        /// Refresh user sign in.
        /// </summary>
        /// <param name="userPrincipal">The user principal to refresh.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task RefreshSignInAsync(ClaimsPrincipal userPrincipal, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Signs out the user.
        /// </summary>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task SignOutAsync(CancellationToken cancellationToken = default);
    }
}