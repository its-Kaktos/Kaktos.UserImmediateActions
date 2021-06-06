using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using UserImmediateActions.Models;
using UserImmediateActions.Stores;
using Xunit;

namespace UserImmediateActions.UnitTest
{
    public class UserImmediateActionsMiddlewareTests
    {
        private readonly Mock<IImmediateActionsStore> _immediateActionsStoreMock = new();
        private readonly Mock<IUserActionStoreKeyGenerator> _userActionStoreKeyGeneratorMock = new();
        private readonly Mock<ICurrentUserWrapperService> _currentUserWrapperServiceMock = new();
        private readonly Mock<HttpContext> _httpContextMock = new();
        private readonly Mock<ILogger<UserImmediateActionsMiddleware>> _loggerMock = new();
        private static readonly TimeSpan ExpirationTimeForRefreshCookie = TimeSpan.FromDays(14);
        private static readonly TimeSpan ExpirationTimeForSignOut = TimeSpan.FromMinutes(30);
        private readonly UserImmediateActionsMiddleware _sut;

        public UserImmediateActionsMiddlewareTests()
        {
            _sut = new UserImmediateActionsMiddleware(_ => Task.CompletedTask,
                _loggerMock.Object,
                new OptionsWrapper<CookieAuthenticationOptions>(new CookieAuthenticationOptions()),
                new OptionsWrapper<SecurityStampValidatorOptions>(new SecurityStampValidatorOptions()));
        }

        [Fact]
        public async Task ShouldDoNothing_When_UserIsNotLoggedIn()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var user = new ClaimsPrincipal(new List<ClaimsIdentity>
            {
                new(new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, userId)
                }, string.Empty)
            });
            _currentUserWrapperServiceMock.Setup(_ => _.IsSignedIn(It.IsAny<ClaimsPrincipal>()))
                .Returns(false);
            _httpContextMock.Setup(_ => _.User)
                .Returns(user);

            // Act
            await _sut.InvokeAsync(_httpContextMock.Object,
                _immediateActionsStoreMock.Object,
                _userActionStoreKeyGeneratorMock.Object,
                _currentUserWrapperServiceMock.Object);

            // Assert
            _currentUserWrapperServiceMock.Verify(_ => _.IsSignedIn(It.Is<ClaimsPrincipal>(c =>
                    c.HasClaim(ClaimTypes.NameIdentifier, userId))),
                Times.Once);
        }

        [Fact]
        public async Task ShouldDoNothing_When_UserIsNotInActionStore()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var userAgent = Guid.NewGuid().ToString();
            const string userIp = "192.168.1.1";
            var user = new ClaimsPrincipal(new List<ClaimsIdentity>
            {
                new(new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, userId)
                }, IdentityConstants.ApplicationScheme)
            });

            var userActionsStoreKey = Guid.NewGuid().ToString();
            var userActionStoreUniqueKey = Guid.NewGuid().ToString();
            _currentUserWrapperServiceMock.Setup(_ => _.IsSignedIn(It.IsAny<ClaimsPrincipal>())).Returns(true);
            _currentUserWrapperServiceMock.Setup(_ => _.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);
            _userActionStoreKeyGeneratorMock.Setup(_ => _.GenerateKey(It.IsAny<string>())).Returns(userActionsStoreKey);
            _userActionStoreKeyGeneratorMock.Setup(_ => _.GenerateKey(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(userActionStoreUniqueKey);
            _currentUserWrapperServiceMock.Setup(_ => _.IsSignedIn(It.IsAny<ClaimsPrincipal>()))
                .Returns(true);
            _httpContextMock.Setup(_ => _.User)
                .Returns(user);
            _httpContextMock.Setup(_ => _.Request.Headers)
                .Returns(() => new HeaderDictionary
                {
                    {"User-Agent", userAgent}
                });
            _httpContextMock.Setup(_ => _.Connection.RemoteIpAddress)
                .Returns(() => IPAddress.Parse(userIp));
            _immediateActionsStoreMock.Setup(_ => _.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ImmediateActionDataModel) null);

            // Act
            await _sut.InvokeAsync(_httpContextMock.Object,
                _immediateActionsStoreMock.Object,
                _userActionStoreKeyGeneratorMock.Object,
                _currentUserWrapperServiceMock.Object);

            // Assert
            _currentUserWrapperServiceMock.Verify(_ => _.IsSignedIn(It.Is<ClaimsPrincipal>(c =>
                    c.HasClaim(ClaimTypes.NameIdentifier, userId))),
                Times.Once);
            _userActionStoreKeyGeneratorMock.Verify(_ => _.GenerateKey(It.Is<string>(s => s == userId)),
                Times.Once);
            _userActionStoreKeyGeneratorMock.Verify(_ => _.GenerateKey(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()),
                Times.Never);
            _immediateActionsStoreMock.Verify(_ => _.GetAsync(It.Is<string>(s => s == userActionsStoreKey), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Theory]
        [MemberData(nameof(GetDataFor_ShouldDoNothing_When_UserIsAddedForAnAction_But_TheActionIsAlreadyPreformed))]
        public async Task ShouldDoNothing_When_UserIsAddedForAnAction_But_TheActionIsAlreadyPreformed(
            ImmediateActionDataModel actionToPreformOnUser,
            ImmediateActionDataModel previousActionPreformedOnUser)
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var userAgent = Guid.NewGuid().ToString();
            const string userIp = "192.168.1.1";
            var user = new ClaimsPrincipal(new List<ClaimsIdentity>
            {
                new(new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, userId)
                }, IdentityConstants.ApplicationScheme)
            });

            var userActionsStoreKey = Guid.NewGuid().ToString();
            var userActionStoreUniqueKey = Guid.NewGuid().ToString();
            _currentUserWrapperServiceMock.Setup(_ => _.IsSignedIn(It.IsAny<ClaimsPrincipal>())).Returns(true);
            _currentUserWrapperServiceMock.Setup(_ => _.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);
            _userActionStoreKeyGeneratorMock.Setup(_ => _.GenerateKey(It.IsAny<string>())).Returns(userActionsStoreKey);
            _userActionStoreKeyGeneratorMock.Setup(_ => _.GenerateKey(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(userActionStoreUniqueKey);
            _currentUserWrapperServiceMock.Setup(_ => _.IsSignedIn(It.IsAny<ClaimsPrincipal>()))
                .Returns(true);
            _httpContextMock.Setup(_ => _.User)
                .Returns(user);
            _httpContextMock.Setup(_ => _.Request.Headers)
                .Returns(() => new HeaderDictionary
                {
                    {"User-Agent", userAgent}
                });
            _httpContextMock.Setup(_ => _.Connection.RemoteIpAddress)
                .Returns(() => IPAddress.Parse(userIp));
            _immediateActionsStoreMock.SetupSequence(_ => _.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(actionToPreformOnUser)
                .ReturnsAsync(previousActionPreformedOnUser);

            // Act
            await _sut.InvokeAsync(_httpContextMock.Object,
                _immediateActionsStoreMock.Object,
                _userActionStoreKeyGeneratorMock.Object,
                _currentUserWrapperServiceMock.Object);

            // Assert
            _currentUserWrapperServiceMock.Verify(_ => _.IsSignedIn(It.Is<ClaimsPrincipal>(c =>
                    c.HasClaim(ClaimTypes.NameIdentifier, userId))),
                Times.Once);
            _userActionStoreKeyGeneratorMock.Verify(_ => _.GenerateKey(It.Is<string>(s => s == userId)),
                Times.Once);
            _userActionStoreKeyGeneratorMock.Verify(_ => _.GenerateKey(
                    It.Is<string>(s => s == userId),
                    It.Is<string>(s => s == userAgent),
                    It.Is<string>(s => s == userIp)),
                Times.Once);
            _immediateActionsStoreMock.Verify(_ => _.GetAsync(It.Is<string>(s => s == userActionsStoreKey), It.IsAny<CancellationToken>()),
                Times.Once);
            _immediateActionsStoreMock.Verify(_ => _.GetAsync(It.Is<string>(s => s == userActionStoreUniqueKey), It.IsAny<CancellationToken>()),
                Times.Once);
            _currentUserWrapperServiceMock.Verify(_ => _.RefreshSignInAsync(It.IsAny<ClaimsPrincipal>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
            _immediateActionsStoreMock.Verify(_ => _.AddAsync(It.IsAny<string>(),
                    It.IsAny<TimeSpan>(),
                    It.IsAny<ImmediateActionDataModel>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        public static IEnumerable<object[]> GetDataFor_ShouldDoNothing_When_UserIsAddedForAnAction_But_TheActionIsAlreadyPreformed()
        {
            yield return new object[]
            {
                new ImmediateActionDataModel(DateTime.Now, AddPurpose.RefreshCookie),
                new ImmediateActionDataModel(DateTime.Now.AddSeconds(1), AddPurpose.UserCookieWasRefreshed)
            };

            yield return new object[]
            {
                new ImmediateActionDataModel(DateTime.Now, AddPurpose.SignOut),
                new ImmediateActionDataModel(DateTime.Now.AddSeconds(1), AddPurpose.SignOut)
            };
        }

        [Theory]
        [MemberData(nameof(GetDataFor_ShouldCall_RefreshSignInAsync_And_AddAsync_WhenUserIsAddedForRefreshingCookie))]
        public async Task ShouldCall_RefreshSignInAsync_And_AddAsync_WhenUserIsAddedForAnAction(
            ImmediateActionDataModel actionToPreformOnUser,
            ImmediateActionDataModel previousActionPreformedOnUser,
            TimeSpan expirationTime)
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var userAgent = Guid.NewGuid().ToString();
            const string userIp = "192.168.1.1";
            var user = new ClaimsPrincipal(new List<ClaimsIdentity>
            {
                new(new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, userId)
                }, IdentityConstants.ApplicationScheme)
            });

            var userActionsStoreKey = Guid.NewGuid().ToString();
            var userActionStoreUniqueKey = Guid.NewGuid().ToString();
            _currentUserWrapperServiceMock.Setup(_ => _.IsSignedIn(It.IsAny<ClaimsPrincipal>())).Returns(true);
            _currentUserWrapperServiceMock.Setup(_ => _.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);
            _userActionStoreKeyGeneratorMock.Setup(_ => _.GenerateKey(It.IsAny<string>())).Returns(userActionsStoreKey);
            _userActionStoreKeyGeneratorMock.Setup(_ => _.GenerateKey(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(userActionStoreUniqueKey);
            _currentUserWrapperServiceMock.Setup(_ => _.IsSignedIn(It.IsAny<ClaimsPrincipal>()))
                .Returns(true);
            _httpContextMock.Setup(_ => _.User)
                .Returns(user);
            _httpContextMock.Setup(_ => _.Request.Headers)
                .Returns(() => new HeaderDictionary
                {
                    {"User-Agent", userAgent}
                });
            _httpContextMock.Setup(_ => _.Connection.RemoteIpAddress)
                .Returns(() => IPAddress.Parse(userIp));
            _immediateActionsStoreMock.SetupSequence(_ => _.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(actionToPreformOnUser)
                .ReturnsAsync(previousActionPreformedOnUser);

            // Act
            await _sut.InvokeAsync(_httpContextMock.Object,
                _immediateActionsStoreMock.Object,
                _userActionStoreKeyGeneratorMock.Object,
                _currentUserWrapperServiceMock.Object);

            // Assert
            _currentUserWrapperServiceMock.Verify(_ => _.IsSignedIn(It.Is<ClaimsPrincipal>(c =>
                    c.HasClaim(ClaimTypes.NameIdentifier, userId))),
                Times.Once);
            _currentUserWrapperServiceMock.Verify(_ => _.GetUserId(It.Is<ClaimsPrincipal>(c =>
                    c.HasClaim(ClaimTypes.NameIdentifier, userId))),
                Times.Once);
            _userActionStoreKeyGeneratorMock.Verify(_ => _.GenerateKey(It.Is<string>(s => s == userId)),
                Times.Once);
            _userActionStoreKeyGeneratorMock.Verify(_ => _.GenerateKey(
                    It.Is<string>(s => s == userId),
                    It.Is<string>(s => s == userAgent),
                    It.Is<string>(s => s == userIp)),
                Times.Once);
            _immediateActionsStoreMock.Verify(_ => _.GetAsync(It.Is<string>(s => s == userActionsStoreKey), It.IsAny<CancellationToken>()),
                Times.Once);
            _immediateActionsStoreMock.Verify(_ => _.GetAsync(It.Is<string>(s => s == userActionStoreUniqueKey), It.IsAny<CancellationToken>()),
                Times.Once);
            if (actionToPreformOnUser.Purpose == AddPurpose.RefreshCookie)
            {
                _currentUserWrapperServiceMock.Verify(_ => _.RefreshSignInAsync(It.Is<ClaimsPrincipal>(c =>
                        c.HasClaim(ClaimTypes.NameIdentifier, userId)), It.IsAny<CancellationToken>()),
                    Times.Once);

                _immediateActionsStoreMock.Verify(_ => _.AddAsync(It.Is<string>(s => s == userActionStoreUniqueKey),
                        It.Is<TimeSpan>(t => t == expirationTime),
                        It.Is<ImmediateActionDataModel>(model => model.Purpose == AddPurpose.UserCookieWasRefreshed),
                        It.IsAny<CancellationToken>()),
                    Times.Once);
            }
            else if (actionToPreformOnUser.Purpose == AddPurpose.SignOut)
            {
                _currentUserWrapperServiceMock.Verify(_ => _.SignOutAsync(It.IsAny<CancellationToken>()),
                    Times.Once);

                _immediateActionsStoreMock.Verify(_ => _.AddAsync(It.Is<string>(s => s == userActionStoreUniqueKey),
                        It.Is<TimeSpan>(t => t == expirationTime),
                        It.Is<ImmediateActionDataModel>(model =>
                            model.Purpose == AddPurpose.UserWasSignedOut),
                        It.IsAny<CancellationToken>()),
                    Times.Once);
            }
        }

        public static IEnumerable<object[]> GetDataFor_ShouldCall_RefreshSignInAsync_And_AddAsync_WhenUserIsAddedForRefreshingCookie()
        {
            yield return new object[]
            {
                new ImmediateActionDataModel(DateTime.Now, AddPurpose.RefreshCookie),
                null,
                ExpirationTimeForRefreshCookie
            };
            yield return new object[]
            {
                new ImmediateActionDataModel(DateTime.Now, AddPurpose.RefreshCookie),
                new ImmediateActionDataModel(DateTime.Now.AddSeconds(-1), AddPurpose.UserWasSignedOut),
                ExpirationTimeForRefreshCookie
            };
            yield return new object[]
            {
                new ImmediateActionDataModel(DateTime.Now, AddPurpose.SignOut),
                null,
                ExpirationTimeForSignOut
            };
            yield return new object[]
            {
                new ImmediateActionDataModel(DateTime.Now, AddPurpose.SignOut),
                new ImmediateActionDataModel(DateTime.Now.AddSeconds(-1), AddPurpose.UserCookieWasRefreshed),
                ExpirationTimeForSignOut
            };
        }
    }
}