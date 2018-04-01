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
                .Aggregate(new CoverallsData(), CombineData);

            CoverallsData CombineData(CoverallsData accum, CoverallsData toAdd) =>
                new CoverallsData
                {
                    Git = accum.Git ?? toAdd.Git,
                    Parallel = accum.Parallel ?? toAdd.Parallel,
                    PullRequestId = accum.PullRequestId ?? toAdd.PullRequestId,
                    RepoToken = accum.RepoToken ?? toAdd.RepoToken,
                    ServiceJobId = accum.ServiceJobId ?? toAdd.ServiceJobId,
                    ServiceName = accum.ServiceName ?? toAdd.ServiceName,
                    ServiceNumber = accum.ServiceNumber ?? toAdd.ServiceNumber,
                    SourceFiles = accum.SourceFiles ?? toAdd.SourceFiles
                };
        }
    }
}
