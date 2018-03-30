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
    public class ExportCodeCoverageParser : ICoverageParser
    {
        private readonly CoverallsOptions options;
        private readonly IFileProvider fileProvider;
        private readonly ICoverageFileBuilder coverageFileBuilder;

        public ExportCodeCoverageParser(
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
            return await coverageFileBuilder.Build(await options.VsCoverage.SelectMany(ParseFile));
        
            async Task<IEnumerable<FileCoverageData>> ParseFile(string fileName)
            {
                var fileInfo = fileProvider.GetFileInfo(fileName);
                if (!fileInfo.Exists) throw new PublishCoverallsException($"{fileName} was not found when parsing visual studio coverage");

                var document = await fileInfo.ReadXml();
                if (document.Root == null)
                {
                    return Enumerable.Empty<FileCoverageData>();
                }
                                
                var builderBySourceId = new Dictionary<string, FileCoverageDataBuilder>();
                var builders = new List<FileCoverageDataBuilder>();

                foreach (var sourceFile in document.Root.Elements("SourceFileNames"))
                {
                    var idElement = sourceFile.Element("SourceFileID");
                    var fileNameElement = sourceFile.Element("SourceFileName");
                    if (idElement != null && fileNameElement != null)
                    {
                        var builder = new FileCoverageDataBuilder(fileNameElement.Value);
                        builderBySourceId.Add(idElement.Value, builder);
                        builders.Add(builder);
                    }
                }

                foreach (var module in document.Root.Elements("Module"))
                {
                    var namespaceTable = module.Element("NamespaceTable");
                    if (namespaceTable == null)
                    {
                        continue;
                    }

                    foreach (var lines in namespaceTable.Elements("Class")
                        .Elements("Method")
                        .Elements("Lines"))
                    {
                        var sourceFileIdElement = lines.Element("SourceFileID");
                        var sourceStartLineElement = lines.Element("LnStart");
                        var sourceEndLineElement = lines.Element("LnEnd");
                        var coveredElement = lines.Element("Coverage");

                        if (sourceFileIdElement == null ||
                            sourceStartLineElement == null ||
                            sourceEndLineElement == null ||
                            coveredElement == null ||
                            !builderBySourceId.TryGetValue(sourceFileIdElement.Value, out var coverageBuilder))
                        {
                            continue;
                        }

                        var sourceStartLine = int.Parse(sourceStartLineElement.Value);
                        var sourceEndLine = int.Parse(sourceEndLineElement.Value);

                        // A value of 2 means completely covered
                        var covered = coveredElement.Value == "2";

                        for (var lineNumber = sourceStartLine; lineNumber <= sourceEndLine; lineNumber++)
                        {
                            coverageBuilder.RecordCoverage(lineNumber, covered ? 1 : 0);
                        }
                    }
                }

                return builders.Select(b => b.CreateFile());
            }
        }
    }
}