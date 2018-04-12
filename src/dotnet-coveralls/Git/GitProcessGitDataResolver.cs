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

        public static class GitArgs
        {
            public const string ID = "log -1 --pretty=format:%H";
            public const string AUTHOR_EMAIL = "log -1 --pretty=format:%ae";
            public const string AUTHOR_NAME = "log -1 --pretty=format:%aN";
            public const string COMMITTER_EMAIL = "log -1 --pretty=format:%ce";
            public const string COMMITTER_NAME = "log -1 --pretty=format:%cN";
            public const string MESSAGE = "log -1 --pretty=format:%s";
            public const string BRANCH = "rev-parse --abbrev-ref HEAD";
            public const string REMOTES = "remote -v";
        }

        public GitProcessGitDataResolver(IProcessExecutor processExecutor)
        {
            this.processExecutor = processExecutor;
        }

        public bool CanProvideData => true;

        public async Task<GitData> CreateGitData()
        {
            return new GitData
            {
                Head = new GitHead
                {
                    Id = await Git(GitArgs.ID),
                    AuthorName = await Git(GitArgs.AUTHOR_NAME),
                    AuthorEmail = await Git(GitArgs.AUTHOR_EMAIL),
                    CommitterName = await Git(GitArgs.COMMITTER_NAME),
                    CommitterEmail = await Git(GitArgs.COMMITTER_EMAIL),
                    Message = await Git(GitArgs.MESSAGE),
                },
                Branch = await Git(GitArgs.BRANCH),
                Remotes = await ParseRemotes()
            };

            async Task<GitRemote[]> ParseRemotes()
            {
                var remotes = (await Git(GitArgs.REMOTES))?.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                if (remotes?.Length > 0)
                {
                    var splits = new[] { '\t', ' ' };
                    return remotes
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

                return null;
            }

            async Task<string> Git(string arguments)
            {
                var psi = new ProcessStartInfo("git", arguments);
                var (stdOut, stdErr, exit) = await processExecutor.Execute(psi);
                if (exit > 0)
                {
                    return null;
                }

                return stdOut.TrimEnd();
            }
        }
    }
}
