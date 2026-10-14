namespace Acme.Workforce.Compensation.Benchmark.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps benchmark overrides by code.</summary>
public interface IBenchmarkOverrideStore
{
    BenchmarkOverride? Find(string code);
    void Save(BenchmarkOverride item);
    bool Remove(string code);
    List<BenchmarkOverride> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBenchmarkOverrideStore(Dictionary<string, BenchmarkOverride> items, List<string> order) : IBenchmarkOverrideStore
{
    public Dictionary<string, BenchmarkOverride> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BenchmarkOverride? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BenchmarkOverride item)
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

    public List<BenchmarkOverride> List()
    {
        var outValue = new List<BenchmarkOverride>();
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
public sealed class CachedBenchmarkOverrideStore(MemoryBenchmarkOverrideStore inner, BenchmarkOverrideCache cache, int hits, int misses) : IBenchmarkOverrideStore
{
    public MemoryBenchmarkOverrideStore Inner { get; set; } = inner;
    public BenchmarkOverrideCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BenchmarkOverride? Find(string code)
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

    public void Save(BenchmarkOverride item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BenchmarkOverride> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects benchmark overrides. Empty fields match all.</summary>
public sealed record BenchmarkOverrideQuery(string Region, double MinRating, List<string> Statuses, int Limit)
{
    public bool Matches(BenchmarkOverride item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.Rating < MinRating)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BenchmarkOverride> Apply(List<BenchmarkOverride> items)
    {
        var matched = Collections.Filter(items, (BenchmarkOverride candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of benchmark override store.</summary>
public static class BenchmarkOverrideStore
{
    public static IBenchmarkOverrideStore NewMemoryBenchmarkOverrideStore()
    {
        return new MemoryBenchmarkOverrideStore(new Dictionary<string, BenchmarkOverride>(), new List<string>());
    }

    public static IBenchmarkOverrideStore NewCachedBenchmarkOverrideStore(MemoryBenchmarkOverrideStore inner, int ttl, IClock clock)
    {
        return new CachedBenchmarkOverrideStore(inner, BenchmarkOverrideCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<BenchmarkOverride> FindBenchmarkOverrides(IBenchmarkOverrideStore store, BenchmarkOverrideQuery query)
    {
        return query.Apply(store.List());
    }
}
