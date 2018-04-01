using System.Collections.Generic;
using System.Threading.Tasks;
using Dotnet.Coveralls.Data;

namespace Dotnet.Coveralls.Parsers
{
    public interface ICoverageParser
    {
        Task<IEnumerable<CoverageFile>> ParseSourceFiles();
    }
}
