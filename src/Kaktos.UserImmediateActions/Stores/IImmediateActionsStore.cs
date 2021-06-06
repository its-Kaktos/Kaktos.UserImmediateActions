using System;
using System.Threading;
using System.Threading.Tasks;
using Kaktos.UserImmediateActions.Models;

namespace Kaktos.UserImmediateActions.Stores
{
    public interface IImmediateActionsStore
    {
        /// <summary>
        /// Adds given <paramref name="key"/> to the underlying store.
        /// </summary>
        /// <param name="key">The unique key to store.</param>
        /// <param name="expirationTime">Expiration time relative to now.</param>
        /// <param name="data">The data to add to the underlying store.</param>
        /// <returns><see cref="Task"/></returns>
        void Add(string key, TimeSpan expirationTime, ImmediateActionDataModel data);

        /// <summary>
        /// Adds given <paramref name="key"/> to the underlying store asynchronously.
        /// </summary>
        /// <param name="key">The unique key to store.</param>
        /// <param name="expirationTime">Expiration time relative to now.</param>
        /// <param name="data">The data to add to the underlying store.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns><see cref="Task"/></returns>
        Task AddAsync(string key, TimeSpan expirationTime, ImmediateActionDataModel data, CancellationToken cancellationToken = default);

        /// <summary>
        /// If key is found in the underlying store, its <see cref="AddPurpose"/> will be returned, else will return <c>null</c>.
        /// </summary>
        /// <param name="key">The key you want to check.</param>
        /// <returns>The <see cref="AddPurpose"/> if <paramref name="key"/> is found, else will return <c>null</c>.</returns>
        ImmediateActionDataModel Get(string key);

        /// <summary>
        /// If key is found in the underlying store, its <see cref="AddPurpose"/> will be returned, else will return <c>null</c>.
        /// </summary>
        /// <param name="key">The key you want to check.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns>The <see cref="Task"/> of <see cref="AddPurpose"/> if <paramref name="key"/> is found, else will return <c>null</c>.</returns>
        Task<ImmediateActionDataModel> GetAsync(string key, CancellationToken cancellationToken = default);
    }
}