namespace Acme.Workforce.Compensation.Benchmark.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps benchmark digests by code.</summary>
public interface IBenchmarkDigestStore
{
    BenchmarkDigest? Find(string code);
    void Save(BenchmarkDigest item);
    bool Remove(string code);
    List<BenchmarkDigest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBenchmarkDigestStore(Dictionary<string, BenchmarkDigest> items, List<string> order) : IBenchmarkDigestStore
{
    public Dictionary<string, BenchmarkDigest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BenchmarkDigest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BenchmarkDigest item)
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

    public List<BenchmarkDigest> List()
    {
        var outValue = new List<BenchmarkDigest>();
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
public sealed class CachedBenchmarkDigestStore(MemoryBenchmarkDigestStore inner, BenchmarkDigestCache cache, int hits, int misses) : IBenchmarkDigestStore
{
    public MemoryBenchmarkDigestStore Inner { get; set; } = inner;
    public BenchmarkDigestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BenchmarkDigest? Find(string code)
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

    public void Save(BenchmarkDigest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BenchmarkDigest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects benchmark digests. Empty fields match all.</summary>
public sealed record BenchmarkDigestQuery(string Title, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(BenchmarkDigest item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Cost < MinCost)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BenchmarkDigest> Apply(List<BenchmarkDigest> items)
    {
        var matched = Collections.Filter(items, (BenchmarkDigest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of benchmark digest store.</summary>
public static class BenchmarkDigestStore
{
    public static IBenchmarkDigestStore NewMemoryBenchmarkDigestStore()
    {
        return new MemoryBenchmarkDigestStore(new Dictionary<string, BenchmarkDigest>(), new List<string>());
    }

    public static IBenchmarkDigestStore NewCachedBenchmarkDigestStore(MemoryBenchmarkDigestStore inner, int ttl, IClock clock)
    {
        return new CachedBenchmarkDigestStore(inner, BenchmarkDigestCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<BenchmarkDigest> FindBenchmarkDigests(IBenchmarkDigestStore store, BenchmarkDigestQuery query)
    {
        return query.Apply(store.List());
    }
}
