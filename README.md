dotnet-coveralls
=============

Hard fork of [coveralls.net](https://github.com/csMACnz/coveralls.net) to create a simple `dotnet coveralls` publishing cli

Usage is available via `--help` but here it is too:

```
 --repo-token             The coveralls.io repository token. If not set, will get value from the COVERALLS_REPO_TOKEN
                          environment variable.
 --dry-run                This flag will stop coverage results being posted to coveralls.io
 --open-cover             One ore more OpenCover xml files to include
 --dynamic-code-coverage  One ore more CodeCoverage.exe xml files to include
 --vs-coverage            One ore more Visual Studio Coverage xml files to include
 --monocov                One ore more monocov results folders to include
 --chutzpah               One ore more chutzpah json files to include
 --lcov                   One ore more lcov files to include
 --parallel               If using parallel builds. If sent, coveralls.io will wait for the webhook before completing
                          the build.
 --use-relative-paths     If set, will attempt to strip the current working directory from the beginning of the source
                          file paths in the coverage reports.
 --base-path              When use-relative-paths and a basePath is provided, this path is used instead of the current
                          working directory.
 --service-name           The name of the service generating coveralls reports.
 --job-id                 The unique job Id to provide to coveralls.io.
 --build-number           The build number. Will increment automatically if not specified.
 --ignore-upload-errors   If set, will exit with code 0 on upload error.
 --commit-id              The git commit hash for the coverage report. If omitted, will attempt to get it from git.
 --commit-branch          The git commit branch for the coverage report. If omitted, will attempt to get it from git.
 --commit-author          The git commit author for the coverage report. If omitted, will attempt to get it from git.
 --commit-email           The git commit email for the coverage report. If omitted, will attempt to get it from git.
 --commit-message         The git commit message for the coverage report. If omitted, will attempt to get it from git.
 --pr-id                  The pull request id. Used for updating status on PRs for source control providers that support
                          them (GitHub, BitBucket, etc.).
 -h, --help               Display this help document.
 -o, --output             The coverage results json will be written to this file if provided.
 --version                Displays the version of the current executable.
 ```