using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RedisSaver
{
    internal class Worker
    {
        private readonly RedisStorage redisStorage;
        private readonly DiskStorage diskStorage;
        private readonly HashCache hashCache;
        private readonly List<string> newKeys = new List<string>();

        private readonly SemaphoreSlim semaphore;

        public Worker(RedisStorage redisStorage)
        {
            this.redisStorage = redisStorage;
            this.diskStorage = new DiskStorage(Params.OutputFolder);
            this.hashCache = new HashCache(3, capacity: 512 * 1024);
            
            this.semaphore = new SemaphoreSlim(1);
        }

        public void ExecuteCommand(string command)
        {
            if (command.StartsWith("changeAskEvery"))
            {
                var value = command.Split()[1];
                Params.SetValue(nameof(Params.AskEvery), value);
                Console.WriteLine($"AskEvery = '{Params.AskEvery}'");
            }
            else if (command.StartsWith("changeBatchSize"))
            {
                var value = command.Split()[1];
                Params.SetValue(nameof(Params.BatchSize), value);
                Console.WriteLine($"BatchSize = '{Params.BatchSize}'");
            }
            else if (command =="getTotal")
            {
                Console.WriteLine($"{diskStorage.Total}");
            }
            else if (command == "getFlushed")
            {
                Console.WriteLine($"{diskStorage.Flushed}");
            }
            else if (command == "getDiskQueueSize")
            {
                Console.WriteLine($"{diskStorage.DiskQueueSize}");
            } else if (command == "flush")
            {
                diskStorage.FlushAll();
                Console.WriteLine("Flushed");
            }
            else
            {
                Console.WriteLine($"Unknown command '{command}'");
            }
        }

        public async Task Do(DateTime shutdownTime)
        {
            var nextRun = DateTime.Now;
            while (nextRun < shutdownTime)
            {
                var delay = nextRun.Subtract(DateTime.Now);
                if (delay > TimeSpan.Zero)
                    await Task.Delay(delay);

                nextRun += Params.AskEvery;

                await Do();
            }

            diskStorage.FlushAll();
        }

        private async Task Do()
        {
            foreach (var queue in await redisStorage.GetQueues())
            {
                newKeys.Clear();

                var collection1 = await redisStorage.GetKeysFromQueue(queue);
                var collection2Task = redisStorage.GetKeysFromContentQueue(queue);

                newKeys.AddRange(collection1.Where(x => hashCache.Add(x)));

                var collection2 = await collection2Task;

                newKeys.AddRange(collection2.Where(x => hashCache.Add(x)));

                foreach (var key in newKeys)
                {
                    try
                    {
                        var data = await redisStorage.GetData(queue, key);
                        if (data != null)
                            diskStorage.Add(data);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }
        }
    }
}