using Dotnet.Coveralls.Data;
using Dotnet.Coveralls.Adapters;
using Dotnet.Coveralls.Publishing;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Dotnet.Coveralls.Git
{
    public class AppVeyorProvider : IGitDataResolver, ICoverallsDataProvider
    {
        private readonly IEnvironmentVariables variables;
        public static class AppVeyor
        {
            public const string REPO_NAME = "APPVEYOR_REPO_NAME";
            public const string REPO_PROVIDER = "APPVEYOR_REPO_PROVIDER";

            public const string COMMIT_ID = "APPVEYOR_REPO_COMMIT";
            public const string COMMIT_AUTHOR = "APPVEYOR_REPO_COMMIT_AUTHOR";
            public const string COMMIT_EMAIL = "APPVEYOR_REPO_COMMIT_AUTHOR_EMAIL";
            public const string COMMIT_MESSAGE = "APPVEYOR_REPO_COMMIT_MESSAGE";
            public const string COMMIT_BRANCH = "APPVEYOR_REPO_BRANCH";
            public const string JOB_ID = "APPVEYOR_JOB_ID";
            public const string BUILD_VERSION = "APPVEYOR_BUILD_VERSION";
            public const string PR_NUMBER = "APPVEYOR_PULL_REQUEST_NUMBER";
            public const string PR_COMMIT_ID = "APPVEYOR_PULL_REQUEST_HEAD_COMMIT";
        }

        public AppVeyorProvider(IEnvironmentVariables variables, ILogger logger)
        {
            this.variables = variables;

            var vars = System.Environment
                .GetEnvironmentVariables()
                .Keys
                .Cast<string>()
                .Where(k => k.StartsWith(nameof(AppVeyor), System.StringComparison.InvariantCultureIgnoreCase));

            foreach (var v in vars)
            {
                logger.LogDebug($"{v}: {variables.GetEnvironmentVariable(v)}");
            }
        }

        public bool CanProvideData => bool.TryParse(variables.GetEnvironmentVariable(nameof(AppVeyor).ToUpper()), out var value) && value;

        public GitData GitData =>
            new GitData
            {
                Head = new GitHead
                {
                    Id = variables.GetEnvironmentVariable(AppVeyor.PR_COMMIT_ID),
                    CommitterName = variables.GetEnvironmentVariable(AppVeyor.COMMIT_AUTHOR),
                    CommitterEmail = variables.GetEnvironmentVariable(AppVeyor.COMMIT_EMAIL),
                    Message = variables.GetEnvironmentVariable(AppVeyor.COMMIT_MESSAGE)
                },
                Branch = variables.GetEnvironmentVariable(AppVeyor.COMMIT_BRANCH),
            };

        public Task<CoverallsData> ProvideCoverallsData() => Task.FromResult(new CoverallsData
        {
            CommitSha = variables.GetEnvironmentVariable(AppVeyor.PR_COMMIT_ID).NullIfEmpty() ?? variables.GetEnvironmentVariable(AppVeyor.COMMIT_ID),
            ServiceBranch = variables.GetEnvironmentVariable(AppVeyor.COMMIT_BRANCH),
            ServiceName = nameof(AppVeyor).ToLower(),
            ServiceJobId = variables.GetEnvironmentVariable(AppVeyor.JOB_ID),
            ServiceNumber = variables.GetEnvironmentVariable(AppVeyor.BUILD_VERSION),
            ServicePullRequest = variables.GetEnvironmentVariable(AppVeyor.PR_NUMBER),
            ServiceBuildUrl = $"https://ci.appveyor.com/project/{variables.GetEnvironmentVariable(AppVeyor.REPO_NAME)}/build/{variables.GetEnvironmentVariable(AppVeyor.BUILD_VERSION)}",
        });

        public Task<GitData> CreateGitData() => Task.FromResult(GitData);
    }
}