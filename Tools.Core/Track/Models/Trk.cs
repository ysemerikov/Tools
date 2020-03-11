using System.Collections.Generic;
using Newtonsoft.Json;

namespace Tools.Core.Track.Models
{
    public class Trk
    {
        public string Name { get; set; }
        public Extension Extensions { get; set; }
        public TrksegElement Trkseg { get; set; }

        public class Extension
        {
            [JsonProperty("entries")] public int EntriesCount { get; set; }
        }

        public class TrksegElement
        {
            [JsonProperty("trkpt")]
            public List<Trkpt> Trkpts { get; set; }
        }
    }
}