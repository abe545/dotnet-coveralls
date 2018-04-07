namespace Dotnet.Coveralls.Adapters
{
    public interface IEnvironmentVariables
    {
        string GetEnvironmentVariable(string key);
    }
}