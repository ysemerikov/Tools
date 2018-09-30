using System;
using System.Collections.Generic;
using System.Linq;

namespace Tools.Core
{
    public class ArgumentReader
    {
        private readonly List<string> args;
        private readonly int from;
        private int position;

        public ArgumentReader(ICollection<string> args, int from = 0)
        {
            if (from < 0)
                throw new ArgumentException($"'from' ({from}) should be >= 0");
            if (from > args.Count)
                throw new ArgumentException($"'from' ({from}) should be <= length of arguments ({args.Count}).");

            this.args = args.Where(x => x != default).ToList();
            this.from = from;
            this.position = from;
        }

        public ArgumentReader GetNextReader()
        {
            return new ArgumentReader(args, position);
        }

        public string ReadNextStringOrDefault()
        {
            return position < args.Count ? args[position++] : default;
        }
    }
}