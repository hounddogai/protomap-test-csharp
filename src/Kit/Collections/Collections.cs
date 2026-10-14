namespace Acme.Workforce.Kit.Collections;

/// <summary>Functions of collections.</summary>
public static class Collections
{
    /// <summary>Returns the items keep accepts, in order.</summary>
    public static List<T> Filter<T>(List<T> items, Func<T, bool> keep)
    {
        var outValue = new List<T>();
        foreach (var item in items)
        {
            if (keep(item))
            {
                outValue.Add(item);
            }
        }
        return outValue;
    }

    public static Dictionary<string, int> CountBy<T>(List<T> items, Func<T, string> key)
    {
        var counts = new Dictionary<string, int>();
        foreach (var item in items)
        {
            counts[key(item)] = counts.GetValueOrDefault(key(item)) + 1;
        }
        return counts;
    }

    /// <summary>Groups items by key, keeping each group in input order.</summary>
    public static Dictionary<string, List<T>> GroupBy<T>(List<T> items, Func<T, string> key)
    {
        var groups = new Dictionary<string, List<T>>();
        foreach (var item in items)
        {
            if (!groups.TryGetValue(key(item), out var bucket))
            {
                bucket = new List<T>();
                groups[key(item)] = bucket;
            }
            bucket.Add(item);
        }
        return groups;
    }

    public static T? First<T>(List<T> items, Func<T, bool> pred)
    {
        foreach (var item in items)
        {
            if (pred(item))
            {
                return item;
            }
        }
        return default;
    }

    public static bool AnyMatch<T>(List<T> items, Func<T, bool> pred)
    {
        foreach (var item in items)
        {
            if (pred(item))
            {
                return true;
            }
        }
        return false;
    }

    public static bool AllMatch<T>(List<T> items, Func<T, bool> pred)
    {
        foreach (var item in items)
        {
            if (!pred(item))
            {
                return false;
            }
        }
        return true;
    }

    public static double SumBy<T>(List<T> items, Func<T, double> measure)
    {
        var total = 0.0;
        foreach (var item in items)
        {
            total += measure(item);
        }
        return total;
    }

    /// <summary>Splits items into batches of at most size, the last one shorter.</summary>
    public static List<List<T>> Chunk<T>(List<T> items, int size)
    {
        var chunks = new List<List<T>>();
        if (size <= 0)
        {
            return chunks;
        }
        var current = new List<T>();
        foreach (var item in items)
        {
            current.Add(item);
            if (current.Count == size)
            {
                chunks.Add(current);
                current = new List<T>();
            }
        }
        if (current.Count > 0)
        {
            chunks.Add(current);
        }
        return chunks;
    }

    /// <summary>Returns a copy of items ordered by score, highest first. The sort is stable.</summary>
    public static List<T> SortedBy<T>(List<T> items, Func<T, double> score)
    {
        var sorted = new List<T>(items);
        for (var i = 0; i < sorted.Count; i++)
        {
            var j = i;
            while (j > 0 && score(sorted[j - 1]) < score(sorted[j]))
            {
                var swap = sorted[j];
                sorted[j] = sorted[j - 1];
                sorted[j - 1] = swap;
                j = j - 1;
            }
        }
        return sorted;
    }

    /// <summary>Drops repeated values, keeping the first of each.</summary>
    public static List<string> UniqueStrings(List<string> values)
    {
        var seen = new Dictionary<string, bool>();
        var outValue = new List<string>();
        foreach (var valueValue in values)
        {
            if (seen.ContainsKey(valueValue))
            {
                continue;
            }
            seen[valueValue] = true;
            outValue.Add(valueValue);
        }
        return outValue;
    }

    public static List<T> Take<T>(List<T> items, int count)
    {
        var outValue = new List<T>();
        foreach (var item in items)
        {
            if (outValue.Count >= count)
            {
                break;
            }
            outValue.Add(item);
        }
        return outValue;
    }
}
