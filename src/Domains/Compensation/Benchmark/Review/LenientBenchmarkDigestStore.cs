namespace Acme.Workforce.Compensation.Benchmark.Review;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient benchmark digests by code.</summary>
public interface ILenientBenchmarkDigestStore
{
    LenientBenchmarkDigest? Find(string code);
    void Save(LenientBenchmarkDigest item);
    bool Remove(string code);
    List<LenientBenchmarkDigest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientBenchmarkDigestStore(Dictionary<string, LenientBenchmarkDigest> items, List<string> order) : ILenientBenchmarkDigestStore
{
    public Dictionary<string, LenientBenchmarkDigest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientBenchmarkDigest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientBenchmarkDigest item)
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

    public List<LenientBenchmarkDigest> List()
    {
        var outValue = new List<LenientBenchmarkDigest>();
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
public sealed class CachedLenientBenchmarkDigestStore(MemoryLenientBenchmarkDigestStore inner, LenientBenchmarkDigestCache cache, int hits, int misses) : ILenientBenchmarkDigestStore
{
    public MemoryLenientBenchmarkDigestStore Inner { get; set; } = inner;
    public LenientBenchmarkDigestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientBenchmarkDigest? Find(string code)
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

    public void Save(LenientBenchmarkDigest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientBenchmarkDigest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient benchmark digests. Empty fields match all.</summary>
public sealed record LenientBenchmarkDigestQuery(string Source, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(LenientBenchmarkDigest item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.FloorValue < MinFloorValue)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientBenchmarkDigest> Apply(List<LenientBenchmarkDigest> items)
    {
        var matched = Collections.Filter(items, (LenientBenchmarkDigest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient benchmark digest store.</summary>
public static class LenientBenchmarkDigestStore
{
    public static ILenientBenchmarkDigestStore NewMemoryLenientBenchmarkDigestStore()
    {
        return new MemoryLenientBenchmarkDigestStore(
            new Dictionary<string, LenientBenchmarkDigest>(),
            new List<string>());
    }

    public static ILenientBenchmarkDigestStore NewCachedLenientBenchmarkDigestStore(MemoryLenientBenchmarkDigestStore inner, int ttl, IClock clock)
    {
        return new CachedLenientBenchmarkDigestStore(
            inner,
            LenientBenchmarkDigestCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<LenientBenchmarkDigest> FindLenientBenchmarkDigests(ILenientBenchmarkDigestStore store, LenientBenchmarkDigestQuery query)
    {
        return query.Apply(store.List());
    }
}
