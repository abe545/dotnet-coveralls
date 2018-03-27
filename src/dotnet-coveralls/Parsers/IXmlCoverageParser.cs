using System.Collections.Generic;
using System.Xml.Linq;
using Dotnet.Coveralls.Data;

namespace Dotnet.Coveralls.Parsers
{
    internal interface IXmlCoverageParser
    {
        List<FileCoverageData> GenerateSourceFiles(XDocument document);
    }
}