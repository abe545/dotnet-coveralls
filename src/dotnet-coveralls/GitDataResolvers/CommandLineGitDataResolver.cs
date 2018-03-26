using BCLExtensions;
using Dotnet.Coveralls.CommandLine;
using Dotnet.Coveralls.Data;

namespace Dotnet.Coveralls.GitDataResolvers
{
    public class CommandLineGitDataResolver : IGitDataResolver
    {
        private readonly CoverallsOptions coverallsOptions;

        public CommandLineGitDataResolver(CoverallsOptions coverallsOptions)
        {
            this.coverallsOptions = coverallsOptions;
        }

        public bool CanProvideData => coverallsOptions.CommitId.IsNotNullOrWhitespace();

        public GitData GitData =>
            new GitData
            {
                Head = new GitHead
                {
                    Id = coverallsOptions.CommitId,
                    AuthorName = coverallsOptions.CommitAuthor,
                    AuthorEmail = coverallsOptions.CommitEmail,
                    CommitterName = coverallsOptions.CommitAuthor,
                    ComitterEmail = coverallsOptions.CommitEmail,
                    Message = coverallsOptions.CommitMessage
                },
                Branch = coverallsOptions.CommitBranch,
            };
    }
}