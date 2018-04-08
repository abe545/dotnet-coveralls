using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Dotnet.Coveralls.CommandLine;
using Dotnet.Coveralls.Data;
using Dotnet.Coveralls.Io;
using Microsoft.Extensions.FileProviders;

namespace Dotnet.Coveralls.Parsers
{
    public class MonoCoverParser : ICoverageParser
    {
        private readonly PathProcessor pathProcessor;
        private readonly CoverallsOptions options;
        private readonly IFileProvider fileProvider;

        public MonoCoverParser(
            PathProcessor pathProcessor,
            CoverallsOptions options,
            IFileProvider fileProvider)
        {
            this.pathProcessor = pathProcessor;
            this.options = options;
            this.fileProvider = fileProvider;
        }

        public async Task<IEnumerable<CoverageFile>> ParseSourceFiles()
        {
            return await options.MonoCov.SelectMany(ParseFile);

            async Task<IEnumerable<CoverageFile>> ParseFile(string directoryName)
            {
                var directory = fileProvider.GetDirectoryContents(directoryName);
                if (!directory.Exists) throw new PublishCoverallsException($"{directoryName} was not found when parsing monocover report");

                var documents = await LoadXDocuments();
                return GenerateSourceFiles();

                async Task<Dictionary<string, XDocument>> LoadXDocuments()
                {
                    var result = new Dictionary<string, XDocument>();

                    foreach (var file in directory.Where(f => f.Name.EndsWith(".xml", StringComparison.InvariantCultureIgnoreCase)))
                    {
                        result.Add(file.Name, await file.ReadXml());
                    }

                    return result;
                }

                IEnumerable<CoverageFile> GenerateSourceFiles() =>
                    documents
                        .Where(kv => kv.Key.StartsWith("class-") && kv.Key.EndsWith(".xml"))
                        .Select(kv => kv.Value.Root?.Element("source"))
                        .Where(sourceElement => sourceElement != null)
                        .Select(sourceElement =>
                        {
                            var coverage = new List<int?>();
                            var source = new List<string>();
                            var filePath = sourceElement.Attribute("sourceFile").Value;
                            filePath = pathProcessor.NormalizePath(filePath);

                            foreach (var line in sourceElement.Elements("l"))
                            {
                                if (!int.TryParse(line.Attribute("count").Value, out var coverageCount))
                                {
                                    coverage.Add(null);
                                }
                                else
                                {
                                    coverage.Add(coverageCount);
                                }
                                source.Add(line.Value);
                            }

                            return new CoverageFile(filePath, source, coverage.ToArray());
                        });
            }
        }
    }
}