using System;
using System.Security.Cryptography;
using System.Text;

namespace nordron.OAuth.Extensions
{
    /// <summary>
    /// Set of extension methods for <see cref="string"/>.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Returns MD5 Hash of input.
        /// </summary>
        /// <param name="input">The line.</param>
        public static string GetMd5Hash(this string input)
        {
            var provider = new MD5CryptoServiceProvider();
            var bytes = Encoding.UTF8.GetBytes(input);
            bytes = provider.ComputeHash(bytes);
            return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
        }
    }
}
