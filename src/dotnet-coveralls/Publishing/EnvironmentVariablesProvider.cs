using System.Threading.Tasks;
using Dotnet.Coveralls.Adapters;
using Dotnet.Coveralls.Data;
using Dotnet.Coveralls.Git;

namespace Dotnet.Coveralls.Publishing
{
    public class EnvironmentVariablesProvider : ICoverallsDataProvider, IGitDataResolver
    {
        private readonly IEnvironmentVariables environmentVariables;

        public EnvironmentVariablesProvider(IEnvironmentVariables environmentVariables)
        {
            this.environmentVariables = environmentVariables;
        }

        public bool CanProvideData => true;

        public GitData GitData => new GitData
        {
            Branch = environmentVariables.GetEnvironmentVariable("CI_BRANCH")
        };

        public Task<CoverallsData> ProvideCoverallsData() => Task.FromResult(new CoverallsData
        {
            ServiceName = environmentVariables.GetEnvironmentVariable("CI_NAME"),
            ServiceNumber = environmentVariables.GetEnvironmentVariable("CI_BUILD_NUMBER"),
            PullRequestId = environmentVariables.GetEnvironmentVariable("CI_PULL_REQUEST"),
            RepoToken = environmentVariables.GetEnvironmentVariable("COVERALLS_REPO_TOKEN")
        });
    }
}
