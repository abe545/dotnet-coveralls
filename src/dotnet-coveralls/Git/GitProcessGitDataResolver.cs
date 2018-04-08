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

        public static class GitArgs
        {
            public const string ID = "log -1 --pretty=format:'%H'";
            public const string AUTHOR_EMAIL = "log -1 --pretty=format:'%ae'";
            public const string AUTHOR_NAME = "log -1 --pretty=format:'%aN'";
            public const string COMMITTER_EMAIL = "log -1 --pretty=format:'%ce'";
            public const string COMMITTER_NAME = "log -1 --pretty=format:'%cN'";
            public const string MESSAGE = "log -1 --pretty=format:'%s'";
            public const string BRANCH = "rev-parse --abbrev-ref HEAD";
            public const string REMOTES = "remote -v";
        }

        public GitProcessGitDataResolver(IProcessExecutor processExecutor, ILoggerFactory loggerFactory)
        {
            this.processExecutor = processExecutor;
            this.logger = loggerFactory.CreateLogger<GitProcessGitDataResolver>();
        }

        public bool CanProvideData => true;

        public async Task<GitData> CreateGitData()
        {
            var gitData = new GitData { Head = new GitHead() };

            gitData.Head.Id = await Git(GitArgs.ID);
            gitData.Head.AuthorName = await Git(GitArgs.AUTHOR_NAME);
            gitData.Head.AuthorEmail = await Git(GitArgs.AUTHOR_EMAIL);
            gitData.Head.CommitterName = await Git(GitArgs.COMMITTER_NAME);
            gitData.Head.CommitterEmail = await Git(GitArgs.COMMITTER_EMAIL);
            gitData.Head.Message = await Git(GitArgs.MESSAGE);
            gitData.Branch = await Git(GitArgs.BRANCH);

            var remotes = (await Git(GitArgs.REMOTES))?.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (remotes?.Length > 0)
            {
                var splits = new[] { '\t', ' ' };
                gitData.Remotes = remotes
                    .Select(r =>
                    {
                        var parts = r.Split(splits, StringSplitOptions.RemoveEmptyEntries);
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

                return stdOut.TrimEnd();
            }
        }
    }
}
