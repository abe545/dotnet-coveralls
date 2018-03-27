using Beefeater;

namespace Dotnet.Coveralls.Ports
{
    public interface ICoverallsService
    {
        Result<bool, string> Upload(string fileData);
    }
}