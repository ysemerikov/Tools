using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace RedisSaver
{
    internal class RedisStorage : IDisposable
    {
        private const string Delimeter = "::";
        private const string Prefix = "redismq" + Delimeter;
        private readonly IDatabase database;
        private readonly string queueSubstring;

        private List<RedisKey> queues;
        private readonly ConnectionMultiplexer connection;

        public RedisStorage(string connectionString, int database, string queueSubstring)
        {
            var configurationOptions = ConfigurationOptions.Parse(connectionString);
            configurationOptions.AbortOnConnectFail = false;
            configurationOptions.SyncTimeout = 5000;

            connection = ConnectionMultiplexer.Connect(configurationOptions);
            connection.PreserveAsyncOrder = false;

            this.database = connection.GetDatabase(database);
            this.queueSubstring = queueSubstring.ToLower();
        }

        public async Task<IEnumerable<RedisKey>> GetQueues()
        {
            if (queues == null)
            {
                var result = await database.HashKeysAsync(Prefix + "queues");
                queues = result
                    .Select(x => (string) x)
                    .Where(x => !x.Contains("LongRunningTasks"))
                    .Where(x => x.ToLower().Contains(queueSubstring))
                    .Select(x => (RedisKey) (Prefix + x))
                    .ToList();
            }

            return queues.AsEnumerable();
        }

        public async Task<IEnumerable<string>> GetKeysFromQueue(RedisKey queue)
        {
            var result = await database.ListRangeAsync(queue, -1024);
            return result.Select(x => (string) x);
        }

        public async Task<IEnumerable<string>> GetKeysFromContentQueue(RedisKey queue)
        {
            var result = await database.ListRangeAsync((string) queue + Delimeter + "consumer", 0, 1024);
            return result.Select(x => (string) x);
        }

        public async Task<byte[]> GetData(RedisKey queue, string key)
        {
            var queueName = ((string) queue).Replace(Delimeter + "consumer", "");
            var result = await database.HashGetAsync(queueName + Delimeter + "content" + Delimeter + key, "body");
            return result;
        }

        public void Dispose()
        {
            connection.Dispose();
        }
    }
}