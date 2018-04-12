using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dotnet.Coveralls.Data;

namespace Dotnet.Coveralls.Publishing
{
    public class CoverallsDataBuilder : ICoverallsDataBuilder
    {
        private readonly IEnumerable<ICoverallsDataProvider> coverallsDataProviders;

        public CoverallsDataBuilder(IEnumerable<ICoverallsDataProvider> coverallsDataProviders)
        {
            this.coverallsDataProviders = coverallsDataProviders;
        }

        public async Task<CoverallsData> ProvideCoverallsData()
        {
            return (await coverallsDataProviders
                .Where(p => p.CanProvideData)
                .Select(p => p.ProvideCoverallsData()))
                .Where(c => c != null)
                .Aggregate(new CoverallsData(), CombineData);

            CoverallsData CombineData(CoverallsData accum, CoverallsData toAdd) =>
                new CoverallsData
                {
                    CommitSha = accum.CommitSha ?? toAdd.CommitSha.NullIfEmpty(),
                    Git = accum.Git ?? toAdd.Git,
                    SourceFiles = accum.SourceFiles ?? toAdd.SourceFiles,
                    Parallel = accum.Parallel ?? toAdd.Parallel,
                    ServicePullRequest = accum.ServicePullRequest.NullIfEmpty() ?? toAdd.ServicePullRequest.NullIfEmpty(),
                    RepoToken = accum.RepoToken.NullIfEmpty() ?? toAdd.RepoToken.NullIfEmpty(),
                    ServiceBranch = accum.ServiceBranch.NullIfEmpty() ?? toAdd.ServiceBranch.NullIfEmpty(),
                    ServiceJobId = accum.ServiceJobId.NullIfEmpty() ?? toAdd.ServiceJobId.NullIfEmpty(),
                    ServiceName = accum.ServiceName.NullIfEmpty() ?? toAdd.ServiceName.NullIfEmpty(),
                    ServiceNumber = accum.ServiceNumber.NullIfEmpty() ?? toAdd.ServiceNumber.NullIfEmpty(),
                    ServiceBuildUrl = accum.ServiceBuildUrl.NullIfEmpty() ?? toAdd.ServiceBuildUrl.NullIfEmpty(),
                    RunAt = accum.RunAt.NullIfEmpty() ?? toAdd.RunAt.NullIfEmpty(),
                };
        }
    }
}
