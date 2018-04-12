using System;

namespace Dotnet.Coveralls.Adapters
{
    public class DateTimeOffsetProvider : IDateTimeOffsetProvider
    {
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    }
}
