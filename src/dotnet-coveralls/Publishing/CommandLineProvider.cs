using System.Threading.Tasks;
using Dotnet.Coveralls.CommandLine;
using Dotnet.Coveralls.Data;

namespace Dotnet.Coveralls.Publishing
{
    public class CommandLineProvider : ICoverallsDataProvider
    {
        private readonly CoverallsOptions options;

        public CommandLineProvider(CoverallsOptions options)
        {
            this.options = options;
        }

        public bool CanProvideData => true;

        public Task<CoverallsData> ProvideCoverallsData() => Task.FromResult(new CoverallsData
        {
            ServiceName = options.ServiceName,
            ServiceJobId = options.JobId,
            ServiceNumber = options.BuildNumber,
            ServicePullRequest = options.PullRequestId,
            RepoToken = options.RepoToken,
            CommitSha = options.CommitId,
            ServiceBranch = options.CommitBranch,
            ServiceBuildUrl = options.BuildUrl,
            Parallel = options.Parallel,
        });
    }
}
