using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace PassManaAlpha.Core.Scurity
{
    public static class HakoHelper
    {
        private static byte[] DeriveKey(string password, byte[] salt)
        {
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256);
            return pbkdf2.GetBytes(32);
        }

        // Derives a separate key just for HMAC so it's not the same as the AES key
        private static byte[] DeriveHmacKey(string password, byte[] salt)
        {
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256);
            pbkdf2.GetBytes(32); // skip the AES key space
            return pbkdf2.GetBytes(32);
        }

        public static string Encrypt(string plainText, string password)
        {
            if (string.IsNullOrWhiteSpace(plainText))
                throw new ArgumentException("Cannot encrypt empty or null plaintext");

            using var aes = Aes.Create();
            aes.GenerateIV();
            byte[] salt = RandomNumberGenerator.GetBytes(16);
            aes.Key = DeriveKey(password, salt);

            using var ms = new MemoryStream();
            ms.Write(salt, 0, salt.Length);
            ms.Write(aes.IV, 0, aes.IV.Length);

            using (var encryptor = aes.CreateEncryptor())
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cs))
            {
                sw.Write(plainText);
                sw.Flush();
                cs.FlushFinalBlock();
            }

            byte[] cipherBytes = ms.ToArray();

            // Compute HMAC over the full ciphertext (salt + IV + encrypted data)
            byte[] hmacKey = DeriveHmacKey(password, salt);
            byte[] hmac;
            using (var hmacSha = new HMACSHA256(hmacKey))
                hmac = hmacSha.ComputeHash(cipherBytes);

            // Final format: [32-byte HMAC][cipherBytes]
            byte[] final = new byte[32 + cipherBytes.Length];
            Buffer.BlockCopy(hmac, 0, final, 0, 32);
            Buffer.BlockCopy(cipherBytes, 0, final, 32, cipherBytes.Length);

            return Convert.ToBase64String(final);
        }

        // Returns null if the HMAC doesn't match (wrong key) — not an error, just not yours
        public static string? Decrypt(string cipherText, string password)
        {
            try
            {
                byte[] fullData = Convert.FromBase64String(cipherText);

                if (fullData.Length < 32 + 32) // 32 HMAC + 16 salt + 16 IV minimum
                    return null;

                byte[] storedHmac = fullData[..32];
                byte[] cipherBytes = fullData[32..];

                // Re-derive HMAC key using the salt embedded in cipherBytes
                byte[] salt = cipherBytes[..16];
                byte[] hmacKey = DeriveHmacKey(password, salt);

                byte[] computedHmac;
                using (var hmacSha = new HMACSHA256(hmacKey))
                    computedHmac = hmacSha.ComputeHash(cipherBytes);

                // Constant-time compare to prevent timing attacks
                if (!CryptographicOperations.FixedTimeEquals(storedHmac, computedHmac))
                    return null; // Wrong key — belongs to someone else, silently skip

                byte[] iv = cipherBytes[16..32];
                byte[] encrypted = cipherBytes[32..];

                using var aes = Aes.Create();
                aes.Key = DeriveKey(password, salt);
                aes.IV = iv;

                using var ms = new MemoryStream(encrypted);
                using var decryptor = aes.CreateDecryptor();
                using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
                using var sr = new StreamReader(cs);
                return sr.ReadToEnd();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Decrypt failed: {ex.Message}");
                return null;
            }
        }
    }
}