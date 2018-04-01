using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dotnet.Coveralls.Data
{
    public interface ICoverageFileBuilder
    {
        Task<IEnumerable<CoverageFile>> Build(IEnumerable<FileCoverageData> coverageData);
    }
}