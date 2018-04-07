using System.Threading.Tasks;
using Dotnet.Coveralls.Data;

namespace Dotnet.Coveralls.Git
{
    public interface IGitDataResolver
    {
        bool CanProvideData { get; }
        Task<GitData> CreateGitData();
    }
}