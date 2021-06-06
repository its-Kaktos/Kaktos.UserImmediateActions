using System.Threading;
using System.Threading.Tasks;

namespace Kaktos.UserImmediateActions
{
    public interface IUserImmediateActionsService
    {
        /// <summary>
        /// Adds <paramref name="userId"/> to the configured store (E.g: Redis cache) so the next time user with the
        /// given <paramref name="userId"/> sends a request, their cookie will be refreshed.
        /// </summary>
        /// <param name="userId">User id to refresh their cookie.</param>
        void RefreshCookie(string userId);

        /// <summary>
        /// Adds <paramref name="userId"/> to the configured store (E.g: Redis cache) so the next time user with the
        /// given <paramref name="userId"/> sends a request, their cookie will be refreshed.
        /// </summary>
        /// <param name="userId">User id to refresh their cookie.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns><see cref="Task"/></returns>
        Task RefreshCookieAsync(string userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds <paramref name="userId"/> to the configured store (E.g: Redis cache) so the next time user with the
        /// given <paramref name="userId"/> sends a request, they will be signed out from their account.
        /// </summary>
        /// <param name="userId">User id to refresh their cookie.</param>
        void SignOut(string userId);

        /// <summary>
        /// Adds <paramref name="userId"/> to the configured store (E.g: Redis cache) so the next time user with the
        /// given <paramref name="userId"/> sends a request, they will be signed out from their account.
        /// </summary>
        /// <param name="userId">User id to refresh their cookie.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns><see cref="Task"/></returns>
        Task SignOutAsync(string userId, CancellationToken cancellationToken = default);
    }
}