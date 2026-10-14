namespace Acme.Workforce.Compensation.Benchmark.BenchmarkSummary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual benchmark allocations by code.</summary>
public interface IManualBenchmarkAllocationStore
{
    ManualBenchmarkAllocation? Find(string code);
    void Save(ManualBenchmarkAllocation item);
    bool Remove(string code);
    List<ManualBenchmarkAllocation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualBenchmarkAllocationStore(Dictionary<string, ManualBenchmarkAllocation> items, List<string> order) : IManualBenchmarkAllocationStore
{
    public Dictionary<string, ManualBenchmarkAllocation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualBenchmarkAllocation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualBenchmarkAllocation item)
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

    public List<ManualBenchmarkAllocation> List()
    {
        var outValue = new List<ManualBenchmarkAllocation>();
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
public sealed class CachedManualBenchmarkAllocationStore(MemoryManualBenchmarkAllocationStore inner, ManualBenchmarkAllocationCache cache, int hits, int misses) : IManualBenchmarkAllocationStore
{
    public MemoryManualBenchmarkAllocationStore Inner { get; set; } = inner;
    public ManualBenchmarkAllocationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualBenchmarkAllocation? Find(string code)
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

    public void Save(ManualBenchmarkAllocation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualBenchmarkAllocation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual benchmark allocations. Empty fields match all.</summary>
public sealed record ManualBenchmarkAllocationQuery(string Label, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(ManualBenchmarkAllocation item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.StartsAt < MinStartsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ManualBenchmarkAllocation> Apply(List<ManualBenchmarkAllocation> items)
    {
        var matched = Collections.Filter(items, (ManualBenchmarkAllocation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual benchmark allocation store.</summary>
public static class ManualBenchmarkAllocationStore
{
    public static IManualBenchmarkAllocationStore NewMemoryManualBenchmarkAllocationStore()
    {
        return new MemoryManualBenchmarkAllocationStore(
            new Dictionary<string, ManualBenchmarkAllocation>(),
            new List<string>());
    }

    public static IManualBenchmarkAllocationStore NewCachedManualBenchmarkAllocationStore(MemoryManualBenchmarkAllocationStore inner, int ttl, IClock clock)
    {
        return new CachedManualBenchmarkAllocationStore(
            inner,
            ManualBenchmarkAllocationCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<ManualBenchmarkAllocation> FindManualBenchmarkAllocations(IManualBenchmarkAllocationStore store, ManualBenchmarkAllocationQuery query)
    {
        return query.Apply(store.List());
    }
}
