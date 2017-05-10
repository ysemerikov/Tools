using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tools.SerializerComparer.Models;
using Tools.SerializerComparer.SerializationsTesters;

namespace Tools.SerializerComparer
{
    internal class Comparer<T> where T : IHasEquals<T>
    {
        private readonly List<SerializationTesterBase<T>> testers;

        public Comparer(IEnumerable<SerializationTesterBase<T>> testers)
        {
            this.testers = testers.ToList();
        }

        internal Result Compare(List<T> models, int countOfTests, bool checkEquals)
        {
            var dict = new Dictionary<string, List<TestResult>>();

            for (var i = 0; i < countOfTests; i++)
            {
                foreach (var tester in testers)
                {
                    List<TestResult> list;
                    if (!dict.TryGetValue(tester.SerializerName, out list))
                        dict[tester.SerializerName] = list = new List<TestResult>();

                    list.Add(tester.Test(models, checkEquals));
                }
            }

            var results = dict.ToDictionary(
                x => x.Key,
                x => Average(x.Value)
                );

            return new Result
            {
                Results = results
            };
        }

        private static TestResult Average(List<TestResult> results)
        {
            var overageSize = 0L;
            var serializeElapsed = 0L;
            var serializeCpu = 0f;
            var deserializeElapsed = 0L;
            var deserializeCpu = 0f;

            foreach (var result in results)
            {
                overageSize += result.OverageSize;

                serializeElapsed += result.Serialize.Elapsed.Ticks;
                serializeCpu += result.Serialize.Cpu;

                deserializeElapsed += result.Deserialize.Elapsed.Ticks;
                deserializeCpu += result.Deserialize.Cpu;
            }

            var count = results.Count;
            return new TestResult
            {
                OverageSize = overageSize/count,
                Serialize = new PerformanceResult
                {
                    Elapsed = TimeSpan.FromTicks(serializeElapsed/count),
                    Cpu = serializeCpu/count
                },
                Deserialize = new PerformanceResult
                {
                    Elapsed = TimeSpan.FromTicks(deserializeElapsed/count),
                    Cpu = deserializeCpu/count
                }
            };
        }

        internal class Result
        {
            internal Dictionary<string, TestResult> Results { get; set; }

            public override string ToString()
            {
                var sb = new StringBuilder();
                var minLength = Results.Keys.Min(k => k.Length);

                foreach (var kv in Results)
                {
                    sb.AppendLine($"{kv.Key.Substring(0, minLength)}\t obj {kv.Value.OverageSize} bytes\t" +
                                  $"SER:{kv.Value.Serialize.Elapsed.TotalSeconds.ToString("F")}s \t{kv.Value.Serialize.Cpu.ToString("F")}% CPU\t" +
                                  $"DES:{kv.Value.Deserialize.Elapsed.TotalSeconds.ToString("F")}s \t{kv.Value.Deserialize.Cpu.ToString("F")}% CPU"
                        );
                }

                return sb.ToString();
            }
        }
    }
}