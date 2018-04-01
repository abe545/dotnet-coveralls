using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dotnet.Coveralls.CommandLine;
using Dotnet.Coveralls.Data;
using Dotnet.Coveralls.Io;
using Microsoft.Extensions.FileProviders;

namespace Dotnet.Coveralls.Parsers
{
    public class OpenCoverParser: ICoverageParser
    {
        private readonly CoverallsOptions options;
        private readonly IFileProvider fileProvider;
        private readonly ICoverageFileBuilder coverageFileBuilder;

        public OpenCoverParser(
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
            return await coverageFileBuilder.Build(await options.OpenCover.SelectMany(ParseFile));

            async Task<IEnumerable<FileCoverageData>> ParseFile(string fileName)
            {
                var fileInfo = fileProvider.GetFileInfo(fileName);
                if (!fileInfo.Exists) throw new PublishCoverallsException($"{fileName} was not found when parsing open cover report");

                var document = await fileInfo.ReadXml();
                if (document.Root == null)
                {
                    return Enumerable.Empty<FileCoverageData>();
                }

                var files = new List<FileCoverageData>();
                var xElement = document.Root?.Element("Modules");
                if (xElement != null)
                {
                    foreach (var module in xElement.Elements("Module"))
                    {
                        var attribute = module.Attribute("skippedDueTo");
                        if (string.IsNullOrEmpty(attribute?.Value))
                        {
                            var filesElement = module.Element("Files");
                            if (filesElement != null)
                            {
                                foreach (var file in filesElement.Elements("File"))
                                {
                                    var fileid = file.Attribute("uid")?.Value;
                                    var fullPath = file.Attribute("fullPath")?.Value;

                                    var coverageBuilder = new FileCoverageDataBuilder(fullPath);

                                    var classesElement = module.Element("Classes");
                                    if (classesElement != null)
                                    {
                                        foreach (var @class in classesElement.Elements("Class"))
                                        {
                                            var methods = @class.Element("Methods");
                                            if (methods != null)
                                            {
                                                foreach (var method in methods.Elements("Method"))
                                                {
                                                    var sequencePointsElement = method.Element("SequencePoints");
                                                    if (sequencePointsElement != null)
                                                    {
                                                        foreach (
                                                            var sequencePoint in
                                                                sequencePointsElement.Elements("SequencePoint"))
                                                        {
                                                            var sequenceFileid = sequencePoint.Attribute("fileid").Value;
                                                            if (fileid == sequenceFileid)
                                                            {
                                                                var sourceLine =
                                                                    int.Parse(sequencePoint.Attribute("sl").Value);
                                                                var visitCount =
                                                                    int.Parse(sequencePoint.Attribute("vc").Value);

                                                                coverageBuilder.RecordCoverage(sourceLine, visitCount);
                                                            }
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
                }

                return files;
            }
        }
    }
}