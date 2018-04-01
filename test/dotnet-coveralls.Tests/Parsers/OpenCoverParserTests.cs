using System;
using Dotnet.Coveralls.Parsers;
using Machine.Specifications;
using NSubstitute;
using Shouldly;

namespace Dotnet.Coveralls.Tests.Parsers.OpenCover
{
    [Subject(typeof(OpenCoverParser))]
    public class when_report_is_empty : when_file_exists
    {
        Establish context = () => FileInfo.CreateReadStream().Returns(_ => OpenEmbeddedResource("EmptyReport.xml"));

        It should_return_no_reports = () => ParsedFiles.ShouldBeEmpty();
    }

    [Subject(typeof(OpenCoverParser))]
    public class when_report_is_for_a_single_file : when_file_exists
    {
        Establish context = () => FileInfo.CreateReadStream().Returns(_ => OpenEmbeddedResource("SingleFileReport.xml"));

        It should_return_one_report = () => ParsedFiles.ShouldHaveSingleItem();
        It should_have_the_filename = () => ParsedFiles.ShouldHaveSingleItem().Name.ShouldEndWith("Class1.cs");
        It should_have_the_coverage_info = () => ParsedFiles.ShouldHaveSingleItem().Coverage.ShouldBe(new int?[] { null, null, null, null, 1 });
    }

    [Subject(typeof(OpenCoverParser))]
    public class when_report_is_for_a_single_file_with_one_line_covered : when_file_exists
    {
        Establish context = () => FileInfo.CreateReadStream().Returns(_ => OpenEmbeddedResource("SingleFileReportOneLineCovered.xml"));

        It should_return_one_report = () => ParsedFiles.ShouldHaveSingleItem();
        It should_have_the_filename = () => ParsedFiles.ShouldHaveSingleItem().Name.ShouldEndWith("Class1.cs");
        It should_have_the_coverage_info = () => ParsedFiles.ShouldHaveSingleItem().Coverage.ShouldBe(new int?[] { null, null, null, null, null, null, null, 1, 1, 1 });
    }

    [Subject(typeof(OpenCoverParser))]
    public class when_report_is_for_a_single_file_with_no_lines_covered : when_file_exists
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

    public class when_file_exists : OpenCoverParserTests
    {
        Establish context = () => FileInfo.Exists.Returns(true);
    }

    public class OpenCoverParserTests : ParserSpec<OpenCoverParser>
    {
        protected const string CoverageFile = "SomeDirectory/SomeFile.xml";

        Establish context = () => Setup(new[] { "--open-cover", CoverageFile }, CoverageFile);
    }
}