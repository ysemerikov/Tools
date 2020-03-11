using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Twilio.Rest.Api.V2010.Account;

namespace Tools.Core.ServiceTitan.TwilioDebugger
{
    public class ReportStorage
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
            {DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate};

        private readonly string path;
        private readonly Dictionary<string, Report> storage = new Dictionary<string, Report>();

        public IEnumerable<Report> GetAllReports()
        {
            return storage.Values;
        }

        public bool Has(NotificationResource n)
        {
            return storage.ContainsKey(n.Sid);
        }

        public Task Add(Report report)
        {
            if (string.IsNullOrEmpty(report.Sid))
                throw new ArgumentNullException(nameof(report));

            if (storage.ContainsKey(report.Sid))
                throw new ArgumentException($"{report.Sid} already exists.", nameof(report));

            storage[report.Sid] = report;
            return File.AppendAllLinesAsync(path, new[] {JsonConvert.SerializeObject(report, JsonSerializerSettings)}, Encoding.UTF8);
        }

        private ReportStorage(string path)
        {
            this.path = path;
        }

        public static ReportStorage Restore(string path)
        {
            var storage = new ReportStorage(path);

            if (File.Exists(path))
            {
                foreach (var r in File.ReadAllLines(path, Encoding.UTF8).Select(JsonConvert.DeserializeObject<Report>))
                    storage.storage[r.Sid] = r;
            }

            return storage;
        }
    }
}