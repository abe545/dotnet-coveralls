using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Dotnet.Coveralls.Io
{
    public class FileWriter : IFileWriter
    {
        public async Task WriteAllText(string path, string text) => await File.WriteAllTextAsync(path, text);
    }
}
