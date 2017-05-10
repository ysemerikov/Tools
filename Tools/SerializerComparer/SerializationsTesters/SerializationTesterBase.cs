using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Tools.SerializerComparer.Models;
using Tools.SerializerComparer.Serializers;

namespace Tools.SerializerComparer.SerializationsTesters
{
    internal abstract class SerializationTesterBase<T> where T : IHasEquals<T>
    {
        public string SerializerName => serializer.Name;
        private readonly ISerializer<T> serializer;

        protected SerializationTesterBase(ISerializer<T> serializer)
        {
            this.serializer = serializer;
        }

        internal TestResult Test(List<T> models, bool checkEquals)
        {
            var data = default(List<SerializationData<T>>);
            try
            {
                data = models.Select(m => new SerializationData<T>(m)).ToList();

                var serializeResult = PerformanceTest(serializer, data, Serialize);

                var overageSize = data.Sum(d => d.Stream.Length)/data.Count;
                data.AsParallel().ForAll(d => d.Stream.Seek(0, SeekOrigin.Begin));

                var deserializeResult = PerformanceTest(serializer, data, Deserialize);

                if (checkEquals && AreEquals(data) == false)
                    throw new Exception("Models are not equals, " + serializer.Name);

                return new TestResult
                {
                    Serialize = serializeResult,
                    OverageSize = overageSize,
                    Deserialize = deserializeResult
                };
            }
            finally
            {
                data?.AsParallel().ForAll(d => d?.Dispose());
            }
        }

        private static bool AreEquals(List<SerializationData<T>> data)
        {
            return data.AsParallel().All(d => d.ModelsAreEquals());
        }

        protected abstract void Serialize(ISerializer<T> serializer, List<SerializationData<T>> data);
        protected abstract void Deserialize(ISerializer<T> serializer, List<SerializationData<T>> data);

        private static PerformanceResult PerformanceTest(ISerializer<T> serializer, List<SerializationData<T>> data,
            Action<ISerializer<T>, List<SerializationData<T>>> action)
        {
            var stopwatch = Stopwatch.StartNew();
            var performanceCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            performanceCounter.NextValue();

            action(serializer, data);

            var cpu = performanceCounter.NextValue();
            stopwatch.Stop();

            return new PerformanceResult
            {
                Elapsed = stopwatch.Elapsed,
                Cpu = cpu
            };
        }
    }
}