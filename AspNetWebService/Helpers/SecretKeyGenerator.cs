using System.Security.Cryptography;

namespace AspNetWebService.Helpers
{
    /// <summary>
    /// Helper class responsible for generating random secret keys.
    /// </summary>
    public class SecretKeyGenerator
    {
        /// <summary>
        /// Generates a random secret key of the specified length.
        /// </summary>
        /// <param name="keyLength">The length of the secret key to be generated.</param>
        /// <returns>A randomly generated secret key encoded as a Base64 string.</returns>
        public static string GenerateRandomSecretKey(int keyLength)
        {
            byte[] keyBytes = new byte[keyLength];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(keyBytes);
            }

            return Convert.ToBase64String(keyBytes);
        }
    }
}
