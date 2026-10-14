namespace Acme.Workforce.Compensation.Benchmark.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual benchmark windows by code.</summary>
public interface IManualBenchmarkWindowStore
{
    ManualBenchmarkWindow? Find(string code);
    void Save(ManualBenchmarkWindow item);
    bool Remove(string code);
    List<ManualBenchmarkWindow> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualBenchmarkWindowStore(Dictionary<string, ManualBenchmarkWindow> items, List<string> order) : IManualBenchmarkWindowStore
{
    public Dictionary<string, ManualBenchmarkWindow> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualBenchmarkWindow? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualBenchmarkWindow item)
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

    public List<ManualBenchmarkWindow> List()
    {
        var outValue = new List<ManualBenchmarkWindow>();
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
public sealed class CachedManualBenchmarkWindowStore(MemoryManualBenchmarkWindowStore inner, ManualBenchmarkWindowCache cache, int hits, int misses) : IManualBenchmarkWindowStore
{
    public MemoryManualBenchmarkWindowStore Inner { get; set; } = inner;
    public ManualBenchmarkWindowCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualBenchmarkWindow? Find(string code)
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

    public void Save(ManualBenchmarkWindow item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualBenchmarkWindow> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual benchmark windows. Empty fields match all.</summary>
public sealed record ManualBenchmarkWindowQuery(string Title, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(ManualBenchmarkWindow item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.LeaveDays < MinLeaveDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ManualBenchmarkWindow> Apply(List<ManualBenchmarkWindow> items)
    {
        var matched = Collections.Filter(items, (ManualBenchmarkWindow candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual benchmark window store.</summary>
public static class ManualBenchmarkWindowStore
{
    public static IManualBenchmarkWindowStore NewMemoryManualBenchmarkWindowStore()
    {
        return new MemoryManualBenchmarkWindowStore(
            new Dictionary<string, ManualBenchmarkWindow>(),
            new List<string>());
    }

    public static IManualBenchmarkWindowStore NewCachedManualBenchmarkWindowStore(MemoryManualBenchmarkWindowStore inner, int ttl, IClock clock)
    {
        return new CachedManualBenchmarkWindowStore(inner, ManualBenchmarkWindowCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<ManualBenchmarkWindow> FindManualBenchmarkWindows(IManualBenchmarkWindowStore store, ManualBenchmarkWindowQuery query)
    {
        return query.Apply(store.List());
    }
}
