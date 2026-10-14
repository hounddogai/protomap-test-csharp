namespace Acme.Workforce.Compensation.Benchmark.Entry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps benchmark entries by code.</summary>
public interface IBenchmarkEntryStore
{
    BenchmarkEntry? Find(string code);
    void Save(BenchmarkEntry item);
    bool Remove(string code);
    List<BenchmarkEntry> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBenchmarkEntryStore(Dictionary<string, BenchmarkEntry> items, List<string> order) : IBenchmarkEntryStore
{
    public Dictionary<string, BenchmarkEntry> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BenchmarkEntry? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BenchmarkEntry item)
    {
        if (!Items.ContainsKey(item.Code))
        {
            Order.Add(item.Code);
        }
        Items[item.Code] = item;
    }

    public bool Remove(string code)
    {
        if (!Items.ContainsKey(code))
        {
            return false;
        }
        Items.Remove(code);
        var kept = new List<string>();
        foreach (var existing in Order)
        {
            if (existing != code)
            {
                kept.Add(existing);
            }
        }
        Order = kept;
        return true;
    }

    public List<BenchmarkEntry> List()
    {
        var outValue = new List<BenchmarkEntry>();
        foreach (var key in Order)
        {
            if (!Items.TryGetValue(key, out var found))
            {
                continue;
            }
            outValue.Add(found);
        }
        return outValue;
    }
}

/// <summary>Serves reads from a short-lived cache in front of another store.</summary>
public sealed class CachedBenchmarkEntryStore(MemoryBenchmarkEntryStore inner, BenchmarkEntryCache cache, int hits, int misses) : IBenchmarkEntryStore
{
    public MemoryBenchmarkEntryStore Inner { get; set; } = inner;
    public BenchmarkEntryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BenchmarkEntry? Find(string code)
    {
        if (Cache.Get(code) is not { } hit)
        {
            Misses += 1;
            if (Inner.Find(code) is not { } found)
            {
                return null;
            }
            Cache.Put(code, found);
            return found;
        }
        Hits += 1;
        return hit;
    }

    public void Save(BenchmarkEntry item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BenchmarkEntry> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects benchmark entries. Empty fields match all.</summary>
public sealed record BenchmarkEntryQuery(string Title, int MinAttempts, List<string> Statuses, int Limit)
{
    public bool Matches(BenchmarkEntry item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Attempts < MinAttempts)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BenchmarkEntry> Apply(List<BenchmarkEntry> items)
    {
        var matched = Collections.Filter(items, (BenchmarkEntry candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of benchmark entry store.</summary>
public static class BenchmarkEntryStore
{
    public static IBenchmarkEntryStore NewMemoryBenchmarkEntryStore()
    {
        return new MemoryBenchmarkEntryStore(new Dictionary<string, BenchmarkEntry>(), new List<string>());
    }

    public static IBenchmarkEntryStore NewCachedBenchmarkEntryStore(MemoryBenchmarkEntryStore inner, int ttl, IClock clock)
    {
        return new CachedBenchmarkEntryStore(inner, BenchmarkEntryCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<BenchmarkEntry> FindBenchmarkEntries(IBenchmarkEntryStore store, BenchmarkEntryQuery query)
    {
        return query.Apply(store.List());
    }
}
