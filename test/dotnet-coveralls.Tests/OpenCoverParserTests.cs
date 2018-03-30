using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Dotnet.Coveralls.Data;
using Dotnet.Coveralls.Parsers;
using Machine.Specifications;
using Microsoft.Extensions.FileProviders;
using NSubstitute;
using Shouldly;
using SimpleInjector;

namespace Dotnet.Coveralls.Tests
{
    [Subject(typeof(OpenCoverParser))]
    public class when_report_is_empty : WhenReportExists
    {
        Establish context = () => FileInfo.CreateReadStream().Returns(_ => OpenEmbeddedResource("EmptyReport.xml"));

        It should_return_no_reports = () => ParsedFiles.ShouldBeEmpty();
    }

    [Subject(typeof(OpenCoverParser))]
    public class when_report_is_for_a_single_file : WhenReportExists
    {
        Establish context = () => FileInfo.CreateReadStream().Returns(_ => OpenEmbeddedResource("SingleFileReport.xml"));

        It should_return_one_report = () => ParsedFiles.ShouldHaveSingleItem();
        It should_have_the_filename = () => ParsedFiles.ShouldHaveSingleItem().Name.ShouldEndWith("Class1.cs");
        It should_have_the_coverage_info = () => ParsedFiles.ShouldHaveSingleItem().Coverage.ShouldBe(new int?[] { null, null, null, null, 1 });
    }

    [Subject(typeof(OpenCoverParser))]
    public class when_report_is_for_a_single_file_with_one_line_covered : WhenReportExists
    {
        Establish context = () => FileInfo.CreateReadStream().Returns(_ => OpenEmbeddedResource("SingleFileReportOneLineCovered.xml"));

        It should_return_one_report = () => ParsedFiles.ShouldHaveSingleItem();
        It should_have_the_filename = () => ParsedFiles.ShouldHaveSingleItem().Name.ShouldEndWith("Class1.cs");
        It should_have_the_coverage_info = () => ParsedFiles.ShouldHaveSingleItem().Coverage.ShouldBe(new int?[] { null, null, null, null, null, null, null, 1, 1, 1 });
    }

    [Subject(typeof(OpenCoverParser))]
    public class when_report_is_for_a_single_file_with_no_lines_covered : WhenReportExists
    {
        Establish context = () => FileInfo.CreateReadStream().Returns(_ => OpenEmbeddedResource("SingleFileReportOneLineUncovered.xml"));

        It should_return_one_report = () => ParsedFiles.ShouldHaveSingleItem();
        It should_have_the_filename = () => ParsedFiles.ShouldHaveSingleItem().Name.ShouldEndWith("Class1.cs");
        It should_have_the_coverage_info = () => ParsedFiles.ShouldHaveSingleItem().Coverage.ShouldBe(new int?[] { null, null, null, null, null, null, null, 0, 0, 0 });
    }

    [Subject(typeof(OpenCoverParser))]
    public class when_file_does_not_exist : OpenCoverParserTests
    {
        It should_throw_exception = () => Exception.ShouldNotBeNull();
        It should_have_a_useful_error = () => Exception.Message.ShouldBe($"{CoverageFile} was not found when parsing open cover report");
    }

    public class WhenReportExists : OpenCoverParserTests
    {
        Establish context = () => FileInfo.Exists.Returns(true);
    }

    public class OpenCoverParserTests
    {
        protected const string CoverageFile = "SomeDirectory/SomeFile.xml";
        protected static IFileInfo FileInfo;
        protected static Scope DiScope;
        protected static IEnumerable<CoverageFile> ParsedFiles;
        protected static Exception Exception;
        protected static OpenCoverParser Subject;

        Establish context = () =>
        {
            FileInfo = Substitute.For<IFileInfo>();
            DiScope = Di.Setup(new[] { "--open-cover", CoverageFile });

            var fileProvider = Substitute.For<IFileProvider>();
            fileProvider.GetFileInfo(CoverageFile).Returns(FileInfo);

            DiScope.Container.Options.AllowOverridingRegistrations = true;
            DiScope.Container.Register(() => fileProvider);

            Subject = DiScope.Container.GetInstance<OpenCoverParser>();
        };

        Cleanup after = () => DiScope.Container.Dispose();

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

        protected static Stream OpenEmbeddedResource(string embeddedResource) =>
            Assembly.GetExecutingAssembly().GetManifestResourceStream($"Dotnet.Coveralls.Tests.{embeddedResource}");
    }
}