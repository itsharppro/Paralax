using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Isopoh.Cryptography.Argon2; 
using BCrypt.Net; 
using SHA3.Net; 

namespace Paralax.Security
{
    internal sealed class Hasher : IHasher
    {
        public string Hash(string data)
        {
            var hash = Hash(Encoding.UTF8.GetBytes(data));
            var builder = new StringBuilder();
            foreach (var @byte in hash)
            {
                builder.Append(@byte.ToString("x2"));
            }

            return builder.ToString();
        }

        public byte[] Hash(byte[] data)
        {
            if (data is null || !data.Any())
            {
                throw new ArgumentException("Data to be hashed cannot be empty.", nameof(data));
            }

            using var sha256Hash = SHA256.Create();
            return sha256Hash.ComputeHash(data);
        }

        public string HashWithBcrypt(string data, int workFactor = 12)
        {
            if (string.IsNullOrEmpty(data))
            {
                throw new ArgumentException("Data to be hashed cannot be empty.", nameof(data));
            }

            return BCrypt.Net.BCrypt.HashPassword(data, workFactor);
        }

        public string HashWithPBKDF2(string data, byte[] salt, int iterations = 10000, int keyLength = 32)
        {
            if (string.IsNullOrEmpty(data))
            {
                throw new ArgumentException("Data to be hashed cannot be empty.", nameof(data));
            }

            using var pbkdf2 = new Rfc2898DeriveBytes(data, salt, iterations, HashAlgorithmName.SHA256);
            var hash = pbkdf2.GetBytes(keyLength);
            return Convert.ToBase64String(hash);
        }

        public string HashWithArgon2(string data, byte[] salt, int memorySize = 65536, int iterations = 3, int parallelism = 1)
        {
            if (string.IsNullOrEmpty(data))
            {
                throw new ArgumentException("Data to be hashed cannot be empty.", nameof(data));
            }

            var argon2Config = new Argon2Config
            {
                Type = Argon2Type.DataIndependentAddressing,
                Salt = salt,
                MemoryCost = memorySize,
                TimeCost = iterations,
                Threads = parallelism,
                Password = Encoding.UTF8.GetBytes(data)
            };

            var argon2 = new Argon2(argon2Config);
            var hash = argon2.Hash();
            return Convert.ToBase64String(hash.Buffer); 
        }

        public string HashWithSHA3(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                throw new ArgumentException("Data to be hashed cannot be empty.", nameof(data));
            }

            var sha3 = Sha3.Sha3256();
            var hash = sha3.ComputeHash(Encoding.UTF8.GetBytes(data));
            return Convert.ToBase64String(hash);
        }
    }
}
