using System.Threading.Tasks;
using Dotnet.Coveralls.Data;

namespace Dotnet.Coveralls.Publishing
{
    public interface ICoverallsDataProvider
    {
        bool CanProvideData { get; }
        Task<CoverallsData> ProvideCoverallsData();
    }
}
