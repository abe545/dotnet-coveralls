using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Dotnet.Coveralls.CommandLine;
using Dotnet.Coveralls.Io;
using Microsoft.Extensions.FileProviders;

namespace Dotnet.Coveralls.Data
{
    public class CoverageFileBuilder : ICoverageFileBuilder
    {
        private readonly IFileProvider fileProvider;
        private readonly PathProcessor pathProcessor;
        private readonly CoverallsOptions options;

        public CoverageFileBuilder(
            IFileProvider fileProvider,
            PathProcessor pathProcessor, 
            CoverallsOptions options)
        {
            this.fileProvider = fileProvider;
            this.pathProcessor = pathProcessor;
            this.options = options;
        }

        public async Task<IEnumerable<CoverageFile>> Build(IEnumerable<FileCoverageData> coverageData)
        {
            return await coverageData.Select(BuildCoverageFile);
                
            async Task<CoverageFile> BuildCoverageFile(FileCoverageData coverageFileData)
            {
                var path = coverageFileData.FullPath;
                if (options.UseRelativePaths)
                {
                    path = pathProcessor.ConvertPath(path);
                }
                path = pathProcessor.UnixifyPath(path);

                var fileInfo = fileProvider.GetFileInfo(coverageFileData.FullPath);
                var lines = new List<string>();
                if (fileInfo.Exists)
                {
                    var readAllText = await fileInfo.ReadToEnd();
                    if (!string.IsNullOrWhiteSpace(readAllText))
                    {
                        // this is kind of dumb, but ReadLine will work for any line ending, and string.split
                        // can possibly generate extra blank lines (or have extra line ending chars) depending
                        // on the approach.
                        using (var sr = new StringReader(readAllText))
                        {
                            string nextLine;
                            while ((nextLine = sr.ReadLine()) != null)
                            {
                                lines.Add(nextLine);
                            }
                        }
                    }
                }

                var length = lines.Count;
                var coverage = coverageFileData.Coverage;
                if (length > coverage.Length)
                {
                    coverage = new int?[length];
                    coverageFileData.Coverage.CopyTo(coverage, 0);
                }

                var sourceDigest = Crypto.CalculateMD5Digest(string.Join("\n", lines?.ToArray() ?? new string[0]));
                return new CoverageFile(path, sourceDigest, coverage);
            }
        }
    }
}