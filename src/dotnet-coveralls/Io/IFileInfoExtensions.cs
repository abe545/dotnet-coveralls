using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.FileProviders;

namespace Dotnet.Coveralls.Io
{
    public static class IFileInfoExtensions
    {
        public static async Task<string> ReadToEnd(this IFileInfo fileInfo)
        {
            using (var str = fileInfo.CreateReadStream())
            using (var reader = new StreamReader(str))
            {
                return await reader.ReadToEndAsync();
            }
        }
        public static async Task<XDocument> ReadXml(this IFileInfo fileInfo)
        {
            using (var str = fileInfo.CreateReadStream())
            {
                return await XDocument.LoadAsync(str, LoadOptions.PreserveWhitespace, CancellationToken.None);
            }
        }

        public static async Task<IEnumerable<string>> ReadAllLines(this IFileInfo fileInfo)
        {
            using (var str = fileInfo.CreateReadStream())
            using (var reader = new StreamReader(str))
            {
                var results = new List<string>();
                while (!reader.EndOfStream)
                {
                    results.Add(await reader.ReadLineAsync());
                }
                return results;
            }
        }
    }
}
