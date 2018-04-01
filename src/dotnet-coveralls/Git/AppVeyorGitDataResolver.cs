using Dotnet.Coveralls.Data;
using Dotnet.Coveralls.Adapters;

namespace Dotnet.Coveralls.Git
{
    public class AppVeyorGitDataResolver : IGitDataResolver
    {
        private readonly IEnvironmentVariables _variables;

        public AppVeyorGitDataResolver(IEnvironmentVariables variables)
        {
            _variables = variables;
        }

        public bool CanProvideData => bool.TryParse(_variables.GetEnvironmentVariable("APPVEYOR"), out var value) && value;

        public GitData GitData =>
            new GitData
            {
                Head = new GitHead
                {
                    Id = _variables.GetEnvironmentVariable("APPVEYOR_REPO_COMMIT"),
                    AuthorName = _variables.GetEnvironmentVariable("APPVEYOR_REPO_COMMIT_AUTHOR"),
                    AuthorEmail = _variables.GetEnvironmentVariable("APPVEYOR_REPO_COMMIT_AUTHOR_EMAIL"),
                    CommitterName = _variables.GetEnvironmentVariable("APPVEYOR_REPO_COMMIT_AUTHOR"),
                    ComitterEmail = _variables.GetEnvironmentVariable("APPVEYOR_REPO_COMMIT_AUTHOR_EMAIL"),
                    Message = _variables.GetEnvironmentVariable("APPVEYOR_REPO_COMMIT_MESSAGE")
                },
                Branch = _variables.GetEnvironmentVariable("APPVEYOR_REPO_BRANCH")
            };
    }
}