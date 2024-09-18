using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Paralax.Security
{
    public interface IHasher
    {
        string Hash(string data);
        byte[] Hash(byte[] data);
        string HashWithBcrypt(string data, int workFactor = 12);
        string HashWithPBKDF2(string data, byte[] salt, int iterations = 10000, int keyLength = 32);
        string HashWithArgon2(string data, byte[] salt, int memorySize = 65536, int iterations = 3, int parallelism = 1);
        string HashWithSHA3(string data);
    }
}