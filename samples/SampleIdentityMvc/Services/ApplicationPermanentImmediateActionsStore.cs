using System;
using System.Threading;
using System.Threading.Tasks;
using Kaktos.UserImmediateActions.Models;
using Kaktos.UserImmediateActions.Stores;
using SampleIdentityMvc.Data;
using SampleIdentityMvc.Models;

namespace SampleIdentityMvc.Services
{
    public class ApplicationPermanentImmediateActionsStore : IPermanentImmediateActionsStore
    {
        private readonly ApplicationDbContext _dbContext;

        public ApplicationPermanentImmediateActionsStore(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void Add(string key, DateTimeOffset expirationTimeUtc, ImmediateActionDataModel data)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentException("Value cannot be null or empty.", nameof(key));
            if (data == null) throw new ArgumentNullException(nameof(data));

            var model = new ImmediateActionDatabaseModel
            {
                ActionKey = key,
                ExpirationTimeUtc = expirationTimeUtc,
                Purpose = data.Purpose,
                AddedDateUtc = data.AddedDate
            };

            _dbContext.ImmediateActionDatabaseModels.Add(model);

            _dbContext.SaveChanges();
        }

        public async Task AddAsync(string key, DateTimeOffset expirationTimeUtc, ImmediateActionDataModel data, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentException("Value cannot be null or empty.", nameof(key));
            if (data == null) throw new ArgumentNullException(nameof(data));

            var model = new ImmediateActionDatabaseModel
            {
                ActionKey = key,
                ExpirationTimeUtc = expirationTimeUtc,
                Purpose = data.Purpose,
                AddedDateUtc = data.AddedDate
            };

            await _dbContext.ImmediateActionDatabaseModels.AddAsync(model, cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}