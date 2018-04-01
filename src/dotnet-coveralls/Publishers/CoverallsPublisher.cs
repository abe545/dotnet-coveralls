using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using BCLExtensions;
using Dotnet.Coveralls.Adapters;
using Dotnet.Coveralls.CommandLine;
using Dotnet.Coveralls.Data;
using Dotnet.Coveralls.GitDataResolvers;
using Dotnet.Coveralls.Io;
using Dotnet.Coveralls.Parsers;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Dotnet.Coveralls.Publishers
{
    public class CoverallsPublisher
    {
        private readonly CoverallsOptions options;
        private readonly IEnumerable<ICoverageParser> coverageParsers;
        private readonly IFileWriter fileWriter;
        private readonly IFileProvider fileProvider;
        private readonly ILogger<CoverallsPublisher> logger;

        public CoverallsPublisher(
            CoverallsOptions options,
            IEnumerable<ICoverageParser> coverageParsers,
            IFileWriter fileWriter, 
            IFileProvider fileProvider,
            ILoggerFactory loggerFactory)
        {
            this.options = options;
            this.coverageParsers = coverageParsers;
            this.fileWriter = fileWriter;
            this.fileProvider = fileProvider;
            this.logger = loggerFactory.CreateLogger<CoverallsPublisher>();
        }

        public async Task<int> Publish()
        {
            var outputFile = ResolveOutpuFile();

            var files = await ParseCoverageFiles();

            var gitData = ResolveGitData();

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
            if (!string.IsNullOrWhiteSpace(outputFile))
            {
                await WriteFileData(SerializeCoverallsData(), outputFile);
            }
            if (!options.DryRun)
            {
                data.RepoToken = ResolveRepoToken();
                await UploadCoverage(SerializeCoverallsData(), options.IgnoreUploadErrors);
            }

            return 0;

            string SerializeCoverallsData() => JsonConvert.SerializeObject(
                data,
                new JsonSerializerSettings { ContractResolver = contractResolver, DefaultValueHandling = DefaultValueHandling.Ignore });
        }

        private string ResolveOutpuFile()
        {
            var outputFile = options.Output;
            if (!string.IsNullOrWhiteSpace(outputFile) && fileProvider.GetFileInfo(outputFile).Exists)
            {
                logger.LogWarning($"output file '{outputFile}' already exists and will be overwritten.");
            }
            return outputFile;
        }

        private  string ResolveRepoToken()
        {
            string repoToken;
            if (options.RepoToken.IsNotNullOrWhitespace())
            {
                repoToken = options.RepoToken;
            }
            else
            {
                repoToken = Environment.GetEnvironmentVariable("COVERALLS_REPO_TOKEN");
                if (repoToken.IsNullOrWhitespace())
                {
                    ExitWithError("No token found in Environment Variable 'COVERALLS_REPO_TOKEN'.");
                }
            }
            return repoToken;
        }

        private  async Task UploadCoverage(string fileData, bool treatErrorsAsWarnings)
        {
            var uploadResult = await Upload();
            if (!uploadResult.Success)
            {
                var message = $"Failed to upload to coveralls\n{uploadResult.Error}";
                if (treatErrorsAsWarnings)
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

        private async Task<CoverageFile[]> ParseCoverageFiles() =>
            (await coverageParsers.SelectMany(p => p.ParseSourceFiles())).ToArray();

        private void ExitWithError(string message) => throw new PublishCoverallsException(message);

        private string ResolveServiceName()
        {
            if (options.ServiceName.IsNotNullOrWhitespace()) return options.ServiceName;
            var isAppVeyor = new EnvironmentVariables().GetEnvironmentVariable("APPVEYOR");
            if (isAppVeyor == "True") return "appveyor";
            return "dotnet-coveralls";
        }

        private string ResolveServiceJobId()
        {
            if (options.JobId.IsNotNullOrWhitespace()) return options.JobId;
            var jobId = new EnvironmentVariables().GetEnvironmentVariable("APPVEYOR_JOB_ID");
            if (jobId.IsNotNullOrWhitespace()) return jobId;
            return null;
        }

        private string ResolveServiceNumber()
        {
            if (options.BuildNumber.IsNotNullOrWhitespace()) return options.BuildNumber;
            var jobId = new EnvironmentVariables().GetEnvironmentVariable("APPVEYOR_BUILD_NUMBER");
            if (jobId.IsNotNullOrWhitespace()) return jobId;
            return null;
        }

        private string ResolvePullRequestId()
        {
            if (options.PullRequestId.IsNotNullOrWhitespace()) return options.PullRequestId;
            var prId = new EnvironmentVariables().GetEnvironmentVariable("APPVEYOR_PULL_REQUEST_NUMBER");
            if (prId.IsNotNullOrWhitespace()) return prId;
            return null;
        }

        private GitData ResolveGitData()
        {
            var providers = new List<IGitDataResolver>
            {
                new CommandLineGitDataResolver(options),
                new AppVeyorGitDataResolver(new EnvironmentVariables())
            };

            return providers
                .Where(p => p.CanProvideData)
                .Select(p => p.GitData)
                .Aggregate(new GitData(), CombineGitData);
        }

        private GitData CombineGitData(GitData accum, GitData toAdd) =>
            new GitData
            {
                Branch = accum.Branch ?? toAdd.Branch,
                Head = new GitHead
                {
                    AuthorEmail = accum.Head.AuthorEmail ?? toAdd.Head.AuthorEmail,
                    AuthorName = accum.Head.AuthorName ?? toAdd.Head.AuthorName,
                    ComitterEmail = accum.Head.ComitterEmail ?? toAdd.Head.ComitterEmail,
                    CommitterName = accum.Head.CommitterName ?? toAdd.Head.CommitterName,
                    Id = accum.Head.Id ?? toAdd.Head.Id,
                    Message = accum.Head.Message ?? toAdd.Head.Message
                },
                Remotes = new GitRemotes
                {
                    Name = accum.Remotes.Name ?? toAdd.Remotes.Name,
                    Url = accum.Remotes.Url ?? toAdd.Remotes.Url
                }
            };

        private async Task WriteFileData(string fileData, string outputFile)
        {
            try
            {
                await fileWriter.WriteAllText(outputFile, fileData);
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed to write data to output file '{outputFile}'.");
                logger.LogError(ex.ToString());
            }
        }

        private class CoverallsResponse
        {
            public string message { get; set; }
        }
    }
}
