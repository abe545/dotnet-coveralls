using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dotnet.Coveralls.Data;

namespace Dotnet.Coveralls.Parsers
{
    public class CoverageProvider : ICoverageProvider
    {
        private readonly IEnumerable<ICoverageParser> coverageParsers;

        public CoverageProvider(IEnumerable<ICoverageParser> coverageParsers)
        {
            this.coverageParsers = coverageParsers;
        }

        public async Task<CoverageFile[]> ProvideCoverageFiles() =>
            (await coverageParsers.SelectMany(p => p.ParseSourceFiles())).ToArray();
    }
}
