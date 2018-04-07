using System.Threading.Tasks;
using Dotnet.Coveralls.CommandLine;
using Dotnet.Coveralls.Data;

namespace Dotnet.Coveralls.Git
{
    public class CommandLineGitDataResolver : IGitDataResolver
    {
        private readonly CoverallsOptions coverallsOptions;

        public CommandLineGitDataResolver(CoverallsOptions coverallsOptions)
        {
            this.coverallsOptions = coverallsOptions;
        }

        public bool CanProvideData => !string.IsNullOrWhiteSpace(coverallsOptions.CommitId);

        public GitData GitData =>
            new GitData
            {
                Head = new GitHead
                {
                    Id = coverallsOptions.CommitId,
                    AuthorName = coverallsOptions.CommitAuthor,
                    AuthorEmail = coverallsOptions.CommitEmail,
                    CommitterName = coverallsOptions.CommitAuthor,
                    CommitterEmail = coverallsOptions.CommitEmail,
                    Message = coverallsOptions.CommitMessage
                },
                Branch = coverallsOptions.CommitBranch,
            };

        public Task<GitData> CreateGitData() => Task.FromResult(GitData);
    }
}