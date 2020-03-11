namespace Tools.Core.Track.Models
{
    public class Gpx
    {
        public Extension Extensions { get; set; }
        public Metadata Metadata { get; set; }
        public Trk Trk { get; set; }

        public class Extension
        {
            public string AppVersion { get; set; }
            public int Tracks { get; set; }
        }
    }

    public class Metadata
    {
        public string Name { get; set; }
        public object Author { get; set; }
    }
}