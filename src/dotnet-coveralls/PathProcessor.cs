using System;
using System.IO;
using Dotnet.Coveralls.CommandLine;

namespace Dotnet.Coveralls
{
    public class PathProcessor
    {
        private CoverallsOptions options;
        private string BasePath => string.IsNullOrWhiteSpace(options.BasePath) ? Directory.GetCurrentDirectory() : options.BasePath;

        public PathProcessor(CoverallsOptions options)
        {
            this.options = options;
        }

        public string NormalizePath(string path)
        {
            if (options.UseRelativePaths && path.StartsWith(BasePath, StringComparison.InvariantCultureIgnoreCase))
            {
                path = path.Substring(BasePath.Length);
            }

            return path.Replace('\\', '/').Replace(":", "");
        }
    }
}