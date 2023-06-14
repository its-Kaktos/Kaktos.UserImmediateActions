using System.Threading;
using System.Threading.Tasks;

namespace Kaktos.UserImmediateActions
{
    public interface IUserImmediateActionsService
    {
        /// <summary>
        /// Refresh cookie AKA regenerate cookie of the user with the given <paramref name="userId"/> next time
        /// they sends a request
        /// </summary>
        /// <param name="userId">User id to refresh their cookie</param>
        void RefreshCookie(string userId);

        /// <summary>
        /// Refresh cookie AKA regenerate cookie of the user with the given <paramref name="userId"/> next time
        /// they sends a request Asynchronously
        /// </summary>
        /// <param name="userId">User id to refresh their cookie</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task RefreshCookieAsync(string userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sings out the user with the given <paramref name="userId"/> next time
        /// they sends a request
        /// </summary>
        /// <remarks>It is recommended to update user Security Stamp when calling this method to ensure
        /// sign out on all devices</remarks>
        /// <param name="userId">The user id to log out of their account</param>
        void SignOut(string userId);

        /// <summary>
        /// Sings out the user with the given <paramref name="userId"/> next time
        /// they sends a request Asynchronously
        /// </summary>
        /// <remarks>It is recommended to update user Security Stamp when calling this method to ensure
        /// sign out on all devices</remarks>
        /// <param name="userId">The user id to log out of their account</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task SignOutAsync(string userId, CancellationToken cancellationToken = default);
    }
}