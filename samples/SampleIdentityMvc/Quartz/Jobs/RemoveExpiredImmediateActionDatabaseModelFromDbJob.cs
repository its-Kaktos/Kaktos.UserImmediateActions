using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using SampleIdentityMvc.Data;

namespace SampleIdentityMvc.Quartz.Jobs
{
    [DisallowConcurrentExecution]
    public class RemoveExpiredImmediateActionDatabaseModelFromDbJob : IJob
    {
        private readonly IServiceProvider _provider;
        private ILogger<RemoveExpiredImmediateActionDatabaseModelFromDbJob> _logger;

        public RemoveExpiredImmediateActionDatabaseModelFromDbJob(IServiceProvider provider, ILogger<RemoveExpiredImmediateActionDatabaseModelFromDbJob> logger)
        {
            _provider = provider;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            using var scope = _provider.CreateScope();
            var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();
            if (dbContext == null) throw new NullReferenceException("DbContext is null, can not execute job");

            _logger.LogInformation("{JobName} Init", nameof(RemoveExpiredImmediateActionDatabaseModelFromDbJob));

            if (await dbContext.ImmediateActionDatabaseModels.AnyAsync())
            {
                var expiredImmediateActions = await dbContext.ImmediateActionDatabaseModels
                    .Where(i => i.ExpirationTimeUtc < DateTimeOffset.UtcNow)
                    .ToListAsync();

                dbContext.RemoveRange(expiredImmediateActions);

                await dbContext.SaveChangesAsync();
            }

            _logger.LogInformation("{JobName} completed successfully", nameof(RemoveExpiredImmediateActionDatabaseModelFromDbJob));
        }
    }
}