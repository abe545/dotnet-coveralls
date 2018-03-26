using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Beefeater;
using Dotnet.Coveralls.Data;
using Dotnet.Coveralls.Io;
using Dotnet.Coveralls.Parsers;
using Dotnet.Coveralls.Ports;
using Microsoft.Extensions.FileProviders;

namespace Dotnet.Coveralls
{
    public class CoverageLoader
    {
        public async Task<Result<List<CoverageFile>, LoadCoverageFilesError>> LoadCoverageFiles(
            CoverageMode mode,
            IFileProvider fileProvider,
            PathProcessor pathProcessor, 
            string modeInput, 
            bool useRelativePaths)
        {
            var fileInfo = fileProvider.GetFileInfo(modeInput);
            if (!fileInfo.Exists)
            {
                return LoadCoverageFilesError.InputFileNotFound;
            }

            List<CoverageFile> files;
            if (mode == CoverageMode.MonoCov)
            {
                if (!fileInfo.IsDirectory)
                {
                    return LoadCoverageFilesError.InputFileNotFound;
                }

                var directory = fileProvider.GetDirectoryContents(modeInput);
                var documents = await LoadXDocuments(directory);

                files = new MonoCoverParser(pathProcessor).GenerateSourceFiles(documents, useRelativePaths);
            }
            else if (mode == CoverageMode.Chutzpah)
            {
                var source = await fileInfo.ReadToEnd();
                if (!string.IsNullOrWhiteSpace(source))
                {
                    files = new ChutzpahJsonParser(pathProcessor).GenerateSourceFiles(source, useRelativePaths);
                }
                else
                {
                    return LoadCoverageFilesError.InputFileNotFound;
                }
            }
            else if (mode == CoverageMode.LCov)
            {
                var lines = await fileInfo.ReadAllLines();

                var coverageData = new LcovParser().GenerateSourceFiles(lines.ToArray(), useRelativePaths);

                files = await BuildCoverageFiles(fileProvider, pathProcessor, useRelativePaths, coverageData);
            }
            else
            {
                //common xml-based single file formats
                var xmlParser = LoadXmlParser(mode);
                if (xmlParser == null)
                {
                    return LoadCoverageFilesError.ModeNotSupported;
                }

                var document = await fileInfo.ReadXml();
                var coverageData = xmlParser.GenerateSourceFiles(document);

                files = await BuildCoverageFiles(fileProvider, pathProcessor, useRelativePaths, coverageData);
            }

            if (files == null)
            {
                return LoadCoverageFilesError.UnknownFilesMissingError;
            }

            return files;
        }

        private async Task<Dictionary<string, XDocument>> LoadXDocuments(IEnumerable<IFileInfo> folderFiles)
        {
            var result = new Dictionary<string, XDocument>();

            foreach (var file in folderFiles.Where(f => f.Name.EndsWith(".xml", StringComparison.InvariantCultureIgnoreCase)))
            {
                result.Add(file.Name, await file.ReadXml());
            }

            return result;
        }

        private async Task<List<CoverageFile>> BuildCoverageFiles(
            IFileProvider fileProvider,
            PathProcessor pathProcessor, 
            bool useRelativePaths,
            List<FileCoverageData> coverageData)
        {
            return (await Task.WhenAll(coverageData.Select(async coverageFileData =>
            {
                var coverageBuilder = new CoverageFileBuilder(coverageFileData);

                var path = coverageFileData.FullPath;
                if (useRelativePaths)
                {
                    path = pathProcessor.ConvertPath(coverageFileData.FullPath);
                }
                path = pathProcessor.UnixifyPath(path);
                coverageBuilder.SetPath(path);

                var fileInfo = fileProvider.GetFileInfo(coverageFileData.FullPath);
                if (fileInfo.Exists)
                {
                    var readAllText = await fileInfo.ReadToEnd();
                    if (!string.IsNullOrWhiteSpace(readAllText))
                    {
                        coverageBuilder.AddSource(readAllText);
                    }
                }

                return coverageBuilder.CreateFile();
            }))).ToList();
        }

        private IXmlCoverageParser LoadXmlParser(CoverageMode mode)
        {
            switch (mode)
            {
                case CoverageMode.OpenCover:
                    return new OpenCoverParser();
                case CoverageMode.DynamicCodeCoverage:
                    return new DynamicCodeCoverageParser();
                case CoverageMode.ExportCodeCoverage:
                    return new ExportCodeCoverageParser();
                default:
                    return null;
            }
        }
    }
}