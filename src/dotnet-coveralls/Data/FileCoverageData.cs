using System;

namespace Dotnet.Coveralls.Data
{
    public class FileCoverageData
    {
        public FileCoverageData(string fullPath, int?[] coverage)
        {
            if (string.IsNullOrEmpty(fullPath)) throw new ArgumentException("fullPath");
            FullPath = fullPath;
            Coverage = coverage ?? throw new ArgumentException("coverage");
        }

        public string FullPath { get; }

        public int?[] Coverage { get; }
    }
}