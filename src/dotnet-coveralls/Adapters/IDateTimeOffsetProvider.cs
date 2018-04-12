using System;

namespace Dotnet.Coveralls.Adapters
{
    public interface IDateTimeOffsetProvider
    {
        DateTimeOffset UtcNow { get; }
    }
}
