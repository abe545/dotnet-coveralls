using System.Threading.Tasks;
using Dotnet.Coveralls.Adapters;
using Dotnet.Coveralls.Data;

namespace Dotnet.Coveralls.Git
{
    public class GitEnvironmentVariableGitDataResolver : IGitDataResolver
    {
        private readonly IEnvironmentVariables environmentVariables;

        public static class Git
        {
            public const string ID = "GIT_ID";
            public const string AUTHOR_EMAIL = "GIT_AUTHOR_EMAIL";
            public const string AUTHOR_NAME = "GIT_AUTHOR_NAME";
            public const string COMMITTER_EMAIL = "GIT_COMMITTER_EMAIL";
            public const string COMMITTER_NAME = "GIT_COMMITTER_NAME";
            public const string MESSAGE = "GIT_MESSAGE";
            public const string BRANCH = "GIT_BRANCH";
        }

        public GitEnvironmentVariableGitDataResolver(IEnvironmentVariables environmentVariables)
        {
            this.environmentVariables = environmentVariables;
        }

        public bool CanProvideData => true;

        public GitData GitData => new GitData
        {
            Head = new GitHead
            {
                Id = environmentVariables.GetEnvironmentVariable(Git.ID),
                AuthorEmail = environmentVariables.GetEnvironmentVariable(Git.AUTHOR_EMAIL),
                AuthorName = environmentVariables.GetEnvironmentVariable(Git.AUTHOR_NAME),
                CommitterEmail = environmentVariables.GetEnvironmentVariable(Git.COMMITTER_EMAIL),
                CommitterName = environmentVariables.GetEnvironmentVariable(Git.COMMITTER_NAME),
                Message = environmentVariables.GetEnvironmentVariable(Git.MESSAGE)
            },
            Branch = environmentVariables.GetEnvironmentVariable(Git.BRANCH)
        };

        public Task<GitData> CreateGitData() => Task.FromResult(GitData);
    }
}
