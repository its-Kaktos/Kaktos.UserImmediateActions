using System;
using FluentAssertions;
using Xunit;

namespace UserImmediateActions.UnitTest
{
    public class UserActionStoreKeyGeneratorTests
    {
        private readonly UserActionStoreKeyGenerator _sut = new();

        [Fact]
        public void GenerateKey_ShouldThrowArgumentException_WhenArgumentIsNullOrEmptyString()
        {
            Assert.Throws<ArgumentException>("userId", () => _sut.GenerateKey(null));
            Assert.Throws<ArgumentException>("userId", () => _sut.GenerateKey(""));
        }

        [Theory]
        [InlineData("sdagliu234", "_IUASK_sdagliu234")]
        [InlineData("dskjh(*@#*(6", "_IUASK_dskjh(*@#*(6")]
        public void GenerateKey_ShouldReturnUserIdWithPrefix(string userId, string expected)
        {
            // Arrange
            // Act
            var actual = _sut.GenerateKey(userId);

            // Assert
            actual.Should().Be(expected);
        }

        [Fact]
        public void GenerateKey_ShouldThrowArgumentException_WhenAnyArgumentIsNullOrEmptyString()
        {
            Assert.Throws<ArgumentException>("userId", () => _sut.GenerateKey(null,"a","a"));
            Assert.Throws<ArgumentException>("userId", () => _sut.GenerateKey("","a","a"));
            Assert.Throws<ArgumentException>("userAgent", () => _sut.GenerateKey("a",null,"a"));
            Assert.Throws<ArgumentException>("userAgent", () => _sut.GenerateKey("a","","a"));
            Assert.Throws<ArgumentException>("userIp", () => _sut.GenerateKey("a","a",null));
            Assert.Throws<ArgumentException>("userIp", () => _sut.GenerateKey("a","a",""));
        }

        [Theory]
        [InlineData("1", "2", "3.4235.3.2", "_IUASK_1_2_3.4235.3.2")]
        [InlineData("dfasg", "(*@#&", "1123.234.24356", "_IUASK_dfasg_(*@#&_1123.234.24356")]
        public void GenerateKey_ShouldReturnUserIdAndUserAgentAndUserIpWithPrefix(string userId, string userAgent, string userIp, string expected)
        {
            // Arrange
            // Act
            var actual = _sut.GenerateKey(userId, userAgent, userIp);

            // Assert
            actual.Should().Be(expected);
        }
    }
}