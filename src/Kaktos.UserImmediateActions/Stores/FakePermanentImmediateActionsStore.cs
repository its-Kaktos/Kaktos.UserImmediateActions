using System;
using System.Threading;
using System.Threading.Tasks;
using Kaktos.UserImmediateActions.Models;

namespace Kaktos.UserImmediateActions.Stores
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