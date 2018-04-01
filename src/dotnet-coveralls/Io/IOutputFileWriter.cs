using System.Threading.Tasks;

namespace Dotnet.Coveralls.Io
{
    public interface IOutputFileWriter
    {
        Task WriteCoverageOutput(string text);
    }
}