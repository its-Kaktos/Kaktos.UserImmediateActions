using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using UserImmediateActions.Models;
using UserImmediateActions.Stores;
using Xunit;

namespace UserImmediateActions.UnitTest
{
    public class UserImmediateActionsServiceTests
    {
        private readonly Mock<IImmediateActionsStore> _immediateActionsStoreMock = new();
        private readonly Mock<IUserActionStoreKeyGenerator> _userActionStoreKeyGeneratorMock = new();
        private readonly Mock<IDateTimeProvider> _dateTimeProviderMock = new();
        private readonly DateTime _dateTimeNow = DateTime.Now;
        private readonly UserImmediateActionsService _sut;

        public UserImmediateActionsServiceTests()
        {
            _dateTimeProviderMock.Setup(_ => _.Now()).Returns(_dateTimeNow);
            
            _sut = new UserImmediateActionsService(_immediateActionsStoreMock.Object,
                _userActionStoreKeyGeneratorMock.Object,
                _dateTimeProviderMock.Object);
        }

        [Fact]
        public void RefreshCookie_ShouldThrowArgumentException_WhenArgumentIsNullOrEmptyString()
        {
            Assert.Throws<ArgumentException>("userId", () => _sut.RefreshCookie(null));
            Assert.Throws<ArgumentException>("userId", () => _sut.RefreshCookie(""));
        }

        [Fact]
        public void RefreshCookie_ShouldCall_AddAsync()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var key = Guid.NewGuid() + userId;
            _userActionStoreKeyGeneratorMock.Setup(_ => _.GenerateKey(It.IsAny<string>())).Returns(key);
            _immediateActionsStoreMock.Setup(_ => _.Add(It.IsAny<string>(), It.IsAny<ImmediateActionDataModel>()));

            // Act
            _sut.RefreshCookie(userId);

            // Assert
            _userActionStoreKeyGeneratorMock.Verify(_ => _.GenerateKey(It.Is<string>(s => s == userId)), Times.Once);
            _immediateActionsStoreMock.Verify(_ => _.Add(
                    It.Is<string>(s => s == key),
                    It.Is<ImmediateActionDataModel>(model => 
                        model.Purpose == AddPurpose.RefreshCookie &&
                        model.AddedDate == _dateTimeNow)),
                Times.Once);
        }

        [Fact]
        public async Task RefreshCookieAsync_ShouldThrowArgumentException_WhenArgumentIsNullOrEmptyString()
        {
            await Assert.ThrowsAsync<ArgumentException>("userId", () => _sut.RefreshCookieAsync(null));
            await Assert.ThrowsAsync<ArgumentException>("userId", () => _sut.RefreshCookieAsync(""));
        }

        [Fact]
        public async Task RefreshCookieAsync_ShouldCall_AddAsync()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var key = Guid.NewGuid() + userId;
            _userActionStoreKeyGeneratorMock.Setup(_ => _.GenerateKey(It.IsAny<string>())).Returns(key);
            _immediateActionsStoreMock.Setup(_ => _.AddAsync(It.IsAny<string>(), It.IsAny<ImmediateActionDataModel>(), It.IsAny<CancellationToken>()));

            // Act
            await _sut.RefreshCookieAsync(userId);

            // Assert
            _userActionStoreKeyGeneratorMock.Verify(_ => _.GenerateKey(It.Is<string>(s => s == userId)), Times.Once);
            _immediateActionsStoreMock.Verify(_ => _.AddAsync(
                    It.Is<string>(s => s == key),
                    It.Is<ImmediateActionDataModel>(model => 
                            model.Purpose == AddPurpose.RefreshCookie &&
                            model.AddedDate == _dateTimeNow),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public void SignOut_ShouldThrowArgumentException_WhenArgumentIsNullOrEmptyString()
        {
            Assert.Throws<ArgumentException>("userId", () => _sut.SignOut(null));
            Assert.Throws<ArgumentException>("userId", () => _sut.SignOut(""));
        }

        [Fact]
        public void SignOut_ShouldCall_AddAsync()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var key = Guid.NewGuid() + userId;
            _userActionStoreKeyGeneratorMock.Setup(_ => _.GenerateKey(It.IsAny<string>())).Returns(key);
            _immediateActionsStoreMock.Setup(_ => _.Add(It.IsAny<string>(), It.IsAny<ImmediateActionDataModel>()));

            // Act
            _sut.SignOut(userId);

            // Assert
            _userActionStoreKeyGeneratorMock.Verify(_ => _.GenerateKey(It.Is<string>(s => s == userId)), Times.Once);
            _immediateActionsStoreMock.Verify(_ => _.Add(
                    It.Is<string>(s => s == key),
                    It.Is<ImmediateActionDataModel>(model => 
                        model.Purpose == AddPurpose.SignOut &&
                        model.AddedDate == _dateTimeNow)),
                Times.Once);
        }

        [Fact]
        public async Task SignOutAsync_ShouldThrowArgumentException_WhenArgumentIsNullOrEmptyString()
        {
            await Assert.ThrowsAsync<ArgumentException>("userId", () => _sut.SignOutAsync(null));
            await Assert.ThrowsAsync<ArgumentException>("userId", () => _sut.SignOutAsync(""));
        }

        [Fact]
        public async Task SignOutAsync_ShouldCall_AddAsync()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var key = Guid.NewGuid() + userId;
            _userActionStoreKeyGeneratorMock.Setup(_ => _.GenerateKey(It.IsAny<string>())).Returns(key);
            _immediateActionsStoreMock.Setup(_ => _.AddAsync(It.IsAny<string>(), It.IsAny<ImmediateActionDataModel>(), It.IsAny<CancellationToken>()));

            // Act
            await _sut.SignOutAsync(userId);

            // Assert
            _userActionStoreKeyGeneratorMock.Verify(_ => _.GenerateKey(It.Is<string>(s => s == userId)), Times.Once);
            _immediateActionsStoreMock.Verify(_ => _.AddAsync(
                    It.Is<string>(s => s == key),
                    It.Is<ImmediateActionDataModel>(model => 
                        model.Purpose == AddPurpose.SignOut &&
                        model.AddedDate == _dateTimeNow),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}