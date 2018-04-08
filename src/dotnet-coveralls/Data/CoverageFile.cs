using System;
using System.Collections.Generic;
using System.Linq;

namespace Dotnet.Coveralls.Data
{
    public sealed class CoverageFile
    {
        public CoverageFile(string name, IEnumerable<string> sourceLines, int?[] coverage)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("name");
            Name = name;
            SourceDigest = Crypto.CalculateMD5Digest(string.Join("\n", sourceLines ?? Enumerable.Empty<string>()));
            Coverage = coverage ?? throw new ArgumentException("coverage");
        }
        
        public string Name { get; }
        public string SourceDigest { get; }
        public int?[] Coverage { get; }
    }
}