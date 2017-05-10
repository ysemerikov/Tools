using System;
using System.Linq;
using Tools.SerializerComparer.Models;
using Tools.SerializerComparer.SerializationsTesters;
using Tools.SerializerComparer.Serializers;

namespace Tools.SerializerComparer
{
    public class PerformanceTester
    {
        private static ISerializer<T>[] GetSerializers<T>()
        {
            return new ISerializer<T>[]
            {
                new MsgSerializer<T>(),
                new SimpleBinnarySerializer<T>(),
                new DeflateBinnarySerializer<T>(),
                new SimpleDataContractSerializer<T>(),
                new DeflateDataContractSerializer<T>(),
                new Deflate2DataContractSerializer<T>()
            };
        }

        public static void Start()
        {
            var serializers = GetSerializers<SmallModel>();
            var models = SmallModel.Generate(64*1024).ToList();
            var countOfTests = 6;

            {
                var comparer =
                    new Comparer<SmallModel>(
                        serializers.Select(s => new SingleThreadSerializationTester<SmallModel>(s))
                        );

                // Warm Up
                comparer.Compare(models, 1, false);

                // Tests
                Console.WriteLine($"SingleThread, {countOfTests} tests, {models.Count} SmallModels");
                Console.WriteLine(comparer.Compare(models, countOfTests, false));
                Console.WriteLine();
            }


            {
                var comparer =
                    new Comparer<SmallModel>(
                        serializers.Select(s => new MultiThreadSerializationTester<SmallModel>(s))
                        );

                // Warm Up
                comparer.Compare(models, 1, false);

                // Tests
                Console.WriteLine($"MultiThread, {countOfTests} tests, {models.Count} SmallModels");
                Console.WriteLine(comparer.Compare(models, countOfTests, false));
                Console.WriteLine();
            }
        }
    }
}