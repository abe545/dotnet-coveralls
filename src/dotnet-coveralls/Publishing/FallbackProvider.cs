using System.Threading.Tasks;
using Dotnet.Coveralls.Adapters;
using Dotnet.Coveralls.Data;

namespace Dotnet.Coveralls.Publishing
{
    public class FallbackProvider : ICoverallsDataProvider
    {
        private readonly IDateTimeOffsetProvider dateTimeOffsetProvider;

        public FallbackProvider(IDateTimeOffsetProvider dateTimeOffsetProvider)
        {
            this.dateTimeOffsetProvider = dateTimeOffsetProvider;
        }

        public bool CanProvideData => true;

        public Task<CoverallsData> ProvideCoverallsData() => Task.FromResult(new CoverallsData
        {
            ServiceName = "dotnet-coveralls",
            RunAt = dateTimeOffsetProvider.UtcNow.ToString("o")
        });
    }
}
