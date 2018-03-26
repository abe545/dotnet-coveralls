using System;
using System.IO;
using BCLExtensions;

namespace Dotnet.Coveralls
{
    public class PathProcessor
    {
        private readonly string _basePath;

        public PathProcessor(string basePath)
        {
            _basePath = basePath.IsNotNullOrWhitespace() ? basePath : Directory.GetCurrentDirectory();
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