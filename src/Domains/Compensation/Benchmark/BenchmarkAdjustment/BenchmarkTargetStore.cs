namespace Acme.Workforce.Compensation.Benchmark.BenchmarkAdjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps benchmark targets by code.</summary>
public interface IBenchmarkTargetStore
{
    BenchmarkTarget? Find(string code);
    void Save(BenchmarkTarget item);
    bool Remove(string code);
    List<BenchmarkTarget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBenchmarkTargetStore(Dictionary<string, BenchmarkTarget> items, List<string> order) : IBenchmarkTargetStore
{
    public Dictionary<string, BenchmarkTarget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BenchmarkTarget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BenchmarkTarget item)
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

    public List<BenchmarkTarget> List()
    {
        var outValue = new List<BenchmarkTarget>();
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
public sealed class CachedBenchmarkTargetStore(MemoryBenchmarkTargetStore inner, BenchmarkTargetCache cache, int hits, int misses) : IBenchmarkTargetStore
{
    public MemoryBenchmarkTargetStore Inner { get; set; } = inner;
    public BenchmarkTargetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BenchmarkTarget? Find(string code)
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

    public void Save(BenchmarkTarget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BenchmarkTarget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects benchmark targets. Empty fields match all.</summary>
public sealed record BenchmarkTargetQuery(string Locale, int MinAttempts, List<string> Statuses, int Limit)
{
    public bool Matches(BenchmarkTarget item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
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

    public List<BenchmarkTarget> Apply(List<BenchmarkTarget> items)
    {
        var matched = Collections.Filter(items, (BenchmarkTarget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of benchmark target store.</summary>
public static class BenchmarkTargetStore
{
    public static IBenchmarkTargetStore NewMemoryBenchmarkTargetStore()
    {
        return new MemoryBenchmarkTargetStore(new Dictionary<string, BenchmarkTarget>(), new List<string>());
    }

    public static IBenchmarkTargetStore NewCachedBenchmarkTargetStore(MemoryBenchmarkTargetStore inner, int ttl, IClock clock)
    {
        return new CachedBenchmarkTargetStore(inner, BenchmarkTargetCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<BenchmarkTarget> FindBenchmarkTargets(IBenchmarkTargetStore store, BenchmarkTargetQuery query)
    {
        return query.Apply(store.List());
    }
}
