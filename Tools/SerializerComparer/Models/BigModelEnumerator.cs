using System.Collections;
using System.Collections.Generic;

namespace Tools.SerializerComparer.Models
{
    internal class BigModelEnumerator : IEnumerator<object>
    {
        private readonly BigModel first;
        private readonly Queue<BigModel> queue;
        private readonly HashSet<int> visited;

        public BigModelEnumerator(BigModel first)
        {
            this.first = first;
            visited = new HashSet<int>();
            queue = new Queue<BigModel>();
            Reset();
        }

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            if (queue.Count == 0)
            {
                Current = null;
                return false;
            }

            var c = queue.Dequeue();
            if (c.Left != null && visited.Add(c.Left.Number))
                queue.Enqueue(c.Left);
            if (c.Right != null && visited.Add(c.Right.Number))
                queue.Enqueue(c.Right);

            Current = c.Value;
            return true;
        }

        public void Reset()
        {
            visited.Clear();
            visited.Add(first.Number);

            queue.Clear();
            queue.Enqueue(first);

            Current = null;
        }

        public object Current { get; private set; }

        object IEnumerator.Current => Current;
    }
}