using Dotnet.Coveralls.Data;

namespace Dotnet.Coveralls.Git
{
    public interface IGitDataProvider
    {
        GitData ProvideGitData();
    }
}