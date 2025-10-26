using System;
using System.Security.Cryptography;
using System.Text;

namespace TestSystem.Models
{
    public static class PasswordHasher
    {
        private const int Iterations = 100_000;
        private const int SaltSize = 16;
        private const int HashSize = 32;

        // Генерация соли для PBKDF2
        public static string GenerateSalt()
        {
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
            return Convert.ToBase64String(salt);
        }

        // Хеш с PBKDF2
        public static string HashPassword(string password, string saltBase64)
        {
            if (string.IsNullOrEmpty(saltBase64))
            {
                throw new ArgumentNullException(nameof(saltBase64), "Salt cannot be null or empty for PBKDF2 hashing.");
            }
            byte[] salt = Convert.FromBase64String(saltBase64);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                password: Encoding.UTF8.GetBytes(password),
                salt: salt,
                iterations: Iterations,
                hashAlgorithm: HashAlgorithmName.SHA256,
                outputLength: HashSize);
            return Convert.ToBase64String(hash);
        }

        // Проверка PBKDF2
        public static bool Verify(string password, string saltBase64, string expectedHashBase64)
        {
            if (string.IsNullOrEmpty(saltBase64) || string.IsNullOrEmpty(expectedHashBase64))
            {
                return false;
            }

            try
            {
                var expected = Convert.FromBase64String(expectedHashBase64);
                var actual = Convert.FromBase64String(HashPassword(password, saltBase64));
                return CryptographicOperations.FixedTimeEquals(actual, expected);
            }
            catch
            {
                return false;
            }
        }

        // Проверка старого SHA256 HEX без соли
        public static bool VerifyLegacySha256(string password, string expectedHexHash)
        {
            if (string.IsNullOrEmpty(expectedHexHash)) return false;

            try
            {
                using var sha256 = SHA256.Create();
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                string hashHex = BitConverter.ToString(hashBytes).Replace("-", "").ToUpperInvariant();
                return hashHex == expectedHexHash.ToUpperInvariant();
            }
            catch
            {
                return false;
            }
        }

        // НОВИЙ МЕТОД: Перевірка чистого тексту (для користувачів 1 та 2)
        public static bool VerifyPlainText(string password, string storedPassword)
        {
            return password == storedPassword;
        }
    }
}