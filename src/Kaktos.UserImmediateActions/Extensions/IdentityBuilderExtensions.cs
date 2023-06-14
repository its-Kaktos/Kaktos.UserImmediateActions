using Kaktos.UserImmediateActions.Stores;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Kaktos.UserImmediateActions.Extensions
{
    public static class IdentityBuilderExtensions
    {
        /// <summary>
        /// Adds default services
        /// </summary>
        /// <param name="builder"><see cref="IdentityBuilder"/></param>
        /// <returns><see cref="IdentityBuilder"/></returns>
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

        /// <summary>
        /// Adds an implementation of permanent store
        /// </summary>
        /// <param name="builder"><see cref="IdentityBuilder"/></param>
        /// <typeparam name="TPermanentStore">An implementation of <see cref="IPermanentImmediateActionsStore"/></typeparam>
        /// <returns><see cref="IdentityBuilder"/></returns>
        public static IdentityBuilder AddPermanentImmediateActionsStore<TPermanentStore>(this IdentityBuilder builder)
            where TPermanentStore : class, IPermanentImmediateActionsStore
        {
            builder.Services.AddTransient<IPermanentImmediateActionsStore, TPermanentStore>();

            return builder;
        }

        /// <summary>
        /// Adds the default distributed cache service 
        /// </summary>
        /// <param name="builder"><see cref="IdentityBuilder"/></param>
        /// <returns><see cref="IdentityBuilder"/></returns>
        public static IdentityBuilder AddDefaultDistributedImmediateActionStore(this IdentityBuilder builder)
        {
            builder.Services.AddTransient<IImmediateActionsStore, DistributedCacheImmediateActionsStore>();

            return builder;
        }

        /// <summary>
        /// Adds an implementation of distributed cache service
        /// </summary>
        /// <param name="builder"><see cref="IdentityBuilder"/></param>
        /// <typeparam name="TStore">An implementation of <see cref="IImmediateActionsStore"/></typeparam>
        /// <returns><see cref="IdentityBuilder"/></returns>
        public static IdentityBuilder AddDistributedImmediateActionStore<TStore>(this IdentityBuilder builder) where TStore : class, IImmediateActionsStore
        {
            builder.Services.AddTransient<IImmediateActionsStore, TStore>();

            return builder;
        }

        /// <summary>
        /// Adds an implementation of user action store key generator service
        /// </summary>
        /// <param name="builder"><see cref="IdentityBuilder"/></param>
        /// <typeparam name="TGenerator">An implementation of <see cref="IUserActionStoreKeyGenerator"/></typeparam>
        /// <returns><see cref="IdentityBuilder"/></returns>
        public static IdentityBuilder AddUserActionStoreKeyGenerator<TGenerator>(this IdentityBuilder builder) where TGenerator : class, IUserActionStoreKeyGenerator
        {
            builder.Services.AddTransient<IUserActionStoreKeyGenerator, TGenerator>();

            return builder;
        }

        /// <summary>
        /// Adds an implementation of user immediate actions service
        /// </summary>
        /// <param name="builder"><see cref="IdentityBuilder"/></param>
        /// <typeparam name="TActionService">An implementation of <see cref="IUserImmediateActionsService"/></typeparam>
        /// <returns><see cref="IdentityBuilder"/></returns>
        public static IdentityBuilder AddUserImmediateActionsService<TActionService>(this IdentityBuilder builder) where TActionService : class, IUserImmediateActionsService
        {
            builder.Services.AddTransient<IUserImmediateActionsService, TActionService>();

            return builder;
        }

        /// <summary>
        /// Adds an implementation of current user wrapper service
        /// </summary>
        /// <param name="builder"><see cref="IdentityBuilder"/></param>
        /// <typeparam name="TUserWrapperService">An implementation of <see cref="ICurrentUserWrapperService"/></typeparam>
        /// <returns><see cref="IdentityBuilder"/></returns>
        public static IdentityBuilder AddCurrentUserWrapperService<TUserWrapperService>(this IdentityBuilder builder) where TUserWrapperService : class, ICurrentUserWrapperService
        {
            builder.Services.AddTransient<ICurrentUserWrapperService, TUserWrapperService>();

            return builder;
        }
    }
}