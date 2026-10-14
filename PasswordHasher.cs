using System;
using System.Security.Cryptography;

namespace SikaiAdmin
{
    public static class PasswordHasher
    {
        private const int Iterations = 100000;

        public const string PasswordAlphabet = "abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        public const string CodeAlphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

        
        public static string CreateEntry(string secret)
        {
            var salt = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
                rng.GetBytes(salt);

            return Convert.ToBase64String(salt) + ":" + Hash(secret, salt);
        }

        public static bool Verify(string secret, string storedEntry)
        {
            if (string.IsNullOrEmpty(secret) || string.IsNullOrEmpty(storedEntry))
                return false;

            string[] parts = storedEntry.Split(':');
            if (parts.Length != 2)
                return false;

            try
            {
                byte[] salt = Convert.FromBase64String(parts[0]);
                return SlowEquals(Hash(secret, salt), parts[1]);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        
        public static string NormalizeAnswer(string answer)
        {
            return (answer ?? "").Trim().ToLowerInvariant();
        }

        public static string RandomText(int length, string alphabet)
        {
            var chars = new char[length];
            int limit = 256 - (256 % alphabet.Length);
            var one = new byte[1];

            using (var rng = new RNGCryptoServiceProvider())
            {
                int i = 0;
                while (i < length)
                {
                    rng.GetBytes(one);
                    if (one[0] >= limit)
                        continue;
                    chars[i++] = alphabet[one[0] % alphabet.Length];
                }
            }
            return new string(chars);
        }

        private static string Hash(string secret, byte[] salt)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(secret, salt, Iterations, HashAlgorithmName.SHA256))
                return Convert.ToBase64String(pbkdf2.GetBytes(32));
        }

        private static bool SlowEquals(string a, string b)
        {
            if (a.Length != b.Length)
                return false;

            int diff = 0;
            for (int i = 0; i < a.Length; i++)
                diff |= a[i] ^ b[i];
            return diff == 0;
        }
    }
}
