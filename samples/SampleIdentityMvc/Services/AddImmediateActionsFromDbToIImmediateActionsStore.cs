using System;
using System.Linq;
using System.Threading.Tasks;
using Kaktos.UserImmediateActions.Models;
using Kaktos.UserImmediateActions.Stores;
using Microsoft.EntityFrameworkCore;
using SampleIdentityMvc.Data;

namespace SampleIdentityMvc.Services
{
    public class AddImmediateActionsFromDbToIImmediateActionsStore
    {
        private readonly IImmediateActionsStore _actionsStore;
        private readonly ApplicationDbContext _dbContext;

        public AddImmediateActionsFromDbToIImmediateActionsStore(IImmediateActionsStore actionsStore, ApplicationDbContext dbContext)
        {
            _actionsStore = actionsStore;
            _dbContext = dbContext;
        }

        public async Task AddNonExpiredImmediateActionsFromDbToIImmediateActionsStoreAsync()
        {
            if (!await _dbContext.ImmediateActionDatabaseModels.AnyAsync()) return;

            var nonExpiredImmediateActions = await _dbContext.ImmediateActionDatabaseModels
                .Where(i => i.ExpirationTime > DateTime.Now)
                .ToListAsync();

            foreach (var immediateAction in nonExpiredImmediateActions)
            {
                var expirationTime = immediateAction.ExpirationTime - DateTime.Now;
                var data = new ImmediateActionDataModel(immediateAction.AddedDate, immediateAction.Purpose);
                
                await _actionsStore.AddAsync(immediateAction.ActionKey,
                    expirationTime,
                    data,
                    // Because we just got the data from DB, we dont want to add it to DB again.
                    // چون که اطلاعات رو تازه از دیتابیس گرفتیم، نمیخوایم باز اطلاعات به دیتابیس اضافه بشن
                    false);
            }
        }
    }
}