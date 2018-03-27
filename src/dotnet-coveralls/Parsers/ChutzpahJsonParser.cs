using System.Collections.Generic;
using System.Linq;
using Dotnet.Coveralls.Data;
using Newtonsoft.Json;

namespace Dotnet.Coveralls.Parsers
{
    public class ChutzpahJsonFileItem
    {
        public string FilePath { get; set; }
        public int?[] LineExecutionCounts { get; set; }
        public string[] SourceLines { get; set; }
        public double CoveragePercentage { get; set; }
    }

    public class ChutzpahJsonParser
    {
        private readonly PathProcessor pathProcessor;

        public ChutzpahJsonParser(PathProcessor pathProcessor)
        {
            this.pathProcessor = pathProcessor;
        }

        public List<CoverageFile> GenerateSourceFiles(string content, bool useRelativePaths)
        {
            var files = new List<CoverageFile>();
            var deserialized = JsonConvert.DeserializeObject<Dictionary<string, ChutzpahJsonFileItem>>(content);

            foreach (var item in deserialized.Values)
            {
                var currentFilePath = item.FilePath;

                if (item.LineExecutionCounts.Length == item.SourceLines.Length + 1)
                {
                    item.LineExecutionCounts = item.LineExecutionCounts.Skip(1).ToArray(); // fix chutzpah issue.
                }

                if (useRelativePaths)
                {
                    currentFilePath = pathProcessor.ConvertPath(currentFilePath);
                }

                currentFilePath = pathProcessor.UnixifyPath(currentFilePath);

                files.Add(new CoverageFile(currentFilePath,
                    Crypto.CalculateMD5Digest(string.Join(",", item.SourceLines)), item.LineExecutionCounts));
            }

            return files;
        }
    }
}