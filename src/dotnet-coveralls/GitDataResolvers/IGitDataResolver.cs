using Dotnet.Coveralls.Data;

namespace Dotnet.Coveralls.GitDataResolvers
{
    public interface IGitDataResolver
    {
        bool CanProvideData { get; }
        GitData GitData { get; }
    }
}