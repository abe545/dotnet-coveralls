using System;
using System.Collections.Generic;
using System.Linq;
using Dotnet.Coveralls.Data;

namespace Dotnet.Coveralls
{
    public class FileCoverageDataBuilder
    {
        private readonly Dictionary<int, int> coverage = new Dictionary<int, int>();
        private readonly string filePath;

        public FileCoverageDataBuilder(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentException("filePath");
            this.filePath = filePath;
        }

        public void RecordCoverage(int lineNumber, int coverageNumber) => coverage[lineNumber - 1] = coverageNumber;

        public FileCoverageData CreateFile()
        {
            var length = coverage.Any() ? coverage.Keys.Max() + 1 : 1;
            var results = Enumerable
                .Range(0, length)
                .Select(i => coverage.TryGetValue(i, out var value) ? (int?)value : null)
                .ToArray();
            return new FileCoverageData(filePath, results);
        }
    }
}