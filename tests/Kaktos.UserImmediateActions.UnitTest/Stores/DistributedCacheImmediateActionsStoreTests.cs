using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Kaktos.UserImmediateActions.Models;
using Kaktos.UserImmediateActions.Stores;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using Xunit;

namespace Kaktos.UserImmediateActions.UnitTest.Stores
{
    public class DistributedCacheImmediateActionsStoreTests
    {
        private readonly DateTimeOffset _dateTimeOffsetUtcNow = DateTimeOffset.UtcNow;
        private readonly Mock<IDateTimeProvider> _dateTimeProviderMock = new();
        private readonly TimeSpan _defaultExpireTimeSpan = TimeSpan.FromDays(14);
        private readonly Mock<IDistributedCache> _distributedCacheMock = new();
        private readonly Mock<IPermanentImmediateActionsStore> _permanentImmediateActionsStoreMock = new();
        private readonly DistributedCacheImmediateActionsStore _sut;

        public DistributedCacheImmediateActionsStoreTests()
        {
            _dateTimeProviderMock.Setup(_ => _.UtcNow()).Returns(_dateTimeOffsetUtcNow);
            _sut = new DistributedCacheImmediateActionsStore(_distributedCacheMock.Object,
                _permanentImmediateActionsStoreMock.Object,
                _dateTimeProviderMock.Object);
        }

        [Fact]
        public void Add_ShouldThrowArgumentException_WhenArgumentIsNull_OrEmptyString()
        {
            var data = new ImmediateActionDataModel(DateTimeOffset.UtcNow, AddPurpose.RefreshCookie);
            Assert.Throws<ArgumentNullException>("data", () => _sut.Add("key", _defaultExpireTimeSpan, null));
            Assert.Throws<ArgumentException>("key", () => _sut.Add(null, _defaultExpireTimeSpan, data));
            Assert.Throws<ArgumentException>("key", () => _sut.Add("", _defaultExpireTimeSpan, data));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Add_ShouldCall_Set(bool shouldCallPermanentStoreAsWell)
        {
            // Arrange
            var key = Guid.NewGuid().ToString();
            var data = new ImmediateActionDataModel(_dateTimeOffsetUtcNow, AddPurpose.RefreshCookie);

            var jsonData = JsonSerializer.Serialize(data);
            _distributedCacheMock.Setup(_ =>
                _.Set(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>()));

            // Act
            _sut.Add(key, _defaultExpireTimeSpan, data, shouldCallPermanentStoreAsWell);

            // Assert
            _distributedCacheMock.Verify(cache => cache
                    .Set(It.Is<string>(s => s == key),
                        It.Is<byte[]>(s => s.SequenceEqual(Encoding.UTF8.GetBytes(jsonData))),
                        It.IsAny<DistributedCacheEntryOptions>()),
                Times.Once);
            _permanentImmediateActionsStoreMock.Verify(store => store.Add(
                    It.Is<string>(s => s == key),
                    It.Is<DateTimeOffset>(d => d == _dateTimeOffsetUtcNow.Add(_defaultExpireTimeSpan)),
                    It.Is<ImmediateActionDataModel>(model =>
                        model.Purpose == AddPurpose.RefreshCookie &&
                        model.AddedDate == _dateTimeOffsetUtcNow)),
                shouldCallPermanentStoreAsWell ? Times.Once : Times.Never);
        }

        [Fact]
        public async Task AddAsync_ShouldThrowArgumentException_WhenArgumentIsNull_OrEmptyString()
        {
            var data = new ImmediateActionDataModel(DateTimeOffset.UtcNow, AddPurpose.RefreshCookie);
            await Assert.ThrowsAsync<ArgumentNullException>("data", () => _sut.AddAsync("key", _defaultExpireTimeSpan, null));
            await Assert.ThrowsAsync<ArgumentException>("key", () => _sut.AddAsync(null, _defaultExpireTimeSpan, data));
            await Assert.ThrowsAsync<ArgumentException>("key", () => _sut.AddAsync("", _defaultExpireTimeSpan, data));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task AddAsync_ShouldCall_SetAsync(bool shouldCallPermanentStoreAsWell)
        {
            // Arrange
            var key = Guid.NewGuid().ToString();
            var data = new ImmediateActionDataModel(_dateTimeOffsetUtcNow, AddPurpose.RefreshCookie);

            var jsonData = JsonSerializer.Serialize(data);
            _distributedCacheMock.Setup(_ =>
                _.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()));

            // Act
            await _sut.AddAsync(key, _defaultExpireTimeSpan, data, shouldCallPermanentStoreAsWell);

            // Assert
            _distributedCacheMock.Verify(cache => cache
                    .SetAsync(It.Is<string>(s => s == key),
                        It.Is<byte[]>(s => s.SequenceEqual(Encoding.UTF8.GetBytes(jsonData))),
                        It.IsAny<DistributedCacheEntryOptions>(),
                        It.IsAny<CancellationToken>()),
                Times.Once);
            _permanentImmediateActionsStoreMock.Verify(store => store.AddAsync(
                    It.Is<string>(s => s == key),
                    It.Is<DateTimeOffset>(d => d == _dateTimeOffsetUtcNow.Add(_defaultExpireTimeSpan)),
                    It.Is<ImmediateActionDataModel>(model =>
                        model.Purpose == AddPurpose.RefreshCookie &&
                        model.AddedDate == _dateTimeOffsetUtcNow),
                    It.IsAny<CancellationToken>()),
                shouldCallPermanentStoreAsWell ? Times.Once : Times.Never);
        }

        [Fact]
        public void Get_ShouldThrowArgumentException_WhenArgumentIsNullOrEmptyString()
        {
            Assert.Throws<ArgumentException>("key", () => _sut.Get(null));
            Assert.Throws<ArgumentException>("key", () => _sut.Get(""));
        }

        [Fact]
        public void Get_ShouldCall_Get()
        {
            // Arrange
            var key = Guid.NewGuid().ToString();
            var expected = new ImmediateActionDataModel(DateTimeOffset.UtcNow, AddPurpose.RefreshCookie);
            _distributedCacheMock.SetupSequence(_ => _.Get(It.IsAny<string>()))
                .Returns(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(expected)))
                .Returns((byte[])null);

            // Act
            var actualValue = _sut.Get(key);
            var actualValue2 = _sut.Get(key);

            // Assert
            _distributedCacheMock.Verify(cache => cache.Get(It.Is<string>(s => s == key)),
                Times.Exactly(2));

            actualValue.Should().NotBeNull();
            Assert.Equal(expected.AddedDate, actualValue.AddedDate);
            Assert.Equal(expected.Purpose, actualValue.Purpose);
            actualValue2.Should().BeNull();
        }

        [Fact]
        public async Task GetAsync_ShouldThrowArgumentException_WhenArgumentIsNullOrEmptyString()
        {
            await Assert.ThrowsAsync<ArgumentException>("key", () => _sut.GetAsync(null));
            await Assert.ThrowsAsync<ArgumentException>("key", () => _sut.GetAsync(""));
        }

        [Fact]
        public async Task GetAsync_ShouldCall_GetAsync()
        {
            // Arrange
            var key = Guid.NewGuid().ToString();
            var expected = new ImmediateActionDataModel(DateTimeOffset.UtcNow, AddPurpose.RefreshCookie);
            _distributedCacheMock.SetupSequence(_ => _.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(expected)))
                .ReturnsAsync((byte[])null);

            // Act
            var actualValue = await _sut.GetAsync(key);
            var actualValue2 = await _sut.GetAsync(key);

            // Assert
            _distributedCacheMock.Verify(cache => cache.GetAsync(It.Is<string>(s => s == key), It.IsAny<CancellationToken>()),
                Times.Exactly(2));

            actualValue.Should().NotBeNull();
            Assert.Equal(expected.AddedDate, actualValue.AddedDate);
            Assert.Equal(expected.Purpose, actualValue.Purpose);
            actualValue2.Should().BeNull();
        }

        [Fact]
        public void Exists_ShouldThrowArgumentException_WhenArgumentIsNullOrEmptyString()
        {
            Assert.Throws<ArgumentException>("key", () => _sut.Exists(null));
            Assert.Throws<ArgumentException>("key", () => _sut.Exists(""));
        }

        [Fact]
        public void Exists_ShouldCall_GetString()
        {
            // Arrange
            var key = Guid.NewGuid().ToString();
            _distributedCacheMock.SetupSequence(_ => _.Get(It.IsAny<string>()))
                .Returns(Encoding.UTF8.GetBytes(""))
                .Returns((byte[])null);

            // Act
            var actualValue = _sut.Exists(key);
            var actualValue2 = _sut.Exists(key);

            // Assert
            _distributedCacheMock.Verify(cache => cache.Get(It.Is<string>(s => s == key)),
                Times.Exactly(2));

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
        public async Task ExistsAsync_ShouldCall_GetStringAsync()
        {
            // Arrange
            var key = Guid.NewGuid().ToString();
            _distributedCacheMock.SetupSequence(_ => _.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Encoding.UTF8.GetBytes(""))
                .ReturnsAsync((byte[])null);

            // Act
            var actualValue = await _sut.ExistsAsync(key);
            var actualValue2 = await _sut.ExistsAsync(key);

            // Assert
            _distributedCacheMock.Verify(cache => cache.GetAsync(It.Is<string>(s => s == key), It.IsAny<CancellationToken>()),
                Times.Exactly(2));

            actualValue.Should().BeTrue();
            actualValue2.Should().BeFalse();
        }
    }
}