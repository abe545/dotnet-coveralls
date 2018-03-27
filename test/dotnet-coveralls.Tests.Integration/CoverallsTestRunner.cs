using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Xunit.Sdk;

namespace Dotnet.Coveralls.Tests.Integration
{
    public static class CoverallsTestRunner
    {
        private const string CoverageDll = "dotnet-coveralls.dll";

        public static CoverageRunResults RunCoveralls(string arguments)
        {
            int exitCode;
            string results, errorsResults;
            var dllPath = Path.Combine(GetCoverallsExePath(), CoverageDll);

            using (var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = "dotnet",
                    Arguments = $"{dllPath} {arguments}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8
                }
            })
            {
                process.Start();

                results = process.StandardOutput.ReadToEnd();
                errorsResults = process.StandardError.ReadToEnd();
                Console.WriteLine(results);

                const int timeoutInMilliseconds = 10000;
                if (!process.WaitForExit(timeoutInMilliseconds))
                {
                    throw new XunitException($"Test execution time exceeded: {timeoutInMilliseconds}ms");
                }
                exitCode = process.ExitCode;
            }

            return new CoverageRunResults
            {
                StandardOutput = results,
                StandardError = errorsResults,
                ExitCode = exitCode
            };
        }

        private static string GetCoverallsExePath()
        {
#if DEBUG
            var configuration = "Debug";
#else
            var configuration = "Release";
#endif
            return Path.Combine(
                Path.GetDirectoryName(typeof(CoverallsTestRunner).Assembly.Location),
                "..", "..", "..", "..", "..", "src", "dotnet-coveralls", "bin", configuration, "netcoreapp2.0");
        }
    }
}