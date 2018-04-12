using System;
using System.Collections.Generic;
using clipr;
using clipr.Usage;

namespace Dotnet.Coveralls.CommandLine
{
    [ApplicationInfo(Name = "dotnet coveralls", Description = "Dotnet CLI command to easily push code coverage to coveralls.io")]
    public class CoverallsOptions
    {
        [NamedArgument("repo-token", Description = "The coveralls.io repository token. If not set, will get value from the COVERALLS_REPO_TOKEN environment variable.")]
        public string RepoToken { get; set; }

        [NamedArgument("dry-run", Action = ParseAction.StoreTrue, Description = "This flag will stop coverage results being posted to coveralls.io")]
        public bool DryRun { get; set; }

        [NamedArgument("output", Description = "The coverage results json will be written to this file if provided, and dry-run is also set.")]
        public string Output { get; set; }

        [NamedArgument("open-cover", Description = "One ore more OpenCover xml files to include", Action = ParseAction.Append)]
        public IEnumerable<string> OpenCover { get; set; }

        [NamedArgument("dynamic-code-coverage", Description = "One ore more CodeCoverage.exe xml files to include", Action = ParseAction.Append)]
        public IEnumerable<string> DynamicCodeCoverage { get; set; }

        [NamedArgument("vs-coverage", Description = "One ore more Visual Studio Coverage xml files to include", Action = ParseAction.Append)]
        public IEnumerable<string> VsCoverage { get; set; }

        [NamedArgument("monocov", Description = "One ore more monocov results folders to include", Action = ParseAction.Append)]
        public IEnumerable<string> MonoCov { get; set; }

        [NamedArgument("chutzpah", Description = "One ore more chutzpah json files to include", Action = ParseAction.Append)]
        public IEnumerable<string> Chutzpah { get; set; }

        [NamedArgument("lcov", Description = "One ore more lcov files to include", Action = ParseAction.Append)]
        public IEnumerable<string> Lcov { get; set; }

        [NamedArgument("parallel", Action = ParseAction.StoreTrue, Description = "Set this flag if you're using parallel builds. If set, coveralls.io will wait for the webhook before completing the build.")]
        public bool Parallel { get; set; }

        [NamedArgument("use-relative-paths", Action = ParseAction.StoreTrue, Description = "If set, will attempt to strip the current working directory from the beginning of the source file paths in the coverage reports.")]
        public bool UseRelativePaths { get; set; }

        [NamedArgument("base-path", Description = "When use-relative-paths and a base-path is provided, this path is used instead of the current working directory.")]
        public string BasePath { get; set; }

        [NamedArgument("service-name", Description = "The name of the service generating coveralls reports.")]
        public string ServiceName { get; set; }

        [NamedArgument("job-id", Description = "The unique job Id to provide to coveralls.io.")]
        public string JobId { get; set; }

        [NamedArgument("build-url", Description = "The url to the CI build.")]
        public string BuildUrl { get; set; }

        [NamedArgument("build-number", Description = "The build number. Will increment automatically if not specified.")]
        public string BuildNumber { get; set; }

        [NamedArgument("ignore-upload-errors", Action = ParseAction.StoreTrue, Description = "If set, will exit with code 0 on upload error.")]
        public bool IgnoreUploadErrors { get; set; }

        [NamedArgument("commit-id", Description = "The git commit hash for the coverage report. If omitted, will attempt to get it from git.")]
        public string CommitId { get; set; }

        [NamedArgument("commit-branch", Description = "The git commit branch for the coverage report. If omitted, will attempt to get it from git.")]
        public string CommitBranch { get; set; }

        [NamedArgument("commit-author", Description = "The git commit author for the coverage report. If omitted, will attempt to get it from git.")]
        public string CommitAuthor { get; set; }

        [NamedArgument("commit-email", Description = "The git commit email for the coverage report. If omitted, will attempt to get it from git.")]
        public string CommitEmail { get; set; }

        [NamedArgument("commit-message", Description = "The git commit message for the coverage report. If omitted, will attempt to get it from git.")]
        public string CommitMessage { get; set; }

        [NamedArgument("pr-id", Description = "The pull request id. Used for updating status on PRs for source control providers that support them (GitHub, BitBucket, etc.).")]
        public string PullRequestId { get; set; }

        [NamedArgument("verbose", Action = ParseAction.StoreTrue, Description = "When set, will display additional log information")]
        public bool Verbose { get; set; }

        [NamedArgument("run-at", Description = "The time the job was run at. If not set, will pass UtcNow.")]
        public string RunAt { get; set; }

        public static string Usage => new AutomaticHelpGenerator<CoverallsOptions>().GetHelp(new CliParser<CoverallsOptions>(new CoverallsOptions()).Config);
        public static CoverallsOptions Parse(string[] args)
        {
            var res = new CoverallsOptions();
            var parser = new CliParser<CoverallsOptions>(res);
            try
            {
                parser.Parse(args);

                if (!(res.OpenCover.SafeAny() ||
                      res.MonoCov.SafeAny() ||
                      res.Chutzpah.SafeAny() ||
                      res.DynamicCodeCoverage.SafeAny() ||
                      res.Lcov.SafeAny() ||
                      res.VsCoverage.SafeAny()))
                {
                    throw new Exception(Usage);
                }

                return res;
            }
            catch (ParseException ex)
            {
                throw new Exception($"{ex.Message}{Environment.NewLine}{Environment.NewLine}{Usage}");
            }
        }
    }
}
