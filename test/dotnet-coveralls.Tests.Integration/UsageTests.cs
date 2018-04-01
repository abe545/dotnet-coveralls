using System.Text.RegularExpressions;
using Xunit;

namespace Dotnet.Coveralls.Tests.Integration
{
    public class UsageTests
    {
        [Fact]
        public void NoArguments_ExitCodeNotSuccess()
        {
            var results = CoverallsTestRunner.RunCoveralls(string.Empty);

            Assert.NotEqual(0, results.ExitCode);
        }

        [Fact]
        public void InvalidArgument_ExitCodeNotSuccess()
        {
            var results = CoverallsTestRunner.RunCoveralls("--notanoption");

            Assert.NotEqual(0, results.ExitCode);
        }

        [Fact]
        public void FileDoesntExist()
        {
            var results =
                CoverallsTestRunner.RunCoveralls("--open-cover opencover.xml --dry-run --repo-token MYTESTREPOTOKEN");

            Assert.NotEqual(0, results.ExitCode);
            Assert.Contains("opencover.xml was not found when parsing open cover report", results.StandardError);
        }
    }
}