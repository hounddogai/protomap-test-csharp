namespace Acme.Workforce.Compensation.Benchmark.Milestone;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps benchmark windows by code.</summary>
public interface IBenchmarkWindowStore
{
    BenchmarkWindow? Find(string code);
    void Save(BenchmarkWindow item);
    bool Remove(string code);
    List<BenchmarkWindow> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBenchmarkWindowStore(Dictionary<string, BenchmarkWindow> items, List<string> order) : IBenchmarkWindowStore
{
    public Dictionary<string, BenchmarkWindow> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BenchmarkWindow? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BenchmarkWindow item)
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

    public List<BenchmarkWindow> List()
    {
        var outValue = new List<BenchmarkWindow>();
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
public sealed class CachedBenchmarkWindowStore(MemoryBenchmarkWindowStore inner, BenchmarkWindowCache cache, int hits, int misses) : IBenchmarkWindowStore
{
    public MemoryBenchmarkWindowStore Inner { get; set; } = inner;
    public BenchmarkWindowCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BenchmarkWindow? Find(string code)
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

    public void Save(BenchmarkWindow item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BenchmarkWindow> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects benchmark windows. Empty fields match all.</summary>
public sealed record BenchmarkWindowQuery(string Name, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(BenchmarkWindow item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Version < MinVersion)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BenchmarkWindow> Apply(List<BenchmarkWindow> items)
    {
        var matched = Collections.Filter(items, (BenchmarkWindow candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of benchmark window store.</summary>
public static class BenchmarkWindowStore
{
    public static IBenchmarkWindowStore NewMemoryBenchmarkWindowStore()
    {
        return new MemoryBenchmarkWindowStore(new Dictionary<string, BenchmarkWindow>(), new List<string>());
    }

    public static IBenchmarkWindowStore NewCachedBenchmarkWindowStore(MemoryBenchmarkWindowStore inner, int ttl, IClock clock)
    {
        return new CachedBenchmarkWindowStore(inner, BenchmarkWindowCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<BenchmarkWindow> FindBenchmarkWindows(IBenchmarkWindowStore store, BenchmarkWindowQuery query)
    {
        return query.Apply(store.List());
    }
}
