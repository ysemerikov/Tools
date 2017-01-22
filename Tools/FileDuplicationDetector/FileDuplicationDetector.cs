using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tools
{
    public class FileDuplicationDetector
    {
        private readonly bool writeLog;
        private readonly Counter counter;
        private readonly DirectoryInfo directory;
        private readonly DirectoryInfo checkDirectory;

        public FileDuplicationDetector(string path, string checkDirectoryPath, bool writeLog)
        {
            this.writeLog = writeLog;
            directory = new DirectoryInfo(path);
            checkDirectory = checkDirectoryPath == null ? null : new DirectoryInfo(checkDirectoryPath);
            counter = new Counter();
        }

        public static void Start()
        {
            Console.OutputEncoding = Encoding.UTF8;

            var detector = new FileDuplicationDetector(@"D:\from3Tb\Photos", @"D:\from3Tb\ToCheck", true);
//            var detector = new FileDuplicationDetector(@"D:\from3Tb\Photos", null, true);
            detector.Detect().Wait();
        }

        public async Task Detect()
        {
            var parallelQuery = directory
                .EnumerateFiles("*", SearchOption.AllDirectories)
                .AsParallel()
                .GroupBy(f => f.Length)
                .Select(g =>
                {
                    var list = g.ToList();
                    Interlocked.Add(ref counter.Total, list.Count);
                    return list;
                })
                .Where(g => g.Count > 1);

            var tasks = parallelQuery
                .Split(ByPart, ByteArrayComparer.Instanse, false)
                .Split(ByMd5, ByteArrayComparer.Instanse, true)
                .Split(BySha256, ByteArrayComparer.Instanse, true)
                .Select(async list =>
                {
                    if (list.Count == 2 && !await IsEquals(list[0], list[1]))
                        return null;

                    return list;
                });

            var duplicates = (await Task.WhenAll(tasks))
                .Where(l => l != null);

            var duplicateCounter = WorkWithDuplicates(duplicates);

            Console.WriteLine(counter.ToString());
            Console.WriteLine($"{nameof(duplicateCounter)}: {duplicateCounter}");
        }

        private async Task<byte[]> ByPart(FileInfo file)
        {
            const int offset = 512*1024;
            const int size = 1024;

            if (file.Length < size + offset)
                return ByteArrayComparer.DefaultElement;

            var result = new byte[size];
            using (var reader = file.OpenRead())
            {
                reader.Seek(offset, SeekOrigin.Begin);
                await reader.ReadAsync(result, 0, result.Length);
            }
            Interlocked.Increment(ref counter.ByPart);
            return result;
        }

        private Task<byte[]> ByMd5(FileInfo file)
        {
            Interlocked.Increment(ref counter.ByMd5);

            return ByHash(file, MD5.Create);
        }

        private Task<byte[]> BySha256(FileInfo file)
        {
            Interlocked.Increment(ref counter.BySha256);

            return ByHash(file, SHA256.Create);
        }

        private static async Task<byte[]> ByHash<T>(FileInfo file, Func<T> getAlgo) where T : HashAlgorithm
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
                return hashAlgo.Hash;
            }
        }

        private async Task<bool> IsEquals(FileInfo file1, FileInfo file2)
        {
            Interlocked.Add(ref counter.ByEquals, 2);

            using (var reader1 = file1.OpenRead())
            using (var reader2 = file2.OpenRead())
            {
                while (reader1.Position < reader1.Length && reader1.Position < reader1.Length)
                {
                    var buffer1 = new byte[128*1024];
                    var buffer2 = new byte[buffer1.Length];

                    var task = reader1.ReadAsync(buffer1, 0, buffer1.Length);
                    var c2 = await reader2.ReadAsync(buffer2, 0, buffer2.Length);
                    var c1 = await task;

                    if (c2 != c1)
                        throw new Exception($"c1 {c1} != c2 {c2}");

                    for (var i = 0; i < c1; i++)
                        if (buffer1[i] != buffer2[i])
                            return false;
                }
            }

            return true;
        }

        private int WorkWithDuplicates(IEnumerable<List<FileInfo>> duplicates)
        {
            var total = 0;
            if (checkDirectory != null)
            {
                var i = 0;
                foreach (var list in duplicates)
                {
                    total += list.Count - 1;
                    CopyToDirectory(i.ToString("0000_"), list.OrderBy(f => f.FullName), checkDirectory);
                    i++;
                }
            }
            else
            {
                foreach (var list in duplicates)
                {
                    total += list.Count - 1;
                    Remove(list.OrderBy(f => f.FullName).Skip(1));
                }
            }

            return total;
        }

        private void CopyToDirectory(string prefix, IOrderedEnumerable<FileInfo> files, DirectoryInfo directoryInfo)
        {
            var extension = default(string);
            var i = 0;
            foreach (var file in files)
            {
                if (extension == null)
                    extension = file.Extension;

                if (writeLog)
                    Console.WriteLine(file.FullName);

                file.CopyTo(Path.Combine(directoryInfo.FullName, $"{prefix}{i++}{extension}"));
            }

            if (writeLog)
                Console.WriteLine();
        }

        private void Remove(IEnumerable<FileInfo> files)
        {
            foreach (var file in files)
            {
                if (writeLog)
                    Console.WriteLine($"RM {file.FullName}");

                file.Delete();
            }

            if (writeLog)
                Console.WriteLine();
        }

        private class Counter
        {
            public int ByEquals;
            public int ByMd5;
            public int ByPart;
            public int BySha256;
            public int Total;

            public override string ToString()
            {
                var sb = new StringBuilder();
                sb.AppendLine($"{nameof(Total)}: {Total}");
                sb.AppendLine($"{nameof(ByPart)}: {ByPart}");
                sb.AppendLine($"{nameof(ByMd5)}: {ByMd5}");
                sb.AppendLine($"{nameof(BySha256)}: {BySha256}");
                sb.AppendLine($"{nameof(ByEquals)}: {ByEquals}");
                return sb.ToString();
            }
        }
    }

    public static class LinqExtension
    {
        public static ParallelQuery<List<FileInfo>> Split<T>(this ParallelQuery<List<FileInfo>> groups,
            Func<FileInfo, Task<T>> keySelector, IEqualityComparer<T> comparer, bool skipIfPair)
        {
            var tasks = groups
                .Select(async g =>
                {
                    if (g.Count == 2 && skipIfPair)
                        return new[] {g};

                    var res = await Task.WhenAll(g.Select(async f =>
                    {
                        var key = await keySelector(f);
                        return new {Key = key, File = f};
                    }));

                    return res.GroupBy(x => x.Key, x => x.File, comparer)
                        .Select(x => x.ToList())
                        .Where(x => x.Count > 1);
                })
                .ToArray();

            Task.WaitAll(tasks);

            return tasks
                .AsParallel()
                .Select(t => t.Result)
                .SelectMany(g => g);
        }
    }
}