using System.Threading.Tasks;
using Dotnet.Coveralls.CommandLine;
using Dotnet.Coveralls.Data;
using Dotnet.Coveralls.Git;

namespace Dotnet.Coveralls.Publishing
{
    public class CommandLineProvider : ICoverallsDataProvider, IGitDataResolver
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
            Parallel = options.Parallel ? (bool?)true : null,
        });

        public Task<GitData> CreateGitData() => Task.FromResult(
            new GitData
            {
                Head = new GitHead
                {
                    Id = options.CommitId,
                    AuthorName = options.CommitAuthor,
                    AuthorEmail = options.CommitEmail,
                    CommitterName = options.CommitAuthor,
                    CommitterEmail = options.CommitEmail,
                    Message = options.CommitMessage
                },
                Branch = options.CommitBranch,
            });
    }
}
