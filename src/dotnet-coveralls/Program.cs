using System;
using System.Threading.Tasks;
using clipr.Core;

namespace Dotnet.Coveralls
{
    public class Program
    {
        public static async Task<int> Main(string[] argv)
        {
            try
            {
                using (var scope = Di.Setup(argv))
                {
                    return await scope.Container.GetInstance<CoverallsPublisher>().Publish();
                }
            }
            catch (ParserExit)
            {
                return 1;
            }
            catch (Exception ex)
            {
                await Console.Error.WriteLineAsync(ex.Message);
                return 1;
            }
        }
    }
}