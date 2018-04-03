using System.Threading.Tasks;
using Dotnet.Coveralls.Adapters;
using Dotnet.Coveralls.Data;
using Dotnet.Coveralls.Git;

namespace Dotnet.Coveralls.Publishing
{
    public class EnvironmentVariablesProvider : ICoverallsDataProvider, IGitDataResolver
    {
        private readonly IEnvironmentVariables environmentVariables;

        public static class CI
        {
            public const string REPO_TOKEN = "COVERALLS_REPO_TOKEN";
            public const string BRANCH = "CI_BRANCH";            
            public const string BUILD_NUMBER = "CI_BUILD_NUMBER";
            public const string BUILD_URL = "CI_BUILD_URL";
            public const string NAME = "CI_NAME";
            public const string PULL_REQUEST = "CI_PULL_REQUEST";
        }

        public EnvironmentVariablesProvider(IEnvironmentVariables environmentVariables)
        {
            this.environmentVariables = environmentVariables;
        }

        public bool CanProvideData => true;

        public GitData GitData => new GitData
        {
            Branch = environmentVariables.GetEnvironmentVariable(CI.BRANCH)
        };

        public Task<CoverallsData> ProvideCoverallsData() => Task.FromResult(new CoverallsData
        {
            RepoToken = environmentVariables.GetEnvironmentVariable(CI.REPO_TOKEN),
            ServiceBuildUrl = environmentVariables.GetEnvironmentVariable(CI.BUILD_URL),
            ServiceName = environmentVariables.GetEnvironmentVariable(CI.NAME),
            ServiceNumber = environmentVariables.GetEnvironmentVariable(CI.BUILD_NUMBER),
            ServicePullRequest = environmentVariables.GetEnvironmentVariable(CI.PULL_REQUEST),
        });
    }
}
