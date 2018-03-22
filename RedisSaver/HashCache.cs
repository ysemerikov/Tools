using System.Collections.Generic;
using System.Linq;

namespace RedisSaver
{
    public class HashCache
    {
        private readonly int capacity;
        private readonly HashSet<string>[] hashSets;
        private int current;

        public HashCache(int countOfSets, int capacity)
        {
            this.capacity = capacity;

            hashSets = new HashSet<string>[countOfSets];
            for (var i = 0; i < hashSets.Length; i++) hashSets[i] = new HashSet<string>();
        }

        public bool Add(string key)
        {
            if (hashSets.Any(hs => hs.Contains(key)))
                return false;

            if (hashSets[current].Count >= capacity)
            {
                current = (current + 1) % hashSets.Length;
                if (hashSets[current].Count >= capacity)
                    hashSets[current].Clear();
            }

            hashSets[current].Add(key);
            return true;
        }
    }
}