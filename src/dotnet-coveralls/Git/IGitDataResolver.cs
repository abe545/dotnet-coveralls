using Dotnet.Coveralls.Data;

namespace Dotnet.Coveralls.Git
{
    public interface IGitDataResolver
    {
        bool CanProvideData { get; }
        GitData GitData { get; }
    }
}