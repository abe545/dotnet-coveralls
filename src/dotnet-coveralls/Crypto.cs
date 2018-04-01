using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Dotnet.Coveralls
{
    public class Crypto
    {
        public static string CalculateMD5Digest(string data)
        {
            var md5 = MD5.Create();
            var inputBytes = Encoding.ASCII.GetBytes(data);
            var hash = md5.ComputeHash(inputBytes);

            return hash.Select(b => b.ToString("X2")).Aggregate((current, next) => current + next);
        }
    }
}