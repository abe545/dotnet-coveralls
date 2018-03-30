using System;
using System.Collections.Generic;
using System.Text;
using clipr;
using Dotnet.Coveralls.CommandLine;
using Dotnet.Coveralls.Data;
using Dotnet.Coveralls.Io;
using Dotnet.Coveralls.Parsers;
using Dotnet.Coveralls.Publishers;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using SimpleInjector;
using SimpleInjector.Lifestyles;

namespace Dotnet.Coveralls
{
    public static class Di
    {
        public static Scope Setup(string[] args)
        {
            var container = new Container();
            container.Options.DefaultLifestyle = Lifestyle.Scoped;
            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            container.Register(() => CliParser.Parse<CoverallsOptions>(args));
            container.Register<IFileWriter, FileWriter>();
            container.Register<IFileProvider>(() => new UnrestrictedFileProvider(Environment.CurrentDirectory));
            container.Register<CoverallsPublisher>();
            container.Register(() => new LoggerFactory().AddConsole());
            container.Register<ICoverageFileBuilder, CoverageFileBuilder>();
            container.RegisterCollection<ICoverageParser>(new[] { typeof(Di).Assembly });
                        
            return AsyncScopedLifestyle.BeginScope(container);
        }
    }
}
