using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Dotnet.Coveralls.CommandLine;
using Dotnet.Coveralls.Data;

namespace Dotnet.Coveralls.Publishing
{
    public class CommandLineProvider : ICoverallsDataProvider
    {
        private readonly CoverallsOptions options;

        public CommandLineProvider(CoverallsOptions options)
        {
            this.options = options;
        }

        public bool CanProvideData => true;

        public Task<CoverallsData> ProvideCoverallsData() => Task.FromResult(new CoverallsData
        {
            ServiceName = options.ServiceName,
            ServiceJobId = options.JobId,
            ServiceNumber = options.BuildNumber,
            PullRequestId = options.PullRequestId,
        });
    }
}
