using System;
using Dotnet.Coveralls.Ports;

namespace Dotnet.Coveralls.Adapters
{
    internal class EnvironmentVariables : IEnvironmentVariables
    {
        public string GetEnvironmentVariable(string key)
        {
            return Environment.GetEnvironmentVariable(key);
        }
    }
}