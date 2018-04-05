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

namespace Dotnet.Coveralls.Publishing
{
    public class CoverallsPublisher
    {
        private readonly CoverallsOptions options;
        private readonly ICoverallsDataBuilder coverallsDataBuilder;
        private readonly IOutputFileWriter fileWriter;
        private readonly IFileProvider fileProvider;
        private readonly IEnvironmentVariables environmentVariables;
        private readonly ILogger<CoverallsPublisher> logger;
        private const string CoverallsEndpoint = "https://coveralls.io/api/v1/jobs";

        public CoverallsPublisher(
            CoverallsOptions options,
            ICoverallsDataBuilder coverallsDataBuilder,
            IOutputFileWriter fileWriter, 
            IFileProvider fileProvider,
            IEnvironmentVariables environmentVariables,
            ILoggerFactory loggerFactory)
        {
            this.options = options;
            this.coverallsDataBuilder = coverallsDataBuilder;
            this.fileWriter = fileWriter;
            this.fileProvider = fileProvider;
            this.environmentVariables = environmentVariables;
            this.logger = loggerFactory.CreateLogger<CoverallsPublisher>();
        }

        public async Task<int> Publish()
        {
            var data = await coverallsDataBuilder.ProvideCoverallsData();

            var contractResolver = new DefaultContractResolver { NamingStrategy = new SnakeCaseNamingStrategy() };

            if (options.DryRun)
            {
                data.RepoToken = "***";
                await fileWriter.WriteCoverageOutput(SerializeCoverallsData());
            }
            else
            {
                if (string.IsNullOrWhiteSpace(data.RepoToken))
                {
                    throw new PublishCoverallsException("No coveralls token specified. Either pass it with --repo-token or set it in the Environment Variable 'COVERALLS_REPO_TOKEN'.");
                }
                await UploadCoverage(SerializeCoverallsData());
                data.RepoToken = "***";
                logger.LogInformation(SerializeCoverallsData());
            }

            return 0;

            string SerializeCoverallsData() => JsonConvert.SerializeObject(
                data,
                new JsonSerializerSettings { ContractResolver = contractResolver, DefaultValueHandling = DefaultValueHandling.Ignore });
        }

        private  async Task UploadCoverage(string fileData)
        {
            var uploadResult = await Upload();
            if (!uploadResult.Success)
            {
                var message = $"Failed to upload to coveralls:{Environment.NewLine}{uploadResult.Response}";
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

                if (!string.IsNullOrWhiteSpace(uploadResult.Response))
                {
                    logger.LogInformation(uploadResult.Response);
                }
            }

            async Task<(bool Success, string Response)> Upload()
            {
                try
                {
                    var response = await CoverallsEndpoint.PostMultipartAsync(content => content.AddString("json_file", fileData));
                    return (response.IsSuccessStatusCode, $"{response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                }
                catch (Exception ex)
                {
                    return (false, ex.ToString());
                }
                //using (var stringContent = new StringContent(fileData))
                //using (var client = new HttpClient())
                //using (var formData = new MultipartFormDataContent())
                //{
                //    formData.Add(stringContent, "json_file", "coverage.json");

                //    var response = await client.PostAsync("https://coveralls.io/api/v1/jobs", formData);

                //    if (!response.IsSuccessStatusCode)
                //    {
                //        var content = await response.Content.ReadAsStringAsync();
                //        var message = JsonConvert.DeserializeObject<CoverallsResponse>(content).message;

                //        if (message.Length > 200)
                //        {
                //            message = message.Substring(0, 200);
                //        }

                //        return (false, $"{response.StatusCode} - {message}");
                //    }
                //    return (true, null);
                //}
            }
        }

        private class CoverallsResponse
        {
            public string message { get; set; }
        }
    }
}
