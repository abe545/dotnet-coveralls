using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;

namespace Dotnet.Coveralls.Tests.Integration
{
    public abstract class IntegrationTest
    {
        public static string SamplesPath => Path.Combine("..", "..", "..", "..", "..", "CoverageSamples");

        Establish context = () =>
        {
            Arguments = new[] { "--dry-run" };

            StdErr = new StringBuilder();
            Console.SetError(new StringWriter(StdErr));

            StdOut = new StringBuilder();
            Console.SetOut(new StringWriter(StdOut));
        };

        Because of = () => ReturnCode = Task.Run(() => Program.Main(Arguments)).Result;

        protected static StringBuilder StdErr;
        protected static StringBuilder StdOut;

        protected static int ReturnCode;
        protected static string[] Arguments;

        protected static void AddArguments(params string[] arguments) => Arguments = arguments.Concat(Arguments ?? Enumerable.Empty<string>()).ToArray();
    }
}
