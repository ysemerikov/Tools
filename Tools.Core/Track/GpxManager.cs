using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;
using Tools.Core.Track.Models;

namespace Tools.Core.Track
{
    public class GpxManager
    {
        public async Task Do(string filePath)
        {
            var gpx = ReadGpx(await File.ReadAllTextAsync(filePath));
            Console.WriteLine(gpx.Trk.Trkseg.Trkpts[0].Extensions.Elapsednanos);
        }

        private static Gpx ReadGpx(string xml)
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xml);
            var json = JsonConvert.SerializeXmlNode(xmlDocument["gpx"]);
            // Console.WriteLine(json.Substring(0, 2000));
            return JsonConvert.DeserializeObject<Wrapper>(json).Gpx;
        }
    }
}