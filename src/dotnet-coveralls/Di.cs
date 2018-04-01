using System;
using Dotnet.Coveralls.Adapters;
using Dotnet.Coveralls.CommandLine;
using Dotnet.Coveralls.Data;
using Dotnet.Coveralls.Git;
using Dotnet.Coveralls.Io;
using Dotnet.Coveralls.Parsers;
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
            var options = CoverallsOptions.Parse(args);            

            var container = new Container();
            container.Options.DefaultLifestyle = Lifestyle.Scoped;
            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            container.Register(() => options);
            container.Register(() => new LoggerFactory().AddConsole());

            container.Register<IFileWriter, FileWriter>();
            container.Register<IFileProvider>(() => new UnrestrictedFileProvider(Environment.CurrentDirectory));
            container.Register<IOutputFileWriter, OutputFileWriter>();
            container.Register<IEnvironmentVariables, EnvironmentVariables>();

            container.Register<CoverallsPublisher>();
            container.Register<ICoverageFileBuilder, CoverageFileBuilder>();

            container.RegisterCollection<ICoverageParser>(new[] { typeof(Di).Assembly });
            container.Register<ICoverageProvider, CoverageProvider>();

            container.RegisterCollection<IGitDataResolver>(new[] { typeof(Di).Assembly });
            container.Register<IGitDataProvider, GitDataProvider>();

            return AsyncScopedLifestyle.BeginScope(container);
        }
    }
}
