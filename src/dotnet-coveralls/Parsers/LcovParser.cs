using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dotnet.Coveralls.CommandLine;
using Dotnet.Coveralls.Data;
using Dotnet.Coveralls.Io;
using Microsoft.Extensions.FileProviders;

namespace Dotnet.Coveralls.Parsers
{
    public class LcovParser : ICoverageParser
    {
        private readonly CoverallsOptions options;
        private readonly IFileProvider fileProvider;
        private readonly ICoverageFileBuilder coverageFileBuilder;

        public LcovParser(
            CoverallsOptions options,
            IFileProvider fileProvider,
            ICoverageFileBuilder coverageFileBuilder)
        {
            this.options = options;
            this.fileProvider = fileProvider;
            this.coverageFileBuilder = coverageFileBuilder;
        }

        public async Task<IEnumerable<CoverageFile>> ParseSourceFiles()
        {
            return await coverageFileBuilder.Build(await options.Lcov.SelectMany(ParseFile));

            async Task<IEnumerable<FileCoverageData>> ParseFile(string fileName)
            {
                var fileInfo = fileProvider.GetFileInfo(fileName);
                if (!fileInfo.Exists) throw new PublishCoverallsException($"{fileName} was not found when parsing lcov report");

                var lines = await fileInfo.ReadAllLines();
                {
                    FileCoverageDataBuilder coverageBuilder = null;
                    var files = new List<FileCoverageData>();
                    foreach (var line in lines)
                    {
                        var matches = Regex.Match(line, "^SF:(.*)");
                        if (matches.Success)
                        {
                            coverageBuilder = new FileCoverageDataBuilder(matches.Groups[1].Value);
                            continue;
                        }
                        matches = Regex.Match(line, @"^DA:(\d+),(\d+)");
                        if (matches.Success)
                        {
                            if (coverageBuilder != null)
                            {
                                var lineNumber = int.Parse(matches.Groups[1].Value);
                                var coverageNumber = int.Parse(matches.Groups[2].Value);
                                coverageBuilder.RecordCoverage(lineNumber, coverageNumber);
                            }
                            continue;
                        }
                        if (line.Equals("end_of_record"))
                        {
                            if (coverageBuilder != null)
                            {
                                files.Add(coverageBuilder.CreateFile());
                                coverageBuilder = null;
                            }
                        }
                    }
                    return files;
                }
            }
        }
    }
}