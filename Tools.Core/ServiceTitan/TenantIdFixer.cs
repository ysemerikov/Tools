using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tools.Core.Logging;

namespace Tools.Core.ServiceTitan
{
    public class TenantIdFixer : StarterBase
    {
        // select Id, Name from TenantRecord
        private const string tenantIdToNameFilename = @"C:\Users\ysemerikov\Downloads\tenants.csv";
        private const string tenantPhonesFilename = @"C:\Users\ysemerikov\Downloads\phones.csv";
        private const string outboundIdsFilename = @"C:\Users\ysemerikov\Downloads\ocids.csv";
        private const string fileToFix = TwilioExtender.OutputFilename;
        private const string outputFilename = TwilioExtender.OutputFilename+"-result.csv";

        public void Do()
        {
            var tenantNames = File.ReadAllLines(tenantIdToNameFilename)
                .Skip(2)
                .Select(x => x.Split(','))
                .ToDictionary(a => long.Parse(a[0]), a => a[1]);

            var tenantIds = tenantNames
                .ToDictionary(x => x.Value, x => x.Key);

            var phones = File.ReadAllLines(tenantPhonesFilename)
                .Skip(2)
                .Select(x => x.Split(','))
                .ToDictionary(a => a[0], a => long.Parse(a[1]));

            var outboundIds = File.ReadAllLines(outboundIdsFilename)
                .Select(x => x.Split(','))
                .Select(a => (TenantName: a[0], Phone: a[1]))
                .Where(x => x.Phone.Length == 12
                            && !x.TenantName.EndsWith("copy")
                            && !x.TenantName.EndsWith("demo")
                            && !x.TenantName.EndsWith("intacct")
                            && !x.TenantName.EndsWith("data")
                            && !x.TenantName.Contains("clone")
                            && !x.TenantName.Contains("backup")
                            && !x.TenantName.Contains("test"))
                .DistinctBy(x => x.Phone)
                .Select(x => tenantIds.TryGetValue(x.TenantName, out var tenantId)
                    ? (TenantId: tenantId, PhoneNumber: x.Phone)
                    : default)
                .Where(x => x != default)
                .ToDictionary(x => x.PhoneNumber, x => x.TenantId);

            var source = File.ReadAllLines(fileToFix);
            var result = new List<string>(source.Length)
            {
                source[0].Replace(",TenantId,", ",TenantName,TenantId,")
            };

            var foundByPhones = 0;
            var foundByOCIds = 0;
            var notFound = 0;

            for (var i = 1; i < source.Length; i++)
            {
                var a = source[i].Split(',');
                long tenantId;
                if (!string.IsNullOrEmpty(a[1]))
                {
                    tenantId = long.Parse(a[1]);
                }
                else
                {
                    var number = a[4] == "inbound" ? a[3] : a[2];
                    if (phones.TryGetValue(number, out tenantId))
                    {
                        foundByPhones++;
                    } else if (outboundIds.TryGetValue(number, out tenantId))
                    {
                        foundByOCIds++;
                    }
                    else
                    {
                        notFound++;
                        continue;
                    }
                }

                var tenantName = tenantNames[tenantId];
                var list = a.ToList();
                list.Insert(1, tenantName);
                list[2] = tenantId.ToString();
                result.Add(string.Join(",", list));
            }

            Console.WriteLine($"foundByPhones: {foundByPhones}");
            Console.WriteLine($"foundByOCIds: {foundByOCIds}");
            Console.WriteLine($"notFound: {notFound}");

            File.WriteAllLines(outputFilename, result);
        }

        public TenantIdFixer(ILogger logger, ArgumentReader argumentReader) : base(logger, argumentReader)
        {
        }
    }
}