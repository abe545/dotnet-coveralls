using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Dotnet.Coveralls.Parsers;
using Machine.Specifications;
using Shouldly;

namespace Dotnet.Coveralls.Tests.Integration.OpenCover
{
    [Subject(typeof(OpenCoverParser)), Tags("Integration")]
    public class when_file_does_not_exist : IntegrationTest
    {
        Establish context = () => AddArguments("--open-cover", "opencover.xml");

        It should_write_error_to_stderr = () => StdErr.ToString().ShouldBe($"opencover.xml was not found when parsing open cover report{Environment.NewLine}");
        It should_return_1 = () => ReturnCode.ShouldBe(1);
    }

    [Subject(typeof(OpenCoverParser)), Tags("Integration")]
    public class when_report_is_empty : IntegrationTest
    {
        Establish context = () => AddArguments(
            "--open-cover",
            Path.Combine(SamplesPath, "opencover", "Sample1", "EmptyReport.xml")
        );

        It should_not_write_to_stderr = () => StdErr.ToString().ShouldBe("");
        It should_return_0 = () => ReturnCode.ShouldBe(0);
    }

    [Subject(typeof(OpenCoverParser)), Tags("Integration")]
    public class when_report_is_not_empty : IntegrationTest
    {
        Establish context = () => AddArguments(
            "--open-cover",
            Path.Combine(SamplesPath, "opencover", "Sample2", "SingleFileReport.xml")
        );

        It should_not_write_to_stderr = () => StdErr.ToString().ShouldBe("");
        It should_return_0 = () => ReturnCode.ShouldBe(0);
    }

    [Subject(typeof(OpenCoverParser)), Tags("Integration")]
    public class when_multiple_reports_are_passed_separately : IntegrationTest
    {
        Establish context = () => AddArguments(
            "--open-cover",
            Path.Combine(SamplesPath, "opencover", "Sample1", "EmptyReport.xml"),
            "--open-cover",
            Path.Combine(SamplesPath, "opencover", "Sample2", "SingleFileReport.xml")
        );

        It should_not_write_to_stderr = () => StdErr.ToString().ShouldBe("");
        It should_return_0 = () => ReturnCode.ShouldBe(0);
    }

    [Subject(typeof(OpenCoverParser)), Tags("Integration")]
    public class when_multiple_reports_are_passed_together : IntegrationTest
    {
        Establish context = () =>
        {
            var filePath1 = Path.Combine(SamplesPath, "opencover", "Sample1", "EmptyReport.xml");
            var filePath2 = Path.Combine(SamplesPath, "opencover", "Sample2", "SingleFileReport.xml");
            AddArguments("--open-cover", $"{filePath1} {filePath2}");
        };

        It should_not_write_to_stderr = () => StdErr.ToString().ShouldBe("");
        It should_return_0 = () => ReturnCode.ShouldBe(0);
    }
}
