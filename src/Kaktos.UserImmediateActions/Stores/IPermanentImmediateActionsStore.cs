using System;
using System.Threading;
using System.Threading.Tasks;
using Kaktos.UserImmediateActions.Models;

namespace Kaktos.UserImmediateActions.Stores
{
    public interface IPermanentImmediateActionsStore
    {
        /// <summary>
        /// This method gets called inside the Add method of <see cref="IImmediateActionsStore"/>.
        /// you can use this method to store the <paramref name="key"/> and <paramref name="data"/> to a permanent store like a DB.
        /// <para>
        /// Make sure to remove the <paramref name="key"/> and <paramref name="data"/> when <paramref name="expirationTime"/> is reached.
        /// you can use Quartz to achieve this functionality.
        /// </para>
        /// </summary>
        /// <param name="key">The key that is passed to the Add method of <see cref="IImmediateActionsStore"/>.</param>
        /// <param name="expirationTime">Expiration time that is passed to the Add method of <see cref="IImmediateActionsStore"/>.</param>
        /// <param name="data">The data that is passed to the Add method of <see cref="IImmediateActionsStore"/>.</param>
        void Add(string key, DateTime expirationTime, ImmediateActionDataModel data);

        /// <summary>
        /// This method gets called inside the AddAsync method of <see cref="IImmediateActionsStore"/>.
        /// you can use this method to store the <paramref name="key"/> and <paramref name="data"/> to a permanent store like a DB.
        /// <para>
        /// Make sure to remove the <paramref name="key"/> and <paramref name="data"/> when <paramref name="expirationTime"/> is reached.
        /// you can use Quartz to achieve this functionality.
        /// </para>
        /// </summary>
        /// <param name="key">The key that is passed to the AddAsync method of <see cref="IImmediateActionsStore"/>.</param>
        /// <param name="expirationTime">Expiration time that is passed to the AddAsync method of <see cref="IImmediateActionsStore"/>.</param>
        /// <param name="data">The data that is passed to the AddAsync method of <see cref="IImmediateActionsStore"/>.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns><see cref="Task"/></returns>
        Task AddAsync(string key, DateTime expirationTime, ImmediateActionDataModel data, CancellationToken cancellationToken = default);
    }
}