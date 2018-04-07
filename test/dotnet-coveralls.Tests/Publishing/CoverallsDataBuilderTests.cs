using System;
using System.Diagnostics;
using System.Linq;
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
                c.RegisterInstance(Substitute.For<ProcessExecutor>());
            });

            DiScope.Container.GetInstance<ProcessExecutor>().Execute(Arg.Any<ProcessStartInfo>()).Returns((null, "none", 1));
            
            Environment.SetEnvironmentVariable(EnvironmentVariablesProvider.CI.BUILD_NUMBER, SomeBuildNumber);
            Environment.SetEnvironmentVariable(EnvironmentVariablesProvider.CI.BRANCH, SomeBranch);
            Environment.SetEnvironmentVariable(EnvironmentVariablesProvider.CI.BUILD_URL, SomeBuildUrl);
            Environment.SetEnvironmentVariable(EnvironmentVariablesProvider.CI.NAME, SomeServiceName);
            Environment.SetEnvironmentVariable(EnvironmentVariablesProvider.CI.REPO_TOKEN, SomeCoverallsToken);
        };

        Cleanup after = () =>
        {
            Environment.SetEnvironmentVariable(EnvironmentVariablesProvider.CI.BUILD_NUMBER, null);
            Environment.SetEnvironmentVariable(EnvironmentVariablesProvider.CI.BRANCH, null);
            Environment.SetEnvironmentVariable(EnvironmentVariablesProvider.CI.BUILD_URL, null);
            Environment.SetEnvironmentVariable(EnvironmentVariablesProvider.CI.NAME, null);
            Environment.SetEnvironmentVariable(EnvironmentVariablesProvider.CI.REPO_TOKEN, null);
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
    public class when_parsing_pr_and_ci_environment_variables_available : when_ci_environment_variables_available
    {
        protected static string SomePullRequestNumber = "17";

        Establish context = () => Environment.SetEnvironmentVariable(EnvironmentVariablesProvider.CI.PULL_REQUEST, SomePullRequestNumber);
        Cleanup after = () => Environment.SetEnvironmentVariable(EnvironmentVariablesProvider.CI.PULL_REQUEST, null);

        It should_set_pull_request = () => CoverallsData.ServicePullRequest.ShouldBe(SomePullRequestNumber);
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
            Environment.SetEnvironmentVariable(nameof(AppVeyorProvider.AppVeyor).ToUpper(), "true");
            Environment.SetEnvironmentVariable(AppVeyorProvider.AppVeyor.BUILD_VERSION, SomeBuildVersion);
            Environment.SetEnvironmentVariable(AppVeyorProvider.AppVeyor.COMMIT_AUTHOR, SomeCommitAuthor);
            Environment.SetEnvironmentVariable(AppVeyorProvider.AppVeyor.COMMIT_BRANCH, AppVeyorBranch);
            Environment.SetEnvironmentVariable(AppVeyorProvider.AppVeyor.COMMIT_EMAIL, SomeCommitEmail);
            Environment.SetEnvironmentVariable(AppVeyorProvider.AppVeyor.COMMIT_ID, SomeCommitId);
            Environment.SetEnvironmentVariable(AppVeyorProvider.AppVeyor.COMMIT_MESSAGE, SomeCommitMessage);
            Environment.SetEnvironmentVariable(AppVeyorProvider.AppVeyor.JOB_ID, SomeJobId);
            Environment.SetEnvironmentVariable(AppVeyorProvider.AppVeyor.REPO_NAME, SomeRepoName);
        };

        Cleanup after = () =>
        {
            Environment.SetEnvironmentVariable(nameof(AppVeyorProvider.AppVeyor).ToUpper(), null);
            Environment.SetEnvironmentVariable(AppVeyorProvider.AppVeyor.BUILD_VERSION, null);
            Environment.SetEnvironmentVariable(AppVeyorProvider.AppVeyor.COMMIT_AUTHOR, null);
            Environment.SetEnvironmentVariable(AppVeyorProvider.AppVeyor.COMMIT_BRANCH, null);
            Environment.SetEnvironmentVariable(AppVeyorProvider.AppVeyor.COMMIT_EMAIL, null);
            Environment.SetEnvironmentVariable(AppVeyorProvider.AppVeyor.COMMIT_ID, null);
            Environment.SetEnvironmentVariable(AppVeyorProvider.AppVeyor.COMMIT_MESSAGE, null);
            Environment.SetEnvironmentVariable(AppVeyorProvider.AppVeyor.JOB_ID, null);
            Environment.SetEnvironmentVariable(AppVeyorProvider.AppVeyor.REPO_NAME, null);
        };

        It should_set_service_name = () => CoverallsData.ServiceName.ShouldBe(nameof(AppVeyorProvider.AppVeyor).ToLower());
        It should_set_service_job_id = () => CoverallsData.ServiceJobId.ShouldBe(SomeJobId);
        It should_set_service_number = () => CoverallsData.ServiceNumber.ShouldBe(SomeBuildVersion);
        It should_set_service_build_url = () => CoverallsData.ServiceBuildUrl.ShouldBe($"https://ci.appveyor.com/project/{SomeRepoName}/build/{SomeBuildVersion}");
        It should_set_commit_sha = () => CoverallsData.CommitSha.ShouldBe(SomeCommitId);
        It should_set_commit_id = () => CoverallsData.Git.Head.Id.ShouldBe(SomeCommitId);
        It should_set_author_name = () => CoverallsData.Git.Head.AuthorName.ShouldBe(SomeCommitAuthor);
        It should_set_autohr_email = () => CoverallsData.Git.Head.AuthorEmail.ShouldBe(SomeCommitEmail);
        It should_set_committer_name = () => CoverallsData.Git.Head.CommitterName.ShouldBe(SomeCommitAuthor);
        It should_set_committer_email = () => CoverallsData.Git.Head.CommitterEmail.ShouldBe(SomeCommitEmail);
        It should_set_commit_message = () => CoverallsData.Git.Head.Message.ShouldBe(SomeCommitMessage);
        It should_set_the_branch = () => CoverallsData.Git.Branch.ShouldBe(AppVeyorBranch);
        It should_not_set_pr = () => CoverallsData.ServicePullRequest.ShouldBeNull();
    }

    [Subject(typeof(AppVeyorProvider))]
    public class when_parsing_pr_and_when_appveyor_available : when_appveyor_available
    {
        protected const string SomePullRequestCommitId = "09871a";
        protected const string SomePullRequestNumber = "18";

        Establish context = () =>
        {
            Environment.SetEnvironmentVariable(AppVeyorProvider.AppVeyor.PR_COMMIT_ID, SomePullRequestCommitId);
            Environment.SetEnvironmentVariable(AppVeyorProvider.AppVeyor.PR_NUMBER, SomePullRequestNumber);
        };

        Cleanup after = () =>
        {
            Environment.SetEnvironmentVariable(AppVeyorProvider.AppVeyor.PR_COMMIT_ID, null);
            Environment.SetEnvironmentVariable(AppVeyorProvider.AppVeyor.PR_NUMBER, null);
        };

        It should_set_commit_id_to_pr_commit_id = () => CoverallsData.Git.Head.Id.ShouldBe(SomePullRequestCommitId);
        It should_set_pull_request = () => CoverallsData.ServicePullRequest.ShouldBe(SomePullRequestNumber);
    }
}
