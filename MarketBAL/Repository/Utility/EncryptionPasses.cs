using System.Security.Cryptography;
using System.Text;

namespace MarketBAL.Repository.Utility
{
    public static class EncryptionPasses
    {
        public static string EncryptNew(string plainText, string initVector, string passPhrase, int keySize)
        {
            byte[] iv = Encoding.UTF8.GetBytes(initVector);
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);

            // Derive the encryption key from the passphrase
            using var keyDerivation = new Rfc2898DeriveBytes(passPhrase, new byte[0], 1000); // Salt is optional here
            byte[] key = keyDerivation.GetBytes(keySize / 8);

            using var aes = Aes.Create();
            aes.Mode = CipherMode.CBC;
            aes.Key = key;
            aes.IV = iv;

            using var memoryStream = new MemoryStream();
            using (var cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
            {
                cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                cryptoStream.FlushFinalBlock();
            }

            byte[] encryptedBytes = memoryStream.ToArray();
            return Convert.ToBase64String(encryptedBytes);
        }

        //Deprecated
        public static string Encrypt(string plainText, string INIT_VECTOR, string PASS_PHRASE, int KEY_SIZE)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(INIT_VECTOR);
            byte[] bytes2 = Encoding.UTF8.GetBytes(plainText);
            byte[] bytes3 = new PasswordDeriveBytes(PASS_PHRASE, null).GetBytes(KEY_SIZE / 8);
            ICryptoTransform transform = new RijndaelManaged
            {
                Mode = CipherMode.CBC
            }.CreateEncryptor(bytes3, bytes);
            MemoryStream memoryStream = new MemoryStream();
            CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write);
            cryptoStream.Write(bytes2, 0, bytes2.Length);
            cryptoStream.FlushFinalBlock();
            byte[] inArray = memoryStream.ToArray();
            memoryStream.Close();
            cryptoStream.Close();
            return Convert.ToBase64String(inArray);
        }


        public static string DecryptNew(string cipherText, string initVector, string passPhrase, int keySize)
        {
            try
            {
                byte[] iv = Encoding.UTF8.GetBytes(initVector);
                byte[] cipherTextBytes = Convert.FromBase64String(cipherText);

                // Derive the decryption key from the passphrase
                using var keyDerivation = new Rfc2898DeriveBytes(passPhrase, new byte[0], 1000); // Salt is optional here
                byte[] key = keyDerivation.GetBytes(keySize / 8);

                using var aes = Aes.Create();
                aes.Mode = CipherMode.CBC;
                aes.Key = key;
                aes.IV = iv;

                using var memoryStream = new MemoryStream(cipherTextBytes);
                using var cryptoStream = new CryptoStream(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Read);
                using var reader = new StreamReader(cryptoStream, Encoding.UTF8);
                return reader.ReadToEnd();
            }
            catch
            {
                return ""; // Handle errors gracefully
            }
        }

        //Deprecated
        public static string Decrypt(string cipherText, string INIT_VECTOR, string PASS_PHRASE, int KEY_SIZE)
        {
            try
            {
                byte[] bytes = Encoding.ASCII.GetBytes(INIT_VECTOR);
                byte[] array = Convert.FromBase64String(cipherText);
                byte[] bytes2 = new PasswordDeriveBytes(PASS_PHRASE, null).GetBytes(KEY_SIZE / 8);
                ICryptoTransform transform = new RijndaelManaged
                {
                    Mode = CipherMode.CBC
                }.CreateDecryptor(bytes2, bytes);
                MemoryStream memoryStream = new MemoryStream(array);
                CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Read);
                byte[] array2 = new byte[array.Length];
                int count = cryptoStream.Read(array2, 0, array2.Length);
                memoryStream.Close();
                cryptoStream.Close();
                return Encoding.UTF8.GetString(array2, 0, count);
            }
            catch
            {
                return "";
            }
        }

    }

}
