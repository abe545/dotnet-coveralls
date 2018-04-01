using System;
using System.Threading.Tasks;
using Dotnet.Coveralls.CommandLine;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;

namespace Dotnet.Coveralls.Io
{
    public class OutputFileWriter : IOutputFileWriter
    {
        private readonly CoverallsOptions options;
        private readonly IFileWriter fileWriter;
        private readonly IFileProvider fileProvider;
        private readonly ILogger logger;

        public OutputFileWriter(
            CoverallsOptions options,
            IFileWriter fileWriter,
            IFileProvider fileProvider,
            ILoggerFactory loggerFactory)
        {
            this.options = options;
            this.fileWriter = fileWriter;
            this.fileProvider = fileProvider;
            this.logger = loggerFactory.CreateLogger<OutputFileWriter>();
        }

        public async Task WriteCoverageOutput(string text)
        {
            var outputFile = options.Output;
            if (!string.IsNullOrWhiteSpace(outputFile))
            {
                if (fileProvider.GetFileInfo(outputFile).Exists)
                {
                    logger.LogWarning($"output file '{outputFile}' already exists and will be overwritten.");
                }
                try
                {
                    await fileWriter.WriteAllText(outputFile, text);
                }
                catch (Exception ex)
                {
                    logger.LogError($"Failed to write data to output file '{outputFile}'.");
                    logger.LogError(ex.ToString());
                }
            }
        }
    }
}
