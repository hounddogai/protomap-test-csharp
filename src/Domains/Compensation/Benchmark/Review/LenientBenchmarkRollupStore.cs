namespace Acme.Workforce.Compensation.Benchmark.Review;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient benchmark rollups by code.</summary>
public interface ILenientBenchmarkRollupStore
{
    LenientBenchmarkRollup? Find(string code);
    void Save(LenientBenchmarkRollup item);
    bool Remove(string code);
    List<LenientBenchmarkRollup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientBenchmarkRollupStore(Dictionary<string, LenientBenchmarkRollup> items, List<string> order) : ILenientBenchmarkRollupStore
{
    public Dictionary<string, LenientBenchmarkRollup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientBenchmarkRollup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientBenchmarkRollup item)
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

    public List<LenientBenchmarkRollup> List()
    {
        var outValue = new List<LenientBenchmarkRollup>();
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
public sealed class CachedLenientBenchmarkRollupStore(MemoryLenientBenchmarkRollupStore inner, LenientBenchmarkRollupCache cache, int hits, int misses) : ILenientBenchmarkRollupStore
{
    public MemoryLenientBenchmarkRollupStore Inner { get; set; } = inner;
    public LenientBenchmarkRollupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientBenchmarkRollup? Find(string code)
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

    public void Save(LenientBenchmarkRollup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientBenchmarkRollup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient benchmark rollups. Empty fields match all.</summary>
public sealed record LenientBenchmarkRollupQuery(string Title, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(LenientBenchmarkRollup item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.ExpiresAt < MinExpiresAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientBenchmarkRollup> Apply(List<LenientBenchmarkRollup> items)
    {
        var matched = Collections.Filter(items, (LenientBenchmarkRollup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient benchmark rollup store.</summary>
public static class LenientBenchmarkRollupStore
{
    public static ILenientBenchmarkRollupStore NewMemoryLenientBenchmarkRollupStore()
    {
        return new MemoryLenientBenchmarkRollupStore(
            new Dictionary<string, LenientBenchmarkRollup>(),
            new List<string>());
    }

    public static ILenientBenchmarkRollupStore NewCachedLenientBenchmarkRollupStore(MemoryLenientBenchmarkRollupStore inner, int ttl, IClock clock)
    {
        return new CachedLenientBenchmarkRollupStore(
            inner,
            LenientBenchmarkRollupCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<LenientBenchmarkRollup> FindLenientBenchmarkRollups(ILenientBenchmarkRollupStore store, LenientBenchmarkRollupQuery query)
    {
        return query.Apply(store.List());
    }
}
