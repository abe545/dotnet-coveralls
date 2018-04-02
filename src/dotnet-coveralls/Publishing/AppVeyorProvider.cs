using Dotnet.Coveralls.Data;
using Dotnet.Coveralls.Adapters;
using Dotnet.Coveralls.Publishing;
using System.Threading.Tasks;

namespace Dotnet.Coveralls.Git
{
    public class AppVeyorProvider : IGitDataResolver, ICoverallsDataProvider
    {
        private readonly IEnvironmentVariables variables;

        public AppVeyorProvider(IEnvironmentVariables variables)
        {
            this.variables = variables;
        }

        public bool CanProvideData => bool.TryParse(variables.GetEnvironmentVariable("APPVEYOR"), out var value) && value;

        public GitData GitData =>
            new GitData
            {
                Head = new GitHead
                {
                    Id = variables.GetEnvironmentVariable("APPVEYOR_REPO_COMMIT"),
                    AuthorName = variables.GetEnvironmentVariable("APPVEYOR_REPO_COMMIT_AUTHOR"),
                    AuthorEmail = variables.GetEnvironmentVariable("APPVEYOR_REPO_COMMIT_AUTHOR_EMAIL"),
                    CommitterName = variables.GetEnvironmentVariable("APPVEYOR_REPO_COMMIT_AUTHOR"),
                    ComitterEmail = variables.GetEnvironmentVariable("APPVEYOR_REPO_COMMIT_AUTHOR_EMAIL"),
                    Message = variables.GetEnvironmentVariable("APPVEYOR_REPO_COMMIT_MESSAGE")
                },
                Branch =
                    variables.GetEnvironmentVariable("APPVEYOR_PULL_REQUEST_HEAD_REPO_BRANCH") ??
                    variables.GetEnvironmentVariable("APPVEYOR_REPO_BRANCH")
            };

        public Task<CoverallsData> ProvideCoverallsData() => Task.FromResult(new CoverallsData
        {
            ServiceName = "appveyor",
            ServiceJobId = variables.GetEnvironmentVariable("APPVEYOR_JOB_ID"),
            ServiceNumber = variables.GetEnvironmentVariable("APPVEYOR_BUILD_VERSION"),
            ServicePullRequest = variables.GetEnvironmentVariable("APPVEYOR_PULL_REQUEST_NUMBER"),
        });
    }
}