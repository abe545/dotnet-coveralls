using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Dotnet.Coveralls.Data;
using Dotnet.Coveralls.Io;
using Microsoft.Extensions.Logging;

namespace Dotnet.Coveralls.Git
{
    public class GitProcessGitDataResolver : IGitDataResolver
    {
        private readonly IProcessExecutor processExecutor;
        private readonly ILogger<GitProcessGitDataResolver> logger;

        public GitProcessGitDataResolver(IProcessExecutor processExecutor, ILoggerFactory loggerFactory)
        {
            this.processExecutor = processExecutor;
            this.logger = loggerFactory.CreateLogger<GitProcessGitDataResolver>();
        }

        public bool CanProvideData => true;

        public async Task<GitData> CreateGitData()
        {
            var gitData = new GitData { Head = new GitHead() };

            gitData.Head.Id = await Git("log -1 --pretty=format:'%H'");
            gitData.Head.AuthorName = await Git("log -1 --pretty=format:'%aN'");
            gitData.Head.AuthorEmail = await Git("log -1 --pretty=format:'%ae'");
            gitData.Head.CommitterName = await Git("log -1 --pretty=format:'%cN'");
            gitData.Head.CommitterEmail = await Git("log -1 --pretty=format:'%ce'");
            gitData.Head.Message = await Git("log -1 --pretty=format:'%s'");
            gitData.Branch = await Git("rev-parse --abbrev-ref HEAD");

            var remotes = (await Git("remote -v"))?.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (remotes?.Length > 0)
            {
                gitData.Remotes = remotes
                    .Select(r =>
                    {
                        var parts = r.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length < 2) return null;
                        return new GitRemote { Name = parts[0], Url = parts[1] };
                    })
                    .Where(r => r != null)
                    .Distinct()
                    .ToArray();
            }

            return gitData;

            async Task<string> Git(string arguments)
            {
                logger.LogDebug($"git {arguments}");
                var psi = new ProcessStartInfo("git", arguments);
                var (stdOut, stdErr, exit) = await processExecutor.Execute(psi);
                if (exit > 0)
                {
                    if (!string.IsNullOrWhiteSpace(stdErr)) logger.LogWarning(stdErr);
                    return null;
                }

                return stdOut;
            }
        }
    }
}
