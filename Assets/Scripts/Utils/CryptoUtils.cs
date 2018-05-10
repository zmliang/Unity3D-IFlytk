using System;
using System.Text;

namespace JinkeGroup.Util
{
    public static class CryptoUtils
    {
        public static string Sha1(string value)
        {
            byte[] valueBuffer = Encoding.UTF8.GetBytes(value);
            return Sha1(valueBuffer);
        }

        public static string Sha1(byte[] value)
        {
            var sha1 = new System.Security.Cryptography.SHA1Managed();
            byte[] hashBuffer = sha1.ComputeHash(value);
            string delimitedHash = BitConverter.ToString(hashBuffer);
            return delimitedHash.Replace("-", string.Empty).ToLowerInvariant() ;
        }
    }
}


