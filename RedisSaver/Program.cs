using System;
using System.Linq;
using System.Threading.Tasks;

namespace RedisSaver
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            ParseArgs(args);
            using (var redisStorage = await PrepareRedisStorage())
            {
                if (redisStorage == null)
                    return;

                var startTime = DateTime.Now;
                Console.WriteLine($"Starting at {startTime}");

                await Run(startTime, redisStorage);

                var finishedTime = DateTime.Now;
                Console.WriteLine($"Finished at {finishedTime} ({finishedTime.Subtract(startTime)})");
            }
        }

        private static Task Run(DateTime startTime, RedisStorage redisStorage)
        {
            var worker = new Worker(redisStorage);
            Task.Run(() =>
            {
                for (;;)
                {
                    try
                    {
                        var line = Console.ReadLine();
                        if (!string.IsNullOrEmpty(line))
                            worker.ExecuteCommand(line);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Failed: {e.Message}");
                    }
                }
            });

            return worker.Do(startTime.Add(Params.ShutdownAfter));
        }

        private static async Task<RedisStorage> PrepareRedisStorage()
        {
            var cludge = new Cludge();

            await ProcessLine("params", cludge);
            Console.WriteLine("---------------------------------");
            await ProcessLine("queues", cludge);
            Console.WriteLine("---------------------------------");

            for (;;)
            {
                var line = Console.ReadLine()?.Trim();
                if (string.IsNullOrEmpty(line))
                    continue;
                try
                {
                    var b = await ProcessLine(line, cludge);
                    if (b.HasValue)
                        return b.Value ? cludge.RedisStorage : null;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Failed: {e.Message}");
                }

                Console.WriteLine("---------------------------------");
            }
        }

        private static async Task<bool?> ProcessLine(string line, Cludge cludge)
        {
            var strings = line.Split(new[] {' ', '\t', '\r', '\n'}, StringSplitOptions.RemoveEmptyEntries);
            if (strings.Length == 2)
            {
                Params.SetValue(strings[0], strings[1]);
                Console.WriteLine("Success");
                
                cludge.RedisStorage?.Dispose();
                cludge.RedisStorage = null;

                return null;
            }
            
            if(strings.Length == 1)
            {
                switch (strings[0].ToLower())
                {
                        case "params":
                            Console.WriteLine(Params.AsString());
                            break;
                        case "queues":
                            cludge.RedisStorage = cludge.RedisStorage ?? new RedisStorage(Params.ConnectionString, Params.Database, Params.QueueSubstring);
                            Console.WriteLine(string.Join(Environment.NewLine, (await cludge.RedisStorage.GetQueues()).OrderBy(x => (string) x)));
                            break;
                        case "go":
                            return true;
                        case "no":
                        case "break":
                        case "exit":
                            cludge.RedisStorage?.Dispose();
                            cludge.RedisStorage = null;
                            return false;
                        default:
                            Console.WriteLine($"Unknown command '{line}'");
                            break;
                }

                return null;
            }
            
            Console.WriteLine($"Unknown command '{line}'");
            return null;
        }

        private static void ParseArgs(string[] args)
        {
            Params.SetValue(nameof(Params.ConnectionString), "localhost");
            Params.SetValue(nameof(Params.Database), "2");
            Params.SetValue(nameof(Params.AskEvery), "100");
            Params.SetValue(nameof(Params.OutputFolder), "data");
            Params.SetValue(nameof(Params.ShutdownAfter), "30s");
            Params.SetValue(nameof(Params.QueueSubstring), "semerikov");
            Params.SetValue(nameof(Params.BatchSize), "999");

            for (var i = 0; i < args.Length; i += 2)
            {
                var name = args[i];
                var value = args[i + 1];
                Params.SetValue(name, value);
            }
        }

        private class Cludge
        {
            public RedisStorage RedisStorage;
        }
    }
}