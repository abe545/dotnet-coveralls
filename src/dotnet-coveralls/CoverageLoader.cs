using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Beefeater;
using Dotnet.Coveralls.Data;
using Dotnet.Coveralls.Io;
using Dotnet.Coveralls.Parsers;
using Dotnet.Coveralls.Ports;
using Microsoft.Extensions.FileProviders;

namespace Dotnet.Coveralls
{
    public class CoverageLoader
    {
        private readonly IEnumerable<ICoverageParser> coverageParsers;

        public CoverageLoader(IEnumerable<ICoverageParser> coverageParsers)
        {
            this.coverageParsers = coverageParsers;
        }

        public async Task<IEnumerable<CoverageFile>> LoadCoverageFiles()
        {
            return await coverageParsers.SelectMany(p => p.ParseSourceFiles());
        }
    }
}