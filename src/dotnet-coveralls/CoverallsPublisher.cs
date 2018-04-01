using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Dotnet.Coveralls.Adapters;
using Dotnet.Coveralls.CommandLine;
using Dotnet.Coveralls.Data;
using Dotnet.Coveralls.Git;
using Dotnet.Coveralls.Io;
using Dotnet.Coveralls.Parsers;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Dotnet.Coveralls
{
    public class CoverallsPublisher
    {
        private readonly CoverallsOptions options;
        private readonly ICoverageProvider coverageProvider;
        private readonly IGitDataProvider gitDataProvider;
        private readonly IOutputFileWriter fileWriter;
        private readonly IFileProvider fileProvider;
        private readonly ILogger<CoverallsPublisher> logger;

        public CoverallsPublisher(
            CoverallsOptions options,
            ICoverageProvider coverageProvider,
            IGitDataProvider gitDataProvider,
            IOutputFileWriter fileWriter, 
            IFileProvider fileProvider,
            ILoggerFactory loggerFactory)
        {
            this.options = options;
            this.coverageProvider = coverageProvider;
            this.gitDataProvider = gitDataProvider;
            this.fileWriter = fileWriter;
            this.fileProvider = fileProvider;
            this.logger = loggerFactory.CreateLogger<CoverallsPublisher>();
        }

        public async Task<int> Publish()
        {
            var files = await coverageProvider.ProvideCoverageFiles();
            var gitData = gitDataProvider.ProvideGitData();

            var serviceName = ResolveServiceName();
            var serviceJobId = ResolveServiceJobId();
            var serviceNumber = ResolveServiceNumber();
            var pullRequestId = ResolvePullRequestId();
            var parallel = options.Parallel;

            var data = new CoverallData
            {
                ServiceJobId = serviceJobId,
                ServiceName = serviceName ?? "dotnet-coveralls",
                ServiceNumber = serviceNumber,
                PullRequestId = pullRequestId,
                SourceFiles = files,
                Parallel = parallel,
                Git = gitData
            };

            var contractResolver = new DefaultContractResolver { NamingStrategy = new SnakeCaseNamingStrategy() };
            await fileWriter.WriteCoverageOutput(SerializeCoverallsData());

            if (!options.DryRun)
            {
                data.RepoToken = ResolveRepoToken();
                await UploadCoverage(SerializeCoverallsData());
            }

            return 0;

            string SerializeCoverallsData() => JsonConvert.SerializeObject(
                data,
                new JsonSerializerSettings { ContractResolver = contractResolver, DefaultValueHandling = DefaultValueHandling.Ignore });
        }

        private  string ResolveRepoToken()
        {
            string repoToken;
            if (!string.IsNullOrWhiteSpace(options.RepoToken))
            {
                repoToken = options.RepoToken;
            }
            else
            {
                repoToken = Environment.GetEnvironmentVariable("COVERALLS_REPO_TOKEN");
                if (string.IsNullOrWhiteSpace(repoToken))
                {
                    ExitWithError("No token found in Environment Variable 'COVERALLS_REPO_TOKEN'.");
                }
            }
            return repoToken;
        }

        private  async Task UploadCoverage(string fileData)
        {
            var uploadResult = await Upload();
            if (!uploadResult.Success)
            {
                var message = $"Failed to upload to coveralls:{Environment.NewLine}{uploadResult.Error}";
                if (options.IgnoreUploadErrors)
                {
                    logger.LogWarning(message);
                }
                else
                {
                    ExitWithError(message);
                }
            }
            else
            {
                logger.LogInformation("Coverage data uploaded to coveralls.");
            }

            async Task<(bool Success, string Error)> Upload()
            {
                using (var stringContent = new StringContent(fileData))
                using (var client = new HttpClient())
                using (var formData = new MultipartFormDataContent())
                {
                    formData.Add(stringContent, "json_file", "coverage.json");

                    var response = await client.PostAsync("https://coveralls.io/api/v1/jobs", formData);

                    if (!response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var message = JsonConvert.DeserializeObject<CoverallsResponse>(content).message;

                        if (message.Length > 200)
                        {
                            message = message.Substring(0, 200);
                        }

                        return (false, $"{response.StatusCode} - {message}");
                    }
                    return (true, null);
                }
            }
        }

        private void ExitWithError(string message) => throw new PublishCoverallsException(message);

        private string ResolveServiceName()
        {
            if (!string.IsNullOrWhiteSpace(options.ServiceName)) return options.ServiceName;
            var isAppVeyor = new EnvironmentVariables().GetEnvironmentVariable("APPVEYOR");
            if (isAppVeyor == "True") return "appveyor";
            return "dotnet-coveralls";
        }

        private string ResolveServiceJobId()
        {
            if (!string.IsNullOrWhiteSpace(options.JobId)) return options.JobId;
            var jobId = new EnvironmentVariables().GetEnvironmentVariable("APPVEYOR_JOB_ID");
            if (!string.IsNullOrWhiteSpace(jobId)) return jobId;
            return null;
        }

        private string ResolveServiceNumber()
        {
            if (!string.IsNullOrWhiteSpace(options.BuildNumber)) return options.BuildNumber;
            var jobId = new EnvironmentVariables().GetEnvironmentVariable("APPVEYOR_BUILD_NUMBER");
            if (!string.IsNullOrWhiteSpace(jobId)) return jobId;
            return null;
        }

        private string ResolvePullRequestId()
        {
            if (!string.IsNullOrWhiteSpace(options.PullRequestId)) return options.PullRequestId;
            var prId = new EnvironmentVariables().GetEnvironmentVariable("APPVEYOR_PULL_REQUEST_NUMBER");
            if (!string.IsNullOrWhiteSpace(prId)) return prId;
            return null;
        }

        private class CoverallsResponse
        {
            public string message { get; set; }
        }
    }
}
