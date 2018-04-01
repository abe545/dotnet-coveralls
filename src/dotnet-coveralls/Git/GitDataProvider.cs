using System.Collections.Generic;
using System.Linq;
using Dotnet.Coveralls.Data;

namespace Dotnet.Coveralls.Git
{
    public class GitDataProvider : IGitDataProvider
    {
        public GitDataProvider(IEnumerable<IGitDataResolver> gitDataResolvers)
        {
            this.gitDataResolvers = gitDataResolvers;
        }

        public GitData ProvideGitData() =>
            gitDataResolvers
                .Where(p => p.CanProvideData)
                .Select(p => p.GitData)
                .Aggregate(new GitData(), CombineGitData);

        private GitData CombineGitData(GitData accum, GitData toAdd) =>
            new GitData
            {
                Branch = accum.Branch ?? toAdd.Branch,
                Head = new GitHead
                {
                    AuthorEmail = accum.Head.AuthorEmail ?? toAdd.Head.AuthorEmail,
                    AuthorName = accum.Head.AuthorName ?? toAdd.Head.AuthorName,
                    ComitterEmail = accum.Head.ComitterEmail ?? toAdd.Head.ComitterEmail,
                    CommitterName = accum.Head.CommitterName ?? toAdd.Head.CommitterName,
                    Id = accum.Head.Id ?? toAdd.Head.Id,
                    Message = accum.Head.Message ?? toAdd.Head.Message
                },
                Remotes = new GitRemotes
                {
                    Name = accum.Remotes.Name ?? toAdd.Remotes.Name,
                    Url = accum.Remotes.Url ?? toAdd.Remotes.Url
                }
            };

        private readonly IEnumerable<IGitDataResolver> gitDataResolvers;
    }
}
