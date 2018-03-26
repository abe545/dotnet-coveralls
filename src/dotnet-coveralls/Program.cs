using System;
using System.Threading.Tasks;
using clipr;
using Dotnet.Coveralls.CommandLine;
using Dotnet.Coveralls.Io;
using Dotnet.Coveralls.Publishers;
using Microsoft.Extensions.FileProviders;

namespace Dotnet.Coveralls
{

    public class Program
    {
        public static async Task<int> Main(string[] argv)
        {
            try
            {
                var options = CliParser.Parse<CoverallsOptions>(argv);
                var publisher = new CoverallsPublisher(new FileWriter(), new UnrestrictedFileProvider(Environment.CurrentDirectory));
                return await publisher.Publish(options);
            }
            catch (Exception ex)
            {
                await Console.Error.WriteLineAsync(ex.Message);
                return 1;
            }
        }
    }
}