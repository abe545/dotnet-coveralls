using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dotnet.Coveralls.Data;
using Dotnet.Coveralls.Parsers;

namespace Dotnet.Coveralls.Publishing
{
    public class CoverageProvider : ICoverallsDataProvider
    {
        private readonly IEnumerable<ICoverageParser> coverageParsers;

        public CoverageProvider(IEnumerable<ICoverageParser> coverageParsers)
        {
            this.coverageParsers = coverageParsers;
        }

        public bool CanProvideData => true;

        public async Task<CoverallsData> ProvideCoverallsData() => new CoverallsData
        {
            SourceFiles = (await coverageParsers.SelectMany(p => p.ParseSourceFiles())).ToArray()
        };
    }
}
