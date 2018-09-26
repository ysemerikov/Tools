using System;
using System.Diagnostics;
using System.Linq;
using Tools.SerializerComparer;

namespace Tools
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Action action;
            switch (args.FirstOrDefault())
            {
                case null:
                case nameof(SerializerComparer):
                    action = EntryPoint.Start;
                    break;
                case nameof(EndOfLinesFixer):
                    action = EndOfLinesFixer.Start;
                    break;
                default:
                    action = null;
                    break;
            }

            TimeCount(action);
        }

        private static void TimeCount(Action action)
        {
            var startNew = Stopwatch.StartNew();
            action();
            Console.WriteLine(startNew.Elapsed);
        }
    }
}