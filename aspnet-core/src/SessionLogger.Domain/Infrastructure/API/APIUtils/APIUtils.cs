using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SessionLogger.Infrastructure.API
{
    public static class APIUtils
    {
        // Encrypting passwords with SHA1 in .NET and Java // http://authors.aspalliance.com/thycotic/articles/view.aspx?id=2 
        public static string b64_sha1(string input)
        {
            SHA1 sha = new SHA1Managed();
            ASCIIEncoding ae = new ASCIIEncoding();
            byte[] data = ae.GetBytes(input);
            byte[] digest = sha.ComputeHash(data);
            return Convert.ToBase64String(digest);
        }

        // generate random nonce 
        public static string generateNonce()
        {
            Random random = new Random();
            int len = 24; string chars = "0123456789abcdef";
            string nonce = "";
            for (int i = 0; i < len; i++)
            {
                nonce += chars.Substring(Convert.ToInt32(Math.Floor(random.NextDouble() * chars.Length)), 1);
            }
            return nonce;
        }

        // Time stamp in UTC string 
        public static string generateTimestamp()
        {
            return DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
        }

        // C#-Base64 Encoding // http://www.vbforums.com/showthread.php?t=287324 
        public static string base64Encode(string data)
        {
            byte[] encData_byte = new byte[data.Length];
            encData_byte = System.Text.Encoding.UTF8.GetBytes(data);
            string encodedData = Convert.ToBase64String(encData_byte);
            return encodedData;
        }
    }
}
