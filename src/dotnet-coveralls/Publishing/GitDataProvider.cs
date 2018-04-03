using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dotnet.Coveralls.Data;
using Dotnet.Coveralls.Git;

namespace Dotnet.Coveralls.Publishing
{
    public class GitDataProvider : ICoverallsDataProvider
    {
        public GitDataProvider(IEnumerable<IGitDataResolver> gitDataResolvers)
        {
            this.gitDataResolvers = gitDataResolvers;
        }

        public bool CanProvideData => true;

        public Task<CoverallsData> ProvideCoverallsData()
        {
            return Task.FromResult(new CoverallsData
            {
                Git = gitDataResolvers
                    .Where(p => p.CanProvideData)
                    .Select(p => p.GitData)
                    .Aggregate(new GitData(), CombineGitData)
            });

            GitData CombineGitData(GitData accum, GitData toAdd) =>
                new GitData
                {
                    Head = new GitHead
                    {
                        Id = accum.Head?.Id ?? toAdd.Head?.Id,
                        AuthorEmail = accum.Head?.AuthorEmail ?? toAdd.Head?.AuthorEmail,
                        AuthorName = accum.Head?.AuthorName ?? toAdd.Head?.AuthorName,
                        CommitterEmail = accum.Head?.CommitterEmail ?? toAdd.Head?.CommitterEmail,
                        CommitterName = accum.Head?.CommitterName ?? toAdd.Head?.CommitterName,
                        Message = accum.Head?.Message ?? toAdd.Head?.Message
                    },
                    Branch = accum.Branch ?? toAdd.Branch,
                    Remotes = accum.Remotes ?? toAdd.Remotes
                };
        }

        private readonly IEnumerable<IGitDataResolver> gitDataResolvers;
    }
}
