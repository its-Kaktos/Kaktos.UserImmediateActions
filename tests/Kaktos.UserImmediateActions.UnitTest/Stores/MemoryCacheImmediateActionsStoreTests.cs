using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Kaktos.UserImmediateActions.Models;
using Kaktos.UserImmediateActions.Stores;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Xunit;

namespace Kaktos.UserImmediateActions.UnitTest.Stores
{
    public class MemoryCacheImmediateActionsStoreTests
    {
        private readonly DateTime _dateTimeNow = DateTime.Now;
        private readonly Mock<IDateTimeProvider> _dateTimeProviderMock = new();
        private readonly TimeSpan _defaultExpireTimeSpan = TimeSpan.FromDays(14);
        private readonly Mock<IMemoryCache> _memoryCacheMock = new();
        private readonly Mock<IPermanentImmediateActionsStore> _permanentImmediateActionsStoreMock = new();
        private readonly MemoryCacheImmediateActionsStore _sut;

        public MemoryCacheImmediateActionsStoreTests()
        {
            _dateTimeProviderMock.Setup(_ => _.Now()).Returns(_dateTimeNow);
            _sut = new MemoryCacheImmediateActionsStore(_memoryCacheMock.Object,
                _permanentImmediateActionsStoreMock.Object,
                _dateTimeProviderMock.Object);
        }

        [Fact]
        public void Add_ShouldThrowArgumentException_WhenArgumentIsNullOrEmptyString()
        {
            var data = new ImmediateActionDataModel(DateTime.Now, AddPurpose.RefreshCookie);
            Assert.Throws<ArgumentException>("key", () => _sut.Add(null, _defaultExpireTimeSpan, data));
            Assert.Throws<ArgumentException>("key", () => _sut.Add("", _defaultExpireTimeSpan, data));
        }

        [Fact]
        public void Add_ShouldCall_CreateEntry()
        {
            // Arrange
            var key = Guid.NewGuid().ToString();
            var cacheEntry = new Mock<ICacheEntry>();
            cacheEntry.Setup(_ => _.Key).Returns(key);
            cacheEntry.Setup(_ => _.AbsoluteExpirationRelativeToNow).Returns(TimeSpan.FromMinutes(30));
            cacheEntry.Setup(_ => _.Value).Returns("");

            _memoryCacheMock.Setup(_ => _.CreateEntry(It.IsAny<object>()))
                .Returns<string>(_ => cacheEntry.Object);

            // Act
            _sut.Add(key, _defaultExpireTimeSpan, new ImmediateActionDataModel(_dateTimeNow, AddPurpose.RefreshCookie));

            // Assert
            _memoryCacheMock.Verify(cache => cache.CreateEntry(It.Is<string>(s => s == key)), Times.Once);
            _permanentImmediateActionsStoreMock.Verify(store => store.Add(
                    It.Is<string>(s => s == key),
                    It.Is<DateTime>(d => d == _dateTimeNow.Add(_defaultExpireTimeSpan)),
                    It.Is<ImmediateActionDataModel>(model =>
                        model.Purpose == AddPurpose.RefreshCookie &&
                        model.AddedDate == _dateTimeNow)),
                Times.Once);
        }

        [Fact]
        public async Task AddAsync_ShouldThrowArgumentException_WhenArgumentIsNullOrEmptyString()
        {
            var data = new ImmediateActionDataModel(DateTime.Now, AddPurpose.RefreshCookie);
            await Assert.ThrowsAsync<ArgumentException>("key", () => _sut.AddAsync(null, _defaultExpireTimeSpan, data));
            await Assert.ThrowsAsync<ArgumentException>("key", () => _sut.AddAsync("", _defaultExpireTimeSpan, data));
        }

        [Fact]
        public async Task AddAsync_ShouldCall_CreateEntry()
        {
            // Arrange
            var key = Guid.NewGuid().ToString();
            var cacheEntry = new Mock<ICacheEntry>();
            cacheEntry.Setup(_ => _.Key).Returns(key);
            cacheEntry.Setup(_ => _.AbsoluteExpirationRelativeToNow).Returns(TimeSpan.FromMinutes(30));
            cacheEntry.Setup(_ => _.Value).Returns("");

            _memoryCacheMock.Setup(_ => _.CreateEntry(It.IsAny<object>()))
                .Returns<string>(_ => cacheEntry.Object);

            // Act
            await _sut.AddAsync(key, _defaultExpireTimeSpan, new ImmediateActionDataModel(_dateTimeNow, AddPurpose.RefreshCookie));

            // Assert
            _memoryCacheMock.Verify(cache => cache.CreateEntry(It.Is<string>(s => s == key)), Times.Once);
            _permanentImmediateActionsStoreMock.Verify(store => store.AddAsync(
                    It.Is<string>(s => s == key),
                    It.Is<DateTime>(d => d == _dateTimeNow.Add(_defaultExpireTimeSpan)),
                    It.Is<ImmediateActionDataModel>(model =>
                        model.Purpose == AddPurpose.RefreshCookie &&
                        model.AddedDate == _dateTimeNow),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public void Get_ShouldThrowArgumentException_WhenArgumentIsNullOrEmptyString()
        {
            Assert.Throws<ArgumentException>("key", () => _sut.Get(null));
            Assert.Throws<ArgumentException>("key", () => _sut.Get(""));
        }

        [Fact]
        public void Get_ShouldCall_TryGetValue()
        {
            // Arrange
            var key = Guid.NewGuid().ToString();
            object expected = new ImmediateActionDataModel(DateTime.Now, AddPurpose.RefreshCookie);
            _memoryCacheMock.SetupSequence(_ => _.TryGetValue(It.IsAny<object>(), out expected))
                .Returns(true)
                .Returns(false);

            // Act
            var actualValue = _sut.Get(key);
            var actualValue2 = _sut.Get(key);

            // Assert
            _memoryCacheMock.Verify(cache => cache.TryGetValue(It.Is<string>(s => s == key),
                out expected), Times.Exactly(2));

            actualValue.Should().NotBeNull();
            Assert.Equal(((ImmediateActionDataModel) expected).AddedDate, actualValue.AddedDate);
            Assert.Equal(((ImmediateActionDataModel) expected).Purpose, actualValue.Purpose);
            actualValue2.Should().BeNull();
        }

        [Fact]
        public async Task GetAsync_ShouldThrowArgumentException_WhenArgumentIsNullOrEmptyString()
        {
            await Assert.ThrowsAsync<ArgumentException>("key", () => _sut.GetAsync(null));
            await Assert.ThrowsAsync<ArgumentException>("key", () => _sut.GetAsync(""));
        }

        [Fact]
        public async Task GetAsync_ShouldCall_TryGetValue()
        {
            // Arrange
            var key = Guid.NewGuid().ToString();
            object expected = new ImmediateActionDataModel(DateTime.Now, AddPurpose.RefreshCookie);
            _memoryCacheMock.SetupSequence(_ => _.TryGetValue(It.IsAny<object>(), out expected))
                .Returns(true)
                .Returns(false);

            // Act
            var actualValue = await _sut.GetAsync(key);
            var actualValue2 = await _sut.GetAsync(key);

            // Assert
            _memoryCacheMock.Verify(cache => cache.TryGetValue(It.Is<string>(s => s == key),
                out expected), Times.Exactly(2));

            actualValue.Should().NotBeNull();
            Assert.Equal(((ImmediateActionDataModel) expected).AddedDate, actualValue.AddedDate);
            Assert.Equal(((ImmediateActionDataModel) expected).Purpose, actualValue.Purpose);
            actualValue2.Should().BeNull();
        }

        [Fact]
        public void Exists_ShouldThrowArgumentException_WhenArgumentIsNullOrEmptyString()
        {
            Assert.Throws<ArgumentException>("key", () => _sut.Exists(null));
            Assert.Throws<ArgumentException>("key", () => _sut.Exists(""));
        }

        [Fact]
        public void Exists_ShouldCall_TryGetValue()
        {
            // Arrange
            var key = Guid.NewGuid().ToString();
            object dummyValue;
            _memoryCacheMock.SetupSequence(_ => _.TryGetValue(It.IsAny<object>(), out dummyValue))
                .Returns(true)
                .Returns(false);

            // Act
            var actualValue = _sut.Exists(key);
            var actualValue2 = _sut.Exists(key);

            // Assert
            _memoryCacheMock.Verify(cache => cache.TryGetValue(It.Is<string>(s => s == key),
                out dummyValue), Times.Exactly(2));

            actualValue.Should().BeTrue();
            actualValue2.Should().BeFalse();
        }

        [Fact]
        public async Task ExistsAsync_ShouldThrowArgumentException_WhenArgumentIsNullOrEmptyString()
        {
            await Assert.ThrowsAsync<ArgumentException>("key", () => _sut.ExistsAsync(null));
            await Assert.ThrowsAsync<ArgumentException>("key", () => _sut.ExistsAsync(""));
        }

        [Fact]
        public async Task ExistsAsync_ShouldCall_Remove()
        {
            // Arrange
            var key = Guid.NewGuid().ToString();
            object dummyValue;
            _memoryCacheMock.SetupSequence(_ => _.TryGetValue(It.IsAny<object>(), out dummyValue))
                .Returns(true)
                .Returns(false);

            // Act
            var actualValue = await _sut.ExistsAsync(key);
            var actualValue2 = await _sut.ExistsAsync(key);

            // Assert
            _memoryCacheMock.Verify(cache => cache.TryGetValue(It.Is<string>(s => s == key),
                out dummyValue), Times.Exactly(2));

            actualValue.Should().BeTrue();
            actualValue2.Should().BeFalse();
        }
    }
}