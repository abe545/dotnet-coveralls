using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dotnet.Coveralls.CommandLine;
using Dotnet.Coveralls.Data;
using Dotnet.Coveralls.Io;
using Microsoft.Extensions.FileProviders;

namespace Dotnet.Coveralls.Parsers
{
    public class DynamicCodeCoverageParser : ICoverageParser
    {
        private readonly CoverallsOptions options;
        private readonly IFileProvider fileProvider;
        private readonly PathProcessor pathProcessor;
        private readonly ICoverageFileBuilder coverageFileBuilder;

        public DynamicCodeCoverageParser(
            CoverallsOptions options,
            IFileProvider fileProvider,
            PathProcessor pathProcessor,
            ICoverageFileBuilder coverageFileBuilder)
        {
            this.options = options;
            this.fileProvider = fileProvider;
            this.pathProcessor = pathProcessor;
            this.coverageFileBuilder = coverageFileBuilder;
        }

        public async Task<IEnumerable<CoverageFile>> ParseSourceFiles()
        {
            return await coverageFileBuilder.Build(await options.DynamicCodeCoverage.SelectMany(ParseFile));

            async Task<IEnumerable<FileCoverageData>> ParseFile(string fileName)
            {
                var fileInfo = fileProvider.GetFileInfo(fileName);
                if (!fileInfo.Exists) throw new PublishCoverallsException($"{fileName} was not found when parsing dynamic coverage");

                var document = await fileInfo.ReadXml();

                var files = new List<FileCoverageData>();
                var xElement = document.Root?.Element("modules");
                if (xElement != null)
                {
                    foreach (var module in xElement.Elements("module"))
                    {
                        var filesElement = module.Element("source_files");
                        if (filesElement != null)
                        {
                            foreach (var file in filesElement.Elements("source_file"))
                            {
                                var fileid = file.Attribute("id").Value;
                                var fullPath = file.Attribute("path").Value;

                                var coverageBuilder = new FileCoverageDataBuilder(fullPath);

                                var classesElement = module.Element("functions");
                                if (classesElement != null)
                                {
                                    foreach (var @class in classesElement.Elements("function"))
                                    {
                                        var ranges = @class.Element("ranges");
                                        if (ranges != null)
                                        {
                                            foreach (var range in ranges.Elements("range"))
                                            {
                                                var rangeFileId = range.Attribute("source_id").Value;
                                                if (fileid == rangeFileId)
                                                {
                                                    var sourceStartLine = int.Parse(range.Attribute("start_line").Value);
                                                    var sourceEndLine = int.Parse(range.Attribute("end_line").Value);
                                                    var covered = range.Attribute("covered").Value == "yes";

                                                    foreach (
                                                        var lineNumber in
                                                            Enumerable.Range(sourceStartLine,
                                                                sourceEndLine - sourceStartLine + 1))
                                                    {
                                                        coverageBuilder.RecordCoverage(lineNumber, covered ? 1 : 0);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }

                                files.Add(coverageBuilder.CreateFile());
                            }
                        }
                    }
                }

                return files;
            }
        }
    }
}