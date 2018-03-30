using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dotnet.Coveralls.CommandLine;
using Dotnet.Coveralls.Data;
using Dotnet.Coveralls.Io;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json;

namespace Dotnet.Coveralls.Parsers
{
    public class ChutzpahJsonParser : ICoverageParser
    {
        private readonly CoverallsOptions options;
        private readonly IFileProvider fileProvider;
        private readonly PathProcessor pathProcessor;

        public ChutzpahJsonParser(
            CoverallsOptions options,
            IFileProvider fileProvider,
            PathProcessor pathProcessor)
        {
            this.options = options;
            this.fileProvider = fileProvider;
            this.pathProcessor = pathProcessor;
        }

        public async Task<IEnumerable<CoverageFile>> ParseSourceFiles()
        {
            return await options.Chutzpah.SelectMany(async file => await ParseFile(file));
            
            async Task<IEnumerable<CoverageFile>> ParseFile(string file)
            {
                var fileInfo = fileProvider.GetFileInfo(file);
                if (!fileInfo.Exists) throw new PublishCoverallsException($"{file} was not found when parsing chutzpah coverage");

                var content = await fileInfo.ReadToEnd();

                var files = new List<CoverageFile>();
                var deserialized = JsonConvert.DeserializeObject<Dictionary<string, ChutzpahJsonFileItem>>(content);

                foreach (var item in deserialized.Values)
                {
                    var currentFilePath = item.FilePath;

                    if (item.LineExecutionCounts.Length == item.SourceLines.Length + 1)
                    {
                        item.LineExecutionCounts = item.LineExecutionCounts.Skip(1).ToArray();
                    }

                    if (options.UseRelativePaths)
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
        public class ChutzpahJsonFileItem
        {
            public string FilePath { get; set; }
            public int?[] LineExecutionCounts { get; set; }
            public string[] SourceLines { get; set; }
            public double CoveragePercentage { get; set; }
        }
    }
}