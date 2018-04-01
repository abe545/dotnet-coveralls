using System.Threading.Tasks;
using Dotnet.Coveralls.Data;

namespace Dotnet.Coveralls.Publishing
{
    public class FallbackProvider : ICoverallsDataProvider
    {
        public bool CanProvideData => true;

        public Task<CoverallsData> ProvideCoverallsData() => Task.FromResult(new CoverallsData
        {
            ServiceName = "dotnet-coveralls"
        });
    }
}
