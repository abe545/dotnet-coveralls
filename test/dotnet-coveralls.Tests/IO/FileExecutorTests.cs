using System.Diagnostics;
using Dotnet.Coveralls.Io;
using Machine.Specifications;
using Shouldly;

namespace Dotnet.Coveralls.Tests.IO.FileExecutorTests
{
    [Subject(typeof(FileExecutor))]
    public class when_the_process_exits_successfully : with_file_executor
    {
        Establish context = () => ProcessStartInfo = new ProcessStartInfo("dotnet", "--version");

        It should_return_0 = () => ExitCode.ShouldBe(0);
        It should_not_have_stderr = () => StandardError.ShouldBeNull();
        It should_return_stdout = () => StandardOut.ShouldNotBeNullOrWhiteSpace();
    }

    [Subject(typeof(FileExecutor))]
    public class when_the_process_fails : with_file_executor
    {
        Establish context = () => ProcessStartInfo = new ProcessStartInfo("dotnet", "--invalid-argument");

        It should_return_1 = () => ExitCode.ShouldBe(1);
        It should_return_stderr = () => StandardError.ShouldNotBeNullOrWhiteSpace();
    }

    public class with_file_executor
    {
        Establish context = () => Subject = new FileExecutor();

        Because of = () => (StandardOut, StandardError, ExitCode) = Subject.Execute(ProcessStartInfo).Result;

        protected static FileExecutor Subject;
        protected static ProcessStartInfo ProcessStartInfo;
        protected static int ExitCode;
        protected static string StandardError;
        protected static string StandardOut;
    }
}
