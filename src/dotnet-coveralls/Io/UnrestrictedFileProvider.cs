using System;
using System.IO;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Internal;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Extensions.Primitives;

namespace Dotnet.Coveralls.Io
{
    public class UnrestrictedFileProvider : IFileProvider
    {
        private readonly string rootPath;

        public UnrestrictedFileProvider(string rootPath)
        {
            if (!Path.IsPathRooted(rootPath))
            {
                throw new ArgumentException("The path must be absolute.", nameof(rootPath));
            }

            this.rootPath = rootPath;
        }

        public IDirectoryContents GetDirectoryContents(string subpath) => new PhysicalDirectoryContents(GetPath(subpath));

        public IFileInfo GetFileInfo(string subpath) => new PhysicalFileInfo(new FileInfo(GetPath(subpath)));

        private string GetPath(string subpath) => Path.IsPathRooted(subpath) ? subpath : Path.Combine(rootPath, subpath);

        IChangeToken IFileProvider.Watch(string filter)
        {
            throw new NotImplementedException();
        }
    }
}
