using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dotnet.Coveralls.Data;
using Dotnet.Coveralls.Publishing;
using static System.Environment;

namespace Dotnet.Coveralls.Git
{
    public class AppVeyorProvider : IGitDataResolver, ICoverallsDataProvider
    {
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

            public const string PR_COMMIT_ID = "APPVEYOR_PULL_REQUEST_HEAD_COMMIT";
            public const string PR_NUMBER = "APPVEYOR_PULL_REQUEST_NUMBER";
            public const string PR_HEAD_REPO_NAME = "APPVEYOR_PULL_REQUEST_HEAD_REPO_NAME";
        }

        public bool CanProvideData => bool.TryParse(GetEnvironmentVariable(nameof(AppVeyor).ToUpper()), out var value) && value;

        public GitData GitData =>
            new GitData
            {
                Head = new GitHead
                {
                    Id =
                        GetEnvironmentVariable(AppVeyor.PR_COMMIT_ID).NullIfEmpty() ??
                        GetEnvironmentVariable(AppVeyor.COMMIT_ID),
                    AuthorName = GetEnvironmentVariable(AppVeyor.COMMIT_AUTHOR),
                    AuthorEmail = GetEnvironmentVariable(AppVeyor.COMMIT_EMAIL),
                    CommitterName = GetEnvironmentVariable(AppVeyor.COMMIT_AUTHOR),
                    CommitterEmail = GetEnvironmentVariable(AppVeyor.COMMIT_EMAIL),
                    Message = GetEnvironmentVariable(AppVeyor.COMMIT_MESSAGE)
                },
                Branch = GetEnvironmentVariable(AppVeyor.COMMIT_BRANCH),
                Remotes = Remotes.ToArray(),
            };

        public Task<CoverallsData> ProvideCoverallsData() => Task.FromResult(new CoverallsData
        {
            CommitSha = GetEnvironmentVariable(AppVeyor.COMMIT_ID),
            ServiceBranch = GetEnvironmentVariable(AppVeyor.COMMIT_BRANCH),
            ServiceName = nameof(AppVeyor).ToLower(),
            ServiceJobId = GetEnvironmentVariable(AppVeyor.JOB_ID),
            ServiceNumber = GetEnvironmentVariable(AppVeyor.BUILD_VERSION),
            ServicePullRequest = GetEnvironmentVariable(AppVeyor.PR_NUMBER),
            ServiceBuildUrl = $"https://ci.appveyor.com/project/{GetEnvironmentVariable(AppVeyor.REPO_NAME)}/build/{GetEnvironmentVariable(AppVeyor.BUILD_VERSION)}",
        });

        private IEnumerable<GitRemote> Remotes
        {
            get
            {
                var host = GetEnvironmentVariable(AppVeyor.REPO_PROVIDER);
                var origin = GetEnvironmentVariable(AppVeyor.REPO_NAME);

                yield return new GitRemote
                {
                    Name = "origin",
                    Url = $"https://{host}.com/{origin}"
                };

                var fork = GetEnvironmentVariable(AppVeyor.PR_HEAD_REPO_NAME);
                if (fork != null && fork != origin)
                {
                    yield return new GitRemote
                    {
                        Name = "fork",
                        Url = $"https://{host}.com/{fork}"
                    };
                }
            }
        }
    }
}