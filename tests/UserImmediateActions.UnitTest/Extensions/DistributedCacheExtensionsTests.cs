using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using UserImmediateActions.Extensions;
using Xunit;

namespace UserImmediateActions.UnitTest.Extensions
{
    public class DistributedCacheExtensionsTests
    {
        private readonly Mock<IDistributedCache> _sut = new();

        [Fact]
        public async Task SetRecord_And_SetRecordAsync_ShouldAddKeyToCache()
        {
            // Arrange
            var key = Guid.NewGuid().ToString();
            var record = "";
            var jsonRecord = JsonSerializer.Serialize(record);
            var bytesRecord = Encoding.UTF8.GetBytes(jsonRecord);
            var absoluteExpirationTime = TimeSpan.FromMinutes(3);
            var unusedExpirationTime = TimeSpan.FromMinutes(3);

            // Act

            // ReSharper disable once MethodHasAsyncOverload
            _sut.Object.SetRecord(key, record, absoluteExpirationTime);
            // ReSharper disable once MethodHasAsyncOverload
            _sut.Object.SetRecord(key, record, absoluteExpirationTime, unusedExpirationTime);
            await _sut.Object.SetRecordAsync(key, record, absoluteExpirationTime);
            await _sut.Object.SetRecordAsync(key, record, absoluteExpirationTime, unusedExpirationTime);

            // Assert
            _sut.Verify(cache => cache.Set(It.Is<string>(s => s == key),
                    It.Is<byte[]>(bytes => bytes.SequenceEqual(bytesRecord)),
                    It.Is<DistributedCacheEntryOptions>(options =>
                        options.AbsoluteExpirationRelativeToNow == absoluteExpirationTime
                        && options.SlidingExpiration == null
                        && options.AbsoluteExpiration == null)),
                Times.Once);

            _sut.Verify(cache => cache.Set(It.Is<string>(s => s == key),
                    It.Is<byte[]>(bytes => bytes.SequenceEqual(bytesRecord)),
                    It.Is<DistributedCacheEntryOptions>(options =>
                        options.AbsoluteExpirationRelativeToNow == absoluteExpirationTime
                        && options.SlidingExpiration == unusedExpirationTime
                        && options.AbsoluteExpiration == null)),
                Times.Once);

            _sut.Verify(cache => cache.SetAsync(It.Is<string>(s => s == key),
                    It.Is<byte[]>(bytes => bytes.SequenceEqual(bytesRecord)),
                    It.Is<DistributedCacheEntryOptions>(options =>
                        options.AbsoluteExpirationRelativeToNow == absoluteExpirationTime
                        && options.SlidingExpiration == null
                        && options.AbsoluteExpiration == null),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _sut.Verify(cache => cache.SetAsync(It.Is<string>(s => s == key),
                    It.Is<byte[]>(bytes => bytes.SequenceEqual(bytesRecord)),
                    It.Is<DistributedCacheEntryOptions>(options =>
                        options.AbsoluteExpirationRelativeToNow == absoluteExpirationTime
                        && options.SlidingExpiration == unusedExpirationTime
                        && options.AbsoluteExpiration == null),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetRecord_And_GetRecordAsync_ShouldAddKeyToCache()
        {
            // Arrange
            var key = Guid.NewGuid().ToString();
            var expected = "";
            var expectedBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(expected));

            _sut.Setup(cache => cache.Get(key))
                .Returns(expectedBytes);
            _sut.Setup(cache => cache.GetAsync(key, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedBytes);

            // Act
            var actual = _sut.Object.GetRecord<string>(key);
            var actualAsync = await _sut.Object.GetRecordAsync<string>(key);

            // Assert
            actual.Should().Be(expected);
            actualAsync.Should().Be(expected);
        }
    }
}