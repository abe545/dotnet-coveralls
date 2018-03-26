using System;

namespace Dotnet.Coveralls
{
    public class PublishCoverallsException : Exception
    {
        public PublishCoverallsException(string message) : base(message)
        {
        }
    }
}
