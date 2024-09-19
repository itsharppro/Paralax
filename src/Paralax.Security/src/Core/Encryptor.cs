using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Paralax.Security.Core
{
    internal sealed class Encryptor : IEncryptor
    {
        private const int KeySize = 32; // AES-256 requires a 32-byte key
        private const int IvSize = 16;  // AES block size for the IV is 16 bytes (128 bits)

        // Helper method to ensure the key is 32 bytes
        private byte[] GetKeyBytes(string key)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            if (keyBytes.Length == KeySize)
                return keyBytes;
            else if (keyBytes.Length > KeySize)
                return keyBytes.Take(KeySize).ToArray(); // Truncate to 32 bytes
            else
                return keyBytes.Concat(new byte[KeySize - keyBytes.Length]).ToArray(); // Pad to 32 bytes
        }

        public string Encrypt(string data, string key)
        {
            if (string.IsNullOrWhiteSpace(data))
                throw new ArgumentException("Data to be encrypted cannot be empty.", nameof(data));

            var keyBytes = GetKeyBytes(key);

            using var aes = Aes.Create();
            aes.Key = keyBytes;
            aes.GenerateIV();
            var iv = Convert.ToBase64String(aes.IV);
            var transform = aes.CreateEncryptor(aes.Key, aes.IV);

            using var memoryStream = new MemoryStream();
            using var cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write);
            using (var streamWriter = new StreamWriter(cryptoStream))
            {
                streamWriter.Write(data);
            }

            return iv + Convert.ToBase64String(memoryStream.ToArray());
        }

        public string Decrypt(string data, string key)
        {
            if (string.IsNullOrWhiteSpace(data))
                throw new ArgumentException("Data to be decrypted cannot be empty.", nameof(data));

            var keyBytes = GetKeyBytes(key);

            using var aes = Aes.Create();
            aes.Key = keyBytes;
            aes.IV = Convert.FromBase64String(data.Substring(0, 24)); // Extract the IV from the encrypted data
            var transform = aes.CreateDecryptor(aes.Key, aes.IV);

            using var memoryStream = new MemoryStream(Convert.FromBase64String(data.Substring(24)));
            using var cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Read);
            using var streamReader = new StreamReader(cryptoStream);

            return streamReader.ReadToEnd();
        }

        public byte[] Encrypt(byte[] data, byte[] iv, byte[] key)
        {
            if (data == null || data.Length == 0)
                throw new ArgumentException("Data to be encrypted cannot be empty.", nameof(data));

            if (iv == null || iv.Length != IvSize)
                throw new ArgumentException("Initialization vector must be 16 bytes.", nameof(iv));

            if (key == null || key.Length != KeySize)
                throw new ArgumentException($"Encryption key must be {KeySize} bytes.", nameof(key));

            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;
            var transform = aes.CreateEncryptor(aes.Key, aes.IV);

            using var memoryStream = new MemoryStream();
            using var cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write);
            cryptoStream.Write(data, 0, data.Length);
            cryptoStream.FlushFinalBlock();

            return memoryStream.ToArray();
        }

        public byte[] Decrypt(byte[] data, byte[] iv, byte[] key)
        {
            if (data == null || data.Length == 0)
                throw new ArgumentException("Data to be decrypted cannot be empty.", nameof(data));

            if (iv == null || iv.Length != IvSize)
                throw new ArgumentException("Initialization vector must be 16 bytes.", nameof(iv));

            if (key == null || key.Length != KeySize)
                throw new ArgumentException($"Encryption key must be {KeySize} bytes.", nameof(key));

            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;
            var transform = aes.CreateDecryptor(aes.Key, aes.IV);

            using var memoryStream = new MemoryStream(data);
            using var cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Read);
            using var decryptedStream = new MemoryStream();
            cryptoStream.CopyTo(decryptedStream);

            return decryptedStream.ToArray();
        }
    }
}
