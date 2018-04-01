using System.Threading.Tasks;
using Dotnet.Coveralls.Data;

namespace Dotnet.Coveralls.Publishing
{
    public interface ICoverallsDataBuilder
    {
        Task<CoverallsData> ProvideCoverallsData();
    }
}