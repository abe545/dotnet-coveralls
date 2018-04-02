namespace Dotnet.Coveralls.Data
{
    public sealed class CoverallsData
    {
        public string RepoToken { get; set; }
        public string ServiceJobId { get; set; }
        public string ServiceName { get; set; }
        public string ServiceBuildUrl { get; set; }
        public string ServiceNumber { get; set; }
        public string ServicePullRequest { get; set; }
        public bool? Parallel { get; set; }
        public CoverageFile[] SourceFiles { get; set; }
        public GitData Git { get; set; }
    }

    public sealed class GitData
    {
        public GitHead Head { get; set; }
        public string Branch { get; set; }
        public GitRemotes Remotes { get; set; }
    }

    public sealed class GitHead
    {
        public string Id { get; set; }
        public string AuthorName { get; set; }
        public string AuthorEmail { get; set; }
        public string CommitterName { get; set; }
        public string ComitterEmail { get; set; }
        public string Message { get; set; }
    }

    public sealed class GitRemotes
    {
        public string Name { get; set; }
        public string Url { get; set; }
    }
}