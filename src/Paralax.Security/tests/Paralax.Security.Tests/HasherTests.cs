using System;
using System.Text;
using Xunit;
using Paralax.Security;

namespace Paralax.Tests.Security
{
    public class HasherTests
    {
        private readonly Hasher _hasher;

        public HasherTests()
        {
            _hasher = new Hasher();
        }

        [Fact]
        public void Hash_WithValidString_ShouldReturnHashedString()
        {
            // Arrange
            var data = "test";

            // Act
            var result = _hasher.Hash(data);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public void Hash_WithValidByteArray_ShouldReturnHashedByteArray()
        {
            // Arrange
            var data = Encoding.UTF8.GetBytes("test");

            // Act
            var result = _hasher.Hash(data);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public void HashWithBcrypt_ShouldReturnBcryptHash()
        {
            // Arrange
            var data = "test";

            // Act
            var result = _hasher.HashWithBcrypt(data);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public void HashWithPBKDF2_ShouldReturnPBKDF2Hash()
        {
            // Arrange
            var data = "test";
            var salt = new byte[16];
            new Random().NextBytes(salt);

            // Act
            var result = _hasher.HashWithPBKDF2(data, salt);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public void HashWithArgon2_ShouldReturnArgon2Hash()
        {
            // Arrange
            var data = "test";
            var salt = new byte[16];
            new Random().NextBytes(salt);

            // Act
            var result = _hasher.HashWithArgon2(data, salt);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public void HashWithSHA3_ShouldReturnSHA3Hash()
        {
            // Arrange
            var data = "test";

            // Act
            var result = _hasher.HashWithSHA3(data);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }
    }
}
