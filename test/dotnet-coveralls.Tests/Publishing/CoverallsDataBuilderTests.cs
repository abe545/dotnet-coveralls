using System;
using System.Diagnostics;
using System.Linq;
using Dotnet.Coveralls.Adapters;
using Dotnet.Coveralls.Data;
using Dotnet.Coveralls.Git;
using Dotnet.Coveralls.Io;
using Dotnet.Coveralls.Parsers;
using Dotnet.Coveralls.Publishing;
using Machine.Specifications;
using NSubstitute;
using Shouldly;

namespace Dotnet.Coveralls.Tests.Publishing.CoverallsDataBuilder
{
    [Subject(typeof(CommandLineProvider))]
    public class when_git_info_passed_on_command_line : Spec
    {
        protected const string SomeBuildNumber = "18";
        protected const string SomeBranch = "vogsphere";
        protected const string SomeServiceName = "Hitchikers guide to the galaxy";
        protected const string SomeCoverallsToken = "vogon";
        protected const string SomeBuildUrl = "htts://my.ci.com/build/panic";
        protected const string SomeAuthorName = "Arthur Dent";
        protected const string SomeAuthorEmail = "arthur@dent.guide";
        protected const string SomeCommitId = "4242";
        protected const string SomeCommitMessage = "Don't Panic!";
        protected static string SomePullRequestNumber = "42";
        protected const string SomeJobId = "42-42";
        protected const string SomeRepoName = "HGTTG";

        protected static CoverallsData CoverallsData;

        Establish context = () =>
        {
            Setup(new[] 
            {
                "--lcov", "dummy",
                "--repo-token", SomeCoverallsToken,
                "--service-name", SomeServiceName,
                "--job-id", SomeJobId,
                "--build-number", SomeBuildNumber,
                "--commit-id", SomeCommitId,
                "--commit-branch", SomeBranch,
                "--commit-author", SomeAuthorName,
                "--commit-email", SomeAuthorEmail,
                "--commit-message", SomeCommitMessage,
                "--pr-id", SomePullRequestNumber,
                "--build-url", SomeBuildUrl
            }, c =>
            {
                c.RegisterCollection(typeof(ICoverageParser), Enumerable.Empty<Type>());
            });
        };

        Because of = () => CoverallsData = DiScope.Container.GetInstance<ICoverallsDataBuilder>().ProvideCoverallsData().Result;

        It should_set_service_name = () => CoverallsData.ServiceName.ShouldBe(SomeServiceName);
        It should_set_service_job_id = () => CoverallsData.ServiceJobId.ShouldBe(SomeJobId);
        It should_set_service_number = () => CoverallsData.ServiceNumber.ShouldBe(SomeBuildNumber);
        It should_set_service_build_url = () => CoverallsData.ServiceBuildUrl.ShouldBe(SomeBuildUrl);
        It should_set_service_branch = () => CoverallsData.ServiceBranch.ShouldBe(SomeBranch);
        It should_set_commit_sha = () => CoverallsData.CommitSha.ShouldBe(SomeCommitId);
        It should_set_the_branch = () => CoverallsData.Git.Branch.ShouldBe(SomeBranch);
        It should_set_coveralls_token = () => CoverallsData.RepoToken.ShouldBe(SomeCoverallsToken);
        It should_set_commit_id = () => CoverallsData.Git.Head.Id.ShouldBe(SomeCommitId);
        It should_set_author_name = () => CoverallsData.Git.Head.AuthorName.ShouldBe(SomeAuthorName);
        It should_set_author_email = () => CoverallsData.Git.Head.AuthorEmail.ShouldBe(SomeAuthorEmail);
        It should_set_committer_name = () => CoverallsData.Git.Head.CommitterName.ShouldBe(SomeAuthorName);
        It should_set_committer_email = () => CoverallsData.Git.Head.CommitterEmail.ShouldBe(SomeAuthorEmail);
        It should_set_commit_message = () => CoverallsData.Git.Head.Message.ShouldBe(SomeCommitMessage);
        It should_set_pull_request = () => CoverallsData.ServicePullRequest.ShouldBe(SomePullRequestNumber);
    }

    [Subject(typeof(EnvironmentVariablesProvider))]
    public class when_ci_environment_variables_available : Spec
    {
        protected const string SomeBuildNumber = "1";
        protected const string SomeBranch = "earth";
        protected const string SomeServiceName = "TOS";
        protected const string SomeCoverallsToken = "star-trek";
        protected const string SomeBuildUrl = "htts://my.ci.com/build/tos";

        protected static CoverallsData CoverallsData;

        Establish context = () =>
        {
            Setup(new[] { "--lcov", "dummy" }, c =>
            {
                c.RegisterCollection(typeof(ICoverageParser), Enumerable.Empty<Type>());
                c.Register(() => Substitute.For<IEnvironmentVariables>());
                c.RegisterInstance(Substitute.For<IProcessExecutor>());
            });

            DiScope.Container.GetInstance<IProcessExecutor>().Execute(Arg.Any<ProcessStartInfo>()).Returns((null, "none", 1));
            var environment = DiScope.Container.GetInstance<IEnvironmentVariables>();

            environment.GetEnvironmentVariable(EnvironmentVariablesProvider.CI.BUILD_NUMBER).Returns(SomeBuildNumber);
            environment.GetEnvironmentVariable(EnvironmentVariablesProvider.CI.BRANCH).Returns(SomeBranch);
            environment.GetEnvironmentVariable(EnvironmentVariablesProvider.CI.BUILD_URL).Returns(SomeBuildUrl);
            environment.GetEnvironmentVariable(EnvironmentVariablesProvider.CI.NAME).Returns(SomeServiceName);
            environment.GetEnvironmentVariable(EnvironmentVariablesProvider.CI.REPO_TOKEN).Returns(SomeCoverallsToken);
        };

        Because of = () => CoverallsData = DiScope.Container.GetInstance<ICoverallsDataBuilder>().ProvideCoverallsData().Result;

        It should_set_service_name = () => CoverallsData.ServiceName.ShouldBe(SomeServiceName);
        It should_set_service_number = () => CoverallsData.ServiceNumber.ShouldBe(SomeBuildNumber);
        It should_set_the_branch = () => CoverallsData.Git.Branch.ShouldBe(SomeBranch);
        It should_set_service_build_url = () => CoverallsData.ServiceBuildUrl.ShouldBe(SomeBuildUrl);
        It should_set_coveralls_token = () => CoverallsData.RepoToken.ShouldBe(SomeCoverallsToken);
        It should_not_set_pr = () => CoverallsData.ServicePullRequest.ShouldBeNull();
    }

    [Subject(typeof(EnvironmentVariablesProvider))]
    public class when_ci_environment_variables_and_coveralls_service_name_available : Spec
    {
        protected const string SomeCoverallsServiceName = "Star Trek";

        protected static CoverallsData CoverallsData;

        Establish context = () => DiScope.Container.GetInstance<IEnvironmentVariables>().GetEnvironmentVariable(EnvironmentVariablesProvider.CI.SERVICE_NAME).Returns(SomeCoverallsServiceName);

        Because of = () => CoverallsData = DiScope.Container.GetInstance<ICoverallsDataBuilder>().ProvideCoverallsData().Result;

        It should_set_service_name = () => CoverallsData.ServiceName.ShouldBe(SomeCoverallsServiceName);
    }

    [Subject(typeof(EnvironmentVariablesProvider))]
    public class when_parsing_pr_and_ci_environment_variables_available : when_ci_environment_variables_available
    {
        protected static string SomePullRequestNumber = "17";

        Establish context = () => DiScope.Container.GetInstance<IEnvironmentVariables>().GetEnvironmentVariable(EnvironmentVariablesProvider.CI.PULL_REQUEST).Returns(SomePullRequestNumber);

        It should_set_pull_request = () => CoverallsData.ServicePullRequest.ShouldBe(SomePullRequestNumber);
    }

    [Subject(typeof(GitEnvironmentVariableGitDataResolver))]
    public class when_git_environment_variables_available : when_ci_environment_variables_available
    {
        protected const string SomeAuthorName = "James T. Kirk";
        protected const string SomeAuthorEmail = "james.kirk@starfleet.gov";
        protected const string SomeCommitterName = "James";
        protected const string SomeCommitterEmail = "kirk@starfleet.gov";
        protected const string GitBranch = "earth";
        protected const string SomeCommitId = "123456";
        protected const string SomeCommitMessage = "Conquest is easy. Control is not.";

        Establish context = () =>
        {
            var environment = DiScope.Container.GetInstance<IEnvironmentVariables>();

            environment.GetEnvironmentVariable(GitEnvironmentVariableGitDataResolver.Git.AUTHOR_NAME).Returns(SomeAuthorName);
            environment.GetEnvironmentVariable(GitEnvironmentVariableGitDataResolver.Git.AUTHOR_EMAIL).Returns(SomeAuthorEmail);
            environment.GetEnvironmentVariable(GitEnvironmentVariableGitDataResolver.Git.COMMITTER_NAME).Returns(SomeCommitterName);
            environment.GetEnvironmentVariable(GitEnvironmentVariableGitDataResolver.Git.COMMITTER_EMAIL).Returns(SomeCommitterEmail);
            environment.GetEnvironmentVariable(GitEnvironmentVariableGitDataResolver.Git.BRANCH).Returns(GitBranch);
            environment.GetEnvironmentVariable(GitEnvironmentVariableGitDataResolver.Git.ID).Returns(SomeCommitId);
            environment.GetEnvironmentVariable(GitEnvironmentVariableGitDataResolver.Git.MESSAGE).Returns(SomeCommitMessage);
        };

        It should_set_commit_id = () => CoverallsData.Git.Head.Id.ShouldBe(SomeCommitId);
        It should_set_author_name = () => CoverallsData.Git.Head.AuthorName.ShouldBe(SomeAuthorName);
        It should_set_author_email = () => CoverallsData.Git.Head.AuthorEmail.ShouldBe(SomeAuthorEmail);
        It should_set_committer_name = () => CoverallsData.Git.Head.CommitterName.ShouldBe(SomeCommitterName);
        It should_set_committer_email = () => CoverallsData.Git.Head.CommitterEmail.ShouldBe(SomeCommitterEmail);
        It should_set_commit_message = () => CoverallsData.Git.Head.Message.ShouldBe(SomeCommitMessage);
        It should_not_set_pr = () => CoverallsData.ServicePullRequest.ShouldBeNull();
    }

    [Subject(typeof(AppVeyorProvider))]
    public class when_appveyor_available : when_ci_environment_variables_available
    {
        protected const string SomeBuildVersion = "number-two";
        protected const string SomeCommitAuthor = "Spock";
        protected const string SomeCommitEmail = "Spock@starfleet.gov";
        protected const string AppVeyorBranch = "vulcan";
        protected const string SomeCommitId = "deadbeef";
        protected const string SomeCommitMessage = "Live long, and prosper.";
        protected const string SomeJobId = "4218";
        protected const string SomeRepoName = "TOS";

        Establish context = () =>
        {
            var environment = DiScope.Container.GetInstance<IEnvironmentVariables>();

            environment.GetEnvironmentVariable(nameof(AppVeyorProvider.AppVeyor).ToUpper()).Returns("true");
            environment.GetEnvironmentVariable(AppVeyorProvider.AppVeyor.BUILD_VERSION).Returns(SomeBuildVersion);
            environment.GetEnvironmentVariable(AppVeyorProvider.AppVeyor.COMMIT_AUTHOR).Returns(SomeCommitAuthor);
            environment.GetEnvironmentVariable(AppVeyorProvider.AppVeyor.COMMIT_BRANCH).Returns(AppVeyorBranch);
            environment.GetEnvironmentVariable(AppVeyorProvider.AppVeyor.COMMIT_EMAIL).Returns(SomeCommitEmail);
            environment.GetEnvironmentVariable(AppVeyorProvider.AppVeyor.COMMIT_ID).Returns(SomeCommitId);
            environment.GetEnvironmentVariable(AppVeyorProvider.AppVeyor.COMMIT_MESSAGE).Returns(SomeCommitMessage);
            environment.GetEnvironmentVariable(AppVeyorProvider.AppVeyor.JOB_ID).Returns(SomeJobId);
            environment.GetEnvironmentVariable(AppVeyorProvider.AppVeyor.REPO_NAME).Returns(SomeRepoName);
        };

        It should_set_service_name = () => CoverallsData.ServiceName.ShouldBe(nameof(AppVeyorProvider.AppVeyor).ToLower());
        It should_set_service_job_id = () => CoverallsData.ServiceJobId.ShouldBe(SomeJobId);
        It should_set_service_number = () => CoverallsData.ServiceNumber.ShouldBe(SomeBuildVersion);
        It should_set_service_build_url = () => CoverallsData.ServiceBuildUrl.ShouldBe($"https://ci.appveyor.com/project/{SomeRepoName}/build/{SomeBuildVersion}");
        It should_set_service_branch = () => CoverallsData.ServiceBranch.ShouldBe(AppVeyorBranch);
        It should_set_commit_sha = () => CoverallsData.CommitSha.ShouldBe(SomeCommitId);
        It should_set_committer_name = () => CoverallsData.Git.Head.CommitterName.ShouldBe(SomeCommitAuthor);
        It should_set_committer_email = () => CoverallsData.Git.Head.CommitterEmail.ShouldBe(SomeCommitEmail);
        It should_set_commit_message = () => CoverallsData.Git.Head.Message.ShouldBe(SomeCommitMessage);
        It should_not_set_pr = () => CoverallsData.ServicePullRequest.ShouldBeNull();
    }

    [Subject(typeof(AppVeyorProvider))]
    public class when_appveyor_available_and_is_pr : when_appveyor_available
    {
        protected const string SomePullRequestNumber = "18";

        Establish context = () =>
        {
            var environment = DiScope.Container.GetInstance<IEnvironmentVariables>();

            environment.GetEnvironmentVariable(AppVeyorProvider.AppVeyor.PR_NUMBER).Returns(SomePullRequestNumber);
        };

        It should_set_pull_request = () => CoverallsData.ServicePullRequest.ShouldBe(SomePullRequestNumber);
    }

    [Subject(typeof(GitProcessGitDataResolver))]
    public class when_is_git_repo : when_ci_environment_variables_available
    {
        protected const string SomeAuthorName = "Jean Luc Picard";
        protected const string SomeAuthorEmail = "jean.picard@starfleet.gov";
        protected const string SomeCommitterName = "Jean";
        protected const string SomeCommitterEmail = "picard@starfleet.gov";
        protected const string GitBranch = "france";
        protected const string SomeCommitId = "098765";
        protected const string SomeCommitMessage = "Tea. Earl Grey. Hot.";
        protected const string SomeRemoteUpstream = "tng@starfleet.git";
        protected const string SomeRemoteOrigin = "enterprise@starfleet.git";

        Establish context = () =>
        {
            var processExecutor = DiScope.Container.GetInstance<IProcessExecutor>();

            SetupGit(GitProcessGitDataResolver.GitArgs.AUTHOR_NAME, SomeAuthorName);
            SetupGit(GitProcessGitDataResolver.GitArgs.AUTHOR_EMAIL, SomeAuthorEmail);
            SetupGit(GitProcessGitDataResolver.GitArgs.COMMITTER_NAME, SomeCommitterName);
            SetupGit(GitProcessGitDataResolver.GitArgs.COMMITTER_EMAIL, SomeCommitterEmail);
            SetupGit(GitProcessGitDataResolver.GitArgs.BRANCH, GitBranch);
            SetupGit(GitProcessGitDataResolver.GitArgs.ID, SomeCommitId);
            SetupGit(GitProcessGitDataResolver.GitArgs.MESSAGE, SomeCommitMessage);
            SetupGit(GitProcessGitDataResolver.GitArgs.REMOTES, $"upstream\t{SomeRemoteUpstream} (fetch)\r\nupstream\t{SomeRemoteUpstream} (push)\norigin {SomeRemoteOrigin}");

            void SetupGit(string arguments, string result) =>
                processExecutor.Execute(Arg.Is<ProcessStartInfo>(psi => psi.FileName == "git" && psi.Arguments == arguments)).Returns((result, null, 0));
        };

        It should_set_commit_id = () => CoverallsData.Git.Head.Id.ShouldBe(SomeCommitId);
        It should_set_author_name = () => CoverallsData.Git.Head.AuthorName.ShouldBe(SomeAuthorName);
        It should_set_author_email = () => CoverallsData.Git.Head.AuthorEmail.ShouldBe(SomeAuthorEmail);
        It should_set_committer_name = () => CoverallsData.Git.Head.CommitterName.ShouldBe(SomeCommitterName);
        It should_set_committer_email = () => CoverallsData.Git.Head.CommitterEmail.ShouldBe(SomeCommitterEmail);
        It should_set_commit_message = () => CoverallsData.Git.Head.Message.ShouldBe(SomeCommitMessage);
        It should_not_set_pr = () => CoverallsData.ServicePullRequest.ShouldBeNull();
        It should_set_remotes = () =>
        {
            CoverallsData.Git.Remotes.ShouldBe(new[]
            {
                new GitRemote { Name = "upstream", Url = SomeRemoteUpstream },
                new GitRemote { Name = "origin", Url = SomeRemoteOrigin }
            });
        };
    }
}
