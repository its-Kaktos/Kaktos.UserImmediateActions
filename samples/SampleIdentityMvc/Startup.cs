using Kaktos.UserImmediateActions.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using SampleIdentityMvc.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using SampleIdentityMvc.Quartz;
using SampleIdentityMvc.Quartz.JobFactories;
using SampleIdentityMvc.Quartz.Jobs;
using SampleIdentityMvc.Services;

namespace SampleIdentityMvc
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(
                    Configuration.GetConnectionString("DefaultConnection")));
            services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>()
                // Add default services to IoC Container.
                // اضافه کردن سرویس های پیشفرض
                .AddUserImmediateActions()
                // Add our custom permanent store.
                // اضافه کردن سرویس ذخیره ساز دائمی
                .AddPermanentImmediateActionsStore<ApplicationPermanentImmediateActionsStore>();

            services.AddControllersWithViews();

            // Add Quartz services and jobs
            services.AddSingleton<IJobFactory, SingletonJobFactory>();
            services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();
            services.AddHostedService<QuartzHostedService>();
            services.AddSingleton<RemoveExpiredImmediateActionDatabaseModelFromDbJob>();
            services.AddSingleton(new JobSchedule(
                jobType: typeof(RemoveExpiredImmediateActionDatabaseModelFromDbJob),
                // job will run every 6 hours to remove expired ImmediateActionDatabaseModel from database.
                // این جاب هر 6 ساعت استارت میشه تا مدل هایی که تاریخ انتقاضاشون گذشته رو از دیتابیس پاک کنه
                cronExpression: "0 0 0/6 1/1 * ? *"));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();

            // Use this middleware between 'UseAuthentication' and 'UseAuthorization'
            // استفاده کنید UseAuthentication و UseAuthorization از این میدل ور بین
            app.UseUserImmediateActions(); 

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }
    }
}