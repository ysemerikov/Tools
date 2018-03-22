using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;

namespace RedisSaver
{
    internal class DiskStorage
    {
        private readonly DirectoryInfo directory;
        private readonly ConcurrentQueue<byte[]> queue = new ConcurrentQueue<byte[]>();
        private readonly Task saveTask;
        public int Flushed;

        public int Total;

        public DiskStorage(string outputFolder)
        {
            directory = new DirectoryInfo(outputFolder);
            if (!directory.Exists)
                directory.Create();

            saveTask = Task.Run(SaveForeverSafely);
        }

        public int DiskQueueSize => queue.Count;

        public void Add(byte[] data)
        {
            Interlocked.Increment(ref Total);
            queue.Enqueue(data);
        }

        public void FlushAll()
        {
            Flush(true);
        }

        private async Task SaveForeverSafely()
        {
            for (;;)
                try
                {
                    await SaveForever();
                    break;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
        }

        private async Task SaveForever()
        {
            for (;;)
                if (!Flush(false))
                    await Task.Delay(200);
        }

        private bool Flush(bool all)
        {
            var batchSize = Params.BatchSize;

            if (!all && queue.Count < batchSize)
                return false;

            var any = false;
            lock (queue)
            {
                while (!queue.IsEmpty && (all || queue.Count >= batchSize))
                {
                    var batch = GetBatch(queue, batchSize);
                    SaveToStorage(batch);
                    any = true;

                    if (batch.Count < batchSize)
                        break;
                }
            }

            return any;
        }

        private static List<byte[]> GetBatch(ConcurrentQueue<byte[]> source, int batchSize)
        {
            var list = new List<byte[]>(batchSize);
            while (!source.IsEmpty && list.Count < batchSize)
                if (source.TryDequeue(out var bytes))
                    list.Add(bytes);

            return list;
        }

        private void SaveToStorage(List<byte[]> batch)
        {
            var fileName = $"{DateTime.Now:s}_{batch.Count}_{Guid.NewGuid()}.bin".Replace(':', '-');
            var fileInfo = new FileInfo(Path.Combine(directory.FullName, fileName));
            if (fileInfo.Exists)
                return;
            using (var stream = fileInfo.OpenWrite())
            {
                var bf = new BinaryFormatter();
                bf.Serialize(stream, batch);
            }

            Interlocked.Add(ref Flushed, batch.Count);
        }
    }
}