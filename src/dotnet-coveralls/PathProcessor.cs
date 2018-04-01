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

        public string ConvertPath(string path)
        {
            if (options.UseRelativePaths && path.StartsWith(BasePath, StringComparison.InvariantCultureIgnoreCase))
            {
                return path.Substring(BasePath.Length);
            }

            return path;
        }

        public string UnixifyPath(string filePath) => filePath.Replace('\\', '/').Replace(":", "");
    }
}