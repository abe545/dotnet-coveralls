using System;
using Dotnet.Coveralls.CommandLine;
using Machine.Specifications;
using Shouldly;

namespace Dotnet.Coveralls.Tests.Integration.Usage
{
    [Subject(typeof(CoverallsOptions)), Tags("Integration")]
    public class when_no_arguments_passed : IntegrationTest
    {
        Establish context = () => Arguments = new string[0];

        It should_return_1 = () => ReturnCode.ShouldBe(1);
        It should_print_usage = () => StdErr.ToString().ShouldBe($"{CoverallsOptions.Usage}{Environment.NewLine}");
    }

    [Subject(typeof(CoverallsOptions)), Tags("Integration")]
    public class when_invalid_arguments_passed : IntegrationTest
    {
        Establish context = () => Arguments = new[] { "--not-an-option" };

        It should_return_1 = () => ReturnCode.ShouldBe(1);
        It should_print_usage = () => StdErr.ToString().ShouldBe(
            $"Unknown argument name 'not-an-option'.{Environment.NewLine}{Environment.NewLine}{CoverallsOptions.Usage}{Environment.NewLine}");
    }
}
