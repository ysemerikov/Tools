using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools.SerializerComparer.Models;

namespace Tools.SerializerComparer.SerializationsTesters
{

    internal class SerializationData<T> : IDisposable
        where T : IHasEquals<T>
    {
        public T Model { get; }
        public Stream Stream { get; }
        public T DeserializedModel { get; set; }

        public SerializationData(T model)
        {
            Model = model;
            Stream = new MemoryStream();
        }

        public bool ModelsAreEquals()
        {
            return Model.IsEquals(DeserializedModel);
        }

        public void Dispose()
        {
            Stream?.Dispose();
        }
    }

    internal class PerformanceResult
    {
        public TimeSpan Elapsed { get; set; }
        public float Cpu { get; set; }
    }

    internal class TestResult
    {
        public PerformanceResult Serialize { get; set; }
        public long OverageSize { get; set; }
        public PerformanceResult Deserialize { get; set; }
    }
}
