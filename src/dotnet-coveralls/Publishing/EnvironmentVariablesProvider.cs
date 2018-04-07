using System.Threading.Tasks;
using Dotnet.Coveralls.Data;
using Dotnet.Coveralls.Git;
using static System.Environment;

namespace Dotnet.Coveralls.Publishing
{
    public class EnvironmentVariablesProvider : ICoverallsDataProvider, IGitDataResolver
    {
        public static class CI
        {
            public const string REPO_TOKEN = "COVERALLS_REPO_TOKEN";
            public const string BRANCH = "CI_BRANCH";            
            public const string BUILD_NUMBER = "CI_BUILD_NUMBER";
            public const string BUILD_URL = "CI_BUILD_URL";
            public const string NAME = "CI_NAME";
            public const string PULL_REQUEST = "CI_PULL_REQUEST";
        }

        public bool CanProvideData => true;

        public GitData GitData => new GitData
        {
            Branch = GetEnvironmentVariable(CI.BRANCH)
        };

        public Task<CoverallsData> ProvideCoverallsData() => Task.FromResult(new CoverallsData
        {
            RepoToken = GetEnvironmentVariable(CI.REPO_TOKEN),
            ServiceBuildUrl = GetEnvironmentVariable(CI.BUILD_URL),
            ServiceName = GetEnvironmentVariable(CI.NAME),
            ServiceNumber = GetEnvironmentVariable(CI.BUILD_NUMBER),
            ServicePullRequest = GetEnvironmentVariable(CI.PULL_REQUEST),
        });

        public Task<GitData> CreateGitData() => Task.FromResult(GitData);
    }
}
