using System;
using System.Collections.Generic;
using System.Linq;
using Tools.SerializerComparer.Models;
using Tools.SerializerComparer.SerializationsTesters;
using Tools.SerializerComparer.Serializers;

namespace Tools.SerializerComparer
{
    internal class PerformanceTester<T> where T:IHasEquals<T>
    {
        private static ISerializer<T>[] GetSerializers()
        {
            return new ISerializer<T>[]
            {
                new SimpleGroBuffSerializer<T>(),
//                new ProtobufSerializer<T>(), 
//                new MsgSerializer<T>(),
                new SimpleBinnarySerializer<T>(),
//                new DeflateBinnarySerializer<T>(),
//                new SimpleDataContractSerializer<T>(),
//                new DeflateDataContractSerializer<T>(),
//                new Deflate2DataContractSerializer<T>()
            };
        }

        public static void Start(List<T> models, int countOfTests, bool checkEquals = true)
        {
            var serializers = GetSerializers();

            {
                var comparer =
                    new Comparer<T>(
                        serializers.Select(s => new SingleThreadSerializationTester<T>(s))
                        );

                // Warm Up
                comparer.Compare(models, 1, checkEquals);

                // Tests
                Console.WriteLine($"SingleThread, {countOfTests} tests, {models.Count} {typeof (T).Name}s");
                Console.WriteLine(comparer.Compare(models, countOfTests, false));
                Console.WriteLine();
            }


            {
                var comparer =
                    new Comparer<T>(
                        serializers.Select(s => new MultiThreadSerializationTester<T>(s))
                        );

                // Warm Up
                comparer.Compare(models, 1, checkEquals);

                // Tests
                Console.WriteLine($"MultiThread, {countOfTests} tests, {models.Count} {typeof(T).Name}s");
                Console.WriteLine(comparer.Compare(models, countOfTests, false));
                Console.WriteLine();
            }
        }
    }
}