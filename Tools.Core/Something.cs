using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Twilio.Clients;
using Twilio.Rest.Api.V2010.Account;

namespace Tools.Core
{
    public enum CallDirection
    {
        /// <summary>Indicates an inbound call.</summary>
        Inbound,

        /// <summary>Indicates an outbound call.</summary>
        Outbound
    }

    public class Clazz
    {
        [DefaultValue(CallDirection.Inbound)]
        public CallDirection? Direction { get; set; }
    }

    public class Something : IAction
    {
        private static readonly JsonSerializer jsonSerializer = new JsonSerializer { DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate };

        public async Task Do()
        {
            using var stream = File.Open(@"C:\servicetitan\s.test.json", FileMode.Open);
            var ms = Deserialize(stream);
            Console.WriteLine(ms.Direction);
        }

        private static MemoryStream Serialize(Clazz data)
        {
            var stream = new MemoryStream();
            using (var writer = new StreamWriter(stream, Encoding.UTF8, 4096, true))
                jsonSerializer.Serialize(writer, data);
            stream.Position = 0;
            return stream;
        }

        private static Clazz Deserialize(Stream stream)
        {
            using (var reader = new StreamReader(stream, Encoding.UTF8, false, 4096, true))
            using (var jsonReader = new JsonTextReader(reader))
                return jsonSerializer.Deserialize<Clazz>(jsonReader);
        }

    }
}