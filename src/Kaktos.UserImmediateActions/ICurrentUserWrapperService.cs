using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Kaktos.UserImmediateActions
{
    public interface ICurrentUserWrapperService
    {
        /// <summary>
        /// Gets user id
        /// </summary>
        /// <param name="userPrincipal">The user instance</param>
        /// <returns>User id</returns>
        string GetUserId(ClaimsPrincipal userPrincipal);

        /// <summary>
        /// Checks if the user is logged in
        /// </summary>
        /// <param name="userPrincipal">The user instance</param>
        /// <returns><c>true</c> if the user is logged in, else <c>false</c></returns>
        bool IsSignedIn(ClaimsPrincipal userPrincipal);

        /// <summary>
        /// Refresh user sign in AKA regenerate user cookie
        /// </summary>
        /// <param name="userPrincipal">The user instance</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task RefreshSignInAsync(ClaimsPrincipal userPrincipal, CancellationToken cancellationToken = default);

        /// <summary>
        /// Signs out the current user
        /// </summary>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task SignOutAsync(CancellationToken cancellationToken = default);
    }
}