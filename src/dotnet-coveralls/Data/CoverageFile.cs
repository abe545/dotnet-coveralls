using System;

namespace Dotnet.Coveralls.Data
{
    public sealed class CoverageFile
    {
        public CoverageFile(string name, string sourceDigest, int?[] coverage)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("name");
            Name = name;
            SourceDigest = sourceDigest ?? throw new ArgumentException("sourceDigest");
            Coverage = coverage ?? throw new ArgumentException("coverage");
        }
        
        public string Name { get; }
        public string SourceDigest { get; }
        public int?[] Coverage { get; }
    }
}