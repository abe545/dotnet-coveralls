using System.Threading.Tasks;

namespace Dotnet.Coveralls.Io
{
    public interface IFileWriter
    {
        Task WriteAllText(string path, string text);
    }
}