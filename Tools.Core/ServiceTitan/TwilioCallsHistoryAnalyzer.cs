using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Tools.Core.ServiceTitan
{
    public class TwilioCallsHistoryAnalyzer : IAction
    {
        public async Task Do()
        {
            var lines = (await File.ReadAllLinesAsync(@"C:\servicetitan\twiliocalls.jsons"))
                    .AsParallel()
                    .Select(x => x.Split(',')[2])
                    // .Take(10)
                    .Distinct()
                ;

            Console.WriteLine(string.Join(", ", lines));
        }
    }
}
