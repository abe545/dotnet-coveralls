namespace Dotnet.Coveralls.Ports
{
    public interface IEnvironmentVariables
    {
        string GetEnvironmentVariable(string key);
    }
}