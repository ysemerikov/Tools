using System;
using Newtonsoft.Json;

namespace Tools.Core.Track.Models
{
    public class Trkpt
    {
        [JsonProperty("@lat")]
        public string Lat { get; set; }
        [JsonProperty("@lon")]
        public string Lon { get; set; }
        public string Ele { get; set; }
        [JsonProperty("time")]
        public DateTime Date { get; set; }
        public string Src { get; set; }
        public string Type { get; set; }
        public Extension Extensions { get; set; }

        public class Extension
        {
            public string Accuracy { get; set; }
            public string Elapsednanos { get; set; }
            public string Speed { get; set; }
            public string Bearing { get; set; }
        }
    }
}