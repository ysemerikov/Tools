using System;

namespace Tools.Core
{
    public class ArgumentReader
    {
        private readonly string[] args;

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
    }
}