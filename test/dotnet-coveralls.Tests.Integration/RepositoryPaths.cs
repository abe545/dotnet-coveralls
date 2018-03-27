using System.IO;

namespace Dotnet.Coveralls.Tests.Integration
{
    public class RepositoryPaths
    {
        public static string GetSamplesPath()
        {
            return Path.Combine("..", "..", "..", "..", "..", "CoverageSamples");
        }
    }
}