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
            Main();
        }

        private static void Main()
        {
            var extensions = new[] {"cs", "config", "csproj", "js", "cshtml", "resx"};
            var startNew = Stopwatch.StartNew();
            EndOfLinesFixer.Start(extensions, true, @"C:\servicetitan\app");
            Console.WriteLine(startNew.Elapsed);
        }
    }
}