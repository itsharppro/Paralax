using System;
using System.Text;
using Xunit;
using Paralax.Security.Core;

namespace Paralax.Tests.Security
{
    public class EncryptorTests
    {
        private readonly Encryptor _encryptor;
        private readonly string _encryptionKey;

        public EncryptorTests()
        {
            // Ensure the encryption key is exactly 32 bytes (AES-256)
            _encryptionKey = "a_secure_encryption_key_32chars!!";  // This string is 32 characters long.
            _encryptor = new Encryptor();
        }

        private byte[] Get32ByteKey()
        {
            var keyBytes = Encoding.UTF8.GetBytes(_encryptionKey);
            return keyBytes.Length == 32 ? keyBytes : new byte[32];
        }

        [Fact]
        public void Encrypt_WithValidData_ShouldReturnEncryptedData()
        {
            // Arrange
            var data = "test";

            // Act
            var result = _encryptor.Encrypt(data, _encryptionKey);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public void Decrypt_WithValidData_ShouldReturnOriginalData()
        {
            // Arrange
            var data = "test";
            var encryptedData = _encryptor.Encrypt(data, _encryptionKey);

            // Act
            var result = _encryptor.Decrypt(encryptedData, _encryptionKey);

            // Assert
            Assert.Equal(data, result);
        }

        [Fact]
        public void Encrypt_ByteArray_ShouldReturnEncryptedByteArray()
        {
            // Arrange
            var data = Encoding.UTF8.GetBytes("test");
            var iv = new byte[16]; // IV must be 16 bytes for AES
            var key = Get32ByteKey(); // Ensure 32-byte key
            new Random().NextBytes(iv);

            // Act
            var result = _encryptor.Encrypt(data, iv, key);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public void Decrypt_ByteArray_ShouldReturnOriginalByteArray()
        {
            // Arrange
            var data = Encoding.UTF8.GetBytes("test");
            var iv = new byte[16]; // IV must be 16 bytes for AES
            var key = Get32ByteKey(); // Ensure 32-byte key
            new Random().NextBytes(iv);
            var encryptedData = _encryptor.Encrypt(data, iv, key);

            // Act
            var result = _encryptor.Decrypt(encryptedData, iv, key);

            // Assert
            Assert.Equal(data, result);
        }
    }
}
