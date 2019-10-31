using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Tools.Core.Logging;

namespace Tools.Core.ServiceTitan
{
    public class FuckingReportStarter
    {
        public FuckingReportStarter(ILogger logger, ArgumentReader argumentReader)
        {
        }

        // ReSharper disable once UnusedMember.Global
        public async Task Fuck()
        {
            var tenantGroups = (await File.ReadAllLinesAsync(@"C:\projects\Tools\tenantGroups.csv"))
                .Skip(1)
                .Select(x => x.Split(new[] {','}, 2))
                .GroupBy(x => x[0].Trim(), x => x[1].Trim())
                .ToDictionary(x => x.Key, x => string.Join(";", x));

            //TenantName,Direction,Month,"Duration, seconds"
            var report = (await File.ReadAllLinesAsync(@"C:\projects\Tools\report.csv"))
                .Skip(1)
                .Select(x =>
                {
                    var a = x.Split(',');
                    if (a.Length != 4)
                        throw new Exception(x);
                    return new
                    {
                        Key = a[0].Trim() + "," + a[1].Trim(),
                        Duration = long.Parse(a[3]),
                    };
                })
                .GroupBy(x => x.Key, x => x.Duration)
                .Select(g =>
                {
                    var a = g.Key.Split(',');
                    var tenantName = a[0];
                    if (!tenantGroups.TryGetValue(tenantName, out var groups))
                        groups = string.Empty;

                    return $"{tenantName},{a[1]},{groups},{g.Sum()}";
                });

            var path = @"C:\projects\Tools\result.csv";
            await File.WriteAllLinesAsync(path, new[] {"Tenant Name, Direction, Tags, Duration (secs)"});
            await File.AppendAllLinesAsync(path, report);
        }
    }
}