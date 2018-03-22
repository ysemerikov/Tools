using System;
using System.Collections.Generic;
using System.Linq;

namespace RedisSaver
{
    public static class Params
    {
        public static string ConnectionString { get; private set; }
        public static int Database { get; private set; }
        public static TimeSpan AskEvery { get; private set; }
        public static string OutputFolder { get; private set; }
        public static TimeSpan ShutdownAfter { get; private set; }
        public static string QueueSubstring { get; private set; }
        public static int BatchSize { get; private set; }

        private static readonly List<(string FieldName, Action<string> Setter)> FieldSetters = new List<(string, Action<string>)>
        {
            (nameof(ConnectionString), s => ConnectionString = s),
            (nameof(Database), s => Database = int.Parse(s)),
            (nameof(AskEvery), s => AskEvery = ParseTimeSpan(s)),
            (nameof(OutputFolder), s => OutputFolder = s),
            (nameof(ShutdownAfter), s => ShutdownAfter = ParseTimeSpan(s)),
            (nameof(QueueSubstring), s => QueueSubstring = s),
            (nameof(BatchSize), s => BatchSize = int.Parse(s)),
        };

        private static TimeSpan ParseTimeSpan(string s)
        {
            var last = s.Last();
            if (last >= '0' && last <= '9')
                return TimeSpan.FromMilliseconds(int.Parse(s));

            var value = int.Parse(s.Substring(0, s.Length - 1));
            switch (last)
            {
                case 's':
                    return TimeSpan.FromSeconds(value);
                case 'm':
                    return TimeSpan.FromMinutes(value);
                case 'h':
                    return TimeSpan.FromHours(value);
                default:
                    throw new Exception($"Shutdown parameter '{s}' has wrong format.");
            }
        }

        public static void SetValue(string name, string value)
        {
            name = name.ToLower();
            FieldSetters.SingleOrDefault(fs => fs.FieldName.ToLower().StartsWith(name)).Setter(value);
        }

        public static string AsString()
        {
            return $"{nameof(ConnectionString)}='{ConnectionString}'\r\n" +
                   $"{nameof(Database)}='{Database}'\r\n" +
                   $"{nameof(AskEvery)}='{AskEvery}'\r\n" +
                   $"{nameof(OutputFolder)}='{OutputFolder}'\r\n" +
                   $"{nameof(ShutdownAfter)}='{ShutdownAfter}'\r\n" +
                   $"{nameof(QueueSubstring)}='{QueueSubstring}'\r\n" +
                   $"{nameof(BatchSize)}='{BatchSize}'"
                ;
        }
        
    }
}