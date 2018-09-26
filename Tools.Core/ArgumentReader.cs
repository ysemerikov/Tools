using System;
using System.Collections;
using System.Collections.Generic;

namespace Tools.Core
{
    public class ArgumentReader : IReadOnlyCollection<string>
    {
        private readonly string[] args;

        public int Count => args.Length;

        public ArgumentReader(string[] args)
        {
            this.args = args;
        }

        public string GetString(int position, bool failIfOutOfRange = false)
        {
            if (position < args.Length)
                return args[position];

            if (failIfOutOfRange)
                throw new ArgumentOutOfRangeException();

            return default;
        }

        public IEnumerator<string> GetEnumerator()
        {
            return ((IEnumerable<string>) args).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}