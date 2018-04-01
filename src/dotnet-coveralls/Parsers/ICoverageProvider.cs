using System.Threading.Tasks;
using Dotnet.Coveralls.Data;

namespace Dotnet.Coveralls.Parsers
{
    public interface ICoverageProvider
    {
        Task<CoverageFile[]> ProvideCoverageFiles();
    }
}