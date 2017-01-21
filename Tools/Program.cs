using System;
using System.Diagnostics;
using NUnit.Framework;

namespace Tools
{
    internal class Program
    {
        [Test]
        public void Test()
        {
            Main(null);
        }

        private static void Main(string[] args)
        {
            var startNew = Stopwatch.StartNew();
            EndOfLinesFixer.Start(args ?? new string[0]);
            Console.WriteLine(startNew.Elapsed);
        }
    }
}