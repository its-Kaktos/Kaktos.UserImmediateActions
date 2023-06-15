using System;
using System.Threading;
using System.Threading.Tasks;
using Kaktos.UserImmediateActions.Models;

namespace Kaktos.UserImmediateActions.Stores
{
    public interface IPermanentImmediateActionsStore
    {
        /// <summary>
        /// Stores the <paramref name="data"/> with the given <paramref name="key"/> to a permanent store like a database
        /// <remarks>
        /// Make sure to remove the <paramref name="key"/> and <paramref name="data"/> when <paramref name="expirationTimeUtc"/> is reached.
        /// you can use Quartz to achieve this functionality.
        /// </remarks>
        /// </summary>
        /// <param name="key">Unique Key</param>
        /// <param name="expirationTimeUtc">Expiration time of the <paramref name="data"/></param>
        /// <param name="data"><see cref="ImmediateActionDataModel"/></param>
        void Add(string key, DateTimeOffset expirationTimeUtc, ImmediateActionDataModel data);

        /// <summary>
        /// Stores the <paramref name="data"/> with the given <paramref name="key"/> to a permanent store like a database Asynchronously
        /// <remarks>
        /// Make sure to remove the <paramref name="key"/> and <paramref name="data"/> when <paramref name="expirationTimeUtc"/> is reached.
        /// you can use Quartz to achieve this functionality.
        /// </remarks>
        /// </summary>
        /// <param name="key">Unique Key</param>
        /// <param name="expirationTimeUtc">Expiration time of the <paramref name="data"/></param>
        /// <param name="data"><see cref="ImmediateActionDataModel"/></param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task AddAsync(string key, DateTimeOffset expirationTimeUtc, ImmediateActionDataModel data, CancellationToken cancellationToken = default);
    }
}