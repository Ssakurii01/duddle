using System;
using System.Text;
using UnityEngine;

namespace Luxodd.Game.Scripts.HelpersAndUtils
{
    /// <summary>
    /// Hashes a string using the key via XOR + Base64
    /// </summary>
    public static class PinCodeHasher 
    {
        public static string HashWithKey(string value, string key)
        {
            byte[] valueBytes = Encoding.UTF8.GetBytes(value);
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] result = new byte[valueBytes.Length];

            for (int i = 0; i < valueBytes.Length; i++)
            {
                result[i] = (byte)(valueBytes[i] ^ keyBytes[i % keyBytes.Length]);
            }

            return Convert.ToBase64String(result);
        }
        
        /// <summary>
        /// Decrypts the string encoded by HashWithKey
        /// </summary>
        public static string UnhashWithKey(string hashedValue, string key)
        {
            byte[] hashedBytes = Convert.FromBase64String(hashedValue);
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] result = new byte[hashedBytes.Length];

            for (int i = 0; i < hashedBytes.Length; i++)
            {
                result[i] = (byte)(hashedBytes[i] ^ keyBytes[i % keyBytes.Length]);
            }

            return Encoding.UTF8.GetString(result);
        }
    }
    
    
}
