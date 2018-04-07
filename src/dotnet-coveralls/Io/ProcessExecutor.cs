using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Dotnet.Coveralls.Io
{
    public class ProcessExecutor : IProcessExecutor
    {
        private readonly ILogger<ProcessExecutor> Logger;

        public ProcessExecutor(ILoggerFactory loggerFactory)
        {
            Logger = loggerFactory.CreateLogger<ProcessExecutor>();
        }

        public async Task<(string StandardOut, string StandardErr, int ReturnCode)> Execute(ProcessStartInfo processStartInfo)
        {
            processStartInfo.RedirectStandardError = true;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.UseShellExecute = false;

            var stdOut = new StringBuilder();
            var stdErr = new StringBuilder();

            using (var process = Process.Start(processStartInfo))
            {
                process.OutputDataReceived += (_, e) =>
                {
                    stdOut.AppendLine(e.Data);
                    Logger.LogDebug(e.Data);
                };

                process.ErrorDataReceived += (_, e) =>
                {
                    stdErr.AppendLine(e.Data);
                    Logger.LogDebug(e.Data);
                };

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                await Task.Run(() => process.WaitForExit());

                return (stdOut.ToString().NullIfEmpty(), stdErr.ToString().NullIfEmpty(), process.ExitCode);
            }
        }
    }
}
