using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Dotnet.Coveralls.Io
{
    public class ProcessExecutor : IProcessExecutor
    {
        private readonly ILogger logger;

        public ProcessExecutor(ILogger logger)
        {
            this.logger = logger;
        }

        public async Task<(string StandardOut, string StandardErr, int ReturnCode)> Execute(ProcessStartInfo processStartInfo)
        {
            processStartInfo.RedirectStandardError = true;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.UseShellExecute = false;

            var stdOut = new StringBuilder();
            var stdErr = new StringBuilder();

            logger.LogDebug($"{processStartInfo.FileName} {processStartInfo.Arguments}");
            using (var process = Process.Start(processStartInfo))
            {
                process.OutputDataReceived += (_, e) =>
                {
                    stdOut.AppendLine(e.Data);
                    if (!string.IsNullOrWhiteSpace(e.Data))
                    {
                        logger.LogDebug(e.Data);
                    }
                };

                process.ErrorDataReceived += (_, e) =>
                {
                    stdErr.AppendLine(e.Data);
                    if (!string.IsNullOrWhiteSpace(e.Data))
                    {
                        logger.LogWarning(e.Data);
                    }
                };

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                await Task.Run(() => process.WaitForExit());

                return (stdOut.ToString().NullIfEmpty(), stdErr.ToString().NullIfEmpty(), process.ExitCode);
            }
        }
    }
}
