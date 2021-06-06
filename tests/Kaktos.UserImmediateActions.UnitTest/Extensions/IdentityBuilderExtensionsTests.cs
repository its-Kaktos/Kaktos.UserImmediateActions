using System;
using System.Linq;
using FluentAssertions;
using Kaktos.UserImmediateActions.Extensions;
using Kaktos.UserImmediateActions.Stores;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace Kaktos.UserImmediateActions.UnitTest.Extensions
{
    public class IdentityBuilderExtensionsTests
    {
        private readonly IServiceCollection _sut = new ServiceCollection();
        private readonly Type _userType = typeof(IdentityUser);
        private readonly IdentityBuilder _identityBuilder;

        public IdentityBuilderExtensionsTests()
        {
            _identityBuilder = new IdentityBuilder(_userType, _sut);
        }

        [Fact]
        public void AddUserImmediateActions_ShouldAddAllDefaultServicesToDi()
        {
            // Arrange
            var expected = new ServiceCollection()
                .AddMemoryCache()
                .AddSingleton<IPermanentImmediateActionsStore, FakePermanentImmediateActionsStore>()
                .AddSingleton<IDateTimeProvider, DateTimeProvider>()
                .AddTransient<IImmediateActionsStore, MemoryCacheImmediateActionsStore>()
                .AddSingleton<IUserActionStoreKeyGenerator, UserActionStoreKeyGenerator>()
                .AddTransient<IUserImmediateActionsService, UserImmediateActionsService>()
                .AddScoped(typeof(ICurrentUserWrapperService), typeof(IdentityCurrentUserWrapperService<>).MakeGenericType(_userType));

            // Act
            _identityBuilder.AddUserImmediateActions();

            // Assert
            _sut.Count.Should().Be(expected.Count);

            foreach (var service in expected)
            {
                _sut.Any(descriptor => descriptor.Lifetime == service.Lifetime &&
                                       descriptor.ImplementationFactory == service.ImplementationFactory &&
                                       descriptor.ImplementationInstance == service.ImplementationInstance &&
                                       descriptor.ImplementationType == service.ImplementationType &&
                                       descriptor.ServiceType == service.ServiceType)
                    .Should().BeTrue();
            }
        }

        [Fact]
        public void AddPermanentImmediateActionsStore_ShouldAdd_PermanentImmediateActionsStore_ToDi()
        {
            // Arrange
            var expected = new ServiceCollection();
            expected.TryAddSingleton<IPermanentImmediateActionsStore, FakePermanentImmediateActionsStore>();
            expected.AddTransient<IPermanentImmediateActionsStore, IPermanentImmediateActionsStore>();

            // Act
            _identityBuilder.AddUserImmediateActions()
                .AddPermanentImmediateActionsStore<IPermanentImmediateActionsStore>();

            // Assert
            foreach (var service in expected)
            {
                _sut.Any(descriptor => descriptor.Lifetime == service.Lifetime &&
                                       descriptor.ImplementationFactory == service.ImplementationFactory &&
                                       descriptor.ImplementationInstance == service.ImplementationInstance &&
                                       descriptor.ImplementationType == service.ImplementationType &&
                                       descriptor.ServiceType == service.ServiceType)
                    .Should().BeTrue();
            }
        }
        
        [Fact]
        public void AddDefaultDistributedImmediateActionStore_ShouldAddDefault_DistributedImmediateActionStore_ToDi()
        {
            // Arrange
            var expected = new ServiceCollection();
            expected.TryAddTransient<IImmediateActionsStore, MemoryCacheImmediateActionsStore>();
            expected.AddTransient<IImmediateActionsStore, DistributedCacheImmediateActionsStore>();

            // Act
            _identityBuilder.AddUserImmediateActions()
                .AddDefaultDistributedImmediateActionStore();

            // Assert
            foreach (var service in expected)
            {
                _sut.Any(descriptor => descriptor.Lifetime == service.Lifetime &&
                                       descriptor.ImplementationFactory == service.ImplementationFactory &&
                                       descriptor.ImplementationInstance == service.ImplementationInstance &&
                                       descriptor.ImplementationType == service.ImplementationType &&
                                       descriptor.ServiceType == service.ServiceType)
                    .Should().BeTrue();
            }
        }

        [Fact]
        public void AddDistributedImmediateActionStore_ShouldAdd_DistributedImmediateActionStore_ToDi()
        {
            // Arrange
            var expected = new ServiceCollection();
            expected.TryAddTransient<IImmediateActionsStore, MemoryCacheImmediateActionsStore>();
            expected.AddTransient<IImmediateActionsStore, IImmediateActionsStore>();

            // Act
            _identityBuilder.AddUserImmediateActions()
                .AddDistributedImmediateActionStore<IImmediateActionsStore>();

            // Assert
            foreach (var service in expected)
            {
                _sut.Any(descriptor => descriptor.Lifetime == service.Lifetime &&
                                       descriptor.ImplementationFactory == service.ImplementationFactory &&
                                       descriptor.ImplementationInstance == service.ImplementationInstance &&
                                       descriptor.ImplementationType == service.ImplementationType &&
                                       descriptor.ServiceType == service.ServiceType)
                    .Should().BeTrue();
            }
        }

        [Fact]
        public void AddUserActionStoreKeyGenerator_ShouldAdd_UserActionStoreKeyGenerator_ToDi()
        {
            // Arrange
            var expected = new ServiceCollection();
            expected.TryAddSingleton<IUserActionStoreKeyGenerator, UserActionStoreKeyGenerator>();
            expected.AddTransient<IUserActionStoreKeyGenerator, IUserActionStoreKeyGenerator>();

            // Act
            _identityBuilder.AddUserImmediateActions()
                .AddUserActionStoreKeyGenerator<IUserActionStoreKeyGenerator>();

            // Assert
            foreach (var service in expected)
            {
                _sut.Any(descriptor => descriptor.Lifetime == service.Lifetime &&
                                       descriptor.ImplementationFactory == service.ImplementationFactory &&
                                       descriptor.ImplementationInstance == service.ImplementationInstance &&
                                       descriptor.ImplementationType == service.ImplementationType &&
                                       descriptor.ServiceType == service.ServiceType)
                    .Should().BeTrue();
            }
        }

        [Fact]
        public void AddUserImmediateActionsService_ShouldAdd_UserImmediateActionsService_ToDi()
        {
            // Arrange
            var expected = new ServiceCollection();
            expected.TryAddTransient<IUserImmediateActionsService, UserImmediateActionsService>();
            expected.AddTransient<IUserImmediateActionsService, IUserImmediateActionsService>();

            // Act
            _identityBuilder.AddUserImmediateActions()
                .AddUserImmediateActionsService<IUserImmediateActionsService>();

            // Assert
            foreach (var service in expected)
            {
                _sut.Any(descriptor => descriptor.Lifetime == service.Lifetime &&
                                       descriptor.ImplementationFactory == service.ImplementationFactory &&
                                       descriptor.ImplementationInstance == service.ImplementationInstance &&
                                       descriptor.ImplementationType == service.ImplementationType &&
                                       descriptor.ServiceType == service.ServiceType)
                    .Should().BeTrue();
            }
        }

        [Fact]
        public void AddCurrentUserWrapperService_ShouldAdd_CurrentUserWrapperService_ToDi()
        {
            // Arrange
            var expected = new ServiceCollection();
            expected.TryAddScoped(typeof(ICurrentUserWrapperService),
                typeof(IdentityCurrentUserWrapperService<>).MakeGenericType(_userType));
            expected.AddTransient<ICurrentUserWrapperService, ICurrentUserWrapperService>();

            // Act
            _identityBuilder.AddUserImmediateActions()
                .AddCurrentUserWrapperService<ICurrentUserWrapperService>();

            // Assert
            foreach (var service in expected)
            {
                _sut.Any(descriptor => descriptor.Lifetime == service.Lifetime &&
                                       descriptor.ImplementationFactory == service.ImplementationFactory &&
                                       descriptor.ImplementationInstance == service.ImplementationInstance &&
                                       descriptor.ImplementationType == service.ImplementationType &&
                                       descriptor.ServiceType == service.ServiceType)
                    .Should().BeTrue();
            }
        }
    }
}