using System;
using System.IO;
using BCLExtensions;
using Dotnet.Coveralls.CommandLine;

namespace Dotnet.Coveralls
{
    public class PathProcessor
    {
        private readonly string _basePath;

        public PathProcessor(CoverallsOptions options)
        {
            _basePath = options.BasePath.IsNotNullOrWhitespace() ? options.BasePath : Directory.GetCurrentDirectory();
        }

        public string ConvertPath(string path)
        {
            if (path.StartsWith(_basePath, StringComparison.InvariantCultureIgnoreCase))
            {
                return path.Substring(_basePath.Length);
            }

            return path;
        }

        public string UnixifyPath(string filePath) => filePath.Replace('\\', '/').Replace(":", "");
    }
}