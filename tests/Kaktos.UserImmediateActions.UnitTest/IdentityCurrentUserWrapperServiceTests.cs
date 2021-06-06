using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Kaktos.UserImmediateActions.UnitTest
{
    public class IdentityCurrentUserWrapperServiceTests
    {
        private readonly Mock<UserManager<IdentityUser>> _userManagerMock;
        private readonly Mock<SignInManager<IdentityUser>> _signInManagerMock;
        private readonly IdentityCurrentUserWrapperService<IdentityUser> _sut;

        public IdentityCurrentUserWrapperServiceTests()
        {
            _userManagerMock = new Mock<UserManager<IdentityUser>>(new Mock<IUserStore<IdentityUser>>().Object,
                new Mock<IOptions<IdentityOptions>>().Object,
                new Mock<IPasswordHasher<IdentityUser>>().Object,
                Array.Empty<IUserValidator<IdentityUser>>(),
                Array.Empty<IPasswordValidator<IdentityUser>>(),
                new Mock<ILookupNormalizer>().Object,
                new Mock<IdentityErrorDescriber>().Object,
                new Mock<IServiceProvider>().Object,
                new Mock<ILogger<UserManager<IdentityUser>>>().Object);
            
            _signInManagerMock = new Mock<SignInManager<IdentityUser>>(_userManagerMock.Object,
                new Mock<IHttpContextAccessor>().Object,
                new Mock<IUserClaimsPrincipalFactory<IdentityUser>>().Object,
                new Mock<IOptions<IdentityOptions>>().Object,
                new Mock<ILogger<SignInManager<IdentityUser>>>().Object,
                new Mock<IAuthenticationSchemeProvider>().Object);
            
            _sut = new IdentityCurrentUserWrapperService<IdentityUser>(_signInManagerMock.Object, _userManagerMock.Object, new OptionsWrapper<IdentityOptions>(new IdentityOptions()));
        }

        [Fact]
        public void GetUserId_ShouldThrowArgumentNullException_WhenArgumentIsNull()
        {
            Assert.Throws<ArgumentNullException>("userPrincipal", () => _sut.GetUserId(null));
        }

        [Fact]
        public void GetUserId_ShouldReturnUserId()
        {
            // Arrange
            var expected = Guid.NewGuid().ToString();
            var claimsPrincipal = new ClaimsPrincipal(new List<ClaimsIdentity>
            {
                new(new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, expected)
                }, IdentityConstants.ApplicationScheme)
            });

            // Act
            var actualValue = _sut.GetUserId(claimsPrincipal);

            // Assert
            actualValue.Should().Be(expected);
        }

        [Fact]
        public void IsSignedIn_ShouldThrowArgumentNullException_WhenArgumentIsNull()
        {
            Assert.Throws<ArgumentNullException>("userPrincipal", () => _sut.IsSignedIn(null));
        }

        [Fact]
        public void IsSignedIn_ShouldCall_IsSignedIn()
        {
            // Arrange
            _signInManagerMock.SetupSequence(manager => manager.IsSignedIn(It.IsAny<ClaimsPrincipal>()))
                .Returns(true)
                .Returns(false);
            
            var claimsPrincipal = new ClaimsPrincipal(Enumerable.Empty<ClaimsIdentity>());

            // Act
            var actualValue = _sut.IsSignedIn(claimsPrincipal);
            var actualValue2 = _sut.IsSignedIn(claimsPrincipal);

            // Assert
            actualValue.Should().Be(true);
            actualValue2.Should().Be(false);
        }

        [Fact]
        public async Task RefreshSignInAsync_ShouldThrowArgumentNullException_WhenArgumentIsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>("userPrincipal", () => _sut.RefreshSignInAsync(null));
        }
        
        [Fact]
        public async Task RefreshSignInAsync_ShouldCall_RefreshSignInAsync()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var claimsPrincipal = new ClaimsPrincipal(new List<ClaimsIdentity>
            {
                new(new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, userId)
                }, IdentityConstants.ApplicationScheme)
            });

            _userManagerMock.Setup(_ => _.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(() => new IdentityUser());
            _signInManagerMock.Setup(_ => _.RefreshSignInAsync(It.IsAny<IdentityUser>()));

            // Act
            await _sut.RefreshSignInAsync(claimsPrincipal);

            // Assert
            _userManagerMock.Verify(_=>_.FindByIdAsync(It.Is<string>(s=>s == userId)), Times.Once);
            _signInManagerMock.Verify(_=>_.RefreshSignInAsync(It.IsAny<IdentityUser>()), Times.Once);
        }
        
        [Fact]
        public async Task SignOutAsync_ShouldCall_SignOutAsync()
        {
            // Arrange
            _signInManagerMock.Setup(_ => _.SignOutAsync());

            // Act
            await _sut.SignOutAsync();

            // Assert
            _signInManagerMock.Verify(_=>_.SignOutAsync(), Times.Once);
        }
    }
}