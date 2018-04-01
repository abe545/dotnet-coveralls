using System.Linq;
using Dotnet.Coveralls.CommandLine;
using Dotnet.Coveralls.Parsers;
using Machine.Specifications;
using NSubstitute;
using Shouldly;

namespace Dotnet.Coveralls.Tests.Parsers.Chutzpah
{
    [Subject(typeof(ChutzpahJsonParser))]
    public class when_paths_are_not_relative : when_chutzpah_file_exists
    {
        It should_not_throw_exception = () => Exception.ShouldBeNull();
        It should_have_multiple_results = () => ParsedFiles.Count().ShouldBe(2);
        It should_parse_the_first_files_name = () => ParsedFiles.First().Name.ShouldBe("D/path/to/file/file.ts");
        It should_parse_the_second_files_name = () => ParsedFiles.Last().Name.ShouldBe("D/path/to/file/file2.ts");
        It should_parse_the_first_files_coverage = () => ParsedFiles.First().Coverage.ShouldBe(new int?[] { 36, 18, null, null, 9, 10, null, null, null, null, null, null, 18, 18, null });
        It should_parse_the_second_files_coverage = () => ParsedFiles.Last().Coverage.ShouldBe(new int?[] { 36, 18, null, null, 9, 10, null, null, null, null, null, null, 18, 18, null });
    }

    [Subject(typeof(ChutzpahJsonParser))]
    public class when_paths_are_relative : when_chutzpah_file_exists
    {
        Establish cotext = () =>
        {
            var options = DiScope.Container.GetInstance<CoverallsOptions>();
            options.UseRelativePaths = true;
            options.BasePath = @"D:\path\to";
        };

        It should_not_throw_exception = () => Exception.ShouldBeNull();
        It should_have_multiple_results = () => ParsedFiles.Count().ShouldBe(2);
        It should_parse_the_first_files_name = () => ParsedFiles.First().Name.ShouldBe("/file/file.ts");
        It should_parse_the_second_files_name = () => ParsedFiles.Last().Name.ShouldBe("/file/file2.ts");
        It should_parse_the_first_files_coverage = () => ParsedFiles.First().Coverage.ShouldBe(new int?[] { 36, 18, null, null, 9, 10, null, null, null, null, null, null, 18, 18, null });
        It should_parse_the_second_files_coverage = () => ParsedFiles.Last().Coverage.ShouldBe(new int?[] { 36, 18, null, null, 9, 10, null, null, null, null, null, null, 18, 18, null });
    }

    [Subject(typeof(ChutzpahJsonParser))]
    public class when_chutzpah_file_does_not_exist : ChutzpahParserTests
    {
        It should_throw_exception = () => Exception.ShouldNotBeNull();
        It should_have_a_useful_error = () => Exception.Message.ShouldBe($"{CoverageFile} was not found when parsing chutzpah coverage");
    }

    public class when_chutzpah_file_exists : ChutzpahParserTests
    {
        Establish context = () =>
        {
            FileInfo.Exists.Returns(true);
            FileInfo.CreateReadStream().Returns(_ => OpenEmbeddedResource("ChutzpahExample.json"));
        };
    }

    public class ChutzpahParserTests : ParserSpec<ChutzpahJsonParser>
    {
        protected const string CoverageFile = "SomeDirectory/SomeFile.json";

        Establish context = () => Setup(new[] { "--chutzpah", CoverageFile }, CoverageFile);
    }
}