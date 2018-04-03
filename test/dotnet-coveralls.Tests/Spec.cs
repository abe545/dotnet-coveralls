using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Dotnet.Coveralls.Data;
using Dotnet.Coveralls.Parsers;
using Machine.Specifications;
using Microsoft.Extensions.FileProviders;
using NSubstitute;
using SimpleInjector;

namespace Dotnet.Coveralls.Tests
{
    public class Spec
    {
        protected static Scope DiScope;

        protected static (IFileProvider, IFileInfo) CreateMockFileProvider(string fileName)
        {
            var fileInfo = Substitute.For<IFileInfo>();

            var fileProvider = Substitute.For<IFileProvider>();
            fileProvider.GetFileInfo(fileName).Returns(fileInfo);

            return (fileProvider, fileInfo);
        }

        protected static Stream OpenEmbeddedResource(string embeddedResource) =>
            Assembly.GetExecutingAssembly().GetManifestResourceStream($"Dotnet.Coveralls.Tests.{embeddedResource}");

        protected static void Setup(string[] args, Action<Container> containerSetup = null)
        {
            DiScope = Di.Setup(args);
            DiScope.Container.Options.AllowOverridingRegistrations = true;
            containerSetup?.Invoke(DiScope.Container);
        }
    }

    public class ParserSpec<T> : Spec where T : class, ICoverageParser
    {
        protected static T Subject;
        protected static IEnumerable<CoverageFile> ParsedFiles;
        protected static IFileInfo FileInfo;
        protected static Exception Exception;

        protected static void Setup(string[] args, string coverageFileName, Action<Container> containerSetup = null)
        {
            var (fileProvider, fileInfo) = CreateMockFileProvider(coverageFileName);
            FileInfo = fileInfo;

            Setup(args, c =>
            {
                c.Register(() => fileProvider);
                containerSetup?.Invoke(c);
            });
            Subject = DiScope.Container.GetInstance<T>();
        }

        Because of = () =>
        {
            try
            {
                ParsedFiles = Subject.ParseSourceFiles().Result;
            }
            catch (AggregateException ae)
            {
                Exception = ae.Flatten().InnerException;
            }
            catch (Exception ex)
            {
                Exception = ex;
            }
        };

        Cleanup after = () => DiScope.Container.Dispose();
    }
}
