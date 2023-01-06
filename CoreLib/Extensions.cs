namespace CoreLib;

public static class Extensions
{
    public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
    {
        foreach (var e in enumerable)
        {
            action(e);
        }
    }

    public static IEnumerable<(TK Key, TE1[] FirstElems, TE2[] SecondElems)> FullOuterGroupJoin<TE1, TE2, TK>(
        this IEnumerable<TE1> enumerable1,
        IEnumerable<TE2> enumerable2,
        Func<TE1, TK> keySelector1,
        Func<TE2, TK> keySelector2)
        where TK : notnull
    {
        var dict1 = enumerable1
            .GroupBy(keySelector1)
            .ToDictionary(x => x.Key, x => x.ToArray());

        foreach (var g2 in enumerable2.GroupBy(keySelector2))
        {
            var key = g2.Key;

            if (!dict1.Remove(key, out var a1))
            {
                a1 = Array.Empty<TE1>();
            }

            yield return (key, a1, g2.ToArray());
        }

        foreach (var kv1 in dict1)
        {
            yield return (kv1.Key, kv1.Value, Array.Empty<TE2>());
        }
    }

    public static T EnsureDefined<T>(this T value, [CallerArgumentExpression("value")] string? paramName = null)
        where T : Enum =>
        Enum.IsDefined(typeof(T), value)
            ? value
            : throw new ArgumentOutOfRangeException(
                paramName,
                value,
                $"The value of type {typeof(T)} isn't defined.");

    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> enumerable)
    {
        return enumerable
            .Where(x => x != null)
            .Select(x => x!);
    }
}
