using System.Threading.Tasks;
using Kaktos.UserImmediateActions.Stores;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SampleIdentityMvc.Data;
using SampleIdentityMvc.Services;

namespace SampleIdentityMvc
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            
            // Add all the Immediate actions from db to the Main store.
            // اضافه کردن تمامی اکشن های موجود در دیتابیس به ذخیره ساز اصلی
            using (var scope = host.Services.CreateScope())
            {
                var actionsStore = scope.ServiceProvider.GetService<IImmediateActionsStore>();
                var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();
                var service = new AddImmediateActionsFromDbToIImmediateActionsStore(actionsStore, dbContext);
                await service.AddNonExpiredImmediateActionsFromDbToIImmediateActionsStoreAsync();
            }

            await host.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}