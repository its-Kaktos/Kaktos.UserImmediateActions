using System;
using System.Threading;
using System.Threading.Tasks;
using UserImmediateActions.Models;

namespace UserImmediateActions.Stores
{
    public sealed class FakePermanentImmediateActionsStore : IPermanentImmediateActionsStore
    {
        public void Add(string key, DateTime expirationTime, ImmediateActionDataModel data)
        {
        }

        public Task AddAsync(string key, DateTime expirationTime, ImmediateActionDataModel data, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}