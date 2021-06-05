using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using UserImmediateActions.Stores;

namespace UserImmediateActions.Extensions
{
    public static class IdentityBuilderExtensions
    {
        public static IdentityBuilder AddUserImmediateActions(this IdentityBuilder builder)
        {
            var services = builder.Services;

            services.AddMemoryCache();
            services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
            services.TryAddSingleton<IPermanentImmediateActionsStore, FakePermanentImmediateActionsStore>();
            services.TryAddTransient<IImmediateActionsStore, MemoryCacheImmediateActionsStore>();
            services.TryAddSingleton<IUserActionStoreKeyGenerator, UserActionStoreKeyGenerator>();
            services.TryAddTransient<IUserImmediateActionsService, UserImmediateActionsService>();
            services.TryAddScoped(typeof(ICurrentUserWrapperService), typeof(IdentityCurrentUserWrapperService<>).MakeGenericType(builder.UserType));

            return builder;
        }

        public static IdentityBuilder AddPermanentImmediateActionsStore<TPermanentStore>(this IdentityBuilder builder)
            where TPermanentStore : class, IPermanentImmediateActionsStore
        {
            builder.Services.AddTransient<IPermanentImmediateActionsStore, TPermanentStore>();

            return builder;
        }

        public static IdentityBuilder AddDefaultDistributedImmediateActionStore(this IdentityBuilder builder)
        {
            builder.Services.AddTransient<IImmediateActionsStore, DistributedCacheImmediateActionsStore>();

            return builder;
        }

        public static IdentityBuilder AddDistributedImmediateActionStore<TStore>(this IdentityBuilder builder) where TStore : class, IImmediateActionsStore
        {
            builder.Services.AddTransient<IImmediateActionsStore, TStore>();

            return builder;
        }

        public static IdentityBuilder AddUserActionStoreKeyGenerator<TGenerator>(this IdentityBuilder builder) where TGenerator : class, IUserActionStoreKeyGenerator
        {
            builder.Services.AddTransient<IUserActionStoreKeyGenerator, TGenerator>();

            return builder;
        }

        public static IdentityBuilder AddUserImmediateActionsService<TActionService>(this IdentityBuilder builder) where TActionService : class, IUserImmediateActionsService
        {
            builder.Services.AddTransient<IUserImmediateActionsService, TActionService>();

            return builder;
        }

        public static IdentityBuilder AddCurrentUserWrapperService<TUserWrapperService>(this IdentityBuilder builder) where TUserWrapperService : class, ICurrentUserWrapperService
        {
            builder.Services.AddTransient<ICurrentUserWrapperService, TUserWrapperService>();

            return builder;
        }
    }
}