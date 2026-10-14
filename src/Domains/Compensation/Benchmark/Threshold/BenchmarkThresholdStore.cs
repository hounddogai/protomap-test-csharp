namespace Acme.Workforce.Compensation.Benchmark.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps benchmark thresholds by code.</summary>
public interface IBenchmarkThresholdStore
{
    BenchmarkThreshold? Find(string code);
    void Save(BenchmarkThreshold item);
    bool Remove(string code);
    List<BenchmarkThreshold> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBenchmarkThresholdStore(Dictionary<string, BenchmarkThreshold> items, List<string> order) : IBenchmarkThresholdStore
{
    public Dictionary<string, BenchmarkThreshold> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BenchmarkThreshold? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BenchmarkThreshold item)
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

    public List<BenchmarkThreshold> List()
    {
        var outValue = new List<BenchmarkThreshold>();
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
public sealed class CachedBenchmarkThresholdStore(MemoryBenchmarkThresholdStore inner, BenchmarkThresholdCache cache, int hits, int misses) : IBenchmarkThresholdStore
{
    public MemoryBenchmarkThresholdStore Inner { get; set; } = inner;
    public BenchmarkThresholdCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BenchmarkThreshold? Find(string code)
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

    public void Save(BenchmarkThreshold item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BenchmarkThreshold> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects benchmark thresholds. Empty fields match all.</summary>
public sealed record BenchmarkThresholdQuery(string Label, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(BenchmarkThreshold item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BenchmarkThreshold> Apply(List<BenchmarkThreshold> items)
    {
        var matched = Collections.Filter(items, (BenchmarkThreshold candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of benchmark threshold store.</summary>
public static class BenchmarkThresholdStore
{
    public static IBenchmarkThresholdStore NewMemoryBenchmarkThresholdStore()
    {
        return new MemoryBenchmarkThresholdStore(new Dictionary<string, BenchmarkThreshold>(), new List<string>());
    }

    public static IBenchmarkThresholdStore NewCachedBenchmarkThresholdStore(MemoryBenchmarkThresholdStore inner, int ttl, IClock clock)
    {
        return new CachedBenchmarkThresholdStore(inner, BenchmarkThresholdCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<BenchmarkThreshold> FindBenchmarkThresholds(IBenchmarkThresholdStore store, BenchmarkThresholdQuery query)
    {
        return query.Apply(store.List());
    }
}
