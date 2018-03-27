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
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Dotnet.Coveralls.Publishers
{
    public class CoverallsPublisher
    {
        private readonly IFileWriter fileWriter;
        private readonly IFileProvider fileProvider;

        public CoverallsPublisher(IFileWriter fileWriter, IFileProvider fileProvider)
        {
            this.fileWriter = fileWriter;
            this.fileProvider = fileProvider;
        }

        public async Task<int> Publish(CoverallsOptions args)
        {
            var repoToken = ResolveRepoToken(args);
            var outputFile = ResolveOutpuFile(args);

            //Main Processing
            var files = await BuildCoverageFiles(args);

            var gitData = ResolveGitData(args);

            var serviceName = ResolveServiceName(args);
            var serviceJobId = ResolveServiceJobId(args);
            var serviceNumber = ResolveServiceNumber(args);
            var pullRequestId = ResolvePullRequestId(args);
            var parallel = args.Parallel;

            var data = new CoverallData
            {
                RepoToken = repoToken,
                ServiceJobId = serviceJobId,
                ServiceName = serviceName ?? "dotnet-coveralls",
                ServiceNumber = serviceNumber,
                PullRequestId = pullRequestId,
                SourceFiles = files,
                Parallel = parallel,
                Git = gitData
            };

            var contractResolver = new DefaultContractResolver { NamingStrategy = new SnakeCaseNamingStrategy() };
            var fileData = JsonConvert.SerializeObject(data, new JsonSerializerSettings { ContractResolver = contractResolver, DefaultValueHandling = DefaultValueHandling.Ignore });
            if (!string.IsNullOrWhiteSpace(outputFile))
            {
                await WriteFileData(fileData, outputFile);
            }
            if (!args.DryRun)
            {
                await UploadCoverage(fileData, args.IgnoreUploadErrors);
            }

            return 0;
        }

        private string ResolveOutpuFile(CoverallsOptions args)
        {
            var outputFile = args.Output;
            if (!string.IsNullOrWhiteSpace(outputFile) && fileProvider.GetFileInfo(outputFile).Exists)
            {
                Console.WriteLine("output file '{0}' already exists and will be overwritten.", outputFile);
            }
            return outputFile;
        }

        private  string ResolveRepoToken(CoverallsOptions args)
        {
            string repoToken;
            if (args.RepoToken.IsNotNullOrWhitespace())
            {
                repoToken = args.RepoToken;
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

        private async Task<CoverageFile[]> BuildCoverageFiles(CoverallsOptions args)
        {
            var pathProcessor = new PathProcessor(args.BasePath);

            return
                (await Load(args.Chutzpah, CoverageMode.Chutzpah)).Concat(
                (await Load(args.DynamicCodeCoverage, CoverageMode.DynamicCodeCoverage))).Concat(
                (await Load(args.VsCoverage, CoverageMode.ExportCodeCoverage))).Concat(
                (await Load(args.Lcov, CoverageMode.LCov))).Concat(
                (await Load(args.MonoCov, CoverageMode.MonoCov))).Concat(
                (await Load(args.OpenCover, CoverageMode.OpenCover)))
                .ToArray();

            async Task<IEnumerable<CoverageFile>> Load(IEnumerable<string> toAdd, CoverageMode mode)
            {
                if (!(toAdd?.Any() ?? false)) return Enumerable.Empty<CoverageFile>();

                var result = new List<CoverageFile>();
                foreach (var f in toAdd)
                {
                    result.AddRange(await LoadCoverageFiles(mode, pathProcessor, f, args.UseRelativePaths));
                }

                return result;
            }
        }

        private  async Task UploadCoverage(string fileData, bool treatErrorsAsWarnings)
        {
            var uploadResult = await Upload();
            if (!uploadResult.Success)
            {
                var message = $"Failed to upload to coveralls\n{uploadResult.Error}";
                if (treatErrorsAsWarnings)
                {
                    await Console.Out.WriteLineAsync(message);
                }
                else
                {
                    ExitWithError(message);
                }
            }
            else
            {
                await Console.Out.WriteLineAsync("Coverage data uploaded to coveralls.");
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

        private async Task<List<CoverageFile>> LoadCoverageFiles(
            CoverageMode mode,
            PathProcessor pathProcessor,
            string inputArgument,
            bool useRelativePaths)
        {
            var coverageFiles = await new CoverageLoader()
                .LoadCoverageFiles(mode, fileProvider, pathProcessor, inputArgument, useRelativePaths);

            if (coverageFiles.Successful)
            {
                return coverageFiles.Value;
            }
            else
            {
                switch (coverageFiles.Error)
                {
                    case LoadCoverageFilesError.InputFileNotFound:
                        ExitWithError($"Input file '{inputArgument}' cannot be found");
                        break;
                    case LoadCoverageFilesError.ModeNotSupported:
                        ExitWithError($"Could not process mode {mode}");
                        break;
                    case LoadCoverageFilesError.UnknownFilesMissingError:
                        ExitWithError($"Unknown Error Finding files processing mode {mode}");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return null;
        }

        private void ExitWithError(string message) => throw new PublishCoverallsException(message);

        private string ResolveServiceName(CoverallsOptions args)
        {
            if (args.ServiceName.IsNotNullOrWhitespace()) return args.ServiceName;
            var isAppVeyor = new EnvironmentVariables().GetEnvironmentVariable("APPVEYOR");
            if (isAppVeyor == "True") return "appveyor";
            return "dotnet-coveralls";
        }

        private string ResolveServiceJobId(CoverallsOptions args)
        {
            if (args.JobId.IsNotNullOrWhitespace()) return args.JobId;
            var jobId = new EnvironmentVariables().GetEnvironmentVariable("APPVEYOR_JOB_ID");
            if (jobId.IsNotNullOrWhitespace()) return jobId;
            return null;
        }

        private string ResolveServiceNumber(CoverallsOptions args)
        {
            if (args.BuildNumber.IsNotNullOrWhitespace()) return args.BuildNumber;
            var jobId = new EnvironmentVariables().GetEnvironmentVariable("APPVEYOR_BUILD_NUMBER");
            if (jobId.IsNotNullOrWhitespace()) return jobId;
            return null;
        }

        private string ResolvePullRequestId(CoverallsOptions args)
        {
            if (args.PullRequestId.IsNotNullOrWhitespace()) return args.PullRequestId;
            var prId = new EnvironmentVariables().GetEnvironmentVariable("APPVEYOR_PULL_REQUEST_NUMBER");
            if (prId.IsNotNullOrWhitespace()) return prId;
            return null;
        }

        private GitData ResolveGitData(CoverallsOptions args)
        {
            var providers = new List<IGitDataResolver>
            {
                new CommandLineGitDataResolver(args),
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
                await Console.Error.WriteLineAsync($"Failed to write data to output file '{outputFile}'.");
                await Console.Error.WriteLineAsync(ex.ToString());
            }
        }

        private class CoverallsResponse
        {
            public string message { get; set; }
        }
    }
}
