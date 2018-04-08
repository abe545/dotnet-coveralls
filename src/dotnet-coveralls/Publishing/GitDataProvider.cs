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

        public async Task<CoverallsData> ProvideCoverallsData()
        {
            return new CoverallsData
            {
                Git = (await gitDataResolvers
                    .Where(p => p.CanProvideData)
                    .Select(p => p.CreateGitData()))
                    .Where(g => g != null)
                    .Aggregate(new GitData(), CombineGitData)
            };

            GitData CombineGitData(GitData accum, GitData toAdd) =>
                new GitData
                {
                    Head = new GitHead
                    {
                        Id = accum.Head?.Id ?? toAdd.Head?.Id?.NullIfEmpty(),
                        AuthorEmail = accum.Head?.AuthorEmail ?? toAdd.Head?.AuthorEmail?.NullIfEmpty(),
                        AuthorName = accum.Head?.AuthorName ?? toAdd.Head?.AuthorName?.NullIfEmpty(),
                        CommitterEmail = accum.Head?.CommitterEmail ?? toAdd.Head?.CommitterEmail?.NullIfEmpty(),
                        CommitterName = accum.Head?.CommitterName ?? toAdd.Head?.CommitterName?.NullIfEmpty(),
                        Message = accum.Head?.Message ?? toAdd.Head?.Message?.NullIfEmpty()
                    },
                    Branch = accum.Branch ?? toAdd.Branch.NullIfEmpty(),
                    Remotes = accum.Remotes ?? toAdd.Remotes
                };
        }

        private readonly IEnumerable<IGitDataResolver> gitDataResolvers;
    }
}
