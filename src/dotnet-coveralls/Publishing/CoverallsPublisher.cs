using System;
using System.Net.Http;
using System.Threading.Tasks;
using Dotnet.Coveralls.Adapters;
using Dotnet.Coveralls.CommandLine;
using Dotnet.Coveralls.Io;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Flurl;
using Flurl.Http;
using System.IO;
using System.Text;

namespace Dotnet.Coveralls.Publishing
{
    public class CoverallsPublisher
    {
        private readonly CoverallsOptions options;
        private readonly ICoverallsDataBuilder coverallsDataBuilder;
        private readonly IOutputFileWriter fileWriter;
        private readonly IFileProvider fileProvider;
        private readonly IEnvironmentVariables environmentVariables;
        private readonly ILogger logger;
        private const string CoverallsEndpoint = "https://coveralls.io/api/v1/jobs";

        public CoverallsPublisher(
            CoverallsOptions options,
            ICoverallsDataBuilder coverallsDataBuilder,
            IOutputFileWriter fileWriter, 
            IFileProvider fileProvider,
            IEnvironmentVariables environmentVariables,
            ILogger logger)
        {
            this.options = options;
            this.coverallsDataBuilder = coverallsDataBuilder;
            this.fileWriter = fileWriter;
            this.fileProvider = fileProvider;
            this.environmentVariables = environmentVariables;
            this.logger = logger;
        }

        public async Task<int> Publish()
        {
            var data = await coverallsDataBuilder.ProvideCoverallsData();

            var contractResolver = new DefaultContractResolver { NamingStrategy = new SnakeCaseNamingStrategy() };

            if (options.DryRun)
            {
                data.RepoToken = "***";
                await fileWriter.WriteCoverageOutput(SerializeCoverallsData());
                logger.LogDebug(SerializeCoverallsData());
            }
            else
            {
                if (string.IsNullOrWhiteSpace(data.RepoToken))
                {
                    throw new PublishCoverallsException("No coveralls token specified. Either pass it with --repo-token or set it in the Environment Variable 'COVERALLS_REPO_TOKEN'.");
                }

                var token = data.RepoToken;
                data.RepoToken = "***";
                logger.LogDebug("Uploading to coveralls");
                logger.LogDebug(SerializeCoverallsData());
                data.RepoToken = token;

                await UploadCoverage();
            }

            return 0;

            string SerializeCoverallsData() => JsonConvert.SerializeObject(
                data,
                new JsonSerializerSettings { ContractResolver = contractResolver, DefaultValueHandling = DefaultValueHandling.Ignore });

            async Task UploadCoverage()
            {
                try
                {
                    var fileData = SerializeCoverallsData();
                    using (var stringContent = new StringContent(fileData))
                    using (var formData = new MultipartFormDataContent())
                    {
                        formData.Add(stringContent, "json_file", "coverage.json");
                        var response = await CoverallsEndpoint.PostAsync(formData);
                        var result = await response.Content.ReadAsStringAsync();

                        if (!response.IsSuccessStatusCode)
                        {
                            var message = $"Failed to upload to coveralls:{Environment.NewLine}{response.StatusCode}: {result}";
                            if (options.IgnoreUploadErrors)
                            {
                                logger.LogWarning(message);
                            }
                            else
                            {
                                throw new PublishCoverallsException(message);
                            }
                        }
                        else
                        {
                            logger.LogInformation("Coverage data uploaded to coveralls.");

                            if (!string.IsNullOrWhiteSpace(result))
                            {
                                logger.LogInformation(result);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    var message = $"Failed to upload to coveralls:{Environment.NewLine}{ex.ToString()}";
                    if (options.IgnoreUploadErrors)
                    {
                        logger.LogWarning(message);
                    }
                    else
                    {
                        throw new PublishCoverallsException(message);
                    }
                }
            }
        }
        
        private class CoverallsResponse
        {
            public string message { get; set; }
        }
    }
}
