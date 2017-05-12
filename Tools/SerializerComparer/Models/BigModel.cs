using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Tools.SerializerComparer.Models
{
    [Serializable]
    [DataContract]
    public class BigModel : IHasEquals<BigModel>, IEnumerable<object>
    {
        [DataMember]
        public BigModel Left { get; set; }

        [DataMember]
        public BigModel Right { get; set; }

        [DataMember]
        public int Number { get; set; }

        [DataMember]
        public object Value { get; set; }

        public IEnumerator<object> GetEnumerator()
        {
            return new BigModelEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool IsEquals(BigModel other)
        {
            return this.Zip(other, (a, b) => new {A = a, B = b}).All(x => AreEquals(x.A, x.B));
        }

        private static bool AreEquals(object a, object b)
        {
            if (a == null || b == null)
                return false;

            if (a is int && b is int)
                return (int) a == (int) b;
            if (a is double && b is double)
                return (double) a == (double) b;

            if (a is string && b is string)
                return (string) a == (string) b;

            var modelA = a as SmallModel;
            var modelB = b as SmallModel;
            if (modelA != null && modelB != null)
                return modelA.IsEquals(modelB);

            return false;
        }

        public static IEnumerable<BigModel> Generate(int count)
        {
            var rand = new Random();
            for (var i = 0; i < count; i++)
            {
                switch (i%3)
                {
                    case 0:
                        yield return GenerateGraph(rand);
                        break;
                    case 1:
                        yield return GenerateCircle(rand);
                        break;
                    case 2:
                        yield return GenerateGraphWithCycle(rand);
                        break;
                }
            }
        }

        private static BigModel GenerateGraph(Random rand)
        {
            var queue = new Queue<BigModel>();
            var first = GenerateBigModelWithValue(rand, 0);
            queue.Enqueue(first);

            var count = rand.Next(500, 1000);
            for (var i = queue.Count; i < count;)
            {
                var nextRandomInt = rand.Next(0, 10);

                var v = queue.Dequeue();
                if (nextRandomInt != 5)
                {
                    v.Left = GenerateBigModelWithValue(rand, i);
                    queue.Enqueue(v.Left);
                    i++;
                }

                if (nextRandomInt != 6)
                {
                    v.Right = GenerateBigModelWithValue(rand, i);
                    queue.Enqueue(v.Right);
                    i++;
                }
            }

            return first;
        }

        private static BigModel GenerateGraphWithCycle(Random rand)
        {
            var first = GenerateGraph(rand);
            var second = default(BigModel);
            var last = first;
            while (last.Left != null && last.Right != null)
            {
                if (rand.Next(0, 2) == 0)
                    last = last.Left ?? last.Right;
                else
                    last = last.Right ?? last.Left;

                second = second ?? last;
            }

            if (rand.Next(0, 2) == 1)
                last.Left = first;
            else
                last.Right = second;
            return first;
        }

        private static BigModel GenerateCircle(Random rand)
        {
            var first = GenerateBigModelWithValue(rand, 0);
            var current = first;

            var count = rand.Next(500, 1000);
            for (var i = 1; i < count; i++)
            {
                current.Left = GenerateBigModelWithValue(rand, i);
                current = current.Left;
            }

            current.Left = first;

            return first;
        }

        private static BigModel GenerateBigModelWithValue(Random rand, int n)
        {
            object value;
            switch (rand.Next(0, 10))
            {
                case 0:
                    value = SmallModel.RandomString(rand, rand.Next(128, 256));
                    break;
                case 1:
                    value = SmallModel.Generate(1).First();
                    break;
                case 2:
                    value = rand.NextDouble();
                    break;
                default:
                    value = rand.Next();
                    break;
            }
            return new BigModel
            {
                Value = value
            };
        }
    }
}