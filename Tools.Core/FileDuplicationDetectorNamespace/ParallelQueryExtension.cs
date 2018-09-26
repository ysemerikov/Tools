using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Tools.Core.FileDuplicationDetectorNamespace
{
    public static class ParallelQueryExtension
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