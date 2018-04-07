using System.Diagnostics;
using System.Threading.Tasks;

namespace Dotnet.Coveralls.Io
{
    public interface IProcessExecutor
    {
        Task<(string StandardOut, string StandardErr, int ReturnCode)> Execute(ProcessStartInfo processStartInfo);
    }
}