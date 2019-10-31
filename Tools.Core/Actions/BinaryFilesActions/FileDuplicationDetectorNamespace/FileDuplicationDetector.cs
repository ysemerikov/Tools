using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Tools.Core.Logging;

namespace Tools.Core.FileDuplicationDetectorNamespace
{
    public class FileDuplicationDetector
    {
        private readonly ILogger logger;
        private readonly DirectoryInfo[] inputDirectories;
        private readonly DirectoryInfo outputDirectory;

        public FileDuplicationDetector(string[] inputDirectories, string outputDirectory, ILogger logger)
        {
            this.inputDirectories = inputDirectories.Select(x => new DirectoryInfo(x)).ToArray();
            this.outputDirectory = new DirectoryInfo(outputDirectory);
            this.logger = logger;

            if (!this.outputDirectory.Exists)
                this.outputDirectory.Create();
        }

        public async Task Detect()
        {
            var startedList = inputDirectories
                .SelectMany(x => x.EnumerateFiles("*", SearchOption.AllDirectories))
                .ToList();

            var duplicates = await SplitBy(file => Task.FromResult(file.Length), new[] {startedList});
            duplicates = await SplitBy(ByPart, duplicates);
            duplicates = await SplitBy(ByMd5, duplicates);
            duplicates = await SplitBy(BySha256, duplicates);

            WorkWithDuplicates(duplicates.ToList());
        }

        private async Task<IEnumerable<List<FileInfo>>> SplitBy<T>(Func<FileInfo, Task<T>> func, IEnumerable<List<FileInfo>> groups)
        {
            var counter = 0;
            var tasks = groups
                .Select(async list =>
                {
                    Interlocked.Add(ref counter, list.Count);
                    var fileAndHashs = await Task.WhenAll(list.Select(async x => (Value: x, Hash: await func(x))));
                    return fileAndHashs
                        .GroupBy(x => x.Hash, x => x.Value)
                        .Select(x => x.ToList())
                        .Where(l => l.Count > 1);
                });

            var result = await Task.WhenAll(tasks);

            logger.WriteLine($"Was {counter} files.", LoggerLevel.Debug);

            return result.SelectMany(x => x);
        }

        private static async Task<ByteArray> ByPart(FileInfo file)
        {
            const int offset = 512*1024;
            const int size = 1024;

            if (file.Length < size + offset)
                return ByteArray.Default;

            var result = new byte[size];
            using (var reader = file.OpenRead())
            {
                reader.Seek(offset, SeekOrigin.Begin);
                await reader.ReadAsync(result, 0, result.Length);
            }

            return new ByteArray(result);
        }

        private Task<ByteArray> ByMd5(FileInfo file)
        {

            return ByHash(file, MD5.Create);
        }

        private Task<ByteArray> BySha256(FileInfo file)
        {

            return ByHash(file, SHA256.Create);
        }

        private static async Task<ByteArray> ByHash<T>(FileInfo file, Func<T> getAlgo) where T : HashAlgorithm
        {
            const int maxBufferSize = 1024*1024;

            var bufferSize = (int) Math.Min(file.Length + 1, maxBufferSize);
            var buffer = new byte[bufferSize];
            var outputBuffer = new byte[buffer.Length];

            using (var hashAlgo = getAlgo())
            {
                using (var reader = file.OpenRead())
                {
                    int readed;
                    while ((readed = await reader.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        hashAlgo.TransformBlock(buffer, 0, readed, outputBuffer, 0);
                }

                hashAlgo.TransformFinalBlock(buffer, 0, 0);
                return new ByteArray(hashAlgo.Hash);
            }
        }

        private void WorkWithDuplicates(List<List<FileInfo>> duplicates)
        {
            var links = new List<string>();

            var counter = 0;
            for (var i = 0; i<duplicates.Count ;i++)
            {
                var list = duplicates[i];
                counter += list.Count;
                links.AddRange(CopyToDirectory(i.ToString("0000_"), list.OrderBy(f => f.FullName)));
            }

            logger.WriteLine($"Was {counter} files.", LoggerLevel.Debug);
            logger.WriteLine($"{duplicates.Count} duplicate groups.");

            logger.WriteLine(null, LoggerLevel.Debug);
            foreach (var link in links)
            {
                logger.WriteLine(link, LoggerLevel.Debug);
            }
        }

        private IEnumerable<string> CopyToDirectory(string prefix, IOrderedEnumerable<FileInfo> files)
        {
            var i = 0;
            foreach (var file in files)
            {
                var newFileName = $"{prefix}{i++}{file.Extension}";
                file.CopyTo(Path.Combine(outputDirectory.FullName, newFileName));

                yield return $"{file.FullName} -> {newFileName}";
            }
        }
    }
}