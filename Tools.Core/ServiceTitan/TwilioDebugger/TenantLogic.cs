using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Tools.Core.ServiceTitan.TwilioDebugger
{
    public class TenantLogic
    {
        private static readonly Dictionary<string, long?> tenantByPhone;
        private static readonly Dictionary<long, string> nameById;

        static TenantLogic()
        {
            tenantByPhone = File.ReadAllLines(@"C:\servicetitan\phone-tenantId.csv")
                .Skip(1)
                .Select(x => x.Split(','))
                .ToDictionary(x => x[0], x => string.IsNullOrEmpty(x[1]) ? default(long?) : long.Parse(x[1]));

            nameById = File.ReadAllLines(@"C:\servicetitan\name-by-id.csv")
                .Skip(1)
                .Select(x => x.Split(','))
                .ToDictionary(x => long.Parse(x[0]), x => x[1]);
        }

        public static string GetTenant(Report report)
        {
            if (string.IsNullOrEmpty(report.TenantPhone))
                throw new ArgumentNullException(nameof(report), $"Report {report.Sid} doesn't have tenant phone.");

            if (report.TenantPhone == "tulsa" || report.TenantPhone == "okc")
                return report.TenantPhone;
            var phone = GetPhone(report.TenantPhone);
            return GetTenantByPhone(phone);
        }

        private static string GetTenantByPhone(string phone)
        {
            if (tenantByPhone.TryGetValue(phone, out var tenantId) && tenantId.HasValue)
                return nameById[tenantId.Value];
            return null;
        }

        private static string GetPhone(string phone)
        {
            if (phone.StartsWith("+"))
                return phone;
            if (phone.StartsWith("sip:+"))
                return phone.Split('@')[0].Substring(4);
            return phone;
        }
    }
}