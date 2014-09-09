using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using SecurityEx.Properties;

namespace SecurityEx
{
    public class PasswordProvider
    {

        public PasswordProvider()
        {
            InitializePasswordList();
        }

        private void InitializePasswordList()
        {
            var passwords = Settings.Default.PasswordCollection;
            ListOfPotentialPasswords = new List<string>();
            foreach (var pass in passwords)
            {
                var newPass =
                    pass.ToLower()
                        .Replace("a", "@")
                        .Replace("e", "3")
                        .Replace("i", "1")
                        .Replace("l", "!")
                        .Replace("s", "$")
                        .Replace("o", "0");
                ListOfPotentialPasswords.Add(newPass);
            }
        }

        #region Constants

        private const int SaltBytes = 128;
        private const int HashBytes = 128;
        private const int Pbkdf2Iterations = 1000;

        #endregion;

        #region Property Implementation

        protected List<string> ListOfPotentialPasswords { get; private set; }

        #endregion

        #region Password Methods

        /// <summary>
        /// Gets a Random Password
        /// </summary>
        /// <returns></returns>
        public string GetRandomPassword()
        {
            var passwordRandomNumber = GetRandomInt(1234, 7894);
            var randomPasswordIndex = GetRandomInt(0, ListOfPotentialPasswords.Count - 1);
            var appendNumberAtEnd = Convert.ToBoolean(GetRandomInt(0, 1));
            var password = appendNumberAtEnd
                               ? ListOfPotentialPasswords[randomPasswordIndex] + passwordRandomNumber
                               : passwordRandomNumber + ListOfPotentialPasswords[randomPasswordIndex];
            return password;
        }

        /// <summary>
        /// Creates a new random password
        /// </summary>
        public PasswordData CreateRandomPassword()
        {
            return CreatePassword(GetRandomPassword());
        }

        /// <summary>
        /// Creates a salted ComputeHash hash of the password.
        /// </summary>
        /// <param name="password">The password to hash.</param>
        /// <returns>The hash of the password.</returns>
        public PasswordData CreatePassword(string password)
        {
            // Generate a random salt
            var csprng = new RNGCryptoServiceProvider();
            var salt = new byte[SaltBytes];
            csprng.GetBytes(salt);

            // Hash the password and encode the parameters
            var hash = ComputeHash(password, salt, Pbkdf2Iterations, HashBytes);
            return new PasswordData() { PasswordHash = Convert.ToBase64String(hash), Salt = Convert.ToBase64String(salt) };
        }

        /// <summary>
        /// Validates a password given a hash of the correct one.
        /// </summary>
        /// <param name="password">The password to check.</param>
        /// <param name="passwordHash">A hash of the correct password.</param>
        /// <param name="passwordSalt">A hash of the correct password salt.</param>
        /// <returns>True if the password is correct. False otherwise.</returns>
        public bool ValidatePassword(string password, string passwordHash, string passwordSalt)
        {
            var salt = Convert.FromBase64String(passwordSalt);
            var hash = Convert.FromBase64String(passwordHash);
            var testHash = ComputeHash(password, salt, Pbkdf2Iterations, hash.Length);
            return SlowEquals(hash, testHash);
        }

        /// <summary>
        /// Compares two byte arrays in length-constant time. This comparison
        /// method is used so that password hashes cannot be extracted from
        /// on-line systems using a timing attack and then attacked off-line.
        /// </summary>
        /// <param name="a">The first byte array.</param>
        /// <param name="b">The second byte array.</param>
        /// <returns>True if both byte arrays are equal. False otherwise.</returns>
        private static bool SlowEquals(byte[] a, byte[] b)
        {
            var diff = (uint)a.Length ^ (uint)b.Length;
            for (var i = 0; i < a.Length && i < b.Length; i++)
                diff |= (uint)(a[i] ^ b[i]);
            return diff == 0;
        }

        /// <summary>
        /// Computes the hash of a password.
        /// </summary>
        /// <param name="password">The password to hash.</param>
        /// <param name="salt">The salt.</param>
        /// <param name="iterations">The ComputeHash iteration count.</param>
        /// <param name="outputBytes">The length of the hash to generate, in bytes.</param>
        /// <returns>A hash of the password.</returns>
        private static byte[] ComputeHash(string password, byte[] salt, int iterations, int outputBytes)
        {
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt) { IterationCount = iterations };
            return pbkdf2.GetBytes(outputBytes);
        }
        #endregion

        #region Methodos

        private int GetRandomInt(int min, int max)
        {
            var rnd = new Random();
            return rnd.Next(min, max);
        }

        #endregion
    }

    public class PasswordData
    {
        public string PasswordHash { get; set; }
        public string Salt { get; set; }
    }
}
